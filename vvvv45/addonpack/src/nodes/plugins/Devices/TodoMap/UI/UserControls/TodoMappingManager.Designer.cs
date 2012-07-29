namespace VVVV.TodoMap.UI.UserControls
{
    partial class TodoMappingManager
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
            this.tblMain = new System.Windows.Forms.TableLayoutPanel();
            this.tblIO = new System.Windows.Forms.TableLayoutPanel();
            this.grpInputs = new System.Windows.Forms.GroupBox();
            this.grpOutputs = new System.Windows.Forms.GroupBox();
            this.layoutOutputs = new System.Windows.Forms.TableLayoutPanel();
            this.lblLearnOutput = new System.Windows.Forms.Label();
            this.grpVariables = new System.Windows.Forms.GroupBox();
            this.tblVarLayout = new System.Windows.Forms.TableLayoutPanel();
            this.layoutInputs = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblLearnMode = new System.Windows.Forms.Label();
            this.lbldevall = new System.Windows.Forms.Label();
            this.tblMain.SuspendLayout();
            this.tblIO.SuspendLayout();
            this.grpInputs.SuspendLayout();
            this.grpOutputs.SuspendLayout();
            this.layoutOutputs.SuspendLayout();
            this.grpVariables.SuspendLayout();
            this.layoutInputs.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblMain
            // 
            this.tblMain.ColumnCount = 2;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 59.24714F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40.75286F));
            this.tblMain.Controls.Add(this.tblIO, 1, 0);
            this.tblMain.Controls.Add(this.grpVariables, 0, 0);
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Location = new System.Drawing.Point(0, 0);
            this.tblMain.Name = "tblMain";
            this.tblMain.RowCount = 1;
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.Size = new System.Drawing.Size(611, 346);
            this.tblMain.TabIndex = 0;
            // 
            // tblIO
            // 
            this.tblIO.ColumnCount = 1;
            this.tblIO.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.Controls.Add(this.grpInputs, 0, 0);
            this.tblIO.Controls.Add(this.grpOutputs, 0, 1);
            this.tblIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblIO.Location = new System.Drawing.Point(365, 3);
            this.tblIO.Name = "tblIO";
            this.tblIO.RowCount = 2;
            this.tblIO.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblIO.Size = new System.Drawing.Size(243, 340);
            this.tblIO.TabIndex = 0;
            // 
            // grpInputs
            // 
            this.grpInputs.Controls.Add(this.layoutInputs);
            this.grpInputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInputs.Location = new System.Drawing.Point(3, 3);
            this.grpInputs.Name = "grpInputs";
            this.grpInputs.Size = new System.Drawing.Size(237, 164);
            this.grpInputs.TabIndex = 0;
            this.grpInputs.TabStop = false;
            this.grpInputs.Text = "Inputs";
            // 
            // grpOutputs
            // 
            this.grpOutputs.Controls.Add(this.layoutOutputs);
            this.grpOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOutputs.Location = new System.Drawing.Point(3, 173);
            this.grpOutputs.Name = "grpOutputs";
            this.grpOutputs.Size = new System.Drawing.Size(237, 164);
            this.grpOutputs.TabIndex = 1;
            this.grpOutputs.TabStop = false;
            this.grpOutputs.Text = "Outputs";
            // 
            // layoutOutputs
            // 
            this.layoutOutputs.ColumnCount = 1;
            this.layoutOutputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutOutputs.Controls.Add(this.lblLearnOutput, 0, 0);
            this.layoutOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutOutputs.Location = new System.Drawing.Point(3, 16);
            this.layoutOutputs.Name = "layoutOutputs";
            this.layoutOutputs.RowCount = 2;
            this.layoutOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.layoutOutputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutOutputs.Size = new System.Drawing.Size(231, 145);
            this.layoutOutputs.TabIndex = 1;
            // 
            // lblLearnOutput
            // 
            this.lblLearnOutput.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblLearnOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLearnOutput.Location = new System.Drawing.Point(3, 0);
            this.lblLearnOutput.Name = "lblLearnOutput";
            this.lblLearnOutput.Size = new System.Drawing.Size(100, 20);
            this.lblLearnOutput.TabIndex = 1;
            this.lblLearnOutput.Text = "Learn Mode";
            this.lblLearnOutput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLearnOutput.Click += new System.EventHandler(this.label1_Click);
            // 
            // grpVariables
            // 
            this.grpVariables.Controls.Add(this.tblVarLayout);
            this.grpVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVariables.Location = new System.Drawing.Point(3, 3);
            this.grpVariables.Name = "grpVariables";
            this.grpVariables.Size = new System.Drawing.Size(356, 340);
            this.grpVariables.TabIndex = 1;
            this.grpVariables.TabStop = false;
            this.grpVariables.Text = "Variables";
            // 
            // tblVarLayout
            // 
            this.tblVarLayout.ColumnCount = 1;
            this.tblVarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblVarLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblVarLayout.Location = new System.Drawing.Point(3, 16);
            this.tblVarLayout.Name = "tblVarLayout";
            this.tblVarLayout.RowCount = 2;
            this.tblVarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tblVarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblVarLayout.Size = new System.Drawing.Size(350, 321);
            this.tblVarLayout.TabIndex = 0;
            // 
            // layoutInputs
            // 
            this.layoutInputs.ColumnCount = 1;
            this.layoutInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutInputs.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.layoutInputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutInputs.Location = new System.Drawing.Point(3, 16);
            this.layoutInputs.Name = "layoutInputs";
            this.layoutInputs.RowCount = 2;
            this.layoutInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.layoutInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutInputs.Size = new System.Drawing.Size(231, 145);
            this.layoutInputs.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.lblLearnMode);
            this.flowLayoutPanel1.Controls.Add(this.lbldevall);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(225, 23);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // lblLearnMode
            // 
            this.lblLearnMode.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblLearnMode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLearnMode.Location = new System.Drawing.Point(3, 0);
            this.lblLearnMode.Name = "lblLearnMode";
            this.lblLearnMode.Size = new System.Drawing.Size(100, 20);
            this.lblLearnMode.TabIndex = 2;
            this.lblLearnMode.Text = "Learn Mode";
            this.lblLearnMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLearnMode.Click += new System.EventHandler(this.lblLearnMode_Click);
            // 
            // lbldevall
            // 
            this.lbldevall.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lbldevall.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbldevall.Location = new System.Drawing.Point(109, 0);
            this.lbldevall.Name = "lbldevall";
            this.lbldevall.Size = new System.Drawing.Size(100, 20);
            this.lbldevall.TabIndex = 3;
            this.lbldevall.Text = "Any Device";
            this.lbldevall.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbldevall.Click += new System.EventHandler(this.lbldevall_Click);
            // 
            // TodoMappingManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tblMain);
            this.Name = "TodoMappingManager";
            this.Size = new System.Drawing.Size(611, 346);
            this.tblMain.ResumeLayout(false);
            this.tblIO.ResumeLayout(false);
            this.grpInputs.ResumeLayout(false);
            this.grpOutputs.ResumeLayout(false);
            this.layoutOutputs.ResumeLayout(false);
            this.grpVariables.ResumeLayout(false);
            this.layoutInputs.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblMain;
        private System.Windows.Forms.TableLayoutPanel tblIO;
        private System.Windows.Forms.GroupBox grpInputs;
        private System.Windows.Forms.GroupBox grpOutputs;
        private System.Windows.Forms.GroupBox grpVariables;
        private System.Windows.Forms.TableLayoutPanel tblVarLayout;
        private System.Windows.Forms.TableLayoutPanel layoutOutputs;
        private System.Windows.Forms.Label lblLearnOutput;
        private System.Windows.Forms.TableLayoutPanel layoutInputs;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblLearnMode;
        private System.Windows.Forms.Label lbldevall;

    }
}
