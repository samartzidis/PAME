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
using System.IO;
using System.Collections;

namespace Pame
{
    public partial class ErrorsForm : DockContent
    {
        public delegate void ErrorSelectedEventHandler(ErrorsForm form, Compiler.Message msg);
        public event ErrorSelectedEventHandler OnErrorSelected;        
        private Color oldColor = SystemColors.Window;
        private readonly Color flashColor = Color.OrangeRed;
        private ListViewColumnSorter lvwColumnSorter;

        public ErrorsForm()
        {
            InitializeComponent();

            lvwColumnSorter = new ListViewColumnSorter();
            listViewErrors.ListViewItemSorter = lvwColumnSorter;
            lvwColumnSorter.SortColumn = 0;
            lvwColumnSorter.Order = SortOrder.Descending;

            ImageList il = new ImageList();
            il.TransparentColor = Color.Magenta;
            il.Images.Add(Properties.Resources.info);
            il.Images.Add(Properties.Resources.warning);
            il.Images.Add(Properties.Resources.Error);
            il.Images.Add(Properties.Resources.CriticalError);
            listViewErrors.SmallImageList = il;
        }

        private void listViewErrors_Resize(object sender, EventArgs e)
        {
            columnHeaderFile.Width = -2;
        }

        public void Clear()
        {                        
            listViewErrors.Items.Clear();
        }

        public void AddMessage(Compiler.Message msg)
        {  
            int iconIndex = 0;
            switch (msg.error.severity)
            {
                case "Note":
                    iconIndex = 0;
                    break;
                case "Warning":
                    iconIndex = 1;
                    break;
                case "Error":
                    iconIndex = 2;
                    break;
                case "Fatal":
                    iconIndex = 3;
                    break;
                default:
                    break;
            }

            string absPath = msg.error.file;
            if(File.Exists(msg.error.file))
                absPath = new FileInfo(msg.error.file).FullName;

            ListViewItem item = new ListViewItem(new string[] 
                { 
                    msg.error.severity, 
                    msg.error.msg, 
                    msg.error.line > 0 ? msg.error.line + "" : "", 
                    msg.error.col > 0 ? msg.error.col + "" : "", 
                    absPath
                },
                iconIndex
                );
            item.Tag = msg;
            listViewErrors.Items.Add(item);
        }        

        public Compiler.Message SelectedError
        {
            get { return (Compiler.Message)listViewErrors.SelectedItems[0].Tag; }
        }

        private void listViewErrors_MouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        private void listViewErrors_MouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (listViewErrors.SelectedItems.Count > 0 && listViewErrors.SelectedItems[0].Tag != null)
            {
                Compiler.Message msg = (Compiler.Message)listViewErrors.SelectedItems[0].Tag;                
                if (OnErrorSelected != null)
                    OnErrorSelected(this, msg);
            }
        }

        private void listViewErrors_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                    lvwColumnSorter.Order = SortOrder.Descending;
                else
                    lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listViewErrors.Sort();
        }

        class SeverityComparer : IComparer
        {
            static Dictionary<string, int> severities = new Dictionary<string, int>();

            static SeverityComparer()
            {
                severities["Note"] = 0;
                severities["Warning"] = 1;
                severities["Error"] = 2;
                severities["Fatal"] = 3;
            }

            public int Compare(object x, object y)
            {
                if (!(x is string))
                    throw new ArgumentException();
                if (!(y is string))
                    throw new ArgumentException();

                if (!severities.ContainsKey(x.ToString()) || !severities.ContainsKey(x.ToString()))
                    return 0;

                return severities[x.ToString()] - severities[y.ToString()];
            }
        }

        class NumberComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                double nx, ny = 0;
                double.TryParse(x.ToString(), out nx);
                double.TryParse(y.ToString(), out ny);

                return (int)(nx - ny);
            }
        }

        class ListViewColumnSorter : IComparer
        {
            private int ColumnToSort;
            private SortOrder OrderOfSort;
            private CaseInsensitiveComparer stringComparer;
            private SeverityComparer severityComparer;
            private NumberComparer numberComparer;

            public ListViewColumnSorter()
            {
                // Initialize the column to '0'
                ColumnToSort = 0;

                // Initialize the sort order to 'none'
                OrderOfSort = SortOrder.None;

                stringComparer = new CaseInsensitiveComparer();
                severityComparer = new SeverityComparer();
                numberComparer = new NumberComparer();
            }

            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                // Compare the two items
                if (ColumnToSort == 0)
                    compareResult = severityComparer.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                else if (ColumnToSort == 2 || ColumnToSort == 3)
                    compareResult = numberComparer.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                else
                    compareResult = stringComparer.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                    return compareResult;
                else if (OrderOfSort == SortOrder.Descending)
                    return (-compareResult);
                else
                    return 0;
            }

            public int SortColumn
            {
                set { ColumnToSort = value; }
                get { return ColumnToSort; }
            }

            public SortOrder Order
            {
                set { OrderOfSort = value; }
                get { return OrderOfSort; }
            }
        }
    }    
}