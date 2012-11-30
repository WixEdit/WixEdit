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
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

using WixEdit.Controls;
using WixEdit.Xml;

namespace WixEdit.Wizard
{
    class StepSheet : BaseSheet
    {
        ErrorProviderFixed errorProvider;
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        XmlElement stepElement;
        XmlNamespaceManager xmlnsmgr;
        Dictionary<string, string> existingNsTranslations = new Dictionary<string, string>();
        Dictionary<string, XmlNode> ifNotPresentMap = new Dictionary<string, XmlNode>();
        List<string> ifNotPresentList = new List<string>();
        public StepSheet(XmlElement step, WizardForm creator) : base(creator)
        {
            this.stepElement = step;
            this.AutoScroll = true;

            errorProvider = new ErrorProviderFixed();
            errorProvider.ContainerControl = this;
            errorProvider.AutoPopDelay = 20000;
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            Bitmap b = new Bitmap(WixFiles.GetResourceStream("bmp.info.bmp"));
            errorProvider.Icon = Icon.FromHandle(b.GetHicon());
        }

        private void ExtractNamespaces(XmlElement xmlElement, XmlNamespaceManager xmlnsmgr, string refAtt)
        {
            Regex r = new Regex(@"[/\[]?(?<name>[A-Za-z0-9]+)[:]");
            foreach (Match m in r.Matches(refAtt))
            {
                string pre = m.Groups["name"].Value;
                string xmlns = "";

                XmlNode parent = xmlElement;
                while (xmlns == String.Empty && parent != null)
                {
                    xmlns = parent.GetNamespaceOfPrefix(pre);
                    parent = parent.ParentNode;
                }
                if (xmlns != String.Empty)
                {
                    string existingPre = Wizard.WixFiles.WxsNsmgr.LookupPrefix(xmlns);
                    if (existingPre != null)
                    {
                        xmlnsmgr.AddNamespace(existingPre, xmlns);
                        if (existingNsTranslations.ContainsKey(pre) == false)
                        {
                            existingNsTranslations.Add(pre, existingPre);
                        }
                    }
                    else
                    {
                        xmlnsmgr.AddNamespace(pre, xmlns);
                    }
                }
            }
        }

        public override bool UndoNext()
        {
            Wizard.WixFiles.UndoManager.Undo();

            return true;
        }

