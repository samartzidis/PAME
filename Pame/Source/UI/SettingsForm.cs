//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Pame
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = new SettingsAdapter();
            
            string str = string.Empty;
            List<DynamicVariables.Variable> vars = 
                new List<DynamicVariables.Variable>(DynamicVariables.Instance.GetVariables());
            foreach (DynamicVariables.Variable var in vars)
                tbDynamicVars.Text += "$(" + var.ToString() + ") = \"" + DynamicVariables.Instance[var] + "\"\r\n";
        }

        private void defaultsButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            propertyGrid.Refresh();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
        }
    }    
}