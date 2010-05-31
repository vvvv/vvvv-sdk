
namespace VVVV.Nodes
{
    partial class CaptionControl
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
        	this.SuspendLayout();
        	// 
        	// labelCaption
        	// 
        	this.labelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.labelCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelCaption.Location = new System.Drawing.Point(0, 0);
        	this.labelCaption.Name = "labelCaption";
        	this.labelCaption.Size = new System.Drawing.Size(148, 23);
        	this.labelCaption.TabIndex = 0;
        	this.labelCaption.Text = "Caption";
        	// 
        	// CaptionControl
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Controls.Add(this.labelCaption);
        	this.Name = "CaptionControl";
        	this.Size = new System.Drawing.Size(148, 23);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label labelCaption;
    }
}
