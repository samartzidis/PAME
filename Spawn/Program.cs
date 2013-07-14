//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace Spawn
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: "  + 
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + 
                    " [path] [param]*");
                return 1;
            }

            if (!File.Exists(args[0]))
            {
                MessageBox.Show("Executable file to be spawned: \"" + args[0] + "\" not found",
                    "Spawn.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            string strArgs = "";
            for (int k = 1; k < args.Length; k++)
                strArgs += args[k] + " ";
            string pwd = Path.GetDirectoryName(args[0]);            

            Process proc = null;
            try
            {                
                proc = new Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = false;
                proc.StartInfo.FileName = args[0];
                proc.StartInfo.WorkingDirectory = pwd;
                
                if (strArgs.Length > 0)
                    proc.StartInfo.Arguments = strArgs;
                if (!proc.Start())
                    return 1;
            }
            catch (Exception m)
            {
                string strParams = "Path = \"" + args[0] + "\", " + "Args = \"" + strArgs + "\", " + "Pwd = \"" + pwd + "\"";
                MessageBox.Show(strParams + " (" + m.Message + ")", 
                    "Spawn.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            //Wait for process termination
            proc.WaitForExit();

            Console.WriteLine();
            Console.WriteLine("Το πρόγραμμα τερματίστηκε, πατήστε Enter για έξοδο...");
            Console.ReadLine();

            return proc.ExitCode;
        }
    }
}
