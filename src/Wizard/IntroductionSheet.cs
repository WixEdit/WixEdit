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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;

using WixEdit.Xml;

namespace WixEdit.Wizard
{
    class IntroductionSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        PictureBox picture;

        public IntroductionSheet(XmlElement template, WizardForm creator)
            : base(creator)
        {
            Initialize(template.GetAttribute("Title"), template.GetAttribute("Description"));
        }

        public IntroductionSheet(string title, string description, WizardForm creator)
            : base(creator)
        {
            Initialize(title, description);
        }

        private void Initialize(string title, string description)
        {
            this.BackColor = Color.White;

            picture = new PictureBox();
            picture.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            picture.Height = this.Height;
            picture.Left = 0;
            picture.Width = 164;
            picture.Image = new Bitmap(WixFiles.GetResourceStream("dlgbmp.bmp"));
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            this.Controls.Add(picture);

            titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            titleLabel.Height = 50;
            titleLabel.Width = this.Width - picture.Width;
            titleLabel.Top = 0;
            titleLabel.Left = picture.Width;
            titleLabel.Padding = new Padding(7, 12, 0, 0);
            titleLabel.Font = new Font("Verdana",
                                        13,
                                        FontStyle.Bold,
                                        GraphicsUnit.Point
                                    );
            titleLabel.BackColor = Color.White;
            this.Controls.Add(titleLabel);

            descriptionLabel = new Label();
            descriptionLabel.Text = description;

            descriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            descriptionLabel.Width = this.Width - picture.Width;
            descriptionLabel.Left = picture.Width;
            descriptionLabel.Height = this.Height - titleLabel.Height;
            descriptionLabel.Top = titleLabel.Height;
            descriptionLabel.Padding = new Padding(7, 15, 5, 5);
            this.Controls.Add(descriptionLabel);
        }
    }
}
