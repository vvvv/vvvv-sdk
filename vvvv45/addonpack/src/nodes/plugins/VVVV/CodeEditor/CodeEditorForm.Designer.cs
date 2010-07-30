
namespace VVVV.HDE.CodeEditor
{
	partial class CodeEditorForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.FStatusStrip = new System.Windows.Forms.StatusStrip();
			this.FStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.FSplitContainer = new System.Windows.Forms.SplitContainer();
			this.FTabControl = new System.Windows.Forms.TabControl();
			this.FErrorTableViewer = new VVVV.HDE.Viewer.WinFormsViewer.TableViewer();
			this.FImageList = new System.Windows.Forms.ImageList(this.components);
			this.FStatusStrip.SuspendLayout();
			this.FSplitContainer.Panel1.SuspendLayout();
			this.FSplitContainer.Panel2.SuspendLayout();
			this.FSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// FStatusStrip
			// 
			this.FStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.FStatusLabel});
			this.FStatusStrip.Location = new System.Drawing.Point(0, 454);
			this.FStatusStrip.Name = "FStatusStrip";
			this.FStatusStrip.Size = new System.Drawing.Size(881, 22);
			this.FStatusStrip.TabIndex = 0;
			this.FStatusStrip.Text = "statusStrip1";
			// 
			// FStatusLabel
			// 
			this.FStatusLabel.Name = "FStatusLabel";
			this.FStatusLabel.Size = new System.Drawing.Size(39, 17);
			this.FStatusLabel.Text = "Ready";
			// 
			// FSplitContainer
			// 
			this.FSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FSplitContainer.Location = new System.Drawing.Point(0, 0);
			this.FSplitContainer.Name = "FSplitContainer";
			this.FSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// FSplitContainer.Panel1
			// 
			this.FSplitContainer.Panel1.Controls.Add(this.FTabControl);
			// 
			// FSplitContainer.Panel2
			// 
			this.FSplitContainer.Panel2.Controls.Add(this.FErrorTableViewer);
			this.FSplitContainer.Panel2Collapsed = true;
			this.FSplitContainer.Size = new System.Drawing.Size(881, 454);
			this.FSplitContainer.SplitterDistance = 293;
			this.FSplitContainer.TabIndex = 1;
			// 
			// FTabControl
			// 
			this.FTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.FTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTabControl.Location = new System.Drawing.Point(0, 0);
			this.FTabControl.Name = "FTabControl";
			this.FTabControl.SelectedIndex = 0;
			this.FTabControl.Size = new System.Drawing.Size(881, 454);
			this.FTabControl.TabIndex = 0;
			// 
			// FErrorTableViewer
			// 
			this.FErrorTableViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FErrorTableViewer.Location = new System.Drawing.Point(0, 0);
			this.FErrorTableViewer.Name = "FErrorTableViewer";
			this.FErrorTableViewer.RowHeight = 16;
			this.FErrorTableViewer.Size = new System.Drawing.Size(150, 46);
			this.FErrorTableViewer.TabIndex = 0;
			// 
			// FImageList
			// 
			this.FImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.FImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.FImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// CodeEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(881, 476);
			this.ControlBox = false;
			this.Controls.Add(this.FSplitContainer);
			this.Controls.Add(this.FStatusStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CodeEditorForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "CodeEditorForm";
			this.TopMost = true;
			this.FStatusStrip.ResumeLayout(false);
			this.FStatusStrip.PerformLayout();
			this.FSplitContainer.Panel1.ResumeLayout(false);
			this.FSplitContainer.Panel2.ResumeLayout(false);
			this.FSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		internal System.Windows.Forms.ToolStripStatusLabel FStatusLabel;
		private System.Windows.Forms.ImageList FImageList;
		private System.Windows.Forms.TabControl FTabControl;
		private VVVV.HDE.Viewer.WinFormsViewer.TableViewer FErrorTableViewer;
		private System.Windows.Forms.SplitContainer FSplitContainer;
		private System.Windows.Forms.StatusStrip FStatusStrip;
	}
}
