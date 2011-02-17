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
            this.mainTab = new System.Windows.Forms.TabControl();
            this.tabMapper = new System.Windows.Forms.TabPage();
            this.tabDevices = new System.Windows.Forms.TabPage();
            this.mainTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTab
            // 
            this.mainTab.Controls.Add(this.tabMapper);
            this.mainTab.Controls.Add(this.tabDevices);
            this.mainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTab.Location = new System.Drawing.Point(0, 0);
            this.mainTab.Name = "mainTab";
            this.mainTab.SelectedIndex = 0;
            this.mainTab.Size = new System.Drawing.Size(824, 422);
            this.mainTab.TabIndex = 0;
            // 
            // tabMapper
            // 
            this.tabMapper.Location = new System.Drawing.Point(4, 22);
            this.tabMapper.Name = "tabMapper";
            this.tabMapper.Padding = new System.Windows.Forms.Padding(3);
            this.tabMapper.Size = new System.Drawing.Size(816, 396);
            this.tabMapper.TabIndex = 0;
            this.tabMapper.Text = "Mappings";
            this.tabMapper.UseVisualStyleBackColor = true;
            // 
            // tabDevices
            // 
            this.tabDevices.Location = new System.Drawing.Point(4, 22);
            this.tabDevices.Name = "tabDevices";
            this.tabDevices.Padding = new System.Windows.Forms.Padding(3);
            this.tabDevices.Size = new System.Drawing.Size(816, 396);
            this.tabDevices.TabIndex = 1;
            this.tabDevices.Text = "Devices";
            this.tabDevices.UseVisualStyleBackColor = true;
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

        private System.Windows.Forms.TabControl mainTab;
        private System.Windows.Forms.TabPage tabMapper;
        private System.Windows.Forms.TabPage tabDevices;
    }
}
