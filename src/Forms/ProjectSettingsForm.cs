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
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.Forms
{
    public class ProjectSettingsForm : Form
    {
        protected WixFiles wixFiles;
        protected CheckBox candleArgsCheck;
        protected TextBox candleArgs;
        protected CheckBox lightArgsCheck;
        protected TextBox lightArgs;

        protected Button cancelButton;
        protected Button okButton;

        public ProjectSettingsForm(WixFiles wixFiles)
        {
            this.wixFiles = wixFiles;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            int padding = 4;
            int indent = 16;
            ClientSize = new Size(400, 200);

            Text = "Edit Settings";
            Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;

            bool hasCandleArgs = (wixFiles.ProjectSettings.CandleArgs != null &&
                                  wixFiles.ProjectSettings.CandleArgs.Length > 0);
            bool hasLightArgs = (wixFiles.ProjectSettings.LightArgs != null &&
                                 wixFiles.ProjectSettings.LightArgs.Length > 0);


            candleArgsCheck = new CheckBox();
            candleArgs = new TextBox();
            lightArgsCheck = new CheckBox();
            lightArgs = new TextBox();
            cancelButton = new Button();
            okButton = new Button();


            candleArgsCheck.Text = "Use custom commandline for Candle.exe";
            candleArgsCheck.CheckedChanged += new EventHandler(candleArgsCheck_CheckedChanged);
            candleArgsCheck.Checked = hasCandleArgs;
            candleArgsCheck.Size = new Size(ClientSize.Width - padding * 2, 27);
            candleArgsCheck.Location = new Point(padding, 0);
            Controls.Add(candleArgsCheck);

            if (hasCandleArgs)
            {
                candleArgs.Text = wixFiles.ProjectSettings.CandleArgs;
            }
            else
            {
                candleArgs.Text = wixFiles.ProjectSettings.DefaultCandleArgs;
                candleArgs.Enabled = false;
            }
            candleArgs.Size = new Size(ClientSize.Width - indent - padding * 2, 27);
            candleArgs.Location = new Point(padding + indent, candleArgsCheck.Bottom);
            Controls.Add(candleArgs);

            lightArgsCheck.Checked = hasLightArgs;
            lightArgsCheck.Text = "Use custom commandline for Light.exe";
            lightArgsCheck.CheckedChanged += new EventHandler(lightArgsCheck_CheckedChanged);
            lightArgsCheck.Size = new Size(ClientSize.Width - padding * 2, 27);
            lightArgsCheck.Location = new Point(padding, candleArgs.Bottom);
            Controls.Add(lightArgsCheck);

            if (hasLightArgs)
            {
                lightArgs.Text = wixFiles.ProjectSettings.LightArgs;
            }
            else
            {
                lightArgs.Text = wixFiles.ProjectSettings.DefaultLightArgs;
                lightArgs.Enabled = false;
            }
            lightArgs.Size = new Size(ClientSize.Width - indent - padding * 2, 27);
            lightArgs.Location = new Point(padding + indent, lightArgsCheck.Bottom);
            Controls.Add(lightArgs);

            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(ClientSize.Width - cancelButton.Width - padding, lightArgs.Bottom + padding);
            cancelButton.FlatStyle = FlatStyle.System;
            Controls.Add(cancelButton);
            CancelButton = cancelButton;

            okButton.Text = "OK";
            okButton.Location = new Point(ClientSize.Width - cancelButton.Width - padding - okButton.Width - padding, lightArgs.Bottom + padding);
            okButton.FlatStyle = FlatStyle.System;
            okButton.Click += new EventHandler(okButton_Click);
            Controls.Add(okButton);
            AcceptButton = okButton;

            ClientSize = new Size(ClientSize.Width, okButton.Bottom + padding);
        }

        private void candleArgsCheck_CheckedChanged(object sender, EventArgs e)
        {
            candleArgs.Enabled = candleArgsCheck.Checked;
        }

        private void lightArgsCheck_CheckedChanged(object sender, EventArgs e)
        {
            lightArgs.Enabled = lightArgsCheck.Checked;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (candleArgsCheck.Checked)
            {
                if (wixFiles.ProjectSettings.CandleArgs != candleArgs.Text)
                {
                    wixFiles.ProjectSettings.CandleArgs = candleArgs.Text;
                }
            }
            else
            {
                if (wixFiles.ProjectSettings.CandleArgs != null &&
                    wixFiles.ProjectSettings.CandleArgs.Length != 0)
                {
                    wixFiles.ProjectSettings.CandleArgs = String.Empty;
                }
            }

            if (lightArgsCheck.Checked)
            {
                if (wixFiles.ProjectSettings.LightArgs != lightArgs.Text)
                {
                    wixFiles.ProjectSettings.LightArgs = lightArgs.Text;
                }
            }
            else
            {
                if (wixFiles.ProjectSettings.LightArgs != null &&
                    wixFiles.ProjectSettings.LightArgs.Length != 0)
                {
                    wixFiles.ProjectSettings.LightArgs = String.Empty;
                }
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}