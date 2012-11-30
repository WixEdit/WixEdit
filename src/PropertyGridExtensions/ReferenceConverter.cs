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
using System.ComponentModel;
using System.Xml;

namespace WixEdit.PropertyGridExtensions {
    public class ReferenceConverter: StringConverter {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
        
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            XmlAttributeAdapter adapter = context.Instance as XmlAttributeAdapter;
            XmlNode xmlNode = adapter.XmlNode;

            string nodeNameRef = xmlNode.Name;
            if (nodeNameRef.EndsWith("Ref")) {
                string nodeName = nodeNameRef.Remove(nodeNameRef.Length-3, 3);

                XmlNodeList referencedNodes = null;
                if (nodeName.IndexOf(":") < 0) {
                    referencedNodes = xmlNode.OwnerDocument.SelectNodes(String.Format("//wix:{0}", nodeName), adapter.WixFiles.WxsNsmgr);
                } else {
                    referencedNodes = xmlNode.OwnerDocument.SelectNodes(String.Format("//{0}", nodeName), adapter.WixFiles.WxsNsmgr);
                }


                ArrayList strings = new ArrayList();
                foreach (XmlNode node in referencedNodes) {
                    if (node.Attributes["Id"] != null) {
                        strings.Add(node.Attributes["Id"].Value);
                    }
                }

                return new StandardValuesCollection(strings.ToArray(typeof(string)));
            } else if (xmlNode.Attributes["BinaryKey"] != null) {
                XmlNodeList referencedNodes = xmlNode.OwnerDocument.SelectNodes("//wix:Binary", adapter.WixFiles.WxsNsmgr);

                ArrayList strings = new ArrayList();
                foreach (XmlNode node in referencedNodes) {
                    strings.Add(node.Attributes["Id"].Value);
                }

                return new StandardValuesCollection(strings.ToArray(typeof(string)));
            } else if (xmlNode.Attributes["FileKey"] != null) {
                XmlNodeList referencedNodes = xmlNode.OwnerDocument.SelectNodes("//wix:File", adapter.WixFiles.WxsNsmgr);

                ArrayList strings = new ArrayList();
                foreach (XmlNode node in referencedNodes) {
                    strings.Add(node.Attributes["Id"].Value);
                }

                return new StandardValuesCollection(strings.ToArray(typeof(string)));
            } else {
                throw new Exception(nodeNameRef + " should be a reference to another nodes. (Should end on \"Ref\")");
            }
        } 
        
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return false;
        }
    }
}