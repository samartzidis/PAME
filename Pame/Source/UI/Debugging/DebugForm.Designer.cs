namespace Pame
{
    partial class DebugForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugForm));
            this.varListView = new Pame.VarListView();
            this.containerColumnHeaderName = new WinControls.ListView.ContainerColumnHeader();
            this.containerColumnHeaderValue = new WinControls.ListView.ContainerColumnHeader();
            this.containerColumnHeaderType = new WinControls.ListView.ContainerColumnHeader();
            this.SuspendLayout();
            // 
            // varListView
            // 
            this.varListView.AllowColumnReOrder = true;
            resources.ApplyResources(this.varListView, "varListView");
            this.varListView.ColumnHeaderStyles = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.varListView.Columns.AddRange(new WinControls.ListView.ContainerColumnHeader[] {
            this.containerColumnHeaderName,
            this.containerColumnHeaderValue,
            this.containerColumnHeaderType});
            this.varListView.DefaultFolderImages = false;
            this.varListView.DefaultImageIndex = -1;
            this.varListView.DefaultSelectedImageIndex = -1;
            this.varListView.Name = "varListView";
            this.varListView.VisualStyles = false;
            // 
            // containerColumnHeaderName
            // 
            resources.ApplyResources(this.containerColumnHeaderName, "containerColumnHeaderName");
            // 
            // containerColumnHeaderValue
            // 
            resources.ApplyResources(this.containerColumnHeaderValue, "containerColumnHeaderValue");
            // 
            // containerColumnHeaderType
            // 
            resources.ApplyResources(this.containerColumnHeaderType, "containerColumnHeaderType");
            // 
            // DebugForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.varListView);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.HideOnClose = true;
            this.Name = "DebugForm";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
            this.ResumeLayout(false);

        }

        #endregion

        private VarListView varListView;
        private WinControls.ListView.ContainerColumnHeader containerColumnHeaderName;
        private WinControls.ListView.ContainerColumnHeader containerColumnHeaderValue;
        private WinControls.ListView.ContainerColumnHeader containerColumnHeaderType;
    }
}