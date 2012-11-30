// Copyright (c) 2005 J.Keuper (j.keuper@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to 
// deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
// sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.


using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;

using WixEdit.Settings;

namespace WixEdit.Xml
{
    public class UndoManager
    {
        ArrayList undoCommands;
        ArrayList redoCommands;

        bool beginNewCommandRange;
        bool isPropertyGridEdit;

        XmlDocument wxsDocument;

        XmlNodeChangedEventHandler nodeChangedHandler;
        XmlNodeChangedEventHandler nodeChangingHandler;
        XmlNodeChangedEventHandler nodeInsertedHandler;
        XmlNodeChangedEventHandler nodeRemovingHandler;

        DateTime timeCheck;

        WixFiles wixFiles;
        int docIsSavedUndoCount;

        public UndoManager(WixFiles wixFiles, XmlDocument wxsDocument)
        {
            undoCommands = new ArrayList();
            redoCommands = new ArrayList();

            beginNewCommandRange = true;

            this.wxsDocument = wxsDocument;
            this.wixFiles = wixFiles;
            this.docIsSavedUndoCount = 0;

            nodeChangedHandler = new XmlNodeChangedEventHandler(OnNodeChanged);
            nodeChangingHandler = new XmlNodeChangedEventHandler(OnNodeChanging);
            nodeInsertedHandler = new XmlNodeChangedEventHandler(OnNodeInserted);
            nodeRemovingHandler = new XmlNodeChangedEventHandler(OnNodeRemoving);

            this.wxsDocument.NodeChanged += nodeChangedHandler;
            this.wxsDocument.NodeChanging += nodeChangingHandler;
            this.wxsDocument.NodeInserted += nodeInsertedHandler;
            this.wxsDocument.NodeRemoving += nodeRemovingHandler;

            timeCheck = DateTime.Now;
        }

        public void RegisterHandlers()
        {
            wxsDocument.NodeChanged += nodeChangedHandler;
            wxsDocument.NodeChanging += nodeChangingHandler;
            wxsDocument.NodeInserted += nodeInsertedHandler;
            wxsDocument.NodeRemoving += nodeRemovingHandler;
        }

        public void DeregisterHandlers()
        {
            wxsDocument.NodeChanged -= nodeChangedHandler;
            wxsDocument.NodeChanging -= nodeChangingHandler;
            wxsDocument.NodeInserted -= nodeInsertedHandler;
            wxsDocument.NodeRemoving -= nodeRemovingHandler;
        }

        public void BeginNewCommandRange()
        {
            timeCheck = DateTime.Now;

            beginNewCommandRange = true;
        }

        public void StartPropertyGridEdit()
        {
            isPropertyGridEdit = true;
        }

        public void EndPropertyGridEdit()
        {
            isPropertyGridEdit = false;
        }

        /// <summary>
        /// Every action needs to start with a call to BeginNewCommandRange(), because there can 
        /// be multiple commands following right after each other in one action. If it's longer 
        /// ago than 250 ms, somewhere the call to BeginNewCommandRange() might be forgotten.
        /// </summary>
        /// <remarks>Disable for releases, but in develop time it could be handy.</remarks>
        public void CheckTime()
        {
            TimeSpan diff = DateTime.Now.Subtract(timeCheck);
            if (diff.TotalMilliseconds > 250)
            {
                // System.Windows.Forms.MessageBox.Show("Warning, the undo-system might be corrupted.");
            }
        }

        public void OnNodeChanged(Object src, XmlNodeChangedEventArgs args)
        {
            // Get new value and node
            redoCommands.Clear();
            if (docIsSavedUndoCount > undoCommands.Count)
            {
                docIsSavedUndoCount = -1;
            }

            CheckTime();

            string affectedInclude = wixFiles.IncludeManager.FindIncludeFile(args.Node);
            undoCommands.Add(new ChangeCommand(args.Node, oldNodeValue, args.Node.Value, beginNewCommandRange, affectedInclude));

            if (affectedInclude != null && affectedInclude.Length > 0)
            {
                HandleChangedInclude((IReversibleCommand)undoCommands[undoCommands.Count - 1]);
            }

            beginNewCommandRange = false;
        }

