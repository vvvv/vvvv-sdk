namespace VVVV.Core.Dialogs
{
    partial class BaseDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        	this.ButtonCancel = new System.Windows.Forms.Button();
        	this.ButtonOK = new System.Windows.Forms.Button();
        	this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
        	this.flowLayoutPanel1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// ButtonCancel
        	// 
        	this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ButtonCancel.Location = new System.Drawing.Point(387, 3);
        	this.ButtonCancel.Name = "ButtonCancel";
        	this.ButtonCancel.Size = new System.Drawing.Size(81, 23);
        	this.ButtonCancel.TabIndex = 1;
        	this.ButtonCancel.Text = "Cancel";
        	this.ButtonCancel.UseVisualStyleBackColor = true;
        	// 
        	// ButtonOK
        	// 
        	this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ButtonOK.Location = new System.Drawing.Point(300, 3);
        	this.ButtonOK.Name = "ButtonOK";
        	this.ButtonOK.Size = new System.Drawing.Size(81, 23);
        	this.ButtonOK.TabIndex = 0;
        	this.ButtonOK.Text = "OK";
        	this.ButtonOK.UseVisualStyleBackColor = true;
        	// 
        	// flowLayoutPanel1
        	// 
        	this.flowLayoutPanel1.Controls.Add(this.ButtonCancel);
        	this.flowLayoutPanel1.Controls.Add(this.ButtonOK);
        	this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        	this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 280);
        	this.flowLayoutPanel1.Name = "flowLayoutPanel1";
        	this.flowLayoutPanel1.Size = new System.Drawing.Size(471, 29);
        	this.flowLayoutPanel1.TabIndex = 2;
        	this.flowLayoutPanel1.WrapContents = false;
        	// 
        	// BaseDialog
        	// 
        	this.AcceptButton = this.ButtonOK;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.SystemColors.ControlDark;
        	this.CancelButton = this.ButtonCancel;
        	this.ClientSize = new System.Drawing.Size(471, 309);
        	this.ControlBox = false;
        	this.Controls.Add(this.flowLayoutPanel1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.Name = "BaseDialog";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        	this.Text = "BaseDialog";
        	this.flowLayoutPanel1.ResumeLayout(false);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        protected System.Windows.Forms.Button ButtonCancel;
        protected System.Windows.Forms.Button ButtonOK;
    }
}