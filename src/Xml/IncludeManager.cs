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
using System.Windows.Forms;
using System.Xml;

using WixEdit.Settings;

namespace WixEdit.Xml
{
    public class IncludeManager
    {
        WixFiles wixFiles;
        XmlDocument wxsDocument;
        bool hasIncludes = false;

        Hashtable fileToNodesMap = new Hashtable();
        Hashtable xpathToNodesArrayMap = new Hashtable();
        Hashtable nodesToFileMap = new Hashtable();

        public IncludeManager(WixFiles wixFiles, XmlDocument wxsDocument)
        {
            this.wixFiles = wixFiles;
            this.wxsDocument = wxsDocument;

            LoadIncludes();
        }

        public bool HasIncludes
        {
            get
            {
                return hasIncludes;
            }
        }

        private void LoadIncludes()
        {
            fileToNodesMap = new Hashtable();
            xpathToNodesArrayMap = new Hashtable();
            nodesToFileMap = new Hashtable();

            XmlNodeList includes = wxsDocument.SelectNodes("//processing-instruction('include')");
            if (includes.Count == 0)
            {
                return;
            }

            try
            {
                // Verify valid xml
                Hashtable includeDoms = new Hashtable();
                foreach (XmlProcessingInstruction include in includes)
                {
                    string data = include.Data.Trim('"', '\r', '\n', ' ');
                    if (Path.IsPathRooted(data) == false)
                    {
                        data = Path.Combine(wixFiles.WxsDirectory.FullName, data);
                    }

                    FileInfo file = new FileInfo(data);
                    if (file.Exists == false)
                    {
                        return;
                    }

                    XmlDocument includeDocument = new XmlDocument();
                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        includeDocument.Load(fs);
                        fs.Close();
                    }

                    includeDoms.Add(include, includeDocument);
                }

                foreach (DictionaryEntry entry in includeDoms)
                {
                    XmlProcessingInstruction include = entry.Key as XmlProcessingInstruction;
                    XmlDocument includeDocument = entry.Value as XmlDocument;

                    // We have to set the Wix namespace 
                    includeDocument.DocumentElement.SetAttribute("xmlns", WixFiles.WixNamespaceUri);
                    includeDocument.LoadXml(includeDocument.OuterXml);

                    ArrayList nodesArray = new ArrayList();

                    XmlNode includeNode = wxsDocument.ImportNode(includeDocument.DocumentElement, true);
                    while (includeNode.FirstChild != null)
                    {
                        nodesArray.Add(includeNode.FirstChild);
                        include.ParentNode.AppendChild(includeNode.FirstChild);
                    }

                    if (nodesArray.Count > 0)
                    {
                        string xpath = GetXPath(include.ParentNode);

                        // 1 xpath can have multiple files (== Multiple includes can be done in one element)
                        ArrayList nodesArrayList = null;
                        if (xpathToNodesArrayMap.Contains(xpath))
                        {
                            nodesArrayList = xpathToNodesArrayMap[xpath] as ArrayList;
                        }
                        else
                        {
                            nodesArrayList = new ArrayList();
                            xpathToNodesArrayMap.Add(xpath, nodesArrayList);
                        }
                        nodesArrayList.Add(nodesArray);
                        nodesToFileMap.Add(nodesArray, include.Data.Trim('"', '\r', '\n', ' '));
                        fileToNodesMap.Add(include, nodesArray);
                    }
                }

                hasIncludes = true;
            }
            catch (Exception ex)
            {
                throw new WixEditException("Importing of include files failed!!!", ex);
            }
        }

