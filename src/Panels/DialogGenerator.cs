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
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.Controls;
using WixEdit.Forms;
using WixEdit.Settings;
using WixEdit.Xml;

namespace WixEdit.Panels
{
    /// <summary>
    /// TODO: Add billboards, but how?
    /// </summary>
    public class DialogGenerator
    {
        private Hashtable definedFonts;
        private WixFiles wixFiles;
        private Control parent;

        static double scale;

        static DialogGenerator()
        {
            scale = WixEditSettings.Instance.Scale;
        }

        public DialogGenerator(WixFiles wixFiles, Control parent)
        {
            this.definedFonts = new Hashtable();
            this.wixFiles = wixFiles;
            this.parent = parent;

            ReadFonts();
        }

        private void ReadFonts()
        {
            XmlNodeList fontElements = wixFiles.WxsDocument.SelectNodes("//wix:UI/wix:TextStyle", wixFiles.WxsNsmgr);
            foreach (XmlNode fontElement in fontElements)
            {

                FontStyle style = FontStyle.Regular;
                if (fontElement.Attributes["Bold"] != null && fontElement.Attributes["Bold"].Value.ToLower() == "yes")
                {
                    style = style | FontStyle.Bold;
                }
                if (fontElement.Attributes["Italic"] != null && fontElement.Attributes["Italic"].Value.ToLower() == "yes")
                {
                    style = style | FontStyle.Italic;
                }
                if (fontElement.Attributes["Strike"] != null && fontElement.Attributes["Strike"].Value.ToLower() == "yes")
                {
                    style = style | FontStyle.Strikeout;
                }
                if (fontElement.Attributes["Underline"] != null && fontElement.Attributes["Underline"].Value.ToLower() == "yes")
                {
                    style = style | FontStyle.Underline;
                }

                Font font = new Font("Tahoma", (float)(scale * 8.00F), FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
                try
                {
                    font = new Font(
                        fontElement.Attributes["FaceName"].Value,
                        (float)(scale * XmlConvert.ToDouble(fontElement.Attributes["Size"].Value)),
                        style,
                        GraphicsUnit.Point
                    );
                }
                catch { }

                definedFonts.Add(fontElement.Attributes["Id"].Value, font);
            }
        }


        public static double Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        int parentHwnd;
        public DesignerForm GenerateDialog(XmlNode dialog, Control parent)
        {
            DesignerForm newDialog = new DesignerForm(wixFiles, dialog);

            parentHwnd = (int)parent.Handle;

            newDialog.Font = new Font("Tahoma", (float)(scale * 8.00F), FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            newDialog.ShowInTaskbar = true;
            // newDialog.TopMost = true;
            // newDialog.Opacity = 0.75;

            newDialog.Icon = new Icon(WixFiles.GetResourceStream("dialog.msi.ico"));

            newDialog.StartPosition = FormStartPosition.Manual;

            newDialog.MinimizeBox = false;
            newDialog.MaximizeBox = false;
            newDialog.FormBorderStyle = FormBorderStyle.FixedDialog;

            if (dialog.Attributes["Width"] == null ||
                dialog.Attributes["Width"].Value.Trim().Length == 0 ||
                dialog.Attributes["Height"] == null ||
                dialog.Attributes["Height"].Value.Trim().Length == 0)
            {
                return null;
            }

            newDialog.ClientSize = new Size(DialogUnitsToPixelsWidth(XmlConvert.ToInt32(dialog.Attributes["Width"].Value.Trim())), DialogUnitsToPixelsHeight(XmlConvert.ToInt32(dialog.Attributes["Height"].Value.Trim())));

            // Background Images should be added first, these controls should be used as parent 
            // to get correct transparancy. For now only 1 bitmap is supported per Dialog.
            // - Is this the correct way to handle the transparancy?
            // - How does MSI handle transparant labels when having 2 bitmaps as background?

            XmlNodeList buttons = dialog.SelectNodes("wix:Control[@Type='PushButton']", wixFiles.WxsNsmgr);
            AddButtons(newDialog, buttons);

            XmlNodeList edits = dialog.SelectNodes("wix:Control[@Type='Edit']", wixFiles.WxsNsmgr);
            AddEditBoxes(newDialog, edits);


            XmlNodeList checks = dialog.SelectNodes("wix:Control[@Type='CheckBox']", wixFiles.WxsNsmgr);
            AddCheckBoxes(newDialog, checks);

            XmlNodeList pathEdits = dialog.SelectNodes("wix:Control[@Type='PathEdit']", wixFiles.WxsNsmgr);
            AddPathEditBoxes(newDialog, pathEdits);

            XmlNodeList lines = dialog.SelectNodes("wix:Control[@Type='Line']", wixFiles.WxsNsmgr);
            AddLines(newDialog, lines);

            XmlNodeList texts = dialog.SelectNodes("wix:Control[@Type='Text']", wixFiles.WxsNsmgr);
            AddTexts(newDialog, texts);

            XmlNodeList rtfTexts = dialog.SelectNodes("wix:Control[@Type='ScrollableText']", wixFiles.WxsNsmgr);
            AddRftTextBoxes(newDialog, rtfTexts);

            XmlNodeList groupBoxes = dialog.SelectNodes("wix:Control[@Type='GroupBox']", wixFiles.WxsNsmgr);
            AddGroupBoxes(newDialog, groupBoxes);

            XmlNodeList icons = dialog.SelectNodes("wix:Control[@Type='Icon']", wixFiles.WxsNsmgr);
            AddIcons(newDialog, icons);

            XmlNodeList listBoxes = dialog.SelectNodes("wix:Control[@Type='ListBox']", wixFiles.WxsNsmgr);
            AddListBoxes(newDialog, listBoxes);

            XmlNodeList comboBoxes = dialog.SelectNodes("wix:Control[@Type='ComboBox']", wixFiles.WxsNsmgr);
            AddComboBoxes(newDialog, comboBoxes);

            XmlNodeList progressBars = dialog.SelectNodes("wix:Control[@Type='ProgressBar']", wixFiles.WxsNsmgr);
            AddProgressBars(newDialog, progressBars);

            XmlNodeList radioButtonGroups = dialog.SelectNodes("wix:Control[@Type='RadioButtonGroup']", wixFiles.WxsNsmgr);
            AddRadioButtonGroups(newDialog, radioButtonGroups);

            XmlNodeList maskedEdits = dialog.SelectNodes("wix:Control[@Type='MaskedEdit']", wixFiles.WxsNsmgr);
            AddMaskedEdits(newDialog, maskedEdits);

            XmlNodeList volumeCostLists = dialog.SelectNodes("wix:Control[@Type='VolumeCostList']", wixFiles.WxsNsmgr);
            AddVolumeCostLists(newDialog, volumeCostLists);

            XmlNodeList volumeComboBoxes = dialog.SelectNodes("wix:Control[@Type='VolumeSelectCombo']", wixFiles.WxsNsmgr);
            AddVolumeComboBoxes(newDialog, volumeComboBoxes);

            // Skipping tooltips
            /*
                        XmlNodeList tooltips = dialog.SelectNodes("wix:Control[@Type='Tooltips']", wixFiles.WxsNsmgr);
                        AddTooltips(newDialog, tooltips);
            */
            XmlNodeList directoryCombos = dialog.SelectNodes("wix:Control[@Type='DirectoryCombo']", wixFiles.WxsNsmgr);
            AddDirectoryCombos(newDialog, directoryCombos);

            XmlNodeList directoryLists = dialog.SelectNodes("wix:Control[@Type='DirectoryList']", wixFiles.WxsNsmgr);
            AddDirectoryLists(newDialog, directoryLists);

            XmlNodeList selectionTrees = dialog.SelectNodes("wix:Control[@Type='SelectionTree']", wixFiles.WxsNsmgr);
            AddSelectionTrees(newDialog, selectionTrees);


            XmlNodeList bitmaps = dialog.SelectNodes("wix:Control[@Type='Bitmap']", wixFiles.WxsNsmgr);
            AddBackgroundBitmaps(newDialog, bitmaps);

            if (dialog.Attributes["Title"] != null)
            {
                newDialog.Text = ExpandWixProperties(dialog.Attributes["Title"].Value);
            }

            if (dialog.Attributes["NoMinimize"] == null)
            {
                newDialog.MinimizeBox = true;
            }
            else
            {
                newDialog.MinimizeBox = (dialog.Attributes["NoMinimize"].Value.ToLower() != "yes");
            }

            return newDialog;
        }

        /// <summary>
        /// The function returns the dialog base units. The low-order word of the return value 
        /// contains the horizontal dialog box base unit, and the high-order word contains the 
        /// vertical dialog box base unit. 
        /// 
        /// One horizontal dialog unit is equal to one-fourth of the average character width for the current system font.
        /// One vertical dialog unit is equal to one-eighth of an average character height for the current system font.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("User32")]
        public static extern int GetDialogBaseUnits(int hwnd);


        // http://msdn.microsoft.com/library/en-us/msi/setup/installer_units.asp
        // Platform SDK: Windows Installer
        // Installer Units

        // A Windows Installer user interface unit is equal to one-twelfth (1/12) 
        // the height of the 10-point MS Sans Serif font size.

        public static int DialogUnitsToPixelsWidth(int dlus)
        {
            long DLUs = GetDialogBaseUnits(0);
            int HorDLUs = (int)DLUs & 0x0000FFFF;

            return (int)Math.Round(((double)scale * dlus * HorDLUs) / 6);
        }

        public static int DialogUnitsToPixelsHeight(int dlus)
        {
            long DLUs = GetDialogBaseUnits(0);
            int VerDLUs = (int)(DLUs >> 16) & 0xFFFF;

            return (int)Math.Round(((double)scale * dlus * VerDLUs) / 12);
        }

        public static int PixelsToDialogUnitsWidth(int pix)
        {
            long DLUs = GetDialogBaseUnits(0);
            int HorDLUs = (int)DLUs & 0x0000FFFF;

            return (int)Math.Round(((double)pix * 6) / (scale * HorDLUs));
        }

        public static int PixelsToDialogUnitsHeight(int pix)
        {
            long DLUs = GetDialogBaseUnits(0);
            int VerDLUs = (int)(DLUs >> 16) & 0xFFFF;

            return (int)Math.Round(((double)pix * 12) / (scale * VerDLUs));
        }

        private string ExpandWixProperties(string value)
        {
            int posStart = value.IndexOf("[", 0);
            int posEnd = 0;
            while (posStart > -1)
            {
                posEnd = value.IndexOf("]", posStart);
                if (posEnd == -1)
                {
                    // Nothing to resolve anymore... (Someone should use an end bracket)
                    break;
                }

                string propName = value.Substring(posStart + 1, posEnd - posStart - 1);

                XmlNode propertyNode = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:Property[@Id='{0}']", propName), wixFiles.WxsNsmgr);
                if (propertyNode != null)
                {
                    string propertyValue = String.Empty;
                    if (propertyNode.Attributes["Value"] != null)
                    {
                        propertyValue = propertyNode.Attributes["Value"].Value;
                    }
                    else
                    {
                        propertyValue = propertyNode.InnerText;
                    }
                    value = value.Replace(String.Format("[{0}]", propName), propertyValue);
                }
                else
                {
                    string specialProp = GetSpecialWixProperty(propName);
                    if (specialProp != String.Empty)
                    {
                        value = value.Replace(String.Format("[{0}]", propName), specialProp);
                    }
                    else
                    {
                        posStart++;
                    }
                }

                posStart = value.IndexOf("[", posStart);
            }

            return value;
        }

        private string GetSpecialWixProperty(string propname)
        {
            switch (propname)
            {
                case "ProductName":
                    return GetProductAttributeValue("Name");
                case "ProductCode":
                    return GetProductAttributeValue("Id");
                case "ProductLanguage":
                    return GetProductAttributeValue("Language");
                case "ProductVersion":
                    return GetProductAttributeValue("Version");
                case "Manufacturer":
                    return GetProductAttributeValue("Manufacturer");
                case "UpgradeCode":
                    return GetProductAttributeValue("UpgradeCode");
                default:
                    return String.Empty;
            }
        }

        private string GetProductAttributeValue(string attributeName)
        {
            string returnValue = String.Empty;

            XmlNode productyNode = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);
            XmlAttribute nameAttribute = productyNode.Attributes[attributeName];
            if (nameAttribute != null)
            {
                returnValue = nameAttribute.Value;
            }

            return returnValue;
        }

        private void AddButtons(DesignerForm newDialog, XmlNodeList buttons)
        {
            foreach (XmlNode button in buttons)
            {
                try
                {
                    Button newButton = new Button();
                    SetControlAttributes(newButton, button);

                    if (button.Attributes["Icon"] != null &&
                        button.Attributes["Icon"].Value.ToLower() == "yes")
                    {
                        string binaryId = GetTextFromXmlElement(button);
                        try
                        {
                            using (Stream imageStream = GetBinaryStream(binaryId))
                            {
                                Bitmap bmp = new Icon(imageStream).ToBitmap();
                                Bitmap dest = new Bitmap((int)Math.Round(bmp.Width * scale), (int)Math.Round(bmp.Height * scale));

                                Graphics g = Graphics.FromImage(dest);
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                g.DrawImage(bmp,
                                    new Rectangle(0, 0, dest.Width, dest.Height),
                                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                                    GraphicsUnit.Pixel);

                                g.Dispose();
                                bmp.Dispose();

                                newButton.Image = dest;
                            }
                        }
                        catch
                        {
                            SetText(newButton, button);
                        }
                    }
                    else
                    {
                        newButton.FlatStyle = FlatStyle.System;
                        SetText(newButton, button);
                    }

                    newDialog.AddControl(button, newButton);
                }
                catch
                {
                }
            }
        }

        private void AddEditBoxes(DesignerForm newDialog, XmlNodeList editboxes)
        {
            foreach (XmlNode edit in editboxes)
            {
                try
                {
                    TextBox newEdit = new TextBox();
                    SetControlAttributes(newEdit, edit);
                    SetText(newEdit, edit);
                    SetMultiline(newEdit, edit);

                    newEdit.BorderStyle = BorderStyle.Fixed3D;

                    newDialog.AddControl(edit, newEdit);
                }
                catch
                {
                }
            }
        }

        private void AddCheckBoxes(DesignerForm newDialog, XmlNodeList checkboxes)
        {
            foreach (XmlNode check in checkboxes)
            {
                try
                {
                    CheckBox checkBox = new CheckBox();
                    SetControlAttributes(checkBox, check);
                    SetText(checkBox, check);

                    newDialog.AddControl(check, checkBox);
                }
                catch
                {
                }
            }
        }

        private void AddPathEditBoxes(DesignerForm newDialog, XmlNodeList patheditboxes)
        {
            foreach (XmlNode pathEdit in patheditboxes)
            {
                try
                {
                    TextBox newPathEdit = new TextBox();
                    SetControlAttributes(newPathEdit, pathEdit);
                    SetText(newPathEdit, pathEdit);
                    SetMultiline(newPathEdit, pathEdit);

                    newDialog.AddControl(pathEdit, newPathEdit);
                }
                catch
                {
                }
            }
        }

        private void AddLines(DesignerForm newDialog, XmlNodeList lines)
        {
            foreach (XmlNode line in lines)
            {
                try
                {
                    Label label = new Label();
                    SetControlAttributes(label, line);

                    label.Height = 2;
                    label.BorderStyle = BorderStyle.Fixed3D;

                    newDialog.AddControl(line, label);
                }
                catch
                {
                }
            }
        }

        private void AddTexts(DesignerForm newDialog, XmlNodeList texts)
        {
            foreach (XmlNode text in texts)
            {
                try
                {
                    Label label = new Label();
                    SetControlAttributes(label, text);
                    SetText(label, text);

                    label.BackColor = Color.Transparent;

                    newDialog.AddControl(text, label);
                }
                catch
                {
                }
            }
        }

        private void AddRftTextBoxes(DesignerForm newDialog, XmlNodeList rtfTexts)
        {
            foreach (XmlNode text in rtfTexts)
            {
                try
                {
                    RichTextBox rtfCtrl = new RichTextBox();
                    SetControlAttributes(rtfCtrl, text);

                    string elementText = GetTextFromXmlElement(text);

                    rtfCtrl.Rtf = elementText;

                    newDialog.AddControl(text, rtfCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddGroupBoxes(DesignerForm newDialog, XmlNodeList groupBoxes)
        {
            foreach (XmlNode group in groupBoxes)
            {
                try
                {
                    GroupBox groupCtrl = new GroupBox();

                    // The FlatStyle.System makes the control look weird.
                    // groupCtrl.FlatStyle = FlatStyle.System;

                    SetControlAttributes(groupCtrl, group);
                    SetText(groupCtrl, group);

                    newDialog.AddControl(group, groupCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddIcons(DesignerForm newDialog, XmlNodeList icons)
        {
            foreach (XmlNode icon in icons)
            {
                try
                {
                    PictureBox picCtrl = new PictureBox();
                    SetControlAttributes(picCtrl, icon);

                    picCtrl.SizeMode = PictureBoxSizeMode.StretchImage;

                    string binaryId = GetTextFromXmlElement(icon);
                    try
                    {
                        using (Stream imageStream = GetBinaryStream(binaryId))
                        {
                            picCtrl.Image = new Icon(imageStream).ToBitmap();
                        }
                    }
                    catch
                    {
                    }

                    newDialog.AddControl(icon, picCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddListBoxes(DesignerForm newDialog, XmlNodeList listBoxes)
        {
            foreach (XmlNode list in listBoxes)
            {
                try
                {
                    ListBox listCtrl = new ListBox();
                    SetControlAttributes(listCtrl, list);

                    listCtrl.Items.Add(GetFromXmlElement(list, "Property"));

                    newDialog.AddControl(list, listCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddComboBoxes(DesignerForm newDialog, XmlNodeList comboBoxes)
        {
            foreach (XmlNode comboBox in comboBoxes)
            {
                try
                {
                    ComboBox comboCtrl = new ComboBox();
                    SetControlAttributes(comboCtrl, comboBox);

                    comboCtrl.Items.Add("ComboBox");
                    comboCtrl.SelectedIndex = 0;

                    newDialog.AddControl(comboBox, comboCtrl);
                }
                catch
                {
                }
            }
        }


        private void AddProgressBars(DesignerForm newDialog, XmlNodeList progressBars)
        {
            foreach (XmlNode progressbar in progressBars)
            {
                try
                {
                    ProgressBar progressCtrl = new ProgressBar();
                    SetControlAttributes(progressCtrl, progressbar);

                    progressCtrl.Maximum = 100;
                    progressCtrl.Value = 33;

                    newDialog.AddControl(progressbar, progressCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddRadioButtonGroups(DesignerForm newDialog, XmlNodeList radioButtonGroups)
        {
            foreach (XmlNode radioButtonGroup in radioButtonGroups)
            {
                try
                {
                    string radioGroupName = radioButtonGroup.Attributes["Property"].Value;
                    string defaultValue = ExpandWixProperties(String.Format("[{0}]", radioGroupName));

                    XmlNode radioGroup = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:RadioGroup[@Property='{0}']", radioGroupName), wixFiles.WxsNsmgr);
                    if (radioGroup == null)
                    {
                        radioGroup = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:RadioButtonGroup[@Property='{0}']", radioGroupName), wixFiles.WxsNsmgr);
                    }

                    Panel panel = new Panel();
                    SetControlAttributes(panel, radioButtonGroup);

                    foreach (XmlNode radioElement in radioGroup.ChildNodes)
                    {
                        RadioButton radioCtrl = new RadioButton();
                        SetText(radioCtrl, radioElement);
                        SetTag(radioCtrl, radioElement);

                        SetControlAttributes(radioCtrl, radioElement);

                        panel.Controls.Add(radioCtrl);

                        if (((string)radioCtrl.Tag).ToLower() == defaultValue.ToLower())
                        {
                            radioCtrl.Checked = true;
                        }
                    }

                    newDialog.AddControl(radioButtonGroup, panel);
                }
                catch
                {
                }
            }
        }

        private void AddMaskedEdits(DesignerForm newDialog, XmlNodeList maskedEdits)
        {
            foreach (XmlNode edit in maskedEdits)
            {
                try
                {
                    TextBox newEdit = new TextBox();
                    SetControlAttributes(newEdit, edit);
                    SetText(newEdit, edit);

                    newEdit.BorderStyle = BorderStyle.Fixed3D;

                    newDialog.AddControl(edit, newEdit);
                }
                catch
                {
                }
            }
        }

        private void AddVolumeCostLists(DesignerForm newDialog, XmlNodeList volumeCostLists)
        {
            foreach (XmlNode volumeCostList in volumeCostLists)
            {
                try
                {
                    ListView listView = new ListView();
                    ColumnHeader columnHeader1 = new ColumnHeader();
                    ColumnHeader columnHeader2 = new ColumnHeader();
                    ColumnHeader columnHeader3 = new ColumnHeader();
                    ColumnHeader columnHeader4 = new ColumnHeader();
                    ColumnHeader columnHeader5 = new ColumnHeader();

                    columnHeader1.Text = "Volume";
                    columnHeader2.Text = "Disk Size";
                    columnHeader3.Text = "Available";
                    columnHeader4.Text = "Required";
                    columnHeader5.Text = "Difference";

                    columnHeader1.TextAlign = HorizontalAlignment.Left;
                    columnHeader2.TextAlign = HorizontalAlignment.Right;
                    columnHeader3.TextAlign = HorizontalAlignment.Right;
                    columnHeader4.TextAlign = HorizontalAlignment.Right;
                    columnHeader5.TextAlign = HorizontalAlignment.Right;

                    listView.Columns.AddRange(new ColumnHeader[] { columnHeader1,
                                                                     columnHeader2,
                                                                     columnHeader3,
                                                                     columnHeader4,
                                                                     columnHeader5});

                    listView.Items.Add(new ListViewItem(new string[] { "C:", "30GB", "3200MB", "1MB", "3189MB" }));
                    listView.View = System.Windows.Forms.View.Details;

                    SetControlAttributes(listView, volumeCostList);

                    newDialog.AddControl(volumeCostList, listView);
                }
                catch
                {
                }
            }
        }

        private void AddVolumeComboBoxes(DesignerForm newDialog, XmlNodeList volumeCombos)
        {
            foreach (XmlNode volumeCombo in volumeCombos)
            {
                try
                {
                    ComboBox comboCtrl = new ComboBox();
                    comboCtrl.Items.Add("VolumeCombo");
                    comboCtrl.SelectedIndex = 0;

                    SetControlAttributes(comboCtrl, volumeCombo);

                    newDialog.AddControl(volumeCombo, comboCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddDirectoryCombos(DesignerForm newDialog, XmlNodeList directoryCombos)
        {
            foreach (XmlNode directoryCombo in directoryCombos)
            {
                try
                {
                    ComboBox comboCtrl = new ComboBox();
                    comboCtrl.Items.Add("Directories");
                    comboCtrl.SelectedIndex = 0;

                    SetControlAttributes(comboCtrl, directoryCombo);

                    newDialog.AddControl(directoryCombo, comboCtrl);
                }
                catch
                {
                }
            }
        }

        private void AddDirectoryLists(DesignerForm newDialog, XmlNodeList directoryLists)
        {
            foreach (XmlNode directoryList in directoryLists)
            {
                try
                {
                    ListBox listBox = new ListBox();
                    listBox.Items.Add("Director content");
                    listBox.SelectedIndex = 0;

                    SetControlAttributes(listBox, directoryList);

                    newDialog.AddControl(directoryList, listBox);
                }
                catch
                {
                }
            }
        }

        private void AddSelectionTrees(DesignerForm newDialog, XmlNodeList selectionTrees)
        {
            foreach (XmlNode selectionTree in selectionTrees)
            {
                try
                {
                    TreeView treeView = new TreeView();
                    treeView.Scrollable = false;
                    treeView.Nodes.Add(new TreeNode("Selection tree"));

                    SetControlAttributes(treeView, selectionTree);

                    newDialog.AddControl(selectionTree, treeView);
                }
                catch
                {
                }
            }
        }

        private void AddBackgroundBitmaps(DesignerForm newDialog, XmlNodeList bitmaps)
        {
            PictureControl pictureCtrl = null;
            ArrayList allPictureControls = new ArrayList();

            foreach (XmlNode bitmap in bitmaps)
            {
                try
                {
                    string binaryId = GetTextFromXmlElement(bitmap);

                    Bitmap bmp = null;
                    try
                    {
                        using (Stream imageStream = GetBinaryStream(binaryId))
                        {
                            bmp = new Bitmap(imageStream);
                        }
                    }
                    catch
                    {
                    }

                    pictureCtrl = new PictureControl(bmp, allPictureControls);
                    allPictureControls.Add(pictureCtrl);

                    SetControlAttributes(pictureCtrl, bitmap);

                    newDialog.AddControl(bitmap, pictureCtrl);
                }
                catch
                {
                }
            }

            if (pictureCtrl != null)
            {
                pictureCtrl.Draw();
            }
        }

        private void AddHyperlinks(DesignerForm newDialog, XmlNodeList hyperlinks)
        {
            foreach (XmlNode hyperlink in hyperlinks)
            {
                try
                {
                    Label label = new Label();
                    SetControlAttributes(label, hyperlink);
                    SetText(label, hyperlink);

                    label.BackColor = Color.Transparent;
                    label.ForeColor = Color.Blue;
                    label.Font = new Font(label.Font, FontStyle.Underline);

                    newDialog.AddControl(hyperlink, label);
                }
                catch
                {
                }
            }
        }

        private void SetControlAttributes(Control control, XmlNode controlElement)
        {
            control.Left = DialogUnitsToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["X"].Value));
            control.Top = DialogUnitsToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Y"].Value));
            control.Width = DialogUnitsToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["Width"].Value));
            control.Height = DialogUnitsToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Height"].Value));

            XmlAttribute disabled = controlElement.Attributes["Disabled"];
            if (disabled != null && disabled.Value == "yes")
            {
                control.Enabled = false;
            }

            //control.ClientSize = new Size(DialogUnitsToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["Width"].Value)), DialogUnitsToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Height"].Value)));
        }

        private void SetText(Control textControl, XmlNode textElement)
        {
            string textValue = GetTextFromXmlElement(textElement);

            int startFont = textValue.IndexOf("{\\");
            if (startFont < 0)
            {
                startFont = textValue.IndexOf("{&");
            }
            if (startFont >= 0)
            {
                int endFont = textValue.IndexOf("}", startFont);

                Font font = definedFonts[textValue.Substring(startFont + 2, endFont - startFont - 2)] as Font;
                if (font != null)
                {
                    textControl.Font = font;
                }

                textValue = textValue.Remove(startFont, endFont - startFont + 1);
            }

            textControl.Text = textValue;
        }

        private void SetMultiline(TextBox control, XmlNode controlElement)
        {
            XmlAttribute multiline = controlElement.Attributes["Multiline"];
            if (multiline != null && multiline.Value == "yes")
            {
                control.Multiline = true;
            }
        }

        private void SetTag(Control textControl, XmlNode textElement)
        {
            string textValue = textElement.InnerText;

            textControl.Tag = textValue;
        }


        private string GetTextFromXmlElement(XmlNode textElement)
        {
            string elementText = String.Empty;

            XmlNode text = textElement.SelectSingleNode("wix:Text", wixFiles.WxsNsmgr);
            if (text != null)
            {
                XmlAttribute srcAttrib = text.Attributes["SourceFile"];

                if (srcAttrib != null)
                {
                    string src = srcAttrib.Value;

                    if (src != null && src.Length != 0)
                    {
                        TextReader reader = null;

                        if (Path.IsPathRooted(src))
                        {
                            if (File.Exists(src))
                            {
                                reader = File.OpenText(src);
                            }
                        }
                        else
                        {
                            if (File.Exists(src))
                            {
                                reader = File.OpenText(src);
                            }
                            else
                            {
                                FileInfo[] files = wixFiles.WxsDirectory.GetFiles(src);
                                if (files.Length == 1)
                                {
                                    reader = files[0].OpenText();
                                }
                            }
                        }

                        if (reader != null)
                        {
                            using (reader)
                            {
                                elementText = reader.ReadToEnd();
                            }
                        }
                    }
                }

                if (elementText == null || elementText.Trim().Length == 0)
                {
                    elementText = ExpandWixProperties(text.InnerText);
                }
            }
            else
            {
                if (textElement.Attributes["Text"] != null)
                {
                    elementText = ExpandWixProperties(textElement.Attributes["Text"].Value);
                }
            }

            return elementText;
        }

        private string GetFromXmlElement(XmlNode textElement, string propertyToGet)
        {
            string textValue = String.Empty;

            if (textElement.Attributes[propertyToGet] != null)
            {
                textValue = ExpandWixProperties(textElement.Attributes[propertyToGet].Value);
            }
            else
            {
                XmlNode text = textElement.SelectSingleNode("wix:" + propertyToGet, wixFiles.WxsNsmgr);
                if (text != null)
                {
                    if (text.Attributes["Value"] != null)
                    {
                        textValue = text.Attributes["Value"].Value;
                    }
                    else
                    {
                        textValue = text.InnerText;
                    }

                    textValue = ExpandWixProperties(textValue);
                }
            }

            return textValue;
        }

        private Stream GetBinaryStream(string binaryId)
        {
            XmlNode binaryNode = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:Binary[@Id='{0}']", binaryId), wixFiles.WxsNsmgr);
            if (binaryNode == null)
            {
                throw new Exception(String.Format("Binary with id \"{0}\" not found", binaryId));
            }

            XmlAttribute srcAttrib = binaryNode.Attributes["SourceFile"];
            if (srcAttrib == null)
            {
                srcAttrib = binaryNode.Attributes["src"];
            }

            if (srcAttrib == null)
            {
                throw new Exception(String.Format("src Attribute of binary with id \"{0}\" not found", binaryId));
            }

            string src = srcAttrib.Value;
            if (src == null || src.Length == 0)
            {
                throw new Exception(String.Format("src Attribute of binary with id \"{0}\" is invalid.", binaryId));
            }

            if (Path.IsPathRooted(src))
            {
                if (File.Exists(src) == false)
                {
                    throw new FileNotFoundException(String.Format("File of binary with id \"{0}\" is not found.", binaryId), src);
                }

                return File.Open(src, FileMode.Open);
            }
            else
            {
                src = Path.Combine(wixFiles.WxsDirectory.FullName, src);
                if (File.Exists(src))
                {
                    return File.Open(src, FileMode.Open);
                }
                else
                {
                    FileInfo[] files = wixFiles.WxsDirectory.GetFiles(src);
                    if (files.Length != 1)
                    {
                        throw new FileNotFoundException(String.Format("File of binary with id \"{0}\" is not found.", binaryId), src);
                    }

                    return files[0].OpenRead();
                }
            }
        }
    }
}
