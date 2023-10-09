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
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Settings;
using System.Collections.Generic;
using System.Threading;

namespace WixEdit.Xml
{
    public class WixFiles : IDisposable
    {
        FileInfo wxsFile;

        UndoManager undoManager;
        DefineManager defineManager;
        IncludeManager includeManager;

        XmlDocument wxsDocument;
        XmlNamespaceManager wxsNsmgr;

        ProjectSettings projectSettings;

        FileSystemWatcher wxsWatcher;
        FileSystemEventHandler wxsWatcher_ChangedHandler;
        public event EventHandler wxsChanged;

        static XmlDocument xsdDocument;
        static XmlNamespaceManager xsdNsmgr;


        static ArrayList xsdExtensionNames;
        static Hashtable xsdExtensions;
        static Hashtable xsdExtensionNsmgrs;
        static Hashtable xsdExtensionTargetNamespaces;
        static Hashtable xsdExtensionTargetNamespacesReverseMap;

        Hashtable xsdExtensionPrefixesMap;
        Hashtable xsdExtensionPrefixesReverseMap;

        bool isTempNewFile = false;

        public static string WixNamespaceUri
        {
            get
            {
                if (WixEditSettings.Instance.IsUsingWix3())
                {
                    return WixNamespaceUri_V3;
                }
                else if (WixEditSettings.Instance.IsUsingWix2())
                {
                    return WixNamespaceUri_V2;
                }
                else if (WixEditSettings.Instance.IsUsingWix4())
                {
                    return WixNamespaceUri_V4;
                }
                else
                {
                    return WixNamespaceUri_V3;
                }
            }
        }
        public static string WixNamespaceUri_V2 = "http://schemas.microsoft.com/wix/2003/01/wi";
        public static string WixNamespaceUri_V3 = "http://schemas.microsoft.com/wix/2006/wi";
        public static string WixNamespaceUri_V4 = "http://wixtoolset.org/schemas/v4/wxs";

        private string customLightArgumentsWarning;

        static WixFiles()
        {
            ReloadXsd();
        }

        public static WixFiles FromTemplate()
        {
            return new WixFiles(string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Wix xmlns=""{2}"">
  <Product Id=""{0}"" Name=""TestProduct"" Language=""1033"" Version=""0.0.0.1"" Manufacturer=""WixEdit"" UpgradeCode=""{1}"">
    <Package Description=""Test file in a Product"" Comments=""Simple test"" InstallerVersion=""200"" Compressed=""yes"" />
    <Media Id=""1"" Cabinet=""simple.cab"" EmbedCab=""yes"" />
    <Directory Id=""TARGETDIR"" Name=""SourceDir"">
      <Directory Id=""ProgramFilesFolder"" Name=""PFiles"" />
    </Directory>
    <Feature Id=""DefaultFeature"" Title=""Main Feature"" Level=""1"">
    </Feature>
    <UI />
  </Product>
</Wix>",
Guid.NewGuid().ToString().ToUpper(),
Guid.NewGuid().ToString().ToUpper(),
WixFiles.WixNamespaceUri));
        }

        public WixFiles(string xml)
        {
            LoadNewWxsFile(xml);

            undoManager = new UndoManager(this, wxsDocument);

            wxsWatcher_ChangedHandler = new FileSystemEventHandler(wxsWatcher_Changed);
        }

        public WixFiles(FileInfo wxsFileInfo)
        {
            wxsFile = wxsFileInfo;

            LoadWxsFile();

            undoManager = new UndoManager(this, wxsDocument);

            wxsWatcher_ChangedHandler = new FileSystemEventHandler(wxsWatcher_Changed);

            wxsWatcher = new FileSystemWatcher(wxsFile.Directory.FullName, wxsFile.Name);
            wxsWatcher.Changed += wxsWatcher_ChangedHandler;
            wxsWatcher.EnableRaisingEvents = true;
        }

        public string GetNamespaceUri(string elementName)
        {
            int nodeColonPos = elementName.IndexOf(":");
            if (nodeColonPos < 0)
            {
                return WixNamespaceUri;
            }
            else
            {
                string theNodeNamespace = LookupExtensionName(elementName.Substring(0, nodeColonPos));

                return (string)xsdExtensionTargetNamespaces[theNodeNamespace];
            }
        }

        public ProjectSettings ProjectSettings
        {
            get
            {
                return projectSettings;
            }
        }

