namespace VVVV.Nodes.Timeliner
{
	partial class TLEditorState
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
			this.NameBox = new System.Windows.Forms.TextBox();
			this.EventsBox = new System.Windows.Forms.TextBox();
			this.EndActionBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// TimeBox
			// 
			this.TimeBox.Size = new System.Drawing.Size(119, 15);
			// 
			// NameBox
			// 
			this.NameBox.BackColor = System.Drawing.Color.Silver;
			this.NameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.NameBox.Location = new System.Drawing.Point(47, 16);
			this.NameBox.Name = "NameBox";
			this.NameBox.Size = new System.Drawing.Size(72, 13);
			this.NameBox.TabIndex = 0;
			this.NameBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextKeyPress);
			// 
			// EventsBox
			// 
			this.EventsBox.BackColor = System.Drawing.Color.Silver;
			this.EventsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.EventsBox.Location = new System.Drawing.Point(0, 46);
			this.EventsBox.Multiline = true;
			this.EventsBox.Name = "EventsBox";
			this.EventsBox.Size = new System.Drawing.Size(119, 77);
			this.EventsBox.TabIndex = 12;
			// 
			// EndActionBox
			// 
			this.EndActionBox.BackColor = System.Drawing.Color.Silver;
			this.EndActionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.EndActionBox.Location = new System.Drawing.Point(47, 124);
			this.EndActionBox.Name = "EndActionBox";
			this.EndActionBox.Size = new System.Drawing.Size(72, 13);
			this.EndActionBox.TabIndex = 13;
			this.EndActionBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextKeyPress);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(0, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 16);
			this.label1.TabIndex = 14;
			this.label1.Text = "Name";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(0, 124);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 17);
			this.label2.TabIndex = 15;
			this.label2.Text = "OnEnd";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(0, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 16);
			this.label3.TabIndex = 16;
			this.label3.Text = "Event Tupel";
			// 
			// TLEditorState
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.EndActionBox);
			this.Controls.Add(this.EventsBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.NameBox);
			this.Name = "TLEditorState";
			this.Size = new System.Drawing.Size(119, 138);
			this.Controls.SetChildIndex(this.TimeBox, 0);
			this.Controls.SetChildIndex(this.NameBox, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.EventsBox, 0);
			this.Controls.SetChildIndex(this.EndActionBox, 0);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TextBox NameBox;
		private System.Windows.Forms.TextBox EventsBox;
		private System.Windows.Forms.TextBox EndActionBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
	}
}
