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
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;

namespace WixEdit.Settings {
    [DescriptionAttribute("The directory with Wix binaries.")]
    public class BinDirectoryStructure {
        private WixEditSettings.WixEditData wixEditData;
        public BinDirectoryStructure(WixEditSettings.WixEditData data) {
            wixEditData = data;
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("dark.exe |dark.exe"),
        Description("The location of the The Windows Installer XML decompiler (dark)")
        ]
        public string Dark {
            get {
                if (wixEditData.DarkLocation == null || wixEditData.DarkLocation.Length == 0) {
                    if (wixEditData.BinDirectory == null || wixEditData.BinDirectory.Length == 0) {
                        return String.Empty;
                    }
                    return Path.Combine(wixEditData.BinDirectory, "dark.exe");
                } else {
                    return wixEditData.DarkLocation;
                }
            }
            set { wixEditData.DarkLocation = value; }
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("light.exe |light.exe"),
        Description("The location of the The Windows Installer XML linker (light)")
        ]
        public string Light {
            get {
                if (wixEditData.LightLocation == null || wixEditData.LightLocation.Length == 0) {
                    if (wixEditData.BinDirectory == null || wixEditData.BinDirectory.Length == 0) {
                        return String.Empty;
                    }
                    return Path.Combine(wixEditData.BinDirectory, "light.exe");
                } else {
                    return wixEditData.LightLocation;
                }
            }
            set { wixEditData.LightLocation = value; }
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("candle.exe |candle.exe"),
        Description("The location of the The Windows Installer XML compiler (candle)")
        ]
        public string Candle {
            get {
                if (wixEditData.CandleLocation == null || wixEditData.CandleLocation.Length == 0) {
                    if (wixEditData.BinDirectory == null || wixEditData.BinDirectory.Length == 0) {
                        return String.Empty;
                    }
                    return Path.Combine(wixEditData.BinDirectory, "candle.exe");
                } else {
                    return wixEditData.CandleLocation;
                }
            }
            set { wixEditData.CandleLocation = value; }
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(BinDirectoryStructureEditor), typeof(System.Drawing.Design.UITypeEditor)),
        Description("The location of the The Windows Installer XML Xml Schema Definitions")
        ]
        public string Xsds {
            get {
                if (wixEditData.XsdsLocation == null || wixEditData.XsdsLocation.Length == 0) {
                    if (wixEditData.BinDirectory == null || wixEditData.BinDirectory.Length == 0) {
                        return String.Empty;
                    }
                    DirectoryInfo retSubOfBin = new DirectoryInfo(Path.Combine(wixEditData.BinDirectory, "doc"));
                    DirectoryInfo retEqualAtBin = new DirectoryInfo(Path.Combine(retSubOfBin.Parent.Parent.FullName, "doc"));
                    string ret = retSubOfBin.FullName;
                    if (retEqualAtBin.Exists) {
                        ret = retEqualAtBin.FullName;
                    }

                    return ret;
                } else {
                    return wixEditData.XsdsLocation;
                }
            }
            set { wixEditData.XsdsLocation = value; }
        }

        public bool HasSameBinDirectory() {
            if (wixEditData.CandleLocation == null && wixEditData.DarkLocation == null && wixEditData.LightLocation == null && wixEditData.XsdsLocation == null) {
                return true;
            }

            if (Candle == null || Dark == null || Light == null || Xsds == null) {
                return false;
            }

            return (new FileInfo(Candle).Directory.FullName == new FileInfo(Dark).Directory.FullName && 
                    new FileInfo(Candle).Directory.FullName == new FileInfo(Light).Directory.FullName && 
                    Xsds.StartsWith(new FileInfo(Candle).Directory.FullName));
        }

        /// <summary>
        /// Not showing in property grid.
        /// </summary>
        [Browsable(false)]
        public string BinDirectory {
            get {
                return wixEditData.BinDirectory;
            }
            set {
                wixEditData.BinDirectory = value;
            }
        }

        public class BinDirectoryExpandableObjectConverter : ExpandableObjectConverter {
            public override bool CanConvertTo(ITypeDescriptorContext context,
                System.Type destinationType) {
                if (destinationType == typeof(BinDirectoryStructure))
                    return true;
                
                return base.CanConvertTo(context, destinationType);
            }
            
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType) {
                if (destinationType == typeof(System.String) && 
                    value is BinDirectoryStructure){             
                    BinDirectoryStructure bd = (BinDirectoryStructure)value;

                    if (bd.HasSameBinDirectory()) {
                        if (bd.Candle == String.Empty || bd.Candle == null) {
                            return null;
                        } else {
                            bd.BinDirectory = new FileInfo(bd.Candle).Directory.FullName;
                            return bd.BinDirectory;
                        }
                    } else {
                        return "...";
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
            
            public override bool CanConvertFrom(ITypeDescriptorContext context,
                System.Type sourceType) {
                if (sourceType == typeof(string))
                    return true;
                
                return false;
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                WixEditSettings.WixEditData data = WixEditSettings.Instance.GetInternalDataStructure();
                data.BinDirectory = value as string;
                data.CandleLocation = String.Empty;
                data.LightLocation = String.Empty;
                data.DarkLocation = String.Empty;
                data.XsdsLocation = String.Empty;

                return new BinDirectoryStructure(data);
            }
        }
    }
}