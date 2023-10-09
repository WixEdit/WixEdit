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
using System.Globalization;
using System.Drawing.Design;
using System.Windows.Forms;

namespace WixEdit.Settings {

    /// <summary>
    /// a type editor for the BinDirectoryStructure
    /// </summary>
    public class BinDirectoryStructureEditor : UITypeEditor {

        /// <summary>
        /// constructor
        /// </summary>
        public BinDirectoryStructureEditor() {
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
            if (value == null) {
                return EditValue("", context);
            } else if (value is string) {
                return EditValue(value as string, context);
            } else if (value is BinDirectoryStructure) {
                return EditValue(value as BinDirectoryStructure, context);
            } else {
                throw new Exception("Invalid type");
            }
        }

        /// <summary>show the form for the new connection string based on an an existing one</summary>
        /// <param name="value">the value prior to editing</param>
        /// <returns>the new connection string after editing</returns>
        public string EditValue(string value, ITypeDescriptorContext context) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            
            DescriptionAttribute descAtt = (DescriptionAttribute) context.PropertyDescriptor.Attributes[typeof(DescriptionAttribute)];
            if (descAtt != null) {
                dialog.Description = descAtt.Description;
            }

            // Allow the user to create new files via the FolderBrowserDialog.
            dialog.ShowNewFolderButton = false;

            // Default to the My Documents folder.
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;

            if (value != null) {
                dialog.SelectedPath = value;
            }

            DialogResult result = dialog.ShowDialog();
            if(result == DialogResult.OK) {
                return dialog.SelectedPath;
            }

            return value;
        }

        public BinDirectoryStructure EditValue(BinDirectoryStructure value, ITypeDescriptorContext context) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DescriptionAttribute descAtt = (DescriptionAttribute) context.PropertyDescriptor.Attributes[typeof(DescriptionAttribute)];
            if (descAtt != null) {
                dialog.Description = descAtt.Description;
            }

            // Allow the user to create new files via the FolderBrowserDialog.
            dialog.ShowNewFolderButton = false;

            // Default to the My Documents folder.
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;

            dialog.SelectedPath = value.BinDirectory;

            DialogResult result = dialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                value.BinDirectory = dialog.SelectedPath;
                value.Wix = null;
                value.Candle = null;
                value.Xsds = null;
                value.Dark = null;
            }

            return value;
        }
    }
}