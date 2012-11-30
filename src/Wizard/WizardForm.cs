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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Xml;

using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.Wizard
{
    public partial class WizardForm : Form
    {
        WixFiles wixFiles;
        int undoCountBefore = 0;
        List<BaseSheet> sheets = new List<BaseSheet>();
        FinishSheet endSheet = null;
        int currentSheetIndex = -1;

        public WizardForm(WixFiles editWixFiles)
        {
            wixFiles = editWixFiles;
            undoCountBefore = wixFiles.UndoManager.UndoCount;
            
            InitializeComponent();

            string welcomeMessage = "The WixEdit wizard helps you to create or edit MSI files. The wizard allows you to easily add files and functionality to your MSI file.\r\n\r\nFor example:\r\nAdd files, Create shortcuts, Create virual directories, etc.\r\n\r\n\r\nClick \"Next\" to continue or \"Cancel\" to exit the WixEdit wizard.";
            if (!editWixFiles.IsNew)
            {
                welcomeMessage += "\r\n\r\nNote: Existing complex installers may give problems with the wizard.";
            }

            IntroductionSheet welcome = new IntroductionSheet("WixEdit wizard", welcomeMessage, this);
            AddSheet(welcome);

            FileSheet files = new FileSheet(this);
            AddSheet(files);

            SelectTemplatesSheet selectTemplates = new SelectTemplatesSheet(this);
            AddSheet(selectTemplates);

            endSheet = new FinishSheet(this);
            endSheet.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(endSheet);
            endSheet.Visible = false;
        }

        public void RemoveLastAddedTemplate()
        {
            for (int i = sheets.Count - 1; i >= 0; i--)
            {
                BaseSheet sheet = sheets[i];
                if (sheet is IntroductionSheet)
                {
                    sheets.RemoveAt(i);
                    break;
                }

                sheets.RemoveAt(i);
            }
        }

        public void AddTemplate(string templateLocation)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

            doc.Load(templateLocation);
            XmlElement template = (XmlElement)doc.SelectSingleNode("/Template");

            AddTemplate(template);
        }

        public void AddTemplate(XmlElement template)
        {
            IntroductionSheet intro = new IntroductionSheet(template, this);

            AddSheet(intro);

            for (int i = 1; ; i++)
            {
                XmlElement step = (XmlElement)template.SelectSingleNode(String.Format("Step[@Sequence={0}]", i));
                if (step == null)
                {
                    break;
                }

                StepSheet stepSheet = new StepSheet(step, this);

                AddSheet(stepSheet);
            }
        }

        public WixFiles WixFiles
        {
            get { return wixFiles; }
            set { wixFiles = value; }
        }

        private void AddSheet(BaseSheet sheet)
        {
            sheets.Add(sheet);

            sheet.Dock = DockStyle.Fill;
            if (sheets.Count > 1)
            {
                sheet.Visible = false;
            }
            else
            {
                currentSheetIndex = 0;
                backButton.Enabled = false;
            }

            contentPanel.Controls.Add(sheet);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // Undo all our actions.
            while (undoCountBefore != WixFiles.UndoManager.UndoCount)
            {
                WixFiles.UndoManager.Undo();
            }

            // Redo should be cleared here.
            WixFiles.UndoManager.ClearRedo();

            this.Close();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (currentSheetIndex == sheets.Count)
            {
                // This is the finish sheet, we're done.
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                if (sheets[currentSheetIndex].OnNext() == false)
                {
                    MessageBox.Show("The values entered are not all correct.");

                    return;
                }

                sheets[currentSheetIndex].Visible = false;
                currentSheetIndex++;
                if (currentSheetIndex == sheets.Count)
                {
                    endSheet.OnShow();
                    endSheet.Visible = true;
                    nextButton.Text = "Finish";
                    cancelButton.Enabled = false;
                }
                else
                {
                    sheets[currentSheetIndex].OnShow();
                    sheets[currentSheetIndex].Visible = true;
                }

                backButton.Enabled = true;
            }

            nextButton.Focus();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (currentSheetIndex <= 0)
            {
                // Hmmm...
            }
            else
            {
                if (currentSheetIndex == sheets.Count)
                {
                    endSheet.Visible = false;
                    cancelButton.Enabled = true;
                }
                else
                {
                    if (sheets[currentSheetIndex].OnBack() == false)
                    {
                        MessageBox.Show("Not able to go back.");

                        return;
                    }

                    sheets[currentSheetIndex].Visible = false;
                }

                sheets[--currentSheetIndex].Visible = true;
                sheets[currentSheetIndex].UndoNext();
                if (currentSheetIndex == 0)
                {
                    backButton.Enabled = false;
                }

                nextButton.Text = "Next";
            }
        }
    }
}