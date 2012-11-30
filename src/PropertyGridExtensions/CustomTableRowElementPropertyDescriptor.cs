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
using System.IO;
using System.Windows.Forms;
using System.Xml;

using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// PropertyDescriptor for CustomTableRowElements.
    /// </summary>
    public class CustomTableRowElementPropertyDescriptor : CustomXmlPropertyDescriptorBase {
        public CustomTableRowElementPropertyDescriptor(XmlNode rowElement, WixFiles wixFiles, string name, Attribute[] attrs)
            :
            base(wixFiles, rowElement, name, attrs)
        {
        }

        public override Type PropertyType
        {
            get
            {
                Type result = typeof(string);

                XmlElement node = (XmlElement)XmlElement.ParentNode.SelectSingleNode(String.Format("wix:Column[@Id='{0}']", this.Name), this.wixFiles.WxsNsmgr);
                if (node != null)
                {
                    switch (node.GetAttribute("Type"))
                    {
                        case "int":
                        case "integer":
                            result = typeof(int);
                            break;
                        case "string":
                            result = typeof(string);
                            break;
                        case "binary":
                            result = typeof(string);
                            break;
                    }
                }

                return result;
            }
        }

        public override object GetValue(object component) {
            CustomTableRowElementAdapter adapter = (CustomTableRowElementAdapter)component;

            XmlNode node = adapter.XmlElement.SelectSingleNode(String.Format("wix:Data[@Column='{0}']", this.Name), this.wixFiles.WxsNsmgr);
            if (node == null)
            {
                return String.Empty;
            }

            return node.InnerText;
        }

        public override void SetValue(object component, object value) {
            wixFiles.UndoManager.BeginNewCommandRange();

            CustomTableRowElementAdapter adapter = (CustomTableRowElementAdapter)component;

            XmlNode node = adapter.XmlElement.SelectSingleNode(String.Format("wix:Data[@Column='{0}']", this.Name), this.wixFiles.WxsNsmgr);
            if (node == null)
            {
                XmlElement newNode = adapter.XmlElement.OwnerDocument.CreateElement("Data", WixFiles.WixNamespaceUri);
                adapter.XmlElement.AppendChild(newNode);
                newNode.SetAttribute("Column", this.Name);
                node = newNode;
            }

            if (value == null || value.ToString().Length == 0) {
                node.InnerText = String.Empty;
            } else {
                node.InnerText = value.ToString();
            }
        }

        public override bool CanResetValue(object component) {
            return true;
        }
    }
}
