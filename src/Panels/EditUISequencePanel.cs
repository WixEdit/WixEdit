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
    /// Panel to edit UISequence data.
    /// </summary>
    public class EditUISequencePanel : DisplayTreeBasePanel
    {
        public EditUISequencePanel(WixFiles wixFiles)
            : base(wixFiles, "/wix:Wix//wix:InstallUISequence|/wix:Wix//wix:AdminUISequence", "Id")
        {
            LoadData();
        }

        protected override void AssignParentNode()
        {
            CurrentParent = ElementLocator.GetUIElement(WixFiles);
        }

        protected override void PopupPanelContextMenu(System.Object sender, System.EventArgs e)
        {
            //clear menu and add import menu
            base.PopupPanelContextMenu(sender, e);
            //add custom menu, index has to be used!!!
            IconMenuItem subMenuItem = new IconMenuItem("New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            IconMenuItem subSubMenuItem1 = new IconMenuItem("InstallUISequence");
            IconMenuItem subSubMenuItem2 = new IconMenuItem("AdminUISequence");

            subSubMenuItem1.Click += new EventHandler(NewCustomElement_Click);
            subSubMenuItem2.Click += new EventHandler(NewCustomElement_Click);

            subMenuItem.MenuItems.Add(subSubMenuItem1);
            subMenuItem.MenuItems.Add(subSubMenuItem2);

            PanelContextMenu.MenuItems.Add(0, subMenuItem);
        }

        protected override void NewCustomElement_Click(object sender, System.EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            CreateNewCustomElement(item.Text);
        }
    }
}