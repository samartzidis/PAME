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

namespace PameNativeAdapter
{
    internal class ConsoleInfo32
    {
        [DllImport("PameNative.dll", CharSet = CharSet.Unicode)]
        internal static extern void SetConsoleFontSizeTo(IntPtr console, int sizeY, int sizeX, string fontName);
    }

    internal class ConsoleInfo64
    {
        [DllImport("PameNative64.dll", CharSet = CharSet.Unicode)]
        internal static extern void SetConsoleFontSizeTo(IntPtr console, int sizeY, int sizeX, string fontName);
    }

    public class ConsoleInfo
    {       

        public static void SetConsoleFontSizeTo(IntPtr console, int sizeY, int sizeX, string fontName)
        {
            if (IntPtr.Size == 8)
                ConsoleInfo64.SetConsoleFontSizeTo(console, sizeY, sizeX, fontName);
            else
                ConsoleInfo32.SetConsoleFontSizeTo(console, sizeY, sizeX, fontName);
        }
    }
}
