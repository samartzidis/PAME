//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Antlr.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using GdbAdapter.Parsers;
using Logging;
using PameNativeAdapter;

namespace GdbAdapter
{
    public class GdbEngine
    {
        public delegate void StatusChangedEventHandler(GdbEngine engine, StatusEnum status);
        public event StatusChangedEventHandler OnStatusChanged;

        public delegate void ResponseReceivedHandler(object sender, GDBMIParser.GDBMIResponse response);
        public enum StatusEnum { Ready, ReplyPending, Terminating, Terminated };

        private Process gdbProc = null;
        private StreamWriter writer = null;
        private byte[] stdoutBuf = new byte[1024];
        private string stdoutBlock = "";
        private string gdbPath = "gdb.exe";
        private ResponseReceivedHandler responseReceivedCallback = null;        
        private AutoResetEvent gdbReadyEvent = null;        
        private StatusEnum status = StatusEnum.Terminated;
        private object statusLock = new object();

        public GdbEngine()
        {
            gdbReadyEvent = new AutoResetEvent(false);
        }

        public string GdbPath
        {
            get { return gdbPath; }
            set { gdbPath = value; }
        }

        public StatusEnum Status
        {
            get { lock (statusLock) return status; }
            private set 
            {
                lock (statusLock)
                {
                    status = value;

                    if (OnStatusChanged != null)
                        OnStatusChanged(this, value);
                }
            }
        }
        
        /// <summary>
        /// Executes the internal gdb debugger process
        /// </summary>
        public void Run()
        {
            if (Status != StatusEnum.Terminated)
                throw new GdbException("Cannot start while in '" + Status + "' status");

            gdbProc = new Process();
            gdbProc.StartInfo.UseShellExecute = false;
            gdbProc.StartInfo.RedirectStandardInput = true;
            gdbProc.StartInfo.RedirectStandardOutput = true;
            gdbProc.StartInfo.CreateNoWindow = true;
            gdbProc.StartInfo.FileName = GdbPath;
            gdbProc.StartInfo.Arguments = "--quiet --interpreter=mi";
            gdbProc.EnableRaisingEvents = true; //required for the Exited handler
            gdbProc.Exited += new EventHandler(gdbProc_Exited);

            if (!gdbProc.Start())
                throw new GdbException("Could not spawn internal gdb debugger");            

            writer = gdbProc.StandardInput;
            writer.AutoFlush = true;

            //Consume initial GDB prompt
            gdbProc.StandardOutput.ReadLine();            

            //Begin async read operation for stdout
            gdbProc.StandardOutput.BaseStream.BeginRead(stdoutBuf, 0, stdoutBuf.Length,
                new AsyncCallback(ReadStandardOutputCallback), null);

            //Set status to Ready and set gdbReadyEvent waking up possibly blocked thread in SendCmd()
            Status = StatusEnum.Ready;
            gdbReadyEvent.Set();

            //Set mandatory gdb parameters
            InitialiseGDBParameters();
        }

        /// <summary>
        /// Sets mandatory parameters for correct communication with gdb
        /// </summary>
        protected void InitialiseGDBParameters()
        {
            try
            {
                GdbEngine.SyncAdapter sa = new GdbEngine.SyncAdapter(this);

                sa.SendCmd("set print repeats 1000", 5000);

                //Ignore the sigtraps created by DbgUiConnectToDbg() from old versions of ntdll.dll
                sa.SendCmd("handle SIGTRAP noprint nostop", 5000);

                sa.SendCmd("set print array-indexes on", 5000);
                sa.SendCmd("set new-console on", 5000);
                sa.SendCmd("set new-group on", 5000);
                sa.SendCmd("set width 999999999", 5000); //Keep GDB from wrapping long lines
            }
            catch (Exception m)
            {
                Logger.Instance.Error(m);
                throw new GdbException("Failed to initialise gdb parameters: " + m.Message);                
            }
        }

