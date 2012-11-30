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
using System.Windows.Forms;

namespace WixEdit.Forms
{
    /// <summary>
    /// Form to enter strings.
    /// </summary>
    public class SelectStringForm : Form
    {
        protected Button ButtonOk;
        protected Button ButtonCancel;
        protected ListBox StringList;

        protected string[] selectedStrings;
        protected string[] possibleStrings;

        public SelectStringForm()
        {
            InitializeComponent("New Attribute Name");
        }

        public SelectStringForm(string title)
        {
            InitializeComponent(title);
        }

        private void InitializeComponent(string title)
        {
            Text = title;
            ShowInTaskbar = false;

            ButtonOk = new Button();
            ButtonCancel = new Button();
            StringList = new ListBox();
            
            ClientSize = new Size(ButtonCancel.Width + 2 + ButtonOk.Width, 262);
            MinimumSize = new Size(ButtonCancel.Width + 2 + ButtonOk.Width, 262);

            ButtonOk.Text = "Ok";
            ButtonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            ButtonOk.Location = new Point(0, 262 - ButtonOk.Height);
            ButtonOk.FlatStyle = FlatStyle.System;
            ButtonOk.Click += new EventHandler(OnOk);
            ButtonOk.Enabled = false;
            Controls.Add(ButtonOk);

            ButtonCancel.Text = "Cancel";
            ButtonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            ButtonCancel.Location = new Point(2 + ButtonOk.Width, 262 - ButtonOk.Height);
            ButtonCancel.FlatStyle = FlatStyle.System;
            Controls.Add(ButtonCancel);

            StringList.Dock = DockStyle.Fill;
            StringList.SelectionMode = SelectionMode.MultiSimple;
            StringList.DoubleClick += new EventHandler(OnDoubleClickList);
            StringList.SelectedValueChanged += new EventHandler(OnSelectionChanged);
            Controls.Add(StringList);

            StringList.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            StringList.Size = new Size(ButtonCancel.Width + 2 + ButtonOk.Width, 238);

            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            AcceptButton = ButtonOk;
            CancelButton = ButtonCancel;

            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;

            StartPosition = FormStartPosition.CenterParent;

            Activated += new EventHandler(OnActivate);
        }

        private void OnActivate(object sender, EventArgs e)
        {
            StringList.Items.Clear();
            foreach (string it in possibleStrings)
            {
                StringList.Items.Add(it);
            }

            UpdateOkButton();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            UpdateOkButton();
        }

        private void UpdateOkButton()
        {
            if (StringList.SelectedItem == null)
            {
                ButtonOk.Enabled = false;
            }
            else
            {
                ButtonOk.Enabled = true;
            }
        }

        public string[] SelectedStrings
        {
            get
            {
                return selectedStrings;
            }
            set
            {
                selectedStrings = value;
            }
        }

        public string[] PossibleStrings
        {
            get
            {
                return possibleStrings;
            }
            set
            {
                possibleStrings = value;
            }
        }

        private void OnOk(object sender, EventArgs e)
        {
            selectedStrings = FillSelectedString();
            DialogResult = DialogResult.OK;
        }

        private void OnDoubleClickList(object sender, EventArgs e)
        {
            // Cannot determine if an item is double clicked or not.
            // but just pretend if we do... ;) and only with one item.
            if (StringList.SelectedItem != null && StringList.SelectedItems.Count == 1)
            {
                selectedStrings = FillSelectedString();
                DialogResult = DialogResult.OK;
            }
        }

        private string[] FillSelectedString()
        {
            string[] strArray = new string[StringList.SelectedItems.Count];
            int i = 0;
            foreach (string s in StringList.SelectedItems)
            {
                strArray.SetValue(s, i);
                i++;
            }
            return strArray;
        }
    }
}