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
using System.IO;
using System.Windows.Forms;

using WixEdit.Settings;
using WixEdit.Xml;
using WixEdit.Helpers;

namespace WixEdit.Controls
{
    //  Identifies the editing control for the FileSelect column type.  It
    //  isn't too much different from a regular FileSelect control, 
    //  except that it implements the IDataGridViewEditingControl interface. 
    public class FileSelectEditingControl: UserControl, IDataGridViewEditingControl
    {
        WixFiles wixFiles;

        protected int rowIndex;
        protected DataGridView dataGridView;
        protected bool valueChanged = false;

        private TextBox filePathTextBox;
        private Button selectFileButton;

        int buttonWidth = 24;

        public FileSelectEditingControl()
        {
            filePathTextBox = new TextBox();
            filePathTextBox.BorderStyle = BorderStyle.None;
            filePathTextBox.Location = new Point(3, 4);
            filePathTextBox.Size = new Size(this.Width - (2*filePathTextBox.Left) - buttonWidth, filePathTextBox.Height);
            filePathTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            filePathTextBox.TextChanged += new EventHandler(filePathTextBox_TextChanged);
            Controls.Add(filePathTextBox);

            selectFileButton = new Button();
            selectFileButton.Location = new Point(filePathTextBox.Width + (2*filePathTextBox.Left), 0);
            selectFileButton.Size = new Size(buttonWidth, this.Height);
            selectFileButton.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            selectFileButton.Text = "...";
            selectFileButton.Click += new EventHandler(selectFileButton_Click);

            Controls.Add(selectFileButton);
        }

        public WixFiles WixFiles
        {
            get
            {
                return wixFiles;
            }
            set
            {
                wixFiles = value;
            }
        }

        public override string Text
        {
            get
            {
                return filePathTextBox.Text;
            }
            set
            {
                filePathTextBox.Text = value;
            }
        }

        void selectFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (!String.IsNullOrEmpty(filePathTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetFullPath(filePathTextBox.Text);
                dialog.FileName = Path.GetFullPath(filePathTextBox.Text);
            }

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = dialog.FileName;
                
                string sepCharString = Path.DirectorySeparatorChar.ToString();

                if (File.Exists(Path.GetFullPath(path)) == false)
                {
                    MessageBox.Show(String.Format("{0} could not be located", path), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    filePathTextBox.Text = path;
                }
                else
                {
                    if (WixEditSettings.Instance.UseRelativeOrAbsolutePaths == PathHandling.ForceAbolutePaths)
                    {
                        filePathTextBox.Text = Path.GetFullPath(path);
                    }
                    else
                    {
                        filePathTextBox.Text = PathHelper.GetRelativePath(path, wixFiles);
                    }
                }
            }
        }

        void filePathTextBox_TextChanged(object sender, EventArgs e)
        {
            // Let the DataGridView know about the value change
            NotifyDataGridViewOfValueChange();
        }

        //  Notify DataGridView that the value has changed.
        protected virtual void NotifyDataGridViewOfValueChange()
        {
            this.valueChanged = true;
            if (this.dataGridView != null)
            {
                this.dataGridView.NotifyCurrentCellDirty(true);
            }
        }

        #region IDataGridViewEditingControl Members

        //  Indicates the cursor that should be shown when the user hovers their
        //  mouse over this cell when the editing control is shown.
		public Cursor EditingPanelCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }


        //  Returns or sets the parent DataGridView.
        public DataGridView EditingControlDataGridView
        {
            get
            {
                return this.dataGridView;
            }

            set
            {
                this.dataGridView = value;
            }
        }


        //  Sets/Gets the formatted value contents of this cell.
		public object EditingControlFormattedValue
        {
            set
            {
                filePathTextBox.Text = value.ToString();
                NotifyDataGridViewOfValueChange();
            }
			get 
			{
                return filePathTextBox.Text;
			}
        }

		//   Get the value of the editing control for formatting.
		public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
		{
			return filePathTextBox.Text;
		}

