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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using WixEdit.About;
using WixEdit.Controls;
using WixEdit.Settings;
using WixEdit.Xml;
using WixEdit.Panels;
using WixEdit.Images;
using WixEdit.Helpers;
using WixEdit.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;
using System.Linq;
using PInvoke;

namespace WixEdit
{
    /// <summary>
    /// The main dialog.
    /// </summary>
    public class EditorForm : Form
    {
        protected OpenFileDialog openWxsFileDialog;

        protected AboutForm splash;
        protected bool splashIsDone = false;
        protected static bool xsdWarningIsDone = false;
        protected EventHandler splashScreenHandler;

        protected Panel mainPanel;
        protected TabButtonControl tabButtonControl;
        protected EditUIPanel editUIPanel;
        protected EditPropertiesPanel editPropertiesPanel;
        protected EditResourcesPanel editResourcesPanel;
        protected EditInstallDataPanel editInstallDataPanel;
        protected EditGlobalDataPanel editGlobalDataPanel;
        protected EditActionsPanel editActionsPanel;
        protected EditCustomTablePanel editCustomTablePanel;

        protected MainMenu mainMenu;
        protected IconMenuItem fileMenu;
        protected IconMenuItem fileNew;
        protected IconMenuItem fileNewEmpty;
        protected IconMenuItem fileLoad;
        protected IconMenuItem fileRecent;
        protected IconMenuItem fileRecentEmpty;
        protected IconMenuItem fileRecentClean;
        protected IconMenuItem fileRecentClear;
        protected IconMenuItem fileSave;
        protected IconMenuItem fileSaveAs;
        protected IconMenuItem fileClose;
        protected IconMenuItem fileSeparator;
        protected IconMenuItem fileExit;
        protected IconMenuItem editMenu;
        protected IconMenuItem editUndo;
        protected IconMenuItem editRedo;

        protected IconMenuItem helpStateBrowser;
        protected Assembly stateBrowserAssm;

        protected IconMenuItem editFind;
        protected IconMenuItem editFindNext;
        protected IconMenuItem editFindPrev;
        protected IconMenuItem editWizard;
        protected IconMenuItem toolsMenu;
        protected IconMenuItem toolsExternal;
        protected IconMenuItem toolsOptions;
        protected IconMenuItem buildProjectSettings;
        protected IconMenuItem buildWixCompile;
        protected IconMenuItem buildWixInstall;
        protected IconMenuItem buildWixUninstall;
        protected IconMenuItem buildMenu;
        protected IconMenuItem helpMenu;
        protected IconMenuItem helpAbout;
        protected IconMenuItem helpTutorial;
        protected IconMenuItem helpMSIReference;
        protected IconMenuItem helpWiXReference;

        protected ResultsPanel resultsPanel;
        protected SearchPanel searchPanel;
        protected OutputPanel outputPanel;
        protected Splitter resultsSplitter;

        protected bool fileIsDirty;

        protected int oldTabIndex = -1;

        const int panelCount = 7;
        BasePanel[] panels = new BasePanel[panelCount];

        internal WixFiles wixFiles;

        protected static ArrayList formInstances = new ArrayList();

        string decompiledWxs;



        public EditorForm()
        {
            InitializeComponent();

            if (xsdWarningIsDone == false && WixFiles.CheckForXsd() == false)
            {
                xsdWarningIsDone = true;

                if (String.IsNullOrEmpty(WixEditSettings.Instance.WixBinariesDirectory.BinDirectory) ||
                    Directory.Exists(WixEditSettings.Instance.WixBinariesDirectory.BinDirectory) == false)
                {
                    MessageBox.Show("Windows Installer XML (WiX) Toolset installation is required to run WixEdit.\r\n\r\nThe WiX installation can be downloaded from http://wixtoolset.org/. Please download and install WiX and specify the install location in the WixEdit options.", "Missing WiX");
                }
                else
                {
                    MessageBox.Show("Please check your WiX installation!\r\n\r\nCannot find Wix.xsd! It should be located in the 'doc' subdirectory of your WiX installation. Please check your WiX installation and the XSDs location in the WixEdit options. This file is required to determine the correct xml schema for your version of WiX.", "Missing Wix.xsd");
                }
            }
        }

        public EditorForm(string fileToOpen)
        {
            InitializeComponent();

            if (xsdWarningIsDone == false && WixFiles.CheckForXsd() == false)
            {
                xsdWarningIsDone = true;

                if (String.IsNullOrEmpty(WixEditSettings.Instance.WixBinariesDirectory.BinDirectory) ||
                    Directory.Exists(WixEditSettings.Instance.WixBinariesDirectory.BinDirectory) == false)
                {
                    MessageBox.Show("Windows Installer XML (WiX) Toolset installation is required to run WixEdit.\r\n\r\nThe WiX installation can be downloaded from http://wixtoolset.org/. Please download and install WiX and specify the install location in the WixEdit options.", "Missing WiX");
                }
                else
                {
                    MessageBox.Show("Please check your WiX installation!\r\n\r\nCannot find Wix.xsd! It should be located in the 'doc' subdirectory of your WiX installation. Please check your WiX installation and the XSDs location in the WixEdit options. This file is required to determine the correct xml schema for your version of WiX.", "Missing Wix.xsd");
                }

                return;
            }

            FileInfo xmlFileInfo = new FileInfo(fileToOpen);
            if (xmlFileInfo.Exists)
            {
                LoadWxsFile(xmlFileInfo);
            }
        }

