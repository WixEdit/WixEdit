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
    public class EditResourcesPanel : DisplayBasePanel
    {
        protected TabControl tabControl;
        protected TabPage editBinariesTabPage;
        protected EditBinariesPanel editBinariesPanel;
        protected TabPage editIconsTabPage;
        protected EditIconsPanel editIconsPanel;

        public EditResourcesPanel(WixFiles wixFiles)
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

            editBinariesPanel = new EditBinariesPanel(WixFiles);
            editBinariesPanel.Dock = DockStyle.Fill;
            editBinariesTabPage = new TabPage("Binaries");
            editBinariesTabPage.Controls.Add(editBinariesPanel);
            tabControl.TabPages.Add(editBinariesTabPage);


            editIconsPanel = new EditIconsPanel(WixFiles);
            editIconsPanel.Dock = DockStyle.Fill;
            editIconsTabPage = new TabPage("Icons");
            editIconsTabPage.Controls.Add(editIconsPanel);
            tabControl.TabPages.Add(editIconsTabPage);
        }
        #endregion

        public override MenuItem Menu
        {
            get
            {
                return editBinariesPanel.Menu;
            }
        }

        public override bool IsOwnerOfNode(XmlNode node)
        {
            bool ret = (editBinariesPanel.IsOwnerOfNode(node) || editIconsPanel.IsOwnerOfNode(node));
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
            if (editBinariesPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editBinariesTabPage;
                editBinariesPanel.ShowNode(node);
            }
            else if (editIconsPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editIconsTabPage;
                editIconsPanel.ShowNode(node);
            }
            else
            {
                tabControl.SelectedTab = editBinariesTabPage;
                editBinariesPanel.ReloadData();
            }
        }

        public override XmlNode GetShowingNode()
        {
            if (tabControl.SelectedTab == editBinariesTabPage)
            {
                return editBinariesPanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editIconsTabPage)
            {
                return editIconsPanel.GetShowingNode();
            }

            return null;
        }

        public override void ReloadData()
        {
            editBinariesPanel.ReloadData();
            editIconsPanel.ReloadData();
        }
    }
}
