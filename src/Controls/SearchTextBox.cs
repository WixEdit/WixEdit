using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WixEdit.src.Controls
{
    /// <summary>
    /// Search Text Box Control
    /// https://stackoverflow.com/a/32100664/90287
    /// </summary>
    /// <seealso cref="System.Windows.Forms.TextBox" />
    public class SearchTextBox : TextBox
    {
        private const int EM_SETMARGINS = 0xd3;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private PictureBox searchPictureBox;

        private Button cancelSearchButton;

        public SearchTextBox()
        {
            cancelSearchButton = new Button();
            cancelSearchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancelSearchButton.Size = new Size(16, 16);
            cancelSearchButton.TabIndex = 0;
            cancelSearchButton.TabStop = false;
            cancelSearchButton.FlatStyle = FlatStyle.Flat;
            cancelSearchButton.FlatAppearance.BorderSize = 0;
            cancelSearchButton.Text = "";
            cancelSearchButton.Cursor = Cursors.Arrow;

            Controls.Add(cancelSearchButton);

            cancelSearchButton.Click += delegate
            {
                Text = "";
                Focus();
            };

            searchPictureBox = new PictureBox();
            searchPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchPictureBox.Size = new Size(16, 16);
            searchPictureBox.TabIndex = 0;
            searchPictureBox.TabStop = false;
            Controls.Add(searchPictureBox);

            // Send EM_SETMARGINS to prevent text from disappearing underneath the button
            SendMessage(Handle, EM_SETMARGINS, (IntPtr)2, (IntPtr)(16 << 16));

            UpdateControlsVisibility();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            UpdateControlsVisibility();
        }

        private void UpdateControlsVisibility()
        {
            if (string.IsNullOrEmpty(Text))
            {
                cancelSearchButton.Visible = false;
                searchPictureBox.Visible = true;
            }
            else
            {
                cancelSearchButton.Visible = true;
                searchPictureBox.Visible = false;
            }
        }

        [Browsable(true)]
        public Image SearchImage
        {
            set
            {
                searchPictureBox.Image = value;
                searchPictureBox.Left = Width - searchPictureBox.Size.Width - 4;
                searchPictureBox.Top = Height - searchPictureBox.Size.Height - 4;
            }

            get { return searchPictureBox.Image; }
        }

        [Browsable(true)]
        public Image CancelSearchImage
        {
            set
            {
                cancelSearchButton.Image = value;
                cancelSearchButton.Left = Width - searchPictureBox.Size.Width - 4;
                cancelSearchButton.Top = Height - searchPictureBox.Size.Height - 4;
            }

            get { return cancelSearchButton.Image; }
        }
    }
}
