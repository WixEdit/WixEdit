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
using System.Drawing.Design;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Xml;
using WixEdit.Settings;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// This class adapts attributes of a xml node to properties, suitable for the <c>PropertyGrid</c>.
    /// </summary>
    public class XmlAttributeAdapter : PropertyAdapterBase {
        protected bool editSubNodes;
        protected XmlNode xmlNode;
        protected XmlNode xmlNodeDefinition;
        protected XmlNode xmlNodeElement;
        protected bool showInnerTextIfEmpty;

        private static ArrayList warnedNodeNames = new ArrayList();

        public bool ShowInnerTextIfEmpty {
            get {
                return showInnerTextIfEmpty;
            }
            set {
                showInnerTextIfEmpty = value;
            }
        }

        public XmlAttributeAdapter(XmlNode xmlNode, WixFiles wixFiles) : this(xmlNode, wixFiles, false) {
        }

        public XmlAttributeAdapter(XmlNode xmlNode, WixFiles wixFiles, bool editSubNodes) : base(wixFiles) {
            this.xmlNode = xmlNode;
            this.editSubNodes = editSubNodes;

            xmlNodeElement = WixFiles.GetXsdElementNode(xmlNode.Name);

            if (xmlNodeElement == null) {
                if (xmlNode.NodeType != XmlNodeType.ProcessingInstruction) {
                    if (warnedNodeNames.Contains(xmlNode.Name) == false) {
                        MessageBox.Show(String.Format("\"{0}\" is not supported in version {1} of WiX!\r\n\r\nPossibly this type is supported in another version of WiX and wix.xsd.", xmlNode.Name, WixEditSettings.Instance.WixBinariesVersion), xmlNode.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        warnedNodeNames.Add(xmlNode.Name);
                    }
                }
            } else {
                XmlNode deprecated = xmlNodeElement.SelectSingleNode("xs:annotation/xs:appinfo/xse:deprecated", wixFiles.XsdNsmgr);
                if (deprecated != null) {
                    if (warnedNodeNames.Contains(xmlNode.Name) == false) {
                        string msg = String.Format("\"{0}\" is deprecated in version {1} of WiX.", xmlNode.Name, WixEditSettings.Instance.WixBinariesVersion);
                        if (deprecated.Attributes["ref"] != null) {
                            msg = String.Format("{0}\r\n\r\nPlease use \"{1}\" instead.", msg, deprecated.Attributes["ref"].Value);
                        }
                        MessageBox.Show(msg, xmlNode.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        warnedNodeNames.Add(xmlNode.Name);
                    }
                }

                if (xmlNodeElement.Attributes["type"] != null && xmlNodeElement.Attributes["type"].Value != null) {
                    xmlNodeDefinition = wixFiles.XsdDocument.SelectSingleNode(String.Format("/xs:schema/xs:complexType[@name='{0}']/xs:simpleContent/xs:extension", xmlNodeElement.Attributes["type"].Value), wixFiles.XsdNsmgr);
                    if (xmlNodeDefinition == null) {
                        xmlNodeDefinition = wixFiles.XsdDocument.SelectSingleNode(String.Format("/xs:schema/xs:complexType[@name='{0}']", xmlNodeElement.Attributes["type"].Value), wixFiles.XsdNsmgr);
                        showInnerTextIfEmpty = false;
                    } else {
                        showInnerTextIfEmpty = true;
                    }
                } else {
                    xmlNodeDefinition = xmlNodeElement.SelectSingleNode("xs:complexType/xs:simpleContent/xs:extension", wixFiles.XsdNsmgr);
                    if (xmlNodeDefinition == null) {
                        xmlNodeDefinition = xmlNodeElement.SelectSingleNode("xs:complexType", wixFiles.XsdNsmgr);
                        if (xmlNodeDefinition == null) {
                            // Nothing?
                            xmlNodeDefinition = xmlNodeElement;
                            showInnerTextIfEmpty = true;
                        } else {
                            showInnerTextIfEmpty = false;
                        }
                    } else {
                        showInnerTextIfEmpty = true;
                    }
                }
            }
        }

        public override void RemoveProperty(XmlNode xmlElement) {
            if (xmlElement == null)
            {
                return;
            }

            if (xmlElement is XmlAttribute) {
                XmlAttribute attrib = xmlElement as XmlAttribute;

                // Show all existing + required elements
                XmlNode xmlAttributeDefinition = xmlNodeDefinition.SelectSingleNode(String.Format("xs:attribute[@name='{0}']", xmlElement.Name), wixFiles.XsdNsmgr);
                if (xmlAttributeDefinition.Attributes["use"] == null || 
                    xmlAttributeDefinition.Attributes["use"].Value != "required") {
                    attrib.OwnerElement.Attributes.Remove((XmlAttribute) xmlElement);
                } else {
                    ((XmlAttribute) xmlElement).Value = "";
                }
            } else { // Is inner text
                if (xmlElement.ChildNodes.Count > 0)
                {
                    xmlElement.RemoveChild(xmlElement.FirstChild);
                }
            }
        }

        public XmlNode XmlNode {
            get { 
                return xmlNode;
            }
        }

        public XmlNode XmlNodeElement {
            get { 
                return xmlNodeElement;
            }
        }

        public XmlNode XmlNodeDefinition {
            get { 
                return xmlNodeDefinition;
            }
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            if (xmlNodeDefinition == null) {
                // maybe the existing properties can be edited?
                ArrayList properties = new ArrayList();
                foreach (XmlAttribute xmlAttribute in xmlNode.Attributes) {
                    // We could add an UITypeEditor if desired
                    ArrayList attrs = new ArrayList();
                    attrs.Add(new EditorAttribute(typeof(UITypeEditor), typeof(UITypeEditor)));
                    attrs.Add(new CategoryAttribute("WXS Attribute"));
                    attrs.Add(new TypeConverterAttribute(typeof(StringConverter)));
    
                
                    // Make Attribute array
                    Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));
    
                    // Create and add PropertyDescriptor
                    XmlAttributePropertyDescriptor pd = new XmlAttributePropertyDescriptor(wixFiles, xmlAttribute, null,
                        xmlAttribute.Name, attrArray);
    
                    properties.Add(pd);
                }


                PropertyDescriptor[] propertiesArray = properties.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[];

                return new PropertyDescriptorCollection(propertiesArray);
            }

            ArrayList props = new ArrayList();

            // Show all existing + required elements
            XmlNodeList xmlAttributeDefinitions = xmlNodeDefinition.SelectNodes("xs:attribute", wixFiles.XsdNsmgr);
            foreach(XmlNode xmlAttributeDefinition in xmlAttributeDefinitions) {
                XmlAttribute xmlAttribute = xmlNode.Attributes[xmlAttributeDefinition.Attributes["name"].Value];

                if (xmlAttribute == null) {
                    if (xmlAttributeDefinition.Attributes["use"] == null || 
                        xmlAttributeDefinition.Attributes["use"].Value != "required") {
                        continue;
                    }

                    // If there is no attibute present, create one.
                    if (xmlAttribute == null) {
                        // Do not call UndoManager.BeginNewCommandRange here, because this can happen after 
                        // an undo/redo action when viewing some node. That would mess up the undo mechanism.
                        // So the adding of mandatory attributes will be added to the last undo command sequence.

                        xmlAttribute = xmlNode.OwnerDocument.CreateAttribute(xmlAttributeDefinition.Attributes["name"].Value);
                        xmlNode.Attributes.Append(xmlAttribute);
                    }                    
                }

                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));
                attrs.Add(new TypeConverterAttribute(GetAttributeTypeConverter(xmlAttributeDefinition)));

                // Get some documentation
                XmlNode deprecated = xmlAttributeDefinition.SelectSingleNode("xs:annotation/xs:appinfo/xse:deprecated", wixFiles.XsdNsmgr);
                XmlNode documentation = xmlAttributeDefinition.SelectSingleNode("xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
                if(deprecated != null || documentation != null) {
                    string docuString = "";
                    if(deprecated != null) {
                        docuString = "** DEPRECATED **\r\n";
                    }

                    if (documentation != null) {
                        docuString = documentation.InnerText;
                        docuString = docuString.Replace("\t", " ");
                        docuString = docuString.Replace("\r\n", " ");
                    }
    
                    string tmpDocuString = docuString;
                    do {
                        docuString = tmpDocuString;
                        tmpDocuString = docuString.Replace("  ", " ");
                    } while (docuString != tmpDocuString);

                    docuString = tmpDocuString;
                    docuString = docuString.Trim(' ');

                    attrs.Add(new DescriptionAttribute(docuString));
                }

                bool needsBrowse = false;
                if (xmlAttributeDefinition.Attributes["name"] != null) {
                    string attName = xmlNodeElement.Attributes["name"].Value;

                    if (xmlAttributeDefinition.Attributes["name"].Value == "SourceFile") {
                        needsBrowse = true;
                    } else if (xmlAttributeDefinition.Attributes["name"].Value == "Source") {
                        needsBrowse = true;
                    } else if (xmlAttributeDefinition.Attributes["name"].Value == "FileSource") {
                        needsBrowse = true;
                    } else if (xmlAttributeDefinition.Attributes["name"].Value == "Layout") {
                        needsBrowse = true;
                    } else if (xmlAttributeDefinition.Attributes["name"].Value == "src") {
                        needsBrowse = true;
                    }
                }

                if (needsBrowse) {
                    // We could add an UITypeEditor if desired
                    attrs.Add(new EditorAttribute(typeof(FilteredFileNameEditor),typeof(System.Drawing.Design.UITypeEditor)));
                
                    // Make Attribute array
                    Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                    // BinaryElementPropertyDescriptor also uses the src attribute, and a possibility to use relative paths.
                    FileXmlAttributePropertyDescriptor pd = new FileXmlAttributePropertyDescriptor(xmlNode, wixFiles, xmlAttributeDefinition, xmlAttributeDefinition.Attributes["name"].Value, attrArray);

                    props.Add(pd);
                } else {
                    // We could add an UITypeEditor if desired
                    // attrs.Add(new EditorAttribute(?property.EditorTypeName?, typeof(UITypeEditor)));
                    attrs.Add(new EditorAttribute(GetAttributeEditor(xmlAttributeDefinition), typeof(UITypeEditor)));
                
                    // Make Attribute array
                    Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                    // Create and add PropertyDescriptor
                    XmlAttributePropertyDescriptor pd = new XmlAttributePropertyDescriptor(wixFiles, xmlAttribute, xmlAttributeDefinition,
                        xmlAttributeDefinition.Attributes["name"].Value, attrArray);

                    props.Add(pd);
                }
            }

            // Add InnerText if required or present.
            if (
                (XmlNodeDefinition.Name == "xs:extension" &&
                    ( (xmlNode.InnerText != null && xmlNode.InnerText.Length > 0) || showInnerTextIfEmpty == true) ) ||
                (XmlNodeDefinition.Name == "xs:element" && showInnerTextIfEmpty == true) ) {
                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));
                attrs.Add(new TypeConverterAttribute(typeof(StringConverter)));
                attrs.Add(new EditorAttribute(typeof(MultiLineUITypeEditor), typeof(UITypeEditor)));
                attrs.Add(new DescriptionAttribute("InnerText of the element."));


                // Make Attribute array
                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                // Create and add PropertyDescriptor
                InnerTextPropertyDescriptor pd = new InnerTextPropertyDescriptor(wixFiles, xmlNode, attrArray);
                
                props.Add(pd);
            }

            if (editSubNodes)
            {
                XmlNodeList subNodes = xmlNode.SelectNodes("*", WixFiles.WxsNsmgr);
                if (subNodes.Count >= 1)
                {
                    ArrayList attrs = new ArrayList();

                    if (subNodes.Count == 1)
                    {
                        attrs.Add(new EditorAttribute(typeof(SearchElementTypeEditor), typeof(UITypeEditor)));
                        attrs.Add(new DescriptionAttribute("Sub Elements of the element."));
                    }

                    // Make Attribute array
                    Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                    // Create and add PropertyDescriptor
                    PropertySearchElementPropertyDescriptor pd = new PropertySearchElementPropertyDescriptor(wixFiles, xmlNode,
                        "SubElements", attrArray);

                    props.Add(pd);
                }
            }

            PropertyDescriptor[] propArray = props.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[];

            return new PropertyDescriptorCollection(propArray);
        }

        private Type GetAttributeEditor(XmlNode xmlAttributeDefinition) {
            if (xmlAttributeDefinition.Attributes["type"] == null) {
                return typeof(UITypeEditor);
            }

            switch (xmlAttributeDefinition.Attributes["type"].Value.ToLower()) {
                case "uuid":
                case "guid":
                case "autogenuuid":
                case "autogenguid":
                case "uuidorexample":
                case "componentguid":
                    return typeof(GuidUITypeEditor);
                default:
                    return typeof(UITypeEditor);
            }
        }

        private Type GetAttributeTypeConverter(XmlNode xmlAttributeDefinition) {
            if (xmlAttributeDefinition.Attributes["type"] == null) {
                return typeof(SimpleTypeConverter);
            }

            if (xmlAttributeDefinition.Attributes["name"] != null &&
                xmlAttributeDefinition.Attributes["name"].Value == "Id" &&
                xmlNodeElement.Attributes["name"].Value.EndsWith("Ref")) {
                return typeof(ReferenceConverter);
            }

            if (xmlAttributeDefinition.Attributes["name"] != null &&
                (xmlAttributeDefinition.Attributes["name"].Value == "BinaryKey" ||
                xmlAttributeDefinition.Attributes["name"].Value == "FileKey")) {
                return typeof(ReferenceConverter);
            }
                

            if (xmlAttributeDefinition.Attributes["name"] != null &&
                xmlAttributeDefinition.Attributes["name"].Value == "Type" &&
                XmlNodeElement.Attributes["name"] != null &&
                XmlNodeElement.Attributes["name"].Value == "Control") {
                return typeof(ControlTypeTypeConverter);
            }

            string name = xmlAttributeDefinition.Attributes["type"].Value;

            XmlNode xmlDefinitionRestriction = xmlNodeDefinition.SelectSingleNode(String.Format("//xs:simpleType[@name='{0}']/xs:restriction", name), wixFiles.XsdNsmgr);
            if (xmlDefinitionRestriction != null &&
                xmlDefinitionRestriction.Attributes["base"] != null &&
                xmlDefinitionRestriction.Attributes["base"].Value != null &&
                xmlDefinitionRestriction.Attributes["base"].Value.Length > 0) {
                name = xmlDefinitionRestriction.Attributes["base"].Value;
            }

            switch (name.ToLower()) {
                case "xs:string":
                    return typeof(StringConverter);
                case "xs:integer":
                case "xs:long":
                case "xs:int":
                case "xs:short":
                case "xs:byte":
                case "xs:nonnegativeinteger":
                case "xs:positiveinteger":
                case "xs:unsignedlong":
                case "xs:unsignedint":
                case "xs:unsignedshort":
                case "xs:unsignedbyte":
                case "xs:nonpositiveinteger":
                case "xs:negativeinteger":
                    return typeof(IntegerConverter);
                case "xs:datetime":
                    return typeof(DateTimeConverter);
                case "yesnotype":
                    return typeof(SimpleTypeConverter);
                default:
                    return typeof(SimpleTypeConverter);
//                    return typeof(StringConverter);
            }
        }
    }
}