        string oldNodeValue;
        public void OnNodeChanging(Object src, XmlNodeChangedEventArgs args)
        {
            oldNodeValue = args.Node.Value;
        }

        public void OnNodeInserted(Object src, XmlNodeChangedEventArgs args)
        {
            // Get parent node and node
            if (args.NewParent.Name == "xmlns:xml")
            {
                return;
            }

            redoCommands.Clear();
            if (docIsSavedUndoCount > undoCommands.Count)
            {
                docIsSavedUndoCount = -1;
            }

            CheckTime();

            string affectedInclude = wixFiles.IncludeManager.FindIncludeFile(args.Node);
            undoCommands.Add(new InsertCommand(args.NewParent, args.Node, beginNewCommandRange, affectedInclude));

            if (affectedInclude != null && affectedInclude.Length > 0)
            {
                HandleChangedInclude((IReversibleCommand)undoCommands[undoCommands.Count - 1]);
            }

            beginNewCommandRange = false;
        }

        public void OnNodeRemoving(Object src, XmlNodeChangedEventArgs args)
        {
            // Get parent node and node
            redoCommands.Clear();
            if (docIsSavedUndoCount > undoCommands.Count)
            {
                docIsSavedUndoCount = -1;
            }

            CheckTime();

            string affectedInclude = wixFiles.IncludeManager.FindIncludeFile(args.Node);
            undoCommands.Add(new RemoveCommand(args.OldParent, args.Node, beginNewCommandRange, affectedInclude));

            if (affectedInclude != null && affectedInclude.Length > 0)
            {
                HandleChangedInclude((IReversibleCommand)undoCommands[undoCommands.Count - 1]);
            }

            beginNewCommandRange = false;
        }

        ArrayList allowChangIncludeFiles = new ArrayList();

        public ArrayList ChangedIncludes
        {
            get
            {
                ArrayList changedIncludes = new ArrayList();
                foreach (IReversibleCommand cmd in undoCommands)
                {
                    if (cmd.AffectedInclude != null && cmd.AffectedInclude.Length > 0 && changedIncludes.Contains(cmd.AffectedInclude) == false)
                    {
                        changedIncludes.Add(cmd.AffectedInclude);
                    }
                }

                return changedIncludes;
            }
        }