        private void InitializeComponent()
        {
            formInstances.Add(this);

            Text = "WiX Edit";
            Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
            ClientSize = new Size(800, 600);
            MinimumSize = new Size(250, 200);

            //allow drag&drop of files
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(EditorForm_DragEnter);
            this.DragDrop += new DragEventHandler(EditorForm_DragDrop);

            openWxsFileDialog = new OpenFileDialog();

            mainMenu = new MainMenu();
            fileMenu = new IconMenuItem();
            fileNew = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            fileNewEmpty = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            fileLoad = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.open.bmp")));
            fileRecent = new IconMenuItem();
            fileRecentEmpty = new IconMenuItem();
            fileRecentClean = new IconMenuItem();
            fileRecentClear = new IconMenuItem();
            fileSave = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.save.bmp")));
            fileSaveAs = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.saveas.bmp")));
            fileClose = new IconMenuItem();
            fileSeparator = new IconMenuItem("-");
            fileExit = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.exit.bmp")));

            fileNew.Text = "&New";
            fileNew.Click += new EventHandler(fileNew_Click);
            fileNew.Shortcut = Shortcut.CtrlN;
            fileNew.ShowShortcut = true;

            fileNewEmpty.Text = "New empty file";
            fileNewEmpty.Click += new EventHandler(fileNewEmpty_Click);
            fileNewEmpty.ShowShortcut = true;

            fileLoad.Text = "&Open";
            fileLoad.Click += new EventHandler(fileLoad_Click);
            fileLoad.Shortcut = Shortcut.CtrlO;
            fileLoad.ShowShortcut = true;

            fileRecentEmpty.Text = "Empty";
            fileRecentEmpty.Enabled = false;
            fileRecentEmpty.ShowShortcut = true;

            fileRecent.Text = "&Reopen";
            fileRecent.Popup += new EventHandler(fileRecent_Popup);
            fileRecent.ShowShortcut = true;
            fileRecent.MenuItems.Add(0, fileRecentEmpty);

            fileRecentClean.Text = "Remove obsolete";
            fileRecentClean.Click += new EventHandler(recentFileClean_Click);
            fileRecentClean.ShowShortcut = true;

            fileRecentClear.Text = "Remove all";
            fileRecentClear.Click += new EventHandler(recentFileClear_Click);
            fileRecentClear.ShowShortcut = true;

            fileSave.Text = "&Save";
            fileSave.Click += new EventHandler(fileSave_Click);
            fileSave.Enabled = false;
            fileSave.Shortcut = Shortcut.CtrlS;
            fileSave.ShowShortcut = true;

            fileSaveAs.Text = "Save &As";
            fileSaveAs.Click += new EventHandler(fileSaveAs_Click);
            fileSaveAs.Enabled = false;
            fileSaveAs.ShowShortcut = false;

            fileIsDirty = false;

            fileClose.Text = "&Close";
            fileClose.Click += new EventHandler(fileClose_Click);
            fileClose.Enabled = false;
            fileClose.Shortcut = Shortcut.CtrlW;
            fileClose.ShowShortcut = true;

            fileExit.Text = "&Exit";
            fileExit.Click += new EventHandler(fileExit_Click);
            fileExit.ShowShortcut = true;

            fileMenu.Text = "&File";
            fileMenu.Popup += new EventHandler(fileMenu_Popup);
            fileMenu.MenuItems.Add(0, fileNew);
            fileMenu.MenuItems.Add(1, fileNewEmpty);
            fileMenu.MenuItems.Add(2, fileLoad);
            fileMenu.MenuItems.Add(3, fileRecent);
            fileMenu.MenuItems.Add(4, fileSave);
            fileMenu.MenuItems.Add(5, fileSaveAs);
            fileMenu.MenuItems.Add(6, fileClose);
            fileMenu.MenuItems.Add(7, fileSeparator);
            fileMenu.MenuItems.Add(8, fileExit);

            mainMenu.MenuItems.Add(0, fileMenu);


            editMenu = new IconMenuItem();
            editUndo = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.undo.bmp")));
            editRedo = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.redo.bmp")));
            editFind = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.find.bmp")));
            editFindNext = new IconMenuItem();
            editFindPrev = new IconMenuItem();
            editWizard = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.wizard.bmp")));

            if (wixFiles == null ||
                WixEditSettings.Instance.ExternalXmlEditor == null ||
                File.Exists(WixEditSettings.Instance.ExternalXmlEditor) == false)
            {
                toolsExternal = new IconMenuItem();
            }
            else
            {
                Icon ico = FileIconFactory.GetFileIcon(WixEditSettings.Instance.ExternalXmlEditor);
                toolsExternal = new IconMenuItem(ico);
            }

            editUndo.Text = "&Undo";
            editUndo.Click += new EventHandler(editUndo_Click);
            editUndo.Shortcut = Shortcut.CtrlZ;
            editUndo.ShowShortcut = true;

            editRedo.Text = "&Redo";
            editRedo.Click += new EventHandler(editRedo_Click);
            editRedo.Shortcut = Shortcut.CtrlY;
            editRedo.ShowShortcut = true;

            editFind.Text = "&Find";
            editFind.Click += new EventHandler(editFind_Click);
            editFind.Shortcut = Shortcut.CtrlF;
            editFind.ShowShortcut = true;

            editFindNext.Text = "Find &Next";
            editFindNext.Click += new EventHandler(editFindNext_Click);
            editFindNext.Shortcut = Shortcut.F3;
            editFindNext.ShowShortcut = true;

            editFindPrev.Text = "Find &Previous";
            editFindPrev.Click += new EventHandler(editFindPrev_Click);
            editFindPrev.Shortcut = Shortcut.ShiftF3;
            editFindPrev.ShowShortcut = true;

            editWizard.Text = "Add Functionality";
            editWizard.Click += new EventHandler(editWizard_Click);
            editWizard.ShowShortcut = true;

            editMenu.Text = "&Edit";
            editMenu.Popup += new EventHandler(editMenu_Popup);
            editMenu.MenuItems.Add(0, editUndo);
            editMenu.MenuItems.Add(1, editRedo);
            editMenu.MenuItems.Add(2, new IconMenuItem("-"));
            editMenu.MenuItems.Add(3, editFind);
            editMenu.MenuItems.Add(4, editFindNext);
            editMenu.MenuItems.Add(5, editFindPrev);
            editMenu.MenuItems.Add(6, new IconMenuItem("-"));
            editMenu.MenuItems.Add(7, editWizard);

            mainMenu.MenuItems.Add(1, editMenu);


            toolsMenu = new IconMenuItem();
            toolsOptions = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.options.bmp")));
            buildProjectSettings = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.settings.bmp")));
            buildWixCompile = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.compile.bmp")));
            buildWixInstall = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.uninstall.bmp")));
            buildWixUninstall = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.install.bmp")));
            buildMenu = new IconMenuItem();

            buildWixCompile.Text = "&Build MSI setup package";
            buildWixCompile.Click += new EventHandler(buildWixCompile_Click);
            buildWixCompile.Enabled = false;
            buildWixCompile.Shortcut = Shortcut.CtrlB;
            buildWixCompile.ShowShortcut = true;

            buildWixInstall.Text = "&Install MSI setup package";
            buildWixInstall.Click += new EventHandler(buildWixInstall_Click);
            buildWixInstall.Enabled = false;
            buildWixInstall.Shortcut = Shortcut.F5;
            buildWixInstall.ShowShortcut = true;

            buildWixUninstall.Text = "&Uninstall MSI setup package";
            buildWixUninstall.Click += new EventHandler(buildWixUninstall_Click);
            buildWixUninstall.Enabled = false;
            buildWixUninstall.Shortcut = Shortcut.ShiftF5;
            buildWixUninstall.ShowShortcut = true;

            buildProjectSettings.Text = "&Build Settings";
            buildProjectSettings.Click += new EventHandler(buildProjectSettings_Click);
            buildProjectSettings.Enabled = false;

            buildMenu.Text = "&Build";
            buildMenu.Popup += new EventHandler(buildMenu_Popup);
            buildMenu.MenuItems.Add(0, buildWixCompile);
            buildMenu.MenuItems.Add(1, new IconMenuItem("-"));
            buildMenu.MenuItems.Add(2, buildWixInstall);
            buildMenu.MenuItems.Add(3, buildWixUninstall);
            buildMenu.MenuItems.Add(4, new IconMenuItem("-"));
            buildMenu.MenuItems.Add(5, buildProjectSettings);

            mainMenu.MenuItems.Add(2, buildMenu);

            toolsOptions.Text = "&Options";
            toolsOptions.Click += new EventHandler(toolsOptions_Click);

            toolsExternal.Text = "Launch &External Editor";
            toolsExternal.Click += new EventHandler(toolsExternal_Click);
            toolsExternal.Shortcut = Shortcut.CtrlE;
            toolsExternal.ShowShortcut = true;

            toolsMenu.Text = "&Tools";
            toolsMenu.Popup += new EventHandler(toolsMenu_Popup);
            toolsMenu.MenuItems.Add(0, toolsExternal);
            toolsMenu.MenuItems.Add(1, new IconMenuItem("-"));
            toolsMenu.MenuItems.Add(2, toolsOptions);

            mainMenu.MenuItems.Add(3, toolsMenu);


            helpMenu = new IconMenuItem();
            helpTutorial = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.web.bmp")));
            helpMSIReference = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.web.bmp")));
            helpWiXReference = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.wix.bmp")));
            helpAbout = new IconMenuItem(new Icon(WixFiles.GetResourceStream("dialog.source.ico"), 16, 16));
            helpStateBrowser = new IconMenuItem();

            helpTutorial.Text = "WiX Tutorial";
            helpTutorial.Click += new EventHandler(helpTutorial_Click);

            helpMSIReference.Text = "Windows Installer Reference";
            helpMSIReference.Click += new EventHandler(helpMSIReference_Click);

            helpWiXReference.Text = "WiX Reference";
            helpWiXReference.Click += new EventHandler(helpWiXReference_Click);

            helpAbout.Text = "&About";
            helpAbout.Click += new EventHandler(helpAbout_Click);

            helpMenu.Text = "&Help";
            helpMenu.MenuItems.Add(helpTutorial);
            helpMenu.MenuItems.Add(helpMSIReference);
            string xsdDir = WixEditSettings.Instance.WixBinariesDirectory.Xsds;
            if (xsdDir != String.Empty &&
                File.Exists(Path.Combine(xsdDir, "WiX.chm")))
            {
                helpMenu.MenuItems.Add(helpWiXReference);
            }
            helpMenu.MenuItems.Add(new IconMenuItem("-"));
            helpMenu.MenuItems.Add(helpAbout);

            mainMenu.MenuItems.Add(helpMenu);

            // Object browser for debug purposes, just drop the statebrowser assembly next to 
            // the WixEdit assembly and select the "Browse Application State" in the help menu.
            // See http://sliver.com/dotnet/statebrowser/
            try
            {
                string location = Assembly.GetExecutingAssembly().Location;
                FileInfo wixEditExe = new FileInfo(location);
                string tryPath = Path.Combine(wixEditExe.Directory.Parent.FullName, @"tools\StateBrowser\sliver.Windows.Forms.StateBrowser.dll");
                if (!File.Exists(tryPath))
                {
                    tryPath = Path.Combine(wixEditExe.Directory.Parent.Parent.FullName, @"tools\StateBrowser\sliver.Windows.Forms.StateBrowser.dll");
                }

                if (!File.Exists(tryPath))
                {
                    stateBrowserAssm = Assembly.Load("sliver.windows.forms.statebrowser, Version=1.5.0.0, Culture=neutral, PublicKeyToken=34afe62596d00324, Custom=null");
                }
                else
                {
                    stateBrowserAssm = Assembly.LoadFile(tryPath);
                }
            }
            catch (Exception) { }

            if (stateBrowserAssm != null)
            {
                helpStateBrowser.Text = "&Browse Application State";
                helpStateBrowser.Click += new EventHandler(helpStateBrowser_Click);

                helpMenu.MenuItems.Add(new IconMenuItem("-"));
                helpMenu.MenuItems.Add(helpStateBrowser);
            }

            Menu = mainMenu;

            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            Controls.Add(mainPanel);

            resultsSplitter = new Splitter();
            resultsSplitter.Dock = DockStyle.Bottom;
            resultsSplitter.Height = 2;
            Controls.Add(resultsSplitter);

            outputPanel = new OutputPanel(this, buildMenu);
            outputPanel.Text = "Output";
            searchPanel = new SearchPanel(this, editMenu);
            searchPanel.Text = "Search Results";

            resultsPanel = new ResultsPanel(new Panel[] { outputPanel, searchPanel });
            resultsPanel.CloseClicked += new EventHandler(ResultsPanelCloseClick);
            resultsPanel.Dock = DockStyle.Bottom;
            resultsPanel.Height = 100;
            resultsPanel.Size = new Size(200, 216);

            Controls.Add(resultsPanel);

            resultsPanel.Visible = false;
            resultsSplitter.Visible = false;

            splashScreenHandler = new EventHandler(EditorForm_Activated);
            this.Activated += splashScreenHandler;
            if (formInstances.Count > 1)
            {
                splashIsDone = true;
            }
        }

