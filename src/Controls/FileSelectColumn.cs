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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using WixEdit.Xml;

namespace WixEdit.Controls
{
    //  The base object for the custom column type.  Programmers manipulate
    //  the column types most often when working with the DataGridView, and
    //  this one sets the basics and Cell Template values controlling the
    //  default behaviour for cells of this column type.
    public class FileSelectColumn : DataGridViewColumn
    {
        WixFiles wixFiles;

        //  Initializes a new instance of this class, making sure to pass
        //  to its base constructor an instance of a FileSelectCell 
        //  class to use as the basic template.
        public FileSelectColumn() : base(new FileSelectCell())
        {
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

                FileSelectCell cellTemplate = (FileSelectCell)CellTemplate;
                cellTemplate.WixFiles = wixFiles;
            }
        }

        public override object Clone()
        {
            FileSelectCell thisCellTemplate = (FileSelectCell)CellTemplate;

            FileSelectColumn clone = (FileSelectColumn)base.Clone();
            FileSelectCell cellTemplate = (FileSelectCell)clone.CellTemplate;
            cellTemplate.WixFiles = thisCellTemplate.WixFiles;

            return clone;
        }

        //  Routine to convert from boolean to DataGridViewTriState.
        private static DataGridViewTriState TriBool(bool value)
        {
            return value ? DataGridViewTriState.True
                         : DataGridViewTriState.False;
        }


        //  The template cell that will be used for this column by default,
        //  unless a specific cell is set for a particular row.
        //
        //  A FileSelectCell cell which will serve as the template cell
        //  for this column.
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }

            set
            {
                //  Only cell types that derive from FileSelectCell are supported as the cell template.
                if (value != null && !value.GetType().IsAssignableFrom(typeof(FileSelectCell)))
                {
                    string s = "Cell type is not based upon the FileSelectCell.";//CustomColumnMain.GetResourceManager().GetString("excNotFileSelect");
                    throw new InvalidCastException(s);
                }

                base.CellTemplate = value;
            }
        }
    }
}
