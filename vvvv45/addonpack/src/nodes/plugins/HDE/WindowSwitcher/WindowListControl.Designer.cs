
namespace VVVV.Nodes
{
    partial class WindowListControl
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
        	this.labelCaption = new System.Windows.Forms.Label();
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.SuspendLayout();
        	// 
        	// labelCaption
        	// 
        	this.labelCaption.BackColor = System.Drawing.Color.Silver;
        	this.labelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.labelCaption.Enabled = false;
        	this.labelCaption.Location = new System.Drawing.Point(13, 0);
        	this.labelCaption.Name = "labelCaption";
        	this.labelCaption.Size = new System.Drawing.Size(222, 18);
        	this.labelCaption.TabIndex = 0;
        	this.labelCaption.Text = "labelCaption";
        	// 
        	// panel1
        	// 
        	this.panel1.BackColor = System.Drawing.Color.Silver;
        	this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
        	this.panel1.Enabled = false;
        	this.panel1.Location = new System.Drawing.Point(0, 0);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(13, 18);
        	this.panel1.TabIndex = 1;
        	// 
        	// WindowListControl
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Controls.Add(this.labelCaption);
        	this.Controls.Add(this.panel1);
        	this.Name = "WindowListControl";
        	this.Size = new System.Drawing.Size(235, 18);
        	this.MouseLeave += new System.EventHandler(this.WindowListControlMouseLeave);
        	this.MouseEnter += new System.EventHandler(this.WindowListControlMouseEnter);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelCaption;
    }
}
