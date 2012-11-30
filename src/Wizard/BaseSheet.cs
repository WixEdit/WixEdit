using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace WixEdit.Wizard
{
    public partial class BaseSheet : UserControl
    {
        WizardForm wizard;

        public BaseSheet()
        {
            InitializeComponent();
        }

        public BaseSheet(WizardForm creator)
        {
            wizard = creator;

            InitializeComponent();
        }

        public WizardForm Wizard
        {
            get { return wizard; }
            set { wizard = value; }
        }

        public virtual bool OnNext()
        {
            return true;
        }

        public virtual bool OnBack()
        {
            return true;
        }

        public virtual bool OnCancel()
        {
            return true;
        }

        public virtual void OnShow()
        {
        }

        public virtual bool UndoNext()
        {
            return true;
        }
    }
}
