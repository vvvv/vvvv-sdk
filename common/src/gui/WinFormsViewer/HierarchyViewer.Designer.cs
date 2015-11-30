namespace VVVV.HDE.Viewer.WinFormsViewer
{
    partial class HierarchyViewer
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
        		
        		if (FRootNode != null)
        			FRootNode.Dispose();
        		
        		if (FRootMapper != null)
        			FRootMapper.Dispose();
        		
        		FGraphEditor.Dispose();
        		FGraphEditor = null;
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
        	this.components = new System.ComponentModel.Container();
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HierarchyViewer));
        	this.FToolTip = new System.Windows.Forms.ToolTip(this.components);
        	this.FGraphEditor = new VVVV.HDE.GraphicalEditing.GraphEditor();
        	this.SuspendLayout();
        	// 
        	// FToolTip
        	// 
        	this.FToolTip.BackColor = System.Drawing.Color.Gray;
        	this.FToolTip.ForeColor = System.Drawing.Color.White;
        	this.FToolTip.Popup += new System.Windows.Forms.PopupEventHandler(this.ToolTipPopupHandler);
        	// 
        	// FGraphEditor
        	// 
        	this.FGraphEditor.Color = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
        	this.FGraphEditor.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FGraphEditor.Host = null;
        	this.FGraphEditor.Location = new System.Drawing.Point(0, 0);
        	this.FGraphEditor.Name = "FGraphEditor";
        	this.FGraphEditor.Size = new System.Drawing.Size(226, 243);
        	this.FGraphEditor.TabIndex = 0;
        	this.FGraphEditor.ViewCenter = ((System.Drawing.PointF)(resources.GetObject("FGraphEditor.ViewCenter")));
        	this.FGraphEditor.ViewSize = new System.Drawing.SizeF(226F, 243F);
        	this.FGraphEditor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FGraphEditorKeyUp);
        	
        	this.FGraphEditor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FGraphEditorKeyPress);
        	this.FGraphEditor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FGraphEditorKeyDown);
        	// 
        	// HierarchyViewer
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Controls.Add(this.FGraphEditor);
        	this.Name = "HierarchyViewer";
        	this.Size = new System.Drawing.Size(226, 243);
        	this.ResumeLayout(false);
        }
        private VVVV.HDE.GraphicalEditing.GraphEditor FGraphEditor;
        private System.Windows.Forms.ToolTip FToolTip;
    }
}
