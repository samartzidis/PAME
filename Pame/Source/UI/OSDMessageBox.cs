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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pame
{
    public partial class OSDMessageBox : Form
    {        
        public OSDMessageBox()
        {
            InitializeComponent();

            Opacity = .9;

            timer.Interval = 80;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();            
        }
        
        void timer_Tick(object sender, EventArgs e)
        {           
            Opacity -= .1;
            Refresh();

            if (Opacity <= 0)
            {
                timer.Stop();
                Close();
            }
        }

        public void Show(IWin32Window owner, string text)
        {            
            label1.Text = text;            

            Show(owner);
            CenterToParent();
            timer.Start();
        }  
    }
}
