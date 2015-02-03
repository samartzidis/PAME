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
    public class CommandManager
    {
        private CommandMap map = null;
        private Dictionary<Type, ICommandExecutor> executors = null;

        public CommandManager()
        {
            map = new CommandMap();
            executors = new Dictionary<Type, ICommandExecutor>();

            Application.Idle += new EventHandler(this.OnIdle);

            RegisterCommandExecutor(typeof(System.Windows.Forms.ToolStripButton), new ToolStripCommandExecutor(this));
            RegisterCommandExecutor(typeof(System.Windows.Forms.ToolStripMenuItem), new ToolStripMenuItemCommandExecutor(this));
        }

        public void Register(ICommand cmd, object obj)
        {
            cmd.Manager = this;
            GetCommandExecutor(obj.GetType()).InitializeTargetObject(obj);
            map.AssociateTargetObject(cmd, obj);
        }

        public void Register(ICommand cmd, object[] objects)
        {
            cmd.Manager = this;
            map.AssociateTargetObjects(cmd, objects);

            foreach (object obj in objects)
                GetCommandExecutor(obj.GetType()).InitializeTargetObject(obj);
        }

        internal CommandMap Map
        {
            get { return map; }
        }

        internal void RegisterCommandExecutor(Type type, ICommandExecutor executor)
        {
            executors.Add(type, executor);
        }

        internal ICommandExecutor GetCommandExecutor(Type targetObjectType)
        {
            return executors[targetObjectType];
        }

        private void OnIdle(object sender, EventArgs args)
        {
            foreach (ICommand cmd in map.GetCommands())
            {
                if (cmd != null)
                    cmd.ProcessUpdates();
            }
        }
    }

    internal class CommandMap
    {
        private Dictionary<object, ICommand> objects = null;
        private Dictionary<ICommand, List<object>> commands = null;

        public CommandMap()
        {
            objects = new Dictionary<object, ICommand>();
            commands = new Dictionary<ICommand, List<object>>();
        }

        public Dictionary<ICommand, List<object>>.KeyCollection GetCommands()
        {
            return commands.Keys;
        }

        public ICommand GetCommand(object obj)
        {
            if (objects.ContainsKey(obj))
                return objects[obj];
            return null;
        }

        public List<object> GetTargetObjects(ICommand cmd)
        {
            if (commands.ContainsKey(cmd))
                return commands[cmd];
            return null;
        }

        public void AssociateTargetObjects(ICommand cmd, object[] array)
        {
            if (commands.ContainsKey(cmd))
                commands[cmd].AddRange(array);
            else
                commands[cmd] = new List<object>(array);

            foreach (object obj in array)
                objects[obj] = cmd;
        }

        public void AssociateTargetObject(ICommand cmd, object obj)
        {
            if (commands.ContainsKey(cmd))
                commands[cmd].Add(obj);
            else
                commands[cmd] = new List<object>(new object[] { obj });

            objects[obj] = cmd;
        }
    }      
}
