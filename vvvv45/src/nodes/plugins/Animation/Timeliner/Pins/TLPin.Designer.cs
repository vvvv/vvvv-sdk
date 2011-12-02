namespace VVVV.Nodes.Timeliner
{
	partial class TLPin
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
			this.AllInOne = new System.Windows.Forms.CheckBox();
			this.TopPanel = new System.Windows.Forms.Panel();
			this.CollapseButton = new System.Windows.Forms.Button();
			this.PinNameEdit = new System.Windows.Forms.TextBox();
			this.RemoveButton = new System.Windows.Forms.Button();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// AllInOne
			// 
			this.AllInOne.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AllInOne.Location = new System.Drawing.Point(5, 18);
			this.AllInOne.Name = "AllInOne";
			this.AllInOne.Size = new System.Drawing.Size(74, 24);
			this.AllInOne.TabIndex = 3;
			this.AllInOne.Text = "AllInOne";
			this.AllInOne.UseVisualStyleBackColor = true;
			this.AllInOne.Click += new System.EventHandler(this.AllInOneClick);
			// 
			// TopPanel
			// 
			this.TopPanel.BackColor = System.Drawing.Color.Silver;
			this.TopPanel.Controls.Add(this.CollapseButton);
			this.TopPanel.Controls.Add(this.PinNameEdit);
			this.TopPanel.Controls.Add(this.RemoveButton);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = new System.Drawing.Point(0, 0);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = new System.Drawing.Size(150, 20);
			this.TopPanel.TabIndex = 2;
			this.TopPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopPanelMouseMove);
			// 
			// CollapseButton
			// 
			this.CollapseButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.CollapseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.CollapseButton.Location = new System.Drawing.Point(20, 0);
			this.CollapseButton.Name = "CollapseButton";
			this.CollapseButton.Size = new System.Drawing.Size(20, 20);
			this.CollapseButton.TabIndex = 1;
			this.CollapseButton.Text = "V";
			this.CollapseButton.UseVisualStyleBackColor = true;
			this.CollapseButton.Click += new System.EventHandler(this.CollapseButtonClick);
			this.CollapseButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CollapseButtonMouseUp);
			// 
			// PinNameEdit
			// 
			this.PinNameEdit.BackColor = System.Drawing.Color.Silver;
			this.PinNameEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PinNameEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PinNameEdit.Location = new System.Drawing.Point(46, 4);
			this.PinNameEdit.Name = "PinNameEdit";
			this.PinNameEdit.Size = new System.Drawing.Size(101, 13);
			this.PinNameEdit.TabIndex = 2;
			this.PinNameEdit.Leave += new System.EventHandler(this.PinNameEditLeave);
			this.PinNameEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PinNameEditKeyPress);
			// 
			// RemoveButton
			// 
			this.RemoveButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.RemoveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RemoveButton.Location = new System.Drawing.Point(0, 0);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = new System.Drawing.Size(20, 20);
			this.RemoveButton.TabIndex = 0;
			this.RemoveButton.Text = "X";
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButtonClick);
			// 
			// TLPin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.AllInOne);
			this.Name = "TLPin";
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button CollapseButton;
		private System.Windows.Forms.Button RemoveButton;
		private System.Windows.Forms.TextBox PinNameEdit;
		
		private System.Windows.Forms.Panel TopPanel;
		private System.Windows.Forms.CheckBox AllInOne;
	}
}
