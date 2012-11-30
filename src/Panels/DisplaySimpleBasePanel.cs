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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using WixEdit.PropertyGridExtensions;
using WixEdit.Xml;
using WixEdit.Controls;
using WixEdit.Forms;

namespace WixEdit.Panels
{
    /// <summary>
    /// Summary description for DisplayBasePanel.
    /// </summary>
    public abstract class DisplaySimpleBasePanel : DisplayBasePanel
    {
        private string currentValueName;

        public DisplaySimpleBasePanel(WixFiles wixFiles, string xpath, string elementName, string keyName, string valueName)
            : base(wixFiles, xpath, elementName, keyName)
        {
            // when valueName is null, the inner text is used.

            Reload += new ReloadHandler(ReloadData);

            currentValueName = valueName;

            InitializeComponent();
            CreateControl();
        }

        public DisplaySimpleBasePanel(WixFiles wixFiles, string xpath, string elementName, string keyName)
            : base(wixFiles, xpath, elementName, keyName)
        {
            Reload += new ReloadHandler(ReloadData);

            InitializeComponent();
            CreateControl();
        }

        public string CurrentValueName
        {
            get
            {
                return currentValueName;
            }
            set
            {
                currentValueName = value;
            }
        }

        #region Initialize Controls
        private void InitializeComponent()
        {
            CustomPropertyGrid currGrid = new CustomPropertyGrid();
            ContextMenu currGridContextMenu = new ContextMenu();

            currGrid.Dock = DockStyle.Fill;
            currGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            currGrid.Location = new Point(140, 0);
            currGrid.Name = "_currGrid";
            currGrid.Size = new Size(269, 266);
            currGrid.TabIndex = 1;
            currGrid.PropertySort = PropertySort.Alphabetical;
            currGrid.ToolbarVisible = false;
            currGrid.HelpVisible = false;
            currGrid.ContextMenu = currGridContextMenu;
            currGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            Controls.Add(currGrid);

            CurrentGrid = currGrid;
            CurrentGridContextMenu = currGridContextMenu;
        }
        #endregion

        protected void LoadData()
        {
            CurrentList = WixFiles.WxsDocument.SelectNodes(CurrentXPath, WixFiles.WxsNsmgr);
            CurrentGrid.SelectedObject = GetPropertyAdapter();

            AssignParentNode();
        }

        public virtual void OnPropertyGridPopupContextMenu(object sender, EventArgs e)
        {
            if (CurrentGrid.SelectedObject == null)
            {
                return;
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");
            MenuItem menuItem1 = new IconMenuItem("Add &New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            MenuItem menuItem3 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
            MenuItem menuItem4 = new IconMenuItem("&Rename");
            MenuItem menuItem5 = new IconMenuItem("&Import XML", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));

            menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
            menuItem3.Click += new EventHandler(OnDeletePropertyGridItem);
            menuItem4.Click += new EventHandler(OnRenamePropertyGridItem);
            menuItem5.Click += new EventHandler(OnImportPropertyGridItem);

            CurrentGridContextMenu.MenuItems.Clear();
            CurrentGridContextMenu.MenuItems.Add(menuItem1);
            if (CurrentGrid.SelectedGridItem.PropertyDescriptor != null)
            {
                CurrentGridContextMenu.MenuItems.Add(menuItem3);
                CurrentGridContextMenu.MenuItems.Add(menuItem4);
            }

            CurrentGridContextMenu.MenuItems.Add(menuItem5);
        }

        public virtual void OnNewPropertyGridItem(object sender, EventArgs e)
        {
            if (CurrentParent == null)
            {
                MessageBox.Show(String.Format("No location found to add \"{0}\" element, need element like module or product!", CurrentElementName));
                return;
            }

            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Enter Resource Name";
            if (DialogResult.OK == frm.ShowDialog())
            {
                WixFiles.UndoManager.BeginNewCommandRange();
                XmlElement newProp = WixFiles.WxsDocument.CreateElement(CurrentElementName, WixFiles.WixNamespaceUri);

                XmlAttribute newAttr = WixFiles.WxsDocument.CreateAttribute(CurrentKeyName);
                newAttr.Value = frm.SelectedString;
                newProp.Attributes.Append(newAttr);

                if (currentValueName != null)
                {
                    newAttr = WixFiles.WxsDocument.CreateAttribute(currentValueName);
                    newProp.Attributes.Append(newAttr);
                }

                InsertNewXmlNode(CurrentParent, newProp);

                RefreshGrid(frm.SelectedString);
            }
        }

        public virtual void OnImportPropertyGridItem(object sender, EventArgs e)
        {
            if (this.ImportItems(CurrentXPath))
            {
                this.RefreshGrid();
            }
            else if (CurrentElementName != null && this.ImportItems("//wix:" + CurrentElementName))
            {
                this.RefreshGrid();
            }
        }

        public virtual void OnAddFilePropertyGridItem(object sender, EventArgs e)
        {
            string filePath = string.Empty;
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.RestoreDirectory = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openDialog.FileName;
                EnterStringForm frm;
                if (filePath != string.Empty & File.Exists(filePath))
                {
                    frm = new EnterStringForm(Path.GetFileName(filePath));
                }
                else
                {
                    frm = new EnterStringForm();
                }
                frm.Text = "Enter Resource Name";
                if (DialogResult.OK == frm.ShowDialog())
                {
                    WixFiles.UndoManager.BeginNewCommandRange();

                    XmlElement newProp = WixFiles.WxsDocument.CreateElement(CurrentElementName, WixFiles.WixNamespaceUri);

                    XmlAttribute newAttr = WixFiles.WxsDocument.CreateAttribute(CurrentKeyName);
                    newAttr.Value = frm.SelectedString;
                    newProp.Attributes.Append(newAttr);
                    newAttr = WixFiles.WxsDocument.CreateAttribute("SourceFile");
                    newAttr.Value = filePath;
                    newProp.Attributes.Append(newAttr);
                    InsertNewXmlNode(CurrentParent, newProp);
                    RefreshGrid(frm.SelectedString);
                }
            }
            openDialog.Dispose();
        }

