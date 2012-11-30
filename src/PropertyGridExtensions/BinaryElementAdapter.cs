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
using System.IO;
using System.Xml;

using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// Summary description for BinaryElementAdapter.
    /// </summary>
    public class BinaryElementAdapter : PropertyAdapterBase {
        protected ArrayList binaryNodes = new ArrayList();

        public BinaryElementAdapter(XmlNodeList binaryNodes, WixFiles wixFiles) : base(wixFiles) {
            foreach (object o in binaryNodes) {
                this.binaryNodes.Add(o);
            }
        }

        public ArrayList BinaryNodes {
            get {
                return binaryNodes;
            }
            set {
                binaryNodes = value;
            }
        }

        public override void RemoveProperty(XmlNode xmlElement) {
            if (xmlElement == null)
            {
                return;
            }

            XmlNode theElement = xmlElement;
            if (xmlElement is XmlAttribute) {
                XmlAttribute xmlAttribute = xmlElement as XmlAttribute;
                theElement = xmlAttribute.OwnerElement;
            }

            binaryNodes.Remove(theElement);
            theElement.ParentNode.RemoveChild(theElement);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList props = new ArrayList();

            foreach(XmlNode binaryNode in binaryNodes) {
                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));

                // Show file name editor
                attrs.Add(new EditorAttribute(typeof(FilteredFileNameEditor),typeof(System.Drawing.Design.UITypeEditor)));

                // Make Attribute array
                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));


                // Create and add PropertyDescriptor
                BinaryElementPropertyDescriptor pd = new BinaryElementPropertyDescriptor (binaryNode, WixFiles,
                    binaryNode.Attributes["Id"].Value, attrArray);
                
                props.Add(pd);
            }

            PropertyDescriptor[] propArray = props.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[];

            return new PropertyDescriptorCollection(propArray);
        }
    }
}