        private static void TestCrashIfSpecified()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("/crash"))
            {
                Crashes.GenerateTestCrash();
            }
        }
        private void EditorForm_Activated(object sender, System.EventArgs e)
        {
            if (splashIsDone)
            {
                HideSplash();
            }
            else
            {
                ShowSplash();

                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = 1500;
                t.Tick += new EventHandler(t_Tick);
                t.Start();
            }
        }

        private void ShowSplash()
        {
            splash = new AboutForm();
            splash.StartPosition = FormStartPosition.Manual;
            splash.Left = this.Left + (this.Width / 2) - (splash.Width / 2);
            splash.Top = this.Top + (this.Height / 2) - (splash.Height / 2);
            splash.Show();

            splashIsDone = true;
        }

        private void t_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
            t.Stop();
            t.Enabled = false;

            HideSplash();
        }

        private void HideSplash()
        {
            // Need to lock for threading.
            // Cannot lock splash because that can be null.
            lock (splashScreenHandler)
            {
                if (splash != null)
                {
                    splash.Close();
                    splash = null;
                }
            }
        }

        private void fileNew_Click(object sender, System.EventArgs e)
        {
            NewWizard();
        }

        private void fileNewEmpty_Click(object sender, EventArgs e)
        {
            //use no wizard
            if (WixEdit.Settings.WixEditSettings.Instance.IsUsingWix2())
            {
                MessageBox.Show("Creating new wxs files with the wizard is not supported for WiX 2.\r\n\r\nPlease use WiX 3 or higher instead.", "Older version of WiX", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (HandlePendingChanges() == false)
            {
                return;
            }

            var newWixFiles = WixFiles.FromTemplate();
            CloseWxsFile();
            LoadWxsFile(newWixFiles);
            ShowProductProperties();
            ReloadAll();
        }

        private void NewWizard()
        {
            if (WixEdit.Settings.WixEditSettings.Instance.IsUsingWix2())
            {
                MessageBox.Show("Creating new wxs files with the wizard is not supported for WiX 2.\r\n\r\nPlease use WiX 3 or higher instead.", "Older version of WiX", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            if (HandlePendingChanges() == false)
            {
                return;
            }

            var newWixFiles = WixFiles.FromTemplate();

            Wizard.WizardForm frm = new WixEdit.Wizard.WizardForm(newWixFiles);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                CloseWxsFile();

                LoadWxsFile(newWixFiles);

                ShowProductProperties();
            }

            ReloadAll();
        }

        private void EditWizard()
        {
            Wizard.WizardForm frm = new WixEdit.Wizard.WizardForm(wixFiles);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ReloadAll();
            }
        }

        private void fileLoad_Click(object sender, System.EventArgs e)
        {
            OpenFile();
        }

        private void EditorForm_DragEnter(object sender, DragEventArgs e)
        {
            //accept only files on drag&drop
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        //delegate for asynchronous call
        private delegate void OpenFileDelegate(String s);

        private void EditorForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                //get file data
                Array aFiles = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (aFiles != null)
                {
                    //just use the first file
                    string sFile = aFiles.GetValue(0).ToString();
                    // Use BeginInvoke for asynchronous call to prevent Explorer freezing while loading file,
                    // as Explorer will wait for this handler to return.
                    this.BeginInvoke(new OpenFileDelegate(this.PrepareOpenFileFromDragDrop), sFile);
                    // in the case Explorer overlaps this form
                    this.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Error in DragDrop!\r\n({0}\r\n{1})", ex.Message, ex.StackTrace));
            }
        }

        private void PrepareOpenFileFromDragDrop(String fileToOpen)
        {
            if (HandlePendingChanges() == false)
            {
                return;
            }
            //close active file
            CloseWxsFile();
            //open new file
            OpenFile(fileToOpen);
        }

        private void OpenFile()
        {
            if (HandlePendingChanges() == false)
            {
                return;
            }

            openWxsFileDialog.Filter = "WiX Files (*.xml;*.wxs;*.wxi)|*.XML;*.WXS;*.WXI|MSI Files (*.msi;*.msm)|*.MSI;*.MSM|All files (*.*)|*.*";
            openWxsFileDialog.RestoreDirectory = true;

            if (openWxsFileDialog.ShowDialog() == DialogResult.OK)
            {
                CloseWxsFile();

                string fileToOpen = openWxsFileDialog.FileName;
                OpenFile(fileToOpen);
            }
        }

        private void OpenFile(string fileToOpen)
        {
            try
            {
                if (fileToOpen.ToLower().EndsWith("msi") || fileToOpen.ToLower().EndsWith("msm"))
                {
                    try
                    {
                        // Either the wxs file doesn't exist or the user gives permission to overwrite the wxs file
                        if (File.Exists(Path.ChangeExtension(fileToOpen, "wxs")) == false ||
                            MessageBox.Show("The existing wxs file will be overwritten.\r\n\r\nAre you sure you want to continue?", "Overwrite?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {

                            decompiledWxs = Path.ChangeExtension(fileToOpen, "wxs");

                            Decompile(fileToOpen, new OutputPanel.OnCompleteDelegate(OnDecompileComplete));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to decompile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    LoadWxsFile(fileToOpen);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to open {0}.\r\n({1}\r\n{2})", fileToOpen, ex.Message, ex.StackTrace));
            }
        }

        protected void OnDecompileComplete(bool isCancelled)
        {
            if (isCancelled)
            {
                return;
            }

            if (File.Exists(decompiledWxs))
            {
                LoadWxsFile(decompiledWxs);
            }
            else
            {
                MessageBox.Show("Dark.exe failed to decompile the msi.", "Failed to decompile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fileSave_Click(object sender, System.EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (!wixFiles.IsNew)
            {
                try
                {
                    wixFiles.Save();
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Failed to save " + wixFiles.WxsFile.Name + ":\r\n\r\n" + ex.Message, "Failed to save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                SaveAs();
            }
        }

        private void fileSaveAs_Click(object sender, System.EventArgs e)
        {
            SaveAs();
        }

        private bool SaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.OverwritePrompt = true;
            dlg.AddExtension = true;
            dlg.DefaultExt = ".wxs";

            if (wixFiles.IsNew)
            {
                dlg.FileName = "untitled.wxs";
            }
            else
            {
                dlg.FileName = wixFiles.WxsFile.FullName;
            }

            dlg.Filter = "WiX Files (*.wxs;*.wxi;*.xml)|*.wxs;*.wxi;*.xml|All files (*.*)|*.*";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string newName = dlg.FileName;
                string ext = Path.GetExtension(newName);
                if (ext == null || ext.Length == 0)
                {
                    newName = newName + ".wxs";
                }

                try
                {
                    wixFiles.SaveAs(newName);
                    WixEditSettings.Instance.AddRecentlyUsedFile(wixFiles.WxsFile);
                    WixEditSettings.Instance.SaveChanges();

                    UpdateTitlebar();

                    return true;
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Failed to save " + newName + ":\r\n\r\n" + ex.Message, "Failed to save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return false;
        }

        public void ForceClose()
        {
            HandlePendingChanges(null, true);

            CloseWxsFile();

            this.Close();
        }

        private void fileClose_Click(object sender, System.EventArgs e)
        {
            if (HandlePendingChanges() == false)
            {
                return;
            }

            CloseWxsFile();

            if (formInstances.Count > 1)
            {
                this.Close();
            }
        }

        private void fileExit_Click(object sender, System.EventArgs e)
        {
            EditorForm[] constEditorArray = new EditorForm[formInstances.Count];
            formInstances.CopyTo(constEditorArray);
            for (int i = 0; i < constEditorArray.Length; i++)
            {

                EditorForm edit = constEditorArray[i];

                if (edit == this)
                {
                    continue;
                }

                edit.Invoke(new VoidVoidDelegate(edit.Close));
            }

            this.Close();
        }

        private void fileMenu_Popup(object sender, System.EventArgs e)
        {
            bool xsdPresent = WixFiles.CheckForXsd();
            fileNew.Enabled = xsdPresent;
            fileNewEmpty.Enabled = xsdPresent;
            fileLoad.Enabled = xsdPresent;
            fileRecent.Enabled = xsdPresent;

            if (wixFiles != null)
            {
                fileSave.Enabled = (wixFiles.IsNew || (!wixFiles.ReadOnly() && wixFiles.HasChanges()));
            }
            else
            {
                fileSave.Enabled = false;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (wixFiles != null)
            {
                if (wixFiles.HasChanges())
                {
                    if (HandlePendingChanges() == false)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                CloseWxsFile();
            }

            this.Hide();

            if (formInstances.Contains(this))
            {
                formInstances.Remove(this);
            }

            if (formInstances.Count == 0)
            {
                Application.Exit();
            }
        }

        private void fileRecent_Popup(object sender, System.EventArgs e)
        {
            // Clear the menu
            fileRecent.MenuItems.Clear();

            string[] recentFiles = WixEditSettings.Instance.GetRecentlyUsedFiles();
            if (recentFiles.Length == 0)
            {
                fileRecent.MenuItems.Add(0, fileRecentEmpty);
            }
            else
            {
                bool hasObsolete = false;

                int i = 0;
                foreach (string recentFile in recentFiles)
                {
                    string recentFileText = recentFile;
                    if (recentFile.Length > 100)
                    {
                        recentFileText = "..." + recentFile.Substring(recentFile.Length - 98, 98);
                    }

                    IconMenuItem recentFileMenuItem = new IconMenuItem();
                    recentFileMenuItem.Text = String.Format("&{0} {1}", i + 1, recentFileText);
                    recentFileMenuItem.Click += new EventHandler(recentFile_Click);

                    if (File.Exists(recentFile))
                    {
                        Icon ico = FileIconFactory.GetFileIcon(recentFile);
                        recentFileMenuItem.Bitmap = ico.ToBitmap();
                    }
                    else
                    {
                        recentFileMenuItem.Enabled = false;
                        hasObsolete = true;
                    }

                    fileRecent.MenuItems.Add(i, recentFileMenuItem);

                    i++;
                }

                fileRecent.MenuItems.Add(i++, new IconMenuItem("-"));

                // only show clean if there are obsolete files
                if (hasObsolete)
                {
                    fileRecent.MenuItems.Add(i++, fileRecentClean);
                }

                fileRecent.MenuItems.Add(i, fileRecentClear);
            }
        }

        private void editMenu_Popup(object sender, System.EventArgs e)
        {
            // Clear the menu, so when we change the text the 
            // IconMenuItem.OnMeasureItem will be fired.

            if (wixFiles == null || wixFiles.UndoManager.CanUndo() == false)
            {
                editUndo.Enabled = false;
                editUndo.Text = "&Undo";
            }
            else
            {
                editUndo.Enabled = true;
                editUndo.Text = "&Undo " + wixFiles.UndoManager.GetNextUndoActionString();
            }

            if (wixFiles == null || wixFiles.UndoManager.CanRedo() == false)
            {
                editRedo.Enabled = false;
                editRedo.Text = "&Redo";
            }
            else
            {
                editRedo.Enabled = true;
                editRedo.Text = "&Redo " + wixFiles.UndoManager.GetNextRedoActionString();
            }

            editFind.Enabled = (wixFiles != null && searchPanel.IsBusy == false);
            editFindNext.Enabled = (wixFiles != null && searchPanel.HasResultSelected);
            editFindPrev.Enabled = (wixFiles != null && searchPanel.HasResultSelected);
            editWizard.Enabled = (wixFiles != null);
        }

        private void toolsMenu_Popup(object sender, System.EventArgs e)
        {
            bool hasExternalEditor = (WixEditSettings.Instance.ExternalXmlEditor != null && File.Exists(WixEditSettings.Instance.ExternalXmlEditor));

            if (wixFiles == null || wixFiles.IsNew || hasExternalEditor == false)
            {
                toolsExternal.Enabled = false;
            }
            else
            {
                toolsExternal.Enabled = true;
            }

            if (toolsExternal.HasIcon() == false && hasExternalEditor)
            {
                Icon ico = FileIconFactory.GetFileIcon(WixEditSettings.Instance.ExternalXmlEditor);
                toolsExternal.Bitmap = ico.ToBitmap();
            }
            if (toolsExternal.HasIcon() == true && hasExternalEditor == false)
            {
                toolsExternal.Bitmap = null;
            }
        }

        private void buildMenu_Popup(object sender, EventArgs e)
        {
            buildWixCompile.Enabled = (wixFiles != null && outputPanel.IsBusy == false);

            bool isEnabled = false;
            if (wixFiles != null)
            {
                if (wixFiles.OutputFile.Exists)
                {
                    isEnabled = (outputPanel.IsBusy == false);
                }
            }

            buildWixInstall.Enabled = isEnabled;
            buildWixUninstall.Enabled = isEnabled;
        }

        private void recentFileClear_Click(object sender, System.EventArgs e)
        {
            WixEditSettings.Instance.ClearRecentlyUsedFiles();
            WixEditSettings.Instance.SaveChanges();
        }

        private void recentFileClean_Click(object sender, System.EventArgs e)
        {
            WixEditSettings.Instance.CleanRecentlyUsedFiles();
            WixEditSettings.Instance.SaveChanges();
        }

        private void recentFile_Click(object sender, System.EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == null)
            {
                return;
            }

            if (HandlePendingChanges() == false)
            {
                return;
            }

            CloseWxsFile();

            string[] recentFiles = WixEditSettings.Instance.GetRecentlyUsedFiles();
            if (File.Exists(recentFiles[item.Index]))
            {
                LoadWxsFile(recentFiles[item.Index]);
            }
            else
            {
                MessageBox.Show("File could not be found.");
            }
        }

        private void editUndo_Click(object sender, System.EventArgs e)
        {
            XmlNode node = wixFiles.UndoManager.Undo();

            // This could slow things down, but make sure every panel is up-to-date.
            ReloadAll();

            ShowNode(node, false);
        }

        private void editRedo_Click(object sender, System.EventArgs e)
        {
            XmlNode node = wixFiles.UndoManager.Redo();

            // This could slow things down, but make sure every panel is up-to-date.
            ReloadAll();

            ShowNode(node, false);
        }

        private void helpStateBrowser_Click(object sender, System.EventArgs e)
        {
            // Get form type
            Type stateBrowserFormType = stateBrowserAssm.GetType("sliver.Windows.Forms.StateBrowserForm");

            // Get ObjectToBrowse property
            PropertyInfo objectToBrowseProp = stateBrowserFormType.GetProperty("ObjectToBrowse");

            // Create instance of form
            object stateBrowserForm = Activator.CreateInstance(stateBrowserFormType);

            PropertyInfo tmpProp = stateBrowserFormType.GetProperty("ShowDataTypes");
            tmpProp.SetValue(stateBrowserForm, true, new object[0]);

            tmpProp = stateBrowserFormType.GetProperty("ShowNonPublicMembers");
            tmpProp.SetValue(stateBrowserForm, true, new object[0]);

            tmpProp = stateBrowserFormType.GetProperty("ShowStaticMembers");
            tmpProp.SetValue(stateBrowserForm, true, new object[0]);

            tmpProp = stateBrowserFormType.GetProperty("WindowState");
            tmpProp.SetValue(stateBrowserForm, FormWindowState.Maximized, new object[0]);

            // Set ObjectToBrowse property to this form
            objectToBrowseProp.SetValue(stateBrowserForm, this, new object[0]);

            // Show the form
            Form form = (Form)stateBrowserForm;
            form.Show(this);
        }

        private void editFind_Click(object sender, System.EventArgs e)
        {
            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Find what text:";
            if (DialogResult.OK == frm.ShowDialog())
            {
                string search = frm.SelectedString;
                ShowSearchPanel();
                searchPanel.Search(wixFiles, search);
            }
        }

        private void editFindNext_Click(object sender, System.EventArgs e)
        {
            if (searchPanel.HasResultSelected)
            {
                ShowSearchPanel();
                searchPanel.FindNext();
            }
        }

        private void editFindPrev_Click(object sender, System.EventArgs e)
        {
            if (searchPanel.HasResultSelected)
            {
                ShowSearchPanel();
                searchPanel.FindPrev();
            }
        }


        private void editWizard_Click(object sender, System.EventArgs e)
        {
            EditWizard();
        }

        private void toolsExternal_Click(object sender, System.EventArgs e)
        {
            if (wixFiles == null ||
                WixEditSettings.Instance.ExternalXmlEditor == null ||
                File.Exists(WixEditSettings.Instance.ExternalXmlEditor) == false)
            {
                return;
            }

            ProcessStartInfo psiExternal = new ProcessStartInfo();
            psiExternal.FileName = WixEditSettings.Instance.ExternalXmlEditor;
            psiExternal.Arguments = String.Format("\"{0}\"", wixFiles.WxsFile.FullName);

            try
            {
                Process.Start(psiExternal);
            }
            catch
            {
                MessageBox.Show("Failed to start external editor.", "Failed to compile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowNode(XmlNode node)
        {
            ShowNode(node, false);
        }

        private void ShowNode(XmlNode node, bool forceReload)
        {
            if (node != null)
            {
                foreach (DisplayBasePanel panel in panels)
                {
                    if (panel == null)
                    {
                        continue;
                    }

                    if (node.Name == "Product")
                    {
                        panel.ReloadData();
                    }
                    else if (panel.IsOwnerOfNode(node))
                    {
                        tabButtonControl.SelectedPanel = panel;
                        if (forceReload)
                        {
                            panel.ReloadData();
                        }
                        panel.ShowNode(node);
                        break;
                    }
                }
            }
        }

        private void buildWixCompile_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (HandlePendingChanges("You need to save all changes before you can compile."))
                {
                    Compile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to compile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buildWixInstall_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (wixFiles != null)
                {
                    if (wixFiles.OutputFile.Exists == false)
                    {
                        MessageBox.Show("Install package doesn't exist. Compile the package first.", "Need to compile", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }

                    if (wixFiles.HasChanges() == true)
                    {
                        if (DialogResult.Cancel == MessageBox.Show("In memory changes to \"" + wixFiles.WxsFile.Name + "\" will be discared with this install.", "Discard changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
                        {
                            return;
                        }
                    }

                    if (wixFiles.WxsFile.LastWriteTime.CompareTo(wixFiles.OutputFile.LastWriteTime) >= 0)
                    {
                        DialogResult outOfDate = MessageBox.Show("The MSI file is out of date, continue?", "Discard changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                        if (outOfDate == DialogResult.Cancel || outOfDate == DialogResult.No)
                        {
                            return;
                        }
                    }

                    Install(wixFiles.OutputFile.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to install", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buildWixUninstall_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (wixFiles != null)
                {
                    if (wixFiles.OutputFile.Exists == false)
                    {
                        MessageBox.Show("Install package doesn't exist. Compile and install the package first.", "Need to compile", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }

                    Uninstall(wixFiles.OutputFile.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to uninstall", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buildProjectSettings_Click(object sender, EventArgs e)
        {
            ProjectSettingsForm frm = new ProjectSettingsForm(wixFiles);
            frm.ShowDialog();
        }


        private void Install(string packagePath)
        {
            //msiexec /i Product.msi /l*v! Product.log
            string msiexec = "msiexec.exe";
            string logFile = Path.ChangeExtension(packagePath, "log");

            ProcessStartInfo psiInstall = new ProcessStartInfo();
            psiInstall.FileName = msiexec;
            psiInstall.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psiInstall.Arguments = String.Format("/i \"{0}\" /l*v! \"{1}\"", packagePath, logFile);
            psiInstall.CreateNoWindow = true;
            psiInstall.UseShellExecute = false;
            psiInstall.RedirectStandardOutput = false;
            psiInstall.RedirectStandardError = false;

            ShowOutputPanel();
            outputPanel.Clear();
            Update();

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            outputPanel.RunWithLogFile(psiInstall, logFile);
        }

        private void Uninstall(string packagePath)
        {
            //msiexec /x Product.msi /l*v! Product.log
            string msiexec = "msiexec.exe";
            string logFile = Path.ChangeExtension(packagePath, "log");

            ProcessStartInfo psUninstall = new ProcessStartInfo();
            psUninstall.FileName = msiexec;
            psUninstall.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psUninstall.Arguments = String.Format("/x \"{0}\" /l*v! \"{1}\"", packagePath, logFile);
            psUninstall.CreateNoWindow = true;
            psUninstall.UseShellExecute = false;
            psUninstall.RedirectStandardOutput = false;
            psUninstall.RedirectStandardError = false;

            ShowOutputPanel();
            outputPanel.Clear();
            Update();

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            outputPanel.RunWithLogFile(psUninstall, logFile);
        }

        private void Compile()
        {
            string candleExe = WixEditSettings.Instance.WixBinariesDirectory.Candle;
            if (File.Exists(candleExe) == false)
            {
                throw new WixEditException("The executable \"candle.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiCandle = new ProcessStartInfo();
            psiCandle.FileName = candleExe;
            psiCandle.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psiCandle.CreateNoWindow = true;
            psiCandle.UseShellExecute = false;
            psiCandle.RedirectStandardOutput = true;
            psiCandle.RedirectStandardError = true;
            psiCandle.Arguments = wixFiles.GetCandleArguments();

            string lightExe = WixEditSettings.Instance.WixBinariesDirectory.Light;
            if (File.Exists(lightExe) == false)
            {
                throw new WixEditException("The executable \"light.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiLight = new ProcessStartInfo();
            psiLight.FileName = lightExe;
            psiLight.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psiLight.CreateNoWindow = true;
            psiLight.UseShellExecute = false;
            psiLight.RedirectStandardOutput = true;
            psiLight.RedirectStandardError = true;
            psiLight.Arguments = wixFiles.GetLightArguments();

            ShowOutputPanel();
            outputPanel.Clear();
            Update();

            outputPanel.Run(new ProcessStartInfo[] { psiCandle, psiLight }, wixFiles);
        }

        private void Decompile(string fileName, OutputPanel.OnCompleteDelegate onComplete)
        {
            FileInfo msiFile = new FileInfo(fileName);

            string darkExe = WixEditSettings.Instance.WixBinariesDirectory.Dark;
            if (File.Exists(darkExe) == false)
            {
                throw new WixEditException("The executable \"dark.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiDark = new ProcessStartInfo();
            psiDark.FileName = darkExe;
            psiDark.CreateNoWindow = true;
            psiDark.UseShellExecute = false;
            psiDark.RedirectStandardOutput = true;
            psiDark.RedirectStandardError = true;
            psiDark.Arguments = String.Format("-nologo -x \"{0}\" \"{1}\" \"{2}\"", msiFile.DirectoryName, msiFile.FullName, Path.ChangeExtension(msiFile.FullName, "wxs"));

            ShowOutputPanel();
            outputPanel.Clear();
            Update();

            outputPanel.Run(psiDark, onComplete);
        }

        private void ShowResultsPanel()
        {
            resultsSplitter.Visible = true;
            resultsPanel.Visible = true;
        }

        private void ShowOutputPanel()
        {
            resultsSplitter.Visible = true;
            resultsPanel.Visible = true;

            resultsPanel.ShowPanel(outputPanel);
        }

        private void ShowSearchPanel()
        {
            resultsSplitter.Visible = true;
            resultsPanel.Visible = true;

            resultsPanel.ShowPanel(searchPanel);
        }

        private void ResultsPanelCloseClick(object sender, System.EventArgs e)
        {
            HideResultsPanel();
        }

        private void HideResultsPanel()
        {
            resultsSplitter.Visible = false;
            resultsPanel.Visible = false;
        }

        protected void ShowProductProperties()
        {
            XmlNode product = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);
            ProductPropertiesForm frm = new ProductPropertiesForm(product, wixFiles);
            frm.ShowDialog();

            editGlobalDataPanel.ReloadData();
        }

        private void toolsOptions_Click(object sender, System.EventArgs e)
        {
            // Track changes to Xsd or Bin path or version, if it changes we need to restart/reload.
            string xsds = WixEditSettings.Instance.WixBinariesDirectory.Xsds;
            string version = WixEditSettings.Instance.WixBinariesVersion.Substring(0, 1);

            SettingsForm frm = new SettingsForm();
            frm.ShowDialog();

            if (xsds != WixEditSettings.Instance.WixBinariesDirectory.Xsds ||
                version != WixEditSettings.Instance.WixBinariesVersion.Substring(0, 1))
            {
                // Close all files...
                if (wixFiles != null || formInstances.Count > 1)
                {
                    MessageBox.Show("You must close all files first before the new setting can be applied.", "Apply settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    HandlePendingChanges(null, true);

                    this.CloseWxsFile();

                    EditorForm[] constEditorArray = new EditorForm[formInstances.Count];
                    formInstances.CopyTo(constEditorArray);
                    for (int i = 0; i < constEditorArray.Length; i++)
                    {
                        EditorForm edit = constEditorArray[i];

                        if (edit == this)
                        {
                            continue;
                        }

                        edit.Invoke(new VoidVoidDelegate(edit.ForceClose));
                    }

                    while (formInstances.Count != 1)
                    {
                        Thread.Sleep(100);
                    }

                    // and reload xsds.
                    WixFiles.ReloadXsd();

                    MessageBox.Show("Settings applied successfully.", "Apply settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // and reload xsds.
                    WixFiles.ReloadXsd();
                }

                if (xsdWarningIsDone == false && WixFiles.CheckForXsd() == false)
                {
                    xsdWarningIsDone = true;

                    if (String.IsNullOrEmpty(WixEditSettings.Instance.WixBinariesDirectory.BinDirectory) ||
                    Directory.Exists(WixEditSettings.Instance.WixBinariesDirectory.BinDirectory) == false)
                    {
                        MessageBox.Show("Windows Installer XML (WiX) Toolset installation is required to run WixEdit.\r\n\r\nThe WiX installation can be downloaded from http://wixtoolset.org/. Please download and install WiX and specify the install location in the WixEdit options.", "Missing WiX");
                    }
                    else
                    {
                        MessageBox.Show("Please check your WiX installation!\r\n\r\nCannot find Wix.xsd! It should be located in the 'doc' subdirectory of your WiX installation. Please check your WiX installation and the XSDs location in the WixEdit options. This file is required to determine the correct xml schema for your version of WiX.", "Missing Wix.xsd");
                    }
                }
            }
        }

        private void helpTutorial_Click(object sender, System.EventArgs e)
        {
            FileHelper.OpenTarget("https://www.firegiant.com/wix/tutorial/");
        }

        private void helpWiXReference_Click(object sender, System.EventArgs e)
        {
            string xsdDir = WixEditSettings.Instance.WixBinariesDirectory.Xsds;
            FileHelper.OpenTarget(Path.Combine(xsdDir, "WiX.chm"));
        }

        private void helpMSIReference_Click(object sender, System.EventArgs e)
        {
            FileHelper.OpenTarget("https://docs.microsoft.com/en-us/windows/win32/msi/windows-installer-reference");
        }

        private void helpAbout_Click(object sender, System.EventArgs e)
        {
            ShowSplash();
        }

        private void OnTabChanged(object sender, EventArgs e)
        {
            if (oldTabIndex == tabButtonControl.SelectedIndex)
            {
                return;
            }

            if (panels[panels.Length - 1] == null)
            {
                return;
            }

            if (panels[oldTabIndex].Menu != null)
            {
                mainMenu.MenuItems.RemoveAt(2);
            }

            if (panels[tabButtonControl.SelectedIndex].Menu != null)
            {
                mainMenu.MenuItems.Add(2, panels[tabButtonControl.SelectedIndex].Menu);
            }

            oldTabIndex = tabButtonControl.SelectedIndex;
        }

        private bool HandlePendingChanges()
        {
            return HandlePendingChanges(null, false);
        }

        private bool HandlePendingChanges(string message)
        {
            return HandlePendingChanges(message, false);
        }

        private bool HandlePendingChanges(string message, bool force)
        {
            if (wixFiles != null && wixFiles.HasChanges())
            {
                StringBuilder messageText = new StringBuilder();
                if (message != null)
                {
                    messageText.Append(message);
                    messageText.AppendLine();
                    messageText.AppendLine();
                }

                if (wixFiles.IsNew)
                {
                    messageText.AppendFormat("Save the newly created file?");
                }
                else
                {
                    messageText.AppendFormat("Save the changes you made to \"{0}\"?", wixFiles.WxsFile.Name);
                }

                MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
                if (force)
                {
                    buttons = MessageBoxButtons.YesNo;
                }

                DialogResult result = MessageBox.Show(messageText.ToString(), "Save changes?", buttons, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    if (wixFiles.ReadOnly())
                    {
                        return SaveAs();
                    }
                    else
                    {
                        Save();
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        private void LoadWxsFile(string filename)
        {
            LoadWxsFile(new FileInfo(filename));
        }

        private void LoadWxsFile(FileInfo file)
        {
            if (file.Exists == false)
            {
                MessageBox.Show(String.Format("File does not exist. ({0}))", file.Name), "File not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            WixFiles newWixFiles = null;

            try
            {
                newWixFiles = new WixFiles(file);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(String.Format("Access is denied. ({0}))", file.Name), "Acces denied", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(String.Format("Failed to open file. ({0}) The xml is not valid:\r\n\r\n{1}", file.Name, ex.Message), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (WixEditException ex)
            {
                MessageBox.Show(String.Format("Cannot open file:\r\n\r\n{0}", ex.Message), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch
            {
                MessageBox.Show(String.Format("Failed to open file. ({0}))", file.Name), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (newWixFiles == null)
            {
                return;
            }

            WixEditSettings.Instance.AddRecentlyUsedFile(file);
            WixEditSettings.Instance.SaveChanges();

            LoadWxsFile(newWixFiles);
        }

        private void LoadWxsFile(WixFiles newWixFiles)
        {
            if (wixFiles != null)
            {
                wixFiles.Dispose();
                wixFiles = null;
            }

            wixFiles = newWixFiles;
            wixFiles.wxsChanged += new EventHandler(wixFiles_wxsChanged);

            tabButtonControl = new TabButtonControl();
            tabButtonControl.Dock = DockStyle.Fill;

            mainPanel.Controls.Add(tabButtonControl);
            tabButtonControl.Visible = false;

            tabButtonControl.TabChange += new EventHandler(OnTabChanged);

            tabButtonControl.Visible = true;

            // Add Global tab
            editGlobalDataPanel = new EditGlobalDataPanel(wixFiles, new VoidVoidDelegate(ReloadAll));
            editGlobalDataPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Global", editGlobalDataPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.global.png")));

            panels[0] = editGlobalDataPanel;

            oldTabIndex = 0;


            // Add Files tab
            editInstallDataPanel = new EditInstallDataPanel(wixFiles);
            editInstallDataPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Files", editInstallDataPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.files.png")));

            panels[1] = editInstallDataPanel;

            if (editInstallDataPanel.Menu != null)
            {
                mainMenu.MenuItems.Add(2, editInstallDataPanel.Menu);
            }

            // Add properties tab
            editPropertiesPanel = new EditPropertiesPanel(wixFiles);
            editPropertiesPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Properties", editPropertiesPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.properties.png")));

            panels[2] = editPropertiesPanel;


            // Add dialog tab
            editUIPanel = new EditUIPanel(wixFiles);
            editUIPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Dialogs", editUIPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.dialogs.png")));

            panels[3] = editUIPanel;


            // Add Resources tab
            editResourcesPanel = new EditResourcesPanel(wixFiles);
            editResourcesPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Resources", editResourcesPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.resources.png")));

            panels[4] = editResourcesPanel;


            // Add Resources tab
            editActionsPanel = new EditActionsPanel(wixFiles);
            editActionsPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Actions", editActionsPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.actions.png")));

            panels[5] = editActionsPanel;


            // Add CustomTable tab
            editCustomTablePanel = new EditCustomTablePanel(wixFiles);
            editCustomTablePanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Tables", editCustomTablePanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.customtables.png")));

            panels[6] = editCustomTablePanel;

            // Update menu
            fileClose.Enabled = true;
            UpdateTitlebar();

            fileSave.Enabled = true;
            fileSaveAs.Enabled = true;

            buildWixCompile.Enabled = true;
            buildWixInstall.Enabled = true;
            buildWixUninstall.Enabled = true;
            buildProjectSettings.Enabled = true;
        }

        private void UpdateTitlebar()
        {
            if (wixFiles == null)
            {
                Text = "WiX Edit";
            }
            else if (wixFiles.IsNew)
            {
                Text = "WiX Edit";
            }
            else if (WixEditSettings.Instance.DisplayFullPathInTitlebar)
            {
                Text = "WiX Edit - " + wixFiles.WxsFile.FullName;
            }
            else
            {
                Text = "WiX Edit - " + wixFiles.WxsFile.Name;
            }
        }

        private void CloseWxsFile()
        {
            if (oldTabIndex >= 0 && oldTabIndex < panels.Length && panels[oldTabIndex].Menu != null)
            {
                mainMenu.MenuItems.RemoveAt(2);
            }

            buildWixCompile.Enabled = false;
            buildWixInstall.Enabled = false;
            buildWixUninstall.Enabled = false;
            buildProjectSettings.Enabled = false;

            if (tabButtonControl != null)
            {
                mainPanel.Controls.Remove(tabButtonControl);
                tabButtonControl.Visible = false;
                tabButtonControl = null;
            }

            oldTabIndex = -1;

            panels = new BasePanel[panelCount];

            if (editUIPanel != null)
            {
                editUIPanel.Visible = false;
                editUIPanel.CloseCurrentDialog();
                editUIPanel = null;
            }
            if (editPropertiesPanel != null)
            {
                editPropertiesPanel.Visible = false;
                editPropertiesPanel = null;
            }
            if (editResourcesPanel != null)
            {
                editResourcesPanel.Visible = false;
                editResourcesPanel = null;
            }
            if (editInstallDataPanel != null)
            {
                editInstallDataPanel.Visible = false;
                editInstallDataPanel = null;
            }
            if (editGlobalDataPanel != null)
            {
                editGlobalDataPanel.Visible = false;
                editGlobalDataPanel = null;
            }
            if (editActionsPanel != null)
            {
                editActionsPanel.Visible = false;
                editActionsPanel = null;
            }
            if (editCustomTablePanel != null)
            {
                editCustomTablePanel.Visible = false;
                editCustomTablePanel = null;
            }

            if (wixFiles != null)
            {
                wixFiles.Dispose();
                wixFiles = null;
            }

            fileClose.Enabled = false;
            Text = "WiX Edit";

            fileSave.Enabled = false;
            fileSaveAs.Enabled = false;

            searchPanel.Clear();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            Crashes.SentErrorReport += (object sender, SentErrorReportEventArgs e) => {
                MessageBox.Show($"Error report sent successfully.", "Error report sent successfully");
            };

            Crashes.FailedToSendErrorReport += (object sender, FailedToSendErrorReportEventArgs e) => {
                MessageBox.Show($"Error occured while reporting an error.\r\n{e.Exception.ToString()}", "Failed to send error report");
            };

            AppCenter.Start("97ffeb19-5eb3-422e-a73a-4a76d3f33379", typeof(Analytics), typeof(Crashes));

            // enable crash reporting
            Task setCrashesEnabledTask = Crashes.SetEnabledAsync(true);
            setCrashesEnabledTask.Wait();

            string fileToOpen = null;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                string xmlFile = args[1];
                if (xmlFile != null && xmlFile.Length > 0)
                {
                    if (File.Exists(xmlFile))
                    {
                        fileToOpen = xmlFile;
                    }
                    else if (xmlFile == "-last" || xmlFile == "/last")
                    {
                        string[] recentFiles = WixEditSettings.Instance.GetRecentlyUsedFiles();
                        if (recentFiles.Length > 0)
                        {
                            fileToOpen = recentFiles[0];
                        }
                    }
                }
            }

            Process otherProcess = FindOtherProcess();
            if (otherProcess != null)
            {
                IntPtr hWnd = otherProcess.MainWindowHandle;

                if (fileToOpen == null)
                {
                    if (User32.IsIconic(hWnd))
                    {
                        User32.ShowWindowAsync(hWnd, User32.WindowShowStyle.SW_RESTORE);
                    }

                    User32.SetForegroundWindow(hWnd);
                }
                else
                {
                    CopyDataMessenger.SendMessage(hWnd, "open|" + fileToOpen);
                }

                return;
            }

            Application.EnableVisualStyles();
            Application.DoEvents();

            try
            {
                EditorForm editorForm = null;
                if (fileToOpen == null)
                {
                    editorForm = new EditorForm();
                }
                else
                {
                    editorForm = new EditorForm(fileToOpen);
                }

                editorForm.Load += OnEditorFormLoad;

                Application.Run(editorForm);
            }
            catch (Exception ex)
            {
                string message = "Caught unhandled exception! Please press OK to report this error to the WixEdit website, so this error can be fixed.";
                ExceptionForm form = new ExceptionForm(message, ex);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ErrorReporter reporter = new ErrorReporter();
                    reporter.Report(ex);
                }
            }
        }

        private static void OnEditorFormLoad(object sender, EventArgs e)
        {
            TestCrashIfSpecified();
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = "An unhandled exception occured! Please press OK to report this error to the WixEdit website, so this error can be fixed.";
            ExceptionForm form = new ExceptionForm(message, e.ExceptionObject as Exception);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ErrorReporter reporter = new ErrorReporter();
                reporter.Report(e.ExceptionObject as Exception);
            }
        }

        private static Process FindOtherProcess()
        {
            Process otherProcess = null;
            string processName = "WixEdit.exe";

            try
            {
                Process thisProcess = Process.GetCurrentProcess();

                processName = thisProcess.ProcessName;

                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length > 1)
                {
                    for (int i = 0; i < processes.Length; i++)
                    {
                        Process aProcess = processes[i];
                        if (aProcess != null && aProcess.MainWindowHandle != IntPtr.Zero)
                        {
                            otherProcess = aProcess;
                        }
                    }

                    return otherProcess;
                }
            }
            catch { }

            return null;
        }

        protected override void WndProc(ref Message message)
        {
            //filter the WM_COPYDATA
            if (message.Msg == CopyDataMessenger.WM_COPYDATA)
            {
                string messageText = CopyDataMessenger.DecodeMessage(ref message);

                string[] messageItems = messageText.Split('|');
                DoAction(messageItems[0], messageItems[1]);
            }
            else
            {
                //be sure to pass along all messages to the base also
                base.WndProc(ref message);
            }
        }

        private void DoAction(string action, string argument)
        {
            switch (action)
            {
                case "open":
                    EditorForm foundForm = null;
                    foreach (EditorForm edit in formInstances)
                    {
                        // Hmmm, how can you compare 2 paths?!
                        if (edit.wixFiles != null && edit.wixFiles.WxsFile.FullName.ToLower() == new FileInfo(argument).FullName.ToLower())
                        {
                            foundForm = edit;
                        }
                    }

                    if (foundForm != null)
                    {
                        if (User32.IsIconic(foundForm.Handle))
                        {
                            User32.ShowWindowAsync(foundForm.Handle, User32.WindowShowStyle.SW_RESTORE);
                        }
                        User32.SetForegroundWindow(foundForm.Handle);
                    }
                    else
                    {
                        if (wixFiles == null || WixEditSettings.Instance.UseInstanceOnly)
                        {
                            LoadWxsFile(argument);
                        }
                        else
                        {
                            NewInstanceStarter starter = new NewInstanceStarter(argument);
                            starter.Start();
                        }
                    }
                    break;
            }
        }

        private delegate void InvokeReloadDataDelegate();
        private delegate void InvokeShowNodeDelegate(XmlNode node);

        private void wixFiles_wxsChanged(object sender, EventArgs e)
        {
            ReloadAll();
        }

        private void ReloadAll()
        {
            try
            {
                XmlNode current = null;
                if (tabButtonControl != null && tabButtonControl.SelectedPanel != null)
                {
                    current = ((DisplayBasePanel)tabButtonControl.SelectedPanel).GetShowingNode();
                }

                foreach (DisplayBasePanel panel in panels)
                {
                    if (panel == null)
                    {
                        continue;
                    }
                    panel.BeginInvoke(new InvokeReloadDataDelegate(panel.ReloadData));
                }

                if (current != null)
                {
                    DisplayBasePanel panel = (DisplayBasePanel)tabButtonControl.SelectedPanel;
                    panel.BeginInvoke(new InvokeShowNodeDelegate(panel.ShowNode), new object[] { current });
                }
            }
            catch (Exception ex)
            {
                string message = "Error with reloading all views, please press OK to report this error to the WixEdit website, so this error can be fixed.";
                ExceptionForm form = new ExceptionForm(message, ex);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ErrorReporter reporter = new ErrorReporter();
                    reporter.Report(ex);
                }
            }

            // Make sure all events are processed here,
            // otherwise the order of events cannot be guaranteed...
            Application.DoEvents();
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (e.Exception.InnerException is IncludeFileChangedException)
            {
                IncludeFileChangedException ifcException = e.Exception.InnerException as IncludeFileChangedException;
                ifcException.UndoManager.Undo(false);
                if (ifcException.NotifyUser)
                {
                    MessageBox.Show(String.Format("You cannot change \"{0}\"", ifcException.Command.AffectedInclude), "Cannot change includes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return;
            }
            else if (e.Exception is WixEditException)
            {
                MessageBox.Show(e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string message = "Unable to perform your action, an error occured! Please press OK to report this error to the WixEdit website, so this error can be fixed.";
            ExceptionForm form = new ExceptionForm(message, e.Exception);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ErrorReporter reporter = new ErrorReporter();
                reporter.Report(e.Exception);
            }
        }
    }
}
