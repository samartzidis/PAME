//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using Logging;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pame
{
    public class SingleInstanceManager
    {
        public delegate void NextInstanceHandler(string[] args);
        public event NextInstanceHandler OnNextInstance = null;

        private static System.Threading.Mutex mutex = null;
        private string guid = null;
#pragma warning disable 414
        private SingleInstanceManager instance = null;
#pragma warning restore 414

        public SingleInstanceManager(string guid) 
        {
            if (guid == null || guid.Trim().Length == 0)
                throw new Exception("Invalid guid parameter");

            this.guid = guid;
        }

        public bool InitInstance(string[] args)
        {
            bool ok;
            mutex = new System.Threading.Mutex(true, guid, out ok);
            if (!ok)
            {
                //mutex already exixts
                SendToServer(args);
                return false;
            }

            //first instance
            BeginWaitForConnection();

            return true;            
        }

        private void SendToServer(string[] args)
        {            
            //Serialize args
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, args);
            byte[] bytes = ms.GetBuffer();

            try
            {
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", guid, PipeDirection.Out);
                pipeClient.Connect(5000);                                
                pipeClient.Write(bytes, 0, bytes.Length);
                pipeClient.Flush();
                pipeClient.Close();
            }
            catch (TimeoutException)
            {
                Logger.Instance.Error("Pipe connection timeout");
            }
        }        

        private void BeginWaitForConnection()
        {
            NamedPipeServerStream pipeServer = 
                new NamedPipeServerStream(guid, PipeDirection.In, 1, 
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            ServerAsyncInfo async = new ServerAsyncInfo();
            async.Server = pipeServer;

            IAsyncResult result = pipeServer.BeginWaitForConnection(Connection, async);            
        }

        private void Connection(IAsyncResult result)
        {
            ServerAsyncInfo async = (ServerAsyncInfo)result.AsyncState;

            try
            {
                async.Server.EndWaitForConnection(result);

                //Process the connection if the server was successfully connected.
                if (async.Server.IsConnected)
                {
                    //Read the message from the secondary instance
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[8192];
                    do
                    {
                        int lastRead = async.Server.Read(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, lastRead);
                    }
                    while (!async.Server.IsMessageComplete);

                    BinaryFormatter bf = new BinaryFormatter();
                    ms.Position = 0;
                    string[] args = (string[])bf.Deserialize(ms);

                    //Let the event handler process the message.
                    if (OnNextInstance != null)
                        OnNextInstance(args);
                }

                async.Server.Close();
                BeginWaitForConnection();
            }
            catch (Exception m)
            {
                Logger.Instance.Error(m);
            }
        }

        private struct ServerAsyncInfo
        {
            public NamedPipeServerStream Server;
        }
    }
}
