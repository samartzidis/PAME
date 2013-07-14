//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace Pame
{
	/// <summary>
	/// Summary description for Splash.
	/// </summary>
	public class SplashForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Timer timer;
        private IContainer components;
        int delay = 0;
        AutoResetEvent ev = new AutoResetEvent(false);

		public SplashForm()
		{
			InitializeComponent();			            
		}

        public void Wait(int delay)
        {
            ev.WaitOne(delay);
        }

        public void Show(int delay)
        {
            if (timer.Enabled)
                timer.Stop();

            Show();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Application.DoEvents();

            if (delay > 0)
            {
                Show();
                delay -= timer.Interval;
            }
            else
            {
                ev.Set();
            }
        }

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // SplashForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::Pame.Properties.Resources.splash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(642, 347);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Splash";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.SystemColors.Control;
            this.ResumeLayout(false);

		}
		#endregion
	}
}
