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
    public class GetVariables : Command<List<VariableInfo>>
    {
        protected static Regex regexDecl = null;
        protected static Regex regexLocation = null;
        
        static GetVariables()
        {
            //eg. "static A : SMALLINT;"
            regexDecl = new Regex(@"^(\w+)?(?:\s+)?(\w+)\s+:\s+([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //eg. "File test.pas:"
            regexLocation = new Regex(@"File\s+(.+):", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public GetVariables(GdbEngine gdb) : base(gdb)
        {
        }

        public override string CommandString
        {
            get { return "info variables"; }
        }

        public override void SetReturnValue(GDBMIParser.GDBMIResponse resp)
        {
            Return = new List<VariableInfo>();

            int level = 0;
            string strFile = null;   
            foreach (GDBMIParser.GDBMIStreamRecord sr in resp.stream)
            {
                if (sr.type != GDBMIParser.GDBMIStreamRecord.GDBMIStreamRecordType.console)
                    continue;

                Match match = regexLocation.Match(sr.str);
                if (match.Success)
                {
                    strFile = match.Groups[1].Value;
                    continue;
                }

                if (sr.str.Trim().Length == 0)
                {
                    strFile = null;
                    continue;
                }

                if (strFile != null)
                {
                    match = regexDecl.Match(sr.str);
                    if (match.Success)
                    {
                        if (level <= 0)
                        {
                            VariableInfo var = new VariableInfo();
                            var.file = strFile;
                            var.raw = match.Groups[0].Value;
                            var.name = match.Groups[2].Value;

                            if(match.Groups[3].Success)
                                var.type = match.Groups[1].Value + " " + match.Groups[3].Value;
                            else
                                var.type = match.Groups[3].Value;

                            Return.Add(var);
                        }
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
