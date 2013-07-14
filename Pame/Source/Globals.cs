//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;

namespace Pame
{
    public static class Globals
    {
        private static string m_ExePath = null;
        private static string m_DirPath = null;

        public static int InRange(this int x, int lo, int hi)
        {
            Debug.Assert(lo <= hi);
            return x < lo ? lo : (x > hi ? hi : x);
        }

        public static bool IsInRange(this int x, int lo, int hi)
        {
            return x >= lo && x <= hi;
        }

        public static Color HalfMix(this Color one, Color two)
        {
            return Color.FromArgb(
                (one.A + two.A) >> 1,
                (one.R + two.R) >> 1,
                (one.G + two.G) >> 1,
                (one.B + two.B) >> 1);
        }

        public static string GetExePath()
        {
            if (m_ExePath == null)
                m_ExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            return m_ExePath;
        }


        public static string GetExeDirPath()
        {
            if (m_DirPath == null)            
                m_DirPath = System.IO.Path.GetDirectoryName(GetExePath());                            

            return m_DirPath;
        }

        public static string FormatVersionString(Version ver)
        {
            return String.Format("v{0}.{1} (build {2:0##})", ver.Major, ver.Minor, ver.Build);
        }        
    }
}
