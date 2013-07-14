//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GdbAdapter.Commands
{
    public class BreakpoitInfo
    {
        public string file = null;
        public int line = 0;
        public int id = 0;
        public string raw = null;
    }

    public class VariableInfo
    {
        public string file = null;
        public string name = null;
        public string value = null;
        public string type = null;
        public string raw = null;
    }

    public class ThreadInfo
    {
        public string pid;
        public string tid;
        public string raw;
    }

    public class StepLocation
    {
        public string raw = null;
        public int? line = 0;
    }

    public class LocationInfo
    {
        public int? line = 0;
        public int? frame = 0;
        public string file = null;
        public string address = null;
        public string raw = null;
    }
}
