/*
 * Created by SharpDevelop.
 * User: joreg
 * Date: 29.08.2009
 * Time: 21:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.Nodes
{
	partial class ProcessControl
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
			this.RemoveButton = new System.Windows.Forms.Button();
			this.ProcessEdit = new System.Windows.Forms.TextBox();
			this.ArgumentsEdit = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// RemoveButton
			// 
			this.RemoveButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.RemoveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RemoveButton.Location = new System.Drawing.Point(0, 0);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = new System.Drawing.Size(25, 20);
			this.RemoveButton.TabIndex = 2;
			this.RemoveButton.Text = "X";
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButtonClick);
			// 
			// ProcessEdit
			// 
			this.ProcessEdit.Dock = System.Windows.Forms.DockStyle.Left;
			this.ProcessEdit.Location = new System.Drawing.Point(25, 0);
			this.ProcessEdit.Name = "ProcessEdit";
			this.ProcessEdit.Size = new System.Drawing.Size(136, 20);
			this.ProcessEdit.TabIndex = 0;
			this.ProcessEdit.TextChanged += new System.EventHandler(this.ProcessEditTextChanged);
			// 
			// ArgumentsEdit
			// 
			this.ArgumentsEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArgumentsEdit.Location = new System.Drawing.Point(161, 0);
			this.ArgumentsEdit.Name = "ArgumentsEdit";
			this.ArgumentsEdit.Size = new System.Drawing.Size(248, 20);
			this.ArgumentsEdit.TabIndex = 1;
			this.ArgumentsEdit.TextChanged += new System.EventHandler(this.ArgumentsEditTextChanged);
			// 
			// ProcessControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ArgumentsEdit);
			this.Controls.Add(this.ProcessEdit);
			this.Controls.Add(this.RemoveButton);
			this.Name = "ProcessControl";
			this.Size = new System.Drawing.Size(409, 20);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TextBox ArgumentsEdit;
		private System.Windows.Forms.TextBox ProcessEdit;
		private System.Windows.Forms.Button RemoveButton;
	}
}