        public void OnDeletePropertyGridItem(object sender, EventArgs e)
        {
            PropertyAdapterBase adapter = CurrentGrid.SelectedObject as PropertyAdapterBase;

            XmlNode element = GetSelectedGridObject();
            if (element != null)
            {
                WixFiles.UndoManager.BeginNewCommandRange();

                adapter.RemoveProperty(element);
                RefreshGrid(string.Empty);
            }
        }

        public virtual void OnRenamePropertyGridItem(object sender, EventArgs e)
        {
            XmlNode element = GetSelectedGridObject();
            if (element != null)
            {
                XmlAttribute att = element.Attributes[CurrentKeyName];
                EnterStringForm frm = new EnterStringForm(att.Value);
                frm.Text = "Enter Name";
                if (DialogResult.OK == frm.ShowDialog())
                {
                    WixFiles.UndoManager.BeginNewCommandRange();
                    att.Value = frm.SelectedString;
                    RefreshGrid();

                    ShowNode(att);
                }
            }
        }

        public override bool IsOwnerOfNode(XmlNode node)
        {
            XmlNode showable = GetShowableNode(node);
            foreach (XmlNode xmlNode in WixFiles.WxsDocument.SelectNodes(CurrentXPath, WixFiles.WxsNsmgr))
            {
                if (showable == xmlNode)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ShowNode(XmlNode node)
        {
            CurrentGrid.SelectedObject = GetPropertyAdapter();

            if (CurrentGrid.SelectedGridItem != null && CurrentGrid.SelectedGridItem.Parent != null)
            {
                string val = null;
                XmlNode showable = GetShowableNode(node);
                if (showable.Attributes[CurrentKeyName] != null)
                {
                    val = showable.Attributes[CurrentKeyName].Value;
                }
                foreach (GridItem item in CurrentGrid.SelectedGridItem.Parent.GridItems)
                {
                    if (val != null && val == item.Label)
                    {
                        CurrentGrid.SelectedGridItem = item;
                        break;
                    }
                }
            }
        }

        public override XmlNode GetShowingNode()
        {
            XmlNodeList properties = WixFiles.WxsDocument.SelectNodes(currentXPath, WixFiles.WxsNsmgr);
            foreach (XmlNode item in properties)
            {
                if (item.Attributes[CurrentKeyName].Value == currentGrid.SelectedGridItem.Label)
                {
                    return item;
                }
            }

            return null;
        }


        public override void ReloadData()
        {
            CurrentGrid.SelectedObject = null;
            LoadData();
        }

        protected void RefreshGrid(string selectString)
        {
            RefreshGrid();

            if (CurrentList.Count > 0 & CurrentGrid.SelectedObjects.Length > 0)
            {
                if (CurrentGrid.SelectedGridItem != null && CurrentGrid.SelectedGridItem.Parent != null)
                {
                    foreach (GridItem it in CurrentGrid.SelectedGridItem.Parent.GridItems)
                    {
                        if (it.Label == selectString)
                        {
                            CurrentGrid.SelectedGridItem = it;
                            break;
                        }
                    }
                }
            }
        }

        protected void RefreshGrid()
        {
            CurrentList = WixFiles.WxsDocument.SelectNodes(CurrentXPath, WixFiles.WxsNsmgr);

            CurrentGrid.SelectedObject = null;
            CurrentGrid.SelectedObject = GetPropertyAdapter();

            CurrentGrid.Update();
        }

        protected virtual XmlNode GetSelectedGridObject()
        {
            CustomXmlPropertyDescriptorBase desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as CustomXmlPropertyDescriptorBase;
            return desc.XmlElement;
        }

        protected abstract object GetPropertyAdapter();

        private delegate void ReloadHandler();
        private event ReloadHandler Reload;
    }
}