        //  Process input key and determine if the key should be used for the editing control
        //  or allowed to be processed by the grid. Handle cursor movement keys for the FileSelect
        //  control; otherwise if the DataGridView doesn't want the input key then let the editing control handle it.
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Right:
                    //
                    // If the end of the selection is at the end of the string
                    // let the DataGridView treat the key message
                    //
                    if (!(filePathTextBox.SelectionLength == 0
                          && filePathTextBox.SelectionStart == filePathTextBox.Text.Length))
                    {
                        return true;
                    }
                    break;

                case Keys.Left:
                    //
                    // If the end of the selection is at the begining of the
                    // string or if the entire text is selected send this character 
                    // to the dataGridView; else process the key event.
                    //
                    if (!(filePathTextBox.SelectionLength == 0
                          && filePathTextBox.SelectionStart == 0))
                    {
                        return true;
                    }
                    break;

                case Keys.Home:
                case Keys.End:
                    if (filePathTextBox.SelectionLength != filePathTextBox.Text.Length)
                    {
                        return true;
                    }
                    break;

                case Keys.Prior:
                case Keys.Next:
                    if (this.valueChanged)
                    {
                        return true;
                    }
                    break;

                case Keys.Delete:
                    if (filePathTextBox.SelectionLength > 0 || filePathTextBox.SelectionStart < filePathTextBox.Text.Length)
                    {
                        return true;
                    }
                    break;
            }

            //
            // defer to the DataGridView and see if it wants it.
            //
            return !dataGridViewWantsInputKey;
        }


        //  Prepare the editing control for edit.
        public void PrepareEditingControlForEdit(bool selectAll)
        {
            if (selectAll)
            {
                filePathTextBox.SelectAll();
            }
            else
            {
                //
                // Do not select all the text, but position the caret at the 
                // end of the text.
                //
                filePathTextBox.SelectionStart = filePathTextBox.Text.Length;
            }
        }

        //  Indicates whether or not the parent DataGridView control should
        //  reposition the editing control every time value change is indicated.
        //  There is no need to do this for the FileSelect.
        public bool RepositionEditingControlOnValueChange
        {
            get
            {
                return false;
            }
        }


        //  Indicates the row index of this cell.  This is often -1 for the
        //  template cell, but for other cells, might actually have a value
        //  greater than or equal to zero.
        public int EditingControlRowIndex
        {
            get
            {
                return this.rowIndex;
            }

            set
            {
                this.rowIndex = value;
            }
        }



        //  Make the FileSelect control match the style and colors of
        //  the host DataGridView control and other editing controls 
        //  before showing the editing control.
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            filePathTextBox.Font = dataGridViewCellStyle.Font;
            filePathTextBox.ForeColor = dataGridViewCellStyle.ForeColor;
            filePathTextBox.BackColor = dataGridViewCellStyle.BackColor;
            filePathTextBox.TextAlign = translateAlignment(dataGridViewCellStyle.Alignment);
        }


        //  Gets or sets our flag indicating whether the value has changed.
        public bool EditingControlValueChanged
        {
            get
            {
                return valueChanged;
            }

            set
            {
                this.valueChanged = value;
            }
        }
    
		#endregion // IDataGridViewEditingControl.
		
        ///   Routine to translate between DataGridView
        ///   content alignments and text box horizontal alignments.
        private static HorizontalAlignment translateAlignment(DataGridViewContentAlignment align)
        {
            switch (align)
            {
                case DataGridViewContentAlignment.TopLeft:
                case DataGridViewContentAlignment.MiddleLeft:
                case DataGridViewContentAlignment.BottomLeft:
                    return HorizontalAlignment.Left;

                case DataGridViewContentAlignment.TopCenter:
                case DataGridViewContentAlignment.MiddleCenter:
                case DataGridViewContentAlignment.BottomCenter:
                    return HorizontalAlignment.Center;

                case DataGridViewContentAlignment.TopRight:
                case DataGridViewContentAlignment.MiddleRight:
                case DataGridViewContentAlignment.BottomRight:
                    return HorizontalAlignment.Right;
            }

            throw new ArgumentException("Error: Invalid Content Alignment!");
        }


    }
}
