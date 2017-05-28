namespace VVVV.Nodes.Timeliner
{
	partial class TLEditorColor
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.HueBox = new TLIOBoxValue();
			this.SatBox = new TLIOBoxValue();
			this.LumBox = new TLIOBoxValue();
			this.AlphaBox = new TLIOBoxValue();
			this.SuspendLayout();
			// 
			// TimeBox
			// 
			this.TimeBox.Size = new System.Drawing.Size(110, 15);
			// 
			// label1
			// 
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(2, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 17);
			this.label1.TabIndex = 5;
			this.label1.Text = "Hue";
			// 
			// label2
			// 
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(2, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 17);
			this.label2.TabIndex = 6;
			this.label2.Text = "Saturation";
			// 
			// label3
			// 
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(2, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(62, 18);
			this.label3.TabIndex = 7;
			this.label3.Text = "Value";
			// 
			// label4
			// 
			this.label4.ForeColor = System.Drawing.Color.Black;
			this.label4.Location = new System.Drawing.Point(2, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 23);
			this.label4.TabIndex = 8;
			this.label4.Text = "Alpha";
			// 
			// HueBox
			// 
			this.HueBox.BackColor = System.Drawing.Color.Silver;
			this.HueBox.Cyclic = false;
			this.HueBox.Location = new System.Drawing.Point(64, 16);
			this.HueBox.Maximum = 1;
			this.HueBox.Minimum = 0;
			this.HueBox.Name = "HueBox";
			this.HueBox.Size = new System.Drawing.Size(45, 15);
			this.HueBox.TabIndex = 12;
			this.HueBox.Value = 0;
			this.HueBox.OnValueChange += new ValueChangeHandler(this.HueBoxOnValueChange);
			this.HueBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BoxKeyPress);
			// 
			// SatBox
			// 
			this.SatBox.BackColor = System.Drawing.Color.Silver;
			this.SatBox.Cyclic = false;
			this.SatBox.Location = new System.Drawing.Point(64, 32);
			this.SatBox.Maximum = 1;
			this.SatBox.Minimum = 0;
			this.SatBox.Name = "SatBox";
			this.SatBox.Size = new System.Drawing.Size(45, 15);
			this.SatBox.TabIndex = 13;
			this.SatBox.Value = 0;
			this.SatBox.OnValueChange += new ValueChangeHandler(this.SatBoxOnValueChange);
			this.SatBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BoxKeyPress);
			// 
			// LumBox
			// 
			this.LumBox.BackColor = System.Drawing.Color.Silver;
			this.LumBox.Cyclic = false;
			this.LumBox.Location = new System.Drawing.Point(64, 48);
			this.LumBox.Maximum = 1;
			this.LumBox.Minimum = 0;
			this.LumBox.Name = "LumBox";
			this.LumBox.Size = new System.Drawing.Size(45, 15);
			this.LumBox.TabIndex = 14;
			this.LumBox.Value = 0;
			this.LumBox.OnValueChange += new ValueChangeHandler(this.LumBoxOnValueChange);
			this.LumBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BoxKeyPress);
			// 
			// AlphaBox
			// 
			this.AlphaBox.BackColor = System.Drawing.Color.Silver;
			this.AlphaBox.Cyclic = false;
			this.AlphaBox.Location = new System.Drawing.Point(64, 64);
			this.AlphaBox.Maximum = 1;
			this.AlphaBox.Minimum = 0;
			this.AlphaBox.Name = "AlphaBox";
			this.AlphaBox.Size = new System.Drawing.Size(45, 15);
			this.AlphaBox.TabIndex = 15;
			this.AlphaBox.Value = 0;
			this.AlphaBox.OnValueChange += new ValueChangeHandler(this.AlphaBoxOnValueChange);
			this.AlphaBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BoxKeyPress);
			// 
			// TLEditorColor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.AlphaBox);
			this.Controls.Add(this.LumBox);
			this.Controls.Add(this.SatBox);
			this.Controls.Add(this.HueBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "TLEditorColor";
			this.Size = new System.Drawing.Size(110, 96);
			this.Controls.SetChildIndex(this.TimeBox, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.HueBox, 0);
			this.Controls.SetChildIndex(this.SatBox, 0);
			this.Controls.SetChildIndex(this.LumBox, 0);
			this.Controls.SetChildIndex(this.AlphaBox, 0);
			this.ResumeLayout(false);
		}
		private TLIOBoxValue HueBox;
		private TLIOBoxValue SatBox;
		private TLIOBoxValue LumBox;
		private TLIOBoxValue AlphaBox;

		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
	}
}
