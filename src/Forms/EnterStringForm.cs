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

namespace WixEdit.Forms
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class EnterStringForm : Form
    {
        protected Button ButtonOk;
        protected Button ButtonCancel;
        protected TextBox StringEdit;
        protected bool multiLine;

        protected string selectedString;

        public EnterStringForm()
        {
            multiLine = false;

            InitializeComponent();
        }

        public EnterStringForm(string stringEditValue)
        {
            multiLine = false;

            InitializeComponent();

            if (stringEditValue != null)
            {
                StringEdit.Text = stringEditValue;
            }
        }

        private void InitializeComponent()
        {
            Text = "Enter String";
            ShowInTaskbar = false;

            ButtonOk = new Button();
            ButtonOk.Text = "Ok";
            ButtonOk.Location = new Point(0, 23);
            ButtonOk.FlatStyle = FlatStyle.System;
            ButtonOk.Click += new EventHandler(OnOk);
            Controls.Add(ButtonOk);

            ButtonCancel = new Button();
            ButtonCancel.Text = "Cancel";
            ButtonCancel.Location = new Point(ButtonOk.Width + 2, 23);

            ButtonCancel.FlatStyle = FlatStyle.System;
            Controls.Add(ButtonCancel);

            StringEdit = new TextBox();
            StringEdit.Location = new Point(0, 0);
            Controls.Add(StringEdit);
            StringEdit.Text = selectedString;
            StringEdit.Size = new Size(ButtonCancel.Width + 2 + ButtonOk.Width, 23);

            ClientSize = new Size(ButtonCancel.Width + 2 + ButtonOk.Width, 46);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;

            AcceptButton = ButtonOk;
            CancelButton = ButtonCancel;

            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;

            ButtonOk.Anchor = AnchorStyles.Bottom;
            ButtonCancel.Anchor = AnchorStyles.Bottom;
            StringEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            StartPosition = FormStartPosition.CenterParent;

            Activated += new EventHandler(IsActivated);
        }

        private void IsActivated(object sender, EventArgs e)
        {
            StringEdit.Focus();
        }

        public string SelectedString
        {
            get
            {
                return selectedString;
            }
            set
            {
                selectedString = value;
                StringEdit.Text = selectedString;
            }
        }

        public bool MultiLine
        {
            get
            {
                return multiLine;
            }
            set
            {
                if (multiLine != value)
                {
                    if (value)
                    {
                        // Make a multi line text box with scroll bar
                        StringEdit.Multiline = true;
                        StringEdit.ScrollBars = ScrollBars.Vertical;

                        // Make the dialog larger and resizable
                        ClientSize = new Size(400, 323);
                        FormBorderStyle = FormBorderStyle.SizableToolWindow;

                        // Make the enter key not confirm the dialog.
                        AcceptButton = null;
                    }
                    else
                    {
                        // Make a single line text box without scroll bar
                        StringEdit.Multiline = false;
                        StringEdit.ScrollBars = ScrollBars.None;

                        // Make the dialog small and not resizable
                        ClientSize = new Size(ButtonCancel.Width + 2 + ButtonOk.Width, 46);
                        FormBorderStyle = FormBorderStyle.FixedToolWindow;

                        // Make the enter key confirm the dialog again.
                        AcceptButton = ButtonOk;
                    }
                }

                multiLine = value;
            }
        }

        protected virtual void OnOk(object sender, EventArgs e)
        {
            selectedString = StringEdit.Text;
            DialogResult = DialogResult.OK;
        }
    }
}