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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.IO;

using WixEdit.Controls;
using WixEdit.Import;
using WixEdit.Server;
using WixEdit.Images;
using WixEdit.Xml;
using WixEdit.Forms;

namespace WixEdit.Wizard
{
    class FileSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        TreeView tree;
        ContextMenu contextMenu;

        Button newFolderButton;
        Button removeButton;
        Button importDirectoryButton;
        Button importFilesButton;

        IconMenuItem importFilesMenuItem;
        IconMenuItem importFolderMenuItem;
        IconMenuItem newFolderMenuItem;
        IconMenuItem newSpecialFolderMenuItem;
        IconMenuItem newComponentMenuItem;
        IconMenuItem deleteMenuItem;

        string[] specialFolders = new string[] { "AdminToolsFolder",
                                                "AppDataFolder",
                                                "CommonAppDataFolder",
                                                "CommonFiles64Folder",
                                                "CommonFilesFolder",
                                                "DesktopFolder",
                                                "FavoritesFolder",
                                                "FontsFolder",
                                                "LocalAppDataFolder",
                                                "MyPicturesFolder",
                                                "PersonalFolder",
                                                "ProgramFiles64Folder",
                                                "ProgramFilesFolder",
                                                "ProgramMenuFolder",
                                                "SendToFolder",
                                                "StartMenuFolder",
                                                "StartupFolder",
                                                "System16Folder",
                                                "System64Folder",
                                                "SystemFolder",
                                                "TempFolder",
                                                "TemplateFolder",
                                                "WindowsFolder",
                                                "WindowsVolume"};