        /// <summary>
        /// Forcefuly kills the internal gdb process
        /// </summary>
        public void Kill()
        {
            if (Status == StatusEnum.Terminated || Status == StatusEnum.Terminating)
                return;

            //Set status to Terminating and wake up possibly blocked thread in SendCmd()
            Status = StatusEnum.Terminating;
            gdbReadyEvent.Set();

            try
            {
                gdbProc.Kill();
            }
            catch (Exception m)
            {
                Logger.Instance.Error(m.Message);
            }
        }

        /// <summary>
        /// Callback method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gdbProc_Exited(object sender, EventArgs e)
        {
            Logger.Instance.Debug("gdbProc_Exited()");

            if (Status == StatusEnum.ReplyPending)
            {
                //Asynchronously notify external async-mode handler that there will be no reply
                if (responseReceivedCallback != null)
                    responseReceivedCallback(this, null);
            }

            //Set status to Terminated
            Status = StatusEnum.Terminated;

            //Wake up other client possibly blocked in SendCmd()
            gdbReadyEvent.Set();
        }

        /// <summary>
        /// Sends a text command to the gdb process. This method may block if the 
        /// stdin of the gdb process is currently blocked.
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void SendCmd(string cmd, ResponseReceivedHandler handler)
        {
            if (Status == StatusEnum.Terminated || Status == StatusEnum.Terminating)
                throw new GdbException("Cannot send command, internal gdb is not active");

            //Wait until we are allowed to enter
            gdbReadyEvent.WaitOne();

            //Set the response callback for this caller
            responseReceivedCallback = handler;

            //If terminating or terminated, immediately notify client with a null reply
            if (Status == StatusEnum.Terminating || Status == StatusEnum.Terminated)
            {
                if (responseReceivedCallback != null)
                    responseReceivedCallback(this, null);
                return;
            }

            //OK, set status to ReplyPending and submit command to gdb
            Status = StatusEnum.ReplyPending;           
            Logger.Instance.Debug("[gdb] <- " + cmd);
            writer.WriteLine(cmd);            
        }

        /// <summary>
        /// Unregisters a ResponseReceivedHandler, being previously registered in SendCmd()        
        /// </summary>
        public void UnregisterResponseReceivedHandler()
        {
            responseReceivedCallback = null;
        }

        private void ReadStandardOutputCallback(IAsyncResult asyncResult)
        {
            int count = gdbProc.StandardOutput.BaseStream.EndRead(asyncResult);
            string buffer = new string(Encoding.ASCII.GetChars(stdoutBuf, 0, count));
            //Logger.Instance.Debug("[gdb] -> " + buffer);
            stdoutBlock += buffer;

            string respTerminator = "(gdb) \r\n";
            int breakPos = stdoutBlock.IndexOf(respTerminator);
            while (breakPos >= 0)
            {
                string rawResponse = stdoutBlock.Substring(0, breakPos);
                stdoutBlock = stdoutBlock.Substring(breakPos + respTerminator.Length);

                GDBMIParser.GDBMIResponse res = null;
                try
                {
                    res = ParseRawGdbResponse(rawResponse);
                }
                catch (RecognitionException m)
                {
                    //Parse error.
                    //Construct an appropriate error response to send back.
                    res = new GDBMIParser.GDBMIResponse();
                    res.result = new GDBMIParser.GDBMIResultRecord();
                    res.result.cls = GDBMIParser.GDBMIResultRecord.ResultClass.error;
                    res.result["msg"] = m.ToString();
                }
                catch (Exception m)
                {
                    //Parse error.
                    //Construct an appropriate error response to send back.
                    res = new GDBMIParser.GDBMIResponse();
                    res.result = new GDBMIParser.GDBMIResultRecord();
                    res.result.cls = GDBMIParser.GDBMIResultRecord.ResultClass.error;
                    res.result["msg"] = m.ToString();
                }               

                //Send the response to the async-mode handler
                if (responseReceivedCallback != null)
                    responseReceivedCallback(this, res);

                if (Status == StatusEnum.ReplyPending)
                    Status = StatusEnum.Ready;
                gdbReadyEvent.Set(); //Allow thread waiting in SendCmd() to continue   

                breakPos = stdoutBlock.IndexOf(respTerminator);
            }

            //Begin next async read operation for stdout
            if(Status == StatusEnum.Ready || Status == StatusEnum.ReplyPending || count > 0)
            {
                gdbProc.StandardOutput.BaseStream.BeginRead(stdoutBuf, 0, stdoutBuf.Length,
                    new AsyncCallback(ReadStandardOutputCallback), null);
            }
        }        

