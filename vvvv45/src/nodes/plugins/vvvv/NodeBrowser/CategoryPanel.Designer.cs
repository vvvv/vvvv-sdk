namespace VVVV.Nodes.NodeBrowser
{
	partial class CategoryPanel
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.FCategoryTreeViewer = new VVVV.HDE.Viewer.WinFormsViewer.TreeViewer();
			this.SuspendLayout();
			// 
			// FCategoryTreeViewer
			// 
			this.FCategoryTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FCategoryTreeViewer.FlatStyle = true;
			this.FCategoryTreeViewer.Location = new System.Drawing.Point(0, 0);
			this.FCategoryTreeViewer.Name = "FCategoryTreeViewer";
			this.FCategoryTreeViewer.ShowLines = false;
			this.FCategoryTreeViewer.ShowPlusMinus = false;
			this.FCategoryTreeViewer.ShowRoot = false;
			this.FCategoryTreeViewer.ShowRootLines = false;
			this.FCategoryTreeViewer.ShowTooltip = true;
			this.FCategoryTreeViewer.Size = new System.Drawing.Size(368, 398);
			this.FCategoryTreeViewer.TabIndex = 9;
			this.FCategoryTreeViewer.MouseDown += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.HandleTreeViewerMouseDown);
			// 
			// CategoryPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FCategoryTreeViewer);
			this.Name = "CategoryPanel";
			this.Size = new System.Drawing.Size(368, 398);
			this.VisibleChanged += new System.EventHandler(this.HandlePanelVisibleChanged);
			this.ResumeLayout(false);
		}
		private VVVV.HDE.Viewer.WinFormsViewer.TreeViewer FCategoryTreeViewer;
	}
}