        public override void OnShow()
        {
            ifNotPresentList.Clear();
            Controls.Clear();

            titleLabel = new Label();
            titleLabel.Text = stepElement.SelectSingleNode("Title").InnerText;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 20;
            titleLabel.Left = 0;
            titleLabel.Top = 0;
            titleLabel.Padding = new Padding(5, 5, 5, 0);
            titleLabel.Font = new Font("Verdana",
                        10,
                        FontStyle.Bold,
                        GraphicsUnit.Point
                    );
            titleLabel.BackColor = Color.White;

            descriptionLabel = new Label();
            descriptionLabel.Text = stepElement.SelectSingleNode("Description").InnerText;
            descriptionLabel.Dock = DockStyle.Top;
            descriptionLabel.Height = 50 - titleLabel.Height;
            descriptionLabel.Left = 0;
            descriptionLabel.Top = titleLabel.Height;
            descriptionLabel.Padding = new Padding(8, 3, 5, 0);
            descriptionLabel.BackColor = Color.White;

            this.Controls.Add(descriptionLabel);

            this.Controls.Add(titleLabel);


            lineLabel = new Label();
            lineLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lineLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lineLabel.Location = new System.Drawing.Point(0, titleLabel.Height + descriptionLabel.Height);
            lineLabel.Size = new System.Drawing.Size(this.Width, 2);

            this.Controls.Add(lineLabel);

            Control prevControl = lineLabel;

            xmlnsmgr = new XmlNamespaceManager(stepElement.OwnerDocument.NameTable);

            // Check the TemplatePart@SelectionTarget, then the first control
            // should be a selection control.

            foreach (XmlElement templatePartNode in stepElement.SelectNodes("TemplatePart"))
            {
                String ifNotPresent = templatePartNode.GetAttribute("IfNotPresent");
                if (!String.IsNullOrEmpty(ifNotPresent))
                {
                    XmlNodeList ifNotPresentNodes = Wizard.WixFiles.WxsDocument.SelectNodes(ifNotPresent, Wizard.WixFiles.WxsNsmgr);
                    if (ifNotPresentNodes.Count > 0)
                    {
                        ifNotPresentList.Add(ifNotPresent);
                        continue;
                    }
                }

                XmlElement templatePart = (XmlElement)templatePartNode;
                String selectionTarget = templatePart.GetAttribute("SelectionTarget");
                if (selectionTarget != null && selectionTarget != String.Empty)
                {
                    Label label = new Label();
                    label.Width = this.Width - 10;
                    label.Height = 14;
                    label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    label.Text = "Select target location";

                    String selectionTargetDescription = templatePart.GetAttribute("Description");
                    if (selectionTargetDescription != null && selectionTargetDescription != String.Empty)
                    {
                        label.Text = selectionTargetDescription;
                    }

                    label.Top = prevControl.Bottom + 4;
                    label.Left = 5;
                    this.Controls.Add(label);

                    ComboBox text = new ComboBox();
                    text.DropDownStyle = ComboBoxStyle.DropDownList;
                    foreach (XmlNode dir in Wizard.WixFiles.WxsDocument.SelectNodes(selectionTarget, Wizard.WixFiles.WxsNsmgr))
                    {
                        text.Items.Add(dir.Attributes["Id"]);
                    }
                    text.DisplayMember = "Value";
                    text.Width = this.Width - 14;
                    text.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    // text.Items...
                    text.Top = prevControl.Bottom + label.Height + 4;
                    text.Left = 7;
                    text.Name = selectionTarget;
                    this.Controls.Add(text);

                    prevControl = text;
                }
            }

            foreach (XmlElement edit in stepElement.SelectNodes("Edit"))
            {
                string editMode = edit.GetAttribute("Mode");
                if (editMode == "GenerateGuid" || editMode == "CopyFromTarget")
                {
                    continue;
                }

                string refAtt = edit.GetAttribute("Ref");
                ExtractNamespaces(edit, xmlnsmgr, refAtt);

                // TODO: What if this edit thingie is in the template part that is not shown due to IfNotPresent? 
                // Check the template part from theNode

                XmlNode theNode = stepElement.SelectSingleNode("TemplatePart/" + TranslateNamespace(refAtt), xmlnsmgr);
                if (theNode != null) // Could be that the IfNotPresent prevents it.
                {
                    XmlElement theTemplateNode = (XmlElement)stepElement.SelectSingleNode("TemplatePart[" + TranslateNamespace(refAtt) + "]", xmlnsmgr);
                    String ifNotPresent = theTemplateNode.GetAttribute("IfNotPresent");
                    if (!String.IsNullOrEmpty(ifNotPresent))
                    {
                        if (ifNotPresentList.Contains(ifNotPresent))
                        {
                            continue;
                        }
                    }

                    Label label = new Label();
                    label.Width = this.Width - 10;
                    label.Height = 14;
                    label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    label.Text = edit.GetAttribute("Description");
                    if (label.Text == String.Empty)
                    {
                        label.Text = edit.GetAttribute("Name");
                    }
                    if (label.Text == String.Empty)
                    {
                        label.Text = refAtt.Replace('/', ' ').Replace('[', ' ').Replace(']', ' ').Replace(':', ' ').Replace('@', ' ').Replace("  ", " ");
                    }
                    label.Top = prevControl.Bottom + 4;
                    label.Left = 5;
                    this.Controls.Add(label);

                    XmlDocumentationManager mgr = new XmlDocumentationManager(this.Wizard.WixFiles);
                    XmlNode xmlNodeDefinition = mgr.GetXmlNodeDefinition(theNode);

                    switch (editMode)
                    {
                        case "Select":
                            ComboBox select = new ComboBox();
                            select.DropDownStyle = ComboBoxStyle.DropDownList;
                            select.Width = this.Width - 14;
                            select.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                            String selectionTarget = edit.GetAttribute("Selection");

                            foreach (XmlNode dir in Wizard.WixFiles.WxsDocument.SelectNodes(selectionTarget, Wizard.WixFiles.WxsNsmgr))
                            {
                                select.Items.Add(dir);
                            }

                            select.DisplayMember = "Value";
                            select.Top = prevControl.Bottom + label.Height + 4;
                            select.Left = 7;
                            select.Name = refAtt;
                            this.Controls.Add(select);

                            prevControl = select;
                            break;
                        case "Dropdown":
                            ComboBox combo = new ComboBox();
                            combo.DropDownStyle = ComboBoxStyle.DropDownList;
                            combo.Width = this.Width - 14;
                            combo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                            combo.DisplayMember = "InnerText";
                            foreach (XmlNode optionNode in edit.SelectNodes("Option"))
                            {
                                XmlElement optionElement = (XmlElement)optionNode;
                                combo.Items.Add(optionNode);
                                if (optionElement.GetAttribute("Value") == theNode.InnerText)
                                {
                                    combo.SelectedItem = optionNode;
                                }
                            }

                            combo.Top = prevControl.Bottom + label.Height + 4;
                            combo.Left = 7;
                            combo.Name = refAtt;
                            this.Controls.Add(combo);

                            prevControl = combo;
                            break;
                        default:
                            TextBox text = new TextBox();
                            text.Width = this.Width - 14;
                            text.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                            text.Text = theNode.Value;
                            text.Top = prevControl.Bottom + label.Height + 4;
                            text.Left = 7;
                            text.Name = refAtt;
                            this.Controls.Add(text);

                            prevControl = text;
                            break;
                    }

                    if (xmlNodeDefinition != null)
                    {
                        string docu = mgr.GetDocumentation(xmlNodeDefinition, true);
                        if (!string.IsNullOrEmpty(docu))
                        {
                            prevControl.Width = prevControl.Width - 18;
                            errorProvider.SetError(prevControl, docu);
                            errorProvider.SetIconPadding(prevControl, 4);
                        }
                    }
                }
            }
        }

