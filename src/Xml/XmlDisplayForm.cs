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
using System.Windows.Forms;
using System.Drawing;

namespace WixEdit.Xml
{
    /// <summary>
    /// A Html browser dialog for showing xml
    /// </summary>
    public class XmlDisplayForm : Form
    {
        protected string url;
        private WebBrowser webBrowser;

        public XmlDisplayForm()
        {
            InitializeComponent();

            Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
        }

        public string Url
        {
            get { return url; }
        }

        private void InitializeComponent()
        {
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowser
            // 
            this.webBrowser.AllowNavigation = false;
            this.webBrowser.AllowWebBrowserDrop = false;
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScriptErrorsSuppressed = true;
            this.webBrowser.Size = new System.Drawing.Size(800, 600);
            this.webBrowser.TabIndex = 2;
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // XmlDisplayForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.webBrowser);
            this.Name = "XmlDisplayForm";
            this.ResumeLayout(false);

        }

        protected override void OnClosed(EventArgs e)
        {
            if (webBrowser != null)
            {
                Controls.Remove(webBrowser);
                webBrowser.Dispose();
                webBrowser = null;
            }
            base.OnClosed(e);
        }

        public void ShowFile(string url)
        {
            webBrowser.Navigate(url);
        }
    }
}