//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GdbAdapter.Parsers;
using System.Text.RegularExpressions;

namespace GdbAdapter.Commands
{    
    public class GetThreads : Command<List<ThreadInfo>>
    {
        protected static Regex regexDecl = null;

        static GetThreads()
        {
            //eg. "* 1 thread 5072.0x120c  $main () at C:/Temp/test.pas:8"
            regexDecl = new Regex(@"^(?:.+)\s+thread\s+(\d+).(\w+)\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public GetThreads(GdbEngine gdb)
            : base(gdb)
        {
            Return = new List<ThreadInfo>();
        }

        public override string CommandString
        {
            get { return "info threads "; }
        }

        public override void SetReturnValue(GDBMIParser.GDBMIResponse resp)
        {            
            foreach (GDBMIParser.GDBMIStreamRecord sr in resp.stream)
            {
                if (sr.type != GDBMIParser.GDBMIStreamRecord.GDBMIStreamRecordType.console)
                    continue;

                Match match = regexDecl.Match(sr.str);
                if (match.Success)
                {
                    ThreadInfo ti = new ThreadInfo();
                    ti.raw = match.Groups[0].Value;
                    ti.pid = match.Groups[1].Value;
                    ti.tid = match.Groups[2].Value;
                    Return.Add(ti);
                }
            }
        }
    }
}
