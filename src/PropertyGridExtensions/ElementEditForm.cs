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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using WixEdit.PropertyGridExtensions;
using WixEdit.Xml;
using WixEdit.Controls;
using WixEdit.Forms;

namespace WixEdit {
    /// <summary>
    /// ElementEditForm edits the Attributes of an element.
    /// </summary>
    public class ElementEditForm : Form {
        protected Button buttonOk;
        protected ContextMenu elementPropertyGridContextMenu;
        protected PropertyGrid elementPropertyGrid;

        protected XmlNode elementNode;
        protected WixFiles wixFiles;

        public ElementEditForm(XmlNode elementNode, WixFiles wixFiles) {
            this.elementNode = elementNode;
            this.wixFiles = wixFiles;

            InitializeComponent();
        }

        private void InitializeComponent() {
            Text = elementNode.Name + " Properties";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;

            buttonOk = new Button();
            buttonOk.Text = "Done";
            buttonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            buttonOk.FlatStyle = FlatStyle.System;
            buttonOk.Click += new EventHandler(OnOk);
            Controls.Add(buttonOk);

            elementPropertyGridContextMenu = new ContextMenu();

            elementPropertyGrid = new CustomPropertyGrid();
            elementPropertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
            elementPropertyGrid.Name = "propertyGrid";
            elementPropertyGrid.TabIndex = 1;
            elementPropertyGrid.PropertySort = PropertySort.Alphabetical;
            elementPropertyGrid.ToolbarVisible = false;
            elementPropertyGrid.Dock = DockStyle.Top;
            elementPropertyGrid.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            elementPropertyGrid.ContextMenu = elementPropertyGridContextMenu;
            elementPropertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);
            Controls.Add(elementPropertyGrid);

            ClientSize = new Size(384, 256);

            elementPropertyGrid.Size = new Size(ClientSize.Width, ClientSize.Height-29);

            buttonOk.Left = ClientSize.Width - buttonOk.Width - 2;
            buttonOk.Top = elementPropertyGrid.Top + elementPropertyGrid.Height + 3;

            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            AcceptButton = buttonOk;

            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = true; 

            StartPosition = FormStartPosition.CenterParent;

            Activated += new EventHandler(IsActivated);

            elementPropertyGrid.SelectedObject = new XmlAttributeAdapter(elementNode, wixFiles, true);
        }

        private void IsActivated(object sender, EventArgs e) {
            // StringEdit.Focus();
        }

        private void OnOk(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }


        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            elementPropertyGridContextMenu.MenuItems.Clear();

