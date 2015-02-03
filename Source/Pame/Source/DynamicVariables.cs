//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Pame
{
    public class DynamicVariables
    {
#region SINGLETON
        private static DynamicVariables instance = null;        

        public static DynamicVariables Instance
        {
            get 
            {
                if (instance == null)
                    instance = new DynamicVariables();
                return instance; 
            }
        }
#endregion SINGLETON

        public enum Variable { AppDir, AppPath };
        private readonly Regex regexDynamicVarDecl = null;

        protected DynamicVariables()
        {
            regexDynamicVarDecl = new Regex(@"(?:\$\((\w+)\)){1,}", RegexOptions.Compiled);
        }        
        
        public IEnumerable<Variable> GetVariables()
        {
            return EnumToList<Variable>();
        }

        public bool Contains(string variableName)
        {
            IEnumerable<Variable> vars = EnumToList<Variable>();
            foreach (Variable var in vars)
            {
                if (var.ToString().Equals(variableName))
                    return true;
            }

            return false;
        }

        public string this[Variable variable]
        {
            get
            {
                return GetValue(variable);
            }
        }

        public string this[string variableName]
        {
            get
            {
                IEnumerable<Variable> vars = EnumToList<Variable>();
                foreach (Variable var in vars)
                {
                    if (var.ToString().Equals(variableName))
                        return GetValue(var);
                }

                throw new ArgumentException("Invalid variable name: " + variableName);
            }
        }

        public string GetValue(Variable name)
        {
            switch (name)
            {
                case Variable.AppDir:
                    return Globals.GetExeDirPath();
                case Variable.AppPath:
                    return Globals.GetExePath();
                default:
                    break;
            }

            return null;
        }

        public IEnumerable<T> EnumToList<T>()
        {
            Type enumType = typeof(T);

            // Can't use generic type constraints on value types,
            // so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);
            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }

        public string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public string StringReplaceDynamicVariables(string value)
        {
            if (value == null)
                return null;            

            return regexDynamicVarDecl.Replace(value, m =>
            {
                string varName = m.Groups[1].Captures[0].Value;
                if (Contains(varName))
                    return this[varName];
                else
                    return varName;
            });
        }

        public void StringCheckDynamicVariables(string value)
        {
            if (value == null)
                return;
   
            MatchCollection mc = regexDynamicVarDecl.Matches(value);
            foreach (Match m in mc)
            {
                string str = string.Empty;
                List<DynamicVariables.Variable> vars = new List<Variable>(DynamicVariables.Instance.GetVariables());
                for (int k = 0; k < vars.Count; k++)
                {
                    str += vars[k].ToString() + " = \"" + this[vars[k]] + "\"";
                    if (k < vars.Count - 1)
                        str += ", ";
                }

                if (!DynamicVariables.Instance.Contains(m.Groups[1].Captures[0].Value))
                    throw new ArgumentException("Invalid dynamic variable: " + m.Groups[1].Value + ". Allowable values are: " + str);
            }
        }
    }
}
