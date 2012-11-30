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
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace WixEdit.Controls {
    public class IconMenuItem : MenuItem {
        private Bitmap bitmap;

        private Font font;
        private int menuSeperaterWidth;

        private Color menuBackColor;
        private Color menuSeperaterColor;
        private Color selectedGradientLeft;
        private Color selectedGradientRight;
        private Color sideBarColor;
        private Color selectedBorder;
        private int sideBarWidth;

        public IconMenuItem() {    
            Init();
        }

        public IconMenuItem(Icon icon) {
            bitmap = icon.ToBitmap();
            
            Init();
        }

        public IconMenuItem(string text, Icon icon) {
            bitmap = icon.ToBitmap();
            Text = text;

            Init();
        }

        public IconMenuItem(Bitmap bitmap) {
            this.bitmap = bitmap;
            this.bitmap.MakeTransparent();
            
            Init();
        }

        public IconMenuItem(string text, Bitmap bitmap) {
            this.bitmap = bitmap;
            this.bitmap.MakeTransparent();
            Text = text;

            Init();
        }

        public IconMenuItem(string text) {
            Text = text;

            Init();
        }

        private bool HasFancyMenus() {
            return (Environment.OSVersion.Version.Major > 5  ||
                ( Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1 ));
        }

        public Bitmap Bitmap {
            get {
                return bitmap;
            }
            set {
                bitmap = value;
            }
        }

        public bool HasIcon() {
            return (bitmap != null);
        }

        private void Init() {
            menuBackColor = SystemColors.Menu;
            
            selectedGradientLeft = Color.FromArgb(199,199,202);
            selectedGradientRight = Color.FromArgb(199,199,202);
            selectedBorder = Color.FromArgb(169,171,181);

            menuSeperaterColor = Color.FromArgb(229,228,232);

            if (HasFancyMenus()) {
                sideBarColor = Color.FromArgb(229,228,232);
            } else {
                sideBarColor = menuBackColor;
            }
            sideBarWidth = 20;

            font = SystemInformation.MenuFont;
            menuSeperaterWidth = 1;

            OwnerDraw = true;
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e) {
            StringFormat format = new StringFormat();
            format.SetTabStops(0, new Single[] {0});
            format.HotkeyPrefix = HotkeyPrefix.Show;
            float textWidth = e.Graphics.MeasureString(GetFormattedText(), font, 50000, format).Width;

            if (Parent.GetType().Equals(typeof(MainMenu))) {
                e.ItemWidth = (int) (textWidth) - 2;
                e.ItemHeight = SystemInformation.MenuHeight + 2;
            } else {
                e.ItemWidth = e.ItemHeight + (int)Math.Ceiling(textWidth) + sideBarWidth + 10;

                if (IsSeparator()) {
                    e.ItemHeight = menuSeperaterWidth + 2;
                } else {
                    e.ItemHeight = SystemInformation.MenuHeight + 2;
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e) {
            if (Parent == null) {
                return;
            }

            if (Parent.GetType().Equals(typeof(MainMenu))) {
                DrawMainMenu(e.Graphics, e.Bounds, e.State);
            } else if (IsSeparator()) {
                DrawSeparator(e.Graphics, e.Bounds);
            } else {
                Rectangle backRect = e.Bounds;
                backRect.X += 1;

                if (IsSelected(e.State)) {
                    if (Enabled) {
                        DrawSelection(e.Graphics, backRect);
                    } else {
                        DrawDisabledSelection(e.Graphics, backRect);
                    }
                    if (HasIcon()) {
                        if (Enabled) {
                            DrawSelectedIcon(e.Graphics, e.Bounds);
                        } else {
                            DrawDisabledIcon(e.Graphics, e.Bounds);
                        }
                    }
                } else {
                    DrawClear(e.Graphics, backRect);
                    DrawSideBar(e.Graphics, e.Bounds);
                    
                    if (HasIcon()) {
                        if (Enabled) {
                            DrawNormalIcon(e.Graphics, e.Bounds);
                        } else {
                            DrawDisabledIcon(e.Graphics, e.Bounds);
                        }
                    }
                }
        
                DrawText(e.Graphics, e.Bounds);

                if (Checked) {
                    if (HasIcon() == false) {
                        DrawCheck(e.Graphics, e.Bounds);
                    } else {
                        DrawCheckedIcon(e.Graphics, e.Bounds);
                    }
                }
            }
        }

        private void DrawClear(Graphics graphics, Rectangle dest) {
            graphics.FillRectangle(new SolidBrush(menuBackColor), dest);
        }

        private void DrawSelectedIcon(Graphics graphics, Rectangle dest) {
            ImageAttributes a = new ImageAttributes();

            Rectangle iconDest = new Rectangle(dest.Left + 4, dest.Top + 5, 16, 16); 
            graphics.DrawImage(MakeMonochrome(Bitmap, Color.Gray), iconDest, 0,0,16,16, GraphicsUnit.Pixel, a);

            iconDest = new Rectangle(dest.Left + 2, dest.Top + 3, 16, 16); 
            graphics.DrawImage(Bitmap, iconDest, 0,0,16,16, GraphicsUnit.Pixel, a);

//            graphics.DrawIcon(icon, dest.Left + 2, dest.Top + 3);
        }

        private void DrawDisabledIcon(Graphics graphics, Rectangle dest) {
            ControlPaint.DrawImageDisabled(graphics, Bitmap, dest.Left + 3, dest.Top + 4, menuBackColor);
        }

        private void DrawNormalIcon(Graphics graphics, Rectangle dest) {
            ImageAttributes a = new ImageAttributes();
            Rectangle iconDest = new Rectangle(dest.Left + 3, dest.Top + 4, 16, 16); 
            
            ColorMatrix cm = new ColorMatrix();
            cm.Matrix33 = 0.7F; // Opacity
            
            a.SetColorMatrix(cm);
            
            graphics.DrawImage(Bitmap, iconDest, 0,0,16,16, GraphicsUnit.Pixel, a);
        }

        private void DrawSideBar(Graphics graphics, Rectangle dest) {
            graphics.FillRectangle(new SolidBrush(sideBarColor), 0, dest.Top, sideBarWidth, dest.Height);
        }

        private void DrawSelection(Graphics graphics, Rectangle dest) {
            Brush br = new LinearGradientBrush(dest, selectedGradientLeft, selectedGradientRight, 0F);
            graphics.FillRectangle(br, dest.X, dest.Y , dest.Width - 1, dest.Height - 1);
            graphics.DrawRectangle(Pens.Gray, dest.X - 1, dest.Y , dest.Width - 1, dest.Height - 1);
        }

        private void DrawDisabledSelection(Graphics graphics, Rectangle dest) {
            graphics.DrawRectangle(Pens.Gray, dest.X - 1, dest.Y , dest.Width - 1, dest.Height - 1);
        }

        private void DrawText(Graphics graphics, Rectangle dest) {
            Brush br;

            StringFormat format = new StringFormat();
            format.SetTabStops(60, new Single[] {0});
            format.HotkeyPrefix = HotkeyPrefix.Show;

            // Alligned Right, looks messy. Because the letter widths are not equal
            float scTextWidth = graphics.MeasureString(GetShortcutText(), font, 50000, format).Width;

            int tabStopWidth = dest.Width - sideBarWidth - 5 - (int) Math.Ceiling(scTextWidth);
            if (tabStopWidth > 0) {
                format.SetTabStops(tabStopWidth, new Single[] {0});
            }

            if (Enabled) {
                br = new SolidBrush(Color.Black);
            } else {
                br = new SolidBrush(Color.Gray);
            }

            graphics.DrawString(GetFormattedText(), font, br, dest.Left + sideBarWidth + 5, dest.Top + 4, format);
        }

        
        private void DrawMainMenu(Graphics graphics, Rectangle dest, DrawItemState state) {
            if ((state & DrawItemState.HotLight) == DrawItemState.HotLight) {
                graphics.FillRectangle(new SolidBrush(selectedGradientLeft), dest.X + 1, dest.Y, dest.Width - 1, dest.Height - 1);
                graphics.DrawRectangle(new Pen(selectedBorder), dest.X + 1, dest.Y, dest.Width - 1, dest.Height - 1);
            } else if ((state & DrawItemState.Selected) == DrawItemState.Selected) {
                graphics.FillRectangle(new SolidBrush(sideBarColor), dest.X + 1, dest.Y + 1, dest.Width - 1, dest.Height - 1);
                graphics.DrawLine(new Pen(selectedBorder), dest.Left + 1, dest.Top, dest.Left + 1, dest.Bottom - 1);
                graphics.DrawLine(new Pen(selectedBorder), dest.Left + 1 + (dest.Width - 1), dest.Top + 1, dest.Left + 1 + (dest.Width - 1), dest.Bottom - 1);
                graphics.DrawLine(new Pen(selectedBorder), dest.Left + 1, dest.Top, dest.Left + 1 + (dest.Width - 1), dest.Top);
            } else {
                graphics.FillRectangle(SystemBrushes.Control, dest.X, dest.Y, dest.Width + 1, dest.Height);
            }

            StringFormat format = new StringFormat();
            format.HotkeyPrefix = HotkeyPrefix.Show;
            format.SetTabStops(60, new Single[] {0});

            graphics.DrawString(Text, font, Brushes.Black, dest.Left + 6, dest.Top + 3, format);
        }

        private void DrawSeparator(Graphics graphics, Rectangle dest) {
            graphics.FillRectangle(new SolidBrush(menuBackColor), 0, dest.Top, dest.Width, dest.Height);
            if (HasFancyMenus()) {
                graphics.FillRectangle(new SolidBrush(menuSeperaterColor), sideBarWidth + 5, dest.Top + dest.Height/2, dest.Width, 1);
                graphics.FillRectangle(new SolidBrush(sideBarColor), 0, dest.Top, sideBarWidth, dest.Height);
            } else {
                int mid = (dest.Top+dest.Bottom)/2;
                graphics.DrawLine(SystemPens.ControlDark, 0, mid, dest.Left + 1 + (dest.Width - 1), mid);
                graphics.DrawLine(SystemPens.ControlLightLight, 0, mid+1, dest.Left + 1 + (dest.Width - 1), mid+1);
            }
        }

        private void DrawCheckedIcon(Graphics graphics, Rectangle dest) {
            ImageAttributes a = new ImageAttributes();

            Rectangle iconDest = new Rectangle(dest.Left + 2, dest.Top + 2, 16, 16); 
            graphics.DrawImage(Bitmap, iconDest, 0,0,16,16, GraphicsUnit.Pixel, a);
//            graphics.DrawIcon(icon, dest.Left + 2, dest.Top + 2);
    
            Pen pen;
            if (Enabled) {
                pen = new Pen(Color.Gray);
            } else {
                pen = new Pen(Color.DarkGray);
            }
    
            graphics.DrawRectangle(pen, 1, dest.Top, 20, 20);
            graphics.DrawRectangle(pen, 3, dest.Top + 2, 16, 16);
        }

        private void DrawCheck(Graphics graphics, Rectangle dest) {
            Pen rectPen;
            Pen checkPen;

            if (Enabled) {
                rectPen = new Pen(Color.Gray);
                checkPen = new Pen(Color.Black);
            } else {
                rectPen = new Pen(Color.DarkGray);
                checkPen = new Pen(Color.Gray);
            }

            if (HasFancyMenus()) {
                graphics.DrawRectangle(rectPen, 1, dest.Top+1, sideBarWidth - 2, SystemInformation.MenuHeight-1);
            }

            Point[] pnts = new Point[6];
        
            pnts[0] = new Point(dest.Left + 13, dest.Top + 8);
            pnts[1] = new Point(dest.Left + 9, dest.Top + 12);
            pnts[2] = new Point(dest.Left + 7, dest.Top + 10);
            pnts[3] = new Point(dest.Left + 7, dest.Top + 11);
            pnts[4] = new Point(dest.Left + 9, dest.Top + 13);
            pnts[5] = new Point(dest.Left + 13, dest.Top + 9);

            graphics.DrawLines(checkPen, pnts);
        }

        private bool IsSelected(DrawItemState state) {
            return ((state & DrawItemState.Selected) != DrawItemState.None);
        }

        private bool IsSeparator() {
            return (GetFormattedText() == "-");
        }

        private Bitmap MakeMonochrome(Bitmap input, Color color) {
            Bitmap bitmap1 = new Bitmap(input.Width, input.Height);
            bitmap1.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            Size size1 = input.Size;
            int num1 = input.Width;
            int num2 = input.Height;
            BitmapData data1 = input.LockBits(new Rectangle(0, 0, num1, num2), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData data2 = bitmap1.LockBits(new Rectangle(0, 0, num1, num2), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int num3 = color.ToArgb();
            for (int num4 = 0; num4 < num2; num4++) {
                IntPtr ptr1 = (IntPtr) (((long) data1.Scan0) + (num4 * data1.Stride));
                IntPtr ptr2 = (IntPtr) (((long) data2.Scan0) + (num4 * data2.Stride));
                for (int num5 = 0; num5 < num1; num5++) {
                    int num6 = System.Runtime.InteropServices.Marshal.ReadInt32(ptr1, (int) (num5 * 4));
                    if ((num6 >> 0x18) == 0) {
                        System.Runtime.InteropServices.Marshal.WriteInt32(ptr2, (int) (num5 * 4), 0);
                    }
                    else {
                        System.Runtime.InteropServices.Marshal.WriteInt32(ptr2, (int) (num5 * 4), num3);
                    }
                }
            }
            input.UnlockBits(data1);
            bitmap1.UnlockBits(data2);
            return bitmap1;
        }

        protected string GetShortcutText() {
            string text = "";

            if (ShowShortcut && Shortcut != Shortcut.None) {
                Shortcut s = Shortcut;
                
                Keys k = (Keys)s;
                text = TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(k);
            }          

            return text;
        }
 
        protected string GetFormattedText() {
            string text = Text;

            if (ShowShortcut && Shortcut != Shortcut.None) {
                Shortcut s = Shortcut;
                
                Keys k = (Keys)s;
                text = text + Convert.ToChar(9) + TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(k);
            }

            return text;
        }
    }
}