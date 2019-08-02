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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WixEdit.Forms
{
    /// <summary>
    /// Form to enter strings.
    /// </summary>
    public partial class SelectStringForm : Form
    {
        public SelectStringForm()
        {
            InitializeComponent();

            this.AcceptButton = ButtonOk;
            this.CancelButton = ButtonCancel;
        }

        public SelectStringForm(string title)
            : this()
        {
            this.Text = title;
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

        public string[] SelectedStrings { get; set; }
        public string[] PossibleStrings { get; set; }

        private void OnOk(object sender, EventArgs e)
        {
            this.SelectedStrings = FillSelectedString();
            DialogResult = DialogResult.OK;
        }

        private void OnDoubleClickList(object sender, EventArgs e)
        {
            // Cannot determine if an item is double clicked or not.
            // but just pretend if we do... ;) and only with one item.
            if (StringList.SelectedItem != null && StringList.SelectedItems.Count == 1)
            {
                this.SelectedStrings = FillSelectedString();
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

        private void OnLoad(object sender, EventArgs e)
        {
            StringList.Items.Clear();
            foreach (string it in this.PossibleStrings)
            {
                StringList.Items.Add(it);
            }

            UpdateOkButton();
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            IEnumerable<string> displayList = this.PossibleStrings;

            if (this.SearchTextBox.Text != string.Empty)
            {
                displayList = this.PossibleStrings.Where(x => x.IndexOf(this.SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            this.StringList.Items.Clear();
            this.StringList.Items.AddRange(displayList.ToArray());
        }
    }
}
