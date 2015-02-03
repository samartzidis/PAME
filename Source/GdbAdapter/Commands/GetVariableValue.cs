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
using Logging;

namespace GdbAdapter.Commands
{
    public class VariableValue
    {
        public int index = 0; //For array value elements
        protected object value = null;
        public VariableValue parent = null;

        public VariableValue()
        {
            value = null;
        }

        public VariableValue(string val)
        {
            value = val;
        }

        public VariableValue(List<VariableValue> val)
        {
            value = val;
        }

        public string Value
        {
            get 
            { 
                if(value is string)
                    return (string)value;
                return null;
            }
            set { this.value = value; }
        }

        public List<VariableValue> Values
        {
            get 
            {
                if (value is List<VariableValue>)
                    return (List<VariableValue>)value;
                return null;
            }
            set { this.value = value; }
        }
    }    

    public class GetVariableValue : Command<VariableValue>
    {
        protected VariableInfo vi = null;
        protected static Regex regexArrayElementValueDecl = null;
        protected static Regex regexValueDecl = null;

        static GetVariableValue()
        {
            regexValueDecl = new Regex(
                @"^\s*\$\d+\s+=\s+(?<value>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            regexArrayElementValueDecl = new Regex(
                @"^\s*(?:\[(?<index>\d+)\]\s+=\s+)?(?:\s*(?<down>\{)\s*)*(?<value>(?:(?:(?!')[^\s{},])|(?:\'.*\'))+)?(?:\s*(?<up>\})\s*)*", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public GetVariableValue(GdbEngine gdb, VariableInfo vi) : base(gdb)
        {
            if (vi == null)
                throw new ArgumentNullException("vi");

            this.vi = vi;
        }

        public override string CommandString
        {
            get { return "print " + vi.name; }
        }

        public override void SetReturnValue(GDBMIParser.GDBMIResponse resp)
        {
            int k = 0;
            for (; k < resp.stream.Count; ++k)
            {
                GDBMIParser.GDBMIStreamRecord sr = resp.stream[k];

                //If simple value
                Match match = regexValueDecl.Match(sr.str);
                if (match.Success)
                {
                    if (match.Groups["value"].Value.StartsWith("{"))
                    {
                        Return = new VariableValue(new List<VariableValue>());
                        Return.parent = Return;
                        k++;                        
                        break;
                    }
                    else
                    {
                        Return = new VariableValue(match.Groups["value"].Value);
                        return;
                    }
                }
            }

            VariableValue cur = Return;          
            for (; k < resp.stream.Count; ++k)
            {
                GDBMIParser.GDBMIStreamRecord sr = resp.stream[k];

                Match match = regexArrayElementValueDecl.Match(sr.str);
                if (match.Success)
                {
                    int index = 0;
                    if (match.Groups["index"].Success)
                        int.TryParse(match.Groups["index"].Value, out index);

                    int count = match.Groups["down"].Captures.Count;
                    while (count-- > 0)
                    {
                        VariableValue v = new VariableValue(new List<VariableValue>());                        
                        v.parent = cur;
                        v.index = index;
                        cur.Values.Add(v);

                        cur = v;
                    }

                    if (match.Groups["value"].Success)
                    {
                        VariableValue v = new VariableValue(match.Groups["value"].Captures[0].Value);
                        v.parent = cur.parent;
                        v.index = index;
                        cur.Values.Add(v);
                    }
                    
                    count = match.Groups["up"].Captures.Count;
                    while (count-- > 0)
                        cur = cur.parent;
                }
                else
                {
                    Logger.Instance.Error("Not matching: " + sr.str);
                    Return = null;
                    break;
                }
            }          
        }              
    }
}