        public override bool OnNext()
        {
            Wizard.WixFiles.UndoManager.BeginNewCommandRange();

            try
            {
                XmlElement stepElementClone = (XmlElement) this.stepElement.Clone();

                // Update the values in the clone element.
                foreach (XmlElement edit in stepElementClone.SelectNodes("Edit"))
                {
                    string refAtt = edit.GetAttribute("Ref");
                    string theNodeValue = "";
                    if (edit.GetAttribute("Mode") == "GenerateGuid")
                    {
                        theNodeValue = Guid.NewGuid().ToString().ToUpper();
                    }
                    else if (edit.GetAttribute("Mode") == "Dropdown")
                    {
                        ComboBox combo = Controls.Find(refAtt, true)[0] as ComboBox;
                        if (combo != null)
                        {
                            XmlElement item = combo.SelectedItem as XmlElement;
                            if (item != null)
                            {
                                theNodeValue = item.GetAttribute("Value");
                            }
                        }
                    }
                    else if (edit.GetAttribute("Mode") == "CopyFromTarget")
                    {
                        XmlNode target = null;
                        Control[] controls = Controls.Find(edit.GetAttribute("Target"), true);
                        if (controls.Length > 0)
                        {
                            ComboBox combo = (ComboBox)controls[0];

                            XmlAttribute att = (XmlAttribute)combo.SelectedItem;
                            target = att.OwnerElement;
                        }
                        else
                        {
                            target = stepElementClone.SelectSingleNode("TemplatePart[@Target='" + edit.GetAttribute("Target") + "']", xmlnsmgr);
                        }
                        
                        XmlNode valueNode = target.SelectSingleNode(edit.GetAttribute("RelativePath"), xmlnsmgr);
                        theNodeValue = valueNode.Value;
                    }
                    else
                    {
                        Control[] ctrls = Controls.Find(refAtt, true);
                        if (ctrls.Length == 0)
                        {
                            // Possible IfNotPresent set
                            continue;
                        }

                        Control ctrl = ctrls[0];
                        if (ctrl != null)
                        {
                            theNodeValue = ctrl.Text;
                        }
                    }

                    string formatString = edit.GetAttribute("FormatString");
                    if (!String.IsNullOrEmpty(formatString))
                    {
                        theNodeValue = String.Format(formatString, theNodeValue);
                    }

                    XmlNodeList theNodeList = stepElementClone.SelectNodes("TemplatePart/" + TranslateNamespace(refAtt), xmlnsmgr);

                    if (theNodeList.Count == 0)
                    {
                        MessageBox.Show(refAtt + " could not be found", "Node not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(theNodeValue) &&
                            String.Compare(edit.GetAttribute("RemoveEmptyAttribute"), "true", true) == 0)
                        {
                            foreach (XmlNode theNode in theNodeList)
                            {
                                XmlAttribute theAtt = theNode as XmlAttribute;
                                if (theAtt != null)
                                {
                                    theAtt.OwnerElement.RemoveAttributeNode(theAtt);
                                }
                            }
                        }
                        else
                        {
                            foreach (XmlNode theNode in theNodeList)
                            {
                                if (theNode.NodeType == XmlNodeType.Element)
                                {
                                    theNode.InnerText = theNodeValue;
                                }
                                else
                                {
                                    theNode.Value = theNodeValue;
                                }
                            }
                        }
                    }
                }

                foreach (XmlElement templatePart in stepElementClone.SelectNodes("TemplatePart"))
                {
                    string ifNotPresent = templatePart.GetAttribute("IfNotPresent");
                    if (ifNotPresentList.Contains(ifNotPresent))
                    {
                        continue;
                    }

                    // Import the nodes into the wxs file.
                    List<XmlNode> importedNodes = new List<XmlNode>();
                    foreach (XmlNode toImport in templatePart.ChildNodes)
                    {
                        importedNodes.Add(Wizard.WixFiles.WxsDocument.ImportNode(toImport, true));
                    }

                    XmlNode target = null;
                    if (templatePart.HasAttribute("SelectionTarget"))
                    {
                        // Get the selection target and filter by selected Id.
                        string selectionTarget = templatePart.GetAttribute("SelectionTarget");
                        ComboBox combo = (ComboBox)Controls.Find(selectionTarget, true)[0];
                        XmlAttribute att = (XmlAttribute)combo.SelectedItem;
                        target = att.OwnerElement;
                    }
                    else
                    {
                        // Fix the prefix of namespaces, namespaces could 
                        // have a different prefix in the templatepart.
                        target = Wizard.WixFiles.WxsDocument.SelectSingleNode(TranslateNamespace(templatePart.GetAttribute("Target")), Wizard.WixFiles.WxsNsmgr);
                    }

                    foreach (XmlNode imported in importedNodes)
                    {
                        target.AppendChild(imported);

                        if (existingNsTranslations.Count > 0)
                        {
                            FixPrefixRecursive(imported);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void FixPrefixRecursive(XmlNode imported)
        {
            if (existingNsTranslations.ContainsKey(imported.Prefix))
            {
                if (Wizard.WixFiles.WxsDocument.DocumentElement.NamespaceURI == xmlnsmgr.LookupNamespace(imported.Prefix))
                {
                    imported.Prefix = "";
                }
                else
                {
                    imported.Prefix = existingNsTranslations[imported.Prefix];
                }
            }

            foreach (XmlNode node in imported.ChildNodes)
            {
                FixPrefixRecursive(node);
            }
        }

        private string TranslateNamespace(string toFix)
        {
            SortedList<int, int> hits = new SortedList<int, int>();
            Dictionary<int, string> nsMap = new Dictionary<int, string>();

            Regex r = new Regex(@"[/\[]?(?<name>[A-Za-z0-9]+)[:]");
            foreach (Match m in r.Matches(toFix))
            {
                string pre = m.Groups["name"].Value;
                if (existingNsTranslations.ContainsKey(pre))
                {
                    pre = existingNsTranslations[pre];
                }
                string ns = xmlnsmgr.LookupNamespace(pre);
                string newPre = Wizard.WixFiles.WxsNsmgr.LookupPrefix(ns);

                if (newPre == null)
                {
                    continue;
                }

                hits.Add(m.Groups["name"].Index, m.Groups["name"].Length);
                nsMap.Add(m.Groups["name"].Index, newPre);
            }

            string result = toFix;
            for (int i = hits.Count-1; i >= 0; i--)
            {
                int key = hits.Keys[i];
                int val = hits[key];

                result = result.Substring(0, key) + nsMap[key] + result.Substring(key + val);
            }

            return result;
        }
    }
}
