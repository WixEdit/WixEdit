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
    public class SimpleTypeConverter : StringConverter {
        XmlNodeList enumeration; 
        
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            XmlNodeList e = GetEnumeration(context);

            return IsValidEnumeration(e);
        }
        
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            XmlNodeList e = GetEnumeration(context);
            if (IsValidEnumeration(e)) {
                ArrayList strings = new ArrayList();
                foreach (XmlNode node in e) {
                    strings.Add(node.Attributes["value"].Value);
                }

                return new StandardValuesCollection(strings.ToArray(typeof(string)));
            }

            return new StandardValuesCollection(new string[]{});
        } 
        
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            XmlAttributeAdapter adapter = context.Instance as XmlAttributeAdapter;
            XmlAttributePropertyDescriptor desc = context.PropertyDescriptor as XmlAttributePropertyDescriptor;

            XmlNode simpleType = GetSimpleType(desc.AttributeDescription, adapter.WixFiles.XsdNsmgr);
            if (simpleType != null) {
                XmlNodeList e = GetEnumeration(context);
    
                if (IsValidEnumeration(e) == false) {
                    return false;
                }

                XmlNode restriction = simpleType.SelectSingleNode("xs:restriction", adapter.WixFiles.XsdNsmgr);
                XmlAttribute baseAtt = restriction.Attributes["base"];
                if (baseAtt != null && baseAtt.Value != null) {
                    if (baseAtt.Value.ToLower() == "xs:nmtoken") {
                        return true;
                    }
                }
            }

            return false;
        }

        private XmlNodeList GetEnumeration(ITypeDescriptorContext context) {
            XmlAttributeAdapter adapter = context.Instance as XmlAttributeAdapter;
            XmlAttributePropertyDescriptor desc = context.PropertyDescriptor as XmlAttributePropertyDescriptor;

            XmlNode simpleType = GetSimpleType(desc.AttributeDescription, adapter.WixFiles.XsdNsmgr);
            return GetEnumeration(context, simpleType);
        }

        private XmlNodeList GetEnumeration(ITypeDescriptorContext context, XmlNode simpleType) {
            if (enumeration == null) {
                XmlAttributeAdapter adapter = context.Instance as XmlAttributeAdapter;
                enumeration = simpleType.SelectNodes("xs:restriction/xs:enumeration", adapter.WixFiles.XsdNsmgr);
            }

            return enumeration;
        }

        private XmlNode GetSimpleType(XmlNode attributeDescription, XmlNamespaceManager xsdNsmgr) {
            XmlAttribute typeAttrib = attributeDescription.Attributes["type"];
            if (typeAttrib == null) {
                return attributeDescription.SelectSingleNode("xs:simpleType", xsdNsmgr);
            } else {
                string simpleType = attributeDescription.Attributes["type"].Value;
                string selectString = String.Format("/xs:schema/xs:simpleType[@name='{0}']", simpleType);

                return attributeDescription.OwnerDocument.SelectSingleNode(selectString, xsdNsmgr);
            }
        }

        private bool IsValidEnumeration(XmlNodeList e) {
            if (e != null && e.Count > 0) {
                return true;
            }

            return false;
        }
    }
}