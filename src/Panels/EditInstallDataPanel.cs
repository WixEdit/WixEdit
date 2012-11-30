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
    public class EditInstallDataPanel : DisplayBasePanel
    {
        protected TabControl tabControl;
        protected TabPage editFilesTabPage;
        protected EditFilesPanel editFilesPanel;
        protected TabPage editFeaturesTabPage;
        protected EditFeaturesPanel editFeaturesPanel;

        public EditInstallDataPanel(WixFiles wixFiles)
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

            editFilesPanel = new EditFilesPanel(WixFiles);
            editFilesPanel.Dock = DockStyle.Fill;

            editFilesTabPage = new TabPage("Files");
            editFilesTabPage.Controls.Add(editFilesPanel);

            tabControl.TabPages.Add(editFilesTabPage);


            editFeaturesPanel = new EditFeaturesPanel(WixFiles);
            editFeaturesPanel.Dock = DockStyle.Fill;

            editFeaturesTabPage = new TabPage("Features");
            editFeaturesTabPage.Controls.Add(editFeaturesPanel);

            tabControl.TabPages.Add(editFeaturesTabPage);

        }
        #endregion

        public override bool IsOwnerOfNode(XmlNode node)
        {
            return (editFilesPanel.IsOwnerOfNode(node) || editFeaturesPanel.IsOwnerOfNode(node));
        }

        public override void ShowNode(XmlNode node)
        {
            if (editFilesPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editFilesTabPage;
                editFilesPanel.ShowNode(node);
            }
            else if (editFeaturesPanel.IsOwnerOfNode(node))
            {
                tabControl.SelectedTab = editFeaturesTabPage;
                editFeaturesPanel.ShowNode(node);
            }
            else
            {
                tabControl.SelectedTab = editFilesTabPage;
                editFilesPanel.ReloadData();
            }
        }


        public override XmlNode GetShowingNode()
        {
            if (tabControl.SelectedTab == editFilesTabPage)
            {
                return editFilesPanel.GetShowingNode();
            }
            else if (tabControl.SelectedTab == editFeaturesTabPage)
            {
                return editFeaturesPanel.GetShowingNode();
            }

            return null;
        }

        public override void ReloadData()
        {
            editFilesPanel.ReloadData();
            editFeaturesPanel.ReloadData();
        }
    }
}
