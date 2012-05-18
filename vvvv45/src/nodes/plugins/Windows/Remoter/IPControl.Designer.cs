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
			this.XButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.EXPButton = new System.Windows.Forms.Button();
			this.AppPanel = new System.Windows.Forms.Panel();
			this.VNCButton = new System.Windows.Forms.Button();
			this.OnlinePanel = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.MacIPLabel = new System.Windows.Forms.Label();
			this.HostNameLabel = new System.Windows.Forms.Label();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// XButton
			// 
			this.XButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.XButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.XButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.XButton.Location = new System.Drawing.Point(356, 0);
			this.XButton.Name = "XButton";
			this.XButton.Size = new System.Drawing.Size(20, 32);
			this.XButton.TabIndex = 1;
			this.XButton.Text = "X";
			this.XButton.UseVisualStyleBackColor = true;
			this.XButton.Click += new System.EventHandler(this.XButtonClick);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Gray;
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(356, 1);
			this.panel1.TabIndex = 7;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.EXPButton);
			this.panel2.Controls.Add(this.AppPanel);
			this.panel2.Controls.Add(this.VNCButton);
			this.panel2.Controls.Add(this.OnlinePanel);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(0, 1);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(65, 31);
			this.panel2.TabIndex = 8;
			// 
			// EXPButton
			// 
			this.EXPButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EXPButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EXPButton.Location = new System.Drawing.Point(32, 15);
			this.EXPButton.Name = "EXPButton";
			this.EXPButton.Size = new System.Drawing.Size(32, 15);
			this.EXPButton.TabIndex = 9;
			this.EXPButton.Text = "EXP";
			this.EXPButton.UseVisualStyleBackColor = true;
			this.EXPButton.Click += new System.EventHandler(this.EXPButtonClick);
			// 
			// AppPanel
			// 
			this.AppPanel.BackColor = System.Drawing.Color.DarkRed;
			this.AppPanel.Location = new System.Drawing.Point(16, 0);
			this.AppPanel.Name = "AppPanel";
			this.AppPanel.Size = new System.Drawing.Size(15, 30);
			this.AppPanel.TabIndex = 8;
			// 
			// VNCButton
			// 
			this.VNCButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.VNCButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VNCButton.Location = new System.Drawing.Point(32, 0);
			this.VNCButton.Name = "VNCButton";
			this.VNCButton.Size = new System.Drawing.Size(32, 15);
			this.VNCButton.TabIndex = 6;
			this.VNCButton.Text = "VNC";
			this.VNCButton.UseVisualStyleBackColor = true;
			this.VNCButton.Click += new System.EventHandler(this.VNCButtonClick);
			// 
			// OnlinePanel
			// 
			this.OnlinePanel.BackColor = System.Drawing.Color.DarkRed;
			this.OnlinePanel.Location = new System.Drawing.Point(0, 0);
			this.OnlinePanel.Name = "OnlinePanel";
			this.OnlinePanel.Size = new System.Drawing.Size(15, 30);
			this.OnlinePanel.TabIndex = 7;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.MacIPLabel);
			this.panel3.Controls.Add(this.HostNameLabel);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(65, 1);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(291, 31);
			this.panel3.TabIndex = 9;
			// 
			// MacIPLabel
			// 
			this.MacIPLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MacIPLabel.Font = new System.Drawing.Font("Lucida Console", 6F);
			this.MacIPLabel.Location = new System.Drawing.Point(0, 18);
			this.MacIPLabel.Name = "MacIPLabel";
			this.MacIPLabel.Size = new System.Drawing.Size(291, 13);
			this.MacIPLabel.TabIndex = 7;
			this.MacIPLabel.Text = "MAC Address";
			this.MacIPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.MacIPLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseMove);
			this.MacIPLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseDown);
			this.MacIPLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseUp);
			// 
			// HostNameLabel
			// 
			this.HostNameLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HostNameLabel.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HostNameLabel.Location = new System.Drawing.Point(0, 0);
			this.HostNameLabel.Name = "HostNameLabel";
			this.HostNameLabel.Size = new System.Drawing.Size(291, 18);
			this.HostNameLabel.TabIndex = 3;
			this.HostNameLabel.Text = "HostName";
			this.HostNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.HostNameLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseMove);
			this.HostNameLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseDown);
			this.HostNameLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IPLabelMouseUp);
			// 
			// IPControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.XButton);
			this.Name = "IPControl";
			this.Size = new System.Drawing.Size(376, 32);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label HostNameLabel;
		private System.Windows.Forms.Label MacIPLabel;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button XButton;
		private System.Windows.Forms.Button EXPButton;
		private System.Windows.Forms.Panel AppPanel;
		private System.Windows.Forms.Panel OnlinePanel;
		private System.Windows.Forms.Button VNCButton;
	}
}
