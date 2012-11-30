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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.Controls;
using WixEdit.Xml;

namespace WixEdit.Forms
{
    public delegate void DesignerFormItemHandler(XmlNode item);
    public delegate void DesignerFormNewItemHandler(XmlNode item, Point position, string controlType);

    public class DesignerForm : Form
    {
        Hashtable controlMap;
        WixFiles wixFiles;
        XmlNode dialogElement;
        string selectedNodeId;

        string[] newControls;
        ContextMenu newContextMenu;
        IconMenuItem newControlElementMenu;

        public event DesignerFormItemHandler ItemChanged;
        public event DesignerFormItemHandler ItemDeleted;
        public event DesignerFormItemHandler SelectionChanged;
        public event DesignerFormNewItemHandler NewControl;

        public DesignerForm(WixFiles wixFiles, XmlNode dialog)
        {
            dialogElement = dialog;
            controlMap = new Hashtable();
            this.wixFiles = wixFiles;
            this.MouseClick += new MouseEventHandler(DesignerForm_MouseClick);
        }

        public string[] NewControls
        {
            set
            {
                newControls = value;
                newControlElementMenu = new IconMenuItem("New Control", new Bitmap(WixFiles.GetResourceStream("elements.control.bmp")));

                foreach (string controlType in newControls)
                {
                    MenuItem menuItem = new MenuItem(controlType);
                    menuItem.Click += new EventHandler(NewControlElement_Click);
                    newControlElementMenu.MenuItems.Add(menuItem);
                }

                newContextMenu = new ContextMenu(new MenuItem[] { newControlElementMenu });
            }
        }

        Point contextMenuClickPosition;

        void DesignerForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuClickPosition = e.Location;
                newContextMenu.Show(this, contextMenuClickPosition);
            }
        }

        private void NewControlElement_Click(object sender, EventArgs e)
        {
            if (NewControl != null)
            {
                MenuItem item = sender as MenuItem;
                NewControl(dialogElement, contextMenuClickPosition, item.Text);
            }
        }

        public XmlNode SelectedNode
        {
            set
            {
                if (value == null)
                {
                    selectedNodeId = null;
                }
                else
                {
                    string tmpSelectedNodeId = null;
                    SelectionOverlay ctrl = null;

                    XmlAttribute att = null;
                    if (value.Attributes != null)
                    {
                        att = value.Attributes["Id"];
                    }
                    if (att != null &&
                        att.Value != null &&
                        att.Value.Trim().Length > 0)
                    {
                        tmpSelectedNodeId = att.Value;
                        if (controlMap.Contains(tmpSelectedNodeId))
                        {
                            ctrl = (SelectionOverlay)controlMap[tmpSelectedNodeId];
                        }
                    }

                    if (ctrl != null)
                    {
                        ctrl.IsSelected = true;
                        selectedNodeId = tmpSelectedNodeId;
                    }
                    else
                    {
                        if (selectedNodeId != null && controlMap.Contains(selectedNodeId))
                        {
                            ctrl = (SelectionOverlay)controlMap[selectedNodeId];
                            ctrl.IsSelected = false;
                        }
                        selectedNodeId = null;
                    }
                }

                Invalidate();
            }
        }

        public void AddControl(XmlNode controlDefinition, Control control)
        {
            SelectionOverlay overlay = new SelectionOverlay(control, controlDefinition, wixFiles);
            Controls.Add(overlay);

            overlay.ItemChanged += new SelectionOverlayItemHandler(OnItemChanged);
            overlay.ItemDeleted += new SelectionOverlayItemHandler(OnItemDeleted);
            overlay.SelectionChanged += new SelectionOverlayItemHandler(OnSelectionChanged);

            String nodeId = controlDefinition.Attributes["Id"].Value;
            controlMap.Add(nodeId, overlay);
        }

        /// <summary>
        /// EventHandler for when a SelectionOverlay object changed
        /// </summary>
        private void OnItemChanged(XmlNode item)
        {
            if (ItemChanged != null)
            {
                ItemChanged(item);
            }
        }

        /// <summary>
        /// EventHandler for when a SelectionOverlay object is deleted
        /// </summary>
        private void OnItemDeleted(XmlNode item)
        {
            if (ItemDeleted != null)
            {
                ItemDeleted(item);
            }
        }

        /// <summary>
        /// EventHandler for when a SelectionOverlay object got selection
        /// </summary>
        public void OnSelectionChanged(XmlNode item)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(item);
            }
        }
    }
}