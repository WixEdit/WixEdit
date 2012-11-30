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
using System.Xml;

using WixEdit.Settings;
using WixEdit.Xml;
using WixEdit.Panels;

namespace WixEdit.Controls {
    public delegate void SelectionOverlayItemHandler(XmlNode item);

    public class SelectionOverlay : UserControl {
        static int snapToGrid = 5;

        WixFiles wixFiles;

        protected Control control;
        protected XmlNode xmlNode;
        protected bool isSelected = false;

        private static event EventHandler OnLosesSelection;

        public event SelectionOverlayItemHandler ItemChanged;
        public event SelectionOverlayItemHandler ItemDeleted;
        public event SelectionOverlayItemHandler SelectionChanged;

        static SelectionOverlay() {
            snapToGrid = WixEditSettings.Instance.SnapToGrid;
        }

        public SelectionOverlay(Control control, XmlNode xmlNode, WixFiles wixFiles) {
            this.control = control;
            this.xmlNode = xmlNode;
            this.wixFiles = wixFiles;

            ClientSize = new Size(control.Size.Width, control.Size.Height);
            Left = control.Left;
            Top = control.Top;

            control.Left = 0;
            control.Top = 0;

            control.MouseMove += new MouseEventHandler(OnMouseMoveControl);
            control.MouseDown += new MouseEventHandler(OnMouseDownControl);
            control.MouseUp += new MouseEventHandler(OnMouseUpControl);
            control.KeyDown += new KeyEventHandler(OnKeyDownControl);

            Controls.Add(control);

            Cursor = Cursors.SizeAll;

            OnLosesSelection += new EventHandler(onLostSelection);

            SetStyle(ControlStyles.Opaque, false); // optional
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(0,0,0,0);
        }

        void OnKeyDownControl(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (ItemDeleted != null)
                {
                    ItemDeleted(xmlNode);
                    e.Handled = true;
                }
            }
        }

        public bool IsSelected {
            get {
                return isSelected;
            }
            set {
                if (value) {
                    GotSelection();
                } else {
                    LostSelection();
                }
            }
        }

