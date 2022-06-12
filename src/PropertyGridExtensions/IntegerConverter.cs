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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;

namespace WixEdit.PropertyGridExtensions {
    public class IntegerConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if ((sourceType == typeof(string))
                || (sourceType == typeof(Int64))) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
 
        public override bool CanConvertTo(ITypeDescriptorContext context, Type t) {
            if (base.CanConvertTo(context, t) || t.IsPrimitive) {
                return true;
            }

            return false;
        }
 
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is Int64)
            {
                return value;
            }
            else if (value is string)
            {
                string textValue = ((string) value).Trim();
                Int64 returnValue;
                try {
                    if (textValue[0] == '#') {
                        return Convert.ToInt64(textValue.Substring(1), 0x10);
                    }
                    if (textValue.StartsWith("0x") || 
                        textValue.StartsWith("0X") ||
                        textValue.StartsWith("&h") ||
                        textValue.StartsWith("&H")) {
                        return Convert.ToInt64(textValue.Substring(2), 0x10);
                    }
                    if (culture == null) {
                        culture = CultureInfo.CurrentCulture;
                    }
                    NumberFormatInfo formatInfo = (NumberFormatInfo) culture.GetFormat(typeof(NumberFormatInfo));
                    returnValue = Int64.Parse(textValue, NumberStyles.Integer, formatInfo);
                } catch (Exception exception) {
                    throw new Exception("Failed to ConvertFrom: " + textValue, exception);
                }

                if (IsValid(context, returnValue) == false) {
                    throw new Exception("Value is not in the valid range of numbers.");
                }

                return returnValue;
            }

            return base.ConvertFrom(context, culture, value);
        }
 
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }
            /*
            if (((destinationType == typeof(string)) && (value != null)) && typeof(Int64).IsInstanceOfType(value)) {
                if (culture == null) {
                    culture = CultureInfo.CurrentCulture;
                }
                NumberFormatInfo formatInfo = (NumberFormatInfo) culture.GetFormat(typeof(NumberFormatInfo));

                Int64 num = (Int64) value;
                return num.ToString("G", formatInfo);
            }*/
            if (destinationType.IsPrimitive) {
                return Convert.ChangeType(value, destinationType);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value) {
            if (base.IsValid (context, value) == false) {
                return false;
            }

            XmlAttributeAdapter adapter = context.Instance as XmlAttributeAdapter;
            XmlAttributePropertyDescriptor desc = context.PropertyDescriptor as XmlAttributePropertyDescriptor;

            long minVal = Int64.MinValue;
            long maxVal = Int64.MaxValue;

            XmlNode restriction = null;

            string typeString = desc.AttributeDescription.Attributes["type"].Value;
            if (typeString.StartsWith("xs:") == false) {
                XmlNode simpleType = GetSimpleType(desc.AttributeDescription, adapter.WixFiles.XsdNsmgr);

                if (simpleType != null) {
                    restriction = simpleType.SelectSingleNode("xs:restriction", adapter.WixFiles.XsdNsmgr);
                    if (IsValidAttribute(restriction.Attributes["base"])) {
                        typeString = restriction.Attributes["base"].Value;
                    }
                }
            }

            switch (typeString.ToLower()) {
                case "xs:integer":
                    break;
                case "xs:long":
                    minVal = long.MinValue;
                    maxVal = long.MaxValue;
                    break;
                case "xs:int":
                    minVal = int.MinValue;
                    maxVal = int.MaxValue;
                    break;
                case "xs:short":
                    minVal = short.MinValue;
                    maxVal = short.MaxValue;
                    break;
                case "xs:byte":
                    minVal = byte.MinValue;
                    maxVal = byte.MaxValue;
                    break;
                case "xs:nonnegativeinteger":
                case "xs:positiveInteger":
                    minVal = 0;
                    break;
                case "xs:unsignedlong":
                    minVal = 0;
                    maxVal = long.MaxValue;
                    break;
                case "xs:unsignedint":
                    minVal = 0;
                    maxVal = int.MaxValue;
                    break;
                case "xs:unsignedshort":
                    minVal = 0;
                    maxVal = short.MaxValue;
                    break;
                case "xs:unsignedbyte":
                    minVal = 0;
                    maxVal = byte.MaxValue;
                    break;
                case "xs:nonpositiveinteger":
                case "xs:negativeinteger":
                    maxVal = 0;
                    break;
                default:
                    throw new WixEditException(typeString + " is a non supported type!");

            }


            if (restriction != null) {    
                XmlNode restrictionSubNode = restriction.SelectSingleNode("xs:maxExclusive", adapter.WixFiles.XsdNsmgr);
                if (restrictionSubNode != null &&
                    IsValidAttribute(restrictionSubNode.Attributes["value"])) {
                    maxVal = Int64.Parse(restrictionSubNode.Attributes["value"].Value) - 1;
                }
    
                restrictionSubNode = restriction.SelectSingleNode("xs:maxInclusive", adapter.WixFiles.XsdNsmgr);
                if (restrictionSubNode != null &&
                    IsValidAttribute(restrictionSubNode.Attributes["value"])) {
                    maxVal = Int64.Parse(restrictionSubNode.Attributes["value"].Value);
                }
    
                restrictionSubNode = restriction.SelectSingleNode("xs:minExclusive", adapter.WixFiles.XsdNsmgr);
                if (restrictionSubNode != null &&
                    IsValidAttribute(restrictionSubNode.Attributes["value"])) {
                    minVal = Int64.Parse(restrictionSubNode.Attributes["value"].Value) + 1;
                }
    
                restrictionSubNode = restriction.SelectSingleNode("xs:minInclusive", adapter.WixFiles.XsdNsmgr);
                if (restrictionSubNode != null &&
                    IsValidAttribute(restrictionSubNode.Attributes["value"])) {
                    minVal = Int64.Parse(restrictionSubNode.Attributes["value"].Value);
                }
            }

            Int64 intValue = (Int64) value;

            if (intValue > maxVal) {
                return false;
            }

            if (intValue < minVal) {
                return false;
            }

            return true;

            /*

            enumeration
            fractionDigits
            length
            maxExclusive
            maxInclusive
            maxLength
            minExclusive
            minInclusive
            minLength
            pattern
            totalDigits
            whiteSpace 
            */
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

        private bool IsValidAttribute(XmlAttribute att) {
            if (att == null) {
                return false;
            }
            if (att.Value == null) {
                return false;
            }
            if (att.Value.Length == 0) {
                return false;
            }

            return true;
        }
    }
}