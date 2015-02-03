namespace Pame
{
    partial class ErrorsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        //private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorsForm));
            this.listViewErrors = new System.Windows.Forms.ListView();
            this.columnHeaderSeverity = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDescription = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderLine = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderColumn = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // listViewErrors
            // 
            this.listViewErrors.AllowColumnReorder = true;
            this.listViewErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSeverity,
            this.columnHeaderDescription,
            this.columnHeaderLine,
            this.columnHeaderColumn,
            this.columnHeaderFile});
            resources.ApplyResources(this.listViewErrors, "listViewErrors");
            this.listViewErrors.FullRowSelect = true;
            this.listViewErrors.GridLines = true;
            this.listViewErrors.HideSelection = false;
            this.listViewErrors.Name = "listViewErrors";
            this.listViewErrors.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listViewErrors.UseCompatibleStateImageBehavior = false;
            this.listViewErrors.View = System.Windows.Forms.View.Details;
            this.listViewErrors.Resize += new System.EventHandler(this.listViewErrors_Resize);
            this.listViewErrors.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewErrors_MouseUp);
            this.listViewErrors.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewErrors_ColumnClick);
            this.listViewErrors.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listViewErrors_MouseDown);
            // 
            // columnHeaderSeverity
            // 
            resources.ApplyResources(this.columnHeaderSeverity, "columnHeaderSeverity");
            // 
            // columnHeaderDescription
            // 
            resources.ApplyResources(this.columnHeaderDescription, "columnHeaderDescription");
            // 
            // columnHeaderLine
            // 
            resources.ApplyResources(this.columnHeaderLine, "columnHeaderLine");
            // 
            // columnHeaderColumn
            // 
            resources.ApplyResources(this.columnHeaderColumn, "columnHeaderColumn");
            // 
            // columnHeaderFile
            // 
            resources.ApplyResources(this.columnHeaderFile, "columnHeaderFile");
            // 
            // ErrorsForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.listViewErrors);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.HideOnClose = true;
            this.Name = "ErrorsForm";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
            this.ResumeLayout(false);

		}
		#endregion

        private System.Windows.Forms.ListView listViewErrors;
        private System.Windows.Forms.ColumnHeader columnHeaderSeverity;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.ColumnHeader columnHeaderColumn;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;

    }
}