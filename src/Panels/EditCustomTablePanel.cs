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
    public class EditCustomTablePanel : DisplayBasePanel
    {
        protected TabControl tabControl;
        protected TabPage editDefinitionTabPage;
        protected EditCustomTableDefinitionPanel editDefinitionPanel;
        protected TabPage editDataTabPage;
        protected EditCustomTableDataPanel editDataPanel;

        public EditCustomTablePanel(WixFiles wixFiles)
            : base(wixFiles)
        {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent()
        {
            tabControl = new CustomTabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.SelectedIndexChanged += new EventHandler(tabControl_SelectedIndexChanged);

            Controls.Add(tabControl);

            editDefinitionPanel = new EditCustomTableDefinitionPanel(WixFiles);
            editDefinitionPanel.Dock = DockStyle.Fill;
            editDefinitionTabPage = new TabPage("Table Definitions");
            editDefinitionTabPage.Controls.Add(editDefinitionPanel);
            tabControl.TabPages.Add(editDefinitionTabPage);

            editDataPanel = new EditCustomTableDataPanel(WixFiles);
            editDataPanel.Dock = DockStyle.Fill;
            editDataTabPage = new TabPage("Table Data");
            editDataTabPage.Controls.Add(editDataPanel);
            tabControl.TabPages.Add(editDataTabPage);
        }

        void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == editDataTabPage)
            {
                editDataPanel.ReloadData();
            }
        }

        #endregion

        public override MenuItem Menu
        {
            get
            {
                return editDefinitionPanel.Menu;
            }
        }

        public override bool IsOwnerOfNode(XmlNode node)
        {
            bool ret = (editDefinitionPanel.IsOwnerOfNode(node) || editDataPanel.IsOwnerOfNode(node));
            if (ret == false)
            {
                if (node.Name == "CustomTable")
                {
                    ret = true;
                }
            }

            return ret;
        }

        public override void ShowNode(XmlNode node)
        {
            if (editDefinitionPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editDefinitionTabPage;
                editDefinitionPanel.ShowNode(node);
            }
            else if (editDataPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editDataTabPage;
                editDataPanel.ShowNode(node);
            }
            else
            {
                tabControl.SelectedTab = editDefinitionTabPage;
                editDefinitionPanel.ReloadData();
            }
        }

        public override XmlNode GetShowingNode()
        {
            if (tabControl.SelectedTab == editDefinitionTabPage)
            {
                return editDefinitionPanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editDataTabPage)
            {
                return editDataPanel.GetShowingNode();
            }

            return null;
        }

        public override void ReloadData()
        {
            editDefinitionPanel.ReloadData();
            editDataPanel.ReloadData();
        }
    }
}
