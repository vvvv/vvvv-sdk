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
	            FTagsTextBox.MouseWheel -= FTagsTextBoxMouseWheel;
	            FRichTextBox.MouseWheel -= FTagsTextBoxMouseWheel;
	            
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
			this.FTagsTextBox = new System.Windows.Forms.TextBox();
			this.FNodeCountLabel = new System.Windows.Forms.Label();
			this.FScrollBar = new System.Windows.Forms.VScrollBar();
			this.FNodeTypePanel = new VVVV.Nodes.DoubleBufferedPanel();
			this.FRichTextBox = new VVVV.Nodes.NodeBrowser.RichTextBoxEx();
			this.FToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// FTagsTextBox
			// 
			this.FTagsTextBox.AcceptsTab = true;
			this.FTagsTextBox.BackColor = System.Drawing.Color.Silver;
			this.FTagsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FTagsTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FTagsTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FTagsTextBox.Location = new System.Drawing.Point(0, 0);
			this.FTagsTextBox.Multiline = true;
			this.FTagsTextBox.Name = "FTagsTextBox";
			this.FTagsTextBox.Size = new System.Drawing.Size(233, 20);
			this.FTagsTextBox.TabIndex = 6;
			this.FTagsTextBox.TabStop = false;
			this.FTagsTextBox.TextChanged += new System.EventHandler(this.FTagsTextBoxTextChanged);
			this.FTagsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FTagsTextBoxKeyDown);
			this.FTagsTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FTagsTextBoxMouseUp);
			// 
			// FNodeCountLabel
			// 
			this.FNodeCountLabel.BackColor = System.Drawing.Color.Silver;
			this.FNodeCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FNodeCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FNodeCountLabel.Location = new System.Drawing.Point(0, 340);
			this.FNodeCountLabel.Name = "FNodeCountLabel";
			this.FNodeCountLabel.Size = new System.Drawing.Size(233, 15);
			this.FNodeCountLabel.TabIndex = 7;
			// 
			// FScrollBar
			// 
			this.FScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
			this.FScrollBar.Location = new System.Drawing.Point(216, 20);
			this.FScrollBar.Name = "FScrollBar";
			this.FScrollBar.Size = new System.Drawing.Size(17, 320);
			this.FScrollBar.TabIndex = 8;
			this.FScrollBar.Value = 100;
			this.FScrollBar.ValueChanged += new System.EventHandler(this.FScrollBarValueChanged);
			// 
			// FNodeTypePanel
			// 
			this.FNodeTypePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.FNodeTypePanel.Location = new System.Drawing.Point(0, 20);
			this.FNodeTypePanel.Name = "FNodeTypePanel";
			this.FNodeTypePanel.Size = new System.Drawing.Size(20, 320);
			this.FNodeTypePanel.TabIndex = 9;
			this.FNodeTypePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.FNodeTypePanelPaint);
			this.FNodeTypePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FNodeTypePanelMouseDown);
			this.FNodeTypePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
			// 
			// FRichTextBox
			// 
			this.FRichTextBox.BackColor = System.Drawing.Color.Silver;
			this.FRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FRichTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.FRichTextBox.DetectUrls = false;
			this.FRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FRichTextBox.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FRichTextBox.Location = new System.Drawing.Point(20, 20);
			this.FRichTextBox.Name = "FRichTextBox";
			this.FRichTextBox.ReadOnly = true;
			this.FRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.FRichTextBox.Size = new System.Drawing.Size(196, 320);
			this.FRichTextBox.TabIndex = 10;
			this.FRichTextBox.TabStop = false;
			this.FRichTextBox.Text = "";
			this.FRichTextBox.WordWrap = false;
			this.FRichTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
			this.FRichTextBox.MouseLeave += new System.EventHandler(this.FRichTextBoxMouseLeave);
			this.FRichTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
			this.FRichTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseUp);
			// 
			// FToolTip
			// 
			this.FToolTip.UseAnimation = false;
			this.FToolTip.UseFading = false;
			// 
			// TagPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FRichTextBox);
			this.Controls.Add(this.FNodeTypePanel);
			this.Controls.Add(this.FScrollBar);
			this.Controls.Add(this.FNodeCountLabel);
			this.Controls.Add(this.FTagsTextBox);
			this.Name = "TagPanel";
			this.Size = new System.Drawing.Size(233, 355);
			this.VisibleChanged += new System.EventHandler(this.TagPanelVisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolTip FToolTip;
		private VVVV.Nodes.NodeBrowser.RichTextBoxEx FRichTextBox;
		private VVVV.Nodes.DoubleBufferedPanel FNodeTypePanel;
		private System.Windows.Forms.VScrollBar FScrollBar;
		private System.Windows.Forms.Label FNodeCountLabel;
		private System.Windows.Forms.TextBox FTagsTextBox;
	}
}
