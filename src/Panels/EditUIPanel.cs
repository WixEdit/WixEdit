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
using System.Xml;
using System.Windows.Forms;

using WixEdit.Controls;
using WixEdit.Xml;

namespace WixEdit.Panels
{
    /// <summary>
    /// Panel to edit install data.
    /// </summary>
    public class EditUIPanel : DisplayBasePanel
    {
        protected TabControl tabControl;
        protected TabPage editDialogTabPage;
        protected EditDialogPanel editDialogPanel;
        protected TabPage editUISequenceTabPage;
        protected EditUISequencePanel editUISequencePanel;
        protected TabPage editUITextTabPage;
        protected EditUITextPanel editUITextPanel;
        protected TabPage editProgressTextTabPage;
        protected EditProgressTextPanel editProgressTextPanel;
        protected TabPage editErrorTabPage;
        protected EditErrorPanel editErrorPanel;

        public EditUIPanel(WixFiles wixFiles)
            : base(wixFiles)
        {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent()
        {
            tabControl = new CustomTabControl();
            tabControl.Dock = DockStyle.Fill;

            Controls.Add(tabControl);

            editDialogPanel = new EditDialogPanel(WixFiles);
            editDialogPanel.Dock = DockStyle.Fill;
            editDialogTabPage = new TabPage("Dialogs");
            editDialogTabPage.Controls.Add(editDialogPanel);
            tabControl.TabPages.Add(editDialogTabPage);


            editUISequencePanel = new EditUISequencePanel(WixFiles);
            editUISequencePanel.Dock = DockStyle.Fill;
            editUISequenceTabPage = new TabPage("UI Sequence");
            editUISequenceTabPage.Controls.Add(editUISequencePanel);
            tabControl.TabPages.Add(editUISequenceTabPage);


            editUITextPanel = new EditUITextPanel(WixFiles);
            editUITextPanel.Dock = DockStyle.Fill;
            editUITextTabPage = new TabPage("UI Text");
            editUITextTabPage.Controls.Add(editUITextPanel);
            tabControl.TabPages.Add(editUITextTabPage);


            editProgressTextPanel = new EditProgressTextPanel(WixFiles);
            editProgressTextPanel.Dock = DockStyle.Fill;
            editProgressTextTabPage = new TabPage("Progress Text");
            editProgressTextTabPage.Controls.Add(editProgressTextPanel);
            tabControl.TabPages.Add(editProgressTextTabPage);


            editErrorPanel = new EditErrorPanel(WixFiles);
            editErrorPanel.Dock = DockStyle.Fill;
            editErrorTabPage = new TabPage("Error Text");
            editErrorTabPage.Controls.Add(editErrorPanel);
            tabControl.TabPages.Add(editErrorTabPage);
        }
        #endregion

        public override MenuItem Menu
        {
            get
            {
                return editDialogPanel.Menu;
            }
        }

        public override bool IsOwnerOfNode(XmlNode node)
        {
            bool ret = (editDialogPanel.IsOwnerOfNode(node) || editUISequencePanel.IsOwnerOfNode(node) || editUITextPanel.IsOwnerOfNode(node) || editProgressTextPanel.IsOwnerOfNode(node) || editErrorPanel.IsOwnerOfNode(node));
            if (ret == false)
            {
                if (node.Name == "UI")
                {
                    ret = true;
                }
            }

            return ret;
        }

        public override void ShowNode(XmlNode node)
        {
            if (editDialogPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editDialogTabPage;
                editDialogPanel.ShowNode(node);
            }
            else if (editUISequencePanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editUISequenceTabPage;
                editUISequencePanel.ShowNode(node);
            }
            else if (editUITextPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editUITextTabPage;
                editUITextPanel.ShowNode(node);
            }
            else if (editProgressTextPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editProgressTextTabPage;
                editProgressTextPanel.ShowNode(node);
            }
            else if (editErrorPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editErrorTabPage;
                editErrorPanel.ShowNode(node);
            }
            else
            {
                tabControl.SelectedTab = editDialogTabPage;
                editDialogPanel.ReloadData();
            }
        }

        public override XmlNode GetShowingNode()
        {
            if (tabControl.SelectedTab == editDialogTabPage)
            {
                return editDialogPanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editUISequenceTabPage)
            {
                return editUISequencePanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editUITextTabPage)
            {
                return editUITextPanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editProgressTextTabPage)
            {
                return editProgressTextPanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editErrorTabPage)
            {
                return editErrorPanel.GetShowingNode();
            }

            return null;
        }

        public override void ReloadData()
        {
            editDialogPanel.ReloadData();
            editUISequencePanel.ReloadData();
            editUITextPanel.ReloadData();
            editProgressTextPanel.ReloadData();
            editErrorPanel.ReloadData();
        }

        public void CloseCurrentDialog()
        {
            editDialogPanel.CloseCurrentDialog();
        }
    }
}
