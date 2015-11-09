
namespace VVVV.HDE.CodeEditor.Gui
{
	partial class SaveDialog
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
				
				FSaveButton.Click -= FSaveButtonClick;
				FCloseButton.Click -= FCloseButtonClick;
				FCancelButton.Click -= FCancelButtonClick;
				this.Load -= SaveDialogLoad;
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
			this.FMessageLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.FCancelButton = new System.Windows.Forms.Button();
			this.FCloseButton = new System.Windows.Forms.Button();
			this.FSaveButton = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// FMessageLabel
			// 
			this.FMessageLabel.AutoSize = true;
			this.FMessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FMessageLabel.Location = new System.Drawing.Point(12, 16);
			this.FMessageLabel.Margin = new System.Windows.Forms.Padding(0);
			this.FMessageLabel.Name = "FMessageLabel";
			this.FMessageLabel.Size = new System.Drawing.Size(74, 20);
			this.FMessageLabel.TabIndex = 1;
			this.FMessageLabel.Text = "Filename";
			this.FMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.FCancelButton);
			this.panel1.Controls.Add(this.FCloseButton);
			this.panel1.Controls.Add(this.FSaveButton);
			this.panel1.Location = new System.Drawing.Point(0, 55);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(324, 101);
			this.panel1.TabIndex = 2;
			// 
			// FCancelButton
			// 
			this.FCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.FCancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FCancelButton.Location = new System.Drawing.Point(212, 0);
			this.FCancelButton.Name = "FCancelButton";
			this.FCancelButton.Size = new System.Drawing.Size(100, 90);
			this.FCancelButton.TabIndex = 3;
			this.FCancelButton.Text = "&Cancel";
			this.FCancelButton.UseVisualStyleBackColor = true;
			this.FCancelButton.Click += new System.EventHandler(this.FCancelButtonClick);
			// 
			// FCloseButton
			// 
			this.FCloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.FCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FCloseButton.Location = new System.Drawing.Point(112, 0);
			this.FCloseButton.Name = "FCloseButton";
			this.FCloseButton.Size = new System.Drawing.Size(100, 90);
			this.FCloseButton.TabIndex = 2;
			this.FCloseButton.Text = "Don\'t Save && Close";
			this.FCloseButton.UseVisualStyleBackColor = true;
			this.FCloseButton.Click += new System.EventHandler(this.FCloseButtonClick);
			// 
			// FSaveButton
			// 
			this.FSaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.FSaveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FSaveButton.Location = new System.Drawing.Point(12, 0);
			this.FSaveButton.Name = "FSaveButton";
			this.FSaveButton.Size = new System.Drawing.Size(100, 90);
			this.FSaveButton.TabIndex = 1;
			this.FSaveButton.Text = "&Save && Close";
			this.FSaveButton.UseVisualStyleBackColor = true;
			this.FSaveButton.Click += new System.EventHandler(this.FSaveButtonClick);
			// 
			// SaveDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(324, 156);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.FMessageLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.SaveDialogLoad);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button FCloseButton;
		private System.Windows.Forms.Button FCancelButton;
		private System.Windows.Forms.Label FMessageLabel;
		private System.Windows.Forms.Button FSaveButton;
	}
}
