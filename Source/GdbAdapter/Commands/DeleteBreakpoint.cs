//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using GdbAdapter.Parsers;

namespace GdbAdapter.Commands
{
    public class DeleteBreakpoint : VoidCommand
    {
        protected BreakpoitInfo bi = null;

        public DeleteBreakpoint(GdbEngine gdb, BreakpoitInfo bi) : base(gdb)
        {
            if (bi == null)
                throw new ArgumentNullException("bi");

            this.bi = bi;
        }

        public override string CommandString
        {
            get { return "delete " + bi.id; }
        }
    }
}
