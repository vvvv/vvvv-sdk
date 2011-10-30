/*
 * Erstellt mit SharpDevelop.
 * Benutzer: joreg
 * Datum: 06.11.2009
 * Zeit: 22:21
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
namespace VVVV.Nodes
{
	partial class TaskControl
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
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.StartButton = new System.Windows.Forms.Button();
			this.RestartButton = new System.Windows.Forms.Button();
			this.KillButton = new System.Windows.Forms.Button();
			this.WatchCheckBox = new System.Windows.Forms.CheckBox();
			this.EditButton = new System.Windows.Forms.Button();
			this.panel3 = new System.Windows.Forms.Panel();
			this.WatchReboot = new System.Windows.Forms.RadioButton();
			this.WatchRestart = new System.Windows.Forms.RadioButton();
			this.WatchDoNothing = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.GroupBox = new System.Windows.Forms.ComboBox();
			this.ProcessBox = new System.Windows.Forms.ComboBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.DescriptionEdit = new System.Windows.Forms.TextBox();
			this.DeleteButton = new System.Windows.Forms.Button();
			this.panel4 = new System.Windows.Forms.Panel();
			this.TimeOutUpDown = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TimeOutUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.DescriptionLabel);
			this.panel1.Controls.Add(this.StartButton);
			this.panel1.Controls.Add(this.RestartButton);
			this.panel1.Controls.Add(this.KillButton);
			this.panel1.Controls.Add(this.WatchCheckBox);
			this.panel1.Controls.Add(this.EditButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(373, 25);
			this.panel1.TabIndex = 14;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DescriptionLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DescriptionLabel.Location = new System.Drawing.Point(0, 0);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = new System.Drawing.Size(139, 25);
			this.DescriptionLabel.TabIndex = 6;
			this.DescriptionLabel.Text = "Description";
			this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// StartButton
			// 
			this.StartButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.StartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StartButton.Location = new System.Drawing.Point(139, 0);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(50, 25);
			this.StartButton.TabIndex = 7;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButtonClick);
			// 
			// RestartButton
			// 
			this.RestartButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RestartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RestartButton.Location = new System.Drawing.Point(189, 0);
			this.RestartButton.Name = "RestartButton";
			this.RestartButton.Size = new System.Drawing.Size(50, 25);
			this.RestartButton.TabIndex = 8;
			this.RestartButton.Text = "Restart";
			this.RestartButton.UseVisualStyleBackColor = true;
			this.RestartButton.Click += new System.EventHandler(this.RestartButtonClick);
			// 
			// KillButton
			// 
			this.KillButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.KillButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KillButton.Location = new System.Drawing.Point(239, 0);
			this.KillButton.Name = "KillButton";
			this.KillButton.Size = new System.Drawing.Size(50, 25);
			this.KillButton.TabIndex = 9;
			this.KillButton.Text = "Kill";
			this.KillButton.UseVisualStyleBackColor = true;
			this.KillButton.Click += new System.EventHandler(this.KillButtonClick);
			// 
			// WatchCheckBox
			// 
			this.WatchCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.WatchCheckBox.Location = new System.Drawing.Point(289, 0);
			this.WatchCheckBox.Name = "WatchCheckBox";
			this.WatchCheckBox.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.WatchCheckBox.Size = new System.Drawing.Size(67, 25);
			this.WatchCheckBox.TabIndex = 5;
			this.WatchCheckBox.Text = "Watch";
			this.WatchCheckBox.UseVisualStyleBackColor = true;
			this.WatchCheckBox.Click += new System.EventHandler(this.WatchCheckBoxClick);
			// 
			// EditButton
			// 
			this.EditButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.EditButton.Location = new System.Drawing.Point(356, 0);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = new System.Drawing.Size(17, 25);
			this.EditButton.TabIndex = 10;
			this.EditButton.Text = "E";
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButtonClick);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.WatchReboot);
			this.panel3.Controls.Add(this.WatchRestart);
			this.panel3.Controls.Add(this.WatchDoNothing);
			this.panel3.Controls.Add(this.label1);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 107);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(373, 18);
			this.panel3.TabIndex = 16;
			// 
			// WatchReboot
			// 
			this.WatchReboot.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchReboot.Location = new System.Drawing.Point(232, 0);
			this.WatchReboot.Name = "WatchReboot";
			this.WatchReboot.Size = new System.Drawing.Size(88, 18);
			this.WatchReboot.TabIndex = 17;
			this.WatchReboot.Text = "reboot PC";
			this.WatchReboot.UseVisualStyleBackColor = true;
			// 
			// WatchRestart
			// 
			this.WatchRestart.Checked = true;
			this.WatchRestart.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchRestart.Location = new System.Drawing.Point(178, 0);
			this.WatchRestart.Name = "WatchRestart";
			this.WatchRestart.Size = new System.Drawing.Size(54, 18);
			this.WatchRestart.TabIndex = 16;
			this.WatchRestart.TabStop = true;
			this.WatchRestart.Text = "restart";
			this.WatchRestart.UseVisualStyleBackColor = true;
			// 
			// WatchDoNothing
			// 
			this.WatchDoNothing.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchDoNothing.Location = new System.Drawing.Point(116, 0);
			this.WatchDoNothing.Name = "WatchDoNothing";
			this.WatchDoNothing.Size = new System.Drawing.Size(62, 18);
			this.WatchDoNothing.TabIndex = 15;
			this.WatchDoNothing.Text = "nothing";
			this.WatchDoNothing.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Left;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(116, 18);
			this.label1.TabIndex = 18;
			this.label1.Text = "On lost process do:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// GroupBox
			// 
			this.GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.GroupBox.Location = new System.Drawing.Point(0, 45);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = new System.Drawing.Size(373, 21);
			this.GroupBox.TabIndex = 21;
			this.GroupBox.SelectedIndexChanged += new System.EventHandler(this.GroupBoxSelectedIndexChanged);
			// 
			// ProcessBox
			// 
			this.ProcessBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProcessBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ProcessBox.Location = new System.Drawing.Point(0, 66);
			this.ProcessBox.Name = "ProcessBox";
			this.ProcessBox.Size = new System.Drawing.Size(373, 21);
			this.ProcessBox.TabIndex = 22;
			this.ProcessBox.SelectedIndexChanged += new System.EventHandler(this.ProcessBoxSelectedIndexChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.DescriptionEdit);
			this.panel2.Controls.Add(this.DeleteButton);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 25);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(373, 20);
			this.panel2.TabIndex = 23;
			// 
			// DescriptionEdit
			// 
			this.DescriptionEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionEdit.Location = new System.Drawing.Point(0, 0);
			this.DescriptionEdit.Name = "DescriptionEdit";
			this.DescriptionEdit.Size = new System.Drawing.Size(356, 20);
			this.DescriptionEdit.TabIndex = 21;
			// 
			// DeleteButton
			// 
			this.DeleteButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.DeleteButton.Location = new System.Drawing.Point(356, 0);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = new System.Drawing.Size(17, 20);
			this.DeleteButton.TabIndex = 22;
			this.DeleteButton.Text = "X";
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.TimeOutUpDown);
			this.panel4.Controls.Add(this.label2);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 87);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(373, 20);
			this.panel4.TabIndex = 24;
			// 
			// TimeOutUpDown
			// 
			this.TimeOutUpDown.Dock = System.Windows.Forms.DockStyle.Left;
			this.TimeOutUpDown.Location = new System.Drawing.Point(65, 0);
			this.TimeOutUpDown.Maximum = new decimal(new int[] {
									999999999,
									0,
									0,
									0});
			this.TimeOutUpDown.Name = "TimeOutUpDown";
			this.TimeOutUpDown.Size = new System.Drawing.Size(51, 20);
			this.TimeOutUpDown.TabIndex = 1;
			this.TimeOutUpDown.Value = new decimal(new int[] {
									10,
									0,
									0,
									0});
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Left;
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 20);
			this.label2.TabIndex = 0;
			this.label2.Text = "Timeout (s):";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TaskControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.ProcessBox);
			this.Controls.Add(this.GroupBox);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "TaskControl";
			this.Size = new System.Drawing.Size(373, 123);
			this.panel1.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TimeOutUpDown)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown TimeOutUpDown;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label DescriptionLabel;
		private System.Windows.Forms.ComboBox GroupBox;
		private System.Windows.Forms.TextBox DescriptionEdit;
		private System.Windows.Forms.Button DeleteButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button EditButton;
		private System.Windows.Forms.ComboBox ProcessBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.CheckBox WatchCheckBox;
		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.Button RestartButton;
		private System.Windows.Forms.Button KillButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton WatchReboot;
		private System.Windows.Forms.RadioButton WatchRestart;
		private System.Windows.Forms.RadioButton WatchDoNothing;
	}
}
