//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Timers;
using System.Windows.Forms;

namespace CmdMgr
{    
    public interface ICommand
    {
        void Execute();
        void ProcessUpdates();
        CommandManager Manager { get; set; }
    }

    public delegate void ExecuteHandler<COMMANDS>(Command<COMMANDS> cmd);
    public delegate void UpdateHandler<COMMANDS>(Command<COMMANDS> cmd);

    public class Command<COMMANDS> : ICommand
    {
        public event UpdateHandler<COMMANDS> OnUpdate;
        public event ExecuteHandler<COMMANDS> OnExecute;        

        private CommandManager manager = null;
        private COMMANDS type;
        
        protected bool enabled = false;
        protected bool check = false;

        public Command(COMMANDS type, ExecuteHandler<COMMANDS> handlerExecute, UpdateHandler<COMMANDS> handlerUpdate)
        {
            this.type = type;
            
            if(handlerExecute != null)
                OnExecute += handlerExecute;                
            if(handlerUpdate != null)
                OnUpdate += handlerUpdate;
        }

        public Command(COMMANDS type, ExecuteHandler<COMMANDS> handlerExecute)
            : this(type, handlerExecute, null)
        {            
        }

        public Command(COMMANDS type)
            : this(type, null, null)
        {
        }

        public COMMANDS CommandType
        {
            get { return type; }
        }

        public CommandManager Manager
        {
            get { return manager; }
            set { manager = value; }
        }

        public override string ToString()
        {
            return type.ToString();
        }

        public void Execute()
        {
            if (OnExecute != null)
                OnExecute(this);
        }

        public void ProcessUpdates()
        {
            if (OnUpdate != null)
                OnUpdate(this);
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;

                foreach (object instance in manager.Map.GetTargetObjects(this))
                    manager.GetCommandExecutor(instance.GetType()).Enable(instance, enabled);
            }
        }

        public bool Checked
        {
            get { return check; }
            set
            {
                check = value;

                foreach (object instance in manager.Map.GetTargetObjects(this))
                    manager.GetCommandExecutor(instance.GetType()).Check(instance, check);
            }
        }
    }
}
