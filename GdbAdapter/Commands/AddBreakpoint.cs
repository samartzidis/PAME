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
using System.Text.RegularExpressions;

namespace GdbAdapter.Commands
{    
    /// <summary>
    /// 
    /// </summary>
    public class AddBreakpoint : Command<BreakpoitInfo>
    {
        protected static Regex regex = null;
        protected string location = null;

        static AddBreakpoint()
        {
            //eg. "Breakpoint 2 at 0x401030: file test.pas, line 1."
            regex = new Regex(@"^Breakpoint\s+([0-9]+)\s+at\s+(.+):\s+file\s+(.+),\s+line\s+([0-9]+).",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public AddBreakpoint(GdbEngine gdb, string location) : base(gdb)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            this.location = location;
        }

        public override string CommandString
        {
            get { return "break " + location; }
        }

        public override void SetReturnValue(GDBMIParser.GDBMIResponse resp)
        {
            foreach (GDBMIParser.GDBMIStreamRecord sr in resp.stream)
            {
                if (sr.type != GDBMIParser.GDBMIStreamRecord.GDBMIStreamRecordType.console)
                    continue;

                Match match = regex.Match(sr.str);
                if (match.Success)
                {
                    Return = new BreakpoitInfo();
                    Return.raw = match.Groups[0].Value;

                    try
                    {
                        Return.id = Int32.Parse(match.Groups[1].Value);
                        Return.file = match.Groups[3].Value;
                        Return.line = Int32.Parse(match.Groups[4].Value);
                    }
                    catch (Exception m)
                    {
                        throw new ExecutionException("Failed to parse response value while parsing \" " + sr.str + "\"", m);
                    }

                    break;
                }
            }
        }        
    }
}
