namespace VVVV.Nodes.Timeliner
{
	partial class TLEditor
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
			this.TimeBox = new TLIOBoxValue();
			this.SuspendLayout();
			// 
			// TimeBox
			// 
			this.TimeBox.BackColor = System.Drawing.Color.Silver;
			this.TimeBox.Cyclic = false;
			this.TimeBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TimeBox.Location = new System.Drawing.Point(0, 0);
			this.TimeBox.Maximum = 0;
			this.TimeBox.Minimum = 0;
			this.TimeBox.Name = "TimeBox";
			this.TimeBox.Size = new System.Drawing.Size(97, 15);
			this.TimeBox.TabIndex = 11;
			this.TimeBox.Value = 0;
			this.TimeBox.OnValueChange += new ValueChangeHandler(this.TimeBoxOnValueChange);
			this.TimeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TimeBoxKeyPress);
			// 
			// TLEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.TimeBox);
			this.Name = "TLEditor";
			this.Size = new System.Drawing.Size(97, 56);
			this.ResumeLayout(false);
		}
		protected TLIOBoxValue TimeBox;
	}
}
