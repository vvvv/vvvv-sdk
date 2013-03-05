using System;

namespace Hoster
{
	partial class PluginPage: System.Windows.Forms.UserControl
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			
			DisposePanel();
						
			base.Dispose(disposing);
		}
			
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.SliceCountsPanel = new System.Windows.Forms.Panel();
			this.OSCPanel = new System.Windows.Forms.Panel();
			this.FrameRateIO = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.OSCMessageIO = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.TargetPortIO = new System.Windows.Forms.NumericUpDown();
			this.ReceivePortIO = new System.Windows.Forms.NumericUpDown();
			this.TargetHostIO = new System.Windows.Forms.TextBox();
			this.EnableOSCCheckBox = new System.Windows.Forms.CheckBox();
			this.InputsPanel = new Hoster.PinPanel();
			this.SplitPanelContainer = new System.Windows.Forms.SplitContainer();
			this.PluginPanel = new System.Windows.Forms.Panel();
			this.OutputsPanel = new Hoster.PinPanel();
			this.DebugLog = new System.Windows.Forms.ListBox();
			this.ClearButton = new System.Windows.Forms.Button();
			this.OSCPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FrameRateIO)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TargetPortIO)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReceivePortIO)).BeginInit();
			this.SplitPanelContainer.Panel1.SuspendLayout();
			this.SplitPanelContainer.Panel2.SuspendLayout();
			this.SplitPanelContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(647, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 3;
			// 
			// SliceCountsPanel
			// 
			this.SliceCountsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SliceCountsPanel.Location = new System.Drawing.Point(0, 23);
			this.SliceCountsPanel.Name = "SliceCountsPanel";
			this.SliceCountsPanel.Size = new System.Drawing.Size(791, 20);
			this.SliceCountsPanel.TabIndex = 4;
			this.SliceCountsPanel.Visible = false;
			// 
			// OSCPanel
			// 
			this.OSCPanel.Controls.Add(this.FrameRateIO);
			this.OSCPanel.Controls.Add(this.label3);
			this.OSCPanel.Controls.Add(this.OSCMessageIO);
			this.OSCPanel.Controls.Add(this.label2);
			this.OSCPanel.Controls.Add(this.TargetPortIO);
			this.OSCPanel.Controls.Add(this.ReceivePortIO);
			this.OSCPanel.Controls.Add(this.TargetHostIO);
			this.OSCPanel.Controls.Add(this.EnableOSCCheckBox);
			this.OSCPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OSCPanel.Location = new System.Drawing.Point(0, 0);
			this.OSCPanel.Name = "OSCPanel";
			this.OSCPanel.Size = new System.Drawing.Size(791, 23);
			this.OSCPanel.TabIndex = 5;
			this.OSCPanel.Visible = false;
			// 
			// FrameRateIO
			// 
			this.FrameRateIO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FrameRateIO.Location = new System.Drawing.Point(481, 1);
			this.FrameRateIO.Maximum = new decimal(new int[] {
									999,
									0,
									0,
									0});
			this.FrameRateIO.Name = "FrameRateIO";
			this.FrameRateIO.Size = new System.Drawing.Size(60, 20);
			this.FrameRateIO.TabIndex = 5;
			this.FrameRateIO.Value = new decimal(new int[] {
									30,
									0,
									0,
									0});
			this.FrameRateIO.ValueChanged += new System.EventHandler(this.FrameRateIOValueChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(414, 3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(61, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "FrameRate";
			// 
			// OSCMessageIO
			// 
			this.OSCMessageIO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.OSCMessageIO.Location = new System.Drawing.Point(245, 1);
			this.OSCMessageIO.Name = "OSCMessageIO";
			this.OSCMessageIO.Size = new System.Drawing.Size(164, 20);
			this.OSCMessageIO.TabIndex = 2;
			this.OSCMessageIO.Text = "timeliner";
			this.OSCMessageIO.TextChanged += new System.EventHandler(this.OSCMessageIOTextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(160, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(89, 17);
			this.label2.TabIndex = 3;
			this.label2.Text = "OSC Address:   /";
			// 
			// TargetPortIO
			// 
			this.TargetPortIO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TargetPortIO.Location = new System.Drawing.Point(96, 1);
			this.TargetPortIO.Maximum = new decimal(new int[] {
									65536,
									0,
									0,
									0});
			this.TargetPortIO.Name = "TargetPortIO";
			this.TargetPortIO.Size = new System.Drawing.Size(58, 20);
			this.TargetPortIO.TabIndex = 1;
			this.TargetPortIO.Value = new decimal(new int[] {
									4444,
									0,
									0,
									0});
			this.TargetPortIO.ValueChanged += new System.EventHandler(this.TargetPortIOValueChanged);
			// 
			// ReceivePortIO
			// 
			this.ReceivePortIO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ReceivePortIO.Location = new System.Drawing.Point(715, 1);
			this.ReceivePortIO.Maximum = new decimal(new int[] {
									65536,
									0,
									0,
									0});
			this.ReceivePortIO.Name = "ReceivePortIO";
			this.ReceivePortIO.Size = new System.Drawing.Size(58, 20);
			this.ReceivePortIO.TabIndex = 1;
			this.ReceivePortIO.Value = new decimal(new int[] {
									5555,
									0,
									0,
									0});
			this.ReceivePortIO.ValueChanged += new System.EventHandler(this.ReceivePortIOValueChanged);
			// 
			// TargetHostIO
			// 
			this.TargetHostIO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TargetHostIO.Location = new System.Drawing.Point(0, 1);
			this.TargetHostIO.Name = "TargetHostIO";
			this.TargetHostIO.Size = new System.Drawing.Size(90, 20);
			this.TargetHostIO.TabIndex = 0;
			this.TargetHostIO.Text = "127.0.0.1";
			this.TargetHostIO.TextChanged += new System.EventHandler(this.TargetHostIOTextChanged);
			// 
			// EnableOSCCheckBox
			// 
			this.EnableOSCCheckBox.Checked = true;
			this.EnableOSCCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.EnableOSCCheckBox.Location = new System.Drawing.Point(620, 0);
			this.EnableOSCCheckBox.Name = "EnableOSCCheckBox";
			this.EnableOSCCheckBox.Size = new System.Drawing.Size(120, 24);
			this.EnableOSCCheckBox.TabIndex = 2;
			this.EnableOSCCheckBox.Text = "Enable OSC";
			this.EnableOSCCheckBox.UseVisualStyleBackColor = true;
			// 
			// InputsPanel
			// 
			this.InputsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.InputsPanel.Location = new System.Drawing.Point(0, 43);
			this.InputsPanel.Name = "InputsPanel";
			this.InputsPanel.Size = new System.Drawing.Size(791, 46);
			this.InputsPanel.TabIndex = 6;
			// 
			// SplitPanelContainer
			// 
			this.SplitPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitPanelContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SplitPanelContainer.Location = new System.Drawing.Point(0, 89);
			this.SplitPanelContainer.Name = "SplitPanelContainer";
			this.SplitPanelContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitPanelContainer.Panel1
			// 
			this.SplitPanelContainer.Panel1.Controls.Add(this.PluginPanel);
			this.SplitPanelContainer.Panel1.Controls.Add(this.OutputsPanel);
			// 
			// SplitPanelContainer.Panel2
			// 
			this.SplitPanelContainer.Panel2.Controls.Add(this.DebugLog);
			this.SplitPanelContainer.Panel2.Controls.Add(this.ClearButton);
			this.SplitPanelContainer.Size = new System.Drawing.Size(791, 372);
			this.SplitPanelContainer.SplitterDistance = 343;
			this.SplitPanelContainer.TabIndex = 8;
			// 
			// PluginPanel
			// 
			this.PluginPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PluginPanel.Location = new System.Drawing.Point(0, 0);
			this.PluginPanel.Name = "PluginPanel";
			this.PluginPanel.Size = new System.Drawing.Size(791, 289);
			this.PluginPanel.TabIndex = 10;
			// 
			// OutputsPanel
			// 
			this.OutputsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OutputsPanel.Location = new System.Drawing.Point(0, 289);
			this.OutputsPanel.Name = "OutputsPanel";
			this.OutputsPanel.Size = new System.Drawing.Size(791, 54);
			this.OutputsPanel.TabIndex = 8;
			// 
			// DebugLog
			// 
			this.DebugLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DebugLog.FormattingEnabled = true;
			this.DebugLog.IntegralHeight = false;
			this.DebugLog.Location = new System.Drawing.Point(20, 0);
			this.DebugLog.Name = "DebugLog";
			this.DebugLog.ScrollAlwaysVisible = true;
			this.DebugLog.Size = new System.Drawing.Size(771, 25);
			this.DebugLog.TabIndex = 0;
			// 
			// ClearButton
			// 
			this.ClearButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ClearButton.Location = new System.Drawing.Point(0, 0);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(20, 25);
			this.ClearButton.TabIndex = 1;
			this.ClearButton.Text = "X";
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButtonClick);
			// 
			// PluginPage
			// 
			this.Controls.Add(this.SplitPanelContainer);
			this.Controls.Add(this.InputsPanel);
			this.Controls.Add(this.SliceCountsPanel);
			this.Controls.Add(this.OSCPanel);
			this.DoubleBuffered = true;
			this.Name = "PluginPage";
			this.Size = new System.Drawing.Size(791, 461);
			this.SizeChanged += new System.EventHandler(this.MainFormSizeChanged);
			this.OSCPanel.ResumeLayout(false);
			this.OSCPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FrameRateIO)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TargetPortIO)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReceivePortIO)).EndInit();
			this.SplitPanelContainer.Panel1.ResumeLayout(false);
			this.SplitPanelContainer.Panel2.ResumeLayout(false);
			this.SplitPanelContainer.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Panel OSCPanel;
		private System.Windows.Forms.SplitContainer SplitPanelContainer;
		private System.Windows.Forms.Button ClearButton;
		private System.Windows.Forms.ListBox DebugLog;
		private Hoster.PinPanel OutputsPanel;
		private Hoster.PinPanel InputsPanel;
		private System.Windows.Forms.TextBox OSCMessageIO;
		private System.Windows.Forms.NumericUpDown TargetPortIO;
		private System.Windows.Forms.NumericUpDown ReceivePortIO;
		private System.Windows.Forms.TextBox TargetHostIO;
		private System.Windows.Forms.NumericUpDown FrameRateIO;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel SliceCountsPanel;
		private System.Windows.Forms.Panel PluginPanel;
		private System.Windows.Forms.Label label1;
		
		private System.Windows.Forms.CheckBox EnableOSCCheckBox;
	}
}
