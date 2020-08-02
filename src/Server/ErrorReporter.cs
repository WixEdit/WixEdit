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
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;

namespace WixEdit {
    public class ErrorReporter {
        protected Dictionary<string, string> properties;
        protected Exception exception;

        public ErrorReporter()
        {
            this.properties = new Dictionary<string, string>();
        }

        public void Report(Exception exception) {

            this.properties.Add("LastModified", File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location).ToString("yyyy-MM-ddTHH:mm:ssZ"));
            this.exception = exception;

            Thread reportThread = new Thread(new ThreadStart(DoReport));
            reportThread.Start();
        }

        protected void DoReport() {
            try {
                Crashes.TrackError(exception, properties);
            } catch (Exception) {
                MessageBox.Show("Error occured while reporting an error.");
                // This happens in a separate thread, don't bother the user with this...
            }
        }
    }
}