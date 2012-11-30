//---------------------------------------------------------------------
//  This file is part of the Microsoft .NET Framework SDK Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
// 
//This source code is intended only as a supplement to Microsoft
//Development Tools and/or on-line documentation.  See these other
//materials for detailed information regarding Microsoft code samples.
// 
//THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
//KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//PARTICULAR PURPOSE.
//---------------------------------------------------------------------

using System;
using System.Windows.Forms;

using WixEdit.Xml;

namespace WixEdit.Controls
{
    class FileSelectCell : DataGridViewTextBoxCell
    {
        WixFiles wixFiles;

        //=------------------------------------------------------------------=
        // FileSelectCell
        //=------------------------------------------------------------------=
        /// <summary>
        ///   Initializes a new instance of this class.  Fortunately, there's
        ///   not much to do here except make sure that our base class is 
        ///   also initialized properly.
        /// </summary>
        /// 
        public FileSelectCell()
            : base()
        {
        }

        public override object Clone()
        {
            FileSelectCell clone = (FileSelectCell)base.Clone();
            clone.WixFiles = this.wixFiles;

            return clone;
        }

        public WixFiles WixFiles
        {
            get
            {
                return wixFiles;
            }
            set
            {
                wixFiles = value;

                //if (DataGridView != null && DataGridView.EditingControl != null)
                //{
                //    FileSelectEditingControl mtbec = DataGridView.EditingControl as FileSelectEditingControl;

                //    mtbec.WixFiles = wixFiles;
                //}
            }
        }

        ///   Whenever the user is to begin editing a cell of this type, the editing
        ///   control must be created, which in this column type's
        ///   case is a subclass of the FileSelect control.
        /// 
        ///   This routine sets up all the properties and values
        ///   on this control before the editing begins.
        public override void InitializeEditingControl(int rowIndex,
                                                      object initialFormattedValue,
                                                      DataGridViewCellStyle dataGridViewCellStyle)
        {
            FileSelectEditingControl mtbec;
            FileSelectColumn mtbcol;
            DataGridViewColumn dgvc;

            base.InitializeEditingControl(rowIndex, initialFormattedValue,
                                          dataGridViewCellStyle);

            mtbec = DataGridView.EditingControl as FileSelectEditingControl;

            mtbec.WixFiles = wixFiles;

            //
            // set up props that are specific to the FileSelect
            //

            dgvc = this.OwningColumn;   // this.DataGridView.Columns[this.ColumnIndex];
            if (dgvc is FileSelectColumn)
            {
                mtbcol = dgvc as FileSelectColumn;

                mtbec.Text = (string)this.Value;
            }
        }

        //  Returns the type of the control that will be used for editing
        //  cells of this type.  This control must be a valid Windows Forms
        //  control and must implement IDataGridViewEditingControl.
        public override Type EditType
        {
            get
            {
                return typeof(FileSelectEditingControl);
            }
        }

        //   Quick routine to convert from DataGridViewTriState to boolean.
        //   True goes to true while False and NotSet go to false.
        protected static bool BoolFromTri(DataGridViewTriState tri)
        {
            return (tri == DataGridViewTriState.True) ? true : false;
        }
    }
}
