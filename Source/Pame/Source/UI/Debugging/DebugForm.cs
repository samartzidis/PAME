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
using WeifenLuo.WinFormsUI.Docking;

namespace Pame
{
    public partial class DebugForm : DockContent
    {
        public DebugForm()
        {
            InitializeComponent();

            varListView.ImageList = new ImageList();
            varListView.ImageList.Images.Add(Properties.Resources.blackbox);
            varListView.ImageList.Images.Add(Properties.Resources.whitebox);
            varListView.DefaultImageIndex = 0;
            varListView.DefaultSelectedImageIndex = 1;
        }

        public VarListView VarList
        {
            get { return varListView; }
        }
    }
}
