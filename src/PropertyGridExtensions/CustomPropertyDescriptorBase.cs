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
using System.ComponentModel;
using System.Reflection;
using System.Xml;

using WixEdit.Xml;

namespace WixEdit.PropertyGridExtensions {
    public abstract class CustomPropertyDescriptorBase : PropertyDescriptor {
        protected WixFiles wixFiles;
        protected PropertyInfo propertyInfo;

        public CustomPropertyDescriptorBase(WixFiles wixFiles, string name, PropertyInfo propInfo, Attribute[] attrs) : base(name, attrs) {
            propertyInfo = propInfo;
            this.wixFiles = wixFiles;
        }

        public CustomPropertyDescriptorBase(WixFiles wixFiles, string name, Attribute[] attrs) : base(name, attrs) {
            propertyInfo = null;
            this.wixFiles = wixFiles;
        }

        public override Type ComponentType {
            get {
                return AttributeArray.GetType();
            }
        }

        public override bool IsReadOnly {
            get {
                foreach (Attribute att in base.Attributes) {
                    if (att.GetType().Equals(typeof(ReadOnlyAttribute))) {
                        ReadOnlyAttribute readonlyAtt = att as ReadOnlyAttribute;
                        return readonlyAtt.IsReadOnly;
                    }
                }

                return false; 
            }
        }

        public override Type PropertyType {
            get { 
                if (propertyInfo == null) {
                    return typeof(string);
                } else {
                    return propertyInfo.PropertyType;
                }
            }
        }

        public override bool CanResetValue(object component) {
            return (GetValue(component).Equals("") == false);
        }

        public override void ResetValue(object component) {
            SetValue(component, "");
        }

        public override bool ShouldSerializeValue(object component) {
            return false;
        }
    }
}