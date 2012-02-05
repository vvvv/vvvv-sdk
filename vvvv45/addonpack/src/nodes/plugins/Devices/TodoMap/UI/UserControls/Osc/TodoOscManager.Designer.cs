namespace VVVV.TodoMap.UI.UserControls.Osc
{
    partial class TodoOscManager
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
            this.tblLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpOscIn = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbInputPort = new System.Windows.Forms.TextBox();
            this.chkEnableIn = new System.Windows.Forms.CheckBox();
            this.chkAutoStartIn = new System.Windows.Forms.CheckBox();
            this.lblstatus = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.chkOutput = new System.Windows.Forms.CheckBox();
            this.chkAutoStartOut = new System.Windows.Forms.CheckBox();
            this.tblLayout.SuspendLayout();
            this.grpOscIn.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblLayout
            // 
            this.tblLayout.ColumnCount = 1;
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayout.Controls.Add(this.grpOscIn, 0, 0);
            this.tblLayout.Controls.Add(this.groupBox1, 0, 1);
            this.tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayout.Location = new System.Drawing.Point(0, 0);
            this.tblLayout.Name = "tblLayout";
            this.tblLayout.RowCount = 2;
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayout.Size = new System.Drawing.Size(607, 228);
            this.tblLayout.TabIndex = 0;
            // 
            // grpOscIn
            // 
            this.grpOscIn.Controls.Add(this.flowLayoutPanel1);
            this.grpOscIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOscIn.Location = new System.Drawing.Point(3, 3);
            this.grpOscIn.Name = "grpOscIn";
            this.grpOscIn.Size = new System.Drawing.Size(601, 108);
            this.grpOscIn.TabIndex = 0;
            this.grpOscIn.TabStop = false;
            this.grpOscIn.Text = "Input";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.tbInputPort);
            this.flowLayoutPanel1.Controls.Add(this.chkEnableIn);
            this.flowLayoutPanel1.Controls.Add(this.chkAutoStartIn);
            this.flowLayoutPanel1.Controls.Add(this.lblstatus);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(595, 89);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbInputPort
            // 
            this.tbInputPort.Location = new System.Drawing.Point(109, 3);
            this.tbInputPort.Name = "tbInputPort";
            this.tbInputPort.Size = new System.Drawing.Size(100, 20);
            this.tbInputPort.TabIndex = 0;
            // 
            // chkEnableIn
            // 
            this.chkEnableIn.Location = new System.Drawing.Point(215, 3);
            this.chkEnableIn.Name = "chkEnableIn";
            this.chkEnableIn.Size = new System.Drawing.Size(75, 24);
            this.chkEnableIn.TabIndex = 2;
            this.chkEnableIn.Text = "Enable";
            this.chkEnableIn.UseVisualStyleBackColor = true;
            this.chkEnableIn.CheckedChanged += new System.EventHandler(this.chkEnableIn_CheckedChanged);
            // 
            // chkAutoStartIn
            // 
            this.chkAutoStartIn.Location = new System.Drawing.Point(296, 3);
            this.chkAutoStartIn.Name = "chkAutoStartIn";
            this.chkAutoStartIn.Size = new System.Drawing.Size(75, 24);
            this.chkAutoStartIn.TabIndex = 4;
            this.chkAutoStartIn.Text = "Auto Start";
            this.chkAutoStartIn.UseVisualStyleBackColor = true;
            this.chkAutoStartIn.CheckedChanged += new System.EventHandler(this.chkAutoStartIn_CheckedChanged);
            // 
            // lblstatus
            // 
            this.lblstatus.Location = new System.Drawing.Point(377, 0);
            this.lblstatus.Name = "lblstatus";
            this.lblstatus.Size = new System.Drawing.Size(185, 23);
            this.lblstatus.TabIndex = 3;
            this.lblstatus.Text = "Status: ";
            this.lblstatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.flowLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 117);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(601, 108);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label2);
            this.flowLayoutPanel2.Controls.Add(this.tbOutput);
            this.flowLayoutPanel2.Controls.Add(this.chkOutput);
            this.flowLayoutPanel2.Controls.Add(this.chkAutoStartOut);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(595, 89);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(109, 3);
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.Size = new System.Drawing.Size(100, 20);
            this.tbOutput.TabIndex = 4;
            // 
            // chkOutput
            // 
            this.chkOutput.Location = new System.Drawing.Point(215, 3);
            this.chkOutput.Name = "chkOutput";
            this.chkOutput.Size = new System.Drawing.Size(75, 24);
            this.chkOutput.TabIndex = 6;
            this.chkOutput.Text = "Enable";
            this.chkOutput.UseVisualStyleBackColor = true;
            this.chkOutput.CheckedChanged += new System.EventHandler(this.chkOutput_CheckedChanged);
            // 
            // chkAutoStartOut
            // 
            this.chkAutoStartOut.Location = new System.Drawing.Point(296, 3);
            this.chkAutoStartOut.Name = "chkAutoStartOut";
            this.chkAutoStartOut.Size = new System.Drawing.Size(75, 24);
            this.chkAutoStartOut.TabIndex = 7;
            this.chkAutoStartOut.Text = "Auto Start";
            this.chkAutoStartOut.UseVisualStyleBackColor = true;
            this.chkAutoStartOut.CheckedChanged += new System.EventHandler(this.chkAutoStartOut_CheckedChanged);
            // 
            // TodoOscManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tblLayout);
            this.Name = "TodoOscManager";
            this.Size = new System.Drawing.Size(607, 228);
            this.tblLayout.ResumeLayout(false);
            this.grpOscIn.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblLayout;
        private System.Windows.Forms.GroupBox grpOscIn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbInputPort;
        private System.Windows.Forms.CheckBox chkEnableIn;
        private System.Windows.Forms.Label lblstatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.CheckBox chkOutput;
        private System.Windows.Forms.CheckBox chkAutoStartIn;
        private System.Windows.Forms.CheckBox chkAutoStartOut;
    }
}
