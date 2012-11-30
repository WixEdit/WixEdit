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
using System.ComponentModel;
using System.Drawing.Design;

namespace WixEdit.PropertyGridExtensions {

    /// <summary>A type editor for multi line text</summary>
    public class SearchElementTypeEditor : UITypeEditor {
        /// <summary>display a modal form </summary>
        /// <param name="context">see documentation on ITypeDescriptorContext</param>
        /// <returns>the style of the editor</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>used to multi line text</summary>
        /// <param name="context">see documentation on ITypeDescriptorContext</param>
        /// <param name="provider">see documentation on IServiceProvider</param>
        /// <param name="value">the value prior to editing</param>
        /// <returns>the new connection string after editing</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            return EditValue(value as SearchElementObject);
        }

        /// <summary>show the form for the multi line text</summary>
        /// <param name="value">the value prior to editing</param>
        /// <returns>the string after editing</returns>
        public SearchElementObject EditValue(SearchElementObject value) {
            ElementEditForm dialog = new ElementEditForm(value.Element, value.WixFiles);

            dialog.ShowDialog();
            
            return value;
        }
    }
}