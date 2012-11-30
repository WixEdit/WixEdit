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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Globalization;
using System.Text;

namespace WixEdit.Controls
{
    /// <summary>
    /// Base on http://msdn.microsoft.com/en-us/library/ms229644.aspx
    /// </summary>
	public class NumericTextBox : TextBox
	{
		bool allowSpace = false;

		// Restricts the entry of characters to digits (including hex), the negative sign,
		// the decimal point, and editing keystrokes (backspace).
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
			string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
			string groupSeparator = numberFormatInfo.NumberGroupSeparator;
			string negativeSign = numberFormatInfo.NegativeSign;

			string keyInput = e.KeyChar.ToString();

			if (Char.IsDigit(e.KeyChar))
			{
				// Digits are OK
			}
			else if (keyInput.Equals(decimalSeparator) || keyInput.Equals(groupSeparator) ||
			 keyInput.Equals(negativeSign))
			{
				// Decimal separator is OK
			}
			else if (e.KeyChar == '\b')
			{
				// Backspace key is OK
			}
			//    else if ((ModifierKeys & (Keys.Control | Keys.Alt)) != 0)
			//    {
			//     // Let the edit control handle control and alt key combinations
			//    }
			else if (this.allowSpace && e.KeyChar == ' ')
			{

			}
			else
			{
				// Consume this invalid key and beep
				e.Handled = true;
				//    MessageBeep();
			}
		}

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    StringBuilder newValue = new StringBuilder();

                    try
                    {

                        foreach (char c in value)
                        {
                            if (char.IsDigit(c))
                            {
                                newValue.Append(c);
                            }
                        }
                    }
                    catch { }

                    base.Text = newValue.ToString();
                }
                else
                {
                    base.Text = value;
                }
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {

            base.OnTextChanged(e);
        }

		public int IntValue
		{
			get
			{
				return Int32.Parse(this.Text);
			}
		}

		public decimal DecimalValue
		{
			get
			{
				return Decimal.Parse(this.Text);
			}
		}

		public bool AllowSpace
		{
			set
			{
				this.allowSpace = value;
			}

			get
			{
				return this.allowSpace;
			}
		}
	}
}