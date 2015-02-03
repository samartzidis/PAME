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
using Logging;

namespace GdbAdapter
{
    public class ExecutionException : Exception
    {
        public ExecutionException(string message)
            : base(message)
        {
        }

        public ExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public abstract class Command
    {
        protected GdbEngine gdb = null;        

        protected Command(GdbEngine gdb)
        {
            if (gdb == null)
                throw new ArgumentNullException("gdb");
            this.gdb = gdb;
        }        

        public abstract string CommandString { get; }
    }

    public abstract class OneWayCommand : Command
    {
        public OneWayCommand(GdbEngine gdb)
            : base(gdb)
        {
        }

        public virtual void ExecuteAsync()
        {
            gdb.SendCmd(CommandString, null);
        }
    }

    public abstract class TwoWayCommand : Command
    {
        protected GDBMIParser.GDBMIResponse res = null;

        public TwoWayCommand(GdbEngine gdb)
            : base(gdb)
        {            
        }

        public virtual void ExecuteAsync()
        {
            gdb.SendCmd(CommandString, null);
        }

        public virtual void Execute()
        {
            Execute(-1);
        }

        public virtual void Execute(int timeout)
        {
            GdbEngine.SyncAdapter syncGdb = new GdbEngine.SyncAdapter(gdb);
            try
            {
                res = syncGdb.SendCmd(CommandString, timeout);
            }
            catch (TimeoutException ex)
            {
                Logger.Instance.Warn(ex.Message);
                throw ex;
            }
        }

        protected void CheckResponseAndThrowExecutionException(GDBMIParser.GDBMIResponse resp)
        {
            if (resp == null)
                throw new ExecutionException("null GDBMIResponse received");
            else if (resp.result == null)
                throw new ExecutionException("null GDBMIResultRecord in GDBMIResponse received");
            else if (resp.result.cls == GDBMIParser.GDBMIResultRecord.ResultClass.error)
            {
                if (resp.result.ContainsKey("msg"))
                    throw new ExecutionException((string)resp.result["msg"]);

                throw new ExecutionException("Command did not execute successfully (no extra msg)");
            }
            else if (resp.result.cls != GDBMIParser.GDBMIResultRecord.ResultClass.done)
            {
                string log = "";
                foreach (GDBMIParser.GDBMIStreamRecord r in resp.stream)
                    log += r.str + "| ";
                throw new ExecutionException("Command did not execute successfully. IO log: " + log);
            }
        }
    }

    public abstract class VoidCommand : TwoWayCommand
    {
        public delegate void AsyncResultHandler(object sender, Exception ex);
        public event AsyncResultHandler OnAsyncResult;

        public VoidCommand(GdbEngine gdb)
            : base(gdb)
        {
        }

        public override void ExecuteAsync()
        {
            gdb.SendCmd(CommandString, new GdbEngine.ResponseReceivedHandler(Gdb_OnResponseReceived));
        }

        public override void Execute(int timeout)
        {
            base.Execute(timeout);
            CheckResponseAndThrowExecutionException(res); //***
        }

        public override void Execute()
        {
            Execute(-1);
        }

        protected void Gdb_OnResponseReceived(object sender, GDBMIParser.GDBMIResponse response)
        {
            if (OnAsyncResult != null)
            {
                try
                {
                    CheckResponseAndThrowExecutionException(response); //***
                    OnAsyncResult(this, null);
                }
                catch (Exception m)
                {
                    OnAsyncResult(this, m);
                }
            }
        }                        
    }

    public abstract class Command<RETURN_TYPE> : TwoWayCommand
       where RETURN_TYPE : class
    {
        public delegate void AsyncResultHandler(object sender, Exception ex, RETURN_TYPE res);
        public event AsyncResultHandler OnAsyncResult;
        private RETURN_TYPE returnValue = null;

        public Command(GdbEngine gdb)
            : base(gdb)
        {
        }

        public RETURN_TYPE Return
        {
            get { return returnValue; }
            protected set { returnValue = value; }
        }

        public override void ExecuteAsync()
        {
            gdb.SendCmd(CommandString, new GdbEngine.ResponseReceivedHandler(Gdb_OnResponseReceived));
        }

        public new RETURN_TYPE Execute()
        {
            return Execute(-1);
        }

        public new RETURN_TYPE Execute(int timeout)
        {
            base.Execute(timeout);

            CheckResponseAndThrowExecutionException(res); //***
            SetReturnValue(res);            

            return returnValue;
        }

        protected void Gdb_OnResponseReceived(object sender, GDBMIParser.GDBMIResponse response)
        {
            if (OnAsyncResult != null)
            {
                try
                {
                    CheckResponseAndThrowExecutionException(response); //***
                    SetReturnValue(response);
                    OnAsyncResult(this, null, returnValue);
                }
                catch (Exception m)
                {
                    OnAsyncResult(this, m, null);
                }
            }
        }

        public abstract void SetReturnValue(GDBMIParser.GDBMIResponse resp);
    }    
}
