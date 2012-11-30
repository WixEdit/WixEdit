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
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WixEdit.Xml;
using WixEdit.Images;

namespace WixEdit.Import
{
    /// <summary>
    /// Summary description for RegistryImport.
    /// </summary>
    public class RegistryImport
    {
        WixFiles wixFiles;
        FileInfo regFileInfo;
        XmlNode componentElement;
        int lineNumber;

        string registryKeyElementName;
        string registryValueElementName;

        public RegistryImport(WixFiles wixFiles, FileInfo regFileInfo, XmlNode componentElement)
        {
            this.wixFiles = wixFiles;
            this.regFileInfo = regFileInfo;
            this.componentElement = componentElement;

            if (WixEdit.Settings.WixEditSettings.Instance.IsUsingWix2())
            {
                registryKeyElementName = "Registry";
                registryValueElementName = "Registry";
            }
            else
            {
                registryKeyElementName = "RegistryKey";
                registryValueElementName = "RegistryValue";
            }
        }

        public void Import(TreeNode treeNode)
        {
            TextReader reader = new StreamReader(regFileInfo.FullName);

            string line = reader.ReadLine();
            lineNumber = 1;
            string trimmedLine = "";

            ArrayList createdChildren = new ArrayList();

            bool currentKeyUsed = false;
            string currentRoot = null;
            string currentKey = null;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                trimmedLine = line.Trim();

                if (trimmedLine == "")
                {
                    continue;
                }
                else if (trimmedLine.StartsWith("["))
                {
                    if (trimmedLine.EndsWith("]") == false ||
                        trimmedLine.IndexOf("\\") < 0)
                    {
                        throw new ImportException(String.Format("Invalid line (Line {0}): \"{1}\"", lineNumber, trimmedLine));
                    }

                    if (currentKeyUsed == false && currentKey != null && currentRoot != null)
                    {
                        XmlElement registryKey = componentElement.OwnerDocument.CreateElement(registryKeyElementName, WixFiles.WixNamespaceUri);
                        registryKey.SetAttribute("Root", currentRoot);
                        registryKey.SetAttribute("Key", currentKey);

                        createdChildren.Add(registryKey);
                    }

                    currentKey = null;
                    currentRoot = null;
                    currentKeyUsed = false;

                    string fullRegKey = line.Substring(1, line.Length - 2);
                    int rootSeparatePos = fullRegKey.IndexOf("\\");
                    string fullRoot = fullRegKey.Substring(0, rootSeparatePos);
                    string root = GetRootString(fullRoot);
                    if (root == null || fullRegKey.Length - rootSeparatePos - 1 <= 0)
                    {
                        throw new ImportException(String.Format("Invalid line (Line {0}): \"{1}\"", lineNumber, trimmedLine));
                    }
                    string restKey = fullRegKey.Substring(rootSeparatePos + 1, fullRegKey.Length - rootSeparatePos - 1);

                    currentRoot = root;
                    currentKey = restKey;
                }
                else if (trimmedLine.StartsWith("\"") || trimmedLine.StartsWith("@"))
                {
                    if ((trimmedLine.IndexOf("@") != 0 && trimmedLine.IndexOf("\"", 1) < 0) ||
                        trimmedLine.IndexOf("=") < 0)
                    {
                        throw new ImportException(String.Format("Invalid line (Line {0}): \"{1}\"", lineNumber, trimmedLine));
                    }
                    else if (currentRoot == null || currentKey == null)
                    {
                        throw new ImportException(String.Format("Invalid line (Line {0}), missing key specification: \"{1}\"", lineNumber, trimmedLine));
                    }

                    string nextLine = null;
                    while (trimmedLine.EndsWith("\\"))
                    {
                        trimmedLine = trimmedLine.Remove(trimmedLine.Length - 1, 1);

                        nextLine = reader.ReadLine();
                        lineNumber++;
                        if (nextLine == null)
                        {
                            throw new ImportException(String.Format("Invalid line (Line {0}), missing part of value: \"{1}\"", lineNumber, trimmedLine));
                        }

                        trimmedLine += nextLine.Trim();
                    }

                    int equalsPos = trimmedLine.IndexOf("=");
                    string nameString = trimmedLine.Substring(0, equalsPos).Trim();
                    string valueString = trimmedLine.Substring(equalsPos + 1).Trim();

                    string currentName = GetNameString(nameString);
                    string currentType = GetTypeString(valueString);

                    XmlElement registryKey = componentElement.OwnerDocument.CreateElement(registryValueElementName, WixFiles.WixNamespaceUri);
                    if (currentName != null && currentName != "")
                    {
                        registryKey.SetAttribute("Name", currentName);
                    }
                    registryKey.SetAttribute("Root", currentRoot);
                    registryKey.SetAttribute("Key", currentKey);
                    registryKey.SetAttribute("Type", currentType);

                    SetValue(registryKey, currentType, valueString);

                    createdChildren.Add(registryKey);

                    currentKeyUsed = true;
                }
                else
                {
                    throw new ImportException(String.Format("Invalid line (Line {0}): \"{1}\"", lineNumber, trimmedLine));
                }
            }

