namespace VVVV.Nodes.Timeliner
{
	partial class TLEditorValue
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
			this.ValueBox = new TLIOBoxValue();
			this.SuspendLayout();
			// 
			// TimeBox
			// 
			this.TimeBox.Size = new System.Drawing.Size(75, 15);
			// 
			// ValueBox
			// 
			this.ValueBox.BackColor = System.Drawing.Color.Silver;
			this.ValueBox.Location = new System.Drawing.Point(0, 16);
			this.ValueBox.Name = "ValueBox";
			this.ValueBox.Size = new System.Drawing.Size(75, 15);
			this.ValueBox.TabIndex = 12;
			this.ValueBox.Value = 0;
			this.ValueBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValueBoxKeyPress);
			// 
			// TLEditorValue
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ValueBox);
			this.Name = "TLEditorValue";
			this.Size = new System.Drawing.Size(75, 38);
			this.Controls.SetChildIndex(this.ValueBox, 0);
			this.Controls.SetChildIndex(this.TimeBox, 0);
			this.ResumeLayout(false);
		}
		
		private TLIOBoxValue ValueBox;
}
}

