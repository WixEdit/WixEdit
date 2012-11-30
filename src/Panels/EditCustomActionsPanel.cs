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
using System.Collections.Specialized;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Xml;
using WixEdit.Controls;

namespace WixEdit.Panels
{
    /// <summary>
    /// Panel to edit Custom Actions.
    /// </summary>
    public class EditCustomActionsPanel : DisplayTreeBasePanel
    {
        public EditCustomActionsPanel(WixFiles wixFiles)
            : base(wixFiles, "/wix:Wix//wix:CustomAction", "CustomAction", "Id")
        {
            LoadData();
        }

        protected override void PopupPanelContextMenu(System.Object sender, System.EventArgs e)
        {
            //clear menu and add import menu
            base.PopupPanelContextMenu(sender, e);

            //add custom menu, index has to be used!!!
            IconMenuItem subMenuItem = new IconMenuItem("New CustomAction", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            subMenuItem.Click += new EventHandler(NewCustomElement_Click);

            PanelContextMenu.MenuItems.Add(0, subMenuItem);
        }
    }
}