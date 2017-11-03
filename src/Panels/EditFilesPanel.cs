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
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Import;
using WixEdit.Server;
using WixEdit.Controls;
using WixEdit.Xml;
using WixEdit.Forms;

namespace WixEdit.Panels
{
    /// <summary>
    /// Panel for adding and removing files and other installable items.
    /// </summary>
    public class EditFilesPanel : DisplayTreeBasePanel
    {
        private TreeNode oldNode = null;

        public EditFilesPanel(WixFiles wixFiles)
            : base(wixFiles, "/wix:Wix/*/wix:Directory|/wix:Wix/*/wix:DirectoryRef", "Id", false)
        {

            LoadData();

            CurrentTreeView.DragEnter += new DragEventHandler(treeView_DragEnter);
            CurrentTreeView.DragLeave += new EventHandler(treeView_DragLeave);
            CurrentTreeView.DragOver += new DragEventHandler(treeView_DragOver);
            CurrentTreeView.DragDrop += new DragEventHandler(treeView_DragDrop);
            CurrentTreeView.AllowDrop = true;
        }

        protected override void AddCustomTreeViewContextMenuItems(XmlNode node, ContextMenu treeViewContextMenu)
        {
            if ((node.Name == "Component")
                || (node.Name == "Directory"))
            {
                IconMenuItem importFilesMenu = new IconMenuItem("&Import Files", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
                importFilesMenu.Click += new System.EventHandler(ImportFiles_Click);
                treeViewContextMenu.MenuItems.Add(1, importFilesMenu);
            }

            if (node.Name == "Directory")
            {
                IconMenuItem importFolderMenu = new IconMenuItem("&Import Folder", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
                importFolderMenu.Click += new System.EventHandler(ImportFolder_Click);
                treeViewContextMenu.MenuItems.Add(1, importFolderMenu);
            }
        }

        //I changed a menu, because it's simple to implement further and it follows menu like in UISeq., 
        //means we should keep one design way of UI. Lets suppose use New and its submenu for it in every case where are more then one possibilities
        //
        protected override void PopupPanelContextMenu(System.Object sender, System.EventArgs e)
        {
            //clear menu and add import menu
            base.PopupPanelContextMenu(sender, e);

            //add custom menu, index has to be used!!!
            IconMenuItem subMenuItem = new IconMenuItem("New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            IconMenuItem subSubMenuItem1 = new IconMenuItem("Directory");
            IconMenuItem subSubMenuItem2 = new IconMenuItem("DirectoryRef");

            subSubMenuItem1.Click += new EventHandler(NewCustomElement_Click);
            subSubMenuItem2.Click += new EventHandler(NewCustomElement_Click);

            subMenuItem.MenuItems.Add(subSubMenuItem1);
            subMenuItem.MenuItems.Add(subSubMenuItem2);

            PanelContextMenu.MenuItems.Add(0, subMenuItem);
        }

        protected override void NewCustomElement_Click(object sender, System.EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            CreateNewCustomElement(item.Text);
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
        }

        private void treeView_DragLeave(object sender, EventArgs e)
        {
            if (oldNode != null)
            {
                TreeNode newNode = new TreeNode();
                oldNode.BackColor = newNode.BackColor;
                oldNode.ForeColor = newNode.ForeColor;
                oldNode = null;
            }
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            TreeNode aNode = CurrentTreeView.GetNodeAt(CurrentTreeView.PointToClient(new Point(e.X, e.Y)));

            if (oldNode == aNode)
            {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                XmlNode aNodeElement = aNode.Tag as XmlNode;
                if (aNodeElement.Name == "Component")
                {
                    bool filesOnly = true;

                    string[] filesAndFolders = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (string fileOrFolder in filesAndFolders)
                    {
                        if (File.Exists(fileOrFolder) == false)
                        {
                            filesOnly = false;
                            break;
                        }
                    }

                    if (filesOnly)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else if (aNodeElement.Name == "Directory")
                {
                    bool dirsOnly = true;

                    string[] filesAndFolders = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (string fileOrFolder in filesAndFolders)
                    {
                        if (Directory.Exists(fileOrFolder) == false)
                        {
                            dirsOnly = false;
                            break;
                        }
                    }

                    if (dirsOnly)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }

            if (oldNode != null)
            {
                oldNode.BackColor = aNode.BackColor;
                oldNode.ForeColor = aNode.ForeColor;
            }

            aNode.BackColor = Color.DarkBlue;
            aNode.ForeColor = Color.White;

            oldNode = aNode;
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode aNode = CurrentTreeView.GetNodeAt(CurrentTreeView.PointToClient(new Point(e.X, e.Y)));
            CurrentTreeView.SelectedNode = aNode;

            XmlNode aNodeElement = aNode.Tag as XmlNode;

            if (oldNode != null)
            {
                oldNode.BackColor = aNode.BackColor;
                oldNode.ForeColor = aNode.ForeColor;
            }

            string[] filesOrFolders = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (aNodeElement.Name == "Component")
            {
                ImportFilesInComponent(aNode, aNodeElement, filesOrFolders);
            }
            else if (aNodeElement.Name == "Directory")
            {
                ImportFoldersInDirectory(aNode, aNodeElement, filesOrFolders);
            }
        }

        private void ImportFiles_Click(object sender, System.EventArgs e)
        {
            TreeNode aNode = CurrentTreeView.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*|Registration Files (*.reg)|*.REG";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;
            ofd.DereferenceLinks = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] files = ofd.FileNames;

                ImportFilesInComponent(aNode, aNodeElement, files);
            }
        }

        private void ImportFolder_Click(object sender, System.EventArgs e)
        {
            TreeNode aNode = CurrentTreeView.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            FolderSelectDialog ofd = new FolderSelectDialog();
            ofd.Description = "Select folder to import";
            ofd.ShowNewFolderButton = false;
            if (ofd.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(ofd.SelectedPath))
            {
                ImportFoldersInDirectory(aNode, aNodeElement, new string[] { ofd.SelectedPath });
            }
        }

        private void ImportFoldersInDirectory(TreeNode node, XmlNode directoryNode, string[] folders)
        {
            if (directoryNode.Name == "Directory")
            {
                bool mustExpand = (node.Nodes.Count == 0);

                CurrentTreeView.SuspendLayout();

                WixFiles.UndoManager.BeginNewCommandRange();
                try
                {
                    DirectoryImport dirImport = new DirectoryImport(WixFiles, folders, directoryNode);
                    dirImport.Import(node);
                }
                catch (WixEditException ex)
                {
                    MessageBox.Show(String.Format("Failed to complete import: {0}\r\n\r\nThe import is aborted and could be partially completed.", ex.Message), "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    ErrorReportHandler r = new ErrorReportHandler(ex, this.TopLevelControl, "An exception occured during the import! Please press OK to report this error to the WixEdit website, so this error can be fixed.");
                    r.ReportInSeparateThread();
                }

                CurrentTreeView.ResumeLayout();
            }
        }

        private void ImportFilesInComponent(TreeNode node, XmlNode componentNode, string[] files)
        {
            if (componentNode.Name == "Directory")
            {
                FileImport.AddFiles(WixFiles, files, node, componentNode);
            }
            else if (componentNode.Name == "Component")
            {
                bool mustExpand = (node.Nodes.Count == 0);

                CurrentTreeView.SuspendLayout();

                bool foundReg = false;
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".reg")
                    {
                        foundReg = true;
                        break;
                    }
                }

                bool importRegistryFiles = false;
                if (foundReg == true)
                {
                    DialogResult result = MessageBox.Show(this, "Import Registry (*.reg) files to Registry elements?", "Import?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                    else if (result == DialogResult.Yes)
                    {
                        importRegistryFiles = true;
                    }
                }

                WixFiles.UndoManager.BeginNewCommandRange();
                StringBuilder errorMessageBuilder = new StringBuilder();

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    try
                    {
                        if (fileInfo.Extension.ToLower() == ".reg" && importRegistryFiles)
                        {
                            RegistryImport regImport = new RegistryImport(WixFiles, fileInfo, componentNode);
                            regImport.Import(node);
                        }
                        else
                        {
                            FileImport fileImport = new FileImport(WixFiles, fileInfo, componentNode);
                            fileImport.Import(node);
                        }
                    }
                    catch (WixEditException ex)
                    {
                        errorMessageBuilder.AppendFormat("{0} ({1})\r\n", fileInfo.Name, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        string message = String.Format("An exception occured during the import of \"{0}\"! Please press OK to report this error to the WixEdit website, so this error can be fixed.", fileInfo.Name);
                        ExceptionForm form = new ExceptionForm(message, ex);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            ErrorReporter reporter = new ErrorReporter();
                            reporter.Report(ex);
                        }
                    }
                }

                if (errorMessageBuilder.Length > 0)
                {
                    MessageBox.Show(this, "Import failed for the following files:\r\n\r\n" + errorMessageBuilder.ToString(), "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ShowNode(componentNode);

                if (mustExpand)
                {
                    node.Expand();
                }

                CurrentTreeView.ResumeLayout();
            }
        }
    }
}