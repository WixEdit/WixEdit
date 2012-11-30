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
using System.IO;
using System.Windows.Forms;
using System.Xml;

using WixEdit.Xml;

namespace WixEdit.Panels
{
    /// <summary>
    /// DisplayBasePanel is the base class for every panel with a PropertyGrid.
    /// </summary>
    public abstract class DisplayBasePanel : BasePanel
    {
        protected string currentElementName;
        protected string currentKeyName;
        protected string currentXPath;

        protected XmlNode currentParent;
        protected XmlNodeList currentList;

        protected PropertyGrid currentGrid;
        protected ContextMenu currentGridContextMenu;

        public DisplayBasePanel(WixFiles wixFiles)
            : base(wixFiles)
        {
            Reload += new ReloadHandler(ReloadData);

            CreateControl();
        }

        public DisplayBasePanel(WixFiles wixFiles, string xpath, string keyName)
            : base(wixFiles)
        {
            if (xpath == null)
            {
                throw new ArgumentException("Require xpath in construction", "xpath");
            }
            if (keyName == null)
            {
                throw new ArgumentException("Require keyName in construction", "keyName");
            }

            currentXPath = xpath;
            currentKeyName = keyName;

            Reload += new ReloadHandler(ReloadData);

            CreateControl();
        }

        public DisplayBasePanel(WixFiles wixFiles, string xpath, string elementName, string keyName)
            : base(wixFiles)
        {
            if (xpath == null)
            {
                throw new ArgumentException("Require xpath in construction", "xpath");
            }
            if (elementName == null)
            {
                throw new ArgumentException("Require elementName in construction", "elementName");
            }
            if (keyName == null)
            {
                throw new ArgumentException("Require keyName in construction", "keyName");
            }

            currentXPath = xpath;
            currentElementName = elementName;
            currentKeyName = keyName;

            Reload += new ReloadHandler(ReloadData);

            CreateControl();
        }

        /// <summary>
        /// Has to be assigned in inherited classes, it defines a name for new element
        /// </summary>
        protected string CurrentElementName
        {
            get
            {
                return currentElementName;
            }
        }

        /// <summary>
        /// Parent node for specific part of wix file 
        /// </summary>
        protected XmlNode CurrentParent
        {
            get
            {
                if (currentParent == null)
                {
                    AssignParentNode();
                }
                return currentParent;
            }
            set
            {
                currentParent = value;
            }
        }

        /// <summary>
        /// PropertyGrid which is build to fit inherited class, specific to wix item
        /// </summary>
        protected PropertyGrid CurrentGrid
        {
            get
            {
                return currentGrid;
            }
            set
            {
                currentGrid = value;
            }
        }

        /// <summary>
        /// Context menu assigned to <see cref="PropertyGrid"/>
        /// </summary>
        protected ContextMenu CurrentGridContextMenu
        {
            get
            {
                return currentGridContextMenu;
            }
            set
            {
                currentGridContextMenu = value;
            }
        }

        /// <summary>
        /// Has to be assigned in inherited classes,it defines a name of identifier
        ///  of specific part of wix file, e.g. for Binaries it is "Id", for ProgressText it is "Action"
        /// </summary>
        protected string CurrentKeyName
        {
            get
            {
                return currentKeyName;
            }
        }

        /// <summary>
        /// Has to be assigned in inherited classes, id defines XPath for a list of 
        /// specific inherited parts of wix schema, e.g. Properties or UIText and so on.
        /// </summary>
        protected string CurrentXPath
        {
            get
            {
                return currentXPath;
            }
        }

        /// <summary>
        /// List of specific item of wix, e.g. Binaries, ErrorText, etc.
        /// </summary>
        protected XmlNodeList CurrentList
        {
            get
            {
                return currentList;
            }
            set
            {
                currentList = value;
            }
        }

        /// <summary>
        /// Checks whether this panel is the owner of this node.
        /// </summary>
        /// <param name="node">Node to check</param>
        /// <returns>Returnes whether this panel is owner.</returns>
        public abstract bool IsOwnerOfNode(XmlNode node);

        /// <summary>
        /// This should display the node in the panel.
        /// </summary>
        /// <remarks>GetShowingNode should do the exact oposite</remarks>
        /// <param name="node">Node to display</param>
        public abstract void ShowNode(XmlNode node);

