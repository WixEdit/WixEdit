using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace WixEdit.Forms
{
    public partial class OrderItemsForm : Form
    {
        public OrderItemsForm()
        {
            InitializeComponent();
        }

        public void SetItemsToOrder(XmlNodeList items, List<string> elementNames, string displayAttributeName)
        {
            itemListView.Items.Clear();
            foreach (XmlNode node in items)
            {
                if (elementNames.Contains(node.Name))
                {
                    XmlElement item = (XmlElement)node;
                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Name = item.GetAttribute(displayAttributeName);
                    listViewItem.Tag = item;
                    itemListView.Items.Add(listViewItem);
                }
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (itemListView.SelectedItems.Count == 1)
            {
                ListViewItem listViewItem = itemListView.SelectedItems[0];
                XmlElement item = (XmlElement)listViewItem.Tag;
                XmlNode parentNode = item.ParentNode;
                int i = 0;
                for (; i < parentNode.ChildNodes.Count; i++)
                {
                    if (parentNode.ChildNodes[i] == item)
                    {
                        parentNode.RemoveChild(item);
                        break;
                    }
                }

                for (; i >= 0; i--)
                {
                    if (parentNode.ChildNodes[i].Name == item.Name)
                    {
                        parentNode.InsertBefore(parentNode.ChildNodes[i], item);
                        int currentIndex = itemListView.Items.IndexOf(listViewItem);
                        itemListView.Items.Remove(listViewItem);
                        itemListView.Items.Insert(currentIndex - 1, listViewItem);
                        break;
                    }
                }
            }
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            if (itemListView.SelectedItems.Count == 1)
            {
                ListViewItem listViewItem = itemListView.SelectedItems[0];
                XmlElement item = (XmlElement)listViewItem.Tag;
                XmlNode parentNode = item.ParentNode;
                int i = parentNode.ChildNodes.Count - 1;
                for (; i >= 0; i--)
                {
                    if (parentNode.ChildNodes[i] == item)
                    {
                        parentNode.RemoveChild(item);
                        break;
                    }
                }

                for (; i < parentNode.ChildNodes.Count; i++)
                {
                    if (parentNode.ChildNodes[i].Name == item.Name)
                    {
                        parentNode.InsertBefore(parentNode.ChildNodes[i], item);
                        int currentIndex = itemListView.Items.IndexOf(listViewItem);
                        itemListView.Items.Remove(listViewItem);
                        itemListView.Items.Insert(currentIndex + 1, listViewItem);
                        break;
                    }
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}