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
using System.Text;

namespace WixEdit.Xml
{
    /// <summary>
    /// Summary description for XPathHelper.
    /// </summary>
    public class XPathHelper
    {
        /// <summary>
        /// Escaping for something like:
        /// <code>What's "Foo Bar Baz"</code>
        /// So that would become
        /// <code>concat('', 'What', "'", 's "Foo Bar Baz")</code>
        /// suitable for a xpath query.
        /// </summary>
        /// <param name="input">Raw search string</param>
        /// <returns>With concat constructed search string</returns>
        public static string EscapeXPathInputString(string input)
        {
            string[] components = input.Split('\'');

            StringBuilder result = new StringBuilder();
            result.Append("concat(''");
            for (int i = 0; i < components.Length; i++)
            {
                result.AppendFormat(", '{0}'", components[i]);
                if (i < components.Length - 1)
                {
                    result.Append(", \"'\"");
                }
            }
            result.Append(")");

            return result.ToString();
        }
    }
}
