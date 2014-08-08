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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TodoMapNode));
            this.tabLog = new System.Windows.Forms.TabPage();
            this.tabOsc = new System.Windows.Forms.TabPage();
            this.tabMidi = new System.Windows.Forms.TabPage();
            this.tabMapper = new System.Windows.Forms.TabPage();
            this.mainTab = new System.Windows.Forms.TabControl();
            this.tabTreeVar = new System.Windows.Forms.TabPage();
            this.layouttree = new System.Windows.Forms.TableLayoutPanel();
            this.grpTree = new System.Windows.Forms.GroupBox();
            this.grpProps = new System.Windows.Forms.GroupBox();
            this.treelist = new System.Windows.Forms.ImageList(this.components);
            this.mainTab.SuspendLayout();
            this.tabTreeVar.SuspendLayout();
            this.layouttree.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLog
            // 
            this.tabLog.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tabLog.ForeColor = System.Drawing.Color.Black;
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Name = "tabLog";
            this.tabLog.Size = new System.Drawing.Size(816, 396);
            this.tabLog.TabIndex = 2;
            this.tabLog.Text = "TodoLog";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // tabOsc
            // 
            this.tabOsc.Location = new System.Drawing.Point(4, 22);
            this.tabOsc.Name = "tabOsc";
            this.tabOsc.Size = new System.Drawing.Size(816, 396);
            this.tabOsc.TabIndex = 3;
            this.tabOsc.Text = "OSC";
            this.tabOsc.UseVisualStyleBackColor = true;
            // 
            // tabMidi
            // 
            this.tabMidi.Location = new System.Drawing.Point(4, 22);
            this.tabMidi.Name = "tabMidi";
            this.tabMidi.Padding = new System.Windows.Forms.Padding(3);
            this.tabMidi.Size = new System.Drawing.Size(816, 396);
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
            this.mainTab.Controls.Add(this.tabTreeVar);
            this.mainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTab.Location = new System.Drawing.Point(0, 0);
            this.mainTab.Name = "mainTab";
            this.mainTab.SelectedIndex = 0;
            this.mainTab.Size = new System.Drawing.Size(824, 422);
            this.mainTab.TabIndex = 0;
            // 
            // tabTreeVar
            // 
            this.tabTreeVar.Controls.Add(this.layouttree);
            this.tabTreeVar.Location = new System.Drawing.Point(4, 22);
            this.tabTreeVar.Name = "tabTreeVar";
            this.tabTreeVar.Size = new System.Drawing.Size(816, 396);
            this.tabTreeVar.TabIndex = 5;
            this.tabTreeVar.Text = "Variables";
            this.tabTreeVar.UseVisualStyleBackColor = true;
            // 
            // layouttree
            // 
            this.layouttree.ColumnCount = 2;
            this.layouttree.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.09804F));
            this.layouttree.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54.90196F));
            this.layouttree.Controls.Add(this.grpTree, 0, 0);
            this.layouttree.Controls.Add(this.grpProps, 1, 0);
            this.layouttree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layouttree.Location = new System.Drawing.Point(0, 0);
            this.layouttree.Name = "layouttree";
            this.layouttree.RowCount = 1;
            this.layouttree.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layouttree.Size = new System.Drawing.Size(816, 396);
            this.layouttree.TabIndex = 0;
            // 
            // grpTree
            // 
            this.grpTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTree.Location = new System.Drawing.Point(3, 3);
            this.grpTree.Name = "grpTree";
            this.grpTree.Size = new System.Drawing.Size(362, 390);
            this.grpTree.TabIndex = 0;
            this.grpTree.TabStop = false;
            this.grpTree.Text = "Variables";
            // 
            // grpProps
            // 
            this.grpProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpProps.Location = new System.Drawing.Point(371, 3);
            this.grpProps.Name = "grpProps";
            this.grpProps.Size = new System.Drawing.Size(442, 390);
            this.grpProps.TabIndex = 1;
            this.grpProps.TabStop = false;
            this.grpProps.Text = "Properties";
            // 
            // treelist
            // 
            this.treelist.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treelist.ImageStream")));
            this.treelist.TransparentColor = System.Drawing.Color.Transparent;
            this.treelist.Images.SetKeyName(0, "folder_with_file.ico");
            this.treelist.Images.SetKeyName(1, "www.eicostudio.com.ico");
            this.treelist.Images.SetKeyName(2, "HD.ico");
            this.treelist.Images.SetKeyName(3, "network.ico");
            this.treelist.Images.SetKeyName(4, "My_computer.ico");
            // 
            // TodoMapNode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainTab);
            this.Name = "TodoMapNode";
            this.Size = new System.Drawing.Size(824, 422);
            this.mainTab.ResumeLayout(false);
            this.tabTreeVar.ResumeLayout(false);
            this.layouttree.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.TabPage tabOsc;
        private System.Windows.Forms.TabPage tabMidi;
        private System.Windows.Forms.TabPage tabMapper;
        private System.Windows.Forms.TabControl mainTab;
        private System.Windows.Forms.TabPage tabTreeVar;
        private System.Windows.Forms.ImageList treelist;
        private System.Windows.Forms.TableLayoutPanel layouttree;
        private System.Windows.Forms.GroupBox grpTree;
        private System.Windows.Forms.GroupBox grpProps;

    }
}
