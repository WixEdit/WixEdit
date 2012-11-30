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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Windows.Forms;

using WixEdit.PropertyGridExtensions;
using System.Collections.Generic;

namespace WixEdit.Settings {
    public enum PathHandling {
        UseRelativePathsWhenPossible = 0,
        ForceRelativePaths = 1,
        ForceAbolutePaths = 2
    }

    public enum IncludeChangesHandling {
        AskForEachFile = 0,
        Allow = 1,
        Disallow = 2
    }

    [DefaultPropertyAttribute("BinDirectory")]
    public class WixEditSettings : PropertyAdapterBase {
        [XmlRoot("WixEdit")]
        public class WixEditData {
            public WixEditData() {
                UseRelativeOrAbsolutePaths = PathHandling.UseRelativePathsWhenPossible;
                ExternalXmlEditor = Path.Combine(Environment.SystemDirectory, "notepad.exe");
                UseInstanceOnly = false;
                WordWrapInResultsPanel = false;

                RecentOpenedFiles = new string[] {};

                DisplayFullPathInTitlebar = false;

                XmlIndentation = 4;

                EditDialog = new EditDialogData();

                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                AllowIncludeChanges = IncludeChangesHandling.AskForEachFile;
                BackupChangedIncludes = true;
            }

            public WixEditData(WixEditData oldVersion, XmlDocument rawData) {
                BinDirectory = oldVersion.BinDirectory;
                DarkLocation = oldVersion.DarkLocation;
                CandleLocation = oldVersion.CandleLocation;
                LightLocation = oldVersion.LightLocation;

                if (oldVersion.XsdsLocation == null) {
                    XmlNode node = rawData.SelectSingleNode("/WixEdit/XsdLocation");
                    if (node != null && node.InnerText.Length > 0) {
                        XsdsLocation = Path.GetDirectoryName(node.InnerText);
                    }
                } else {
                    XsdsLocation = oldVersion.XsdsLocation;
                }

                // Some magic to deal with a new WixEdit installation including a newer WiX version...
                if (!String.IsNullOrEmpty(BinDirectory) &&
                    (String.IsNullOrEmpty(DarkLocation) && String.IsNullOrEmpty(CandleLocation) && String.IsNullOrEmpty(LightLocation) && String.IsNullOrEmpty(XsdsLocation)))
                {
                    if (!Directory.Exists(BinDirectory))
                    {
                        DirectoryInfo oldBinDirectory = new DirectoryInfo(BinDirectory);
                        if (oldBinDirectory.Parent != null)
                        {
                            FileInfo[] files = oldBinDirectory.Parent.GetFiles("candle.exe", SearchOption.AllDirectories);
                            if (files.Length == 0 && oldBinDirectory.Parent.Parent != null)
                            {
                                files = oldBinDirectory.Parent.Parent.GetFiles("candle.exe", SearchOption.AllDirectories);
                            }

                            if (files.Length == 1)
                            {
                                BinDirectory = files[0].Directory.FullName;
                            } else if (files.Length > 1)
                            {
                                ArrayList directoryList = new ArrayList();
                                foreach (FileInfo file in files) {
                                    directoryList.Add(file.Directory.FullName);
                                }
                                directoryList.Sort();

                                BinDirectory = (string)directoryList[directoryList.Count - 1];
                            }
                        }
                    }
                }

                TemplateDirectory = oldVersion.TemplateDirectory;
                if (!String.IsNullOrEmpty(TemplateDirectory))
                {
                    TemplateDirectory = TemplateDirectory.Replace("templates", "wizard");
                }
                UseRelativeOrAbsolutePaths = oldVersion.UseRelativeOrAbsolutePaths;
                if (oldVersion.ExternalXmlEditor == null || oldVersion.ExternalXmlEditor.Length == 0) {
                    ExternalXmlEditor = Path.Combine(Environment.SystemDirectory, "notepad.exe");
                } else {
                    ExternalXmlEditor = oldVersion.ExternalXmlEditor;
                }

                UseInstanceOnly = oldVersion.UseInstanceOnly;
                WordWrapInResultsPanel = oldVersion.WordWrapInResultsPanel;

                if (oldVersion.IgnoreFilesAndDirectories != null && oldVersion.IgnoreFilesAndDirectories.Count > 0) {
                    IgnoreFilesAndDirectories = oldVersion.IgnoreFilesAndDirectories;
                } else {
                    ArrayList oldValues = new ArrayList();
                    XmlNodeList nodes = rawData.SelectNodes("/WixEdit/IgnoreFilesAndDirectories/string");
                    foreach (XmlNode node in nodes) {
                        oldValues.Add(node.InnerText);
                    }

                    if (oldValues.Count > 0) {
                        IgnoreFilesAndDirectories = oldValues;
                    }
                }

                RecentOpenedFiles = oldVersion.RecentOpenedFiles;

                DisplayFullPathInTitlebar = oldVersion.DisplayFullPathInTitlebar;

                XmlIndentation = oldVersion.XmlIndentation;

                AllowIncludeChanges = oldVersion.AllowIncludeChanges;
                BackupChangedIncludes = oldVersion.BackupChangedIncludes;
                
                if (oldVersion.EditDialog == null) {
                    EditDialog = new EditDialogData();
                } else {
                    EditDialog = oldVersion.EditDialog;
                }

                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            public string BinDirectory;
            public string DarkLocation;
            public string CandleLocation;
            public string LightLocation;
            public string XsdsLocation;
            public string TemplateDirectory;
            public string ExternalXmlEditor;
            public bool UseInstanceOnly;
            public bool WordWrapInResultsPanel;
            public string Version;
            public PathHandling UseRelativeOrAbsolutePaths;

            public string[] RecentOpenedFiles;

            public bool DisplayFullPathInTitlebar;

            public EditDialogData EditDialog;

            public int XmlIndentation;

            public IncludeChangesHandling AllowIncludeChanges;
            public bool BackupChangedIncludes;

            public ArrayList IgnoreFilesAndDirectories;
        }
        public class EditDialogData {
            public int SnapToGrid = 5;
            public double Scale = 1.00;
            public double Opacity = 1.00;
            public bool AlwaysOnTop = false;
        }

