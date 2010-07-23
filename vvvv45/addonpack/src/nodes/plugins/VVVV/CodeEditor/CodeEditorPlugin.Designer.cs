
namespace VVVV.HDE.CodeEditor
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
			this.FSplitContainer1 = new System.Windows.Forms.SplitContainer();
			this.FProjectTreeViewer = new VVVV.HDE.Viewer.WinFormsViewer.TreeViewer();
			this.FSplitContainer2 = new System.Windows.Forms.SplitContainer();
			this.FTabControl = new System.Windows.Forms.TabControl();
			this.FErrorTableViewer = new VVVV.HDE.Viewer.WinFormsViewer.TableViewer();
			this.FStatusStrip.SuspendLayout();
			this.FSplitContainer1.Panel1.SuspendLayout();
			this.FSplitContainer1.Panel2.SuspendLayout();
			this.FSplitContainer1.SuspendLayout();
			this.FSplitContainer2.Panel1.SuspendLayout();
			this.FSplitContainer2.Panel2.SuspendLayout();
			this.FSplitContainer2.SuspendLayout();
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
			// FSplitContainer1
			// 
			this.FSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FSplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.FSplitContainer1.Location = new System.Drawing.Point(0, 0);
			this.FSplitContainer1.Name = "FSplitContainer1";
			// 
			// FSplitContainer1.Panel1
			// 
			this.FSplitContainer1.Panel1.Controls.Add(this.FProjectTreeViewer);
			// 
			// FSplitContainer1.Panel2
			// 
			this.FSplitContainer1.Panel2.Controls.Add(this.FSplitContainer2);
			this.FSplitContainer1.Size = new System.Drawing.Size(906, 635);
			this.FSplitContainer1.SplitterDistance = 302;
			this.FSplitContainer1.TabIndex = 2;
			// 
			// FProjectTreeViewer
			// 
			this.FProjectTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FProjectTreeViewer.FlatStyle = false;
			this.FProjectTreeViewer.Location = new System.Drawing.Point(0, 0);
			this.FProjectTreeViewer.Name = "FProjectTreeViewer";
			this.FProjectTreeViewer.ShowLines = true;
			this.FProjectTreeViewer.ShowPlusMinus = true;
			this.FProjectTreeViewer.ShowRoot = true;
			this.FProjectTreeViewer.ShowRootLines = true;
			this.FProjectTreeViewer.ShowTooltip = false;
			this.FProjectTreeViewer.Size = new System.Drawing.Size(302, 635);
			this.FProjectTreeViewer.TabIndex = 0;
			this.FProjectTreeViewer.DoubleClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FProjectTreeViewerDoubleClick);
			// 
			// FSplitContainer2
			// 
			this.FSplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FSplitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.FSplitContainer2.Location = new System.Drawing.Point(0, 0);
			this.FSplitContainer2.Name = "FSplitContainer2";
			this.FSplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// FSplitContainer2.Panel1
			// 
			this.FSplitContainer2.Panel1.Controls.Add(this.FTabControl);
			this.FSplitContainer2.Panel1MinSize = 200;
			// 
			// FSplitContainer2.Panel2
			// 
			this.FSplitContainer2.Panel2.Controls.Add(this.FErrorTableViewer);
			this.FSplitContainer2.Panel2Collapsed = true;
			this.FSplitContainer2.Size = new System.Drawing.Size(600, 635);
			this.FSplitContainer2.SplitterDistance = 493;
			this.FSplitContainer2.TabIndex = 1;
			// 
			// FTabControl
			// 
			this.FTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTabControl.Location = new System.Drawing.Point(0, 0);
			this.FTabControl.Name = "FTabControl";
			this.FTabControl.SelectedIndex = 0;
			this.FTabControl.Size = new System.Drawing.Size(600, 635);
			this.FTabControl.TabIndex = 0;
			// 
			// FErrorTableViewer
			// 
			this.FErrorTableViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FErrorTableViewer.Location = new System.Drawing.Point(0, 0);
			this.FErrorTableViewer.Name = "FErrorTableViewer";
			this.FErrorTableViewer.Size = new System.Drawing.Size(600, 138);
			this.FErrorTableViewer.TabIndex = 0;
			// 
			// CodeEditorPlugin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FSplitContainer1);
			this.Controls.Add(this.FStatusStrip);
			this.Name = "CodeEditorPlugin";
			this.Size = new System.Drawing.Size(906, 657);
			this.FStatusStrip.ResumeLayout(false);
			this.FStatusStrip.PerformLayout();
			this.FSplitContainer1.Panel1.ResumeLayout(false);
			this.FSplitContainer1.Panel2.ResumeLayout(false);
			this.FSplitContainer1.ResumeLayout(false);
			this.FSplitContainer2.Panel1.ResumeLayout(false);
			this.FSplitContainer2.Panel2.ResumeLayout(false);
			this.FSplitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private VVVV.HDE.Viewer.WinFormsViewer.TableViewer FErrorTableViewer;
		private System.Windows.Forms.TabControl FTabControl;
		private System.Windows.Forms.ToolStripStatusLabel FStatusLabel;
		private VVVV.HDE.Viewer.WinFormsViewer.TreeViewer FProjectTreeViewer;
		private System.Windows.Forms.SplitContainer FSplitContainer1;
		private System.Windows.Forms.SplitContainer FSplitContainer2;
		private System.Windows.Forms.ImageList FImageList;
		private System.Windows.Forms.StatusStrip FStatusStrip;
	}
}
