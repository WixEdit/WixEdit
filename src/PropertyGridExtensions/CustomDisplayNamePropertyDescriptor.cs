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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions
{
    /// <summary>
    /// Summary description for XmlAttributeBinaryDescriptor.
    /// </summary>
    public class CustomDisplayNamePropertyDescriptor : CustomPropertyDescriptorBase {
        public CustomDisplayNamePropertyDescriptor(WixFiles wixFiles, PropertyInfo propInfo, Attribute[] attrs) 
            : base(wixFiles, AddSpaces(propInfo.Name), propInfo, attrs) {
        }

        private static string AddSpaces(string input) {
            string ret = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");

            // WiX should be spelled correctly... ;-)
            return ret.Replace("Wix", "WiX").Replace("wix", "WiX");
        }

        private static string MakeUppercaseFirstLetter(bool useUppercaseFirstLetter, string input) {
            if (useUppercaseFirstLetter) {
                return String.Format("{0}{1}", input.Substring(0, 1).ToUpper(), input.Substring(1));
            } else {
                return input;
            }
        }

        public CustomDisplayNamePropertyDescriptor(WixFiles wixFiles, PropertyInfo propInfo, Attribute[] attrs, bool useUppercaseFirstLetter) 
            : base(wixFiles, MakeUppercaseFirstLetter(useUppercaseFirstLetter, AddSpaces(propInfo.Name)), propInfo, attrs) {
        }

        public override object GetValue(object component) {
            return propertyInfo.GetValue(component, new object[] {});
        }

        public override void SetValue(object component, object value) {
            if (wixFiles != null) {
                wixFiles.UndoManager.BeginNewCommandRange();
            }

            propertyInfo.SetValue(component, value, null);
        }
    }
}