        public void SaveIncludes(ArrayList changedIncludeFiles)
        {
            foreach (DictionaryEntry entry in fileToNodesMap)
            {
                XmlProcessingInstruction include = entry.Key as XmlProcessingInstruction;
                string file = include.Data.Trim('"', '\r', '\n', ' ');
                ArrayList nodes = entry.Value as ArrayList;

                if (changedIncludeFiles.Contains(file) == false)
                {
                    continue;
                }

                XmlDocument includeDoc = new XmlDocument();
                XmlElement includeElement = includeDoc.CreateElement("Include", WixFiles.WixNamespaceUri);
                includeDoc.AppendChild(includeElement);

                ArrayList obsolete = new ArrayList();
                foreach (XmlNode node in nodes)
                {
                    XmlNode imported = includeDoc.ImportNode(node, true);
                    includeElement.AppendChild(imported);
                }

                if (Path.IsPathRooted(file) == false)
                {
                    file = Path.Combine(wixFiles.WxsDirectory.FullName, file);
                }

                FileInfo fileInfo = new FileInfo(file);
                FileInfo bakFileInfo = new FileInfo(file + ".wixedit.original");
                if (fileInfo.Exists)
                {
                    if (WixEditSettings.Instance.BackupChangedIncludes && bakFileInfo.Exists == false)
                    {
                        fileInfo.CopyTo(bakFileInfo.FullName);
                    }

                    UnauthorizedAccessException theEx = null;
                    do
                    {
                        try
                        {
                            includeDoc.Save(fileInfo.FullName);
                            break;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            theEx = ex;
                        }
                    } while (DialogResult.Retry == MessageBox.Show("Failed to save include file. " + theEx.Message, "Failed to save include file", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning));
                }
            }
        }

        public void RemoveIncludes()
        {
            foreach (DictionaryEntry entry in fileToNodesMap)
            {
                ArrayList nodes = entry.Value as ArrayList;

                ArrayList obsolete = new ArrayList();
                foreach (XmlNode node in nodes)
                {
                    if (node.ParentNode != null)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                    else
                    {
                        // Remove node from nodes
                        obsolete.Add(node);
                    }
                }

                foreach (XmlNode node in obsolete)
                {
                    nodes.Remove(node);
                }
            }
        }

        public void RestoreIncludes()
        {
            foreach (DictionaryEntry entry in fileToNodesMap)
            {
                XmlProcessingInstruction include = entry.Key as XmlProcessingInstruction;
                string file = include.Data.Trim('"', '\r', '\n', ' ');
                ArrayList nodes = entry.Value as ArrayList;

                foreach (XmlNode node in nodes)
                {
                    include.ParentNode.AppendChild(node);
                }
            }
        }

        public string FindIncludeFile(XmlNode node)
        {
            if (hasIncludes == false || node == null)
            {
                return null;
            }

            XmlNode element = GetElement(node);
            string xpath = GetXPath(element);

            int last = xpath.LastIndexOf("/");
            if (last <= 0)
            {
                return null;
            }

            if (xpathToNodesArrayMap.Contains(xpath) == true)
            {
                // this means the changed node is a parent of our include, so just return null;
                return null;
            }

            // The xpathToNodesArrayMap contains xpaths of parents, so make the parent xpath
            xpath = xpath.Remove(last, xpath.Length - last);

            while ((last = xpath.LastIndexOf("/")) > 0)
            {
                if (xpathToNodesArrayMap.Contains(xpath))
                {
                    ArrayList nodesArrayList = xpathToNodesArrayMap[xpath] as ArrayList;
                    foreach (ArrayList nodes in nodesArrayList)
                    {
                        foreach (XmlNode aNode in nodes)
                        {
                            if (aNode.Equals(element))
                            {
                                return nodesToFileMap[nodes] as string;
                            }
                        }
                    }
                }

                xpath = xpath.Remove(last, xpath.Length - last);
                element = element.ParentNode;
            }

            return null;
        }

        private XmlElement GetElement(XmlNode node)
        {
            XmlNode theNode = node;
            while (theNode != null && theNode.NodeType != XmlNodeType.Element)
            {
                if (theNode.NodeType == XmlNodeType.Attribute)
                {
                    theNode = ((XmlAttribute)theNode).OwnerElement;
                }
                else
                {
                    theNode = theNode.ParentNode;
                }
            }

            return (XmlElement)theNode;
        }

        private string GetXPath(XmlNode node)
        {
            ArrayList names = new ArrayList();
            XmlNode theNode = node;
            while (theNode != null)
            {
                if (theNode.NodeType == XmlNodeType.Document)
                {
                    break;
                }

                if (theNode.NodeType == XmlNodeType.Attribute)
                {
                    theNode = ((XmlAttribute)theNode).OwnerElement;
                }
                else
                {
                    if (theNode.NodeType == XmlNodeType.Element)
                    {
                        names.Insert(0, theNode.Name);
                    }
                    theNode = theNode.ParentNode;
                }
            }

            string result = String.Join("/", names.ToArray(typeof(string)) as string[]);

            return "/" + result;
        }
    }
}
