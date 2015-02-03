//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PameNativeAdapter
{
    class Test
    {
        static void Main(string[] args)
        {
            new Test();
        }

        public Test()
        {  
            IntPtr hWndConsole = Win32.GetConsoleWindow();
            ConsoleInfo.SetConsoleFontSizeTo(hWndConsole, 0, 0, "Lucida Console");
            Console.WriteLine("Γεια!");
        }
    }
}
