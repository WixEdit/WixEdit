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

using WixEdit.PropertyGridExtensions;
using WixEdit.Xml;
using WixEdit.Controls;

namespace WixEdit.Panels
{
    /// <summary>
    /// Summary description for EditResourcesPanel.
    /// </summary>
    public class EditBinariesPanel : DisplaySimpleBasePanel
    {
        public EditBinariesPanel(WixFiles wixFiles)
            : base(wixFiles, "/wix:Wix/*/wix:Binary", "Binary", "Id", GetValueAttributeName(wixFiles))
        {
            LoadData();
        }

        protected static string GetValueAttributeName(WixFiles wixFiles)
        {
            if (wixFiles.XsdDocument.SelectSingleNode("/xs:schema/xs:element[@name='Binary']/xs:complexType/xs:attribute[@name='SourceFile']", wixFiles.XsdNsmgr) != null)
            {
                return "SourceFile";
            }
            else if (wixFiles.XsdDocument.SelectSingleNode("/xs:schema/xs:element[@name='Binary']/xs:complexType/xs:attribute[@name='src']", wixFiles.XsdNsmgr) != null)
            {
                return "src";
            }

            throw new ApplicationException("WiX xsd should define src or SourceFile attribute on Binary element");
        }

        protected override object GetPropertyAdapter()
        {
            return new BinaryElementAdapter(CurrentList, WixFiles);
        }

        public override void OnPropertyGridPopupContextMenu(object sender, EventArgs e)
        {
            base.OnPropertyGridPopupContextMenu(sender, e);
            MenuItem menuItem2 = new IconMenuItem("Add &File", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            menuItem2.Click += new EventHandler(OnAddFilePropertyGridItem);
            CurrentGridContextMenu.MenuItems.Add(1, menuItem2);
        }
    }
}

