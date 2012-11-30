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


using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System;

namespace WixEdit.Controls {
    /// <summary>
    /// Description of CustomTabControl.
    /// </summary>
    public class CustomTabControl : TabControl {
		public CustomTabControl() {
			this.SetStyle(ControlStyles.DoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e); 
			
			DrawControl(e.Graphics);
		}

		private void DrawControl(Graphics g) {
			if (Visible == false) {
				return;
            }

			Brush br = new SolidBrush(SystemColors.Control);
			g.FillRectangle(br, this.ClientRectangle);
			br.Dispose();

			Pen border = new Pen(SystemColors.ControlDark);
			Rectangle tabRectangle = this.DisplayRectangle;

			if (this.Alignment == TabAlignment.Top) {
    			tabRectangle.Width += 7;
    			tabRectangle.Height += 8;
       		    tabRectangle.Offset(-4, -5);
            } else {
    			tabRectangle.Width += 7;
    			tabRectangle.Height += 7;
       		    tabRectangle.Offset(-4, -4);
            }

			g.DrawRectangle(border, tabRectangle);

			border.Dispose();

			for (int i = 0; i < this.TabCount; i++) {
				DrawTab(g, this.TabPages[i], i);
            }
		}

		private void DrawTab(Graphics g, TabPage tabPage, int index) {
			bool isSelected = (this.SelectedIndex == index);

			Rectangle boundsRectangle = this.GetTabRect(index);
            
            int isSelectOffset = 2;
            if (isSelected) {
                isSelectOffset = 0;
            }

			Point[] pts = new Point[6];
			if (this.Alignment == TabAlignment.Top) {
				pts[0] = new Point(boundsRectangle.Left, boundsRectangle.Bottom - 3);
				pts[1] = new Point(boundsRectangle.Left, boundsRectangle.Top + 2 - 2 + isSelectOffset);
				pts[2] = new Point(boundsRectangle.Left + 2, boundsRectangle.Top - 2 + isSelectOffset);
				pts[3] = new Point(boundsRectangle.Right - 2, boundsRectangle.Top - 2 + isSelectOffset);
				pts[4] = new Point(boundsRectangle.Right, boundsRectangle.Top + 2 - 2 + isSelectOffset);
				pts[5] = new Point(boundsRectangle.Right, boundsRectangle.Bottom - 3);
			} else {
				pts[0] = new Point(boundsRectangle.Left, boundsRectangle.Top + 1);
				pts[1] = new Point(boundsRectangle.Right, boundsRectangle.Top + 1);
				pts[2] = new Point(boundsRectangle.Right, boundsRectangle.Bottom - 2 - isSelectOffset);
				pts[3] = new Point(boundsRectangle.Right - 2, boundsRectangle.Bottom - isSelectOffset);
				pts[4] = new Point(boundsRectangle.Left + 2, boundsRectangle.Bottom - isSelectOffset);
				pts[5] = new Point(boundsRectangle.Left, boundsRectangle.Bottom - 2 - isSelectOffset);
			}

            Brush br;
			if (isSelected) {
    			br = new SolidBrush(tabPage.BackColor);
            } else {
    			br = new SolidBrush(SystemColors.ControlLightLight);
            }

            g.FillPolygon(br, pts);

  			br.Dispose();

			if (isSelected) {
    			g.DrawPolygon(SystemPens.ControlDark, pts);

				Pen pen = new Pen(tabPage.BackColor);
				if  (this.Alignment == TabAlignment.Top) {
					g.DrawLine(pen, boundsRectangle.Left + 1, boundsRectangle.Bottom - 3, boundsRectangle.Right - 1, boundsRectangle.Bottom-3);
                } else {
					g.DrawLine(pen, boundsRectangle.Left + 1, boundsRectangle.Top + 1, boundsRectangle.Right - 1, boundsRectangle.Top+1);
				}
								
				pen.Dispose();
			} else {
    			g.DrawPolygon(SystemPens.ControlDark, pts);
            }

			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;  
			stringFormat.LineAlignment = StringAlignment.Center;

			RectangleF tabTextArea = (RectangleF)this.GetTabRect(index);
			br = new SolidBrush(tabPage.ForeColor);
			if (this.Alignment == TabAlignment.Top) {
       		    tabTextArea.Offset(0, -2);
                if (isSelected == false) {
       		        tabTextArea.Offset(0, isSelectOffset/2);
                }
            } else {
       		    tabTextArea.Offset(0, 1);
                if (isSelected == false) {
       		        tabTextArea.Offset(0, isSelectOffset/-2);
                }
            }

			g.DrawString(tabPage.Text, Font, br, tabTextArea, stringFormat);
		}
    }
}
