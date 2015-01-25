namespace VVVV.Nodes.Timeliner
{
	partial class TLMidiPin
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
			this.FilenameLabel = new System.Windows.Forms.Label();
			this.SaveButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.MaxNote = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.MinNote = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.Numerator = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.Denominator = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.BPM = new VVVV.Nodes.Timeliner.TLIOBoxValue();
			this.SuspendLayout();
			// 
			// FilenameLabel
			// 
			this.FilenameLabel.Location = new System.Drawing.Point(28, 44);
			this.FilenameLabel.Name = "FilenameLabel";
			this.FilenameLabel.Size = new System.Drawing.Size(119, 15);
			this.FilenameLabel.TabIndex = 4;
			this.FilenameLabel.Text = "*.mid";
			this.FilenameLabel.Click += new System.EventHandler(this.FilenameLabelClick);
			// 
			// SaveButton
			// 
			this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SaveButton.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveButton.Location = new System.Drawing.Point(5, 40);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(20, 20);
			this.SaveButton.TabIndex = 5;
			this.SaveButton.Text = "S";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButtonClick);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(5, 65);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "Max Note";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(5, 85);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 15);
			this.label3.TabIndex = 7;
			this.label3.Text = "Min Note";
			// 
			// MaxNote
			// 
			this.MaxNote.BackColor = System.Drawing.Color.Silver;
			this.MaxNote.Cyclic = false;
			this.MaxNote.IsInteger = true;
			this.MaxNote.Location = new System.Drawing.Point(61, 65);
			this.MaxNote.Maximum = 127;
			this.MaxNote.Minimum = 0;
			this.MaxNote.Name = "MaxNote";
			this.MaxNote.Size = new System.Drawing.Size(30, 15);
			this.MaxNote.TabIndex = 8;
			this.MaxNote.Value = 90;
			this.MaxNote.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.MinMaxIOChangedCB);
			// 
			// MinNote
			// 
			this.MinNote.BackColor = System.Drawing.Color.Silver;
			this.MinNote.Cyclic = false;
			this.MinNote.IsInteger = true;
			this.MinNote.Location = new System.Drawing.Point(61, 85);
			this.MinNote.Maximum = 127;
			this.MinNote.Minimum = 0;
			this.MinNote.Name = "MinNote";
			this.MinNote.Size = new System.Drawing.Size(30, 15);
			this.MinNote.TabIndex = 9;
			this.MinNote.Value = 30;
			this.MinNote.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.MinMaxIOChangedCB);
			// 
			// Numerator
			// 
			this.Numerator.BackColor = System.Drawing.Color.Silver;
			this.Numerator.Cyclic = false;
			this.Numerator.IsInteger = true;
			this.Numerator.Location = new System.Drawing.Point(61, 105);
			this.Numerator.Maximum = 64;
			this.Numerator.Minimum = 1;
			this.Numerator.Name = "Numerator";
			this.Numerator.Size = new System.Drawing.Size(30, 15);
			this.Numerator.TabIndex = 10;
			this.Numerator.Value = 4;
			this.Numerator.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.TimeSignatureChange);
			// 
			// Denominator
			// 
			this.Denominator.BackColor = System.Drawing.Color.Silver;
			this.Denominator.Cyclic = false;
			this.Denominator.IsInteger = true;
			this.Denominator.Location = new System.Drawing.Point(97, 105);
			this.Denominator.Maximum = 64;
			this.Denominator.Minimum = 1;
			this.Denominator.Name = "Denominator";
			this.Denominator.Size = new System.Drawing.Size(30, 15);
			this.Denominator.TabIndex = 11;
			this.Denominator.Value = 4;
			this.Denominator.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.TimeSignatureChange);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 105);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 15);
			this.label1.TabIndex = 12;
			this.label1.Text = "Rhythm";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(5, 125);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 15);
			this.label4.TabIndex = 13;
			this.label4.Text = "BPM";
			// 
			// BPM
			// 
			this.BPM.BackColor = System.Drawing.Color.Silver;
			this.BPM.Cyclic = false;
			this.BPM.IsInteger = true;
			this.BPM.Location = new System.Drawing.Point(61, 125);
			this.BPM.Maximum = 999;
			this.BPM.Minimum = 20;
			this.BPM.Name = "BPM";
			this.BPM.Size = new System.Drawing.Size(30, 15);
			this.BPM.TabIndex = 14;
			this.BPM.Value = 120;
			this.BPM.OnValueChange += new VVVV.Nodes.Timeliner.ValueChangeHandler(this.TimeSignatureChange);
			// 
			// TLMidiPin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Controls.Add(this.BPM);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.MinNote);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Denominator);
			this.Controls.Add(this.Numerator);
			this.Controls.Add(this.MaxNote);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.FilenameLabel);
			this.Controls.SetChildIndex(this.FilenameLabel, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.MaxNote, 0);
			this.Controls.SetChildIndex(this.Numerator, 0);
			this.Controls.SetChildIndex(this.Denominator, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.MinNote, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.BPM, 0);
			this.ResumeLayout(false);
		}
		private VVVV.Nodes.Timeliner.TLIOBoxValue BPM;
		private System.Windows.Forms.Label label4;
		private VVVV.Nodes.Timeliner.TLIOBoxValue Numerator;
		private VVVV.Nodes.Timeliner.TLIOBoxValue Denominator;
		private System.Windows.Forms.Label label1;
		private VVVV.Nodes.Timeliner.TLIOBoxValue MinNote;
		private VVVV.Nodes.Timeliner.TLIOBoxValue MaxNote;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.Label FilenameLabel;

	}
}
