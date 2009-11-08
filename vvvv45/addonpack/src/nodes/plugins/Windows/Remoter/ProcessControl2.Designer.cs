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
	partial class ProcessControl2
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
			this.KillButton = new System.Windows.Forms.Button();
			this.RestartButton = new System.Windows.Forms.Button();
			this.StartButton = new System.Windows.Forms.Button();
			this.WatchCheckBox = new System.Windows.Forms.CheckBox();
			this.NameLabel = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.ProcessArgumentsEdit = new System.Windows.Forms.TextBox();
			this.ProcessPathEdit = new System.Windows.Forms.TextBox();
			this.ProcessNameEdit = new System.Windows.Forms.TextBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.WatchReboot = new System.Windows.Forms.RadioButton();
			this.WatchRestart = new System.Windows.Forms.RadioButton();
			this.WatchDoNothing = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.OKButton = new System.Windows.Forms.Button();
			this.CancelButton = new System.Windows.Forms.Button();
			this.WriteToBatButton = new System.Windows.Forms.Button();
			this.panel4 = new System.Windows.Forms.Panel();
			this.ShowSelectionButton = new System.Windows.Forms.Button();
			this.TakeSelectionButton = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.NameLabel);
			this.panel1.Controls.Add(this.StartButton);
			this.panel1.Controls.Add(this.RestartButton);
			this.panel1.Controls.Add(this.KillButton);
			this.panel1.Controls.Add(this.WatchCheckBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(380, 30);
			this.panel1.TabIndex = 14;
			// 
			// KillButton
			// 
			this.KillButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.KillButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KillButton.Location = new System.Drawing.Point(263, 0);
			this.KillButton.Name = "KillButton";
			this.KillButton.Size = new System.Drawing.Size(50, 30);
			this.KillButton.TabIndex = 9;
			this.KillButton.Text = "Kill";
			this.KillButton.UseVisualStyleBackColor = true;
			// 
			// RestartButton
			// 
			this.RestartButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RestartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RestartButton.Location = new System.Drawing.Point(213, 0);
			this.RestartButton.Name = "RestartButton";
			this.RestartButton.Size = new System.Drawing.Size(50, 30);
			this.RestartButton.TabIndex = 8;
			this.RestartButton.Text = "Restart";
			this.RestartButton.UseVisualStyleBackColor = true;
			this.RestartButton.Click += new System.EventHandler(this.RestartButtonClick);
			// 
			// StartButton
			// 
			this.StartButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.StartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StartButton.Location = new System.Drawing.Point(163, 0);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(50, 30);
			this.StartButton.TabIndex = 7;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButtonClick);
			// 
			// WatchCheckBox
			// 
			this.WatchCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.WatchCheckBox.Location = new System.Drawing.Point(313, 0);
			this.WatchCheckBox.Name = "WatchCheckBox";
			this.WatchCheckBox.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.WatchCheckBox.Size = new System.Drawing.Size(67, 30);
			this.WatchCheckBox.TabIndex = 5;
			this.WatchCheckBox.Text = "Watch";
			this.WatchCheckBox.UseVisualStyleBackColor = true;
			// 
			// NameLabel
			// 
			this.NameLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.NameLabel.Location = new System.Drawing.Point(0, 0);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Size = new System.Drawing.Size(161, 30);
			this.NameLabel.TabIndex = 6;
			this.NameLabel.Text = "Process Name (SelectionCount)";
			this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.ProcessArgumentsEdit);
			this.panel2.Controls.Add(this.ProcessPathEdit);
			this.panel2.Controls.Add(this.ProcessNameEdit);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 30);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(380, 62);
			this.panel2.TabIndex = 15;
			// 
			// ProcessArgumentsEdit
			// 
			this.ProcessArgumentsEdit.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProcessArgumentsEdit.Location = new System.Drawing.Point(0, 40);
			this.ProcessArgumentsEdit.Name = "ProcessArgumentsEdit";
			this.ProcessArgumentsEdit.Size = new System.Drawing.Size(380, 20);
			this.ProcessArgumentsEdit.TabIndex = 16;
			this.ProcessArgumentsEdit.Text = "Arguments";
			// 
			// ProcessPathEdit
			// 
			this.ProcessPathEdit.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProcessPathEdit.Location = new System.Drawing.Point(0, 20);
			this.ProcessPathEdit.Name = "ProcessPathEdit";
			this.ProcessPathEdit.Size = new System.Drawing.Size(380, 20);
			this.ProcessPathEdit.TabIndex = 15;
			this.ProcessPathEdit.Text = "Path";
			// 
			// ProcessNameEdit
			// 
			this.ProcessNameEdit.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProcessNameEdit.Location = new System.Drawing.Point(0, 0);
			this.ProcessNameEdit.Name = "ProcessNameEdit";
			this.ProcessNameEdit.Size = new System.Drawing.Size(380, 20);
			this.ProcessNameEdit.TabIndex = 14;
			this.ProcessNameEdit.Text = "Name";
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.WatchReboot);
			this.panel3.Controls.Add(this.WatchRestart);
			this.panel3.Controls.Add(this.WatchDoNothing);
			this.panel3.Controls.Add(this.label1);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 118);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(380, 25);
			this.panel3.TabIndex = 16;
			// 
			// WatchReboot
			// 
			this.WatchReboot.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchReboot.Location = new System.Drawing.Point(287, 0);
			this.WatchReboot.Name = "WatchReboot";
			this.WatchReboot.Size = new System.Drawing.Size(88, 25);
			this.WatchReboot.TabIndex = 17;
			this.WatchReboot.Text = "reboot PC";
			this.WatchReboot.UseVisualStyleBackColor = true;
			// 
			// WatchRestart
			// 
			this.WatchRestart.Checked = true;
			this.WatchRestart.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchRestart.Location = new System.Drawing.Point(201, 0);
			this.WatchRestart.Name = "WatchRestart";
			this.WatchRestart.Size = new System.Drawing.Size(86, 25);
			this.WatchRestart.TabIndex = 16;
			this.WatchRestart.TabStop = true;
			this.WatchRestart.Text = "restart";
			this.WatchRestart.UseVisualStyleBackColor = true;
			// 
			// WatchDoNothing
			// 
			this.WatchDoNothing.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchDoNothing.Location = new System.Drawing.Point(111, 0);
			this.WatchDoNothing.Name = "WatchDoNothing";
			this.WatchDoNothing.Size = new System.Drawing.Size(90, 25);
			this.WatchDoNothing.TabIndex = 15;
			this.WatchDoNothing.Text = "nothing";
			this.WatchDoNothing.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Left;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(111, 25);
			this.label1.TabIndex = 18;
			this.label1.Text = "On lost process do:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// OKButton
			// 
			this.OKButton.Location = new System.Drawing.Point(219, 146);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(75, 23);
			this.OKButton.TabIndex = 17;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// CancelButton
			// 
			this.CancelButton.Location = new System.Drawing.Point(300, 146);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = new System.Drawing.Size(75, 23);
			this.CancelButton.TabIndex = 18;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// WriteToBatButton
			// 
			this.WriteToBatButton.Location = new System.Drawing.Point(3, 146);
			this.WriteToBatButton.Name = "WriteToBatButton";
			this.WriteToBatButton.Size = new System.Drawing.Size(187, 23);
			this.WriteToBatButton.TabIndex = 19;
			this.WriteToBatButton.Text = "Write .bat file";
			this.WriteToBatButton.UseVisualStyleBackColor = true;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.ShowSelectionButton);
			this.panel4.Controls.Add(this.TakeSelectionButton);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 92);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(380, 26);
			this.panel4.TabIndex = 20;
			// 
			// ShowSelectionButton
			// 
			this.ShowSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShowSelectionButton.Location = new System.Drawing.Point(190, 0);
			this.ShowSelectionButton.Name = "ShowSelectionButton";
			this.ShowSelectionButton.Size = new System.Drawing.Size(190, 26);
			this.ShowSelectionButton.TabIndex = 11;
			this.ShowSelectionButton.Text = "Show Selection";
			this.ShowSelectionButton.UseVisualStyleBackColor = true;
			// 
			// TakeSelectionButton
			// 
			this.TakeSelectionButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.TakeSelectionButton.Location = new System.Drawing.Point(0, 0);
			this.TakeSelectionButton.Name = "TakeSelectionButton";
			this.TakeSelectionButton.Size = new System.Drawing.Size(190, 26);
			this.TakeSelectionButton.TabIndex = 10;
			this.TakeSelectionButton.Text = "Take Selection";
			this.TakeSelectionButton.UseVisualStyleBackColor = true;
			// 
			// ProcessControl2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.WriteToBatButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Name = "ProcessControl2";
			this.Size = new System.Drawing.Size(380, 173);
			this.Load += new System.EventHandler(this.ProcessControl2Load);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Button CancelButton;
		private System.Windows.Forms.Button OKButton;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label NameLabel;
		private System.Windows.Forms.CheckBox WatchCheckBox;
		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.Button RestartButton;
		private System.Windows.Forms.Button KillButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox ProcessArgumentsEdit;
		private System.Windows.Forms.TextBox ProcessPathEdit;
		private System.Windows.Forms.RadioButton WatchReboot;
		private System.Windows.Forms.RadioButton WatchRestart;
		private System.Windows.Forms.RadioButton WatchDoNothing;
		private System.Windows.Forms.Button ShowSelectionButton;
		private System.Windows.Forms.TextBox ProcessNameEdit;
		private System.Windows.Forms.Button TakeSelectionButton;
		private System.Windows.Forms.Button WriteToBatButton;
	}
}
