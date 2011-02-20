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
            this.grpMidiInput = new System.Windows.Forms.GroupBox();
            this.grpMidiOutput = new System.Windows.Forms.GroupBox();
            this.grpOsc = new System.Windows.Forms.GroupBox();
            this.lvMidiInput = new ListViewEx.ListViewEx();
            this.lvMidiOutput = new ListViewEx.ListViewEx();
            this.mainLayout.SuspendLayout();
            this.grpMidiInput.SuspendLayout();
            this.grpMidiOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.Controls.Add(this.grpMidiInput, 0, 0);
            this.mainLayout.Controls.Add(this.grpMidiOutput, 0, 1);
            this.mainLayout.Controls.Add(this.grpOsc, 0, 2);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 131F));
            this.mainLayout.Size = new System.Drawing.Size(602, 371);
            this.mainLayout.TabIndex = 0;
            // 
            // grpMidiInput
            // 
            this.grpMidiInput.Controls.Add(this.lvMidiInput);
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
            this.grpMidiOutput.Controls.Add(this.lvMidiOutput);
            this.grpMidiOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMidiOutput.Location = new System.Drawing.Point(3, 123);
            this.grpMidiOutput.Name = "grpMidiOutput";
            this.grpMidiOutput.Size = new System.Drawing.Size(596, 114);
            this.grpMidiOutput.TabIndex = 1;
            this.grpMidiOutput.TabStop = false;
            this.grpMidiOutput.Text = "Midi Output Settings";
            // 
            // grpOsc
            // 
            this.grpOsc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOsc.Location = new System.Drawing.Point(3, 243);
            this.grpOsc.Name = "grpOsc";
            this.grpOsc.Size = new System.Drawing.Size(596, 125);
            this.grpOsc.TabIndex = 2;
            this.grpOsc.TabStop = false;
            this.grpOsc.Text = "OSC Settings";
            // 
            // lvMidiInput
            // 
            this.lvMidiInput.AllowColumnReorder = true;
            this.lvMidiInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMidiInput.DoubleClickActivation = false;
            this.lvMidiInput.FullRowSelect = true;
            this.lvMidiInput.Location = new System.Drawing.Point(3, 16);
            this.lvMidiInput.Name = "lvMidiInput";
            this.lvMidiInput.Size = new System.Drawing.Size(590, 95);
            this.lvMidiInput.TabIndex = 0;
            this.lvMidiInput.UseCompatibleStateImageBehavior = false;
            this.lvMidiInput.View = System.Windows.Forms.View.Details;
            // 
            // lvMidiOutput
            // 
            this.lvMidiOutput.AllowColumnReorder = true;
            this.lvMidiOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMidiOutput.DoubleClickActivation = false;
            this.lvMidiOutput.FullRowSelect = true;
            this.lvMidiOutput.Location = new System.Drawing.Point(3, 16);
            this.lvMidiOutput.Name = "lvMidiOutput";
            this.lvMidiOutput.Size = new System.Drawing.Size(590, 95);
            this.lvMidiOutput.TabIndex = 0;
            this.lvMidiOutput.UseCompatibleStateImageBehavior = false;
            this.lvMidiOutput.View = System.Windows.Forms.View.Details;
            // 
            // TodoDeviceManagerCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayout);
            this.Name = "TodoDeviceManagerCtrl";
            this.Size = new System.Drawing.Size(602, 371);
            this.mainLayout.ResumeLayout(false);
            this.grpMidiInput.ResumeLayout(false);
            this.grpMidiOutput.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.GroupBox grpMidiInput;
        private System.Windows.Forms.GroupBox grpMidiOutput;
        private System.Windows.Forms.GroupBox grpOsc;
        private ListViewEx.ListViewEx lvMidiInput;
        private ListViewEx.ListViewEx lvMidiOutput;
    }
}
