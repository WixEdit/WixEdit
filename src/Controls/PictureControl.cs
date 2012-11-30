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
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WixEdit.Controls {
    public class PictureControl : UserControl {
        protected ArrayList pictureControls;
        Bitmap bitmap;

        public PictureControl(Bitmap bitmap, ArrayList pictureControls) {
            this.pictureControls = pictureControls;
            // pictureControls.Add(this);
            this.bitmap = bitmap;

            SetStyle(ControlStyles.Opaque, false); // optional
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(0,0,0,0);
        }

        public Bitmap Bitmap {
            get {
                return bitmap;
            }
        }

        public void Redraw() {
            Draw();
        }

        public void Draw() {
            Control parentForm = Parent.Parent;

            Bitmap bgImage = new Bitmap(parentForm.Width, parentForm.Height);
            parentForm.BackgroundImage = bgImage;

            Graphics graphic = Graphics.FromImage(bgImage);
            graphic.Clear(SystemColors.Control);

            foreach (PictureControl picControl in pictureControls) {
                Point startPoint = picControl.Parent.PointToScreen(new Point(picControl.Left, picControl.Top));
                startPoint = parentForm.PointToClient(startPoint);

                if (picControl.Bitmap != null) {
                    graphic.DrawImage(picControl.Bitmap,
                        startPoint.X,
                        startPoint.Y,
                        picControl.Width,
                        picControl.Height);
                } else {
                    // Oke, just make the area nice and picture like :)
                    Brush brush = new HatchBrush(HatchStyle.OutlinedDiamond, Color.LightGray, Color.GhostWhite);
                    graphic.FillRectangle(brush,
                        startPoint.X,
                        startPoint.Y,
                        picControl.Width,
                        picControl.Height);
                }
            }
        }
    }
}
