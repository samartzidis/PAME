//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pame
{
    public class ScopedCursor : IDisposable
    {
        protected Cursor oldCursor = null;

        protected ScopedCursor()
        {
        }

        public ScopedCursor(Cursor c)
        {
            oldCursor = Cursor.Current;
            Cursor.Current = c;
        }

        ~ScopedCursor()
        {
            if (oldCursor != null)
                Cursor.Current = oldCursor;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (oldCursor != null)
                Cursor.Current = oldCursor;
        }

        #endregion
    }
}
