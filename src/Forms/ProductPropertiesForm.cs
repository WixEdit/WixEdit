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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.PropertyGridExtensions;
using WixEdit.Xml;

namespace WixEdit.Forms
{
    /// <summary>
    /// ProductPropertiesForm edits the Attributes of the Product element.
    /// </summary>
    public class ProductPropertiesForm : Form
    {
        protected Button buttonOk;
        //        protected Button buttonCancel;

        protected PropertyGrid productPropertyGrid;

        protected XmlNode productNode;
        protected WixFiles wixFiles;

        public ProductPropertiesForm(XmlNode productNode, WixFiles wixFiles)
        {
            this.productNode = productNode;
            this.wixFiles = wixFiles;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Product Properties";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;

            buttonOk = new Button();
            buttonOk.Text = "Done";
            buttonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            buttonOk.FlatStyle = FlatStyle.System;
            buttonOk.Click += new EventHandler(OnOk);
            Controls.Add(buttonOk);

            //            buttonCancel = new Button();
            //            buttonCancel.Text = "Cancel";
            //            buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            //            buttonCancel.FlatStyle = FlatStyle.System;
            //            Controls.Add(buttonCancel);

            productPropertyGrid = new CustomPropertyGrid();
            productPropertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            productPropertyGrid.Name = "propertyGrid";
            productPropertyGrid.TabIndex = 1;
            productPropertyGrid.PropertySort = PropertySort.Alphabetical;
            productPropertyGrid.ToolbarVisible = false;
            productPropertyGrid.Dock = DockStyle.Top;
            productPropertyGrid.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            Controls.Add(productPropertyGrid);

            ClientSize = new Size(384, 256);

            productPropertyGrid.Size = new Size(ClientSize.Width, ClientSize.Height - 29);


            //            buttonCancel.Left = ClientSize.Width - buttonCancel.Width;
            //            buttonOk.Left = buttonCancel.Left - buttonOk.Width - 2;
            buttonOk.Left = ClientSize.Width - buttonOk.Width - 2;

            //            buttonCancel.Top = productPropertyGrid.Top + productPropertyGrid.Height + 3;
            buttonOk.Top = productPropertyGrid.Top + productPropertyGrid.Height + 3;

            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            AcceptButton = buttonOk;
            //            CancelButton = buttonCancel;

            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = true;

            StartPosition = FormStartPosition.CenterParent;

            Activated += new EventHandler(IsActivated);

            productPropertyGrid.SelectedObject = new XmlAttributeAdapter(productNode, wixFiles);
        }

        private void IsActivated(object sender, EventArgs e)
        {
            // StringEdit.Focus();
        }

        private void OnOk(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}