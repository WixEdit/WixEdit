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

using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions
{
    /// <summary>
    /// Summary description for ProgressTextElementAdapter.
    /// </summary>
    public class ProgressTextElementAdapter : PropertyAdapterBase {
        protected ArrayList progressTextNodes = new ArrayList();

        public ProgressTextElementAdapter(XmlNodeList progressTextNodes, WixFiles wixFiles) : base(wixFiles) {
            foreach (object o in progressTextNodes) {
                this.progressTextNodes.Add(o);
            }
        }

        public ArrayList ProgressTextNodes {
            get {
                return progressTextNodes;
            }
            set {
                progressTextNodes = value;
            }
        }

        public override void RemoveProperty(XmlNode xmlElement) {
            if (xmlElement == null)
            {
                return;
            }

            progressTextNodes.Remove(xmlElement);
            xmlElement.ParentNode.RemoveChild(xmlElement);
        }


        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList props = new ArrayList();

            foreach(XmlNode progressTextNode in progressTextNodes) {
                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));

                // Show file name editor
                attrs.Add(new EditorAttribute(typeof(MultiLineUITypeEditor),typeof(System.Drawing.Design.UITypeEditor)));

                // Make Attribute array
                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));


                // Create and add PropertyDescriptor
                ProgressTextElementPropertyDescriptor pd = new ProgressTextElementPropertyDescriptor(wixFiles, progressTextNode, progressTextNode.Attributes["Action"].Value, attrArray);
                
                props.Add(pd);
            }

            PropertyDescriptor[] propArray = props.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[];

            return new PropertyDescriptorCollection(propArray);
        }
    }
}
