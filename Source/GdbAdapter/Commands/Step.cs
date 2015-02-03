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
    public abstract class Step : Command<StepLocation>
    {
        protected static Regex regex = null;

        static Step()
        {
            //eg. "12\t\tc[1] := 'a''''b';\n"
            regex = new Regex(@"^([0-9]+)\t(.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public Step(GdbEngine gdb)
            : base(gdb)
        {
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
                    Return = new StepLocation();
                    Return.raw = match.Groups[0].Value;
                    int parsed = 0;
                    if (Int32.TryParse(match.Groups[1].Value, out parsed))
                        Return.line = parsed;
                }
            }
        }
    }

    public class StepIn : Step
    {
        public StepIn(GdbEngine gdb) : base(gdb)
        {
        }

        public override string CommandString
        {
            get {return "step";}
        } 
    }

    public class StepOut : Step
    {
        public StepOut(GdbEngine gdb)
            : base(gdb)
        {
        }

        public override string CommandString
        {
            get { return "return"; }
        }
    }

    public class StepOver : Step
    {        
        public StepOver(GdbEngine gdb)
            : base(gdb)
        {
        }

        public override string CommandString
        {
            get { return "next"; }
        }        
    }

    public class StepContinue : Step
    {
        public StepContinue(GdbEngine gdb)
            : base(gdb)
        {
        }

        public override string CommandString
        {
            get { return "continue"; }
        }
    }
}
