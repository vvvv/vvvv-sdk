
namespace VVVV.Nodes
{
	partial class CodeEditorPlugin
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
			this.FImageList = new System.Windows.Forms.ImageList(this.components);
			this.FSplitContainer = new System.Windows.Forms.SplitContainer();
			this.FProjectTreeViewer = new VVVV.HDE.Viewer.TreeViewer();
			this.FTabControl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.textEditorControl1 = new ICSharpCode.TextEditor.TextEditorControl();
			this.FStatusStrip.SuspendLayout();
			this.FSplitContainer.Panel1.SuspendLayout();
			this.FSplitContainer.Panel2.SuspendLayout();
			this.FSplitContainer.SuspendLayout();
			this.FTabControl.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// FStatusStrip
			// 
			this.FStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.FStatusLabel});
			this.FStatusStrip.Location = new System.Drawing.Point(0, 635);
			this.FStatusStrip.Name = "FStatusStrip";
			this.FStatusStrip.Size = new System.Drawing.Size(906, 22);
			this.FStatusStrip.TabIndex = 2;
			this.FStatusStrip.Text = "statusStrip1";
			// 
			// FStatusLabel
			// 
			this.FStatusLabel.Name = "FStatusLabel";
			this.FStatusLabel.Size = new System.Drawing.Size(38, 17);
			this.FStatusLabel.Text = "Ready";
			// 
			// FImageList
			// 
			this.FImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.FImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.FImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// FSplitContainer
			// 
			this.FSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.FSplitContainer.Location = new System.Drawing.Point(0, 0);
			this.FSplitContainer.Name = "FSplitContainer";
			// 
			// FSplitContainer.Panel1
			// 
			this.FSplitContainer.Panel1.Controls.Add(this.FProjectTreeViewer);
			// 
			// FSplitContainer.Panel2
			// 
			this.FSplitContainer.Panel2.Controls.Add(this.FTabControl);
			this.FSplitContainer.Size = new System.Drawing.Size(906, 635);
			this.FSplitContainer.SplitterDistance = 302;
			this.FSplitContainer.TabIndex = 1;
			// 
			// FProjectTreeViewer
			// 
			this.FProjectTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FProjectTreeViewer.Location = new System.Drawing.Point(0, 0);
			this.FProjectTreeViewer.Name = "FProjectTreeViewer";
			this.FProjectTreeViewer.ShowRoot = false;
			this.FProjectTreeViewer.Size = new System.Drawing.Size(302, 635);
			this.FProjectTreeViewer.TabIndex = 0;
			// 
			// FTabControl
			// 
			this.FTabControl.Controls.Add(this.tabPage1);
			this.FTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTabControl.Location = new System.Drawing.Point(0, 0);
			this.FTabControl.Name = "FTabControl";
			this.FTabControl.SelectedIndex = 0;
			this.FTabControl.Size = new System.Drawing.Size(600, 635);
			this.FTabControl.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.textEditorControl1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(592, 609);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// textEditorControl1
			// 
			this.textEditorControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textEditorControl1.IsReadOnly = false;
			this.textEditorControl1.Location = new System.Drawing.Point(0, 0);
			this.textEditorControl1.Name = "textEditorControl1";
			this.textEditorControl1.Size = new System.Drawing.Size(592, 609);
			this.textEditorControl1.TabIndex = 0;
			this.textEditorControl1.Text = "textEditorControl1";
			// 
			// CodeEditorPlugin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FSplitContainer);
			this.Controls.Add(this.FStatusStrip);
			this.Name = "CodeEditorPlugin";
			this.Size = new System.Drawing.Size(906, 657);
			this.FStatusStrip.ResumeLayout(false);
			this.FStatusStrip.PerformLayout();
			this.FSplitContainer.Panel1.ResumeLayout(false);
			this.FSplitContainer.Panel2.ResumeLayout(false);
			this.FSplitContainer.ResumeLayout(false);
			this.FTabControl.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private ICSharpCode.TextEditor.TextEditorControl textEditorControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.ToolStripStatusLabel FStatusLabel;
		private System.Windows.Forms.TabControl FTabControl;
		private VVVV.HDE.Viewer.TreeViewer FProjectTreeViewer;
		private System.Windows.Forms.SplitContainer FSplitContainer;
		private System.Windows.Forms.ImageList FImageList;
		private System.Windows.Forms.StatusStrip FStatusStrip;
	}
}
