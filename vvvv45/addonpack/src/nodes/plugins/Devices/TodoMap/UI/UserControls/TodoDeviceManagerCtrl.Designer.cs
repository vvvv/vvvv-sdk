namespace VVVV.TodoMap.UI.UserControls
{
    partial class TodoDeviceManagerCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpClock = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbClock = new System.Windows.Forms.ComboBox();
            this.grpMidiInput = new System.Windows.Forms.GroupBox();
            this.grpMidiOutput = new System.Windows.Forms.GroupBox();
            this.chkenableclock = new System.Windows.Forms.CheckBox();
            this.lbltime = new System.Windows.Forms.Label();
            this.mainLayout.SuspendLayout();
            this.grpClock.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.Controls.Add(this.grpClock, 0, 2);
            this.mainLayout.Controls.Add(this.grpMidiInput, 0, 0);
            this.mainLayout.Controls.Add(this.grpMidiOutput, 0, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 131F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayout.Size = new System.Drawing.Size(602, 371);
            this.mainLayout.TabIndex = 0;
            // 
            // grpClock
            // 
            this.grpClock.Controls.Add(this.lbltime);
            this.grpClock.Controls.Add(this.chkenableclock);
            this.grpClock.Controls.Add(this.label2);
            this.grpClock.Controls.Add(this.label1);
            this.grpClock.Controls.Add(this.cmbClock);
            this.grpClock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpClock.Location = new System.Drawing.Point(3, 243);
            this.grpClock.Name = "grpClock";
            this.grpClock.Size = new System.Drawing.Size(596, 125);
            this.grpClock.TabIndex = 2;
            this.grpClock.TabStop = false;
            this.grpClock.Text = "Midi Clock Settings";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(331, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current Time:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Clock Device:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbClock
            // 
            this.cmbClock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbClock.FormattingEnabled = true;
            this.cmbClock.Location = new System.Drawing.Point(106, 19);
            this.cmbClock.Name = "cmbClock";
            this.cmbClock.Size = new System.Drawing.Size(142, 21);
            this.cmbClock.TabIndex = 0;
            // 
            // grpMidiInput
            // 
            this.grpMidiInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMidiInput.Location = new System.Drawing.Point(3, 3);
            this.grpMidiInput.Name = "grpMidiInput";
            this.grpMidiInput.Size = new System.Drawing.Size(596, 114);
            this.grpMidiInput.TabIndex = 0;
            this.grpMidiInput.TabStop = false;
            this.grpMidiInput.Text = "Midi Input Settings";
            // 
            // grpMidiOutput
            // 
            this.grpMidiOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMidiOutput.Location = new System.Drawing.Point(3, 123);
            this.grpMidiOutput.Name = "grpMidiOutput";
            this.grpMidiOutput.Size = new System.Drawing.Size(596, 114);
            this.grpMidiOutput.TabIndex = 1;
            this.grpMidiOutput.TabStop = false;
            this.grpMidiOutput.Text = "Midi Output Settings";
            // 
            // chkenableclock
            // 
            this.chkenableclock.Location = new System.Drawing.Point(254, 18);
            this.chkenableclock.Name = "chkenableclock";
            this.chkenableclock.Size = new System.Drawing.Size(104, 24);
            this.chkenableclock.TabIndex = 3;
            this.chkenableclock.Text = "Enabled";
            this.chkenableclock.UseVisualStyleBackColor = true;
            this.chkenableclock.CheckedChanged += new System.EventHandler(this.chkenableclock_CheckedChanged);
            // 
            // lbltime
            // 
            this.lbltime.Location = new System.Drawing.Point(429, 19);
            this.lbltime.Name = "lbltime";
            this.lbltime.Size = new System.Drawing.Size(78, 23);
            this.lbltime.TabIndex = 4;
            this.lbltime.Text = "N/A";
            this.lbltime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TodoDeviceManagerCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayout);
            this.Name = "TodoDeviceManagerCtrl";
            this.Size = new System.Drawing.Size(602, 371);
            this.mainLayout.ResumeLayout(false);
            this.grpClock.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.GroupBox grpMidiInput;
        private System.Windows.Forms.GroupBox grpMidiOutput;
        private System.Windows.Forms.GroupBox grpClock;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbClock;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkenableclock;
        private System.Windows.Forms.Label lbltime;

    }
}
