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
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.Xml;


namespace WixEdit.Forms
{
    public class ExceptionForm : Form
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.Label errorMessageLabel;
        private System.Windows.Forms.Button detailsButton;

        private string message;
        private Exception exception;

        public ExceptionForm(string msg, Exception ex)
        {
            message = msg;
            exception = ex;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Error";

            okButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            detailsButton = new System.Windows.Forms.Button();
            pictureBox = new System.Windows.Forms.PictureBox();
            errorLabel = new System.Windows.Forms.Label();
            errorMessageLabel = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // okButton
            // 
            okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            okButton.Location = new System.Drawing.Point(160, 100);
            okButton.Name = "okButton";
            okButton.TabIndex = 0;
            okButton.Text = "OK";
            okButton.Click += new EventHandler(okButton_Click);
            AcceptButton = okButton;
            // 
            // cancelButton
            // 
            cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            cancelButton.Location = new System.Drawing.Point(240, 100);
            cancelButton.Name = "cancelButton";
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            CancelButton = cancelButton;
            // 
            // detailsButton
            // 
            detailsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            detailsButton.Location = new System.Drawing.Point(320, 100);
            detailsButton.Name = "detailsButton";
            detailsButton.TabIndex = 2;
            detailsButton.Text = "Details";
            detailsButton.Click += new EventHandler(detailsButton_Click);
            // 
            // pictureBox
            // 
            pictureBox.Location = new System.Drawing.Point(11, 11);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(40, 40);
            pictureBox.TabIndex = 3;
            pictureBox.TabStop = false;
            Bitmap errorImage = new Bitmap(WixFiles.GetResourceStream("error.png"));
            errorImage.MakeTransparent();
            pictureBox.Image = errorImage;
            // 
            // errorLabel
            // 
            errorLabel.Location = new System.Drawing.Point(64, 20);
            errorLabel.Name = "errorLabel";
            errorLabel.Size = new System.Drawing.Size(456, 40);
            errorLabel.TabIndex = 4;
            errorLabel.Text = message;
            // 
            // errorLabel
            // 
            errorMessageLabel.Location = new System.Drawing.Point(64, 60);
            errorMessageLabel.Name = "errorMessageLabel";
            errorMessageLabel.Size = new System.Drawing.Size(456, 40);
            errorMessageLabel.TabIndex = 4;
            errorMessageLabel.Text = "Error message: " + exception.Message;
            // 
            // ExceptionForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(545, 134);
            Controls.Add(errorLabel);
            Controls.Add(errorMessageLabel);
            Controls.Add(pictureBox);
            Controls.Add(detailsButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExceptionForm";
            ShowInTaskbar = false;
            ResumeLayout(false);

            okButton.Focus();

            CenterToScreen();
        }

        private void detailsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(exception.ToString(), "Exception Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}