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

namespace WixEdit.Xml
{
    /// <summary>
    /// Class to contain logic to find and/or create elements.
    /// </summary>
    public class ElementLocator
    {
        public static XmlNode GetUIElement(WixFiles wixFiles)
        {
            XmlNode ui = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*/wix:UI", wixFiles.WxsNsmgr);
            if (ui == null)
            {
                XmlNodeList parents = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*", wixFiles.WxsNsmgr);
                if (parents.Count == 0)
                {
                    return null;
                }

                XmlNode theParent = null;
                foreach (XmlNode possibleParent in parents)
                {
                    XmlNode def = wixFiles.XsdDocument.SelectSingleNode(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType/xs:sequence//xs:element[@ref='UI']", possibleParent.Name), wixFiles.XsdNsmgr);
                    if (def != null)
                    {
                        theParent = possibleParent;
                        break;
                    }
                }

                if (theParent == null)
                {
                    return null;
                }

                ui = wixFiles.WxsDocument.CreateElement("UI", WixFiles.WixNamespaceUri);

                theParent.AppendChild(ui);
            }

            return ui;
        }
    }
}
