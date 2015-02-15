using System;
using System.Windows.Forms;

namespace VVVV.HDE.ProjectExplorer
{
	partial class ProjectExplorerPlugin
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
                Solution.Projects.Added -= Projects_Added;
		        FHideUnusedProjectsIn.Changed -= FHideUnusedProjectsIn_Changed;
				FBuildConfigIn.Changed -= FBuildConfigIn_Changed;
				FHideUnusedProjectsCheckBox.CheckedChanged -= FHideUnusedProjectsCheckBox_CheckedChanged;
				FTreeViewer.DoubleClick -= FTreeViewer_DoubleClick;
		        
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.FHideUnusedProjectsCheckBox = new System.Windows.Forms.CheckBox();
			this.FBuildConfigComboBox = new System.Windows.Forms.ComboBox();
			this.FTreeViewer = new VVVV.HDE.Viewer.WinFormsViewer.TreeViewer();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.DarkGray;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.FHideUnusedProjectsCheckBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.FBuildConfigComboBox, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(422, 21);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// FHideUnusedProjectsCheckBox
			// 
			this.FHideUnusedProjectsCheckBox.AutoSize = true;
			this.FHideUnusedProjectsCheckBox.BackColor = System.Drawing.Color.DarkGray;
			this.FHideUnusedProjectsCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.FHideUnusedProjectsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.FHideUnusedProjectsCheckBox.ForeColor = System.Drawing.Color.White;
			this.FHideUnusedProjectsCheckBox.Location = new System.Drawing.Point(0, 0);
			this.FHideUnusedProjectsCheckBox.Margin = new System.Windows.Forms.Padding(0);
			this.FHideUnusedProjectsCheckBox.Name = "FHideUnusedProjectsCheckBox";
			this.FHideUnusedProjectsCheckBox.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.FHideUnusedProjectsCheckBox.Size = new System.Drawing.Size(126, 21);
			this.FHideUnusedProjectsCheckBox.TabIndex = 0;
			this.FHideUnusedProjectsCheckBox.Text = "Hide unused projects";
			this.FHideUnusedProjectsCheckBox.UseVisualStyleBackColor = false;
			// 
			// FBuildConfigComboBox
			// 
			this.FBuildConfigComboBox.BackColor = System.Drawing.Color.DarkGray;
			this.FBuildConfigComboBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.FBuildConfigComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FBuildConfigComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.FBuildConfigComboBox.ForeColor = System.Drawing.Color.White;
			this.FBuildConfigComboBox.FormattingEnabled = true;
			this.FBuildConfigComboBox.Location = new System.Drawing.Point(289, 0);
			this.FBuildConfigComboBox.Margin = new System.Windows.Forms.Padding(0);
			this.FBuildConfigComboBox.Name = "FBuildConfigComboBox";
			this.FBuildConfigComboBox.Size = new System.Drawing.Size(133, 21);
			this.FBuildConfigComboBox.TabIndex = 2;
			this.FBuildConfigComboBox.SelectedIndexChanged += new System.EventHandler(this.FBuildConfigComboBox_SelectedIndexChanged);
			// 
			// FTreeViewer
			// 
			this.FTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTreeViewer.FlatStyle = false;
			this.FTreeViewer.Location = new System.Drawing.Point(0, 21);
			this.FTreeViewer.Name = "FTreeViewer";
			this.FTreeViewer.ShowLines = true;
			this.FTreeViewer.ShowPlusMinus = true;
			this.FTreeViewer.ShowRoot = false;
			this.FTreeViewer.ShowRootLines = true;
			this.FTreeViewer.ShowTooltip = true;
			this.FTreeViewer.Size = new System.Drawing.Size(422, 309);
			this.FTreeViewer.TabIndex = 1;
			// 
			// ProjectExplorerPlugin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FTreeViewer);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "ProjectExplorerPlugin";
			this.Size = new System.Drawing.Size(422, 330);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private VVVV.HDE.Viewer.WinFormsViewer.TreeViewer FTreeViewer;
		private System.Windows.Forms.CheckBox FHideUnusedProjectsCheckBox;
		private System.Windows.Forms.ComboBox FBuildConfigComboBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}
