namespace VVVV.Nodes.Timeliner
{
	partial class TLValuePin
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
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.MinIO = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.MaxIO = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.StepRadio = new System.Windows.Forms.RadioButton();
			this.LinearRadio = new System.Windows.Forms.RadioButton();
			this.CubicRadio = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(5, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "Minimum";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(5, 41);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 15);
			this.label3.TabIndex = 7;
			this.label3.Text = "Maximum";
			// 
			// MinIO
			// 
			this.MinIO.BackColor = System.Drawing.Color.Silver;
			this.MinIO.Cyclic = false;
			this.MinIO.IsInteger = false;
			this.MinIO.Location = new System.Drawing.Point(61, 61);
			this.MinIO.Maximum = 1;
			this.MinIO.Minimum = -1;
			this.MinIO.Name = "MinIO";
			this.MinIO.Size = new System.Drawing.Size(48, 15);
			this.MinIO.TabIndex = 8;
			this.MinIO.Value = -1;
			this.MinIO.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.MinMaxIOChangedCB);
			// 
			// MaxIO
			// 
			this.MaxIO.BackColor = System.Drawing.Color.Silver;
			this.MaxIO.Cyclic = false;
			this.MaxIO.IsInteger = false;
			this.MaxIO.Location = new System.Drawing.Point(61, 41);
			this.MaxIO.Maximum = 1;
			this.MaxIO.Minimum = -1;
			this.MaxIO.Name = "MaxIO";
			this.MaxIO.Size = new System.Drawing.Size(48, 15);
			this.MaxIO.TabIndex = 9;
			this.MaxIO.Value = 1;
			this.MaxIO.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.MinMaxIOChangedCB);
			// 
			// StepRadio
			// 
			this.StepRadio.Location = new System.Drawing.Point(3, 81);
			this.StepRadio.Name = "StepRadio";
			this.StepRadio.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.StepRadio.Size = new System.Drawing.Size(80, 17);
			this.StepRadio.TabIndex = 10;
			this.StepRadio.TabStop = true;
			this.StepRadio.Text = "Step";
			this.StepRadio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.StepRadio.UseVisualStyleBackColor = true;
			this.StepRadio.Click += new System.EventHandler(this.RadioButtonClick);
			// 
			// LinearRadio
			// 
			this.LinearRadio.Location = new System.Drawing.Point(3, 101);
			this.LinearRadio.Name = "LinearRadio";
			this.LinearRadio.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.LinearRadio.Size = new System.Drawing.Size(80, 15);
			this.LinearRadio.TabIndex = 11;
			this.LinearRadio.TabStop = true;
			this.LinearRadio.Text = "Linear";
			this.LinearRadio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.LinearRadio.UseVisualStyleBackColor = true;
			this.LinearRadio.Click += new System.EventHandler(this.RadioButtonClick);
			// 
			// CubicRadio
			// 
			this.CubicRadio.Location = new System.Drawing.Point(3, 121);
			this.CubicRadio.Name = "CubicRadio";
			this.CubicRadio.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.CubicRadio.Size = new System.Drawing.Size(80, 15);
			this.CubicRadio.TabIndex = 12;
			this.CubicRadio.TabStop = true;
			this.CubicRadio.Text = "Cubic";
			this.CubicRadio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CubicRadio.UseVisualStyleBackColor = true;
			this.CubicRadio.Click += new System.EventHandler(this.RadioButtonClick);
			// 
			// TLValuePin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Controls.Add(this.CubicRadio);
			this.Controls.Add(this.LinearRadio);
			this.Controls.Add(this.StepRadio);
			this.Controls.Add(this.MaxIO);
			this.Controls.Add(this.MinIO);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			//this.Size = new System.Drawing.Size(150, 143);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.MinIO, 0);
			this.Controls.SetChildIndex(this.MaxIO, 0);
			this.Controls.SetChildIndex(this.StepRadio, 0);
			this.Controls.SetChildIndex(this.LinearRadio, 0);
			this.Controls.SetChildIndex(this.CubicRadio, 0);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.RadioButton StepRadio;
		private System.Windows.Forms.RadioButton LinearRadio;
		private System.Windows.Forms.RadioButton CubicRadio;
		private VVVV.Nodes.Timeliner.TLIOBoxValue MinIO;
		private VVVV.Nodes.Timeliner.TLIOBoxValue MaxIO;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;

	}
}