            if (currentKeyUsed == false)
            {
                XmlElement registryKey = componentElement.OwnerDocument.CreateElement(registryKeyElementName, WixFiles.WixNamespaceUri);
                registryKey.SetAttribute("Root", currentRoot);
                registryKey.SetAttribute("Key", currentKey);

                createdChildren.Add(registryKey);
            }

            foreach (XmlNode child in createdChildren)
            {
                componentElement.AppendChild(child);

                string displayName = child.Name;
                string root = child.Attributes["Root"].Value;
                string key = child.Attributes["Key"].Value;
                XmlAttribute name = child.Attributes["Name"];
                if (name != null)
                {
                    if (key.EndsWith("\\") == false)
                    {
                        key = key + "\\";
                    }
                    key = key + "@" + name.Value;
                }

                displayName = root + "\\" + key;

                int imageIndex = ImageListFactory.GetImageIndex(child.Name);
                TreeNode newNode = new TreeNode(displayName, imageIndex, imageIndex);
                newNode.Tag = child;

                treeNode.Nodes.Add(newNode);
            }
        }

        private string GetRootString(string fullRoot)
        {
            string ret = null;
            switch (fullRoot)
            {
                case "HKEY_CLASSES_ROOT":
                    ret = "HKCR";
                    break;
                case "HKEY_CURRENT_USER":
                    ret = "HKCU";
                    break;
                case "HKEY_LOCAL_MACHINE":
                    ret = "HKLM";
                    break;
                case "HKEY_USERS":
                    ret = "HKU";
                    break;
            }

            // HKMU not supported.

            return ret;
        }

        private string GetNameString(string rawNameString)
        {
            if (rawNameString == "@")
            {
                return "";
            }

            return rawNameString.Substring(1, rawNameString.Length - 2);
        }

        private string GetTypeString(string rawValueString)
        {
            if (rawValueString.StartsWith("\"") &&
                rawValueString.EndsWith("\""))
            {
                return "string";
            }
            else if (rawValueString.StartsWith("dword"))
            {
                return "integer";
            }
            else if (rawValueString.StartsWith("hex"))
            {
                string rest = rawValueString.Remove(0, 3).Trim();
                rest = rest.Replace(" ", "");

                if (rest.StartsWith(":"))
                {
                    return "binary";
                }
                else if (rest.StartsWith("(2):"))
                {
                    return "expandable";
                }
                else if (rest.StartsWith("(7):"))
                {
                    return "multiString";
                }
                else
                {
                    throw new ImportException(String.Format("Invalid hex specification (Line {0}): \"{1}\"", lineNumber, rawValueString));
                }
            }
            else
            {
                throw new ImportException(String.Format("Invalid value (Line {0}): \"{1}\"", lineNumber, rawValueString));
            }
        }

        private void SetValue(XmlElement registryKey, string currentType, string valueString)
        {
            string ret = "";
            int valStart = valueString.IndexOf(":") + 1;

            switch (currentType)
            {
                case "string":
                    ret = valueString.Substring(1, valueString.Length - 2);
                    break;
                case "integer":
                    string intStr = valueString.Substring(valStart, valueString.Length - valStart).Trim();
                    try
                    {
                        ret = Int32.Parse(intStr, System.Globalization.NumberStyles.HexNumber).ToString();
                    }
                    catch (FormatException ex)
                    {
                        throw new ImportException(String.Format("Failed to parse dword value (Line {0}): {1}", lineNumber, intStr), ex);
                    }
                    break;
                case "binary":
                    ret = valueString.Substring(valStart, valueString.Length - valStart).Trim();
                    ret = ret.Replace(",", "");
                    ret = ret.Replace(" ", "");
                    break;
                case "expandable":
                    ret = ret.Replace(" ", "");
                    ret = valueString.Substring(valStart, valueString.Length - valStart).Trim();

                    ret = GetStringFromBinary(ret);

                    ret = ret.Trim('\0');
                    break;
                case "multiString":
                    ret = ret.Replace(" ", "");
                    ret = valueString.Substring(valStart, valueString.Length - valStart).Trim();

                    ret = GetStringFromBinary(ret);
                    if (ret.EndsWith("\0"))
                    {
                        ret = ret.Remove(ret.Length - 1, 1);
                    }

                    break;
            }

            if (ret.IndexOf("\0") >= 0)
            {
                string[] retStrings = ret.Split('\0');
                foreach (string retString in retStrings)
                {
                    XmlElement registryValue = registryKey.OwnerDocument.CreateElement("RegistryValue", WixFiles.WixNamespaceUri);
                    registryValue.InnerText = retString;

                    registryKey.AppendChild(registryValue);
                }
            }
            else
            {
                registryKey.SetAttribute("Value", ret);
            }
        }

        private string GetStringFromBinary(string binaryString)
        {
            string[] stringBytes = binaryString.Trim(',').Split(',');
            byte[] bytes = new byte[stringBytes.Length];

            try
            {
                for (int i = 0; i < stringBytes.Length; i++)
                {
                    bytes[i] = Byte.Parse(stringBytes[i], System.Globalization.NumberStyles.HexNumber);
                }
            }
            catch (FormatException ex)
            {
                throw new ImportException(String.Format("Failed to parse binary value (Line {0}): {1}", lineNumber, binaryString), ex);
            }

            char[] unicodeChars = Encoding.Unicode.GetChars(bytes, 0, bytes.Length);

            return new string(unicodeChars);
        }
    }
}
