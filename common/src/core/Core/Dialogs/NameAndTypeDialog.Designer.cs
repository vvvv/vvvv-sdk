namespace VVVV.Core.Dialogs
{
    partial class NameAndTypeDialog
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
        	this.NameTextBox = new System.Windows.Forms.TextBox();
        	this.TypeTextBox = new System.Windows.Forms.TextBox();
        	this.label1 = new System.Windows.Forms.Label();
        	this.label2 = new System.Windows.Forms.Label();
        	this.SuspendLayout();
        	// 
        	// NameTextBox
        	// 
        	this.NameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.NameTextBox.Location = new System.Drawing.Point(12, 27);
        	this.NameTextBox.Name = "NameTextBox";
        	this.NameTextBox.Size = new System.Drawing.Size(150, 20);
        	this.NameTextBox.TabIndex = 3;
        	this.NameTextBox.Text = "Name";
        	this.NameTextBox.Click += new System.EventHandler(this.textBox1_Click);
        	this.NameTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBoxFunctionName_KeyPress);
        	this.NameTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxFunctionName_KeyUp);
        	// 
        	// TypeTextBox
        	// 
        	this.TypeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.TypeTextBox.Location = new System.Drawing.Point(12, 69);
        	this.TypeTextBox.Name = "TypeTextBox";
        	this.TypeTextBox.Size = new System.Drawing.Size(150, 20);
        	this.TypeTextBox.TabIndex = 4;
        	this.TypeTextBox.Text = "Type";
        	// 
        	// label1
        	// 
        	this.label1.Location = new System.Drawing.Point(15, 8);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(103, 16);
        	this.label1.TabIndex = 4;
        	this.label1.Text = "Name";
        	// 
        	// label2
        	// 
        	this.label2.Location = new System.Drawing.Point(15, 50);
        	this.label2.Name = "label2";
        	this.label2.Size = new System.Drawing.Size(103, 16);
        	this.label2.TabIndex = 5;
        	this.label2.Text = "Type";
        	// 
        	// NameAndTypeDialog
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.ClientSize = new System.Drawing.Size(174, 123);
        	this.Controls.Add(this.label2);
        	this.Controls.Add(this.label1);
        	this.Controls.Add(this.TypeTextBox);
        	this.Controls.Add(this.NameTextBox);
        	this.Name = "NameAndTypeDialog";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        	this.Text = "";
        	this.Controls.SetChildIndex(this.NameTextBox, 0);
        	this.Controls.SetChildIndex(this.TypeTextBox, 0);
        	this.Controls.SetChildIndex(this.label1, 0);
        	this.Controls.SetChildIndex(this.label2, 0);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox TypeTextBox;

        #endregion

        protected System.Windows.Forms.TextBox NameTextBox;

    }
}
