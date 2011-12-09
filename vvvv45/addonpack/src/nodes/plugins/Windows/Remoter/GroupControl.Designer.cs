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
	partial class GroupControl
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.AppPanel = new System.Windows.Forms.Panel();
			this.OnlinePanel = new System.Windows.Forms.Panel();
			this.IPsEdit = new System.Windows.Forms.TextBox();
			this.TopPanel = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.GroupLabel = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.EditButton = new System.Windows.Forms.Button();
			this.NamePanel = new System.Windows.Forms.Panel();
			this.GroupNameEdit = new System.Windows.Forms.TextBox();
			this.XButton = new System.Windows.Forms.Button();
			this.TopPanel.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.NamePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Gray;
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(238, 1);
			this.panel1.TabIndex = 7;
			// 
			// AppPanel
			// 
			this.AppPanel.BackColor = System.Drawing.Color.DarkRed;
			this.AppPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.AppPanel.Location = new System.Drawing.Point(16, 0);
			this.AppPanel.Name = "AppPanel";
			this.AppPanel.Size = new System.Drawing.Size(15, 32);
			this.AppPanel.TabIndex = 10;
			// 
			// OnlinePanel
			// 
			this.OnlinePanel.BackColor = System.Drawing.Color.DarkRed;
			this.OnlinePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.OnlinePanel.Location = new System.Drawing.Point(0, 0);
			this.OnlinePanel.Name = "OnlinePanel";
			this.OnlinePanel.Size = new System.Drawing.Size(15, 32);
			this.OnlinePanel.TabIndex = 9;
			// 
			// IPsEdit
			// 
			this.IPsEdit.AcceptsReturn = true;
			this.IPsEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IPsEdit.Location = new System.Drawing.Point(0, 53);
			this.IPsEdit.Multiline = true;
			this.IPsEdit.Name = "IPsEdit";
			this.IPsEdit.Size = new System.Drawing.Size(238, 87);
			this.IPsEdit.TabIndex = 10;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.panel3);
			this.TopPanel.Controls.Add(this.panel2);
			this.TopPanel.Controls.Add(this.EditButton);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = new System.Drawing.Point(0, 1);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = new System.Drawing.Size(238, 32);
			this.TopPanel.TabIndex = 11;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.GroupLabel);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(31, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(187, 32);
			this.panel3.TabIndex = 10;
			// 
			// GroupLabel
			// 
			this.GroupLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupLabel.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GroupLabel.Location = new System.Drawing.Point(0, 0);
			this.GroupLabel.Name = "GroupLabel";
			this.GroupLabel.Size = new System.Drawing.Size(187, 32);
			this.GroupLabel.TabIndex = 7;
			this.GroupLabel.Text = "ungrouped";
			this.GroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.GroupLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GroupLabelMouseMove);
			this.GroupLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GroupLabelMouseDown);
			this.GroupLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GroupLabelMouseUp);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.AppPanel);
			this.panel2.Controls.Add(this.OnlinePanel);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(31, 32);
			this.panel2.TabIndex = 9;
			// 
			// EditButton
			// 
			this.EditButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.EditButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EditButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditButton.Location = new System.Drawing.Point(218, 0);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = new System.Drawing.Size(20, 32);
			this.EditButton.TabIndex = 5;
			this.EditButton.Text = "E";
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButtonClick);
			// 
			// NamePanel
			// 
			this.NamePanel.Controls.Add(this.GroupNameEdit);
			this.NamePanel.Controls.Add(this.XButton);
			this.NamePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.NamePanel.Location = new System.Drawing.Point(0, 33);
			this.NamePanel.Name = "NamePanel";
			this.NamePanel.Size = new System.Drawing.Size(238, 20);
			this.NamePanel.TabIndex = 12;
			// 
			// GroupNameEdit
			// 
			this.GroupNameEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupNameEdit.Location = new System.Drawing.Point(0, 0);
			this.GroupNameEdit.Name = "GroupNameEdit";
			this.GroupNameEdit.Size = new System.Drawing.Size(218, 20);
			this.GroupNameEdit.TabIndex = 10;
			// 
			// XButton
			// 
			this.XButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.XButton.Location = new System.Drawing.Point(218, 0);
			this.XButton.Name = "XButton";
			this.XButton.Size = new System.Drawing.Size(20, 20);
			this.XButton.TabIndex = 11;
			this.XButton.Text = "X";
			this.XButton.UseVisualStyleBackColor = true;
			this.XButton.Click += new System.EventHandler(this.XButtonClick);
			// 
			// GroupControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.IPsEdit);
			this.Controls.Add(this.NamePanel);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.panel1);
			this.Name = "GroupControl";
			this.Size = new System.Drawing.Size(238, 140);
			this.TopPanel.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.NamePanel.ResumeLayout(false);
			this.NamePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel NamePanel;
		private System.Windows.Forms.Panel TopPanel;
		private System.Windows.Forms.TextBox IPsEdit;
		private System.Windows.Forms.TextBox GroupNameEdit;
		private System.Windows.Forms.Button EditButton;
		private System.Windows.Forms.Label GroupLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button XButton;
		private System.Windows.Forms.Panel AppPanel;
		private System.Windows.Forms.Panel OnlinePanel;
	}
}
