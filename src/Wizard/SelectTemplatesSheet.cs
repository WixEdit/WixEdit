using System;
using System.Collections.Generic;
using System.Text;
using WixEdit.Wizard;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using WixEdit.Import;
using WixEdit.Server;
using System.IO;
using WixEdit.Settings;

namespace WixEdit.Wizard
{
    class SelectTemplatesSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        CheckedListBox checkList;
        GroupBox templateDescriptionGroupBox;
        Panel templateDescriptionPanel;
        Label templateDescriptionLabel;

        public SelectTemplatesSheet(WizardForm creator)
            : base(creator)
        {
            this.AutoScroll = true;

            titleLabel = new Label();
            titleLabel.Text = "Select featues to add";
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
            descriptionLabel.Text = "Select functionality you want to add to the installer";
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
            lineLabel.Location = new Point(0, titleLabel.Height + descriptionLabel.Height);
            lineLabel.Size = new Size(this.Width, 2);

            this.Controls.Add(lineLabel);

            checkList = new CheckedListBox();
            checkList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            checkList.Location = new Point(4, lineLabel.Top + lineLabel.Height + 5);
            checkList.Width = this.Width - 8 - 190;
            checkList.Height = this.Height - checkList.Top - 5;
            checkList.DisplayMember = "Value";
            checkList.Sorted = true;
            checkList.SelectedIndexChanged += new EventHandler(checkList_SelectedIndexChanged);

            this.Controls.Add(checkList);

            templateDescriptionGroupBox = new GroupBox();
            templateDescriptionGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            templateDescriptionGroupBox.Location = new Point(checkList.Width + 12, checkList.Top);
            templateDescriptionGroupBox.Width = 190 - 8;
            templateDescriptionGroupBox.Height = checkList.Height;
            templateDescriptionGroupBox.Text = "Description";

            this.Controls.Add(templateDescriptionGroupBox);

            templateDescriptionPanel = new Panel();
            templateDescriptionPanel.Dock = DockStyle.Fill;
            templateDescriptionPanel.AutoScroll = true;
            templateDescriptionGroupBox.Controls.Add(templateDescriptionPanel);

            templateDescriptionLabel = new Label();
            //templateDescriptionLabel.Width = templateDescriptionPanel.Width;
            templateDescriptionLabel.Dock = DockStyle.Fill;
            templateDescriptionLabel.Text = "";
            templateDescriptionLabel.Visible = false;

            templateDescriptionPanel.Controls.Add(templateDescriptionLabel);

            this.Resize += new EventHandler(SelectTemplatesSheet_Resize);

            DirectoryInfo oldTemplateDir = null;
            DirectoryInfo templateDir = null;

            if (!String.IsNullOrEmpty(WixEditSettings.Instance.TemplateDirectory))
            {
                oldTemplateDir = new DirectoryInfo(WixEditSettings.Instance.TemplateDirectory);
                templateDir = new DirectoryInfo(Path.Combine(oldTemplateDir.Parent.FullName, "wizard"));
            }

            if (templateDir != null && 
                templateDir.Exists)
            {
                FileInfo[] files = templateDir.GetFiles("template.xml", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    if (file.Directory.Parent.FullName == templateDir.FullName)
                    {
                        string title = file.Directory.Name;

                        XmlDocument doc = new XmlDocument();
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

                        doc.Load(file.FullName);
                        XmlElement template = (XmlElement)doc.SelectSingleNode("/Template");

                        checkList.Items.Add(template.Attributes["Title"]);
                    }
                }
            }
            else
            {
                StringBuilder message = new StringBuilder("Directory containing the templates could not be found. Please reinstall WixEdit.");
                if (templateDir != null)
                {
                    message.AppendFormat("\r\n\r\nDirectory:\r\n{0}", templateDir.FullName);
                }

                MessageBox.Show(message.ToString(), "Templates not found");
            }
        }

        public override bool OnNext()
        {
            foreach (XmlAttribute titleAtt in checkList.CheckedItems)
            {
                XmlElement template = titleAtt.OwnerElement;
                Wizard.AddTemplate(template);
            }

            return base.OnNext();
        }

        public override bool UndoNext()
        {
            int numberOfTemplates = checkList.CheckedItems.Count;

            for (int i = 0; i < numberOfTemplates; i++)
            {
                Wizard.RemoveLastAddedTemplate();
            }

            return base.UndoNext();
        }

        void SelectTemplatesSheet_Resize(object sender, EventArgs e)
        {
            if (checkList.Height != templateDescriptionGroupBox.Height)
            {
                templateDescriptionGroupBox.Height = checkList.Height;
            }
        }

        void checkList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkList.SelectedIndex < 0)
            {
                templateDescriptionLabel.Text = "";
                templateDescriptionLabel.Visible = false;
            }
            else
            {
                XmlAttribute titleAtt = checkList.SelectedItem as XmlAttribute;
                XmlElement template = titleAtt.OwnerElement;
                StringBuilder text = new StringBuilder(template.GetAttribute("Description"));
                text.Replace(@"\r\n", "\r\n");
                text.Replace(@"\r", "\r\n");
                text.Replace(@"\n", "\r\n");
                text.Replace(@"\t", "    ");
                templateDescriptionLabel.Text = text.ToString();
                templateDescriptionLabel.Visible = true;
                templateDescriptionLabel.Height = templateDescriptionLabel.GetPreferredSize(new Size(templateDescriptionLabel.Width, 1000)).Height;
            }
        }
    }
}
