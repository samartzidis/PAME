//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Pame
{
    public partial class OutputForm : DockContent
    {
        public OutputForm()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            lock(tbOutput)
                tbOutput.Clear();
        }

        public void OutputString(string str)
        {
            lock (tbOutput)
                tbOutput.AppendText(str);
        }

        public void OutputStringLn(string str)
        {
            OutputString(str + "\r\n");
        }
    }
}