        /// <summary>
        /// Uses the ANTLR parser to parse the GDB string into a GDBMIResponse.
        /// </summary>
        /// <param name="raw">the GDB command reply string</param>
        /// <returns>GDBMIResponse</returns>
        private GDBMIParser.GDBMIResponse ParseRawGdbResponse(string raw)
        {
            Logger.Instance.Debug("Parsing:\r\n" + raw);

            ANTLRStringStream ss = new ANTLRStringStream(raw);
            GDBMILexer lex = new GDBMILexer(ss);
            CommonTokenStream ts = new CommonTokenStream(lex);
            GDBMIParserStrict parser = new GDBMIParserStrict(ts);
            GDBMIParser.GDBMIResponse res = null;

            try
            {
                res = parser.output();                
            }
            catch (RecognitionException m)
            {
                Logger.Instance.Error("While parsing:\r\n" + m.Input.ToString() + "\r\nGot error:\r\n" + m.ToString());
                throw m;
            }
            catch (Exception m)
            {
                Logger.Instance.Error(m.StackTrace);
                throw m;
            }

            return res;
        }

        /// <summary>
        /// Strict version of GDBMIParser that does not try to recover and 
        /// throws a RecognitionException on first error found
        /// </summary>
        class GDBMIParserStrict : GDBMIParser
        {
            public GDBMIParserStrict(ITokenStream source)
                : base(source)
            {
            }

            public override void ReportError(RecognitionException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Synchronous GdbEngine adapter atop the asynchronous GdbEngine
        /// </summary>
        public class SyncAdapter
        {
            GdbEngine gdb = null;
            private AutoResetEvent responseEvent = null;
            GDBMIParser.GDBMIResponse response = null;

            public SyncAdapter(GdbEngine gdb)
            {
                this.gdb = gdb;
                responseEvent = new AutoResetEvent(false);
            }

            void gdb_OnResponseReceived(object sender, GDBMIParser.GDBMIResponse response)
            {
                if (sender != gdb)
                    return;

                //Update last response
                this.response = response;

                //Signal internal responseEvent
                responseEvent.Set();
            }

            /// <summary>
            /// Receives a response in synchronous mode
            /// </summary>
            /// <param name="timeout">If in blocking mode, a timeout value in milliseconds can be specified.
            /// if this value is reached and no response is received, null will be returned</param>
            /// <returns></returns>
            public GDBMIParser.GDBMIResponse SendCmd(string cmd, int timeout)
            {
                response = null;
                responseEvent.Reset();
                gdb.SendCmd(cmd, new GdbEngine.ResponseReceivedHandler(gdb_OnResponseReceived));

                bool bSet = responseEvent.WaitOne(timeout);
                if (!bSet)
                {
                    //timeout
                    Logger.Instance.Warn("SendCmd("+cmd+") TIMEOUT, returning null response");
                    gdb.UnregisterResponseReceivedHandler();
                    throw new TimeoutException("Timeout while executing: " + cmd);
                }

                return response;
            }

            public GDBMIParser.GDBMIResponse SendCmd(string cmd)
            {
                return SendCmd(cmd, -1);
            }
        }
    }

    public class GdbException : Exception
    {
        public GdbException(string message)
            : base(message)
        {
        }
    }
}
