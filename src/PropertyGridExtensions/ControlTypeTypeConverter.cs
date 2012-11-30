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
using System.ComponentModel;
using System.Xml;

namespace WixEdit.PropertyGridExtensions {
    public class ControlTypeTypeConverter : StringConverter {
        // From http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/controls.asp
        string[] controlTypes = new string[] {   "Billboard",
                                                 "Bitmap",
                                                 "CheckBox",
                                                 "ComboBox",
                                                 "DirectoryCombo",
                                                 "DirectoryList",
                                                 "Edit",
                                                 "GroupBox",
                                                 "Icon",
                                                 "Line",
                                                 "ListBox",
                                                 "ListView",
                                                 "MaskedEdit",
                                                 "PathEdit",
                                                 "ProgressBar",
                                                 "PushButton",
                                                 "RadioButtonGroup",
                                                 "ScrollableText",
                                                 "SelectionTree",
                                                 "Text",
                                                 "VolumeCostList",
                                                 "VolumeSelectCombo" };
        
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
        
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(controlTypes);
        } 
        
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            // If any controls are added, make sure it's possible to type them manually.
            return false;
        }
    }
}