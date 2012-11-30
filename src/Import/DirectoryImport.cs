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

using WixEdit.Settings;
using WixEdit.Xml;
using WixEdit.Helpers;
using WixEdit.Images;

namespace WixEdit.Import
{
    /// <summary>
    /// Summary description for DirectoryImport.
    /// </summary>
    public class DirectoryImport
    {
        WixFiles wixFiles;
        string[] folders;
        XmlNode directoryElement;
        TreeNode firstShowableNode;

        string ShortName;
        string LongName;

        public DirectoryImport(WixFiles wixFiles, string[] folders, XmlNode directoryElement)
        {
            this.wixFiles = wixFiles;
            this.folders = folders;
            this.directoryElement = directoryElement;

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
            RecurseDirectories(folders, treeNode, directoryElement);

            if (firstShowableNode != null && firstShowableNode.TreeView != null)
            {
                firstShowableNode.TreeView.SelectedNode = firstShowableNode;
            }
        }

        private bool NeedToIgnore(string fileOrDir)
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

        private void RecurseDirectories(string[] subFolders, TreeNode treeNode, XmlNode parentDirectoryElement)
        {
            foreach (string folder in subFolders)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folder);

                if (NeedToIgnore(dirInfo.Name))
                {
                    continue;
                }

                XmlElement newElement = parentDirectoryElement.OwnerDocument.CreateElement("Directory", WixFiles.WixNamespaceUri);

                newElement.SetAttribute("Id", FileImport.GenerateValidIdentifier(dirInfo.Name, newElement, wixFiles));

                newElement.SetAttribute(LongName, FileImport.GenerateValidLongName(dirInfo.Name));
                if (WixEditSettings.Instance.IsUsingWix2())
                {
                    newElement.SetAttribute(ShortName, FileImport.GenerateValidShortName(PathHelper.GetShortDirectoryName(dirInfo, wixFiles, parentDirectoryElement)));
                }

                TreeNode newNode = new TreeNode(newElement.GetAttribute(LongName));
                newNode.Tag = newElement;

                if (firstShowableNode == null)
                {
                    firstShowableNode = newNode;
                }

                int imageIndex = ImageListFactory.GetImageIndex("Directory");
                if (imageIndex >= 0)
                {
                    newNode.ImageIndex = imageIndex;
                    newNode.SelectedImageIndex = imageIndex;
                }

                XmlNodeList sameNodes = parentDirectoryElement.SelectNodes("wix:Directory", wixFiles.WxsNsmgr);
                if (sameNodes.Count > 0)
                {
                    parentDirectoryElement.InsertAfter(newElement, sameNodes[sameNodes.Count - 1]);
                }
                else
                {
                    parentDirectoryElement.AppendChild(newElement);
                }

                treeNode.Nodes.Add(newNode);

                string[] subFiles = Directory.GetFiles(dirInfo.FullName);
                if (subFiles.Length > 0)
                {
                    AddFiles(subFiles, newNode, newElement, dirInfo);
                }

                string[] subSubFolders = Directory.GetDirectories(dirInfo.FullName);
                RecurseDirectories(subSubFolders, newNode, newElement);
            }
        }

        private void AddFiles(string[] files, TreeNode treeNode, XmlNode parentDirectoryElement, DirectoryInfo dirInfo)
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
                newFileElement.SetAttribute(LongName, FileImport.GenerateValidLongName(fileInfo.Name));
                if (WixEditSettings.Instance.IsUsingWix2())
                {
                    newFileElement.SetAttribute(ShortName, FileImport.GenerateValidShortName(PathHelper.GetShortFileName(fileInfo, wixFiles, newComponentElement)));
                }
                newFileElement.SetAttribute("Source", PathHelper.GetRelativePath(fileInfo.FullName, wixFiles));

                TreeNode newFileNode = new TreeNode(newFileElement.GetAttribute(LongName));
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
    }
}
