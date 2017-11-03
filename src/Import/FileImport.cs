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
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Forms;
using System.Xml.XPath;

using WixEdit.Settings;
using WixEdit.Xml;
using WixEdit.Helpers;
using WixEdit.Images;

namespace WixEdit.Import
{
    /// <summary>
    /// Summary description for FileImport.
    /// </summary>
    public class FileImport
    {
        WixFiles wixFiles;
        FileInfo fileInfo;
        XmlNode componentElement;

        string ShortName;
        string LongName;

        public FileImport(WixFiles wixFiles, FileInfo fileInfo, XmlNode componentElement)
        {
            this.wixFiles = wixFiles;
            this.fileInfo = fileInfo;
            this.componentElement = componentElement;

            if (WixEditSettings.Instance.IsUsingWix2())
            {
                this.ShortName = "Name";
                this.LongName = "LongName";
            }
            else
            {
                this.ShortName = "ShortName";
                this.LongName = "Name";
            }
        }

        public void Import(TreeNode treeNode)
        {
            XmlElement newElement = componentElement.OwnerDocument.CreateElement("File", WixFiles.WixNamespaceUri);

            newElement.SetAttribute("Id", GenerateValidIdentifier(fileInfo.Name, newElement, wixFiles));
            newElement.SetAttribute(LongName, GenerateValidLongName(fileInfo.Name));
            if (WixEditSettings.Instance.IsUsingWix2())
            {
                newElement.SetAttribute(ShortName, GenerateValidShortName(PathHelper.GetShortFileName(fileInfo, wixFiles, componentElement)));
            }
            newElement.SetAttribute("Source", PathHelper.GetRelativePath(fileInfo.FullName, wixFiles));

            TreeNode newNode = new TreeNode(newElement.GetAttribute(LongName));
            newNode.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex("File");
            if (imageIndex >= 0)
            {
                newNode.ImageIndex = imageIndex;
                newNode.SelectedImageIndex = imageIndex;
            }

            XmlNodeList sameNodes = componentElement.SelectNodes("wix:File", wixFiles.WxsNsmgr);
            if (sameNodes.Count > 0)
            {
                componentElement.InsertAfter(newElement, sameNodes[sameNodes.Count - 1]);
            }
            else
            {
                componentElement.AppendChild(newElement);
            }

            treeNode.Nodes.Add(newNode);
        }

        /// <summary>
        /// Identifier's may contain ASCII characters A-Z, a-z, digits, underscores (_), 
        /// or periods (.). Every identifier must begin with either a letter or an underscore.
        /// </summary>
        /// <remarks>http://msdn.microsoft.com/library/en-us/msi/setup/identifier.asp</remarks>
        /// <param name="filename">Name of file to generate Id from</param>
        /// <param name="wixDocument">The xmldocument containing the file or directory.</param>
        /// <returns>A valid Id.</returns>
        public static string GenerateValidIdentifier(string filename, XmlElement newElement, WixFiles wixFiles)
        {
            string newId = filename.ToUpper();
            newId = Regex.Replace(newId, "[^A-Z0-9_.]", "_");

            if (Regex.Match(newId, "^[A-Z_]").Length == 0)
            {
                newId = "_" + newId;
            }

            XmlDocument owner = newElement.OwnerDocument;

            // Make sure you search case insensitive.
            string searchTerm1 = string.Format(
                "//wix:{0}[translate(@Id, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='{1}']",
                newElement.Name,
                newId);
            if (owner.SelectSingleNode(searchTerm1, wixFiles.WxsNsmgr) == null)
            {
                return newId;
            }

            // Make sure you search case insensitive.
            string searchTerm2 = string.Format(
                "//wix:{0}[translate(@Id, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='{1}_{{0}}']",
                newElement.Name,
                newId);

            int index = 1;
            while (owner.SelectSingleNode(String.Format(searchTerm2, index), wixFiles.WxsNsmgr) != null)
            {
                index++;
            }

            return newId + "_" + index;
        }

        /// <summary>
        /// Type LongFileNameType in Wix.xsd:
        /// Values of this type will look like: "Long File Name.extension".  
        /// The following characters are not allowed: \ ? | > : / * " or less-than.  
        /// The name must be shorter than 260 characters.  The value could 
        /// also be a localization variable with the format $(loc.VARIABLE).
        /// </summary>
        /// <remarks>Hardcoded, because it't not possible to read this from the Wix.xsd</remarks>
        /// <param name="filename">Name of file to generate Id from</param>
        /// <returns>A valid Id.</returns>
        public static string GenerateValidLongName(string filename)
        {
            string newId = filename;
            newId = Regex.Replace(newId, "[\\?|><:/*\"]", "_");

            if (Regex.Match(newId, "~[0-9]").Length > 0)
            {
                newId = newId.Replace("~", "_");
            }

            return newId;
        }

