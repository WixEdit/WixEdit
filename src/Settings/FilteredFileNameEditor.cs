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
using System.Windows.Forms;
using System.IO;

namespace WixEdit.Settings {
    /// <summary>
    /// a type editor for files with selection filter
    /// </summary>
    public class FilteredFileNameEditor : UITypeEditor {

        public class FilterAttribute : Attribute {
            string filter;
		
            public FilterAttribute(string filter) {
                this.filter = filter;
            }

            public string Filter {
                get { return filter; }
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public FilteredFileNameEditor() {
        }

        /// <summary>display a modal form </summary>
        /// <param name="context">see documentation on ITypeDescriptorContext</param>
        /// <returns>the style of the editor</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>used to show the connection</summary>
        /// <param name="context">see documentation on ITypeDescriptorContext</param>
        /// <param name="provider">see documentation on IServiceProvider</param>
        /// <param name="value">the value prior to editing</param>
        /// <returns>the new connection string after editing</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            if (value is string) {
                FilterAttribute filterAtt = (FilterAttribute) context.PropertyDescriptor.Attributes[typeof(FilterAttribute)];

                string filter = null;
                if (filterAtt != null) {
                    filter = filterAtt.Filter;
                }

                return EditValue(value as string, filter);
            } else {
                throw new Exception("Invalid type");
            }
        }

        /// <summary>show the form for the new connection string based on an an existing one</summary>
        /// <param name="value">the value prior to editing</param>
        /// <returns>the new connection string after editing</returns>
        public string EditValue(string value, string filter) {
            OpenFileDialog dialog = new OpenFileDialog();

            if (filter != null) {
                dialog.Filter = filter;
            }
            
            if (value != null && value.Length > 0) {
                dialog.InitialDirectory = Path.GetFullPath(value);
                dialog.FileName = Path.GetFullPath(value);
            }

            DialogResult result = dialog.ShowDialog();
            if(result == DialogResult.OK) {
                return dialog.FileName;
            }

            return value;
        }
    }
}