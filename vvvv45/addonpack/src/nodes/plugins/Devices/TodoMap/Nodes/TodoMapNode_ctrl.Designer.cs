namespace VVVV.TodoMap.Nodes
{
    partial class TodoMapNode
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
            this.tabLog = new System.Windows.Forms.TabPage();
            this.tabOsc = new System.Windows.Forms.TabPage();
            this.tabMidi = new System.Windows.Forms.TabPage();
            this.tabMapper = new System.Windows.Forms.TabPage();
            this.mainTab = new System.Windows.Forms.TabControl();
            this.mainTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLog
            // 
            this.tabLog.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tabLog.ForeColor = System.Drawing.Color.Black;
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Name = "tabLog";
            this.tabLog.Size = new System.Drawing.Size(816, 372);
            this.tabLog.TabIndex = 2;
            this.tabLog.Text = "TodoLog";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // tabOsc
            // 
            this.tabOsc.Location = new System.Drawing.Point(4, 22);
            this.tabOsc.Name = "tabOsc";
            this.tabOsc.Size = new System.Drawing.Size(816, 372);
            this.tabOsc.TabIndex = 3;
            this.tabOsc.Text = "OSC";
            this.tabOsc.UseVisualStyleBackColor = true;
            // 
            // tabMidi
            // 
            this.tabMidi.Location = new System.Drawing.Point(4, 22);
            this.tabMidi.Name = "tabMidi";
            this.tabMidi.Padding = new System.Windows.Forms.Padding(3);
            this.tabMidi.Size = new System.Drawing.Size(816, 372);
            this.tabMidi.TabIndex = 1;
            this.tabMidi.Text = "Midi";
            this.tabMidi.UseVisualStyleBackColor = true;
            // 
            // tabMapper
            // 
            this.tabMapper.Location = new System.Drawing.Point(4, 22);
            this.tabMapper.Name = "tabMapper";
            this.tabMapper.Size = new System.Drawing.Size(816, 396);
            this.tabMapper.TabIndex = 4;
            this.tabMapper.Text = "Mappings";
            this.tabMapper.UseVisualStyleBackColor = true;
            // 
            // mainTab
            // 
            this.mainTab.Controls.Add(this.tabMapper);
            this.mainTab.Controls.Add(this.tabMidi);
            this.mainTab.Controls.Add(this.tabOsc);
            this.mainTab.Controls.Add(this.tabLog);
            this.mainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTab.Location = new System.Drawing.Point(0, 0);
            this.mainTab.Name = "mainTab";
            this.mainTab.SelectedIndex = 0;
            this.mainTab.Size = new System.Drawing.Size(824, 422);
            this.mainTab.TabIndex = 0;
            // 
            // TodoMapNode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainTab);
            this.Name = "TodoMapNode";
            this.Size = new System.Drawing.Size(824, 422);
            this.mainTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.TabPage tabOsc;
        private System.Windows.Forms.TabPage tabMidi;
        private System.Windows.Forms.TabPage tabMapper;
        private System.Windows.Forms.TabControl mainTab;

    }
}