        /// <summary>
        /// Reloads the panel.
        /// </summary>
        public abstract void ReloadData();

        /// <summary>
        /// Method assign root node of wixfile, which is /wix:Wix/*
        /// <remarks>it has to be override if root node is different, e.g UI section</remarks>
        protected virtual void AssignParentNode()
        {
            currentParent = WixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", WixFiles.WxsNsmgr);
        }

        /// <summary>
        /// Find the first parent node that can be shown, default behavior is the first XmlNodeType.Element.
        /// </summary>
        /// <param name="node">Node to get showable parent from.</param>
        /// <returns>Parent node which can be shown.</returns>
        protected virtual XmlNode GetShowableNode(XmlNode node)
        {
            XmlNode showableNode = node;
            while (showableNode != null && showableNode.NodeType != XmlNodeType.Element)
            {
                if (showableNode.NodeType == XmlNodeType.Attribute)
                {
                    showableNode = ((XmlAttribute)showableNode).OwnerElement;
                }
                else
                {
                    showableNode = showableNode.ParentNode;
                }
            }

            return showableNode;
        }

        /// <summary>
        /// Get currently shown node.
        /// </summary>
        /// <remarks>ShowNode should do the exact oposite</remarks>
        /// <returns></returns>
        public abstract XmlNode GetShowingNode();

        /// <summary>
        /// Inserts the new element at the right place, right after nodes with the same name.
        /// </summary>
        /// <param name="parentElement">Parent node which gets the new element as child</param>
        /// <param name="newElement">The new element which gets inserted as child of the parent element.</param>
        protected virtual void InsertNewXmlNode(XmlNode parentElement, XmlNode newElement)
        {
            string newName = newElement.Name;
            if (newName.IndexOf(":") < 0)
            {
                newName = "wix:" + newElement.Name;
            }
            XmlNodeList sameNodes = parentElement.SelectNodes(newName, WixFiles.WxsNsmgr);
            if (sameNodes.Count > 0)
            {
                parentElement.InsertAfter(newElement, sameNodes[sameNodes.Count - 1]);
            }
            else
            {
                parentElement.AppendChild(newElement);
            }
        }

        /// <summary>
        /// Method do import of specific part of wixfile, depends on given Xpath. It gives back all answers which are fit to Xpath.
        /// </summary>
        /// <param name="xPath">Xpath query</param>
        protected virtual bool ImportItems(string xPath)
        {
            if (xPath == null || xPath.Trim().Length == 0)
            {
                throw new NullReferenceException("xPath");
            }

            string importFile = this.OpenDialogFile();
            if (importFile == null || !File.Exists(importFile))
            {
                return false;
            }

            XmlDocument importXml = new XmlDocument();
            try
            {
                importXml.Load(importFile);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to load xml from file.", "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return false;
            }

            // We have to set the Wix namespace 
            importXml.DocumentElement.SetAttribute("xmlns", WixFiles.WixNamespaceUri);
            importXml.LoadXml(importXml.OuterXml);

            XmlNodeList itemList = importXml.SelectNodes(xPath, WixFiles.WxsNsmgr);
            if (itemList.Count > 0)
            {
                foreach (XmlNode item in itemList)
                {
                    if (item.Attributes[currentKeyName] == null)
                    {
                        continue;
                    }

                    string itemName = item.Attributes[currentKeyName].Value;
                    XmlNode importedItem = WixFiles.WxsDocument.ImportNode(item, true);

                    InsertNewXmlNode(currentParent, importedItem);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method open FileDialog to ask wixfiles, no multiple selection
        /// </summary>
        /// <returns>Absolute full path to file included filename with extension</returns>
        private string OpenDialogFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "WiX Files (*.xml;*.wxs;*.wxi)|*.XML;*.WXS;*.WXI|All files (*.*)|*.*";
            ofd.Multiselect = false;
            ofd.InitialDirectory = WixFiles.WxsDirectory.FullName;

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            else
            {
                return ofd.FileName;
            }
        }

        private delegate void ReloadHandler();
        private event ReloadHandler Reload;

        /// <summary>
        /// Reloads the panel.
        /// </summary>
        public void DoReload()
        {
            Reload();
        }
    }
}
