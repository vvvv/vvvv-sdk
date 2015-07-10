namespace VVVV.TodoMap.UI.Controls
{
    partial class VariableFilterCtrl
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tbvarname = new System.Windows.Forms.TextBox();
            this.lblnewVar = new System.Windows.Forms.Label();
            this.cmbCatFilter = new System.Windows.Forms.ComboBox();
            this.lblsetdefault = new System.Windows.Forms.Label();
            this.lblsave = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.tbvarname);
            this.flowLayoutPanel1.Controls.Add(this.lblnewVar);
            this.flowLayoutPanel1.Controls.Add(this.cmbCatFilter);
            this.flowLayoutPanel1.Controls.Add(this.lblsetdefault);
            this.flowLayoutPanel1.Controls.Add(this.lblsave);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(559, 30);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tbvarname
            // 
            this.tbvarname.Location = new System.Drawing.Point(3, 3);
            this.tbvarname.Name = "tbvarname";
            this.tbvarname.Size = new System.Drawing.Size(100, 20);
            this.tbvarname.TabIndex = 3;
            // 
            // lblnewVar
            // 
            this.lblnewVar.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblnewVar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblnewVar.Location = new System.Drawing.Point(109, 0);
            this.lblnewVar.Name = "lblnewVar";
            this.lblnewVar.Size = new System.Drawing.Size(100, 23);
            this.lblnewVar.TabIndex = 1;
            this.lblnewVar.Text = "New Variable";
            this.lblnewVar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblnewVar.Click += new System.EventHandler(this.lblnewVar_Click);
            // 
            // cmbCatFilter
            // 
            this.cmbCatFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCatFilter.FormattingEnabled = true;
            this.cmbCatFilter.Location = new System.Drawing.Point(215, 3);
            this.cmbCatFilter.Name = "cmbCatFilter";
            this.cmbCatFilter.Size = new System.Drawing.Size(121, 21);
            this.cmbCatFilter.TabIndex = 0;
            // 
            // lblsetdefault
            // 
            this.lblsetdefault.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblsetdefault.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblsetdefault.Location = new System.Drawing.Point(342, 0);
            this.lblsetdefault.Name = "lblsetdefault";
            this.lblsetdefault.Size = new System.Drawing.Size(97, 23);
            this.lblsetdefault.TabIndex = 4;
            this.lblsetdefault.Text = "Set Default";
            this.lblsetdefault.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblsetdefault.Visible = false;
            this.lblsetdefault.Click += new System.EventHandler(this.label1_Click);
            // 
            // lblsave
            // 
            this.lblsave.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblsave.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblsave.Location = new System.Drawing.Point(445, 0);
            this.lblsave.Name = "lblsave";
            this.lblsave.Size = new System.Drawing.Size(56, 23);
            this.lblsave.TabIndex = 2;
            this.lblsave.Text = "Save";
            this.lblsave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblsave.Click += new System.EventHandler(this.lblsave_Click);
            // 
            // VariableFilterCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "VariableFilterCtrl";
            this.Size = new System.Drawing.Size(559, 30);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ComboBox cmbCatFilter;
        private System.Windows.Forms.Label lblnewVar;
        private System.Windows.Forms.Label lblsave;
        private System.Windows.Forms.TextBox tbvarname;
        private System.Windows.Forms.Label lblsetdefault;

    }
}
