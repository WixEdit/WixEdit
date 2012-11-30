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
    /// Summary description for SearchPanel.
    /// </summary>
    public class SearchPanel : Panel
    {
        protected OutputTextbox outputTextBox;

        private System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();
        private bool isFirstClick = true;
        private bool isSecondClick = true;
        private int milliseconds = 0;

        private int currentSelectionStart = 0;
        private int currentSelectionLength = 0;

        XmlNodeList lastNodes;

        EditorForm editorForm;

        Thread currentProcessThread;
        WixFiles currentWixFiles;
        string currentSearch;
        bool isCancelled = false;

        IconMenuItem editMenu;
        IconMenuItem cancelMenuItem;

        public SearchPanel(EditorForm editorForm, IconMenuItem editMenu)
        {
            this.editorForm = editorForm;
            this.editMenu = editMenu;

            cancelMenuItem = new IconMenuItem();
            cancelMenuItem.Text = "Cancel Find";
            cancelMenuItem.Click += new EventHandler(cancelMenuItem_Click);
            cancelMenuItem.Shortcut = Shortcut.CtrlC;
            cancelMenuItem.ShowShortcut = true;

            InitializeComponent();
        }

        public RichTextBox RichTextBox
        {
            get { return outputTextBox; }
        }

        private void InitializeComponent()
        {
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

        public void Search(WixFiles wixFiles, string search)
        {
            if (IsBusy)
            {
                throw new Exception("OutputPanel is already busy.");
            }

            isCancelled = false;

            editMenu.MenuItems.Add(cancelMenuItem);
            outputTextBox.Cursor = Cursors.WaitCursor;

            currentWixFiles = wixFiles;
            currentSearch = search;

            currentProcessThread = new Thread(new ThreadStart(InternalSearch));
            currentProcessThread.Start();
        }

        private void InternalSearch()
        {
            Clear();

            try
            {
                string searchAttrib = String.Format("//@*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),{0})]", XPathHelper.EscapeXPathInputString(currentSearch.ToLower()));
                string searchElement = String.Format("//*[contains(translate(text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),{0})]", XPathHelper.EscapeXPathInputString(currentSearch.ToLower()));
                lastNodes = currentWixFiles.WxsDocument.SelectNodes(searchAttrib + "|" + searchElement);
                foreach (XmlNode node in lastNodes)
                {
                    if (isCancelled)
                    {
                        break;
                    }
                    OutputResult(currentSearch, node, GetFirstElement(node));
                }

                if (isCancelled)
                {
                    Output("Aborted...", true);
                }
                else
                {
                    Output("--------------------------------", false);
                    Output(String.Format("Found \"{0}\" {1} {2}", currentSearch, lastNodes.Count, (lastNodes.Count == 1) ? "time" : "times"), true);
                }
            }
            catch (Exception ex)
            {
                Output(ex.ToString(), true);
            }

            editMenu.MenuItems.Remove(cancelMenuItem);
            outputTextBox.Cursor = Cursors.IBeam;
        }

        private XmlElement GetFirstElement(XmlNode node)
        {
            XmlNode showableNode = node;
            while (showableNode.NodeType != XmlNodeType.Element)
            {
                if (showableNode.NodeType == XmlNodeType.Attribute)
                {
                    showableNode = ((XmlAttribute)showableNode).OwnerElement;
                }
                else
                {
                    showableNode = showableNode.ParentNode;
                }
            }

            return (XmlElement)showableNode;
        }

        private void OutputResult(string search, XmlNode node, XmlElement element)
        {
            bool isAttribute = (node != element);

            string strValue = null;
            if (isAttribute)
            {
                strValue = node.Value;
            }
            else
            {
                strValue = node.InnerText;
            }

            int startPos = strValue.ToLower().IndexOf(search.ToLower());

            string firstPart = strValue.Substring(0, startPos).Replace("\\", "\\\\");
            string secondPart = strValue.Substring(startPos, search.Length).Replace("\\", "\\\\");
            string thirdPart = strValue.Substring(startPos + search.Length).Replace("\\", "\\\\");

            strValue = String.Format("{0}\\b {1}\\b0 {2}", firstPart, secondPart, thirdPart);

            if (isAttribute)
            {
                OutputRaw(String.Format("{0}/@{1} = '{2}'\\par\r\n", element.Name, node.Name, strValue));
            }
            else
            {
                if (element.HasAttribute("Id"))
                {
                    OutputRaw(String.Format("{0}[@Id='{1}'] = '{2}'\\par\r\n", element.Name, element.Attributes["Id"].Value, strValue));
                }
                else
                {
                    OutputRaw(String.Format("{0} = '{1}'\\par\r\n", element.Name, strValue));
                }
            }
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
                else if (isSecondClick)
                { // This is the second mouse click.
                    isSecondClick = false;
                    // Verify that the mouse click is within the double click
                    // rectangle and is within the system-defined double 
                    // click period.
                    if (milliseconds < SystemInformation.DoubleClickTime)
                    {
                        OpenLine(e.X, e.Y);
                    }
                }
            }
        }

        int lastLineY = -1;
        public bool HasResultSelected
        {
            get
            {
                return lastNodes != null;
            }
        }

        public void FindNext()
        {
            if (lastLineY >= 0)
            {
                int charIndex = outputTextBox.GetCharIndexFromPosition(new Point(1, lastLineY));
                int line = outputTextBox.GetLineFromCharIndex(charIndex) + 1;
                int newCharIndex = outputTextBox.GetFirstCharIndexFromLine(line);

                Point p = outputTextBox.GetPositionFromCharIndex(newCharIndex);

                OpenLine(p.X, p.Y);
            }
            else
            {
                OpenLine(1, 1);
            }
        }

        public void FindPrev()
        {
            if (lastLineY >= 0)
            {
                int charIndex = outputTextBox.GetCharIndexFromPosition(new Point(1, lastLineY));
                int line = outputTextBox.GetLineFromCharIndex(charIndex) - 1;
                if (line >= 0)
                {
                    int newCharIndex = outputTextBox.GetFirstCharIndexFromLine(line);

                    Point p = outputTextBox.GetPositionFromCharIndex(newCharIndex);

                    OpenLine(p.X, p.Y);
                }
                else
                {
                    OpenLine(-1, -1);
                }
            }
            else
            {
                int line = outputTextBox.GetLineFromCharIndex(outputTextBox.Text.Length) - 3;
                if (line >= 0)
                {
                    int newCharIndex = outputTextBox.GetFirstCharIndexFromLine(line);

                    Point p = outputTextBox.GetPositionFromCharIndex(newCharIndex);

                    OpenLine(p.X, p.Y);
                }
            }
        }

        private void OpenLine(int x, int y)
        {
            if (lastNodes == null)
            {
                return;
            }

            // Obtain the character index at which the mouse cursor was clicked at.
            int currentIndex = outputTextBox.GetCharIndexFromPosition(new Point(x, y));
            int currentLine = 0;
            int beginLineIndex = 0;

            foreach (string line in outputTextBox.Lines)
            {
                if (beginLineIndex <= currentIndex && currentIndex < beginLineIndex + line.Length + 1)
                {
                    break;
                }
                beginLineIndex += line.Length + 1;
                currentLine++;
            }

            int lineCount = outputTextBox.Lines.Length;

            if (currentLine == 0)
            {
                beginLineIndex = 0;
            }

            outputTextBox.SuspendLayout();

            if (currentSelectionStart + currentSelectionLength > 0)
            {
                int oldSelectionStart = outputTextBox.SelectionStart;
                int oldSelectionLength = outputTextBox.SelectionLength;

                outputTextBox.Select(currentSelectionStart, currentSelectionLength);
                outputTextBox.SelectionBackColor = Color.White;
                outputTextBox.SelectionColor = Color.Black;

                currentSelectionStart = 0;
                currentSelectionLength = 0;

                if (currentLine <= lastNodes.Count)
                {
                    outputTextBox.Select(oldSelectionStart, oldSelectionLength);
                }
            }



            if (currentLine < lastNodes.Count && y > 0)
            {
                lastLineY = y;

                currentSelectionStart = beginLineIndex;
                currentSelectionLength = outputTextBox.Lines[currentLine].Length + 1;

                outputTextBox.Select(beginLineIndex, outputTextBox.Lines[currentLine].Length + 1);
                outputTextBox.SelectionBackColor = Color.DarkBlue; //SystemColors.Highlight; // HighLight colors seem not to be working.
                outputTextBox.SelectionColor = Color.White; //SystemColors.HighlightText;

                outputTextBox.Select(beginLineIndex, 0);

                editorForm.ShowNode(lastNodes[currentLine]);
            }
            else
            {
                lastLineY = -1;
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
                isSecondClick = true;

                milliseconds = 0;
            }
        }

        private void Output(string message, bool bold)
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

            OutputRaw(output);
        }

        private void OutputRaw(string output)
        {
            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                                            @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.Focus();
            outputTextBox.ScrollToCaret();
        }

        public void Clear()
        {
            outputTextBox.Text = "";
            lastNodes = null;
            lastLineY = -1;
        }

        public void Cancel()
        {
            isCancelled = true;
        }

        private void cancelMenuItem_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }
    }
}
