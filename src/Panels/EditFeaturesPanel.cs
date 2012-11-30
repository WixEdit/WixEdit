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
using System.Xml;
using System.Windows.Forms;

using WixEdit.Xml;
using WixEdit.Forms;
using WixEdit.Controls;

namespace WixEdit.Panels
{
    /// <summary>
    /// Panel to edit features.
    /// </summary>
    public class EditFeaturesPanel : DisplayTreeBasePanel
    {
        public EditFeaturesPanel(WixFiles wixFiles)
            : base(wixFiles, "/wix:Wix/*/wix:Feature", "Feature", "Id", false)
        {
            LoadData();
        }

        /// <summary>
        /// Runs when the user displays the context menu for an item in 
        /// the tree control on the Features panel.
        /// 
        /// Adds the "Add components" menu item if the selected node is
        /// a feature
        /// </summary>
        /// <param name="node">The selected node</param>
        /// <param name="currTreeViewContextMenu">The context menu to extend</param>
        protected override void AddCustomTreeViewContextMenuItems(XmlNode node, ContextMenu currTreeViewContextMenu)
        {
            if (node.Name == "Feature")
            {
                IconMenuItem item = new IconMenuItem("Select Components to add");
                item.Click += new EventHandler(mnuAddComponents_Click);
                currTreeViewContextMenu.MenuItems.Add(2, new IconMenuItem("-")); // "separator"
                currTreeViewContextMenu.MenuItems.Add(3, item);
            }
        }

        void mnuAddComponents_Click(object sender, EventArgs e)
        {
            if (currTreeView.SelectedNode == null)
            {
                return;
            }

            XmlNodeList components = WixFiles.WxsDocument.GetElementsByTagName("Component");
            XmlNode node = currTreeView.SelectedNode.Tag as XmlNode;

            ArrayList componentIds = new ArrayList();
            foreach (XmlNode component in components)
            {
                if (component.Attributes["Id"] != null &&
                    component.Attributes["Id"].Value != String.Empty)
                {
                    componentIds.Add(component.Attributes["Id"].Value);
                }
            }

            SelectStringForm frm = new SelectStringForm("Select components");
            frm.PossibleStrings = componentIds.ToArray(typeof(String)) as String[];
            if (DialogResult.OK != frm.ShowDialog())
            {
                return;
            }

            foreach (string componentId in frm.SelectedStrings)
            {
                TreeNode newNode = CreateNewSubElement("ComponentRef");
                XmlNode newCompRef = newNode.Tag as XmlNode;
                XmlAttribute newAttr = WixFiles.WxsDocument.CreateAttribute("Id");
                newNode.Text = componentId;
                newAttr.Value = componentId;
                newCompRef.Attributes.Append(newAttr);
            }
        }

        protected override void PopupPanelContextMenu(System.Object sender, System.EventArgs e)
        {
            // clear menu and add import menu
            base.PopupPanelContextMenu(sender, e);

            // add custom menu, index has to be used!!!
            IconMenuItem subMenuItem = new IconMenuItem("New Feature", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            subMenuItem.Click += new EventHandler(NewCustomElement_Click);
            PanelContextMenu.MenuItems.Add(0, subMenuItem);
        }
    }
}