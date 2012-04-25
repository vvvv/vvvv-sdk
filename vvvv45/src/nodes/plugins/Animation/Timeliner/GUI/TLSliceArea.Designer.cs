namespace VVVV.Nodes.Timeliner
{
	partial class TLSliceArea
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
			this.SliceMenuPanel = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.AddBelowButton = new System.Windows.Forms.Button();
			this.AddAboveButton = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.MoveDownButton = new System.Windows.Forms.Button();
			this.MoveUpButton = new System.Windows.Forms.Button();
			this.RemoveSliceButton = new System.Windows.Forms.Button();
			this.SliceMenuPanel.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// SliceMenuPanel
			// 
			this.SliceMenuPanel.Controls.Add(this.panel3);
			this.SliceMenuPanel.Controls.Add(this.panel2);
			this.SliceMenuPanel.Controls.Add(this.RemoveSliceButton);
			this.SliceMenuPanel.Location = new System.Drawing.Point(0, 0);
			this.SliceMenuPanel.Name = "SliceMenuPanel";
			this.SliceMenuPanel.Size = new System.Drawing.Size(60, 111);
			this.SliceMenuPanel.TabIndex = 5;
			this.SliceMenuPanel.Visible = false;
			this.SliceMenuPanel.MouseLeave += new System.EventHandler(this.SliceMenuLeave);
			this.SliceMenuPanel.Resize += new System.EventHandler(this.MenuPanelResize);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.AddBelowButton);
			this.panel3.Controls.Add(this.AddAboveButton);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel3.Location = new System.Drawing.Point(40, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(20, 111);
			this.panel3.TabIndex = 15;
			// 
			// AddBelowButton
			// 
			this.AddBelowButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddBelowButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddBelowButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddBelowButton.ForeColor = System.Drawing.Color.Gray;
			this.AddBelowButton.Location = new System.Drawing.Point(0, 55);
			this.AddBelowButton.Name = "AddBelowButton";
			this.AddBelowButton.Size = new System.Drawing.Size(20, 56);
			this.AddBelowButton.TabIndex = 16;
			this.AddBelowButton.Text = "+";
			this.AddBelowButton.UseVisualStyleBackColor = false;
			this.AddBelowButton.MouseLeave += new System.EventHandler(this.SliceMenuLeave);
			this.AddBelowButton.Click += new System.EventHandler(this.AddBelowButtonClick);
			// 
			// AddAboveButton
			// 
			this.AddAboveButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.AddAboveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddAboveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddAboveButton.ForeColor = System.Drawing.Color.Gray;
			this.AddAboveButton.Location = new System.Drawing.Point(0, 0);
			this.AddAboveButton.Name = "AddAboveButton";
			this.AddAboveButton.Size = new System.Drawing.Size(20, 55);
			this.AddAboveButton.TabIndex = 15;
			this.AddAboveButton.Text = "+";
			this.AddAboveButton.UseVisualStyleBackColor = false;
			this.AddAboveButton.MouseLeave += new System.EventHandler(this.SliceMenuLeave);
			this.AddAboveButton.Click += new System.EventHandler(this.AddAboveButtonClick);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.MoveDownButton);
			this.panel2.Controls.Add(this.MoveUpButton);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(20, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(20, 111);
			this.panel2.TabIndex = 10;
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MoveDownButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MoveDownButton.ForeColor = System.Drawing.Color.Gray;
			this.MoveDownButton.Location = new System.Drawing.Point(0, 55);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = new System.Drawing.Size(20, 56);
			this.MoveDownButton.TabIndex = 18;
			this.MoveDownButton.Text = "v";
			this.MoveDownButton.UseVisualStyleBackColor = false;
			this.MoveDownButton.MouseLeave += new System.EventHandler(this.SliceMenuLeave);
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButtonClick);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.MoveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MoveUpButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MoveUpButton.ForeColor = System.Drawing.Color.Gray;
			this.MoveUpButton.Location = new System.Drawing.Point(0, 0);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = new System.Drawing.Size(20, 55);
			this.MoveUpButton.TabIndex = 17;
			this.MoveUpButton.Text = "^";
			this.MoveUpButton.UseVisualStyleBackColor = false;
			this.MoveUpButton.MouseLeave += new System.EventHandler(this.SliceMenuLeave);
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButtonClick);
			// 
			// RemoveSliceButton
			// 
			this.RemoveSliceButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.RemoveSliceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RemoveSliceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RemoveSliceButton.ForeColor = System.Drawing.Color.Gray;
			this.RemoveSliceButton.Location = new System.Drawing.Point(0, 0);
			this.RemoveSliceButton.Name = "RemoveSliceButton";
			this.RemoveSliceButton.Size = new System.Drawing.Size(20, 111);
			this.RemoveSliceButton.TabIndex = 5;
			this.RemoveSliceButton.TabStop = false;
			this.RemoveSliceButton.Text = "X";
			this.RemoveSliceButton.UseVisualStyleBackColor = false;
			this.RemoveSliceButton.MouseLeave += new System.EventHandler(this.SliceMenuLeave);
			this.RemoveSliceButton.Click += new System.EventHandler(this.RemoveSliceButtonClick);
			// 
			// TLSliceArea
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.Controls.Add(this.SliceMenuPanel);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.SystemColors.Window;
			this.Name = "TLSliceArea";
			this.Size = new System.Drawing.Size(307, 255);
			this.SliceMenuPanel.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Panel SliceMenuPanel;
		private System.Windows.Forms.Button MoveUpButton;
		private System.Windows.Forms.Button MoveDownButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button AddAboveButton;
		private System.Windows.Forms.Button AddBelowButton;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button RemoveSliceButton;
	}
}
