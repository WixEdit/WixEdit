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

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// A customized PropertyGrid control.
    /// </summary>
    public class CustomPropertyGrid : PropertyGrid {
        public CustomPropertyGrid() {
            this.KeyDown += new KeyEventHandler(CustomPropertyGrid_KeyDown);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown (e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress (e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp (e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            return base.ProcessCmdKey (ref msg, keyData);
        }

        protected override bool ProcessDialogChar(char charCode) {
            return base.ProcessDialogChar (charCode);
        }

        protected override bool ProcessDialogKey(Keys keyData) {
            if (SelectedGridItem != null &&
                ActiveControl != null &&
                ActiveControl.GetType().ToString() == "System.Windows.Forms.PropertyGridInternal.PropertyGridView" &&
                keyData == Keys.Delete) {
                PropertyAdapterBase adapter = this.SelectedObject as PropertyAdapterBase;

                if (adapter.WixFiles != null) {
                    adapter.WixFiles.UndoManager.BeginNewCommandRange();
                }

                CustomXmlPropertyDescriptorBase descriptor = this.SelectedGridItem.PropertyDescriptor as CustomXmlPropertyDescriptorBase;
                if (descriptor != null && descriptor.XmlElement != null) {
                    this.SelectedObject = null;
                    adapter.RemoveProperty(descriptor.XmlElement);
                    this.SelectedObject = adapter;

                    return true;
                }
            }

            return base.ProcessDialogKey (keyData);
        }

        protected override bool ProcessKeyEventArgs(ref Message m) {
            return base.ProcessKeyEventArgs (ref m);
        }

        protected override bool ProcessKeyPreview(ref Message m) {
            return base.ProcessKeyPreview (ref m);
        }

        protected void CustomPropertyGrid_KeyDown(object sender, KeyEventArgs e) {

        }

        protected override bool ProcessTabKey(bool forward) {
            bool foundItem = false;
            bool done = false;

            if (SelectedGridItem != null && SelectedGridItem.Parent != null) {
                foreach (GridItem item in SelectedGridItem.Parent.GridItems) {
                    if (foundItem == true) {
                        SelectedGridItem = item;
                        done = true;
                        break;
                    }
                    if (item == SelectedGridItem) {
                        foundItem = true;
                    }
                } 
    
                if (foundItem == true && done == false) {
                    SelectedGridItem = SelectedGridItem.Parent.GridItems[0];
                    done = true;
                }
            }

            return done;
        }
    }
}