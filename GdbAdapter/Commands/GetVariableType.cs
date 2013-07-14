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
    public class GetVariableType : Command<string>
    {
        protected static Regex regexDecl = null;
        protected string name = null;

        static GetVariableType()
        {
            //eg. "type = SMALLINT"
            regexDecl = new Regex(@"^(.+)\s+=\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public GetVariableType(GdbEngine gdb, string name)
            : base(gdb)
        {
            this.name = name;
        }

        public override string CommandString
        {
            get { return "ptype " + name; }
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
                    Return = match.Groups[2].Value;
                }
            }
        }
    }
}
