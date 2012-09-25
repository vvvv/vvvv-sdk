namespace VVVV.HDE.Viewer.WinFormsViewer
{
	partial class TreeViewer
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
				
				FontChanged -= UserControl_FontChanged;
				
				this.FTreeView.NodeMouseDoubleClick -= this.TreeViewNodeMouseDoubleClick;
				this.FTreeView.DrawNode -= this.FTreeViewDrawNode;
				this.FTreeView.AfterLabelEdit -= this.TreeViewAfterLabelEdit;
				this.FTreeView.VisibleChanged -= this.FTreeViewVisibleChanged;
				this.FTreeView.DragDrop -= this.TreeViewDragDrop;
				this.FTreeView.AfterSelect -= this.FTreeViewAfterSelect;
				this.FTreeView.MouseMove -= this.FTreeViewMouseMove;
				this.FTreeView.MouseDown -= this.FTreeViewMouseDown;
				this.FTreeView.NodeMouseClick -= this.TreeViewNodeMouseClick;
				this.FTreeView.BeforeLabelEdit -= this.TreeViewBeforeLabelEdit;
				this.FTreeView.KeyDown -= this.FTreeViewKeyDown;
				this.FTreeView.ItemDrag -= this.TreeViewItemDrag;
				this.FTreeView.DragOver -= this.TreeViewDragOver;
				
				this.FToolTip.Popup -= this.ToolTipPopupHandler;
				
				if (FSynchronizer != null)
					FSynchronizer.Dispose();
				
				foreach (MapperTreeNode mapperTreeNode in FTreeView.Nodes)
				{
					mapperTreeNode.Dispose();
				}
				
				if (FRootMapper != null)
					FRootMapper.Dispose();
				
				if (ParentForm != null)
				{
				    ParentForm.Activated -= ParentForm_Activated;
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
			this.FTreeView = new VVVV.HDE.Viewer.WinFormsViewer.DoubleBufferedTreeView();
			this.FToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// FTreeView
			// 
			this.FTreeView.AllowDrop = true;
			this.FTreeView.BackColor = System.Drawing.Color.Silver;
			this.FTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
			this.FTreeView.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FTreeView.FullRowSelect = true;
			this.FTreeView.HideSelection = false;
			this.FTreeView.LabelEdit = true;
			this.FTreeView.Location = new System.Drawing.Point(0, 0);
			this.FTreeView.Name = "FTreeView";
			this.FTreeView.Size = new System.Drawing.Size(226, 243);
			this.FTreeView.TabIndex = 0;
			this.FTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewNodeMouseDoubleClick);
			this.FTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.FTreeViewDrawNode);
			this.FTreeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeViewAfterLabelEdit);
			this.FTreeView.VisibleChanged += new System.EventHandler(this.FTreeViewVisibleChanged);
			this.FTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeViewDragDrop);
			this.FTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FTreeViewAfterSelect);
			this.FTreeView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FTreeViewMouseMove);
			this.FTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FTreeViewMouseDown);
			this.FTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewNodeMouseClick);
			this.FTreeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeViewBeforeLabelEdit);
			this.FTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FTreeViewKeyDown);
			this.FTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TreeViewItemDrag);
			this.FTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeViewDragOver);
			this.FTreeView.MouseLeave += new System.EventHandler(this.FTreeViewMouseLeave);
			// 
			// FToolTip
			// 
			this.FToolTip.BackColor = System.Drawing.Color.Gray;
			this.FToolTip.ForeColor = System.Drawing.Color.White;
			this.FToolTip.Popup += new System.Windows.Forms.PopupEventHandler(this.ToolTipPopupHandler);
			// 
			// TreeViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FTreeView);
			this.Name = "TreeViewer";
			this.Size = new System.Drawing.Size(226, 243);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ToolTip FToolTip;
		private VVVV.HDE.Viewer.WinFormsViewer.DoubleBufferedTreeView FTreeView;
		
		void FTreeViewMouseLeave(object sender, System.EventArgs e)
		{
		    FToolTip.Hide(this);
		}
	}
}
