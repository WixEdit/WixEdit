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

using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// Summary description for PropertyElementAdapter.
    /// </summary>
    public class PropertyElementAdapter : PropertyAdapterBase {
        protected ArrayList propertyNodes = new ArrayList();

        public PropertyElementAdapter(XmlNodeList propertyNodes, WixFiles wixFiles) : base(wixFiles) {
            foreach (object o in propertyNodes) {
                this.propertyNodes.Add(o);
            }
        }

        public ArrayList PropertyNodes {
            get {
                return propertyNodes;
            }
            set {
                propertyNodes = value;
            }
        }

        public override void RemoveProperty(XmlNode xmlElement) {
            if (xmlElement == null)
            {
                return;
            }

            propertyNodes.Remove(xmlElement);
            xmlElement.ParentNode.RemoveChild(xmlElement);
        }


        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList props = new ArrayList();

            foreach(XmlNode propertyNode in propertyNodes) {
                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));
                attrs.Add(new TypeConverterAttribute(typeof(StringConverter)));

                XmlNodeList subNodes = propertyNode.SelectNodes("*", WixFiles.WxsNsmgr);
                if (subNodes.Count >= 1) {
                    if (subNodes.Count == 1) {
                        attrs.Add(new EditorAttribute(typeof(SearchElementTypeEditor), typeof(UITypeEditor)));
                    }
        
                    // Make Attribute array
                    Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                    // Create and add PropertyDescriptor
                    PropertySearchElementPropertyDescriptor pd = new PropertySearchElementPropertyDescriptor (wixFiles, propertyNode,
                        propertyNode.Attributes["Id"].Value, attrArray);
                    
                    props.Add(pd);
                } else {
                    // Make Attribute array
                    Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                    // Create and add PropertyDescriptor
                    PropertyElementPropertyDescriptor pd = 
                        new PropertyElementPropertyDescriptor (wixFiles, propertyNode,
                            propertyNode.Attributes["Id"].Value, attrArray);                    
                    props.Add(pd);
                }
            }

            PropertyDescriptor[] propArray = props.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[];

            return new PropertyDescriptorCollection(propArray);
        }
    }
}
