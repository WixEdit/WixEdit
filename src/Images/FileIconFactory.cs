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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace WixEdit.Images
{
    /// <summary>
    /// Gets icon of file
    /// </summary>
    public class FileIconFactory
    {
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_LINKOVERLAY = 0x8000;

        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        private static Hashtable extensionList = new Hashtable();

        [StructLayout(LayoutKind.Sequential)]
        private struct ShFileInfo
        {
            public System.IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            public string szDisplayName;
            public string szTypeName;
        };

        [DllImport("User32.dll")]
        private static extern int DestroyIcon(System.IntPtr hIcon);

        [DllImport("Shell32.dll")]
        private static extern System.IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi, uint cbFileInfo, uint uFlags);

        public static Icon GetFileIcon(string filePath)
        {
            return GetFileIcon(filePath, false);
        }

        public static Icon GetFileIcon(string filePath, bool isLink)
        {
            string ext = Path.GetExtension(filePath).ToUpper();
            if (extensionList.ContainsKey(ext))
            {
                return (Icon)extensionList[ext];
            }

            try
            {
                uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON;
                if (isLink)
                {
                    flags |= SHGFI_LINKOVERLAY;
                }

                ShFileInfo shellFileInfo = new ShFileInfo();

                SHGetFileInfo(filePath, FILE_ATTRIBUTE_NORMAL, ref shellFileInfo, (uint)Marshal.SizeOf(shellFileInfo), (uint)flags);

                if (shellFileInfo.hIcon == IntPtr.Zero)
                {
                    return null;
                }

                Icon icon = (Icon)Icon.FromHandle(shellFileInfo.hIcon).Clone();

                DestroyIcon(shellFileInfo.hIcon);

                extensionList[ext] = icon;

                return icon;
            }
            catch
            {
                extensionList[ext] = null;

                return null;
            }
        }
    }
}