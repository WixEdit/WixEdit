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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.Controls;
using WixEdit.PropertyGridExtensions;
using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.Panels
{
    /// <summary>
    /// Editing of dialogs.
    /// </summary>
    public class EditCustomTableDataPanel : DisplayBasePanel
    {
        #region Controls

        private ListView wxsCustomTables;
        private Splitter splitter1;
        private Panel panel1;
        private DataGridView dataGridView;
        private DataGridViewTextBoxColumn testColumn;

        #endregion

        public EditCustomTableDataPanel(WixFiles wixFiles)
            : base(wixFiles)
        {
            InitializeComponent();
        }

        private void OnResizeWxsCustomTables(object sender, EventArgs e)
        {
            if (wxsCustomTables.Columns.Count > 0 && wxsCustomTables.Columns[0] != null)
            {
                wxsCustomTables.Columns[0].Width = wxsCustomTables.ClientSize.Width - 4;
            }
        }

        #region Initialize Controls
        private void InitializeComponent()
        {
            wxsCustomTables = new ListView();
            splitter1 = new Splitter();
            panel1 = new Panel();

            // 
            // wxsCustomTables
            // 
            wxsCustomTables.Dock = DockStyle.Left;
            wxsCustomTables.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
            wxsCustomTables.Location = new Point(0, 0);
            wxsCustomTables.Name = "wxsCustomTables";
            wxsCustomTables.Size = new Size(140, 264);
            wxsCustomTables.TabIndex = 0;
            wxsCustomTables.View = View.Details;
            wxsCustomTables.MultiSelect = false;
            wxsCustomTables.HideSelection = false;
            wxsCustomTables.FullRowSelect = true;
            wxsCustomTables.GridLines = false;
            wxsCustomTables.SelectedIndexChanged += new EventHandler(OnSelectedCustomTableChanged);

            splitter1.Dock = DockStyle.Left;
            splitter1.Location = new Point(140, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(2, 266);
            splitter1.TabIndex = 7;
            splitter1.TabStop = false;

            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.testColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();

            // 
            // dataGridView
            // 
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            dataGridView.Location = new System.Drawing.Point(0, 0);
            dataGridView.Name = "dataGridView";
            dataGridView.Size = new System.Drawing.Size(292, 266);
            dataGridView.TabIndex = 0;
            dataGridView.DataError += new DataGridViewDataErrorEventHandler(dataGridView_DataError);
            dataGridView.AllowUserToAddRows = true;
            dataGridView.AllowUserToDeleteRows = true;
            dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;

            panel1.Controls.Add(dataGridView);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(142, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 266);
            panel1.TabIndex = 9;

            Controls.Add(panel1);
            Controls.Add(splitter1);
            Controls.Add(wxsCustomTables);

            wxsCustomTables.Columns.Add("Item Column", -2, HorizontalAlignment.Left);
            wxsCustomTables.HeaderStyle = ColumnHeaderStyle.None;
            wxsCustomTables.Resize += new EventHandler(OnResizeWxsCustomTables);
            LoadData();
        }

        protected override void OnParentBindingContextChanged(EventArgs e)
        {
            dataGridView.CancelEdit();

            base.OnParentBindingContextChanged(e);
        }

        void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                e.ThrowException = false;
            }
        }

        #endregion

        protected void LoadData()
        {
            wxsCustomTables.Items.Clear();

            XmlNodeList customTables = WixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:CustomTable", WixFiles.WxsNsmgr);
            foreach (XmlNode customTable in customTables)
            {
                XmlAttribute attr = customTable.Attributes["Id"];
                if (attr != null)
                {
                    ListViewItem toAdd = new ListViewItem(attr.Value);
                    toAdd.Tag = customTable;

                    wxsCustomTables.Items.Add(toAdd);
                }
            }
        }


        #region DisplayBasePanel overrides and helpers

        public override void ReloadData()
        {
            ShowCustomTable(null);

            LoadData();
        }

        public override bool IsOwnerOfNode(XmlNode node)
        {
            XmlNodeList tables = WixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:CustomTable/wix:Row", WixFiles.WxsNsmgr);
            return FindNode(GetShowableNode(node), tables);
        }

        private bool FindNode(XmlNode nodeToFind, IEnumerable xmlNodes)
        {
            foreach (XmlNode node in xmlNodes)
            {
                if (node == nodeToFind)
                {
                    return true;
                }

                if (FindNode(nodeToFind, node.ChildNodes))
                {
                    return true;
                }
            }

            return false;
        }

        public override void ShowNode(XmlNode node)
        {
            this.SuspendLayout();

            LoadData();

            XmlNode showable = GetShowableNode(node);

            XmlNode table = showable;
            while (table.Name != "CustomTable")
            {
                table = table.ParentNode;
            }

            foreach (ListViewItem item in wxsCustomTables.Items)
            {
                item.Selected = false;
            }

            foreach (ListViewItem item in wxsCustomTables.Items)
            {
                if (table == item.Tag)
                {
                    item.Selected = true;
                    /*
                    ShowCustomTable(null);

                    ShowCustomTable(table);
                    */
                    break;
                }
            }

            this.ResumeLayout();
        }

        public override XmlNode GetShowingNode()
        {
            if (wxsCustomTables.SelectedItems.Count > 0 && wxsCustomTables.SelectedItems[0] != null)
            {
                XmlNode table = (XmlNode)wxsCustomTables.SelectedItems[0].Tag;
                XmlNode row = table.SelectSingleNode("wix:Row", WixFiles.WxsNsmgr);

                if (row != null)
                {
                    return row;
                }
                else
                {
                    return table;
                }

            }

            return null;
        }

        #endregion


        private void OnSelectedCustomTableChanged(object sender, EventArgs e)
        {
            if (wxsCustomTables.SelectedItems.Count > 0 && wxsCustomTables.SelectedItems[0] != null)
            {
                string currentTableId = wxsCustomTables.SelectedItems[0].Text;
                XmlNode table = GetCustomTableNode(currentTableId);

                ShowCustomTable(table);
            }
        }


        protected XmlNode GetCustomTableNode(string tableId)
        {
            XmlNode table = WixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/*/wix:CustomTable[@Id='{0}']", tableId), WixFiles.WxsNsmgr);

            return table;
        }

        XmlNode currentTable;
        private void ShowCustomTable(XmlNode table)
        {
            currentTable = table;

            dataGridView.Columns.Clear();
            dataGridView.DataSource = null;

            if (table != null)
            {
                foreach (XmlNode xmlNode in table.ChildNodes)
                {
                    if (xmlNode.Name != "Column")
                    {
                        continue;
                    }

                    XmlElement xmlElement = (XmlElement)xmlNode;

                    string id = xmlElement.GetAttribute("Id");
                    if (String.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    DataGridViewColumn newColumn = null;

                    string type = xmlElement.GetAttribute("Type");

                    switch (type)
                    {
                        case "integer":
                        case "int":
                            newColumn = new NumericTextBoxColumn();
                            break;
                        case "binary":
                            newColumn = new FileSelectColumn();
                            ((FileSelectColumn)newColumn).WixFiles = WixFiles;
                            break;
                        default:
                            newColumn = new DataGridViewTextBoxColumn();
                            break;
                    }

                    DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
                    dataGridViewCellStyle.NullValue = null;

                    newColumn.DefaultCellStyle = dataGridViewCellStyle;
                    newColumn.HeaderText = id;
                    newColumn.Name = id;
                    newColumn.DataPropertyName = id;
                    newColumn.MinimumWidth = 40;

                    this.dataGridView.Columns.Add(newColumn);
                }

                CustomTableRowBindingList data = new CustomTableRowBindingList();
                data.AllowEdit = true;
                data.AllowNew = true;
                data.AllowRemove = true;
                data.AddingNew += new AddingNewEventHandler(OnAddingNew);
                data.RaiseListChangedEvents = true;

                foreach (XmlNode xmlNode in table.SelectNodes("wix:Row", WixFiles.WxsNsmgr))
                {
                    if (xmlNode.Name != "Row")
                    {
                        continue;
                    }

                    XmlElement xmlElement = (XmlElement)xmlNode;

                    data.Add(new CustomTableRowElementAdapter(xmlElement, WixFiles));
                }

                dataGridView.DataSource = data;
            }

            dataGridView.Focus();

            dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        void OnAddingNew(object sender, AddingNewEventArgs e)
        {
            WixFiles.UndoManager.BeginNewCommandRange();
            XmlElement rowElement = currentTable.OwnerDocument.CreateElement("Row", WixFiles.WixNamespaceUri);
            currentTable.AppendChild(rowElement);

            e.NewObject = new CustomTableRowElementAdapter(rowElement, WixFiles);
        }
    }

    public class PropertyComparer<T> : IComparer<T>
    {
        PropertyDescriptor property;
        ListSortDirection sortDirection;

        // Constructor
        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            this.property = property;
            this.sortDirection = direction;
        }

        // IComparer<T> interface
        public int Compare(T xValue, T yValue)
        {
            int direction = 1;
            if (sortDirection == ListSortDirection.Descending)
            {
                direction = -1;
            }

            string x = (string)property.GetValue(xValue);
            string y = (string)property.GetValue(yValue);

            return direction * String.Compare(x, y);
        }

        public bool Equals(T xValue, T yValue)
        {
            string x = (string)property.GetValue(xValue);
            string y = (string)property.GetValue(yValue);

            return String.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            string value = (string)property.GetValue(obj);
            return value.GetHashCode();
        }
    }

    public class CustomTableRowBindingList : BindingList<CustomTableRowElementAdapter>
    {
        private bool isSorted;
        private ListSortDirection sortDirectionCore;
        private PropertyDescriptor sortPropertyCore;

        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return sortDirectionCore;
            }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return sortPropertyCore;
            }
        }

        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        protected override void RemoveSortCore()
        {
            isSorted = false;
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            this.sortDirectionCore = direction;
            this.sortPropertyCore = property;

            // Get list to sort
            List<CustomTableRowElementAdapter> items = this.Items as List<CustomTableRowElementAdapter>;

            // Apply and set the sort, if items to sort
            if (items != null)
            {
                PropertyComparer<CustomTableRowElementAdapter> pc = new PropertyComparer<CustomTableRowElementAdapter>(property, direction);
                items.Sort(pc);
                isSorted = true;
            }
            else
            {
                isSorted = false;
            }

            // Let bound controls know they should refresh their views
            this.OnListChanged(
              new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override bool IsSortedCore
        {
            get { return isSorted; }
        }

        protected override void RemoveItem(int index)
        {
            base[index].RemoveElement();

            base.RemoveItem(index);
        }
    }
}
