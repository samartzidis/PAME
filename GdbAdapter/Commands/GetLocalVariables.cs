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
    public class GetLocalVariables : Command<List<VariableInfo>>
    {
        protected static Regex regexDecl = null;

        static GetLocalVariables()
        {
            //eg. "AA = 10"
            regexDecl = new Regex(@"^(\w+)\s+=\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public GetLocalVariables(GdbEngine gdb)
            : base(gdb)
        {
        }

        public override string CommandString
        {
            get { return "info locals"; }
        }

        public override void SetReturnValue(GDBMIParser.GDBMIResponse resp)
        {
            Return = new List<VariableInfo>();

            int level = 0;
            foreach (GDBMIParser.GDBMIStreamRecord sr in resp.stream)
            {
                if (sr.type != GDBMIParser.GDBMIStreamRecord.GDBMIStreamRecordType.console)
                    continue;        
        
                Match match = regexDecl.Match(sr.str);
                if (match.Success)
                {                    
                    if(level <= 0)
                    {
                        VariableInfo var = new VariableInfo();
                        var.raw = match.Groups[0].Value;
                        var.name = match.Groups[1].Value;
                        Return.Add(var);
                    }                    
                }

                //Skip enclosed types
                if (sr.str.Trim().EndsWith("{"))
                    level++;
                else if (sr.str.Trim().EndsWith("}"))
                    level--;
            }
        }
    }
}
