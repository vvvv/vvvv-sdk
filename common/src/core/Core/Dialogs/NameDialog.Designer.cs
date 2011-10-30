namespace VVVV.Core.Dialogs
{
    partial class NameDialog
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
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TextBoxName
            // 
            this.TextBoxName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBoxName.Location = new System.Drawing.Point(12, 12);
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.Size = new System.Drawing.Size(150, 20);
            this.TextBoxName.TabIndex = 2;
            this.TextBoxName.Text = "Name";
            this.TextBoxName.Click += new System.EventHandler(this.textBox1_Click);
            this.TextBoxName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBoxFunctionName_KeyPress);
            this.TextBoxName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxFunctionName_KeyUp);
            // 
            // NameDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(174, 68);
            this.Controls.Add(this.TextBoxName);
            this.Name = "NameDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "";
            this.Controls.SetChildIndex(this.TextBoxName, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.TextBox TextBoxName;

    }
}