        public bool ReadOnly()
        {
            if (!wxsFile.Exists)
            {
                return true;
            }

            return ((wxsFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
        }

        public void LoadWxsFile()
        {
            if (wxsDocument == null)
            {
                wxsDocument = new XmlDocument();
            }

            FileMode mode = FileMode.Open;
            using (FileStream fs = new FileStream(wxsFile.FullName, mode, FileAccess.Read, FileShare.Read))
            {
                wxsDocument.Load(fs);
                fs.Close();
            }

            if (wxsDocument.DocumentElement.GetAttribute("xmlns").ToLower() != WixNamespaceUri.ToLower())
            {
                string errorMessage = String.Format("\"{0}\" has the wrong namespace!\r\n\r\nFound namespace \"{1}\",\r\nbut WiX binaries version \"{2}\" require \"{3}\".\r\n\r\nYou can either convert the WiX source file to use the correct namespace (use WixCop.exe for upgrading from 2.0 to 3.0), or configure the correct version of the WiX binaries in the WixEdit settings.", wxsFile.Name, wxsDocument.DocumentElement.GetAttribute("xmlns"), WixEditSettings.Instance.WixBinariesVersion, WixNamespaceUri);
                throw new WixEditException(errorMessage);
            }

            if (ReadOnly())
            {
                MessageBox.Show(String.Format("\"{0}\" is read-only.", wxsFile.Name), "Read Only!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            XmlNode possibleComment = wxsDocument.FirstChild;
            if (possibleComment.NodeType == XmlNodeType.XmlDeclaration)
            {
                possibleComment = wxsDocument.FirstChild.NextSibling;
            }
            if (possibleComment != null && possibleComment.Name == "#comment")
            {
                string comment = possibleComment.Value;

                string candleArgs = String.Empty;
                string lightArgs = String.Empty;
                bool foundArg = false;
                foreach (string fullLine in comment.Split('\r', '\n'))
                {
                    string line = fullLine.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    string candleStart = "candleargs:";
                    if (line.ToLower().StartsWith(candleStart))
                    {
                        candleArgs = line.Remove(0, candleStart.Length);
                        foundArg = true;
                    }

                    string lightStart = "lightargs:";
                    if (line.ToLower().StartsWith(lightStart))
                    {
                        lightArgs = line.Remove(0, lightStart.Length);
                        foundArg = true;
                    }
                }

                if (foundArg == true)
                {
                    wxsDocument.RemoveChild(possibleComment);
                }

                projectSettings = new ProjectSettings(candleArgs.Trim(), lightArgs.Trim());
            }
            else
            {
                projectSettings = new ProjectSettings(String.Empty, String.Empty);
            }

            xsdExtensionPrefixesMap = new Hashtable();
            xsdExtensionPrefixesReverseMap = new Hashtable();
            foreach (XmlAttribute att in wxsDocument.DocumentElement.Attributes)
            {
                string attName = att.Name;
                if (attName.StartsWith("xmlns:"))
                {
                    if (xsdExtensionTargetNamespacesReverseMap.ContainsKey(att.Value))
                    {
                        string existingNamespaceName = (string)xsdExtensionTargetNamespacesReverseMap[att.Value];
                        string namespaceName = attName.Substring(6);
                        if (namespaceName != existingNamespaceName)
                        {
                            xsdExtensionPrefixesMap.Add(namespaceName, existingNamespaceName);
                            xsdExtensionPrefixesReverseMap.Add(existingNamespaceName, namespaceName);
                        }
                    }
                }
            }

            foreach (DictionaryEntry entry in xsdExtensionTargetNamespaces)
            {
                if (xsdExtensionPrefixesMap.ContainsValue(entry.Key) == false)
                {
                    xsdExtensionPrefixesMap.Add(entry.Key, entry.Key);
                    xsdExtensionPrefixesReverseMap.Add(entry.Key, entry.Key);

                    wxsDocument.DocumentElement.SetAttribute("xmlns:" + (string)entry.Key, (string)entry.Value);
                }
            }

            wxsDocument.LoadXml(wxsDocument.OuterXml);

            wxsNsmgr = new XmlNamespaceManager(wxsDocument.NameTable);
            wxsNsmgr.AddNamespace("wix", wxsDocument.DocumentElement.NamespaceURI);
            foreach (DictionaryEntry entry in xsdExtensionTargetNamespaces)
            {
                wxsNsmgr.AddNamespace(LookupExtensionNameReverse((string)entry.Key), (string)entry.Value);
            }

            //init define manager to allow include manager to add dynamic includes
            defineManager = new DefineManager(this, wxsDocument);

            // Init IncludeManager after all doc.LoadXml(doc.OuterXml), because all references to nodes would dissapear!
            includeManager = new IncludeManager(this, wxsDocument);

            //re-init define manager using final includes
            defineManager = new DefineManager(this, wxsDocument);
        }

        public void LoadNewWxsFile(string xml)
        {
            wxsFile = new FileInfo(@"c:\foo.bar.baz");
            isTempNewFile = true;

            if (wxsDocument == null)
            {
                wxsDocument = new XmlDocument();
                wxsDocument.PreserveWhitespace = true;
            }

            wxsDocument.LoadXml(xml);

            if (wxsDocument.DocumentElement.GetAttribute("xmlns").ToLower() != WixNamespaceUri.ToLower())
            {
                string errorMessage = String.Format("\"{0}\" has the wrong namespace!\r\n\r\nFound namespace \"{1}\",\r\nbut WiX binaries version \"{2}\" require \"{3}\".\r\n\r\nYou can either convert the WiX source file to use the correct namespace (use WixCop.exe for upgrading from 2.0 to 3.0), or configure the correct version of the WiX binaries in the WixEdit settings.", wxsFile.Name, wxsDocument.DocumentElement.GetAttribute("xmlns"), WixEditSettings.Instance.WixBinariesVersion, WixNamespaceUri);
                throw new WixEditException(errorMessage);
            }

            XmlNode possibleComment = wxsDocument.FirstChild;
            if (possibleComment.NodeType == XmlNodeType.XmlDeclaration)
            {
                possibleComment = wxsDocument.FirstChild.NextSibling;
            }
            if (possibleComment != null && possibleComment.Name == "#comment")
            {
                string comment = possibleComment.Value;

                string candleArgs = String.Empty;
                string lightArgs = String.Empty;
                bool foundArg = false;
                foreach (string fullLine in comment.Split('\r', '\n'))
                {
                    string line = fullLine.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    string candleStart = "candleargs:";
                    if (line.ToLower().StartsWith(candleStart))
                    {
                        candleArgs = line.Remove(0, candleStart.Length);
                        foundArg = true;
                    }

                    string lightStart = "lightargs:";
                    if (line.ToLower().StartsWith(lightStart))
                    {
                        lightArgs = line.Remove(0, lightStart.Length);
                        foundArg = true;
                    }
                }

                if (foundArg == true)
                {
                    wxsDocument.RemoveChild(possibleComment);
                }

                projectSettings = new ProjectSettings(candleArgs.Trim(), lightArgs.Trim());
            }
            else
            {
                projectSettings = new ProjectSettings(String.Empty, String.Empty);
            }

            xsdExtensionPrefixesMap = new Hashtable();
            xsdExtensionPrefixesReverseMap = new Hashtable();
            foreach (XmlAttribute att in wxsDocument.DocumentElement.Attributes)
            {
                string attName = att.Name;
                if (attName.StartsWith("xmlns:"))
                {
                    if (xsdExtensionTargetNamespacesReverseMap.ContainsKey(att.Value))
                    {
                        string existingNamespaceName = (string)xsdExtensionTargetNamespacesReverseMap[att.Value];
                        string namespaceName = attName.Substring(6);
                        if (namespaceName != existingNamespaceName)
                        {
                            xsdExtensionPrefixesMap.Add(namespaceName, existingNamespaceName);
                            xsdExtensionPrefixesReverseMap.Add(existingNamespaceName, namespaceName);
                        }
                    }
                }
            }

            foreach (DictionaryEntry entry in xsdExtensionTargetNamespaces)
            {
                if (xsdExtensionPrefixesMap.ContainsValue(entry.Key) == false)
                {
                    xsdExtensionPrefixesMap.Add(entry.Key, entry.Key);
                    xsdExtensionPrefixesReverseMap.Add(entry.Key, entry.Key);

                    wxsDocument.DocumentElement.SetAttribute("xmlns:" + (string)entry.Key, (string)entry.Value);
                }
            }

            wxsDocument.LoadXml(wxsDocument.OuterXml);

            wxsNsmgr = new XmlNamespaceManager(wxsDocument.NameTable);
            wxsNsmgr.AddNamespace("wix", wxsDocument.DocumentElement.NamespaceURI);
            foreach (DictionaryEntry entry in xsdExtensionTargetNamespaces)
            {
                wxsNsmgr.AddNamespace(LookupExtensionNameReverse((string)entry.Key), (string)entry.Value);
            }

            //init define manager to allow include manager to add dynamic includes
            defineManager = new DefineManager(this, wxsDocument);

            // Init IncludeManager after all doc.LoadXml(doc.OuterXml), because all references to nodes would dissapear!
            includeManager = new IncludeManager(this, wxsDocument);

            //re-init define manager using final includes
            defineManager = new DefineManager(this, wxsDocument);
        }

        public bool IsNew
        {
            get
            {
                return isTempNewFile;
            }
        }

        public UndoManager UndoManager
        {
            get
            {
                return undoManager;
            }
        }

        public DefineManager DefineManager
        {
            get
            {
                return defineManager;
            }
        }

        public IncludeManager IncludeManager
        {
            get
            {
                return includeManager;
            }
        }

        public static void ReloadXsd()
        {
            bool reloadXsd = false;

            xsdDocument = new XmlDocument();

            if (WixEditSettings.Instance.IsUsingWix4())
            {
                // load xsd from embedded resource
                string resourcePath = "WixEdit.src.Xsd.wix4.xsd";

                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream xsdStream = assembly.GetManifestResourceStream(resourcePath);
                xsdDocument.Load(XmlReader.Create(xsdStream));

                reloadXsd = true;
            }
            else if (File.Exists(WixEditSettings.Instance.GetWixXsdLocation()))
            {
                xsdDocument.Load(WixEditSettings.Instance.GetWixXsdLocation());

                reloadXsd = true;
            }

            if (reloadXsd)
            {
                xsdNsmgr = new XmlNamespaceManager(xsdDocument.NameTable);
                xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
                xsdNsmgr.AddNamespace("xse", "http://schemas.microsoft.com/wix/2005/XmlSchemaExtension");

                ReloadExtensionXsds();
            }
        }

        public static bool CheckForXsd()
        {
            if (WixEditSettings.Instance.IsUsingWix4())
            {
                // the Wix 4 xsd is an embedded resource
                return true;
            }

            return File.Exists(WixEditSettings.Instance.GetWixXsdLocation());
        }

        private string LookupExtensionName(string name)
        {
            if (xsdExtensionPrefixesMap.ContainsKey(name))
            {
                return (string)xsdExtensionPrefixesMap[name];
            }

            return null;
        }

        private string LookupExtensionNameReverse(string name)
        {
            if (xsdExtensionPrefixesReverseMap.ContainsKey(name))
            {
                return (string)xsdExtensionPrefixesReverseMap[name];
            }

            return null;
        }

        public XmlNode GetXsdElementNode(string nodeName)
        {
            XmlNode ret = null;

            int nodeColonPos = nodeName.IndexOf(":");
            if (nodeColonPos < 0)
            {
                ret = xsdDocument.SelectSingleNode(String.Format("//xs:element[@name='{0}']", nodeName), xsdNsmgr);
            }
            else
            {
                string theNodeName = nodeName.Substring(nodeColonPos + 1);
                string theNodeNamespace = LookupExtensionName(nodeName.Substring(0, nodeColonPos));

                if (theNodeNamespace != null)
                {
                    XmlDocument extensionXsdDocument = xsdExtensions[theNodeNamespace] as XmlDocument;
                    XmlNamespaceManager extensionXsdNsmgr = xsdExtensionNsmgrs[theNodeNamespace] as XmlNamespaceManager;

                    if (extensionXsdDocument == null || extensionXsdNsmgr == null)
                    {
                        ret = null;
                    }
                    else
                    {
                        ret = extensionXsdDocument.SelectSingleNode(String.Format("//xs:element[@name='{0}']", theNodeName), extensionXsdNsmgr);
                    }
                }
                else
                {
                    // Just try the default namespace...
                    ret = xsdDocument.SelectSingleNode(String.Format("//xs:element[@name='{0}']", theNodeName), xsdNsmgr);
                }
            }

            return ret;
        }

        public static ArrayList GetXsdAllElementNames()
        {
            ArrayList ret = new ArrayList();
            XmlNodeList nodes = xsdDocument.SelectNodes("/xs:schema/xs:element", WixFiles.GetXsdNsmgr());
            foreach (XmlNode node in nodes)
            {
                ret.Add(node);
            }

            foreach (string extensionName in xsdExtensionNames)
            {
                XmlDocument extXsd = xsdExtensions[extensionName] as XmlDocument;
                XmlNamespaceManager extXsdNsmgr = xsdExtensionNsmgrs[extensionName] as XmlNamespaceManager;

                nodes = extXsd.SelectNodes("/xs:schema/xs:element", extXsdNsmgr);
                foreach (XmlNode node in nodes)
                {
                    ret.Add(node);
                }
            }

            return ret;
        }

        public ArrayList GetXsdSubElements(string elementName)
        {
            return GetXsdSubElements(elementName, new StringCollection());
        }

        public ArrayList GetXsdSubElements(string elementName, StringCollection skipElements)
        {
            ArrayList ret = new ArrayList();
            ArrayList retExt = new ArrayList();

            int nodeColonPos = elementName.IndexOf(":");
            if (nodeColonPos < 0)
            {
                XmlNodeList xmlSubElements = xsdDocument.SelectNodes(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType//xs:element", elementName), xsdNsmgr);
                foreach (XmlNode xmlSubElement in xmlSubElements)
                {
                    XmlAttribute refAtt = xmlSubElement.Attributes["ref"];
                    if (refAtt != null)
                    {
                        if (refAtt.Value != null && refAtt.Value.Length > 0)
                        {
                            if (skipElements.Contains(refAtt.Value))
                            {
                                continue;
                            }
                            ret.Add(refAtt.Value);
                        }
                    }
                }

                foreach (string extensionName in xsdExtensionNames)
                {
                    string displayNamespaceName = LookupExtensionNameReverse(extensionName);

                    XmlDocument extXsd = xsdExtensions[extensionName] as XmlDocument;
                    XmlNamespaceManager extXsdNsmgr = xsdExtensionNsmgrs[extensionName] as XmlNamespaceManager;

                    XmlNodeList subNodes = extXsd.SelectNodes(String.Format("/xs:schema/xs:element[xs:annotation/xs:appinfo/xse:parent/@ref='{0}']", elementName), extXsdNsmgr);
                    foreach (XmlElement subNode in subNodes)
                    {
                        retExt.Add(String.Format("{0}:{1}", displayNamespaceName, subNode.GetAttribute("name")));
                    }
                }
            }
            else
            {
                string theNodeName = elementName.Substring(nodeColonPos + 1);
                string theNodeDisplayNamespace = elementName.Substring(0, nodeColonPos);
                string theNodeNamespace = LookupExtensionName(theNodeDisplayNamespace);

                if (theNodeNamespace != null && xsdExtensions.ContainsKey(theNodeNamespace))
                {
                    XmlDocument extXsd = xsdExtensions[theNodeNamespace] as XmlDocument;
                    XmlNamespaceManager extXsdNsmgr = xsdExtensionNsmgrs[theNodeNamespace] as XmlNamespaceManager;

                    XmlNodeList xmlSubElements = extXsd.SelectNodes(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType//xs:element", theNodeName), extXsdNsmgr);
                    foreach (XmlNode xmlSubElement in xmlSubElements)
                    {
                        XmlAttribute refAtt = xmlSubElement.Attributes["ref"];
                        if (refAtt != null)
                        {
                            if (refAtt.Value != null && refAtt.Value.Length > 0)
                            {
                                if (skipElements.Contains(refAtt.Value))
                                {
                                    continue;
                                }
                                retExt.Add(String.Format("{0}:{1}", theNodeDisplayNamespace, refAtt.Value));
                            }
                        }
                    }
                }
            }

            ret.Sort();
            retExt.Sort();
            ret.AddRange(retExt);


            return ret;
        }

        private static void ReloadExtensionXsds()
        {
            xsdExtensions = new Hashtable();
            xsdExtensionNsmgrs = new Hashtable();
            xsdExtensionTargetNamespaces = new Hashtable();
            xsdExtensionTargetNamespacesReverseMap = new Hashtable();
            xsdExtensionNames = new ArrayList();

            // WiX uses extensions after version 2.
            if (WixEditSettings.Instance.IsUsingWix2() == false &&
                WixEditSettings.Instance.WixBinariesDirectory.Xsds != null && WixEditSettings.Instance.WixBinariesDirectory.Xsds.Length > 0)
            {
                DirectoryInfo xsdInfo = new DirectoryInfo(WixEditSettings.Instance.WixBinariesDirectory.Xsds);
                if (xsdInfo.Exists)
                {
                    foreach (FileInfo extensionFileInfo in xsdInfo.GetFiles("*.xsd"))
                    {
                        string extension = Path.GetFileNameWithoutExtension(extensionFileInfo.Name);
                        if (extension.ToLower() == "wix" || extension.ToLower() == "wixloc")
                        {
                            continue;
                        }

                        XmlDocument extensionXsdDocument = new XmlDocument();
                        extensionXsdDocument.Load(extensionFileInfo.FullName);

                        xsdExtensions.Add(extension, extensionXsdDocument);

                        XmlNamespaceManager extensionXsdNsmgr = new XmlNamespaceManager(extensionXsdDocument.NameTable);
                        extensionXsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
                        extensionXsdNsmgr.AddNamespace("xse", "http://schemas.microsoft.com/wix/2005/XmlSchemaExtension");

                        xsdExtensionNsmgrs.Add(extension, extensionXsdNsmgr);

                        XmlNode targetNamespaceAtt = extensionXsdDocument.SelectSingleNode("/xs:schema/@targetNamespace", extensionXsdNsmgr);
                        string targetNamespace = targetNamespaceAtt.Value;

                        xsdExtensionTargetNamespaces.Add(extension, targetNamespace);
                        xsdExtensionTargetNamespacesReverseMap.Add(targetNamespace, extension);

                        xsdExtensionNames.Add(extension);
                    }
                }
            }
        }

        public static XmlDocument GetXsdDocument()
        {
            return xsdDocument;
        }

        public static XmlNamespaceManager GetXsdNsmgr()
        {
            return xsdNsmgr;
        }

        public XmlDocument WxsDocument
        {
            get { return wxsDocument; }
        }

        public XmlNamespaceManager WxsNsmgr
        {
            get { return wxsNsmgr; }
        }

        public XmlDocument XsdDocument
        {
            get { return xsdDocument; }
        }

        public XmlNamespaceManager XsdNsmgr
        {
            get { return xsdNsmgr; }
        }

        public FileInfo WxsFile
        {
            get { return wxsFile; }
        }

        public FileInfo OutputFile
        {
            get
            {
                if (HasLightArguments)
                {
                    string customArgs = GetCustomLightArguments();
                    int outPos = customArgs.IndexOf("-out");
                    if (outPos >= 0)
                    {
                        customArgs = customArgs.Substring(outPos + 4).Trim();

                        string result = customArgs;
                        if (customArgs.StartsWith("\""))
                        {
                            int endQuotePos = customArgs.IndexOf("\"", 1);
                            if (endQuotePos > 1)
                            {
                                result = customArgs.Substring(1, endQuotePos - 1);
                            }
                        }
                        else if (customArgs.StartsWith("'"))
                        {
                            int endQuotePos = customArgs.IndexOf("'", 1);
                            if (endQuotePos > 1)
                            {
                                result = customArgs.Substring(1, endQuotePos - 1);
                            }
                        }
                        else
                        {
                            int endSpacePos = customArgs.IndexOf(" ");
                            int endQuotePos = customArgs.IndexOf("\"");
                            if (endSpacePos > 0)
                            {
                                result = customArgs.Substring(0, endSpacePos);
                            }
                            else if (endQuotePos > 0)
                            {
                                result = customArgs.Substring(0, endQuotePos);
                            }
                            else
                            {
                                result = customArgs;
                            }
                        }

                        if (result != null)
                        {
                            try
                            {
                                if (Path.IsPathRooted(result))
                                {
                                    return new FileInfo(result);
                                }
                                else
                                {
                                    return new FileInfo(Path.Combine(wxsFile.Directory.FullName, result));
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                if (customLightArgumentsWarning != GetCustomLightArguments())
                                {
                                    customLightArgumentsWarning = GetCustomLightArguments();
                                    MessageBox.Show(String.Format("Could not determine output file from custom commandline, please check your custom arguments for light.exe.\r\n\r\nCustom light arguments: \"{0}\"\r\nDetermined output file: \"{1}\"\r\nError message: \"{2}\"", GetCustomLightArguments(), result, ex.Message), "Illegal custom arguments", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }

                string extension = "msi";
                if (wxsDocument.SelectSingleNode("/wix:Wix/wix:Module", wxsNsmgr) != null)
                {
                    extension = "msm";
                }

                return new FileInfo(Path.ChangeExtension(wxsFile.FullName, extension));
            }
        }

        public bool HasLightArguments
        {
            get
            {
                if (projectSettings.LightArgs != null && projectSettings.LightArgs.Trim().Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string GetLightArguments()
        {
            if (HasLightArguments)
            {
                return GetCustomLightArguments();
            }
            else
            {
                string ret = String.Format("-nologo \"{0}\" -out \"{1}\"", Path.ChangeExtension(wxsFile.FullName, "wixobj"), OutputFile);

                // WiX uses extensions after version 2.
                if (WixEditSettings.Instance.IsUsingWix2() == false)
                {
                    ret = String.Format("{0} {1}", ret, GetExtensionArguments());
                }

                return ret;
            }
        }

        public string GetCustomLightArguments()
        {
            string lightArgs = projectSettings.LightArgs;
            lightArgs = lightArgs.Replace("<projectfile>", wxsFile.FullName);
            lightArgs = lightArgs.Replace("<projectname>", Path.GetFileNameWithoutExtension(wxsFile.Name));
            lightArgs = lightArgs.Replace("<extensions>", GetExtensionArguments());

            return lightArgs;
        }

        public bool HasCandleArguments
        {
            get
            {
                if (projectSettings.CandleArgs != null && projectSettings.CandleArgs.Trim().Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string GetExtensionArguments()
        {
            StringBuilder ret = new StringBuilder();
            List<string> selectedLowerCaseExtensions = new List<string>();

            // Handle extension namespaces. Remove all those which are not used in the file.
            foreach (string ext in xsdExtensionNames)
            {
                XmlNodeList list = wxsDocument.SelectNodes(String.Format("//{0}:*", ext), wxsNsmgr);
                if (list.Count > 0)
                {
                    ret.AppendFormat(" -ext Wix{0}{1}Extension ", ext.Substring(0, 1).ToUpper(), ext.Substring(1));
                    selectedLowerCaseExtensions.Add(ext.ToLower());
                }
            }

            if (!selectedLowerCaseExtensions.Contains("ui"))
            {
                XmlNodeList uiRefList = wxsDocument.SelectNodes("//wix:UIRef", wxsNsmgr);
                if (uiRefList.Count > 0)
                {
                    ret.Append(" -ext WixUIExtension ");
                }
            }

            if (!selectedLowerCaseExtensions.Contains("netfx"))
            {
                XmlNodeList propRefList = wxsDocument.SelectNodes("//wix:PropertyRef[starts-with(@Id, 'NETFRAMEWORK')]", wxsNsmgr);
                if (propRefList.Count > 0)
                {
                    ret.Append(" -ext WixNetFxExtension ");
                }
            }

            if (!selectedLowerCaseExtensions.Contains("util"))
            {
                XmlNodeList binaryKeyWixCAList = wxsDocument.SelectNodes("//@BinaryKey[.='WixCA']", wxsNsmgr);
                if (binaryKeyWixCAList.Count > 0)
                {
                    ret.Append(" -ext WixUtilExtension ");
                }
            }

            return ret.ToString();
        }

        public string GetCandleArguments()
        {
            if (HasCandleArguments)
            {
                return GetCustomCandleArguments();
            }
            else
            {
                string ret = String.Format("-nologo \"{0}\" -out \"{1}\"", wxsFile.FullName, Path.ChangeExtension(wxsFile.FullName, "wixobj"));

                // WiX uses extensions after version 2.
                if (WixEditSettings.Instance.IsUsingWix2() == false)
                {
                    ret = String.Format("{0} {1}", ret, GetExtensionArguments());
                }

                return ret;
            }
        }

        public string GetWixArguments()
        {
            return $"build \"{wxsFile.FullName}\" -out \"{OutputFile}\"";
        }

        private string GetCustomCandleArguments()
        {
            string candleArgs = projectSettings.CandleArgs;
            candleArgs = candleArgs.Replace("<projectfile>", wxsFile.FullName);
            candleArgs = candleArgs.Replace("<projectname>", Path.GetFileNameWithoutExtension(wxsFile.Name));
            candleArgs = candleArgs.Replace("<extensions>", GetExtensionArguments());

            return candleArgs;
        }

        public DirectoryInfo WxsDirectory
        {
            get { return wxsFile.Directory; }
        }

        public static bool HasResource(string resourceName)
        {
            string resourceNamespace = "WixEdit.res.";
            Assembly assembly = Assembly.GetAssembly(typeof(WixFiles));
            if (assembly.GetManifestResourceInfo(resourceNamespace + resourceName) == null)
            {
                return false;
            }

            return true;
        }

        public static Stream GetResourceStream(string resourceName)
        {
            string resourceNamespace = "WixEdit.res.";
            Assembly assembly = Assembly.GetAssembly(typeof(WixFiles));
            if (assembly.GetManifestResourceInfo(resourceNamespace + resourceName) == null)
            {
                throw new Exception("Could not find resource: " + resourceNamespace + resourceName);
            }

            Stream resourceStream = assembly.GetManifestResourceStream(resourceNamespace + resourceName);
            if (resourceStream == null)
            {
                throw new Exception("Could not load resource: " + resourceNamespace + resourceName);
            }

            return resourceStream;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (wxsWatcher != null)
            {
                wxsWatcher.EnableRaisingEvents = false;
                wxsWatcher.Changed -= wxsWatcher_ChangedHandler;
                wxsWatcher.Dispose();
                wxsWatcher = null;
            }

            wxsDocument = null;
            wxsNsmgr = null;
        }

        #endregion

        public bool HasChanges()
        {
            return (UndoManager.HasChanges() || projectSettings.HasChanges() || IsNew);
        }


        public void SaveAs(string newFile)
        {
            FileInfo oldWxsFile = wxsFile;

            wxsFile = new FileInfo(newFile);

            if (IsNew)
            {
                try
                {
                    Save();
                }
                catch
                {
                    wxsFile = oldWxsFile;

                    throw;
                }
            }
            else
            {
                // Save as, is like creating a new file...
                isTempNewFile = true;

                if (wxsWatcher != null)
                {
                    wxsWatcher.EnableRaisingEvents = false;
                    wxsWatcher.Changed -= wxsWatcher_ChangedHandler;
                    wxsWatcher = null;
                }

                try
                {
                    Save();
                }
                catch
                {
                    isTempNewFile = false;
                    wxsFile = oldWxsFile;

                    wxsWatcher = new FileSystemWatcher(wxsFile.Directory.FullName, wxsFile.Name);
                    wxsWatcher.Changed += wxsWatcher_ChangedHandler;
                    wxsWatcher.EnableRaisingEvents = true;

                    throw;
                }
            }

            // After saving make sure this is not a "new" file anymore
            isTempNewFile = false;

            wxsWatcher = new FileSystemWatcher(wxsFile.Directory.FullName, wxsFile.Name);
            wxsWatcher.Changed += wxsWatcher_ChangedHandler;
            wxsWatcher.EnableRaisingEvents = true;
        }

        public void Save()
        {
            if (!IsNew && ReadOnly())
            {
                MessageBox.Show(String.Format("\"{0}\" is read-only, cannot save this file.", wxsFile.Name), "Read Only!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (wxsWatcher != null)
            {
                wxsWatcher.EnableRaisingEvents = false;
            }

            UndoManager.DeregisterHandlers();

            XmlComment commentElement = null;
            if (projectSettings.IsEmpty() == false)
            {
                StringBuilder commentBuilder = new StringBuilder();
                commentBuilder.Append("\r\n");
                commentBuilder.Append("    # This comment is generated by WixEdit, the specific commandline\r\n");
                commentBuilder.Append("    # arguments for the WiX Toolset are stored here.\r\n\r\n");
                commentBuilder.AppendFormat("    candleArgs: {0}\r\n", projectSettings.CandleArgs);
                commentBuilder.AppendFormat("    lightArgs: {0}\r\n", projectSettings.LightArgs);

                commentElement = wxsDocument.CreateComment(commentBuilder.ToString());

                XmlNode firstElement = wxsDocument.FirstChild;
                if (firstElement != null)
                {
                    if (firstElement.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        firstElement = wxsDocument.FirstChild.NextSibling;
                    }

                    wxsDocument.InsertBefore(commentElement, firstElement);
                }
                else
                {
                    wxsDocument.AppendChild(commentElement);
                }
            }

            // Handle extension namespaces. Remove all those which are not used in the file.
            foreach (string ext in xsdExtensionNames)
            {
                string theNodeNamespace = LookupExtensionNameReverse(ext);

                XmlNodeList list = wxsDocument.SelectNodes(String.Format("//{0}:*", theNodeNamespace), wxsNsmgr);
                if (list.Count == 0)
                {
                    // Error reports show that a NullReferenceException occurs on the next line now, how can this be?
                    // The wxsDocument.DocumentElement is null.
                    wxsDocument.DocumentElement.RemoveAttribute(String.Format("xmlns:{0}", theNodeNamespace));
                }
            }

            if (IncludeManager.HasIncludes)
            {
                IncludeManager.RemoveIncludes();

                ArrayList changedIncludes = UndoManager.ChangedIncludes;
                if (changedIncludes.Count > 0)
                {
                    string filesString = String.Join("\r\n\x2022 ", changedIncludes.ToArray(typeof(string)) as string[]);
                    if (DialogResult.Yes == MessageBox.Show(String.Format("Do you want to save the following changed include files?\r\n\r\n\x2022 {0}", filesString), "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        IncludeManager.SaveIncludes(UndoManager.ChangedIncludes);
                    }
                }
            }

            FileMode mode = FileMode.OpenOrCreate;
            if (File.Exists(wxsFile.FullName))
            {
                mode = mode | FileMode.Truncate;
            }

            using (FileStream fs = new FileStream(wxsFile.FullName, mode))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = new string(' ', WixEditSettings.Instance.XmlIndentation);
                settings.NewLineHandling = NewLineHandling.None;
                settings.Encoding = new System.Text.UTF8Encoding();
                XmlWriter writer = XmlWriter.Create(fs, settings);

                wxsDocument.Save(writer);

                writer.Close();
                fs.Close();
            }

            if (IncludeManager.HasIncludes)
            {
                // Remove nodes from main xml document
                IncludeManager.RestoreIncludes();
            }

            projectSettings.ChangesHasBeenSaved();

            if (commentElement != null)
            {
                wxsDocument.RemoveChild(commentElement);
            }

            if (wxsWatcher != null)
            {
                wxsWatcher.EnableRaisingEvents = true;
            }

            undoManager.DocumentIsSaved();
            UndoManager.RegisterHandlers();
        }

        private void wxsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Form mainForm = FindForm();

            // Make it happen on the correct thread.
            mainForm.Invoke(new ThreadStart(delegate () { OnWxsChanged(); }));
        }

        private void OnWxsChanged()
        {
            wxsWatcher.EnableRaisingEvents = false;

            DialogResult result = DialogResult.None;
            if (undoManager.HasChanges())
            {
                Form mainForm = FindForm();
                result = MessageBox.Show(mainForm, String.Format("An external program changed \"{0}\", do you want to load the changes from disk and ignore the changes in memory?", wxsFile.Name), "Reload?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            else
            {
                Form mainForm = FindForm();
                result = MessageBox.Show(mainForm, String.Format("An external program changed \"{0}\", do you want to load the changes from disk?", wxsFile.Name), "Reload?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (result == DialogResult.Yes)
            {
                try
                {
                    LoadWxsFile();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(String.Format("Access is denied. ({0}))", wxsFile.Name), "Acces denied", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (XmlException ex)
                {
                    MessageBox.Show(String.Format("Failed to open file. ({0}) The xml is not valid:\r\n\r\n{1}", wxsFile.Name, ex.Message), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (WixEditException ex)
                {
                    MessageBox.Show(String.Format("Cannot open file:\r\n\r\n{0}", ex.Message), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch
                {
                    MessageBox.Show(String.Format("Failed to open file. ({0}))", wxsFile.Name), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                UndoManager.Clear();
                UndoManager.DocumentIsSaved();

                if (wxsChanged != null)
                {
                    wxsChanged(this, new EventArgs());
                }
            }

            wxsWatcher.EnableRaisingEvents = true;
        }

        private Form FindForm()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EditorForm)
                {
                    EditorForm mainForm = f as EditorForm;
                    if (mainForm.wixFiles != null &&
                        mainForm.wixFiles.WxsFile.FullName == this.WxsFile.FullName)
                    {
                        return mainForm;
                    }
                }
            }

            return null;
        }
    }

    public class ProjectSettings
    {
        private bool hasChanges;
        private string candleArgs;
        private string lightArgs;

        public readonly string DefaultCandleArgs = "\"<projectfile>\" -out \"<projectname>.wixobj\" <extensions>";
        public readonly string DefaultLightArgs = "\"<projectname>.wixobj\" -out \"<projectname>.msi\" <extensions>";

        public ProjectSettings(string candleArguments, string lightArguments)
        {
            candleArgs = candleArguments;
            lightArgs = lightArguments;

            hasChanges = false;
        }

        public string CandleArgs
        {
            get
            {
                return candleArgs;
            }
            set
            {
                candleArgs = value;
                hasChanges = true;
            }
        }
        public string LightArgs
        {
            get
            {
                return lightArgs;
            }
            set
            {
                lightArgs = value;
                hasChanges = true;
            }
        }

        public bool IsEmpty()
        {
            return ((lightArgs == String.Empty) && (candleArgs == String.Empty));
        }

        public bool HasChanges()
        {
            return hasChanges;
        }

        public void ChangesHasBeenSaved()
        {
            hasChanges = false;
        }
    }
}
