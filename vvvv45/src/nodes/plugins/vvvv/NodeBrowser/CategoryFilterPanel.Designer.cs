/*
 * Created by SharpDevelop.
 * User: joreg
 * Date: 05.03.2013
 * Time: 20:54
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace VVVV.Nodes.NodeBrowser
{
	partial class CategoryFilterPanel
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
			this.CheckboxPanel = new System.Windows.Forms.Panel();
			this.FHiddenCategoryCountLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(5, 245);
			this.panel1.TabIndex = 0;
			// 
			// CheckboxPanel
			// 
			this.CheckboxPanel.AutoScroll = true;
			this.CheckboxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CheckboxPanel.Location = new System.Drawing.Point(5, 0);
			this.CheckboxPanel.Name = "CheckboxPanel";
			this.CheckboxPanel.Size = new System.Drawing.Size(260, 245);
			this.CheckboxPanel.TabIndex = 1;
			// 
			// FHiddenCategoryCountLabel
			// 
			this.FHiddenCategoryCountLabel.BackColor = System.Drawing.Color.Silver;
			this.FHiddenCategoryCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FHiddenCategoryCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FHiddenCategoryCountLabel.Location = new System.Drawing.Point(0, 245);
			this.FHiddenCategoryCountLabel.Name = "FHiddenCategoryCountLabel";
			this.FHiddenCategoryCountLabel.Size = new System.Drawing.Size(265, 15);
			this.FHiddenCategoryCountLabel.TabIndex = 8;
			// 
			// CategoryFilterPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.Silver;
			this.Controls.Add(this.CheckboxPanel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.FHiddenCategoryCountLabel);
			this.Name = "CategoryFilterPanel";
			this.Size = new System.Drawing.Size(265, 260);
			this.VisibleChanged += new System.EventHandler(this.CategoryFilterPanelVisibleChanged);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label FHiddenCategoryCountLabel;
		private System.Windows.Forms.Panel CheckboxPanel;
		private System.Windows.Forms.Panel panel1;
	}
}
