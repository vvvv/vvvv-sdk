using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.Core.Viewer;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    public delegate void ClickHandler(ModelMapper sender, MouseEventArgs e);
    
    public partial class TreeViewer : Viewer, ILabelEditor, ISelectionProvider
    {
//		private MapperTreeNode FRootNode;
        private ModelMapper FRootMapper;
        private MapperTreeNode FLastTooltipNode;
        private Synchronizer<object, object> FSynchronizer;
        
        public new event ClickHandler Click;
        protected void OnClick(ModelMapper sender, MouseEventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }
        public new event ClickHandler DoubleClick;
        protected void OnDoubleClick(ModelMapper sender, MouseEventArgs e)
        {
            if (DoubleClick != null)
                DoubleClick(sender, e);
        }
        public new event ClickHandler MouseDown;
        protected virtual void OnMouseDown(ModelMapper sender, MouseEventArgs e)
        {
            if (MouseDown != null) {
                MouseDown(sender, e);
            }
        }
        
        public bool ShowRoot{get;set;}
        public bool ShowTooltip{get;set;}
        public bool FlatStyle{get;set;}
        public bool ShowPlusMinus{get{return FTreeView.ShowPlusMinus;}set{FTreeView.ShowPlusMinus = value;}}
        public bool ShowRootLines{get{return FTreeView.ShowRootLines;}set{FTreeView.ShowRootLines = value;}}
        public bool ShowLines{get{return FTreeView.ShowLines;}set{FTreeView.ShowLines = value;}}
        
        [Browsable(true), DefaultValue(typeof(System.Drawing.Color), "Silver")]
        public override Color BackColor
        {
            get
            {
                return FTreeView.BackColor;
            }
            set
            {
                FTreeView.BackColor = value;
            }
        }
        
        public TreeNode SelectedNode
        {
            get;
            private set;
        }
        
        private Color CHoverColor = Color.FromArgb(255, 216, 216, 216);
        
        #region initialization
        public TreeViewer()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            FontChanged += UserControl_FontChanged;
        }

        void UserControl_FontChanged(object sender, EventArgs e)
        {
            FTreeView.Font = Font;
        }
        
        private void ToolTipPopupHandler(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(Math.Min(e.ToolTipSize.Width, 300), e.ToolTipSize.Height);
        }
        
        #endregion initialization
        
        protected override void InitializeMappingRegistry(MappingRegistry registry)
        {
            registry.RegisterDefaultInstance<ILabelEditor>(this);
        }
        
        protected override void OnLoad(EventArgs e)
        {
            if (ParentForm != null)
            {
                ParentForm.Activated += ParentForm_Activated;
            }
            
            base.OnLoad(e);
        }
        
        void ParentForm_Activated(object sender, EventArgs e)
        {
            if (Registry.HasService<ISelectionService>())
            {
                var selectionService = Registry.LocateService<ISelectionService>();
                selectionService.SelectionProvider = this;
            }
        }
        
        #region tree building
        public override void Reload()
        {
            FTreeView.BeginUpdate();
            
            if (FRootMapper != null)
                FRootMapper.Dispose();
            
            FRootMapper = new ModelMapper(Input, Registry);
            
            if (ShowRoot)
            {
                var rootNode = new MapperTreeNode(FRootMapper);
                rootNode.Expand();
                FTreeView.Nodes.Add(rootNode);
            }
            else
            {
                var items = FRootMapper.Map<IParent>();
                if (FSynchronizer != null)
                    FSynchronizer.Dispose();
                FSynchronizer = FTreeView.Nodes.SyncWith(items.Childs, item => new MapperTreeNode(FRootMapper.CreateChildMapper(item)));
            }
            
            FTreeView.EndUpdate();
        }

        protected MapperTreeNode CreateTree(MapperTreeNode parentNode, object item)
        {
            return parentNode.CreateChildNode(item);
        }
        #endregion tree building
        
        #region TreeView events
        void FTreeViewMouseDown(object sender, MouseEventArgs e)
        {
            HideToolTip();
            
            var treeNode = FTreeView.GetNodeAt(e.X, e.Y) as MapperTreeNode;
            if (treeNode != null)
            {
                var mapper = treeNode.Mapper;
                
                //middle mousebutton is not handled in TreeViewNode_MouseClick, so do it here
                //as it is also obviously not handled in TreeView_MouseClick
                if (e.Button == MouseButtons.Middle)
                    OnClick(mapper, e);
                else
                    OnMouseDown(mapper, e);
            }
        }
        
        void TreeViewNodeMouseClick(object sender, TreeNodeMouseClickEventArgs args)
        {
            HideToolTip();
            
            var treeNode = args.Node as MapperTreeNode;
            var mapper = treeNode.Mapper;
            
            OnClick(mapper, args);
            if (args.Button == MouseButtons.Right && mapper.CanMap<IMenuEntry>())
            {
                var menuItem = mapper.Map<IMenuEntry>();
                
                //create context menu on clicked node
                treeNode.ContextMenuStrip = new ContextMenuStrip();
                CreateSubMenu(treeNode.ContextMenuStrip.Items, menuItem);
                treeNode.ContextMenuStrip.Show();
            }
        }
        
        void TreeViewNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            HideToolTip();
            
            var treeNode = e.Node as MapperTreeNode;
            var mapper = treeNode.Mapper;
            
            OnDoubleClick(mapper, e);
        }
        
        void TreeViewItemDrag(object sender, ItemDragEventArgs e)
        {
            var treeNode = e.Item as MapperTreeNode;
            var mapper = treeNode.Mapper;
            
            if (mapper.CanMap<IDraggable>())
            {
                var dragable = mapper.Map<IDraggable>();
                
                // check if this item allows being dragged
                if (dragable.AllowDrag())
                {
                    // start the dragdrop operation
                    FTreeView.DoDragDrop(dragable.ItemToDrag(), DragDropEffects.All);
                }
            }
        }
        
        void TreeViewDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            var destinationNode = ((TreeView)sender).GetNodeAt(pt) as MapperTreeNode;
            
            if (destinationNode != null)
            {
                var mapper = destinationNode.Mapper;
                
                if (mapper.CanMap<IDroppable>())
                {
                    var droppable = mapper.Map<IDroppable>();
                    var items = CreateDropItems(e.Data);
                    
                    if (droppable.AllowDrop(items))
                        e.Effect = DragDropEffects.Copy;
                    else
                        e.Effect = DragDropEffects.None;
                }
            }
        }
        
        void TreeViewDragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            var destinationNode = ((TreeView)sender).GetNodeAt(pt) as MapperTreeNode;

            if (destinationNode != null)
            {
                var mapper = destinationNode.Mapper;
                
                if (mapper.CanMap<IDroppable>())
                {
                    var droppable = mapper.Map<IDroppable>();
                    var items = CreateDropItems(e.Data);
                    
                    droppable.DropItems(items, pt);
                }
            }
        }
        
        private Dictionary<string, object> CreateDropItems(IDataObject data)
        {
            var dropItems = new Dictionary<string, object>();
            foreach (string df in data.GetFormats(true))
                dropItems[df] = data.GetData(df, true);
            return dropItems;
        }
        #endregion TreeView events
        
        #region functionality
        protected MapperTreeNode FindNode(MapperTreeNode parentNode, object tag)
        {
            if (parentNode.Tag == tag)
                return parentNode;
            
            foreach (MapperTreeNode node in parentNode)
            {
                var result = FindNode(node, tag);
                if (result != null)
                    return result;
            }
            
            return null;
        }
        
        public void Expand(object tag, bool expandChildren)
        {
            foreach (MapperTreeNode rootNode in FTreeView.Nodes)
            {
                var node = FindNode(rootNode, tag);
                if (node != null)
                {
                    if (expandChildren)
                        node.ExpandAll();
                    else
                        node.Expand();
                    break;
                }
            }
        }
        
        public void Collapse(object tag, bool collapseChildren)
        {
            foreach (MapperTreeNode rootNode in FTreeView.Nodes)
            {
                var node = FindNode(rootNode, tag);
                if (node != null)
                {
                    if (collapseChildren)
                        CollapseAll(node);
                    else
                        node.Collapse();
                    break;
                }
            }
        }
        
        private void CollapseAll(TreeNode node)
        {
            foreach(TreeNode tn in node.Nodes)
            {
                tn.Collapse();
                CollapseAll(tn);
            }
        }
        
        public bool IsExpanded(object tag)
        {
            foreach (MapperTreeNode rootNode in FTreeView.Nodes)
            {
                var node = FindNode(rootNode, tag);
                if (node != null)
                    return node.IsExpanded;
            }
            return false;
        }
        
        public void Solo(object tag)
        {
            foreach (MapperTreeNode rootNode in FTreeView.Nodes)
            {
                var node = FindNode(rootNode, tag);
                if (node != null)
                {
                    if (node.Parent == null)
                    {
                        foreach (TreeNode tn in node.TreeView.Nodes)
                            tn.Collapse();
                        node.Expand();
                    }
                    else
                    {
                        node.Parent.Collapse();
                        node.Expand();
                    }
                    break;
                }
            }
        }
        
        public void HideToolTip()
        {
            FToolTip.Hide(FTreeView);
        }
        
        #endregion functionality
        
        #region Menu
        private void CreateSubMenu(ToolStripItemCollection parentItems, IMenuEntry parentMenuItem)
        {
            bool separatorPending = false;
            
            foreach(IMenuEntry mi in parentMenuItem)
            {
                if (mi is MenuSeparator)
                    separatorPending = true;
                else
                {
                    if (separatorPending)
                    {
                        parentItems.Add(new ToolStripSeparator());
                        separatorPending = false;
                    }
                    
                    var tsmi = new ToolStripMenuItem(mi.Name);
                    tsmi.Tag = mi;
                    tsmi.Click += MenuItemClickedCB;
                    tsmi.ShortcutKeys = mi.ShortcutKeys;
                    tsmi.Enabled = mi.Enabled;
                    parentItems.Add(tsmi);
                    
                    //recursively add downtree menus
                    CreateSubMenu(tsmi.DropDownItems, mi);
                }
            }
        }
        
        private void MenuItemClickedCB(object sender, EventArgs e)
        {
            var toolStripMenuItem = sender as ToolStripMenuItem;
            var menuEntry = toolStripMenuItem.Tag as IMenuEntry;
            menuEntry.Click();
        }
        #endregion Menu

        #region Drawing
        
        void FTreeViewDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (FlatStyle)
            {
                var rect = new Rectangle(e.Bounds.Location, e.Bounds.Size);
                rect.X += FTreeView.Indent * e.Node.Level;
                
                int gray = Math.Min(160 + (e.Node.Level * 20), 255);
                using (var brush = new SolidBrush(Color.FromArgb(255, gray, gray, gray)))
                {
                    if (e.Node.Equals(FLastTooltipNode))
                    {
                        brush.Color = CHoverColor;
                    }
                    
                    string prefix = string.Empty;
                    if (e.Node.Nodes.Count > 0)
                    {
                        prefix = e.Node.IsExpanded ? "v  " : "> ";
                    }

                    e.Graphics.FillRectangle(brush, e.Bounds);
                    
                    using (var blackBrush = new SolidBrush(Color.Black))
                    {
                        e.Graphics.DrawString(prefix + e.Node.Text, (sender as TreeView).Font, blackBrush, rect.Location);
                    }
                    
                    if ((e.Node.Parent != null) && (e.Node == e.Node.Parent.LastNode))
                    {
                        int y = e.Bounds.Y + e.Bounds.Height-1;
                        //  e.Graphics.DrawLine(SystemPens.ControlDark, rect.X - FTreeView.Indent, y, e.Bounds.Width, y);
                    }
                    
                    var treeNode = e.Node as MapperTreeNode;
                    var mapper = treeNode.Mapper;
                    if (mapper.CanMap<ISelectable>())
                    {
                        var selectable = mapper.Map<ISelectable>();
                        if (selectable.Selected)
                        {
                            using (var pen = new Pen(Color.Black))
                            {
                                e.Graphics.DrawRectangle(pen, e.Bounds);
                            }
                        }
                    }
                }
            }
            else
                e.DrawDefault = true;
        }
        
        void InvalidateTreeNode(TreeNode treeNode)
        {
        	if (treeNode != null)
        	{
        		var bounds = new Rectangle(0, treeNode.Bounds.Y, FTreeView.Width, treeNode.Bounds.Height);
            	FTreeView.Invalidate(bounds);
        	}
        }
        
        void FTreeViewMouseMove(object sender, MouseEventArgs e)
        {
            if (!ShowTooltip) return;
            
            var treeNode = FTreeView.GetNodeAt(e.Location) as MapperTreeNode;
            
            if (treeNode != FLastTooltipNode)
            {
            	InvalidateTreeNode(FLastTooltipNode);
            	FLastTooltipNode = treeNode;
            	InvalidateTreeNode(FLastTooltipNode);
                
                if (treeNode != null)
                {
                    var mapper = treeNode.Mapper;
                    if (mapper.CanMap<IDescripted>())
                    {
                        string tip = mapper.Map<IDescripted>().Description;
                        if (!string.IsNullOrEmpty(tip))
                        {
                            FToolTip.Show(tip, FTreeView, e.X + 15, treeNode.Bounds.Y + 30);
                            return;
                        }
                    }
                }
                
                HideToolTip();
            }
        }
        
        #endregion
        
        void FTreeViewVisibleChanged(object sender, EventArgs e)
        {
            HideToolTip();
        }
        
        protected void FTreeViewKeyDown(object sender, KeyEventArgs e)
        {
            var treeNode = FTreeView.SelectedNode as MapperTreeNode;
            if (treeNode != null)
            {
                var mapper = treeNode.Mapper;
                
                if (mapper.CanMap<IMenuEntry>())
                {
                    var entry = FindMenuEntryByKeyData(mapper.Map<IMenuEntry>(), e.KeyData);
                    if (entry != null && entry.Enabled)
                        entry.Click();
                }
            }
        }
        
        private IMenuEntry FindMenuEntryByKeyData(IMenuEntry parent, Keys keyData)
        {
            if (parent.ShortcutKeys == keyData)
                return parent;
            
            foreach (var entry in parent)
            {
                var result = FindMenuEntryByKeyData(entry, keyData);
                if (result != null)
                    return result;
            }
            
            return null;
        }
        
        #region ILabelEditor Members
        
        public event VVVV.Core.Viewer.LabelEditEventHandler AfterLabelEdit;
        public event VVVV.Core.Viewer.LabelEditEventHandler BeforeLabelEdit;
        
        public bool BeginEdit(object model)
        {
            var node = SelectedNode as MapperTreeNode;
            if (node != null)
            {
                node.BeginEdit();
                return true;
            }
            return false;
        }
        
        private delegate void DoItLater();
        
        void TreeViewAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            var label = e.Label;
            
            if (label != null && AfterLabelEdit != null)
            {
                // Fire the event AFTER tree node editing took place. Weired behaviour otherwise.
                // See http://www.codeproject.com/KB/tree/CustomizedLabelEdit.aspx and comments
                var doItLater = new DoItLater(
                    delegate()
                    {
                        var args = new VVVV.Core.Viewer.LabelEditEventArgs(e.Node.Tag, label);
                        AfterLabelEdit(this, args);
                        
                        if (args.CancelEdit)
                            e.Node.BeginEdit();
                        else
                            FTreeView.SelectedNode = e.Node;
                    });
                
                FTreeView.BeginInvoke(doItLater);
            }
            
            e.CancelEdit = true;
        }
        
        void TreeViewBeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (BeforeLabelEdit != null)
            {
                var args = new VVVV.Core.Viewer.LabelEditEventArgs(e.Node.Tag, e.Label);
                BeforeLabelEdit(this, args);
                
                e.CancelEdit = args.CancelEdit;
            }
            else
                e.CancelEdit = true;
        }
        
        void FTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedNode = e.Node;
            
            var doItLater = new DoItLater(() => OnSelectionChanged(CurrentSelection));
            
            FTreeView.BeginInvoke(doItLater);
        }
        
        #endregion
        
        #region ISelectionProvider Members
        
        public ISelection CurrentSelection
        {
            get
            {
                if (SelectedNode != null)
                    return Selection.Single(SelectedNode.Tag);
                else
                    return Selection.Empty;
            }
        }
        
        public event SelectionChangedEventHandler SelectionChanged;
        
        protected virtual void OnSelectionChanged(ISelection selection)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectionChangedEventArgs(selection));
            }
        }
        
        #endregion
    }
}
