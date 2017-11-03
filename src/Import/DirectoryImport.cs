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

        public DirectoryImport(WixFiles wixFiles, string[] folders, XmlNode directoryElement)
        {
            this.wixFiles = wixFiles;
            this.folders = folders;
            this.directoryElement = directoryElement;
        }

        public void Import(TreeNode treeNode)
        {
            RecurseDirectories(folders, treeNode, directoryElement);

            if (firstShowableNode != null && firstShowableNode.TreeView != null)
            {
                firstShowableNode.TreeView.SelectedNode = firstShowableNode;
            }
        }

        private void RecurseDirectories(string[] subFolders, TreeNode treeNode, XmlNode parentDirectoryElement)
        {
            foreach (string folder in subFolders)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folder);

                if (FileImport.NeedToIgnore(dirInfo.Name))
                {
                    continue;
                }

                XmlElement newElement = parentDirectoryElement.OwnerDocument.CreateElement("Directory", WixFiles.WixNamespaceUri);

                newElement.SetAttribute("Id", FileImport.GenerateValidIdentifier(dirInfo.Name, newElement, wixFiles));

                newElement.SetAttribute(WixEditSettings.Instance.LongName, FileImport.GenerateValidLongName(dirInfo.Name));
                if (WixEditSettings.Instance.IsUsingWix2())
                {
                    newElement.SetAttribute(WixEditSettings.Instance.ShortName, FileImport.GenerateValidShortName(PathHelper.GetShortDirectoryName(dirInfo, wixFiles, parentDirectoryElement)));
                }

                TreeNode newNode = new TreeNode(newElement.GetAttribute(WixEditSettings.Instance.LongName));
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
                    FileImport.AddFiles(wixFiles, subFiles, newNode, newElement);
                }

                string[] subSubFolders = Directory.GetDirectories(dirInfo.FullName);
                RecurseDirectories(subSubFolders, newNode, newElement);
            }
        }
    }
}
