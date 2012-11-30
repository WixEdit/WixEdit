/*
 * FolderSelect.cs: A folder browser example
 * Version 1.0
 * Copyright (C) 2001 Chris Warner
 * 
 * You are free to use this in any personal or commercial application
 * so long as none of these comments are changed or removed from this file.
 * 
 * E-mail: jabrwoky@pacbell.net
 * 
 * From: http://www.codeproject.com/KB/selection/folderseldlg.aspx
 * */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using WixEdit.Xml;

namespace WixEdit.Controls
{
    /// <summary> class FolderSelect 
    /// <para>An example on how to build a folder browser dialog window using C# and the .Net framework.</para>
    /// </summary>
    public class FolderSelectDialog : Form
    {
        private System.ComponentModel.IContainer components;

        private DirectoryInfo folder;

        private System.Windows.Forms.TextBox _pathTextBox;
        private System.Windows.Forms.Label _descriptionLabel;
        private System.Windows.Forms.TreeView _folderTreeView;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _selectButton;
        private System.Windows.Forms.ImageList _imageList;
        private Button _newFolderButton;

        public FolderSelectDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();

            // initialize the treeView
            SetTreeImageList();
            FillTree();

            if (_folderTreeView.Nodes.Count > 0)
            {
                _folderTreeView.SelectedNode = _folderTreeView.Nodes[0];
            }
   
            _pathTextBox.Focus();
        }

        private void SetTreeImageList()
        {
            Bitmap theBmp = new Bitmap(WixFiles.GetResourceStream("directory.drive.bmp"));
            theBmp.MakeTransparent();
            _imageList.Images.Add(theBmp);

            theBmp = new Bitmap(WixFiles.GetResourceStream("directory.closed.bmp"));
            theBmp.MakeTransparent();
            _imageList.Images.Add(theBmp);

            theBmp = new Bitmap(WixFiles.GetResourceStream("directory.open.bmp"));
            theBmp.MakeTransparent();
            _imageList.Images.Add(theBmp);
        }

        /// <summary> method FillTree
        /// <para>This method is used to initially fill the treeView control with a list
        /// of available drives from which you can search for the desired folder.</para>
        /// </summary>
        private void FillTree()
        {
            DirectoryInfo directory;

            // clear out the old values
            _folderTreeView.Nodes.Clear();

            // loop through the drive letters and find the available drives.
            foreach (string sCurPath in Directory.GetLogicalDrives())
            {
                // Checking floppy takes too long to check now...
                if (sCurPath.StartsWith("A:"))
                {
                    continue;
                }
                try
                {
                    // get the directory informaiton for this path.
                    directory = new DirectoryInfo(sCurPath);

                    // if the retrieved directory information points to a valid
                    // directory or drive in this case, add it to the root of the 
                    // treeView.
                    if (directory.Exists == true)
                    {
                        TreeNode newNode = new TreeNode(directory.FullName);
                        newNode.ImageIndex = 0;
                        newNode.SelectedImageIndex = newNode.ImageIndex;
                        newNode.Tag = directory;

                        _folderTreeView.Nodes.Add(newNode);	// add the new node to the root level.

                        getSubDirs(newNode);			// scan for any sub folders on this drive.
                    }
                }
                catch
                {
                }
            }
        }


