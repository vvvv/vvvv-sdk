
namespace VVVV.Nodes
{
    partial class CloneInfo
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the control.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloneInfo));
        	this.FNameTextBox = new System.Windows.Forms.TextBox();
        	this.FCategoryTextBox = new System.Windows.Forms.TextBox();
        	this.FVersionTextBox = new System.Windows.Forms.TextBox();
        	this.label1 = new System.Windows.Forms.Label();
        	this.FCloneButton = new System.Windows.Forms.Button();
        	this.label2 = new System.Windows.Forms.Label();
        	this.label3 = new System.Windows.Forms.Label();
        	this.label4 = new System.Windows.Forms.Label();
        	this.FCancelButton = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// FNameTextBox
        	// 
        	this.FNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FNameTextBox.Location = new System.Drawing.Point(64, 136);
        	this.FNameTextBox.Name = "FNameTextBox";
        	this.FNameTextBox.Size = new System.Drawing.Size(155, 20);
        	this.FNameTextBox.TabIndex = 0;
        	this.FNameTextBox.TextChanged += new System.EventHandler(this.FNameTextBoxTextChanged);
        	// 
        	// FCategoryTextBox
        	// 
        	this.FCategoryTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FCategoryTextBox.Location = new System.Drawing.Point(64, 162);
        	this.FCategoryTextBox.Name = "FCategoryTextBox";
        	this.FCategoryTextBox.Size = new System.Drawing.Size(155, 20);
        	this.FCategoryTextBox.TabIndex = 1;
        	this.FCategoryTextBox.TextChanged += new System.EventHandler(this.FCategoryTextBoxTextChanged);
        	// 
        	// FVersionTextBox
        	// 
        	this.FVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FVersionTextBox.Location = new System.Drawing.Point(64, 188);
        	this.FVersionTextBox.Name = "FVersionTextBox";
        	this.FVersionTextBox.Size = new System.Drawing.Size(155, 20);
        	this.FVersionTextBox.TabIndex = 2;
        	this.FVersionTextBox.TextChanged += new System.EventHandler(this.FVersionTextBoxTextChanged);
        	// 
        	// label1
        	// 
        	this.label1.Location = new System.Drawing.Point(8, 11);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(211, 122);
        	this.label1.TabIndex = 99;
        	this.label1.Text = resources.GetString("label1.Text");
        	// 
        	// FCloneButton
        	// 
        	this.FCloneButton.Enabled = false;
        	this.FCloneButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FCloneButton.Location = new System.Drawing.Point(8, 214);
        	this.FCloneButton.Name = "FCloneButton";
        	this.FCloneButton.Size = new System.Drawing.Size(90, 23);
        	this.FCloneButton.TabIndex = 4;
        	this.FCloneButton.Text = "Clone";
        	this.FCloneButton.UseVisualStyleBackColor = true;
        	this.FCloneButton.Click += new System.EventHandler(this.FCloneButtonClick);
        	// 
        	// label2
        	// 
        	this.label2.Location = new System.Drawing.Point(8, 138);
        	this.label2.Name = "label2";
        	this.label2.Size = new System.Drawing.Size(47, 20);
        	this.label2.TabIndex = 5;
        	this.label2.Text = "Name";
        	// 
        	// label3
        	// 
        	this.label3.Location = new System.Drawing.Point(8, 164);
        	this.label3.Name = "label3";
        	this.label3.Size = new System.Drawing.Size(55, 20);
        	this.label3.TabIndex = 6;
        	this.label3.Text = "Category";
        	// 
        	// label4
        	// 
        	this.label4.Location = new System.Drawing.Point(8, 190);
        	this.label4.Name = "label4";
        	this.label4.Size = new System.Drawing.Size(55, 20);
        	this.label4.TabIndex = 7;
        	this.label4.Text = "Version";
        	// 
        	// FCancelButton
        	// 
        	this.FCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FCancelButton.Location = new System.Drawing.Point(129, 214);
        	this.FCancelButton.Name = "FCancelButton";
        	this.FCancelButton.Size = new System.Drawing.Size(90, 23);
        	this.FCancelButton.TabIndex = 5;
        	this.FCancelButton.Text = "Cancel";
        	this.FCancelButton.UseVisualStyleBackColor = true;
        	this.FCancelButton.Click += new System.EventHandler(this.FCancelButtonClick);
        	// 
        	// CloneInfo
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.Silver;
        	this.Controls.Add(this.FCancelButton);
        	this.Controls.Add(this.label2);
        	this.Controls.Add(this.FCloneButton);
        	this.Controls.Add(this.label1);
        	this.Controls.Add(this.FVersionTextBox);
        	this.Controls.Add(this.FCategoryTextBox);
        	this.Controls.Add(this.FNameTextBox);
        	this.Controls.Add(this.label4);
        	this.Controls.Add(this.label3);
        	this.Name = "CloneInfo";
        	this.Size = new System.Drawing.Size(230, 247);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Button FCloneButton;
        private System.Windows.Forms.Button FCancelButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FVersionTextBox;
        private System.Windows.Forms.TextBox FCategoryTextBox;
        private System.Windows.Forms.TextBox FNameTextBox;
    }
}
