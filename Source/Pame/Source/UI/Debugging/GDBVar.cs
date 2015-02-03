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
using GdbAdapter.Commands;

namespace Pame
{   
    public class GDBVar : IVarListViewItem
    {        
        string name = "";
        string type = "";
        string value = "";
        SubVals subVals = null;

        public GDBVar(GdbAdapter.Commands.VariableInfo gdbInfo, VariableValue gdbValue)
        {
            name = gdbInfo.name;
            type = gdbInfo.type;

            if (gdbValue.Value != null)
                value = gdbValue.Value;
            else if (gdbValue.Values != null)
                subVals = new SubVals(this, gdbValue.Values);
        }

        public GDBVar(GDBVar parent, VariableValue gdbValue)
        {
            if (gdbValue.Value != null)
            {
                name = parent.Name + "[" + gdbValue.index + "]";
                value = gdbValue.Value;
            }
            else if (gdbValue.Values != null)
            {
                name = parent.Name + "[" + gdbValue.index + "]";
                subVals = new SubVals(this, gdbValue.Values);
            }
        }

        #region IVarListViewVar Members

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IVarListViewVar Members

        public IEnumerable<IVarListViewItem> SubItems
        {
            get 
            {
                return subVals; 
            }
        }

        #endregion
    }

    public class SubVals : IEnumerable<IVarListViewItem>
    {
        List<VariableValue> subVals = null;
        GDBVar parent = null;

        public SubVals(GDBVar parent, List<VariableValue> subVals)
        {
            this.subVals = subVals;
            this.parent = parent;
        }

        #region IEnumerable<VarListViewGDBSubVar> Members

        public IEnumerator<IVarListViewItem> GetEnumerator()
        {
            return new SubValsEnum(parent, subVals);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class SubValsEnum : IEnumerator<IVarListViewItem>
    {
        List<VariableValue> vals = null;
        GDBVar parent = null;
        int position = -1;

        public SubValsEnum(GDBVar parent, List<VariableValue> vals)
        {
            this.parent = parent;
            this.vals = vals;
        }

        public IVarListViewItem Current
        {
            get
            {
                try
                {
                    VariableValue gdbVal = vals[position];
                    return new GDBVar(parent, gdbVal);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Dispose()
        {         
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            position++;
            return (position < (vals == null ? 0 : vals.Count));
        }

        public void Reset()
        {
            position = -1;
        }
    }

}
