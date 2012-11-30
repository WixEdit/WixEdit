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
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.PropertyGridExtensions;
using WixEdit.Xml;

namespace WixEdit.Settings {
	/// <summary>
	/// Form for WixEdit Settings.
	/// </summary>
	public class SettingsForm : Form 	{
        #region Controls
        protected PropertyGrid propertyGrid;
        protected ContextMenu propertyGridContextMenu;
        protected Button ok;
        protected Button cancel;
        
        #endregion

		public SettingsForm() {
            
            InitializeComponent();
		}

        #region Initialize Controls

        private void InitializeComponent() {
            Text = "WiX Edit Settings";
            Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
            ClientSize = new System.Drawing.Size(500, 450); 
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;

            propertyGrid = new CustomPropertyGrid();
            propertyGridContextMenu = new ContextMenu();

            // 
            // propertyGrid
            //
            propertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            propertyGrid.Location = new Point(0, 0);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.TabIndex = 1;
            propertyGrid.PropertySort = PropertySort.CategorizedAlphabetical;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.HelpVisible = true;
            propertyGrid.ContextMenu = propertyGridContextMenu;

            // 
            // propertyGridContextMenu
            //
            propertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            propertyGrid.SelectedObject = WixEditSettings.Instance;

            Controls.Add(propertyGrid);

            ok = new Button();
            ok.Text = "OK";
            ok.FlatStyle = FlatStyle.System;
            ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(ok);
            ok.Click += new EventHandler(OnOk);


            cancel = new Button();
            cancel.Text = "Cancel";
            cancel.FlatStyle = FlatStyle.System;
            cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(cancel);       
            cancel.Click += new EventHandler(OnCancel);

            AcceptButton = ok;
            CancelButton = cancel;     

            int padding = 2;

            int w = (ClientSize.Width - cancel.ClientSize.Width) - padding;
            int h = (ClientSize.Height - cancel.ClientSize.Height) - padding;

            cancel.Location = new Point(w, h);

            w -= ok.ClientSize.Width + padding;
            ok.Location = new Point(w, h);

            h -= ok.ClientSize.Height + padding;

            propertyGrid.Size = new Size(ClientSize.Width, ClientSize.Height - (padding*2) - ok.ClientSize.Height);
        }

        #endregion

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
        }

        private void OnOk(object sender, EventArgs e) {
            WixEditSettings.Instance.SaveChanges();
            DialogResult = DialogResult.OK;
        }
       
        private void OnCancel(object sender, EventArgs e) {
            WixEditSettings.Instance.DiscardChanges();
            DialogResult = DialogResult.Cancel;
        }        
    }
}
