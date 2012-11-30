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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.About;
using WixEdit.Settings;

namespace WixEdit.Controls {
	/// <summary>
	/// The tab buttons control.
	/// </summary>
	public class TabButtonControl : Panel 	{
        ListBox tabButtons;
        Panel contentPanel;

        ArrayList tabPanels;
        ArrayList tabTexts;
        ArrayList tabBitmaps;

        int selectedIndex;

        bool tabButtonsHasVScrollBar;

        public event EventHandler TabChange;
        
        public TabButtonControl() : base() {
            tabButtons = new ListBox();
            tabPanels = new ArrayList();
            tabTexts = new ArrayList();
            tabBitmaps = new ArrayList();

            tabButtons.Dock = DockStyle.Left;
            tabButtons.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            tabButtons.Location = new Point(0, 0);
            tabButtons.Name = "tabButtons";
            tabButtons.Size = new Size(72, 264);
            tabButtons.DrawMode = DrawMode.OwnerDrawVariable;
            tabButtons.DrawItem += new DrawItemEventHandler(tabButtons_DrawItem);
            tabButtons.MeasureItem += new MeasureItemEventHandler(tabButtons_MeasureItem);
            tabButtons.SelectedIndexChanged += new EventHandler(tabButtons_SelectedIndexChanged);
            tabButtons.SizeChanged += new EventHandler(tabButtons_SizeChanged);

            tabButtons.BackColor = SystemColors.ControlDark;
            tabButtons.ForeColor = Color.White;

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;

            Controls.Add(contentPanel);

            Controls.Add(tabButtons);

            tabButtonsHasVScrollBar = false;
        }
 
        public void AddTab(string tabText, Panel tabPanel, Bitmap tabBitmap) {
            tabButtons.Items.Add(tabText);

            tabPanels.Add(tabPanel);
            tabTexts.Add(tabText);

            tabBitmap.MakeTransparent();
            tabBitmaps.Add(tabBitmap);

            if (tabPanels.Count == 1) {
                contentPanel.Controls.Add(tabPanel);
                tabButtons.SelectedIndex = 0;
            }
        }

        public void ClearTabs() {
            tabButtons.Items.Clear();
            tabPanels.Clear();
            tabTexts.Clear();
            tabBitmaps.Clear();

            contentPanel.Controls.Clear();
        }

        public int SelectedIndex {
            get {
                return selectedIndex;
            }
            set {
                tabButtons.SelectedIndex = value;
            }
        }


        public Panel SelectedPanel {
            get {
                return (Panel) tabPanels[selectedIndex];
            }
            set {
                for (int i = 0; i < tabButtons.Items.Count; i++) {
                    if (value == tabPanels[i]) {
                        tabButtons.SelectedIndex = i;
                        return;
                    }
                }

                throw new Exception("Panel not found in TabButtonControl.");
            }
        }

        private void tabButtons_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e) {
            if (e.Index == -1) {
                return;
            }

            // Draw the background of the ListBox control for each item.
            e.DrawBackground();

            // Create a new Brush and initialize to a Black colored brush by default.
            Brush myBrush = Brushes.Black;
            
            // Determine the color of the brush to draw each item based on the index of the item to draw.
            myBrush = Brushes.White;
            
            
            StringFormat textFormat = StringFormat.GenericDefault.Clone() as StringFormat;
            textFormat.Alignment =  StringAlignment.Center;

            SizeF textSize = e.Graphics.MeasureString(tabButtons.Items[e.Index].ToString(), e.Font, new Point(e.Bounds.X, e.Bounds.Y), textFormat);

            Rectangle bounds = e.Bounds;//new Rectangle(e.Bounds.X,e.Bounds.Y,e.Bounds.Width,e.Bounds.Height);
            bounds.Y += bounds.Height;
            bounds.Y -= Convert.ToInt32(textSize.Height);

            e.Graphics.DrawString(tabButtons.Items[e.Index].ToString(), e.Font, myBrush,bounds, textFormat);
            
            Bitmap img = tabBitmaps[e.Index] as Bitmap;
            Rectangle imgBounds = e.Bounds;

            Point imgPos = new Point(e.Bounds.X, e.Bounds.Y+2);
            imgPos.X += (e.Bounds.Width-img.Width)/2;
            imgPos.Y += ((e.Bounds.Height-Convert.ToInt32(textSize.Height))/2) - ((img.Width)/2);
            e.Graphics.DrawImage(img, imgPos);

            if (e.Index == selectedIndex) {
                Rectangle borderRect = e.Bounds;
                borderRect.Height = borderRect.Height - 1;
                borderRect.Width = borderRect.Width - 1;
                e.Graphics.DrawRectangle(new Pen(SystemColors.ControlDarkDark), borderRect);
            }
        }

        private void tabButtons_SizeChanged(object sender, EventArgs e) {
            bool hasVScroll = (tabButtons.ClientSize.Width + SystemInformation.VerticalScrollBarWidth < tabButtons.Size.Width);
            if (hasVScroll != tabButtonsHasVScrollBar) {
                tabButtons.Invalidate();
                tabButtonsHasVScrollBar = hasVScroll;
            }
        }

        private void tabButtons_SelectedIndexChanged(object sender, EventArgs e) {
            if (tabButtons.SelectedIndex == -1) {
                contentPanel.Controls.Clear();
            } else if (contentPanel.Controls.Count == 0) {
                Panel newPanel = tabPanels[tabButtons.SelectedIndex] as Panel;
                contentPanel.Controls.Add(newPanel);
            } else if (tabPanels[tabButtons.SelectedIndex] != contentPanel.Controls[0]) {
                Panel newPanel = tabPanels[tabButtons.SelectedIndex] as Panel;
                contentPanel.Controls.Clear();
                contentPanel.Controls.Add(newPanel);
            }

            selectedIndex = tabButtons.SelectedIndex;
            tabButtons.Invalidate();

            if (TabChange != null) {
                TabChange(this, new EventArgs());
            }
        }
        
        private void tabButtons_MeasureItem(object sender, MeasureItemEventArgs e) {
            e.ItemHeight += 50;
        }
    }
}