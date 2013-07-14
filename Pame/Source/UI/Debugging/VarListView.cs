//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinControls.ListView;
using System.Drawing;
using System.Windows.Forms;

namespace Pame
{
    public interface IVarListViewItem
    {
        string Name { get; }
        string Value { get; set; }
        string Type { get; set; }

        IEnumerable<IVarListViewItem> SubItems { get; }
    }

    public class VarListView : TreeListView
    {
        int updateId = 0;

        public VarListView()
        {
        }

        public void UpdateVars(IVarListViewItem[] srcs)
        {
            updateId++; //Get new updateID            

            foreach (IVarListViewItem src in srcs)
            {
                VarNode dst = null;
                if ((dst = FindVarNode(this, src.Name)) != null)
                    Update(dst, src, updateId);
                else
                {
                    dst = new VarNode(src.Name, src.Value, src.Type);
                    this.Nodes.Add(dst);
                    Update(dst, src, updateId);
                }
            }

            //Remove all nodes that have not been added or modified 
            List<VarNode> toRemove = new List<VarNode>();
            foreach(VarNode node in Nodes)
                Clean(toRemove, node, updateId);
            foreach(VarNode node in toRemove)
                node.Remove();
        }

        void Clean(List<VarNode> toRemove, VarNode node, int updateId)
        {
            if (node.UpdateID != updateId)
                toRemove.Add(node);
            else
                foreach (VarNode childNode in node.Nodes)
                    Clean(toRemove, childNode, updateId);
        }

        void Update(VarNode dst, IVarListViewItem src, int updateId)
        {
            if (dst.Value != src.Value)
            {
                dst.Value = src.Value;
                dst.Highlighted = true;
            }
            else
                dst.Highlighted = false;


            dst.Type = src.Type;
            dst.UpdateID = updateId;

            if (src == null || src.SubItems == null)
                return;
            foreach (IVarListViewItem childSrc in src.SubItems)
            {
                VarNode childMatch = FindChildVarNode(dst, childSrc.Name);

                if (childMatch != null) //Node already exists
                    Update(childMatch, childSrc, updateId);
                else //New node
                {
                    VarNode newChild = new VarNode(childSrc.Name);
                    dst.Nodes.Add(newChild);

                    Update(newChild, childSrc, updateId);
                }
            }
        }

        VarNode FindVarNode(VarListView tlv, string name)
        {
            foreach (VarNode node in tlv.Nodes)
                if (node.Name == name)
                    return node;

            return null;
        }

        VarNode FindChildVarNode(VarNode node, string name)
        {
            foreach (VarNode childNode in node.Nodes)
                if (childNode.Name == name)
                    return childNode;

            return null;
        }

        public class VarNode : TreeListNode
        {
            protected int updateId = 0;
            protected bool bHighlighted = false;

            public VarNode(string name, string value, string type)
                : base(name)
            {               
                SubItems.Add(value);
                SubItems.Add(type);
            }

            public VarNode(string name)
                : this(name, string.Empty, string.Empty)
            {
            }

            public int UpdateID
            {
                get { return this.updateId; }
                set { this.updateId = value; }
            }

            public bool Highlighted
            {
                set 
                {
                    if (value == true)
                        ForeColor = Color.Red;
                    else
                        ForeColor = Color.Black;

                    bHighlighted = value; 
                }
                get { return bHighlighted; }
            }

            public string Name
            {
                get { return Text; }
            }

            public string Value
            {
                get { return SubItems[0].Text; }
                set { SubItems[0].Text = value; }
            }

            public string Type
            {
                get { return SubItems[1].Text; }
                set { SubItems[1].Text = value; }
            }
        }
    }
}