        private void HandleChangedInclude(IReversibleCommand cmd)
        {
            if (WixEditSettings.Instance.AllowIncludeChanges == IncludeChangesHandling.Disallow)
            {
                // Stop people from making more changes to this file,
                // and undo this command.
                if (isPropertyGridEdit)
                {
                    this.Undo(false);

                    MessageBox.Show(String.Format("You cannot change include file \"{0}\".", cmd.AffectedInclude), "Cannot modify include file.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    throw new ApplicationException("Not allowed to change include file.", new IncludeFileChangedException(this, cmd, true));
                }
            }
            else if (WixEditSettings.Instance.AllowIncludeChanges == IncludeChangesHandling.Allow)
            {
                // Do nothing...
                if (allowChangIncludeFiles.Count == 0 || allowChangIncludeFiles.Contains(cmd.AffectedInclude) == false)
                {
                    allowChangIncludeFiles.Add(cmd.AffectedInclude);
                }
            }
            else if (WixEditSettings.Instance.AllowIncludeChanges == IncludeChangesHandling.AskForEachFile)
            {
                if (allowChangIncludeFiles.Count == 0 || allowChangIncludeFiles.Contains(cmd.AffectedInclude) == false)
                {
                    DialogResult result = MessageBox.Show(String.Format("You are changing \"{0}\", do you wish to continue?", cmd.AffectedInclude), "Modify include file?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        allowChangIncludeFiles.Add(cmd.AffectedInclude);
                    }
                    else
                    {
                        if (isPropertyGridEdit)
                        {
                            this.Undo(false);
                        }
                        else
                        {
                            throw new ApplicationException("Not allowed to change include file.", new IncludeFileChangedException(this, cmd, false));
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            undoCommands.Clear();
            redoCommands.Clear();
            docIsSavedUndoCount = -1;
        }

        public void ClearRedo()
        {
            redoCommands.Clear();
        }

        public void DocumentIsSaved()
        {
            docIsSavedUndoCount = undoCommands.Count;
        }

        public bool CanRedo()
        {
            return redoCommands.Count > 0;
        }

        public bool CanUndo()
        {
            return undoCommands.Count > 0;
        }

        public bool HasChanges()
        {
            if (docIsSavedUndoCount == undoCommands.Count)
            {
                return false;
            }

            return true;
        }

        public XmlNode Redo()
        {
            XmlNode affectedNode = null;
            ArrayList affectedNodes = new ArrayList();

            if (redoCommands.Count > 0)
            {
                DeregisterHandlers();

                IReversibleCommand command = (IReversibleCommand)redoCommands[redoCommands.Count - 1];
                do
                {
                    affectedNodes.Add(command.Redo());

                    redoCommands.Remove(command);
                    undoCommands.Add(command);

                    if (redoCommands.Count > 0)
                    {
                        command = (IReversibleCommand)redoCommands[redoCommands.Count - 1];
                    }
                } while (redoCommands.Count > 0 && command.BeginCommandRange == false);

                RegisterHandlers();
            }

            foreach (XmlNode node in affectedNodes)
            {
                if (node is XmlText)
                {
                    if (node.ParentNode is XmlAttribute)
                    {
                        XmlAttribute att = node.ParentNode as XmlAttribute;
                        if (att.OwnerElement != null)
                        {
                            affectedNode = att.OwnerElement;
                            break;
                        }
                    }
                }
                else if (node.ParentNode != null)
                {
                    affectedNode = node;
                    break;
                }
                else if (node is XmlAttribute)
                {
                    XmlAttribute att = node as XmlAttribute;
                    if (att.OwnerElement != null)
                    {
                        affectedNode = att.OwnerElement;
                        break;
                    }
                }
            }

            return affectedNode;
        }

        public XmlNode Undo()
        {
            return Undo(true);
        }

        public XmlNode Undo(bool canRedo)
        {
            XmlNode affectedNode = null;
            ArrayList affectedNodes = new ArrayList();

            if (undoCommands.Count > 0)
            {
                DeregisterHandlers();

                IReversibleCommand command;
                do
                {
                    command = (IReversibleCommand)undoCommands[undoCommands.Count - 1];

                    affectedNodes.Add(command.Undo());

                    undoCommands.Remove(command);
                    redoCommands.Add(command);
                } while (undoCommands.Count > 0 && command.BeginCommandRange == false);

                if (canRedo == false)
                {
                    redoCommands.Clear();
                }

                RegisterHandlers();

                foreach (XmlNode node in affectedNodes)
                {
                    if (node.ParentNode != null)
                    {
                        affectedNode = node;
                        break;
                    }
                    else if (node is XmlAttribute)
                    {
                        XmlAttribute att = node as XmlAttribute;
                        if (att.OwnerElement != null)
                        {
                            affectedNode = att.OwnerElement;
                            break;
                        }
                    }
                }
            }

            return affectedNode;
        }

        public string GetNextUndoActionString()
        {
            if (undoCommands.Count == 0)
            {
                return String.Empty;
            }

            for (int i = undoCommands.Count - 1; i >= 0; i--)
            {
                IReversibleCommand cmd = (IReversibleCommand)undoCommands[i];
                if (cmd.BeginCommandRange)
                {
                    return cmd.GetDisplayActionString();
                }
            }

            // If the command starting the CommandRange is not found, return the first...
            return ((IReversibleCommand)undoCommands[undoCommands.Count - 1]).GetDisplayActionString();
        }

        public string GetNextRedoActionString()
        {
            if (redoCommands.Count == 0)
            {
                return String.Empty;
            }

            return ((IReversibleCommand)redoCommands[redoCommands.Count - 1]).GetDisplayActionString();
        }

        public int UndoCount
        {
            get { return undoCommands.Count; }
        }
    }

    public interface IReversibleCommand
    {
        bool BeginCommandRange
        {
            get;
        }

        string AffectedInclude
        {
            get;
        }

        XmlNode Undo();
        XmlNode Redo();

        string GetDisplayActionString();
    }

    public abstract class BaseCommand : IReversibleCommand
    {
        protected bool beginCommandRange;
        protected string affectedInclude;
        protected string displayActionString;

        public BaseCommand(bool beginCommandRange, string affectedInclude, string displayActionString)
        {
            this.beginCommandRange = beginCommandRange;
            this.affectedInclude = affectedInclude;
            this.displayActionString = displayActionString;
        }

        public abstract XmlNode Undo();
        public abstract XmlNode Redo();

        public bool BeginCommandRange
        {
            get
            {
                return beginCommandRange;
            }
        }

        public string AffectedInclude
        {
            get
            {
                return affectedInclude;
            }
        }

        public string GetDisplayActionString()
        {
            if (affectedInclude != null && affectedInclude.Length > 0)
            {
                return String.Format("{0} ({1})", displayActionString, affectedInclude);
            }
            else
            {
                return displayActionString;
            }
        }
    }

    public class InsertCommand : BaseCommand
    {
        XmlNode parentNode;
        XmlNode insertedNode;
        XmlNode previousSiblingNode;

        public InsertCommand(XmlNode parentNode, XmlNode insertedNode, bool beginCommandRange, string affectedInclude)
            :
                            base(beginCommandRange, affectedInclude, "Insert")
        {
            this.parentNode = parentNode;
            this.insertedNode = insertedNode;
        }

        public override XmlNode Undo()
        {
            previousSiblingNode = insertedNode.PreviousSibling;
            if (insertedNode is XmlAttribute)
            {
                parentNode.Attributes.Remove(insertedNode as XmlAttribute);
            }
            else
            {
                parentNode.RemoveChild(insertedNode);
            }

            return parentNode;
        }

        public override XmlNode Redo()
        {
            if (previousSiblingNode != null)
            {
                if (insertedNode is XmlAttribute)
                {
                    parentNode.Attributes.InsertAfter(insertedNode as XmlAttribute, previousSiblingNode as XmlAttribute);
                }
                else
                {
                    parentNode.InsertAfter(insertedNode, previousSiblingNode);
                }
            }
            else
            {
                if (insertedNode is XmlAttribute)
                {
                    parentNode.Attributes.Append(insertedNode as XmlAttribute);
                }
                else
                {
                    parentNode.InsertBefore(insertedNode, parentNode.FirstChild);
                }
            }

            return insertedNode;
        }
    }

    public class RemoveCommand : BaseCommand
    {
        XmlNode parentNode;
        XmlNode removedNode;
        XmlNode previousSiblingNode;

        public RemoveCommand(XmlNode parentNode, XmlNode removedNode, bool beginCommandRange, string affectedInclude)
            :
                            base(beginCommandRange, affectedInclude, "Delete")
        {
            this.parentNode = parentNode;
            this.removedNode = removedNode;
            previousSiblingNode = removedNode.PreviousSibling;
        }

        public override XmlNode Undo()
        {
            if (previousSiblingNode != null)
            {
                if (removedNode is XmlAttribute)
                {
                    parentNode.Attributes.InsertAfter(removedNode as XmlAttribute, previousSiblingNode as XmlAttribute);
                }
                else
                {
                    parentNode.InsertAfter(removedNode, previousSiblingNode);
                }
            }
            else
            {
                if (removedNode is XmlAttribute)
                {
                    parentNode.Attributes.Append(removedNode as XmlAttribute);
                }
                else
                {
                    parentNode.InsertBefore(removedNode, parentNode.FirstChild);
                }
            }

            return removedNode;
        }

        public override XmlNode Redo()
        {
            if (removedNode is XmlAttribute)
            {
                parentNode.Attributes.Remove(removedNode as XmlAttribute);
            }
            else
            {
                parentNode.RemoveChild(removedNode);
            }

            return parentNode;
        }
    }

    public class ChangeCommand : BaseCommand
    {
        XmlNode changedNode;
        string oldValue;
        string newValue;

        public ChangeCommand(XmlNode changedNode, string oldValue, string newValue, bool beginCommandRange, string affectedInclude)
            : base(beginCommandRange, affectedInclude, "Change")
        {
            this.changedNode = changedNode;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public override XmlNode Undo()
        {
            changedNode.Value = oldValue;

            return changedNode;
        }

        public override XmlNode Redo()
        {
            changedNode.Value = newValue;

            return changedNode;
        }
    }
}