        /// <summary> method getSubDirs
        /// <para>this function will scan the specified parent for any subfolders 
        /// if they exist.  To minimize the memory usage, we only scan a single 
        /// folder level down from the existing, and only if it is needed.</para>
        /// <param name="parent">the parent folder in which to search for sub-folders.</param>
        /// </summary>
        private void getSubDirs(TreeNode parent)
        {
            DirectoryInfo directory;
            try
            {
                // if we have not scanned this folder before
                if (parent.Nodes.Count == 0)
                {
                    
                    directory = new DirectoryInfo(parent.FullPath);
                    foreach (DirectoryInfo dir in directory.GetDirectories())
                    {
                        TreeNode newNode = new TreeNode(dir.Name);
                        if (dir.Name == "RECYCLER" || dir.Name == "RECYCLED" || dir.Name == "Recycled")
                        {
                            newNode.ImageIndex = 1;
                            newNode.SelectedImageIndex = 2;
                        }
                        else
                        {
                            newNode.ImageIndex = 1;
                            newNode.SelectedImageIndex = 2;
                        }
                        newNode.Tag = dir;

                        parent.Nodes.Add(newNode);
                    }
                }

                // now that we have the children of the parent, see if they
                // have any child members that need to be scanned.  Scanning 
                // the first level of sub folders insures that you properly 
                // see the '+' or '-' expanding controls on each node that represents
                // a sub folder with it's own children.
                foreach (TreeNode node in parent.Nodes)
                {
                    // if we have not scanned this node before.
                    if (node.Nodes.Count == 0)
                    {
                        // get the folder information for the specified path.
                        directory = new DirectoryInfo(node.FullPath);

                        try 
                        {
                            // check this folder for any possible sub-folders
                            foreach (DirectoryInfo dir in directory.GetDirectories())
                            {
                                // make a new TreeNode and add it to the treeView.
                                TreeNode newNode = new TreeNode(dir.Name);
                                newNode.ImageIndex = 1;
                                newNode.SelectedImageIndex = 2;
                                newNode.Tag = dir;
                                node.Nodes.Add(newNode);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary> method fixPath
        /// <para>For some reason, the treeView will only work with paths constructed like the following example.
        /// "c:\\Program Files\Microsoft\...".  What this method does is strip the leading "\\" next to the drive
        /// letter.</para>
        /// <param name="node">the folder that needs it's path fixed for display.</param>
        /// <returns>The correctly formatted full path to the selected folder.</returns>
        /// </summary>
        private string fixPath(TreeNode node)
        {
            string sRet = "";
            try
            {
                sRet = node.FullPath;
                int index = sRet.IndexOf("\\\\");
                if (index > 1)
                {
                    sRet = node.FullPath.Remove(index, 1);
                }
            }
            catch
            {
            }

            return sRet;
        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Node2");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Node1", new System.Windows.Forms.TreeNode[] {
            treeNode4});
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Node0", new System.Windows.Forms.TreeNode[] {
            treeNode5});
            this._descriptionLabel = new System.Windows.Forms.Label();
            this._pathTextBox = new System.Windows.Forms.TextBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._selectButton = new System.Windows.Forms.Button();
            this._folderTreeView = new System.Windows.Forms.TreeView();
            this._imageList = new System.Windows.Forms.ImageList(this.components);
            this._newFolderButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _descriptionLabel
            // 
            this._descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._descriptionLabel.Location = new System.Drawing.Point(6, 6);
            this._descriptionLabel.Name = "_descriptionLabel";
            this._descriptionLabel.Size = new System.Drawing.Size(240, 15);
            this._descriptionLabel.TabIndex = 0;
            this._descriptionLabel.Text = "Description";
            // 
            // _pathTextBox
            // 
            this._pathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTextBox.Location = new System.Drawing.Point(9, 24);
            this._pathTextBox.Name = "_pathTextBox";
            this._pathTextBox.Size = new System.Drawing.Size(237, 20);
            this._pathTextBox.TabIndex = 1;
            this._pathTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this._pathTextBox_KeyUp);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(171, 234);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
            // 
            // _selectButton
            // 
            this._selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._selectButton.Location = new System.Drawing.Point(90, 234);
            this._selectButton.Name = "_selectButton";
            this._selectButton.Size = new System.Drawing.Size(75, 23);
            this._selectButton.TabIndex = 4;
            this._selectButton.Text = "OK";
            this._selectButton.Click += new System.EventHandler(this._selectButton_Click);
            // 
            // _folderTreeView
            // 
            this._folderTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._folderTreeView.HideSelection = false;
            this._folderTreeView.ImageIndex = 0;
            this._folderTreeView.ImageList = this._imageList;
            this._folderTreeView.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._folderTreeView.Location = new System.Drawing.Point(9, 50);
            this._folderTreeView.Name = "_folderTreeView";
            treeNode4.Name = "";
            treeNode4.Text = "Node2";
            treeNode5.Name = "";
            treeNode5.Text = "Node1";
            treeNode6.Name = "";
            treeNode6.Text = "Node0";
            this._folderTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode6});
            this._folderTreeView.SelectedImageIndex = 0;
            this._folderTreeView.Size = new System.Drawing.Size(237, 178);
            this._folderTreeView.TabIndex = 2;
            this._folderTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this._folderTreeView_BeforeExpand);
            this._folderTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this._folderTreeView_BeforeSelect);
            // 
            // _imageList
            // 
            this._imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this._imageList.ImageSize = new System.Drawing.Size(16, 16);
            this._imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _newFolderButton
            // 
            this._newFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._newFolderButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._newFolderButton.Location = new System.Drawing.Point(9, 234);
            this._newFolderButton.Name = "_newFolderButton";
            this._newFolderButton.Size = new System.Drawing.Size(75, 23);
            this._newFolderButton.TabIndex = 3;
            this._newFolderButton.Text = "Make Folder";
            this._newFolderButton.Visible = false;
            // 
            // FolderSelectDialog
            // 
            this.AcceptButton = this._selectButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(255, 266);
            this.Controls.Add(this._newFolderButton);
            this.Controls.Add(this._folderTreeView);
            this.Controls.Add(this._descriptionLabel);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._pathTextBox);
            this.Controls.Add(this._selectButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(263, 300);
            this.Name = "FolderSelectDialog";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Browse For Folder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary> method _folderTreeView_BeforeSelect
        /// <para>Before we select a tree node we want to make sure that we scan the soon to be selected
        /// tree node for any sub-folders.  this insures proper tree construction on the fly.</para>
        /// <param name="sender">The object that invoked this event</param>
        /// <param name="e">The TreeViewCancelEventArgs event arguments.</param>
        /// <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/>
        /// <see cref="System.Windows.Forms.TreeView"/>
        /// </summary>
        private void _folderTreeView_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            getSubDirs(e.Node);					// get the sub-folders for the selected node.
            _pathTextBox.Text = fixPath(e.Node);	// update the user selection text box.
            _pathTextBox.Select(_pathTextBox.Text.Length, 0); // Cursor at the end
            folder = new DirectoryInfo(e.Node.FullPath);	// get it's Directory info.
        }

        /// <summary> method _folderTreeView_BeforeExpand
        /// <para>Before we expand a tree node we want to make sure that we scan the soon to be expanded
        /// tree node for any sub-folders.  this insures proper tree construction on the fly.</para>
        /// <param name="sender">The object that invoked this event</param>
        /// <param name="e">The TreeViewCancelEventArgs event arguments.</param>
        /// <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/>
        /// <see cref="System.Windows.Forms.TreeView"/>
        /// </summary>
        private void _folderTreeView_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            getSubDirs(e.Node);					// get the sub-folders for the selected node.
            _pathTextBox.Text = fixPath(e.Node);	// update the user selection text box.
            folder = new DirectoryInfo(e.Node.FullPath);	// get it's Directory info.
        }

        /// <summary> method _cancelButton_Click
        /// <para>This method cancels the folder browsing.</para>
        /// </summary>
        private void _cancelButton_Click(object sender, System.EventArgs e)
        {
            folder = null;
            this.Close();
        }

        /// <summary> method _selectButton_Click
        /// <para>This method accepts which ever folder is selected and closes this application 
        /// with a DialogResult.OK result if you invoke this form though Form.ShowDialog().  
        /// In this example this method simply looks up the selected folder, and presents the 
        /// user with a message box displaying the name and path of the selected folder.
        /// </para>
        /// </summary>
        private void _selectButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string Description
        {
            get
            {
                return _descriptionLabel.Text;
            }
            set
            {
                _descriptionLabel.Text = value;
            }
        }

        public bool ShowNewFolderButton
        {
            get
            {
                return _newFolderButton.Visible;
            }
            set
            {
                _newFolderButton.Visible = value;
            }
        }

        /// <summary> 
        /// SelectedPath returns the current selected path.
        /// </summary>
        /// <returns>The correctly formatted full path to the selected folder.</returns>
        public string SelectedPath
        {
            get
            {
                if (folder != null &&
                    folder.Exists &&
                    _folderTreeView.SelectedNode != null)
                {
                    return fixPath(_folderTreeView.SelectedNode);
                }
                else
                {
                    return null;
                }
            }
        }

        private void _pathTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                string path = _pathTextBox.Text.Trim();
                if (path.Length >= 3 && (path.StartsWith(@"\\") || path.Substring(1, 2) == (@":\")))
                {
                    if (Directory.Exists(_pathTextBox.Text))
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        ArrayList dirs = new ArrayList();
                        DirectoryInfo iDir = dir;
                        while (iDir.Parent != null)
                        {
                            dirs.Add(iDir);
                            iDir = iDir.Parent;
                        }

                        dirs.Add(iDir);

                        TreeNode foundNode = null;
                        TreeNodeCollection nodes = _folderTreeView.Nodes;
                        for (int i = dirs.Count - 1; i >= 0; i--)
                        {
                            if (foundNode != null)
                            {
                                foundNode.Expand();
                            }

                            foundNode = null;
                            foreach (TreeNode node in nodes)
                            {
                                if (((DirectoryInfo)dirs[i]).Name.ToLower() == ((DirectoryInfo)node.Tag).Name.ToLower())
                                {
                                    foundNode = node;
                                    break;
                                }
                            }

                            if (foundNode == null)
                            {
                                break;
                            }
                            else
                            {
                                nodes = foundNode.Nodes;
                            }
                        }

                        _folderTreeView.SelectedNode = foundNode;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
