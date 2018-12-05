namespace Hoster
{
	partial class PinPanel
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
			this.SuspendLayout();
			// 
			// PinPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.DoubleBuffered = true;
			this.Name = "PinPanel";
			this.Size = new System.Drawing.Size(702, 53);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PinPanelMouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PinPanelMouseMove);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.PinPanelPaint);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PinPanelMouseUp);
			this.ResumeLayout(false);
		}
	}
}
