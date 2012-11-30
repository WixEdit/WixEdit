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
using System.Text.RegularExpressions;
using System.Xml;

using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// Summary description for XmlAttributePropertyDescriptor.
    /// </summary>
    public class XmlAttributePropertyDescriptor : CustomXmlPropertyDescriptorBase {
        XmlNode description;

        public XmlAttributePropertyDescriptor(WixFiles wixFiles, XmlAttribute attribute, XmlNode description, string name, Attribute[] attrs) :
            base(wixFiles, attribute, name, attrs) {
            this.description = description;
        }

        public XmlNode AttributeDescription {
            get { return description; }
        }

        public XmlAttribute Attribute {
            get { return (XmlAttribute) XmlElement; }
        }

        public override object GetValue(object component) {
            return Attribute.Value;
        }

        public override void SetValue(object component, object value) {
            wixFiles.UndoManager.BeginNewCommandRange();

            // Object can be a Int or DateTime or String. Etc.
            if (value == null) {
                wixFiles.UndoManager.StartPropertyGridEdit();

                Attribute.Value = String.Empty;

                wixFiles.UndoManager.EndPropertyGridEdit();
            } else {
                string stringValue = value.ToString();

                XmlAttributeAdapter adapter = component as XmlAttributeAdapter;
                XmlNode simpleType = null;

                if (AttributeDescription != null) {
                    XmlAttribute typeAttrib = AttributeDescription.Attributes["type"];
                    if (typeAttrib == null) {
                        simpleType = AttributeDescription.SelectSingleNode("xs:simpleType", adapter.WixFiles.XsdNsmgr);
                    } else {
                        string simpleTypeString = AttributeDescription.Attributes["type"].Value;
                        string selectString = String.Format("/xs:schema/xs:simpleType[@name='{0}']", simpleTypeString);
        
                        simpleType = AttributeDescription.OwnerDocument.SelectSingleNode(selectString, adapter.WixFiles.XsdNsmgr);
                    }
                }

                if (simpleType != null) {
                    XmlNode pattern = simpleType.SelectSingleNode("xs:restriction/xs:pattern", adapter.WixFiles.XsdNsmgr);
                    if (pattern != null && pattern.Attributes["value"] != null) {
                        string patternValue = pattern.Attributes["value"].Value;
                        if (patternValue != null && patternValue.Length > 0) {
                            Match match = Regex.Match(stringValue, patternValue);
                            if (match.Success == false) {
                                XmlNode documentation = simpleType.SelectSingleNode("xs:annotation/xs:documentation", adapter.WixFiles.XsdNsmgr);
                                if (documentation != null) {
                                    throw new Exception(documentation.InnerText);
                                } else {
                                    throw new Exception("Invalide by xsd definition");
                                }
                            }
                        }                    
                    }
                }

                wixFiles.UndoManager.StartPropertyGridEdit();

                Attribute.Value = value.ToString();

                wixFiles.UndoManager.EndPropertyGridEdit();
            }
        }
    }
}
