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
    public class Where : Command<LocationInfo>
    {        
        protected static Regex regexSrc, regexNoSrc = null;    

        static Where()
        {
            //eg. "#0  apotelesmata (d1=1, d2=1, d3=1, d4=1, m=1)\n" 
            //    "    at D:/Temp/samples/dwdeka_1.pas:29\n" 
            //    "#1  0x0040171f in $main () at D:/Temp/samples/dwdeka_1.pas:38\n"            
            regexSrc = new Regex(@"^#([0-9]+)\s+(.+)\s+at\s+(.+):([0-9]+)", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //eg. "#0  0x77c20996 in ntdll!RtlDeleteAce () from C:\Windows\system32\tdll.dll"
            regexNoSrc = new Regex(@"^#([0-9]+)\s+(.+)\s+in\s+(.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public Where(GdbEngine gdb) : base(gdb)
        {
        }

        public override string CommandString
        {
            get { return "where"; }
        }

        public override void SetReturnValue(GDBMIParser.GDBMIResponse resp)
        {
            foreach (GDBMIParser.GDBMIStreamRecord sr in resp.stream)
            {
                if (sr.type != GDBMIParser.GDBMIStreamRecord.GDBMIStreamRecordType.console)
                    continue;

                Match match = regexSrc.Match(sr.str);
                if (match.Success)
                {
                    Return = new LocationInfo();

                    Return.raw = match.Groups[0].Value;

                    int val = 0;
                    if (Int32.TryParse(match.Groups[1].Value, out val))
                        Return.frame = val;

                    Return.address = match.Groups[2].Value;
                    Return.file = match.Groups[3].Value;

                    val = 0;
                    if (Int32.TryParse(match.Groups[4].Value, out val))
                        Return.line = val;

                    break;
                }

                match = regexNoSrc.Match(sr.str);
                if (match.Success)
                {
                    Return = new LocationInfo();

                    Return.raw = match.Groups[0].Value;

                    int val = 0;
                    if (Int32.TryParse(match.Groups[1].Value, out val))
                        Return.frame = val;

                    Return.address = match.Groups[2].Value;
                    Return.file = match.Groups[3].Value;                    

                    break;
                }
            } 
        }        
    }
}
