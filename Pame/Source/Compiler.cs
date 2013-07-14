//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Logging;
using System.Text.RegularExpressions;
using System.IO;

namespace Pame
{
    public class Compiler
    {        
        public class Error
        {
            public string msg = null;
            public int line = 0;
            public int col = 0;
            public string file = null;
            public string severity = null;
        }

        public class Message
        {
            public Error error = null;
            public string raw = null;
        }

        public delegate void MessageReceivedHandler(Compiler instance, Message msg);
        public event MessageReceivedHandler OnMessageReceived = null;

        public delegate void CompilerFinishedHandler(Compiler instance, int exitCode);
        public event CompilerFinishedHandler OnCompilerFinished = null;

        string fpcPath = "fpc.exe";
        string flags = null;
        Process proc = null;
        byte[] stdoutReadBuffer = new byte[1024];
        string stdoutBuffer = "";
        static Regex regex = null;
        string filePath = null;
        object tag = null;

        static Compiler()
        {
            //eg. test.pas(14,14) Fatal: Syntax error, "DO" expected but "identifier DOS" found
            //eg. test.pas(4,2) Note: Local variable "b" is assigned but never used
            //eg. test.pas(13,7) Error: Incompatible types: got "Constant String" expected "Extended"
            //eg. test.pas(22) Fatal: There were 1 errors compiling module, stopping
            //eg. Fatal: Syntax error, "BEGIN" expected but "identifier DFD" found    
            //
            //Note: Use non-capturing groups (?: ... ) to preserve order of the capturing ones
            //
            regex = new Regex(@"^(?:(.+)\(([0-9]+)(?:,([0-9]+))?\))?\s*(info|error|warning|fatal|note):(.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public bool Compile()
        {
            if (Running())
            {
                Logger.Instance.Error("Compiler is already running");
                return false;
            }

            try
            {
                proc = new Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.FileName = (fpcPath == null) ? "fpc.exe" : fpcPath;
                proc.StartInfo.Arguments = string.Format("{0} \"{1}\"", (flags == null) ? "" : flags, filePath);
                proc.StartInfo.CreateNoWindow = true;

                proc.Exited += new EventHandler(proc_Exited);

                if (!proc.Start())
                    return false;
            }
            catch (Exception m)
            {
                Logger.Instance.Error("proc.Start() failed: " + m.Message);
                throw m;
            }

            //Begin async read operation for stdout
            proc.StandardOutput.BaseStream.BeginRead(stdoutReadBuffer, 0, stdoutReadBuffer.Length,
                new AsyncCallback(ReadStandardOutputCallback), null);

            return true;            
        }
        
        public bool Running()
        {
            try
            {
                return proc != null && !proc.HasExited;
            }
            catch (InvalidOperationException)
            {
                //May be thrown by proc.HasExited if the proccess internal members have been disposed
            }
            catch (Exception m)
            {
                Logger.Instance.Debug(m);
            }

            return false;
        }
        
        public object Tag
        {
            get { return tag; }
            set 
            {
                if (Running())
                    throw new Exception("Cannot set property while running");

                tag = value; 
            }
        }

        public string FilePath
        {
            get { return filePath; }
            set 
            {
                if (Running())
                    throw new Exception("Cannot set property while running");

                filePath = value; 
            }
        }

        public string Flags
        {
            get { return flags; }
            set 
            {
                if (Running())
                    throw new Exception("Cannot set property while running");

                flags = value; 
            }
        }

        public string FpcPath
        {
            get { return fpcPath; }
            set 
            {
                if (Running())
                    throw new Exception("Cannot set property while running");

                fpcPath = value; 
            }
        }

        private void ReadStandardOutputCallback(IAsyncResult asyncResult)
        {
            int count = proc.StandardOutput.BaseStream.EndRead(asyncResult);            
            string readBuf = new string(Encoding.Default/*.UTF8*/.GetChars(stdoutReadBuffer, 0, count));
            Logger.Instance.Debug("(" + fpcPath + ") -> " + readBuf);
            stdoutBuffer += readBuf.Replace("\r\n", "\n");

            char[] seps = new char[] { '\r', '\n' };
            int index = stdoutBuffer.IndexOfAny(seps);
            while (index >= 0)
            {
                string line = stdoutBuffer.Substring(0, index);
                stdoutBuffer = stdoutBuffer.Substring(index + 1);

                ProcessLine(line);

                index = stdoutBuffer.IndexOfAny(seps);
            }

            //Begin next async read operation for stdout
            if (Running() || count > 0)
                proc.StandardOutput.BaseStream.BeginRead(stdoutReadBuffer, 0, stdoutReadBuffer.Length, 
                    new AsyncCallback(ReadStandardOutputCallback), null);
        }

        private void ProcessLine(string line)
        {
            Message msg = new Message();
            msg.raw = line;

            Match match = regex.Match(line);
            if (match.Success)
            {
                msg.error = new Error();
                msg.error.file = match.Groups[1].Success ? match.Groups[1].Value : null;
                Int32.TryParse(match.Groups[2].Value, out msg.error.line);
                Int32.TryParse(match.Groups[3].Value, out msg.error.col);
                msg.error.severity = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;
                msg.error.msg = match.Groups[5].Success ? match.Groups[5].Value.Trim() : null;
            }

            if (OnMessageReceived != null)
                OnMessageReceived(this, msg);            
        }        

        void proc_Exited(object sender, EventArgs e)
        {
            if (OnCompilerFinished != null)
                OnCompilerFinished(this, proc.ExitCode);
        }
    }
}
