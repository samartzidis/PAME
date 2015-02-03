//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;

namespace CmdMgr
{
    public interface ICommandExecutor
    {
        void InitializeTargetObject(object item);
        void Enable(object item, bool bEnable);
        void Check(object item, bool bCheck);
    }

    public class ToolStripMenuItemCommandExecutor : ICommandExecutor
    {
        CommandManager mgr = null;
        public ToolStripMenuItemCommandExecutor(CommandManager mgr)
        {
            this.mgr = mgr;                 
        }

        public void InitializeTargetObject(object item)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)item;
            mi.Click += new System.EventHandler(menuItem_Click);
        }

        public void Enable(object item, bool bEnable)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)item;
            if(mi.Enabled != bEnable)
                mi.Enabled = bEnable;
        }

        public void Check(object item, bool bCheck)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)item;
            if(mi.Checked != bCheck)
                mi.Checked = bCheck;
        }

        private void menuItem_Click(object sender, System.EventArgs e)
        {
            ICommand cmd = mgr.Map.GetCommand(sender);
            if (cmd != null)
                cmd.Execute();
        }
    }

    public class ToolStripCommandExecutor : ICommandExecutor
    {
        CommandManager mgr = null;
        public ToolStripCommandExecutor(CommandManager mgr)
        {
            this.mgr = mgr;                 
        }

        public void InitializeTargetObject(object item)
        {
            ToolStripButton button = (ToolStripButton)item;
            button.Click += new EventHandler(button_Click);
        }

        void button_Click(object sender, EventArgs e)
        {
            ICommand cmd = mgr.Map.GetCommand(sender);
            if (cmd != null)
                cmd.Execute();
        }

        public void Enable(object item, bool bEnable)
        {
            ToolStripButton button = (ToolStripButton)item;            
            if(button.Enabled != bEnable)
                button.Enabled = bEnable;
        }

        public void Check(object item, bool bCheck)
        {
            ToolStripButton button = (ToolStripButton)item;
            if(button.Checked != bCheck)
                button.Checked = bCheck;
        }
    }
}