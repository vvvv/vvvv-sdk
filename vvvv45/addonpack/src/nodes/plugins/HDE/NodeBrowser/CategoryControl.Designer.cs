/*
 * Erstellt mit SharpDevelop.
 * Benutzer: joreg
 * Datum: 27.04.2010
 * Zeit: 15:04
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
namespace VVVV.Nodes
{
    partial class CategoryControl
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
        	this.NodeListBox = new System.Windows.Forms.ListBox();
        	this.CategoryLabel = new System.Windows.Forms.Label();
        	this.topPanel = new System.Windows.Forms.Panel();
        	this.topPanel.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// NodeListBox
        	// 
        	this.NodeListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.NodeListBox.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.NodeListBox.FormattingEnabled = true;
        	this.NodeListBox.IntegralHeight = false;
        	this.NodeListBox.Location = new System.Drawing.Point(0, 19);
        	this.NodeListBox.Name = "NodeListBox";
        	this.NodeListBox.Size = new System.Drawing.Size(193, 130);
        	this.NodeListBox.Sorted = true;
        	this.NodeListBox.TabIndex = 3;
        	// 
        	// CategoryLabel
        	// 
        	this.CategoryLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.CategoryLabel.Location = new System.Drawing.Point(0, 0);
        	this.CategoryLabel.Name = "CategoryLabel";
        	this.CategoryLabel.Size = new System.Drawing.Size(193, 19);
        	this.CategoryLabel.TabIndex = 0;
        	this.CategoryLabel.Text = "label1";
        	this.CategoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	this.CategoryLabel.Click += new System.EventHandler(this.CategoryLabelClick);
        	// 
        	// topPanel
        	// 
        	this.topPanel.Controls.Add(this.CategoryLabel);
        	this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
        	this.topPanel.Location = new System.Drawing.Point(0, 0);
        	this.topPanel.Name = "topPanel";
        	this.topPanel.Size = new System.Drawing.Size(193, 19);
        	this.topPanel.TabIndex = 0;
        	// 
        	// CategoryControl
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.Controls.Add(this.NodeListBox);
        	this.Controls.Add(this.topPanel);
        	this.Name = "CategoryControl";
        	this.Size = new System.Drawing.Size(193, 149);
        	this.topPanel.ResumeLayout(false);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.ListBox NodeListBox;
        private System.Windows.Forms.Label CategoryLabel;
    }
}