        public static int SnapToGrid {
            get {
                return snapToGrid;
            }
            set {
                snapToGrid = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint (e);

            if (isSelected) {
                DrawSelection(control, e.Graphics);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) {
            base.OnPaintBackground (pevent);
        }

        bool isMoving = false;
        Point isPositionPoint;
        SizingDirection sizingDirection = SizingDirection.None;

        private enum SizingDirection {
            None = 0,
            SizingNW = 1,
            SizingN = 2,
            SizingNE = 3,
            SizingE = 4,
            SizingSE = 5,
            SizingS = 6,
            SizingSW = 7,
            SizingW = 8
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            Point currentPoint = PointToScreen(new Point(e.X, e.Y));
            
            if (e.Button == MouseButtons.Left) {
                CheckMouseDown(currentPoint);
            }

            GotSelection();
        }

        protected void OnMouseDownControl(object sender, MouseEventArgs e) {
            Point currentPoint = control.PointToScreen(new Point(e.X, e.Y));

            if (e.Button == MouseButtons.Left) {
                CheckMouseDown(currentPoint);
            }

            GotSelection();
        }

        private void GotSelection() {
            OnLosesSelection(this, new EventArgs());

            if (isSelected == false) {
                SuspendLayout();

                Size = new Size(Width + 14, Height + 14);
                Location = new Point(Left - 7, Top - 7);
                control.Location = new Point(7, 7);

                isSelected = true;

                ResumeLayout();

                if (control is PictureControl || control is GroupBox) {
                    SendToBack();
                } else {
                    BringToFront();
                }

                if (control is PictureControl) {
                    PictureControl picControl = (PictureControl) control;
                    picControl.Redraw();
                }

                if (SelectionChanged != null) {
                    SelectionChanged(xmlNode);
                }
            }
        }

        private void onLostSelection(object sender, EventArgs e) {
            if (sender == this) {
                return;
            }

            LostSelection();
        }
        
        private void LostSelection() {
            if (isSelected) {
                SuspendLayout();

                Size = new Size(Width - 14, Height - 14);
                Location = new Point(Left + 7, Top + 7);
                control.Location = new Point(0, 0);
                
                isSelected = false;

                ResumeLayout();
            }
        }

        protected void CheckMouseDown(Point screenPoint) {
            Point clientPoint = PointToClient(screenPoint);
            if (clientPoint.X < 7 && clientPoint.Y < 7) {
                sizingDirection = SizingDirection.SizingNW;
            } else if (clientPoint.X > Width - 7 && clientPoint.Y < 7) {
                sizingDirection = SizingDirection.SizingNE;
            } else if (clientPoint.X < 7 && clientPoint.Y > Height - 7) {
                sizingDirection = SizingDirection.SizingSW;
            } else if (clientPoint.X > Width - 7 && clientPoint.Y > Height - 7) {
                sizingDirection = SizingDirection.SizingSE;
            } else if (clientPoint.X < 7 && clientPoint.Y > ((Height-7)/2) && clientPoint.Y < ((Height+7)/2)) {
                sizingDirection = SizingDirection.SizingW;
            } else if (clientPoint.X > Width - 7 && clientPoint.Y > ((Height-7)/2) && clientPoint.Y < ((Height+7)/2)) {
                sizingDirection = SizingDirection.SizingE;
            } else if (clientPoint.Y < 7 && clientPoint.X > ((Width-7)/2) && clientPoint.X < ((Width+7)/2)) {
                sizingDirection = SizingDirection.SizingN;
            } else if (clientPoint.Y > Height - 7 && clientPoint.X > ((Width-7)/2) && clientPoint.X < ((Width+7)/2)) {
                sizingDirection = SizingDirection.SizingS;
            } else {
                isMoving = true;
            }

            isPositionPoint = screenPoint;
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp (e);

            OnMouseUp();
        }

        protected void OnMouseUpControl(object sender, MouseEventArgs e) {
            OnMouseUp();
        }

        private void OnMouseUp() {
            isMoving = false;
            sizingDirection = SizingDirection.None;

            Point topLeft = PointToScreen(new Point(control.Left, control.Top));
            topLeft = Parent.PointToClient(topLeft);

            wixFiles.UndoManager.BeginNewCommandRange();

            AssignIfChanged(xmlNode.Attributes["Width"], DialogGenerator.PixelsToDialogUnitsWidth(control.Width));
            AssignIfChanged(xmlNode.Attributes["X"], DialogGenerator.PixelsToDialogUnitsWidth(topLeft.X));
            AssignIfChanged(xmlNode.Attributes["Height"], DialogGenerator.PixelsToDialogUnitsHeight(control.Height));
            AssignIfChanged(xmlNode.Attributes["Y"], DialogGenerator.PixelsToDialogUnitsHeight(topLeft.Y));

            if (ItemChanged != null) {
                ItemChanged(xmlNode);
            }
        }

        private void AssignIfChanged(XmlAttribute att, int newValue) {
            string stringValue = att.Value;
            int integerValue = Int32.Parse(stringValue);

            if (integerValue != newValue) {
                att.Value = newValue.ToString();
            }
        }
        
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove (e);

            Point currentPoint = PointToScreen(new Point(e.X, e.Y));
            OnMove(currentPoint);
        }

        protected void OnMouseMoveControl(object sender, MouseEventArgs e) {
            Point currentPoint = control.PointToScreen(new Point(e.X, e.Y));
            OnMove(currentPoint);
        }

        protected void OnMove(Point currentPoint) {
            if (isMoving) {
                bool needsRedraw = false;
                if (Math.Abs(currentPoint.X - isPositionPoint.X) >= snapToGrid) {
                    if (snapToGrid <= 1) {
                        Left = Left + currentPoint.X - isPositionPoint.X;
                        isPositionPoint.X = currentPoint.X;
                    } else {
                        Left = (((Left + 7 + currentPoint.X - isPositionPoint.X) / snapToGrid) * snapToGrid) - 7;
                        isPositionPoint.X = currentPoint.X - ((Left + 7 + currentPoint.X - isPositionPoint.X) % snapToGrid);
                    }
                    needsRedraw = true;
                }

                if (snapToGrid <= 1 || Math.Abs(currentPoint.Y - isPositionPoint.Y) >= snapToGrid) {
                    if (snapToGrid <= 1) {
                        Top = Top + currentPoint.Y - isPositionPoint.Y;
                        isPositionPoint.Y = currentPoint.Y;
                    } else {
                        Top = (((Top + 7 + currentPoint.Y - isPositionPoint.Y) / snapToGrid) * snapToGrid) - 7;
                        isPositionPoint.Y = currentPoint.Y - ((Top + 7 + currentPoint.Y - isPositionPoint.Y) % snapToGrid);
                    }
                    needsRedraw = true;
                }

                if (needsRedraw && control is PictureControl) {
                    PictureControl picControl = (PictureControl) control;
                    picControl.Redraw();
                }
            } else if (sizingDirection != SizingDirection.None) {
                Point snapToGridPoint = new Point(currentPoint.X, currentPoint.Y);
                bool needsRedraw = false;
                if (snapToGrid <= 1 || Math.Abs(currentPoint.X - isPositionPoint.X) >= snapToGrid) {
                    snapToGridPoint.X = currentPoint.X;
                    needsRedraw = true;
                }

                if (snapToGrid <= 1 || Math.Abs(currentPoint.Y - isPositionPoint.Y) >= snapToGrid) {
                    snapToGridPoint.Y = currentPoint.Y;
                    needsRedraw = true;
                }

                if (needsRedraw) {
                    ResizeControl(snapToGridPoint);
                    isPositionPoint = snapToGridPoint;
    
                    if (control is PictureControl) {
                        PictureControl picControl = (PictureControl) control;
                        picControl.Redraw();
                    }
                }
            } else {
                CheckCursor(currentPoint);
            }   
        }

        protected void ResizeControl(Point screenPoint) {
            Point clientPoint = PointToClient(screenPoint);
            int clientX = clientPoint.X;
            int clientY = clientPoint.Y;
            int controlWidth = control.Width;
            int controlHeight = control.Height;
            int oldWidth = Width;
            int oldHeight = Height;

            switch (sizingDirection) {
                case SizingDirection.SizingSE:
                    Height = clientY;
                    Width = clientX;
                    control.Height = clientY - 14;
                    control.Width = clientX - 14;
                    break;
                case SizingDirection.SizingSW:
                    Left += clientX;
                    Height = clientY;
                    Width -= clientX;
                    control.Height = clientY - 14;
                    control.Width -= clientX;
                    break;
                case SizingDirection.SizingNW:
                    Top += clientY;
                    Left += clientX;
                    Height -= clientY;
                    Width -= clientX;
                    control.Height -= clientY;
                    control.Width -= clientX;
                    break;
                case SizingDirection.SizingNE:
                    Top += clientY;
                    Height -= clientY;
                    Width = clientX;
                    control.Height -= clientY;
                    control.Width = clientX;
                    break;
                case SizingDirection.SizingS:
                    Height = clientY;
                    control.Height = clientY - 14;
                    break;
                case SizingDirection.SizingN:
                    Top += clientY;
                    Height -= clientY;
                    control.Height -= clientY;
                    break;
                case SizingDirection.SizingE:
                    Width = clientX;
                    control.Width = clientX - 14;
                    break;
                case SizingDirection.SizingW:
                    Left += clientX;
                    Width -= clientX;
                    control.Width -= clientX;
                    break;
            }

            if (controlWidth == control.Width) {
                Width = oldWidth;
            }
            if (controlHeight == control.Height) {
                Height = oldHeight;
            }

            Invalidate();
        }

        public void CheckCursor(Point screenPoint) {
            Point clientPoint = PointToClient(screenPoint);

            if (clientPoint.X < 7 && clientPoint.Y < 7) {
                Cursor = Cursors.SizeNWSE;
            } else if (clientPoint.X > Width - 7 && clientPoint.Y < 7) {
                Cursor = Cursors.SizeNESW;
            } else if (clientPoint.X < 7 && clientPoint.Y > Height - 7) {
                Cursor = Cursors.SizeNESW;
            } else if (clientPoint.X > Width - 7 && clientPoint.Y > Height - 7) {
                Cursor = Cursors.SizeNWSE;
            } else if ((clientPoint.X < 7 || clientPoint.X > Width - 7) && clientPoint.Y > ((Height-7)/2) && clientPoint.Y < ((Height+7)/2)) {
                Cursor = Cursors.SizeWE;
            } else if ((clientPoint.Y < 7 || clientPoint.Y > Height - 7) && clientPoint.X > ((Width-7)/2) && clientPoint.X < ((Width+7)/2)) {
                Cursor = Cursors.SizeNS;
            } else {
                Cursor = Cursors.SizeAll;
            }
        }
        
        public void DrawSelection(Control ctrl, Graphics formGraphics) {
            int size = 6;
            Brush horBorderBrush = new TextureBrush(new Bitmap(WixFiles.GetResourceStream("hcontrolborder.bmp")));
            Brush verBorderBrush = new TextureBrush(new Bitmap(WixFiles.GetResourceStream("vcontrolborder.bmp")));

            Rectangle topBorder = new Rectangle(ctrl.Left, ctrl.Top - size - 1, ctrl.Width, size + 1);
            Rectangle bottomBorder = new Rectangle(ctrl.Left, ctrl.Bottom, ctrl.Width, size + 1);

            formGraphics.FillRectangles(horBorderBrush, new Rectangle[] {topBorder, bottomBorder});

            Rectangle rightBorder = new Rectangle(ctrl.Right, ctrl.Top, size + 1, ctrl.Height);
            Rectangle leftBorder = new Rectangle(ctrl.Left - size - 1, ctrl.Top, size + 1, ctrl.Height);

            formGraphics.FillRectangles(verBorderBrush, new Rectangle[] {rightBorder, leftBorder});


            Rectangle leftTop = new Rectangle(ctrl.Left - size - 1, ctrl.Top - size - 1, size, size);
            Rectangle rightTop = new Rectangle(ctrl.Right, ctrl.Top - size - 1, size, size);

            Rectangle leftBottom = new Rectangle(ctrl.Left - size - 1, ctrl.Bottom, size, size);
            Rectangle rightBottom = new Rectangle(ctrl.Right, ctrl.Bottom, size, size);

            Rectangle leftMid = new Rectangle(ctrl.Left - size - 1, ctrl.Top + (ctrl.Height-size)/2, size, size);
            Rectangle rightMid = new Rectangle(ctrl.Right, ctrl.Top + (ctrl.Height-size)/2, size, size);

            Rectangle midBottom = new Rectangle(ctrl.Left + (ctrl.Width-size)/2, ctrl.Bottom, size, size);
            Rectangle midTop = new Rectangle(ctrl.Left + (ctrl.Width-size)/2, ctrl.Top - size - 1, size, size);


            formGraphics.FillRectangles(Brushes.White, new Rectangle[] {leftTop, rightTop, leftBottom, rightBottom, leftMid, rightMid, midBottom, midTop});
            formGraphics.DrawRectangles(Pens.Black, new Rectangle[] {leftTop, rightTop, leftBottom, rightBottom, leftMid, rightMid, midBottom, midTop});
        }
    }
}