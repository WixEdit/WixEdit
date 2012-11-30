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
using System.Xml;

using WixEdit.Settings;

namespace WixEdit.Xml
{
    public class IncludeFileChangedException : Exception
    {
        UndoManager undoMgr;
        IReversibleCommand cmd;
        bool notifyUser;
        public IncludeFileChangedException(UndoManager undoMgr, IReversibleCommand cmd, bool notifyUser)
        {
            this.undoMgr = undoMgr;
            this.cmd = cmd;
            this.notifyUser = notifyUser;
        }

        public UndoManager UndoManager
        {
            get
            {
                return undoMgr;
            }
        }

        public IReversibleCommand Command
        {
            get
            {
                return cmd;
            }
        }

        public bool NotifyUser
        {
            get
            {
                return notifyUser;
            }
        }
    }
}