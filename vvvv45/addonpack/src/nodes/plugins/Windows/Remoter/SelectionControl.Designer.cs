/*
 * Created by SharpDevelop.
 * User: joreg
 * Date: 09.11.2009
 * Time: 13:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.Nodes
{
	partial class SelectionControl
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
			this.NameLabel = new System.Windows.Forms.Label();
			this.TakeSelectionButton = new System.Windows.Forms.Button();
			this.ShowSelectionButton = new System.Windows.Forms.Button();
			this.EditButton = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.NameEdit = new System.Windows.Forms.TextBox();
			this.DeleteButton = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.NameLabel);
			this.panel1.Controls.Add(this.TakeSelectionButton);
			this.panel1.Controls.Add(this.ShowSelectionButton);
			this.panel1.Controls.Add(this.EditButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(415, 25);
			this.panel1.TabIndex = 16;
			// 
			// NameLabel
			// 
			this.NameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NameLabel.Location = new System.Drawing.Point(0, 0);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.NameLabel.Size = new System.Drawing.Size(238, 25);
			this.NameLabel.TabIndex = 18;
			this.NameLabel.Text = "Name (Count)";
			this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TakeSelectionButton
			// 
			this.TakeSelectionButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.TakeSelectionButton.Location = new System.Drawing.Point(238, 0);
			this.TakeSelectionButton.Name = "TakeSelectionButton";
			this.TakeSelectionButton.Size = new System.Drawing.Size(80, 25);
			this.TakeSelectionButton.TabIndex = 16;
			this.TakeSelectionButton.Text = "Take";
			this.TakeSelectionButton.UseVisualStyleBackColor = true;
			this.TakeSelectionButton.Click += new System.EventHandler(this.TakeSelectionButtonClick);
			// 
			// ShowSelectionButton
			// 
			this.ShowSelectionButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.ShowSelectionButton.Location = new System.Drawing.Point(318, 0);
			this.ShowSelectionButton.Name = "ShowSelectionButton";
			this.ShowSelectionButton.Size = new System.Drawing.Size(80, 25);
			this.ShowSelectionButton.TabIndex = 17;
			this.ShowSelectionButton.Text = "Show";
			this.ShowSelectionButton.UseVisualStyleBackColor = true;
			this.ShowSelectionButton.Click += new System.EventHandler(this.ShowSelectionButtonClick);
			// 
			// EditButton
			// 
			this.EditButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.EditButton.Location = new System.Drawing.Point(398, 0);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = new System.Drawing.Size(17, 25);
			this.EditButton.TabIndex = 19;
			this.EditButton.Text = "E";
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButtonClick);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.NameEdit);
			this.panel2.Controls.Add(this.DeleteButton);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 25);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(415, 21);
			this.panel2.TabIndex = 18;
			// 
			// NameEdit
			// 
			this.NameEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NameEdit.Location = new System.Drawing.Point(0, 0);
			this.NameEdit.Name = "NameEdit";
			this.NameEdit.Size = new System.Drawing.Size(398, 20);
			this.NameEdit.TabIndex = 18;
			// 
			// DeleteButton
			// 
			this.DeleteButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.DeleteButton.Location = new System.Drawing.Point(398, 0);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = new System.Drawing.Size(17, 21);
			this.DeleteButton.TabIndex = 20;
			this.DeleteButton.Text = "X";
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
			// 
			// SelectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Name = "SelectionControl";
			this.Size = new System.Drawing.Size(415, 73);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button DeleteButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TextBox NameEdit;
		private System.Windows.Forms.Button EditButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label NameLabel;
		private System.Windows.Forms.Button TakeSelectionButton;
		private System.Windows.Forms.Button ShowSelectionButton;
	}
}