        /// <summary>
        /// Type ShortFileNameType in Wix.xsd:
        /// Values of this type will look like: "FileName.ext".  
        /// The following characters are not allowed: \ ? | > : / * " + , ; = [ ] less-than, or whitespace.  
        /// The name cannot be longer than 8 characters and the extension cannot exceed 3 characters.  
        /// The value could also be a localization variable with the format $(loc.VARIABLE).
        /// </summary>
        /// <remarks>Hardcoded, because it't not possible to read this from the Wix.xsd</remarks>
        /// <param name="filename">Name of file to generate Id from</param>
        /// <returns>A valid Id.</returns>
        public static string GenerateValidShortName(string filename)
        {
            string newId = GenerateValidLongName(filename);
            newId = Regex.Replace(newId, "[\\+,;=\\[\\] ]", "_");

            if (Regex.Match(newId, "~[0-9]").Length > 0)
            {
                newId = newId.Replace("~", "_");
            }

            return newId;
        }

        public static void AddFiles(WixFiles wixFiles, string[] files, TreeNode treeNode, XmlNode parentDirectoryElement)
        {
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                if (NeedToIgnore(fileInfo.Name))
                {
                    continue;
                }

                XmlElement newComponentElement = parentDirectoryElement.OwnerDocument.CreateElement("Component", WixFiles.WixNamespaceUri);

                newComponentElement.SetAttribute("Id", FileImport.GenerateValidIdentifier(fileInfo.Name, newComponentElement, wixFiles));
                newComponentElement.SetAttribute("DiskId", "1");
                newComponentElement.SetAttribute("Guid", Guid.NewGuid().ToString().ToUpper());

                parentDirectoryElement.AppendChild(newComponentElement);

                TreeNode newComponentNode = new TreeNode(newComponentElement.GetAttribute("Id"));
                newComponentNode.Tag = newComponentElement;

                int imageIndex = ImageListFactory.GetImageIndex("Component");
                if (imageIndex >= 0)
                {
                    newComponentNode.ImageIndex = imageIndex;
                    newComponentNode.SelectedImageIndex = imageIndex;
                }

                treeNode.Nodes.Add(newComponentNode);

                XmlElement newFileElement = parentDirectoryElement.OwnerDocument.CreateElement("File", WixFiles.WixNamespaceUri);

                newFileElement.SetAttribute("Id", FileImport.GenerateValidIdentifier(fileInfo.Name, newFileElement, wixFiles));
                newFileElement.SetAttribute(WixEditSettings.Instance.LongName, FileImport.GenerateValidLongName(fileInfo.Name));
                if (WixEditSettings.Instance.IsUsingWix2())
                {
                    newFileElement.SetAttribute(WixEditSettings.Instance.ShortName, FileImport.GenerateValidShortName(PathHelper.GetShortFileName(fileInfo, wixFiles, newComponentElement)));
                }
                newFileElement.SetAttribute("Source", PathHelper.GetRelativePath(fileInfo.FullName, wixFiles));

                TreeNode newFileNode = new TreeNode(newFileElement.GetAttribute(WixEditSettings.Instance.LongName));
                newFileNode.Tag = newFileElement;

                imageIndex = ImageListFactory.GetImageIndex("File");
                if (imageIndex >= 0)
                {
                    newFileNode.ImageIndex = imageIndex;
                    newFileNode.SelectedImageIndex = imageIndex;
                }

                XmlNodeList sameNodes = newComponentElement.SelectNodes("wix:File", wixFiles.WxsNsmgr);
                if (sameNodes.Count > 0)
                {
                    newComponentElement.InsertAfter(newFileElement, sameNodes[sameNodes.Count - 1]);
                }
                else
                {
                    newComponentElement.AppendChild(newFileElement);
                }

                newComponentNode.Nodes.Add(newFileNode);
            }
        }

        public static bool NeedToIgnore(string fileOrDir)
        {
            bool ignoreThis = false;
            foreach (string test in WixEdit.Settings.WixEditSettings.Instance.IgnoreFilesAndDirectories)
            {
                string escapedTest = Regex.Escape(test);
                escapedTest = escapedTest.Replace("\\*", ".*");
                escapedTest = String.Format("^{0}$", escapedTest);
                if (Regex.IsMatch(fileOrDir, escapedTest, RegexOptions.IgnoreCase))
                {
                    ignoreThis = true;
                    break;
                }
            }

            return ignoreThis;
        }
    }
}
