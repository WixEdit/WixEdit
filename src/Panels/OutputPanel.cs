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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Xsl;

using WixEdit.Controls;
using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.Panels
{
    /// <summary>
    /// Summary description for OutputPanel.
    /// </summary>
    public class OutputPanel : Panel
    {
        protected OutputTextbox outputTextBox;
        protected Process activeProcess;

        private System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();
        private bool isFirstClick = true;
        private int milliseconds = 0;

        private int currentSelectionStart = 0;
        private int currentSelectionLength = 0;

        XmlDisplayForm xmlDisplayForm = new XmlDisplayForm();

        Thread currentProcessThread;
        ProcessStartInfo currentProcessStartInfo;
        ProcessStartInfo[] currentProcessStartInfos;
        string currentLogFile;
        bool isCancelled = false;

        WixFiles wixFiles = null;

        EditorForm editorForm;

        IconMenuItem buildMenu;
        IconMenuItem cancelMenuItem;

        public delegate void OnCompleteDelegate(bool isCancelled);
        private delegate void DelegateClearRtf();
        private delegate void DelegateOutput(String line);
        private delegate void DelegateOutputLine(string message, bool bold);
        private delegate void DelegateOutputStart(ProcessStartInfo processStartInfo, DateTime start);
        private delegate void DelegateOutputDone(Process process, DateTime start);
        private delegate void DelegateProcessDone();

        OnCompleteDelegate onCompletedOutput;
        DelegateClearRtf invokeClearRTF;
        DelegateOutput invokeOutput;
        DelegateOutputLine invokeOutputLine;
        DelegateOutputStart invokeOutputStart;
        DelegateOutputDone invokeOutputDone;
        DelegateProcessDone invokeProcessDone;

        public OutputPanel(EditorForm editorForm, IconMenuItem buildMenu)
        {
            this.editorForm = editorForm;
            this.buildMenu = buildMenu;

            TabStop = true;

            outputTextBox = new OutputTextbox();

            outputTextBox.Dock = DockStyle.Fill;
            outputTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            outputTextBox.WordWrap = WixEditSettings.Instance.WordWrapInResultsPanel;
            outputTextBox.AllowDrop = false;

            Controls.Add(outputTextBox);

            outputTextBox.TabStop = true;
            outputTextBox.HideSelection = false;

            outputTextBox.MouseUp += new MouseEventHandler(outputTextBox_MouseDown);

            doubleClickTimer.Interval = 100;
            doubleClickTimer.Tick += new EventHandler(doubleClickTimer_Tick);


            cancelMenuItem = new IconMenuItem();
            cancelMenuItem.Text = "Cancel Action";
            cancelMenuItem.Click += new EventHandler(cancelMenuItem_Click);
            cancelMenuItem.Shortcut = Shortcut.CtrlC;
            cancelMenuItem.ShowShortcut = true;

            invokeClearRTF = new DelegateClearRtf(ClearRtf);
            invokeOutput = new DelegateOutput(Output);
            invokeOutputLine = new DelegateOutputLine(OutputLine);
            invokeOutputStart = new DelegateOutputStart(OutputStart);
            invokeOutputDone = new DelegateOutputDone(OutputDone);
            invokeProcessDone = new DelegateProcessDone(ProcessDone);
        }

        public RichTextBox RichTextBox
        {
            get { return outputTextBox; }
        }

        private void outputTextBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // This is the first mouse click.
                if (isFirstClick)
                {
                    isFirstClick = false;

                    // Start the double click timer.
                    doubleClickTimer.Start();
                }
                else
                { // This is the second mouse click.
                    // Verify that the mouse click is within the double click rectangle and 
                    // is within the system-defined double click period.
                    if (milliseconds < SystemInformation.DoubleClickTime)
                    {
                        OpenLine(e.X, e.Y);
                    }
                }
            }
        }


        void doubleClickTimer_Tick(object sender, EventArgs e)
        {
            milliseconds += 100;

            // The timer has reached the double click time limit.
            if (milliseconds >= SystemInformation.DoubleClickTime)
            {
                doubleClickTimer.Stop();

                // Allow the MouseDown event handler to process clicks again.
                isFirstClick = true;
                milliseconds = 0;
            }
        }

        private void OpenLine(int x, int y)
        {
            int start = outputTextBox.SelectionStart;
            int length = outputTextBox.SelectionLength;

            // Obtain the character index at which the mouse cursor was clicked at.
            int currentIndex = outputTextBox.GetCharIndexFromPosition(new Point(x, y));
            int currentLine = 0;
            int beginLineIndex = 0;
            foreach (string line in outputTextBox.Lines)
            {
                if (beginLineIndex < currentIndex && currentIndex < beginLineIndex + line.Length + 1)
                {
                    break;
                }
                beginLineIndex += line.Length + 1;
                currentLine++;
            }

            int lineCount = outputTextBox.Lines.Length;

            if (lineCount <= currentLine || outputTextBox.Lines[currentLine] == null)
            {
                return;
            }

            if (currentLine == 0)
            {
                beginLineIndex = 0;
            }

            outputTextBox.SuspendLayout();

            if (currentSelectionStart + currentSelectionLength > 0)
            {
                outputTextBox.Select(currentSelectionStart, currentSelectionLength);
                outputTextBox.SelectionBackColor = Color.White;
                outputTextBox.SelectionColor = Color.Black;
            }

            currentSelectionStart = beginLineIndex;
            currentSelectionLength = outputTextBox.Lines[currentLine].Length + 1;

            string text = outputTextBox.Lines[currentLine];

            int bracketEnd = text.IndexOf(") : ");
            int bracketStart = -1;
            if (bracketEnd > -1)
            {
                bracketStart = text.LastIndexOf("(", bracketEnd, bracketEnd);
            }

            if (bracketStart == -1 || bracketEnd == -1)
            {
                // outputTextBox.Select(start, length);
                // outputTextBox.Select(beginLineIndex, 0);
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0;
                currentSelectionLength = 0;

                return;
            }

            string fileName = text.Substring(0, bracketStart);

            int lineNumber = 0;
            try
            {
                lineNumber = Int32.Parse(text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1));
            }
            catch (Exception)
            {
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0;
                currentSelectionLength = 0;

                return;
            }

            string message = text.Substring(bracketEnd + 1);

            if (File.Exists(fileName) == false)
            {
                // outputTextBox.Select(start, length);
                // outputTextBox.Select(beginLineIndex, 0);
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0;
                currentSelectionLength = 0;

                return;
            }

            // Finding the right anchor only works correctly when there are not multiple elements on one line.
            int anchorCount = 0;
            int numberOfElements = 1;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    XmlTextReader reader = new XmlTextReader(sr);
                    int reads = 0;
                    int readElement = 0;

                    // Parse the XML and display each node.
                    while (reader.Read())
                    {
                        reads++;
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            readElement++;
                        }
                        if (reader.LineNumber == lineNumber)
                        {
                            anchorCount = readElement;
                            while (reader.Read())
                            {
                                if (reader.LineNumber == lineNumber)
                                {
                                    if (reader.NodeType == XmlNodeType.Element)
                                    {
                                        numberOfElements++;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (XmlException)
            {
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0;
                currentSelectionLength = 0;

                return;
            }

            outputTextBox.Select(currentSelectionStart, currentSelectionLength);
            outputTextBox.SelectionBackColor = Color.DarkBlue; //SystemColors.Highlight; // HighLight colors seem not to be working.
            outputTextBox.SelectionColor = Color.White; //SystemColors.HighlightText;
            outputTextBox.Select(beginLineIndex, 0);

            outputTextBox.ResumeLayout();

            if (numberOfElements == 1)
            {
                ShowElement(anchorCount);
            }

            LaunchFile(fileName, anchorCount, numberOfElements, lineNumber, message.Trim());
        }

        protected void ShowElement(int elementCount)
        {
            if (wixFiles == null)
            {
                return;
            }

            try
            {
                XmlNode node = wixFiles.WxsDocument;
                int count = -1;
                while (count < elementCount)
                {
                    if (node.ChildNodes.Count > 0)
                    {
                        node = node.ChildNodes[0];
                    }
                    else
                    {
                        if (node.NextSibling != null)
                        {
                            node = node.NextSibling;
                        }
                        else
                        {
                            XmlNode parentNode = node.ParentNode;
                            while (parentNode != null && parentNode.NextSibling == null)
                            {
                                parentNode = parentNode.ParentNode;
                            }

                            if (parentNode == null)
                            {
                                return;
                            }

                            node = parentNode.NextSibling;
                        }
                    }

                    count++;
                }

                editorForm.ShowNode(node);
            }
            catch
            {
                // Too bad, just ignore.
            }
        }

        protected void LaunchFile(string filename, int anchorNumber, int numberOfAnchors, int lineNumber, string message)
        {
            try
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                using (Stream strm = WixFiles.GetResourceStream("viewWixXml.xsl"))
                {
                    XmlTextReader xr = new XmlTextReader(strm);
                    transform.Load(xr, null, null);
                }

                string outputFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(filename)) + ".html";
                if (
                    (File.Exists(outputFile) &&
                    (File.GetLastWriteTimeUtc(outputFile).CompareTo(File.GetLastWriteTimeUtc(filename)) > 0) &&
                    (File.GetLastWriteTimeUtc(outputFile).CompareTo(File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location)) > 0)) == false
                   )
                {
                    File.Delete(outputFile);
                    transform.Transform(filename, outputFile);
                }

                if (xmlDisplayForm.Visible == false)
                {
                    xmlDisplayForm = new XmlDisplayForm();
                }

                StringBuilder anchorBuilder = new StringBuilder();
                for (int i = 0; i < numberOfAnchors; i++)
                {
                    anchorBuilder.AppendFormat("{0},", anchorNumber + i);
                }
                anchorBuilder.Remove(anchorBuilder.Length - 1, 1);

                xmlDisplayForm.Text = String.Format("{0}({1}) {2}", Path.GetFileName(filename), lineNumber, message);
                xmlDisplayForm.ShowFile(String.Format("{0}?{1}#a{1}", outputFile, anchorBuilder.ToString()));
                xmlDisplayForm.Show();
                xmlDisplayForm.Activate();
            }
            catch (XmlException ex)
            {
                // Invalid XML
                MessageBox.Show("Failed to show XML, is it a valid XML file?\r\n\r\nMessage:\r\n" + ex.Message, "Failed to load XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool IsBusy
        {
            get
            {
                if (currentProcessThread == null)
                {
                    return false;
                }

                return currentProcessThread.IsAlive;
            }
        }

        public void Run(ProcessStartInfo[] processStartInfos)
        {
            Run(processStartInfos, null, null);
        }

        public void Run(ProcessStartInfo[] processStartInfos, WixFiles theWixFiles)
        {
            Run(processStartInfos, theWixFiles, null);
        }

        public void Run(ProcessStartInfo[] processStartInfos, OnCompleteDelegate onComplete)
        {
            Run(processStartInfos, null, onComplete);
        }

        public void Run(ProcessStartInfo[] processStartInfos, WixFiles theWixFiles, OnCompleteDelegate onComplete)
        {
            if (IsBusy)
            {
                throw new WixEditException("OutputPanel is already busy.");
            }

            wixFiles = theWixFiles;
            onCompletedOutput = onComplete;

            isCancelled = false;

            buildMenu.MenuItems.Add(cancelMenuItem);
            outputTextBox.Cursor = Cursors.WaitCursor;

            currentProcessStartInfos = processStartInfos;

            currentProcessThread = new Thread(new ThreadStart(InternalThreadRunMultiple));
            currentProcessThread.Start();
        }

        private void InternalThreadRunMultiple()
        {
            Invoke(invokeClearRTF);

            DateTime start = DateTime.Now;

            foreach (ProcessStartInfo processStartInfo in currentProcessStartInfos)
            {
                DateTime subStart = DateTime.Now;
                Invoke(invokeOutputStart, new object[] { processStartInfo, subStart });

                activeProcess = Process.Start(processStartInfo);

                Thread readStandardOut = new Thread(new ThreadStart(ReadStandardOut));
                Thread readStandardError = new Thread(new ThreadStart(ReadStandardError));

                readStandardOut.Start();
                readStandardError.Start();

                activeProcess.WaitForExit();

                readStandardOut.Join();
                readStandardError.Join();

                if (activeProcess.ExitCode != 0)
                {
                    break;
                }

                if (isCancelled)
                {
                    break;
                }

                Invoke(invokeOutputDone, new object[] { activeProcess, start });
            }

            if (isCancelled)
            {
                Invoke(invokeOutputLine, new object[] { "Aborted...", true });
            }
            else
            {
                Invoke(invokeOutputLine, new object[] { "", true });
                Invoke(invokeOutputLine, new object[] { "----- Finished", true });
                Invoke(invokeOutputLine, new object[] { "", false });

                if (activeProcess.ExitCode != 0)
                {
                    Invoke(invokeOutputLine, new object[] { "Error in " + Path.GetFileNameWithoutExtension(activeProcess.StartInfo.FileName), true });
                }
                else
                {
                    Invoke(
                        invokeOutputLine,
                            new object[] 
								{ 
									String.Format("Finished in: {0} seconds", activeProcess.ExitTime.Subtract(start).Seconds.ToString()), true	
								}
                            );
                }
            }

            Invoke(invokeProcessDone);

            if (onCompletedOutput != null)
            {
                Invoke(onCompletedOutput, new object[] { isCancelled });
            }
        }

        public void Run(ProcessStartInfo processStartInfo)
        {
            Run(processStartInfo, null, null);
        }

        public void Run(ProcessStartInfo processStartInfo, WixFiles theWixFiles)
        {
            Run(processStartInfo, theWixFiles, null);
        }

        public void Run(ProcessStartInfo processStartInfo, OnCompleteDelegate onComplete)
        {
            Run(processStartInfo, null, onComplete);
        }

        public void Run(ProcessStartInfo processStartInfo, WixFiles theWixFiles, OnCompleteDelegate onComplete)
        {
            if (IsBusy)
            {
                throw new WixEditException("OutputPanel is already busy.");
            }

            wixFiles = theWixFiles;
            onCompletedOutput = onComplete;

            invokeClearRTF = new DelegateClearRtf(ClearRtf);
            invokeOutput = new DelegateOutput(Output);
            invokeOutputLine = new DelegateOutputLine(OutputLine);
            invokeOutputStart = new DelegateOutputStart(OutputStart);
            invokeOutputDone = new DelegateOutputDone(OutputDone);
            invokeProcessDone = new DelegateProcessDone(ProcessDone);

            isCancelled = false;

            buildMenu.MenuItems.Add(cancelMenuItem);
            outputTextBox.Cursor = Cursors.WaitCursor;

            currentProcessStartInfo = processStartInfo;

            currentProcessThread = new Thread(new ThreadStart(InternalThreadRunSingle));
            currentProcessThread.Start();
        }

        private void InternalThreadRunSingle()
        {
            DateTime start = DateTime.Now;

            Invoke(invokeOutputStart, new object[] { currentProcessStartInfo, start });

            activeProcess = Process.Start(currentProcessStartInfo);

            Thread readStandardOut = new Thread(new ThreadStart(ReadStandardOut));
            Thread readStandardError = new Thread(new ThreadStart(ReadStandardError));

            readStandardOut.Start();
            readStandardError.Start();

            readStandardOut.Join();
            readStandardError.Join();

            activeProcess.WaitForExit();

            if (isCancelled)
            {
                Invoke(invokeOutputLine, new object[] { "Aborted...", true });
            }
            else
            {
                Invoke(invokeOutputDone, new object[] { activeProcess, start });
            }

            Invoke(invokeProcessDone);

            if (onCompletedOutput != null)
            {
                Invoke(onCompletedOutput, new object[] { isCancelled });
            }
        }

        public void RunWithLogFile(ProcessStartInfo processStartInfo, string logFile)
        {
            if (IsBusy)
            {
                throw new WixEditException("OutputPanel is already busy.");
            }

            isCancelled = false;

            buildMenu.MenuItems.Add(cancelMenuItem);
            outputTextBox.Cursor = Cursors.WaitCursor;

            currentProcessStartInfo = processStartInfo;
            currentLogFile = logFile;

            currentProcessThread = new Thread(new ThreadStart(InternalThreadRunSingleWithLogFile));
            currentProcessThread.Start();
        }

        private void InternalThreadRunSingleWithLogFile()
        {
            DateTime start = DateTime.Now;

            Invoke(invokeOutputStart, new object[] { currentProcessStartInfo, start });

            activeProcess = Process.Start(currentProcessStartInfo);

            while (activeProcess.WaitForExit(100) == false)
            {
                if (File.Exists(currentLogFile))
                {
                    ReadLogFile(currentLogFile);
                    break;
                }
                Application.DoEvents();
            }

            if (isCancelled)
            {
                Invoke(invokeOutputLine, new object[] { "Aborted...", true });
            }
            else
            {
                Invoke(invokeOutputDone, new object[] { activeProcess, start });
            }

            Invoke(invokeProcessDone);

            if (onCompletedOutput != null)
            {
                Invoke(onCompletedOutput, new object[] { isCancelled });
            }
        }

        private void ReadLogFile(string logFile)
        {
            FileInfo log = new FileInfo(logFile);

            if (log.Exists)
            {
                int read = 0;
                string line = null;

                byte[] bytes = new byte[1024];
                char[] chars = new char[1024];

                using (FileStream fs = log.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    TextReader r = new StreamReader(fs);
                    while (isCancelled == false)
                    {
                        if (fs.Position == fs.Length)
                        {
                            if (activeProcess.WaitForExit(200))
                            {
                                if (fs.Position == fs.Length)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }

                        try
                        {
                            read = r.Read(chars, 0, 1024);
                            if (read > 0)
                            {
                                try
                                {
                                    line = null;
                                    line = new String(chars, 0, read);
                                }
                                catch { }

                                if (line != null)
                                {
                                    Invoke(invokeOutput, new object[] { line });
                                }
                            }
                        }
                        catch (IOException)
                        {
                        }

                        Application.DoEvents();
                    }
                }
            }
        }


        private void ReadStandardOut()
        {
            if (activeProcess != null)
            {
                string line;
                using (StreamReader sr = activeProcess.StandardOutput)
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Invoke(invokeOutputLine, new object[] { line, false });
                    }
                }
            }
        }

        private void ReadStandardError()
        {
            if (activeProcess != null)
            {
                string line;
                using (StreamReader sr = activeProcess.StandardError)
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Invoke(invokeOutputLine, new object[] { line, true });
                    }
                }
            }
        }

        private void Output(string message)
        {
            if (message == null || message.Length == 0)
            {
                return;
            }

            string escaped = message.Replace("\\", "\\\\");
            string output = escaped.Replace("\r\n", "\\par\r\n");

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.Focus();
            outputTextBox.ScrollToCaret();
        }

        private void ClearRtf()
        {
            outputTextBox.Rtf = "";
        }

        private void OutputLine(string message, bool bold)
        {
            string output;
            if (message == null || message.Length == 0)
            {
                output = "\\par\r\n";
            }
            else
            {
                string escaped = message.Replace("\\", "\\\\");
                if (bold == false)
                {
                    output = String.Format("{0}\\par\r\n", escaped);
                }
                else
                {
                    output = String.Format("\\b {0}\\b0\\par\r\n", escaped);
                }
            }

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                                            @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.Focus();
            outputTextBox.ScrollToCaret();
        }

        private void OutputStart(ProcessStartInfo processStartInfo, DateTime start)
        {
            OutputLine(String.Format("----- Starting {0} {1} at {2}", processStartInfo.FileName, processStartInfo.Arguments, start), true);
            OutputLine("", true);
        }

        private void OutputDone(Process process, DateTime start)
        {
            OutputLine("", true);
            OutputLine(String.Format("Done in: {0} ms", process.ExitTime.Subtract(start).Milliseconds), true);
            OutputLine("", true);
        }

        private void ProcessDone()
        {
            buildMenu.MenuItems.Remove(cancelMenuItem);
            outputTextBox.Cursor = Cursors.IBeam;
        }

        public void Clear()
        {
            outputTextBox.Text = "";
            wixFiles = null;
        }

        public void Cancel()
        {
            if (DialogResult.Yes == MessageBox.Show("Do you want to stop your current action?", "WixEdit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            {
                isCancelled = true;
                if (activeProcess != null && activeProcess.HasExited == false)
                {
                    try
                    {
                        activeProcess.Kill();
                    }
                    catch { }
                }
            }
        }

        private void cancelMenuItem_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }
    }
}
