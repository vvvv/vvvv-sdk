/*
 * Created by SharpDevelop.
 * User: joreg
 * Date: 17.03.2009
 * Time: 01:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.Nodes
{
	partial class IPControl
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
			this.VNCButton = new System.Windows.Forms.Button();
			this.XButton = new System.Windows.Forms.Button();
			this.IPLabel = new System.Windows.Forms.Label();
			this.OnlinePanel = new System.Windows.Forms.Panel();
			this.AppPanel = new System.Windows.Forms.Panel();
			this.EXPButton = new System.Windows.Forms.Button();
			this.MacLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// VNCButton
			// 
			this.VNCButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.VNCButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VNCButton.Location = new System.Drawing.Point(32, 2);
			this.VNCButton.Name = "VNCButton";
			this.VNCButton.Size = new System.Drawing.Size(32, 15);
			this.VNCButton.TabIndex = 0;
			this.VNCButton.Text = "VNC";
			this.VNCButton.UseVisualStyleBackColor = true;
			this.VNCButton.Click += new System.EventHandler(this.VNCButtonClick);
			// 
			// XButton
			// 
			this.XButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.XButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.XButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.XButton.Location = new System.Drawing.Point(214, 0);
			this.XButton.Name = "XButton";
			this.XButton.Size = new System.Drawing.Size(20, 33);
			this.XButton.TabIndex = 1;
			this.XButton.Text = "X";
			this.XButton.UseVisualStyleBackColor = true;
			this.XButton.Click += new System.EventHandler(this.XButtonClick);
			// 
			// IPLabel
			// 
			this.IPLabel.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IPLabel.Location = new System.Drawing.Point(64, 2);
			this.IPLabel.Name = "IPLabel";
			this.IPLabel.Size = new System.Drawing.Size(144, 15);
			this.IPLabel.TabIndex = 2;
			this.IPLabel.Text = "192.168.0.0";
			this.IPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.IPLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseMove);
			this.IPLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseDown);
			this.IPLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseUp);
			// 
			// OnlinePanel
			// 
			this.OnlinePanel.BackColor = System.Drawing.Color.DarkRed;
			this.OnlinePanel.Location = new System.Drawing.Point(0, 2);
			this.OnlinePanel.Name = "OnlinePanel";
			this.OnlinePanel.Size = new System.Drawing.Size(15, 30);
			this.OnlinePanel.TabIndex = 3;
			// 
			// AppPanel
			// 
			this.AppPanel.BackColor = System.Drawing.Color.DarkRed;
			this.AppPanel.Location = new System.Drawing.Point(16, 2);
			this.AppPanel.Name = "AppPanel";
			this.AppPanel.Size = new System.Drawing.Size(15, 30);
			this.AppPanel.TabIndex = 4;
			// 
			// EXPButton
			// 
			this.EXPButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EXPButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EXPButton.Location = new System.Drawing.Point(32, 17);
			this.EXPButton.Name = "EXPButton";
			this.EXPButton.Size = new System.Drawing.Size(32, 15);
			this.EXPButton.TabIndex = 5;
			this.EXPButton.Text = "EXP";
			this.EXPButton.UseVisualStyleBackColor = true;
			this.EXPButton.Click += new System.EventHandler(this.EXPButtonClick);
			// 
			// MacLabel
			// 
			this.MacLabel.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MacLabel.Location = new System.Drawing.Point(64, 17);
			this.MacLabel.Name = "MacLabel";
			this.MacLabel.Size = new System.Drawing.Size(144, 15);
			this.MacLabel.TabIndex = 6;
			this.MacLabel.Text = "MAC Address";
			this.MacLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.MacLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseMove);
			this.MacLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseDown);
			this.MacLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseUp);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Gray;
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(214, 1);
			this.panel1.TabIndex = 7;
			// 
			// IPControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.XButton);
			this.Controls.Add(this.MacLabel);
			this.Controls.Add(this.EXPButton);
			this.Controls.Add(this.AppPanel);
			this.Controls.Add(this.VNCButton);
			this.Controls.Add(this.OnlinePanel);
			this.Controls.Add(this.IPLabel);
			this.Name = "IPControl";
			this.Size = new System.Drawing.Size(234, 33);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label MacLabel;
		private System.Windows.Forms.Button XButton;
		private System.Windows.Forms.Button EXPButton;
		private System.Windows.Forms.Panel AppPanel;
		private System.Windows.Forms.Panel OnlinePanel;
		private System.Windows.Forms.Label IPLabel;
		private System.Windows.Forms.Button VNCButton;
	}
}
