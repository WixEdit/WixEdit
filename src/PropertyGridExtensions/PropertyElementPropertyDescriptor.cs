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
using System.Xml;

using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// Summary description for PropertyElementPropertyDescriptor.
    /// </summary>
    public class PropertyElementPropertyDescriptor : CustomXmlPropertyDescriptorBase {
        public PropertyElementPropertyDescriptor(WixFiles wixFiles, XmlNode propertyElement, string name, Attribute[] attrs) :
            base(wixFiles, propertyElement, name, attrs) {
        }

        public override object GetValue(object component) {
            if (XmlElement.Attributes["Value"] != null) {
                return XmlElement.Attributes["Value"].Value;
            } else {
                return XmlElement.InnerText;
            }
        }

        public override void SetValue(object component, object value) {
            wixFiles.UndoManager.BeginNewCommandRange();

            wixFiles.UndoManager.StartPropertyGridEdit();

            XmlNode node = this.XmlElement;

            // Object can be a Int or DateTime or String. Etc.
            if (value == null) {
	            if (node.Attributes["Value"] != null) {
	                node.Attributes["Value"].Value = String.Empty;
	            }
                if (node.InnerText != String.Empty) {
	                node.InnerText = String.Empty;
	            }
            } else {
                if (node.Attributes["Value"] != null) {
	                node.Attributes["Value"].Value = value.ToString();
                    if (node.InnerText != String.Empty) {
                        node.InnerText = String.Empty;
                    }
	            } else if (node.FirstChild != null && 
                    node.FirstChild.NodeType == XmlNodeType.CDATA) {
                    if (node.FirstChild.FirstChild != null) {
                        node.FirstChild.AppendChild(node.OwnerDocument.CreateTextNode(value.ToString()));
                    } else {
                        node.FirstChild.InnerText = value.ToString();
                    }
                } else {
                    node.InnerText = value.ToString();
                }
            }

            wixFiles.UndoManager.EndPropertyGridEdit();
        }
    }
}