        private static string filename = "WixEditSettings.xml";
        // private static string defaultXml = "<WixEdit><EditDialog /></WixEdit>";

        protected WixEditData data;

        public readonly static WixEditSettings Instance = new WixEditSettings();
        
        private WixEditSettings() : base(null) {
            LoadFromDisk();
        }

        public WixEditData GetInternalDataStructure() {
            return data;
        }

        private string SettingsFile {
            get {
                string wixEditDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WixEdit");
                return Path.Combine(wixEditDataFolder, filename);
            }
        }

        // The location of where the SettingFile once used to be.
        private string OldSettingsFile {
            get {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            }
        }

        public override void RemoveProperty(XmlNode xmlElement) {
            xmlElement.InnerText = "";
        }


        void LoadFromDisk() {
            Stream xmlStream = null;
            if (File.Exists(SettingsFile)) {
                // A FileStream is needed to read the XML document.
                xmlStream = new FileStream(SettingsFile, FileMode.Open);

                using (xmlStream) {
                    // Create an instance of the XmlSerializer class;
                    // specify the type of object to be deserialized.
                    XmlSerializer serializer = new XmlSerializer(typeof(WixEditData));
    
                    // If the XML document has been altered with unknown 
                    // nodes or attributes, handle them with the 
                    // UnknownNode and UnknownAttribute events.
                    serializer.UnknownNode += new XmlNodeEventHandler(DeserializeUnknownNode);
                    serializer.UnknownAttribute += new XmlAttributeEventHandler(DeserializeUnknownAttribute);
                
    
                    // Use the Deserialize method to restore the object's state with
                    // data from the XML document
                    data = (WixEditData) serializer.Deserialize(xmlStream);
                }
            } else {
                // Support the previous situation gracefully.
                if (File.Exists(OldSettingsFile)) {
                    // Try to move to new location
                    try {
                        File.Move(OldSettingsFile, SettingsFile);
                    } catch {
                        // Move didn't work, delete the file
                        try {
                            File.Delete(OldSettingsFile);
                        } catch {}
                    }

                    // If we got rid of the old file, then load again, otherwise just continue...
                    if (File.Exists(OldSettingsFile) == false) {
                        LoadFromDisk();
                        return;
                    }
                }

                data = new WixEditData();
            }

            XmlDocument rawData = new XmlDocument();
            try {
                xmlStream = new FileStream(SettingsFile, FileMode.Open);
                using (xmlStream) {
                    rawData.Load(xmlStream);
                }
            } catch {
                rawData = null;
            }

            try {
                if (data.Version == null) {
                    data = new WixEditData(data, rawData);
                } else {
                    Version current = GetCurrentVersion();
                    Version old = new Version(data.Version);

                    if (current.CompareTo(old) != 0) {
                        // Ok, watch out.
                        if (current.CompareTo(old) < 0) {
                            // This is a config file of a future version.
                            MessageBox.Show(String.Format("The version of the configuration file is newer than the version of this application, if any problems occur remove the settings file: {0}.", SettingsFile), "Configuration file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            data = new WixEditData(data, rawData);
                        } else {
                            // This is a config file of an old version.
                            data = new WixEditData(data, rawData);

                            if (File.Exists(SettingsFile)) {
                                string oldFileName = SettingsFile + "_v" + old.ToString();
                                while (File.Exists(oldFileName)) {
                                    oldFileName = oldFileName + "_";
                                }

                                File.Copy(SettingsFile, oldFileName);
                            }
                        }

                        SaveChanges();
                    }
                }
            } catch {
                MessageBox.Show("Failed to convert the existing configuration file to the current version, using a default configuration.", "Configuration file", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                data = new WixEditData();
            }
        }


        public void DiscardChanges() {
            LoadFromDisk();
        }

        public void SaveChanges() {
            XmlSerializer ser = new XmlSerializer(typeof(WixEditData));
            // A FileStream is used to write the file.

            FileMode mode = FileMode.OpenOrCreate;
            if (File.Exists(SettingsFile)) {
                mode = mode|FileMode.Truncate;
            } else {
                string wixEditDataFolder = Path.GetDirectoryName(SettingsFile);
                if (Directory.Exists(wixEditDataFolder) == false) {
                    Directory.CreateDirectory(wixEditDataFolder);
                }
            }

            using (FileStream fs = new FileStream(SettingsFile, mode)) {
                ser.Serialize(fs, data);
                fs.Close();
            }
        }

        [
        Category("Locations"), 
        Description("The directory where the WiX binaries are located. The wix.xsd is also located by this path."), 
        Editor(typeof(BinDirectoryStructureEditor), typeof(System.Drawing.Design.UITypeEditor)),
        TypeConverter(typeof(BinDirectoryStructure.BinDirectoryExpandableObjectConverter))
        ]
        public BinDirectoryStructure WixBinariesDirectory {
            get {
                if (data.BinDirectory == null && data.CandleLocation == null && data.DarkLocation == null && data.LightLocation == null && data.XsdsLocation == null) {
                    List<DirectoryInfo> dirs = new List<DirectoryInfo>();

                    // With the installation of WixEdit the WiX toolset binaries are installed in "..\wix*", 
                    // relative to the WixEdit binary.
                    DirectoryInfo parent = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
                    if (parent != null) {
                        dirs.AddRange(parent.GetDirectories("wix*"));
                        dirs.AddRange(parent.GetDirectories("Windows Installer XML*"));
                    }

                    if (Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") != null)
                    {
                        parent = new DirectoryInfo(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"));
                        dirs.AddRange(parent.GetDirectories("Windows Installer XML*"));
                        dirs.AddRange(parent.GetDirectories("wix*"));

                        parent = new DirectoryInfo(Environment.GetEnvironmentVariable("PROGRAMFILES"));
                        dirs.AddRange(parent.GetDirectories("Windows Installer XML*"));
                        dirs.AddRange(parent.GetDirectories("wix*"));
                    }
                    else
                    {
                        parent = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                        dirs.AddRange(parent.GetDirectories("Windows Installer XML*"));
                        dirs.AddRange(parent.GetDirectories("wix*"));
                    }

                    parent = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
                    dirs.AddRange(parent.GetDirectories("Windows Installer XML*"));
                    dirs.AddRange(parent.GetDirectories("wix*"));

                    foreach (DirectoryInfo dir in dirs)
                    {
                        foreach (FileInfo file in dir.GetFiles("*.exe", SearchOption.AllDirectories))
                        {
                            if (file.Name.ToLower().Equals("candle.exe"))
                            {
                                data.BinDirectory = file.Directory.FullName;
                                break;
                            }
                        }

                        if (data.BinDirectory != null)
                        {
                            break;
                        }
                    }
                }

                return new BinDirectoryStructure(data);
            }
            set {
                if (value.HasSameBinDirectory()) {
                    data.BinDirectory = new FileInfo(value.Candle).Directory.FullName;
                } else {
                    data.CandleLocation = value.Candle;
                    data.LightLocation = value.Light;
                    data.DarkLocation = value.Dark;
                    data.XsdsLocation = value.Xsds;
                    data.BinDirectory = value.BinDirectory;
                }
            }
        }

        [
        Category("Locations"), 
        Description("The version of the WiX binaries."), 
        ReadOnly(true)
        ]
        public string WixBinariesVersion {
            get {
                BinDirectoryStructure binaries = WixBinariesDirectory;
                if (File.Exists(binaries.Candle) == false ||
                    File.Exists(binaries.Light) == false ||
                    File.Exists(binaries.Dark) == false) {
                    return "(Not all files present)";
                }

                int majorPart;
                bool majorPartMatches = true;
                int minorPart;
                bool minorPartMatches = true;
                int buildPart;
                bool buildPartMatches = true;
                int privatePart;
                bool privatePartMatches = true;

                FileVersionInfo info = FileVersionInfo.GetVersionInfo(binaries.Candle);
                majorPart = info.ProductMajorPart;
                minorPart = info.ProductMinorPart;
                buildPart = info.ProductBuildPart;
                privatePart = info.ProductPrivatePart;


                string[] otherExes = new string[] {binaries.Light, binaries.Dark};
                foreach (string exe in otherExes) {
                    info = FileVersionInfo.GetVersionInfo(exe);
                    if (majorPart != info.ProductMajorPart) {
                        majorPartMatches = false;
                    }
                    if (minorPart != info.ProductMinorPart) {
                        minorPartMatches = false;
                    }
                    if (buildPart != info.ProductBuildPart) {
                        buildPartMatches = false;
                    }
                    if (privatePart != info.ProductPrivatePart) {
                        privatePartMatches = false;
                    }
                }


                if (majorPartMatches == false) {
                    return "WARNING: Using mixed up versions of WiX!";
                }
                if (minorPartMatches == false) {
                    return String.Format("{0}.* (Minor part of versions differ)", majorPart);
                }
                if (buildPartMatches == false) {
                    return String.Format("{0}.{1}.* (Build part of versions differ)", majorPart, minorPart);
                }
                if (privatePartMatches == false) {
                    return String.Format("{0}.{1}.{2}.*", majorPart, minorPart, buildPart);
                }
                
                return String.Format("{0}.{1}.{2}.{3}", majorPart, minorPart, buildPart, privatePart);
            }
        }

        public bool IsWixVersionOk() {
            if (WixBinariesVersion.StartsWith("WARNING")) {
                return false;
            }
            
            return true;
        }

        public bool IsUsingWix2() {
            return WixBinariesVersion.StartsWith("2");
        }

        public bool IsUsingWix3() {
            return WixBinariesVersion.StartsWith("3");
        }

        public string GetWixXsdLocation() {
            return Path.Combine(WixBinariesDirectory.Xsds, "wix.xsd");
        }

        [
        Category("Locations"), 
        Description("The directory where the WixEdit templates are located."), 
        Editor(typeof(BinDirectoryStructureEditor), typeof(System.Drawing.Design.UITypeEditor))
        ]
        public string TemplateDirectory {
            get {
                if (data.TemplateDirectory != null && data.TemplateDirectory.Length > 0) {
                    return data.TemplateDirectory;
                }

                // With the installation of WixEdit the WixEdit Templates are installed in "..\wizard", 
                // relative to the WixEdit binary.
                DirectoryInfo parent = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
                if (parent != null) {
                    string templateDir = Path.Combine(parent.FullName, "wizard");
                    if (Directory.Exists(templateDir)) {
                        return templateDir;
                    }
                }

                return String.Empty;
            }
            set {
                data.TemplateDirectory = value;
            }
        }

        [
        Category("Locations"), 
        Description("The location of your favourite xml editor."), 
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("*.exe |*.exe")
        ]
        public string ExternalXmlEditor {
            get {
                if (data.ExternalXmlEditor != null && data.ExternalXmlEditor.Length > 0) {
                    return data.ExternalXmlEditor;
                }

                return String.Empty;
            }
            set {
                data.ExternalXmlEditor = value;
            }
        }

        public class MyStringCollection : StringCollection {

        }

        [
        Category("Importing"), 
        Description("Directories and files to ignore. The \"*\" is threated as wildcard for zero or more characters."),
        Editor(
            "System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
// .NET 2.0:
//        Editor(
//             "System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
//             "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
        ]
        public ArrayList IgnoreFilesAndDirectories {
            get {
                if (data.IgnoreFilesAndDirectories == null) {
                    data.IgnoreFilesAndDirectories = new ArrayList(
                        new string[] {  "*~",
                                        "#*#",
                                        ".#*",
                                        "%*%",
                                        "._*",
                                        "CVS",
                                        ".cvsignore",
                                        "SCCS",
                                        "vssver.scc",
                                        ".svn",
                                        ".DS_Store" });
                }

                return data.IgnoreFilesAndDirectories;
            }
            set {
                if (value == null) {
                    data.IgnoreFilesAndDirectories = new ArrayList();
                } else {
                    data.IgnoreFilesAndDirectories = value;
                }
            }
        }
        
        [
        Category("View Options"), 
        Description("Display full path in title bar.")
        ]
        public bool DisplayFullPathInTitlebar{
            get {
                return data.DisplayFullPathInTitlebar;
            }
            set {
                data.DisplayFullPathInTitlebar = value;
            }
        }

        [
        Category("View Options"), 
        Description("Use one instance or spawn a new instance of WixEdit for each file."), 
        ]
        public bool UseInstanceOnly {
            get {
                return data.UseInstanceOnly;
            }
            set {
                data.UseInstanceOnly = value;
            }
        }

        [
        Category("View Options"), 
        Description("Use Word Wrap in the result panels as default. (Search & Output panels)"),
        ]
        public bool WordWrapInResultsPanel {
            get {
                return data.WordWrapInResultsPanel;
            }
            set {
                data.WordWrapInResultsPanel = value;
            }
        }

        [
        Category("Miscellaneous"), 
        Description("Use relative or absolute paths.")
        ]
        public PathHandling UseRelativeOrAbsolutePaths {
            get {
                return data.UseRelativeOrAbsolutePaths;
            }
            set {
                data.UseRelativeOrAbsolutePaths = value;
            }
        }

        [
        Category("Include Files"), 
        Description("How to handle changes in include files.")
        ]
        public IncludeChangesHandling AllowIncludeChanges {
            get {
                return data.AllowIncludeChanges;
            }
            set {
                data.AllowIncludeChanges = value;
            }
        }

        [
        Category("Include Files"), 
        Description("Make a backup copy of changed include files? (Extension: .wixedit.original)")
        ]
        public bool BackupChangedIncludes {
            get {
                return data.BackupChangedIncludes;
            }
            set {
                data.BackupChangedIncludes = value;
            }
        }

        [
        Category("Miscellaneous"), 
        Description("How many spaces to use for the indentation of the xml files.")
        ]
        public int XmlIndentation {
            get {
                // Minimum of 0
                return Math.Max(0, data.XmlIndentation);
            }
            set {
                data.XmlIndentation = Math.Max(0, value);
            }
        }


        [
        Category("Version"), 
        Description("The version number of the WixEdit application."), 
        ReadOnly(true)
        ]
        public string ApplicationVersion {
            get { return GetCurrentVersion().ToString(); }
            set {}
        }

        private Version GetCurrentVersion() {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
        
        public string[] GetRecentlyUsedFiles() {
            return data.RecentOpenedFiles;
        }

        public void ClearRecentlyUsedFiles() {
            data.RecentOpenedFiles = new string[] {};
        }

        public void CleanRecentlyUsedFiles() {
            StringCollection recent = new StringCollection();
            recent.AddRange(data.RecentOpenedFiles);

            int i = 0;

            while (recent.Count > i) {
                if (File.Exists(recent[i])) {
                    i++;
                } else {
                    recent.RemoveAt(i);
                }
            }

            string[] result = new string[recent.Count];
            recent.CopyTo(result, 0);
            data.RecentOpenedFiles = result;
        }

        public void AddRecentlyUsedFile(FileInfo newFile) {
            StringCollection recent = new StringCollection();
            recent.AddRange(data.RecentOpenedFiles);

            while(recent.IndexOf(newFile.FullName) >= 0) {
                recent.Remove(newFile.FullName);
            }

            recent.Insert(0, newFile.FullName);

            while (recent.Count > 16) {
                recent.RemoveAt(16);
            }

            string[] result = new string[recent.Count];
            recent.CopyTo(result, 0);
            data.RecentOpenedFiles = result;
        }

        #region EditDialog properties

        [
        Category("Dialog Editor Settings"),
        Description("Number of pixels to snap to in the dialog edior. (Mimimal 1 pixel)"),
        Browsable(false)
        ]
        public int SnapToGrid {
            get {
                // Minimum of 1 pixel
                return Math.Max(1, data.EditDialog.SnapToGrid);
            }
            set {
                data.EditDialog.SnapToGrid = value;
            }
        }

        [
        Category("Dialog Editor Settings"),
        Description("Scale of the dialog in the dialog designer. (For example: 0.50 or 0,50 depending on your regional settings.)"),
        Browsable(false)
        ]
        public double Scale {
            get {
                return data.EditDialog.Scale;
            }
            set {
                data.EditDialog.Scale = value;
            }
        }

        [
        Category("Dialog Editor Settings"),
        Description("Opacity of the dialog in the dialog designer. (For example: 0.50 or 0,50 depending on your regional settings.)"),
        Browsable(false)
        ]
        public double Opacity {
            get {
                // Default to 5 pixels
                return Math.Min(1.00, data.EditDialog.Opacity);
            }
            set {
                data.EditDialog.Opacity = value;
            }
        }

        [
        Category("Dialog Editor Settings"),
        Description("Keeps the dialog in the dialog designer on top of everything."),
        Browsable(false)
        ]
        public bool AlwaysOnTop {
            get {
                return data.EditDialog.AlwaysOnTop;
            }
            set {
                data.EditDialog.AlwaysOnTop = value;
            }
        }

        #endregion

        #region Serialization helpers

        static protected void DeserializeUnknownNode(object sender, XmlNodeEventArgs e) {
            // MessageBox.Show("Ignoring Unknown Node from settings file: " +   e.Name);
        }
        
        static protected void DeserializeUnknownAttribute(object sender, XmlAttributeEventArgs e) {
            // System.Xml.XmlAttribute attr = e.Attr;
            // MessageBox.Show("Ignoring Unknown Attribute from settings file: " + attr.Name + " = '" + attr.Value + "'");
        }

        #endregion

        #region PropertyAdapterBase overrides

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList propertyDescriptors = new ArrayList();
            foreach (PropertyInfo propInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance)) {
                ArrayList atts = new ArrayList(propInfo.GetCustomAttributes(false));
                propertyDescriptors.Add(new CustomDisplayNamePropertyDescriptor(null, propInfo, (Attribute[]) atts.ToArray(typeof(Attribute)), true));
            }

            return new PropertyDescriptorCollection((PropertyDescriptor[]) propertyDescriptors.ToArray(typeof(PropertyDescriptor)));
        }

        #endregion
   }
}