namespace VVVV.Nodes.NodeBrowser
{
	partial class TagPanel
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
			if (disposing) 
			{
				FToolTip.Popup -= ToolTipPopupHandler;
	            FRichTextBox.MouseWheel -= DoMouseWheel;
	            
				if (components != null) 
				{
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
            this.components = new System.ComponentModel.Container();
            this.FNodeCountLabel = new System.Windows.Forms.Label();
            this.FScrollBar = new System.Windows.Forms.VScrollBar();
            this.FToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.FRichTextBox = new VVVV.Nodes.NodeBrowser.RichTextBoxEx();
            this.SuspendLayout();
            // 
            // FNodeCountLabel
            // 
            this.FNodeCountLabel.BackColor = System.Drawing.Color.Silver;
            this.FNodeCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FNodeCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FNodeCountLabel.Location = new System.Drawing.Point(0, 656);
            this.FNodeCountLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.FNodeCountLabel.Name = "FNodeCountLabel";
            this.FNodeCountLabel.Size = new System.Drawing.Size(466, 27);
            this.FNodeCountLabel.TabIndex = 7;
            // 
            // FScrollBar
            // 
            this.FScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.FScrollBar.Location = new System.Drawing.Point(449, 0);
            this.FScrollBar.Name = "FScrollBar";
            this.FScrollBar.Size = new System.Drawing.Size(17, 656);
            this.FScrollBar.TabIndex = 8;
            this.FScrollBar.Value = 100;
            this.FScrollBar.ValueChanged += new System.EventHandler(this.FScrollBarValueChanged);
            // 
            // FToolTip
            // 
            this.FToolTip.UseAnimation = false;
            this.FToolTip.UseFading = false;
            // 
            // FRichTextBox
            // 
            this.FRichTextBox.BackColor = System.Drawing.Color.Silver;
            this.FRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FRichTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.FRichTextBox.DetectUrls = false;
            this.FRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FRichTextBox.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.FRichTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.FRichTextBox.Name = "FRichTextBox";
            this.FRichTextBox.ReadOnly = true;
            this.FRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.FRichTextBox.Size = new System.Drawing.Size(449, 656);
            this.FRichTextBox.TabIndex = 10;
            this.FRichTextBox.TabStop = false;
            this.FRichTextBox.Text = "";
            this.FRichTextBox.WordWrap = false;
            this.FRichTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
            this.FRichTextBox.MouseLeave += new System.EventHandler(this.FRichTextBoxMouseLeave);
            this.FRichTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
            this.FRichTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseUp);
            // 
            // TagPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FRichTextBox);
            this.Controls.Add(this.FScrollBar);
            this.Controls.Add(this.FNodeCountLabel);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "TagPanel";
            this.Size = new System.Drawing.Size(466, 683);
            this.VisibleChanged += new System.EventHandler(this.TagPanelVisibleChanged);
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.ToolTip FToolTip;
		private VVVV.Nodes.NodeBrowser.RichTextBoxEx FRichTextBox;
		private System.Windows.Forms.VScrollBar FScrollBar;
		private System.Windows.Forms.Label FNodeCountLabel;
	}
}
