using System;
using System.Collections;
using System.Windows.Forms;
using VVVV.Core;
using VVVV.Core.View;
using VVVV.Core.Collections.Sync;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    public delegate void TreeNodeEventHandler(MapperTreeNode parentNode, MapperTreeNode node);
    
    /// <summary>
    /// Extends the Windows Forms TreeNode by providing access to the
    /// ModelMapper used to retrieve information for the model element
    /// stored in this node.
    /// Implements IViewableList so it can easily be synchronized with the root node collection
    /// of the TreeView.
    /// </summary>
    public class MapperTreeNode : TreeNode, IViewableList, IDisposable
    {
    	private Synchronizer<object, object> FSynchronizer;
    	private INamed FNamed;
    	private ISelectable FSelectable;
    	
        public event OrderChangedHandler OrderChanged;
        public event CollectionDelegate Added;
        public event CollectionDelegate Removed;
#pragma warning disable CS0067
        public event CollectionUpdateDelegate Cleared;
        public event CollectionUpdateDelegate UpdateBegun;
        public event CollectionUpdateDelegate Updated;
#pragma warning restore

        public MapperTreeNode(ModelMapper mapper)
            :base()
        {
            Mapper = mapper;
            
            FNamed = mapper.Map<INamed>();
            
            Text = FNamed.Name;
            Tag = mapper.Model;
            FNamed.Renamed += Item_Renamed;
            
            if (mapper.CanMap<IParent>())
            {
                var items = mapper.Map<IParent>();
                if (items.Childs != null)
                {
	                // Keep Nodes and items in sync
	                FSynchronizer = Nodes.SyncWith(items.Childs, CreateChildNode);
	                FSynchronizer.Synced += synchronizer_Synced;
                }
            }            
            if (mapper.CanMap<ISelectable>())
            {
                FSelectable = mapper.Map<ISelectable>();
                Checked = FSelectable.Selected;
                FSelectable.SelectionChanged += Item_SelectionChanged;
            }
        }
        
        /// <summary>
        /// The ModelMapper containing all the necessary mappings for the
        /// stored model element.
        /// </summary>
        public ModelMapper Mapper
        {
            get;
            private set;
        }

        public MapperTreeNode CreateChildNode(object item)
        {
            var mapper = Mapper.CreateChildMapper(item);
            return new MapperTreeNode(mapper);
        }
        
        public object this[int index]
        {
            get 
            {
                return Nodes[index];
            }
        }
        
        public int Count 
        {
            get 
            {
                return Nodes.Count;
            }
        }
        
        public bool Contains(object item)
        {
            return Nodes.Contains((TreeNode) item);
        }
        
        public IEnumerator GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }
        
        void synchronizer_Synced(object sender, SyncEventArgs<object, object> args)
        {
            var node = args.TargetItem as MapperTreeNode;
            
            switch (args.Action) 
            {
                case CollectionAction.Added:
                    if (Added != null)
                        Added(this, node);
                    break;
                case CollectionAction.Removed:
                    if (Removed != null)
                        Removed(this, node);
                    break;
                case CollectionAction.OrderChanged:
                    if (OrderChanged != null)
                        OrderChanged(this);
                    break;
                default:
                    // Ignore
                    break;
            }
        }

        void Item_Renamed(INamed sender, string newName)
        {
            this.Text = newName;
        }
        
        void Item_SelectionChanged(ISelectable sender, EventArgs e)
        {
            this.Checked = sender.Selected;
        }
    	
		public void Dispose()
		{
			FNamed.Renamed -= Item_Renamed;
			
			if (FSelectable != null)
				FSelectable.SelectionChanged -= Item_SelectionChanged;
			
			if (FSynchronizer != null)
			{
				FSynchronizer.Synced -= synchronizer_Synced;
				FSynchronizer.Dispose();
			}
			
			foreach (MapperTreeNode mapperTreeNode in Nodes)
			{
				mapperTreeNode.Dispose();
			}
			
			Mapper.Dispose();
		}
    }
}