            if (elementPropertyGrid.SelectedObject == null) {
                return;
            }

            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) elementPropertyGrid.SelectedObject;
            if (attAdapter.XmlNodeDefinition == null) {
                // Don't know, but can not show the context menu.
                return;
            }
            
            // Need to change "Delete" to "Clear" for required items.
            bool isRequired = false;

            // Get the XmlAttribute from the PropertyDescriptor
            XmlAttributePropertyDescriptor desc = elementPropertyGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
            if (desc != null) {
                XmlAttribute att = desc.Attribute;
                XmlNode xmlAttributeDefinition = attAdapter.XmlNodeDefinition.SelectSingleNode(String.Format("xs:attribute[@name='{0}']", att.Name), wixFiles.XsdNsmgr);

                if (xmlAttributeDefinition.Attributes["use"] != null &&
                    xmlAttributeDefinition.Attributes["use"].Value == "required") {
                    isRequired = true;
                }
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");

            // See if new menu item should be shown.
            bool canCreateNew = false;

            XmlNodeList xmlAttributes = attAdapter.XmlNodeDefinition.SelectNodes("xs:attribute", wixFiles.XsdNsmgr);
            foreach (XmlNode at in xmlAttributes) {
                string attName = at.Attributes["name"].Value;
                if (attAdapter.XmlNode.Attributes[attName] == null) {
                    canCreateNew = true;
                }
            }

            if (canCreateNew) {
                // Define the MenuItem objects to display for the TextBox.
                MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
                menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
                elementPropertyGridContextMenu.MenuItems.Add(menuItem1);
            }

            // Add the clear or delete menu item
            MenuItem menuItem2 = null;
            if (elementPropertyGrid.SelectedGridItem.PropertyDescriptor != null &&
                !(elementPropertyGrid.SelectedGridItem.PropertyDescriptor is InnerTextPropertyDescriptor)) {
                if (isRequired) {
                    menuItem2 = new IconMenuItem("&Clear", new Bitmap(WixFiles.GetResourceStream("bmp.clear.bmp")));
                } else {
                    menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
                }
                menuItem2.Click += new EventHandler(OnDeletePropertyGridItem);
                elementPropertyGridContextMenu.MenuItems.Add(menuItem2);
            }

            if (elementPropertyGridContextMenu.MenuItems.Count > 0) {
                elementPropertyGridContextMenu.MenuItems.Add(menuItemSeparator);
            }

            MenuItem menuItem3 = new IconMenuItem("Description");
            menuItem3.Click += new EventHandler(OnToggleDescriptionPropertyGrid);
            menuItem3.Checked = elementPropertyGrid.HelpVisible;

            elementPropertyGridContextMenu.MenuItems.Add(menuItem3);
        }

        protected void OnToggleDescriptionPropertyGrid(object sender, EventArgs e) {
            elementPropertyGrid.HelpVisible = !elementPropertyGrid.HelpVisible;
        }


        public void OnNewPropertyGridItem(object sender, EventArgs e) {
            wixFiles.UndoManager.BeginNewCommandRange();

            // Temporarily store the XmlAttributeAdapter
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) elementPropertyGrid.SelectedObject;

            ArrayList attributes = new ArrayList();

            XmlNodeList xmlAttributes = attAdapter.XmlNodeDefinition.SelectNodes("xs:attribute", wixFiles.XsdNsmgr);
            foreach (XmlNode at in xmlAttributes) {
                string attName = at.Attributes["name"].Value;
                if (attAdapter.XmlNode.Attributes[attName] == null) {
                    attributes.Add(attName);
                }
            }

            if (attAdapter.XmlNodeDefinition.Name == "xs:extension") {
                bool hasInnerText = false;
                foreach (GridItem it in elementPropertyGrid.SelectedGridItem.Parent.GridItems) {
                    if (it.Label == "InnerText") {
                        hasInnerText = true;
                        break;
                    }
                }
                if (hasInnerText == false) {
                    attributes.Add("InnerText");
                }
            }

            attributes.Sort();

            SelectStringForm frm = new SelectStringForm();
            frm.PossibleStrings = attributes.ToArray(typeof(String)) as String[];
            if (DialogResult.OK != frm.ShowDialog()) {
                return;
            }

            // Show dialog to choose from available items.
            XmlAttribute att = null;
            for (int i = 0; i < frm.SelectedStrings.Length; i++) {
                string newAttributeName = frm.SelectedStrings[i];
                if (string.Equals(newAttributeName,"InnerText")) {
                    attAdapter.ShowInnerTextIfEmpty = true;
                } else {
                    att = wixFiles.WxsDocument.CreateAttribute(newAttributeName);
                    attAdapter.XmlNode.Attributes.Append(att);
                }
            }
    
            // resetting the elementPropertyGrid.
            elementPropertyGrid.SelectedObject = null;
            // Update the elementPropertyGrid.
            elementPropertyGrid.SelectedObject = attAdapter;
            elementPropertyGrid.Update();

            string firstNewAttributeName = frm.SelectedStrings[0];
            foreach (GridItem it in elementPropertyGrid.SelectedGridItem.Parent.GridItems) {
                if (it.Label == firstNewAttributeName) {
                    elementPropertyGrid.SelectedGridItem = it;
                    break;
                }
            }
        }

        protected void OnDeletePropertyGridItem(object sender, EventArgs e) {
            XmlNode element = GetSelectedProperty();
            if (element == null) {
                throw new WixEditException("No element found to delete!");
            }

            // Temporarily store the XmlAttributeAdapter, while resetting the elementPropertyGrid.
            PropertyAdapterBase attAdapter = (PropertyAdapterBase) elementPropertyGrid.SelectedObject;
            elementPropertyGrid.SelectedObject = null;

            wixFiles.UndoManager.BeginNewCommandRange();
            attAdapter.RemoveProperty(element);

            // Update the elementPropertyGrid.
            elementPropertyGrid.SelectedObject = attAdapter;
            elementPropertyGrid.Update();
        }


        protected XmlNode GetSelectedProperty() {
            // Get the XmlAttribute from the PropertyDescriptor
            XmlNode element = null;
            if (elementPropertyGrid.SelectedGridItem.PropertyDescriptor is XmlAttributePropertyDescriptor) {
                XmlAttributePropertyDescriptor desc = elementPropertyGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
                element = desc.Attribute;
            } else if (elementPropertyGrid.SelectedGridItem.PropertyDescriptor is CustomXmlPropertyDescriptorBase) {
                CustomXmlPropertyDescriptorBase desc = elementPropertyGrid.SelectedGridItem.PropertyDescriptor as CustomXmlPropertyDescriptorBase;
                element = desc.XmlElement;
            } else {
                string typeString = "null";
                if (elementPropertyGrid.SelectedGridItem.PropertyDescriptor != null) {
                    typeString = elementPropertyGrid.SelectedGridItem.PropertyDescriptor.GetType().ToString();
                }

                throw new Exception(String.Format("Expected XmlAttributePropertyDescriptor, but got {0} in GetSelectedProperty", typeString));
            }

            return element;
        }
    }
}