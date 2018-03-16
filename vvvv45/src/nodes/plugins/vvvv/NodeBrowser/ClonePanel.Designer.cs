
namespace VVVV.Nodes.NodeBrowser
{
    partial class ClonePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClonePanel));
            this.FNameTextBox = new System.Windows.Forms.TextBox();
            this.FCategoryTextBox = new System.Windows.Forms.TextBox();
            this.FVersionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.FCloneButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FCancelButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.FPathTextBox = new System.Windows.Forms.TextBox();
            this.FPathButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // FNameTextBox
            // 
            this.FNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FNameTextBox.Location = new System.Drawing.Point(58, 3);
            this.FNameTextBox.Name = "FNameTextBox";
            this.FNameTextBox.Size = new System.Drawing.Size(240, 20);
            this.FNameTextBox.TabIndex = 0;
            this.FNameTextBox.TextChanged += new System.EventHandler(this.FNameTextBoxTextChanged);
            // 
            // FCategoryTextBox
            // 
            this.FCategoryTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FCategoryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FCategoryTextBox.Location = new System.Drawing.Point(58, 29);
            this.FCategoryTextBox.Name = "FCategoryTextBox";
            this.FCategoryTextBox.Size = new System.Drawing.Size(240, 20);
            this.FCategoryTextBox.TabIndex = 1;
            this.FCategoryTextBox.TextChanged += new System.EventHandler(this.FCategoryTextBoxTextChanged);
            // 
            // FVersionTextBox
            // 
            this.FVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FVersionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FVersionTextBox.Location = new System.Drawing.Point(58, 55);
            this.FVersionTextBox.Name = "FVersionTextBox";
            this.FVersionTextBox.Size = new System.Drawing.Size(240, 20);
            this.FVersionTextBox.TabIndex = 2;
            this.FVersionTextBox.TextChanged += new System.EventHandler(this.FVersionTextBoxTextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(363, 78);
            this.label1.TabIndex = 99;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // FCloneButton
            // 
            this.FCloneButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.FCloneButton.Enabled = false;
            this.FCloneButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FCloneButton.Location = new System.Drawing.Point(3, 3);
            this.FCloneButton.Name = "FCloneButton";
            this.FCloneButton.Size = new System.Drawing.Size(90, 23);
            this.FCloneButton.TabIndex = 4;
            this.FCloneButton.Text = "Clone";
            this.FCloneButton.UseVisualStyleBackColor = true;
            this.FCloneButton.Click += new System.EventHandler(this.FCloneButtonClick);
            this.FCloneButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FCloneButtonKeyPress);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Name";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Category";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Version";
            // 
            // FCancelButton
            // 
            this.FCancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.FCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FCancelButton.Location = new System.Drawing.Point(270, 3);
            this.FCancelButton.Name = "FCancelButton";
            this.FCancelButton.Size = new System.Drawing.Size(90, 23);
            this.FCancelButton.TabIndex = 5;
            this.FCancelButton.Text = "Cancel";
            this.FCancelButton.UseVisualStyleBackColor = true;
            this.FCancelButton.Click += new System.EventHandler(this.FCancelButtonClick);
            this.FCancelButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FCancelButtonKeyPress);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(370, 303);
            this.flowLayoutPanel1.TabIndex = 101;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.FNameTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.FVersionTextBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.FCategoryTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.FPathTextBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.FPathButton, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 81);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(363, 107);
            this.tableLayoutPanel1.TabIndex = 101;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Path";
            // 
            // FPathTextBox
            // 
            this.FPathTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FPathTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FPathTextBox.Location = new System.Drawing.Point(58, 81);
            this.FPathTextBox.Name = "FPathTextBox";
            this.FPathTextBox.Size = new System.Drawing.Size(240, 20);
            this.FPathTextBox.TabIndex = 3;
            // 
            // FPathButton
            // 
            this.FPathButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.FPathButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FPathButton.Location = new System.Drawing.Point(308, 81);
            this.FPathButton.Name = "FPathButton";
            this.FPathButton.Size = new System.Drawing.Size(52, 23);
            this.FPathButton.TabIndex = 10;
            this.FPathButton.Text = "...";
            this.FPathButton.UseVisualStyleBackColor = true;
            this.FPathButton.Click += new System.EventHandler(this.FPathButtonClick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.FCloneButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.FCancelButton, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 194);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(363, 29);
            this.tableLayoutPanel2.TabIndex = 102;
            // 
            // ClonePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "ClonePanel";
            this.Size = new System.Drawing.Size(370, 303);
            this.VisibleChanged += new System.EventHandler(this.ClonePanelVisibleChanged);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Button FPathButton;
        private System.Windows.Forms.TextBox FPathTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
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
