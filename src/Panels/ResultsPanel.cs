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
using System.Windows.Forms;

using WixEdit.Controls;
using WixEdit.Xml;

namespace WixEdit.Panels
{
    public class ResultsPanel : Panel
    {
        Button closeButton;
        Label outputLabel;
        TabControl tabControl;

        Panel[] thePanels;

        public ResultsPanel(Panel[] panels)
        {
            thePanels = panels;
            InitializeComponent(panels);
        }

        public event EventHandler CloseClicked;

        private void InitializeComponent(Panel[] panels)
        {
            TabStop = true;

            int buttonWidth = 11;
            int buttonHeigth = 11;
            int paddingX = 2;
            int paddingY = 2;

            closeButton = new Button();
            outputLabel = new Label();
            tabControl = new CustomTabControl();

            closeButton.Size = new Size(buttonWidth, buttonHeigth);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Location = new Point(ClientSize.Width - buttonWidth - 2 * paddingX, paddingY);
            closeButton.BackColor = Color.Transparent;
            closeButton.Click += new EventHandler(OnCloseClick);

            outputLabel.Text = "Results Panel";
            outputLabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((System.Byte)(0))); ;
            outputLabel.BorderStyle = BorderStyle.FixedSingle;

            Bitmap bmp = new Bitmap(WixFiles.GetResourceStream("close_8x8.bmp"));
            bmp.MakeTransparent();
            closeButton.Image = bmp;
            closeButton.FlatStyle = FlatStyle.Flat;

            tabControl.Alignment = TabAlignment.Bottom;

            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, buttonHeigth + 3 * paddingY);
            tabControl.Size = new Size(200, ClientSize.Height - tabControl.Location.Y);
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            TabPage page = null;
            foreach (Panel panel in panels)
            {
                page = new TabPage(panel.Text);
                page.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;

                tabControl.TabPages.Add(page);
            }

            outputLabel.Size = new Size(ClientSize.Width - 2 * paddingX, buttonHeigth + (2 * paddingY));
            outputLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputLabel.Location = new Point(paddingX, 0);

            outputLabel.BackColor = Color.Gray;
            outputLabel.ForeColor = Color.LightGray;

            Controls.Add(closeButton);
            Controls.Add(outputLabel);
            Controls.Add(tabControl);

            closeButton.TabStop = true;
            outputLabel.TabStop = true;

            closeButton.LostFocus += new EventHandler(HasFocus);
            outputLabel.LostFocus += new EventHandler(HasFocus);

            closeButton.GotFocus += new EventHandler(HasFocus);
            outputLabel.GotFocus += new EventHandler(HasFocus);

            closeButton.Enter += new EventHandler(HasFocus);
            outputLabel.Enter += new EventHandler(HasFocus);

            closeButton.Click += new EventHandler(HasFocus);
            outputLabel.Click += new EventHandler(HasFocus);
        }

        protected void OnCloseClick(Object sender, EventArgs e)
        {
            if (CloseClicked != null)
            {
                CloseClicked(sender, e);
            }
        }

        public void ShowPanel(Panel panel)
        {
            for (int i = 0; i < thePanels.Length; i++)
            {
                if (thePanels[i] == panel)
                {
                    tabControl.SelectedIndex = i;
                    break;
                }
            }
        }

        protected void HasFocus(Object sender, EventArgs e)
        {
            if (closeButton.Focused ||
                outputLabel.Focused)
            {
                outputLabel.BackColor = Color.DimGray;
                outputLabel.ForeColor = Color.White;
            }
            else
            {
                outputLabel.BackColor = Color.Gray;
                outputLabel.ForeColor = Color.LightGray;
            }
        }
    }
}