        public FileSheet(WizardForm creator)
            : base(creator)
        {
            this.AutoScroll = true;

            titleLabel = new Label();
            titleLabel.Text = "Add files and folders to install.";
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 20;
            titleLabel.Left = 0;
            titleLabel.Top = 0;
            titleLabel.Padding = new Padding(5, 5, 5, 0);
            titleLabel.Font = new Font("Verdana",
                        10,
                        FontStyle.Bold,
                        GraphicsUnit.Point
                    );
            titleLabel.BackColor = Color.White;

            descriptionLabel = new Label();
            descriptionLabel.Text = "Select Files and Directories you want to add to the installer";
            descriptionLabel.Dock = DockStyle.Top;
            descriptionLabel.Height = 50 - titleLabel.Height;
            descriptionLabel.Left = 0;
            descriptionLabel.Top = titleLabel.Height;
            descriptionLabel.Padding = new Padding(8, 3, 5, 0);
            descriptionLabel.BackColor = Color.White;

            this.Controls.Add(descriptionLabel);

            this.Controls.Add(titleLabel);


            lineLabel = new Label();
            lineLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lineLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lineLabel.Location = new Point(0, titleLabel.Height + descriptionLabel.Height);
            lineLabel.Size = new Size(this.Width, 2);

            this.Controls.Add(lineLabel);

            tree = new TreeView();
            tree.HideSelection = false;
            tree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tree.Location = new Point(4, titleLabel.Height + descriptionLabel.Height + lineLabel.Height + 5);
            tree.Width = this.Width - 8 - 100 - 8;
            tree.Height = this.Height - tree.Top - 7;
            tree.ImageList = ImageListFactory.GetImageList();
            tree.MouseDown += new MouseEventHandler(tree_MouseDown);

            this.Controls.Add(tree);

            newFolderButton = new Button();
            newFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newFolderButton.Location = new Point(tree.Location.X + tree.Width + 8, tree.Top);
            newFolderButton.Width = 100;
            newFolderButton.Height = 23;
            newFolderButton.Text = "New folder";
            newFolderButton.Click += new EventHandler(newFolderButton_Click);

            this.Controls.Add(newFolderButton);

            removeButton = new Button();
            removeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            removeButton.Location = new Point(tree.Location.X + tree.Width + 8, newFolderButton.Bottom + 8);
            removeButton.Width = 100;
            removeButton.Height = 23;
            removeButton.Text = "Remove folder";
            removeButton.Click += new EventHandler(removeButton_Click);

            this.Controls.Add(removeButton);

            importDirectoryButton = new Button();
            importDirectoryButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            importDirectoryButton.Location = new Point(tree.Location.X + tree.Width + 8, removeButton.Bottom + 8);
            importDirectoryButton.Width = 100;
            importDirectoryButton.Height = 23;
            importDirectoryButton.Text = "Import directory";
            importDirectoryButton.Click += new EventHandler(importDirectoryButton_Click);

            this.Controls.Add(importDirectoryButton);

            importFilesButton = new Button();
            importFilesButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            importFilesButton.Location = new Point(tree.Location.X + tree.Width + 8, importDirectoryButton.Bottom + 8);
            importFilesButton.Width = 100;
            importFilesButton.Height = 23;
            importFilesButton.Text = "Import files";
            importFilesButton.Click += new EventHandler(importFilesButton_Click);

            this.Controls.Add(importFilesButton);

            contextMenu = new ContextMenu();
            contextMenu.Popup += new EventHandler(contextMenu_Popup);
            // tree.ContextMenu = contextMenu;

            importFilesMenuItem = new IconMenuItem("&Import Files", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            importFilesMenuItem.Click += new System.EventHandler(importFilesMenuItem_Click);
            contextMenu.MenuItems.Add(importFilesMenuItem);

            newFolderMenuItem = new IconMenuItem("&New Folder", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            newFolderMenuItem.Click += new System.EventHandler(newFolderMenuItem_Click);
            contextMenu.MenuItems.Add(newFolderMenuItem);

            importFolderMenuItem = new IconMenuItem("&Import Folder", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            importFolderMenuItem.Click += new System.EventHandler(importFolderMenuItem_Click);
            contextMenu.MenuItems.Add(importFolderMenuItem);

            newSpecialFolderMenuItem = new IconMenuItem("New Special Folder", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            foreach (string specialFolder in specialFolders)
            {
                IconMenuItem subItem = new IconMenuItem(specialFolder);
                subItem.Click += new EventHandler(specialFolderSubItem_Click);
                newSpecialFolderMenuItem.MenuItems.Add(subItem);
            }
            contextMenu.MenuItems.Add(newSpecialFolderMenuItem);

            newComponentMenuItem = new IconMenuItem("New Component", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            newComponentMenuItem.Click += new EventHandler(newComponentMenuItem_Click);
            contextMenu.MenuItems.Add(newComponentMenuItem);

            deleteMenuItem = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
            deleteMenuItem.Click += new EventHandler(deleteMenuItem_Click);
            contextMenu.MenuItems.Add(deleteMenuItem);


            XmlDocument wxsDoc = Wizard.WixFiles.WxsDocument;
            XmlNamespaceManager wxsNsmgr = Wizard.WixFiles.WxsNsmgr;

            XmlNodeList dirNodes = wxsDoc.SelectNodes("/wix:Wix/*/wix:Directory", wxsNsmgr);
            TreeNodeCollection treeNodes = tree.Nodes;

            InitTreeView(dirNodes);
        }

        private void importDirectoryButton_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null ||
                tree.SelectedNode.Tag == null ||
                ((XmlNode)tree.SelectedNode.Tag).Name != "Directory")
            {
                MessageBox.Show("Please select a folder in the tree first.", "Select folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ImportFolder();
            }
        }

        private void importFilesButton_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null ||
                tree.SelectedNode.Tag == null ||
                ((XmlNode)tree.SelectedNode.Tag).Name != "Component")
            {
                MessageBox.Show("Please select a component in the tree first.", "Select folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ImportFiles();
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null)
            {
                MessageBox.Show("Please select an item in the tree first.", "Select folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (tree.SelectedNode.Level == 0)
            {
                MessageBox.Show("Cannot remove the SourceDir with the id \"TARGETDIR\".", "Select folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                XmlElement selectedNode = tree.SelectedNode.Tag as XmlElement;
                if (selectedNode != null)
                {
                    selectedNode.ParentNode.RemoveChild(selectedNode);
                    tree.SelectedNode.Remove();
                }
            }
        }

        private void newFolderButton_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null ||
                tree.SelectedNode.Tag == null ||
                ((XmlNode)tree.SelectedNode.Tag).Name != "Directory")
            {
                MessageBox.Show("Please select a folder in the tree first.", "Select folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                NewFolder();
            }
        }

        private void NewFolder()
        {
            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Enter Directory Name";
            if (DialogResult.OK == frm.ShowDialog())
            {
                XmlNode node = tree.SelectedNode.Tag as XmlNode;
                XmlElement newDirectory = node.OwnerDocument.CreateElement("Directory", WixFiles.WixNamespaceUri);
                newDirectory.SetAttribute("Name", frm.SelectedString);
                newDirectory.SetAttribute("Id", FileImport.GenerateValidIdentifier(frm.SelectedString, newDirectory, Wizard.WixFiles));
                node.AppendChild(newDirectory);

                TreeNode treeNode = new TreeNode();
                treeNode.Text = frm.SelectedString;
                treeNode.ImageIndex = ImageListFactory.GetImageIndex("Directory");
                treeNode.SelectedImageIndex = treeNode.ImageIndex;
                treeNode.SelectedImageIndex = treeNode.ImageIndex;
                treeNode.Tag = newDirectory;
                tree.SelectedNode.Nodes.Add(treeNode);
                tree.SelectedNode.Expand();
                treeNode.EnsureVisible();
            }
        }

        private void tree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.Visible == false)
            {
                return;
            }
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = tree.GetNodeAt(e.X, e.Y);
                if (node == null)
                {
                    return;
                }
                tree.SelectedNode = node;

                Point spot = PointToClient(tree.PointToScreen(new Point(e.X, e.Y)));
                contextMenu.Show(this, spot);
            }
        }

        void contextMenu_Popup(object sender, EventArgs e)
        {
            foreach (MenuItem item in contextMenu.MenuItems)
            {
                item.Visible = false;
            }

            if (tree.SelectedNode == null)
            {
                return;
            }

            XmlNode selectedNode = tree.SelectedNode.Tag as XmlNode;
            if (selectedNode == null)
            {
                return;
            }

            if (selectedNode.Name == "Directory")
            {
                importFilesMenuItem.Visible = false;
                importFolderMenuItem.Visible = true;
                newFolderMenuItem.Visible = true;
                newComponentMenuItem.Visible = true;
            }
            else if (selectedNode.Name == "Component")
            {
                importFilesMenuItem.Visible = true;
                importFolderMenuItem.Visible = false;
                newFolderMenuItem.Visible = false;
                newComponentMenuItem.Visible = false;
            }
            else if (selectedNode.Name == "File")
            {
                importFilesMenuItem.Visible = false;
                importFolderMenuItem.Visible = false;
                newFolderMenuItem.Visible = false;
                newComponentMenuItem.Visible = false;
            }

            if (tree.SelectedNode.Level == 0)
            {
                deleteMenuItem.Visible = false;
                newSpecialFolderMenuItem.Visible = true;
            }
            else
            {
                deleteMenuItem.Visible = true;
                newSpecialFolderMenuItem.Visible = false;
            }

            //contextMenu.MenuItems.Clear();

            //if (tree.SelectedNode == null)
            //{
            //    return;
            //}

            //XmlNode node = tree.SelectedNode.Tag as XmlNode;
            //if (node == null)
            //{
            //    return;
            //}

            //if (node.Name == "Component")
            //{
            //    IconMenuItem importFilesMenu = new IconMenuItem("&Import Files", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            //    importFilesMenu.Click += new System.EventHandler(importFilesMenuItem_Click);
            //    contextMenu.MenuItems.Add(0, importFilesMenu);
            //}
            //else if (node.Name == "Directory")
            //{
            //    IconMenuItem newFolderMenu = new IconMenuItem("&New Folder", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            //    newFolderMenu.Click += new System.EventHandler(newFolderMenuItem_Click);
            //    contextMenu.MenuItems.Add(0, newFolderMenu);

            //    IconMenuItem importFolderMenu = new IconMenuItem("&Import Folder", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            //    importFolderMenu.Click += new System.EventHandler(importFolderMenuItem_Click);
            //    contextMenu.MenuItems.Add(1, importFolderMenu);
            //}
        }
        private void InitTreeView(XmlNodeList dirNodes)
        {
            this.SuspendLayout();

            tree.Nodes.Clear();

            foreach (XmlNode file in dirNodes)
            {
                if (file.NodeType == XmlNodeType.ProcessingInstruction)
                {
                    continue;
                }

                AddTreeNodesRecursive(file, tree.Nodes);
            }

            tree.ExpandAll();

            if (tree.Nodes.Count > 0)
            {
                tree.SelectedNode = tree.Nodes[0];
            }

            this.ResumeLayout();
        }


        protected void AddTreeNodesRecursive(XmlNode file, TreeNodeCollection nodes)
        {
            if (file.Name.StartsWith("#"))
            {
                return;
            }

            TreeNode node = new TreeNode(GetDisplayName(file));
            node.Tag = file;

            int imageIndex = -1;
            if (file.Name == "File" && file.Attributes["Source"] != null)
            {
                string filePath = Path.Combine(Wizard.WixFiles.WxsDirectory.FullName, file.Attributes["Source"].Value);
                if (File.Exists(filePath))
                {
                    string key = Path.GetExtension(filePath).ToUpper();
                    imageIndex = tree.ImageList.Images.IndexOfKey(key);
                    if (imageIndex < 0)
                    {
                        try
                        {
                            Icon ico = FileIconFactory.GetFileIcon(filePath);
                            if (ico != null)
                            {
                                tree.ImageList.Images.Add(key, ico);
                                imageIndex = tree.ImageList.Images.Count - 1;
                            }
                        } // if icon retrieved from icon factory
                        catch { }
                    } // if image not already in tree image list
                } // if file exists
            } // node is a file and Source attribute is not null
            if (imageIndex < 0)
            {
                imageIndex = ImageListFactory.GetImageIndex(file.Name);
            }
            if (imageIndex >= 0)
            {
                node.ImageIndex = imageIndex;
                node.SelectedImageIndex = imageIndex;
            }

            nodes.Add(node);

            if (file.ChildNodes.Count > 10000)
            {
                TreeNode tooManyNodes = new TreeNode("<< Too many children to display >>");
                node.ImageIndex = ImageListFactory.GetUnsupportedImageIndex();
                node.SelectedImageIndex = node.ImageIndex;
                node.Nodes.Add(tooManyNodes);

                return;
            }

            foreach (XmlNode child in file.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.ProcessingInstruction)
                {
                    continue;
                }

                AddTreeNodesRecursive(child, node.Nodes);
            }
        }


        protected string GetDisplayName(XmlNode element)
        {
            string displayName = null;
            try
            {
                switch (element.Name)
                {
                    case "Directory":
                    case "DirectoryRef":
                    case "File":
                        XmlAttribute nameAtt = element.Attributes["LongName"];
                        if (nameAtt == null)
                        {
                            nameAtt = element.Attributes["Name"];
                        }
                        if (nameAtt == null)
                        {
                            nameAtt = element.Attributes["Id"];
                        }
                        displayName = nameAtt.Value;
                        break;
                    case "Registry":
                        string root = element.Attributes["Root"].Value;
                        string key = element.Attributes["Key"].Value;
                        XmlAttribute name = element.Attributes["Name"];
                        if (name != null)
                        {
                            if (key.EndsWith("\\") == false)
                            {
                                key = key + "\\";
                            }
                            key = key + name.Value;
                        }

                        displayName = root + "\\" + key;
                        break;
                    case "RegistryValue":
                        if (element.Attributes["Root"] == null ||
                            element.Attributes["Key"] == null)
                        {
                            if (element.Attributes["Name"] != null)
                            {
                                displayName = element.Attributes["Name"].Value;
                            }
                            else
                            {
                                displayName = element.Name;
                            }
                        }
                        else
                        {
                            string valueRoot = element.Attributes["Root"].Value;
                            string valueKey = element.Attributes["Key"].Value;

                            displayName = valueRoot + "\\" + valueKey;
                            if (element.Attributes["Name"] != null)
                            {
                                displayName = valueRoot + "\\" + valueKey + "\\" + element.Attributes["Name"].Value;
                            }
                        }
                        break;
                    case "RegistryKey":
                        if (element.Attributes["Root"] == null ||
                            element.Attributes["Key"] == null)
                        {
                            displayName = element.Name;
                        }
                        else
                        {
                            string keyRoot = element.Attributes["Root"].Value;
                            string keyKey = element.Attributes["Key"].Value;

                            displayName = keyRoot + "\\" + keyKey;
                        }
                        break;
                    case "Component":
                    case "CustomAction":
                    case "Feature":
                    case "ComponentRef":
                        XmlAttribute idAtt = element.Attributes["Id"];
                        if (idAtt != null)
                        {
                            displayName = idAtt.Value;
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    case "Show":
                        XmlAttribute dlgAtt = element.Attributes["Dialog"];
                        if (dlgAtt != null)
                        {
                            displayName = dlgAtt.Value;
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    case "Custom":
                        XmlAttribute actionAtt = element.Attributes["Action"];
                        if (actionAtt != null)
                        {
                            displayName = actionAtt.Value;
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    case "Condition":
                        string innerText = element.InnerText;
                        if (innerText != null && innerText.Length > 1)
                        {
                            displayName = String.Format("({0})", innerText);
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    default:
                        displayName = element.Name;
                        break;
                }
            }
            catch
            {
                displayName = element.Name;
            }

            if (displayName == null || displayName == "")
            {
                displayName = element.Name;
            }

            return displayName;
        }

        private static void AddDirectoryTreeNodes(XmlNodeList dirNodes, TreeNodeCollection treeNodes)
        {
            foreach (XmlNode dirNode in dirNodes)
            {
                if (dirNode.Name != "Directory")
                {
                    continue;
                }

                XmlElement dirElement = (XmlElement)dirNode;

                TreeNode treeNode = new TreeNode();
                treeNode.Tag = dirElement;
                treeNode.Text = dirElement.GetAttribute("Name");
                treeNode.ImageIndex = ImageListFactory.GetImageIndex("Directory");
                
                treeNodes.Add(treeNode);
                treeNode.Expand();

                AddDirectoryTreeNodes(dirNode.ChildNodes, treeNode.Nodes);
            }
        }

        private void ImportFiles()
        {
            TreeNode aNode = tree.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*|Registration Files (*.reg)|*.REG";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] files = ofd.FileNames;

                ImportFilesInComponent(aNode, aNodeElement, files);
            }
        }

        private void importFilesMenuItem_Click(object sender, System.EventArgs e)
        {
            ImportFiles();
        }

        private void importFolderMenuItem_Click(object sender, System.EventArgs e)
        {
            ImportFolder();
        }

        private void newFolderMenuItem_Click(object sender, System.EventArgs e)
        {
            NewFolder();
        }

        private void deleteMenuItem_Click(object sender, EventArgs e)
        {
            XmlElement selectedNode = tree.SelectedNode.Tag as XmlElement;
            if (selectedNode == null ||
                tree.SelectedNode.Level == 0)
            {
                return;
            }

            selectedNode.ParentNode.RemoveChild(selectedNode);
            tree.SelectedNode.Remove();
        }

        private void specialFolderSubItem_Click(object sender, EventArgs e)
        {
            XmlElement selectedNode = tree.SelectedNode.Tag as XmlElement;
            if (selectedNode == null)
            {
                return;
            }

            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                XmlElement newProp = Wizard.WixFiles.WxsDocument.CreateElement("Directory", WixFiles.WixNamespaceUri);

                newProp.SetAttribute("Id", menuItem.Text);
                newProp.SetAttribute("Name", menuItem.Text);

                selectedNode.AppendChild(newProp);
                TreeNode newNode = new TreeNode();
                newNode.Text = menuItem.Text;
                newNode.ImageIndex = ImageListFactory.GetImageIndex("Directory");
                newNode.SelectedImageIndex = newNode.ImageIndex;
                newNode.Tag = newProp;
                newNode.EnsureVisible();

                tree.SelectedNode.Nodes.Add(newNode);
                tree.SelectedNode.Expand();
            }
        }

        private void newComponentMenuItem_Click(object sender, EventArgs e)
        {
            XmlElement selectedNode = tree.SelectedNode.Tag as XmlElement;
            if (selectedNode == null)
            {
                return;
            }

            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Enter component id";
            if (DialogResult.OK == frm.ShowDialog())
            {
                XmlElement newProp = Wizard.WixFiles.WxsDocument.CreateElement("Component", WixFiles.WixNamespaceUri);

                newProp.SetAttribute("Id", frm.SelectedString);
                newProp.SetAttribute("DiskId", "1");
                newProp.SetAttribute("KeyPath", "yes");
                newProp.SetAttribute("Guid", Guid.NewGuid().ToString("D"));

                selectedNode.AppendChild(newProp);
                TreeNode newNode = new TreeNode();
                newNode.Text = frm.SelectedString;
                newNode.ImageIndex = ImageListFactory.GetImageIndex("Component");
                newNode.SelectedImageIndex = newNode.ImageIndex;
                newNode.Tag = newProp;
                newNode.EnsureVisible();

                tree.SelectedNode.Nodes.Add(newNode);
                tree.SelectedNode.Expand();
            }
        }

        private void ImportFolder()
        {
            TreeNode aNode = tree.SelectedNode;
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

                tree.SuspendLayout();

                Wizard.WixFiles.UndoManager.BeginNewCommandRange();
                try
                {
                    DirectoryImport dirImport = new DirectoryImport(Wizard.WixFiles, folders, directoryNode);
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

                tree.ResumeLayout();
            }
        }

        private void ImportFilesInComponent(TreeNode node, XmlNode componentNode, string[] files)
        {
            if (componentNode.Name == "Component")
            {
                bool mustExpand = (node.Nodes.Count == 0);

                tree.SuspendLayout();

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

                Wizard.WixFiles.UndoManager.BeginNewCommandRange();
                StringBuilder errorMessageBuilder = new StringBuilder();

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    try
                    {
                        if (fileInfo.Extension.ToLower() == ".reg" && importRegistryFiles)
                        {
                            RegistryImport regImport = new RegistryImport(Wizard.WixFiles, fileInfo, componentNode);
                            regImport.Import(node);
                        }
                        else
                        {
                            FileImport fileImport = new FileImport(Wizard.WixFiles, fileInfo, componentNode);
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

                // ShowNode(componentNode);

                if (mustExpand)
                {
                    node.Expand();
                }

                tree.ResumeLayout();
            }
        }
    }
}
