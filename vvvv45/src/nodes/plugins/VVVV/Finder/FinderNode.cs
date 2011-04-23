#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.HDE.Viewer;
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;

#endregion usings

namespace VVVV.Nodes.Finder
{
    [Flags]
    public enum FilterFlags
    {
        None = 0x0,
        Send = 0x1,
        Comment = 0x2,
        Label = 0x4,
        Effect = 0x8,
        Freeframe = 0x10,
        Module = 0x20,
        Plugin = 0x40,
        IONode = 0x80,
        Native = 0x100,
        VST = 0x200,
        Patch = 0x400,
        Unknown = 0x800,
        Boygrouped = 0x1000,
        Name = 0x2000,
        Window = 0x4000,
        ID = 0x8000,
        Dynamic = 0x10000,
        Text = 0x20000,
        Receive = 0x40000,
        Addon = Effect | Freeframe | Module | Plugin | VST | Dynamic,
        AllNodeTypes = Addon | Send | Comment | IONode | Native | Patch | Unknown | Boygrouped | Window | Dynamic | Text | Receive
    }
    
    public class NodeFilter
    {
        public static bool IsGlobalSearchScope(string query)
        {
            var tags = ParseQuery(query);
            return tags.Contains("<");
        }
        
        static List<string> ParseQuery(string query)
        {
            query += (char) 160;
            var tags = query.Split(new char[1]{' '}).ToList();
            for (int i = tags.Count-1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(tags[i].Trim()))
                    tags.RemoveAt(i);
            }
            
            return tags;
        }
        
        public NodeView UpdateFilter(string query, INode2 startNode)
        {
            // Parse query
            Tags = ParseQuery(query);

            // Set filter scope
            if (Tags.Contains("<"))
            {
                MinLevel = int.MinValue;
                MaxLevel = int.MaxValue;
                Tags.Remove("<");
            }
            else if (Tags.Contains(">"))
            {
                MinLevel = 0;
                MaxLevel = int.MaxValue;
                Tags.Remove(">");
            }
            else
            {
                MinLevel = 0;
                MaxLevel = 1;
            }
            
            // Set filter flags which control what to search
            Flags = FilterFlags.None;
            if (Tags.Contains("s"))
            {
                Flags |= FilterFlags.Send;
                Flags |= FilterFlags.Receive;
                Tags.Remove("s");
            }
            if (Tags.Contains("/"))
            {
                Flags |= FilterFlags.Comment;
                Tags.Remove("/");
            }
            if (Tags.Contains("x"))
            {
                Flags |= FilterFlags.Effect;
                Tags.Remove("x");
            }
            if (Tags.Contains("f"))
            {
                Flags |= FilterFlags.Freeframe;
                Tags.Remove("f");
            }
            if (Tags.Contains("m"))
            {
                Flags |= FilterFlags.Module;
                Tags.Remove("m");
            }
            if (Tags.Contains("p"))
            {
                Flags |= FilterFlags.Plugin;
                Tags.Remove("p");
            }
            if (Tags.Contains("d"))
            {
                Flags |= FilterFlags.Dynamic;
                Tags.Remove("d");
            }
            if (Tags.Contains("i"))
            {
                Flags |= FilterFlags.IONode;
                Tags.Remove("i");
            }
            if (Tags.Contains("n"))
            {
                Flags |= FilterFlags.Native;
                Tags.Remove("n");
            }
            if (Tags.Contains("v"))
            {
                Flags |= FilterFlags.VST;
                Tags.Remove("v");
            }
            if (Tags.Contains("t"))
            {
                Flags |= FilterFlags.Patch;
                Tags.Remove("t");
            }
            if (Tags.Contains("r"))
            {
                Flags |= FilterFlags.Unknown;
                Tags.Remove("r");
            }
            if (Tags.Contains("a"))
            {
                Flags |= FilterFlags.Addon;
                Tags.Remove("a");
            }
            if (Tags.Contains("b"))
            {
                Flags |= FilterFlags.Boygrouped;
                Tags.Remove("b");
            }
            if (Tags.Contains("w"))
            {
                Flags |= FilterFlags.Window;
                Tags.Remove("w");
            }
            
            // If nothing set look for all kind of nodes
            if (Flags == FilterFlags.None)
                Flags = FilterFlags.AllNodeTypes;
            
            // Set filter tags which control where to search
            var wFlags = FilterFlags.None;
            if (Tags.Contains("l"))
            {
                wFlags |= FilterFlags.Label;
                Tags.Remove("l");
            }
            if (Tags.Contains("#"))
            {
                wFlags |= FilterFlags.ID;
                Tags.Remove("#");
            }
            
            // If nothing set search in node name
            if (wFlags == FilterFlags.None)
                wFlags = FilterFlags.Name;
            
            Flags |= wFlags;
            
            // Set filter tags
            for (int i = 0; i < Tags.Count; i++)
                Tags[i] = Tags[i].Trim((char) 160);
            
            return new NodeView(null, startNode, this, 0);
        }
        
        public int MinLevel
        {
            get;
            private set;
        }
        
        public int MaxLevel
        {
            get;
            private set;
        }
        
        public bool ScopeIsGlobal
        {
            get
            {
                return MinLevel == int.MinValue && MaxLevel == int.MaxValue;
            }
        }
        
        public bool ScopeIsLocal
        {
            get
            {
                return MinLevel == 0 && MaxLevel == 1;
            }
        }
        
        public INode2 StartNode
        {
            get;
            private set;
        }
        
        public IWindow2 ActiveWindow
        {
            get;
            private set;
        }
        
        public FilterFlags Flags
        {
            get;
            private set;
        }
        
        public List<string> Tags
        {
            get;
            private set;
        }
    }
    
    [PluginInfo(Name = "Finder",
                Category = "HDE",
                Shortcut = "Ctrl+F",
                Author = "vvvv group",
                Help = "Finds Nodes, Comments and Send/Receive channels and more.",
                InitialBoxWidth = 200,
                InitialBoxHeight = 250,
                InitialWindowWidth = 320,
                InitialWindowHeight = 500,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class FinderPluginNode: UserControl, IPluginBase
    {
        #region field declaration
        private IDiffSpread<string> FTagsPin;
        
        private IPluginHost2 FPluginHost;
        private IHDEHost FHDEHost;
        private MappingRegistry FMappingRegistry;
        
        private IWindow2 FActivePatchWindow;
        private IWindow2 FActiveWindow;
        //        private PatchNode FSearchResult;
        private NodeView FNodeView;
        
        //        private Filter FFilter;
        private readonly NodeFilter FNodeFilter;

        // Track whether Dispose has been called.
        private bool FDisposed = false;
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public FinderPluginNode(IHDEHost host, IPluginHost2 pluginHost, [Config("Tags")] IDiffSpread<string> tagsPin)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FHDEHost = host;
            FPluginHost = pluginHost;
            
            FSearchTextBox.ContextMenu = new ContextMenu();
            FSearchTextBox.ContextMenu.Popup += FSearchTextBox_ContextMenu_Popup;
            FSearchTextBox.MouseWheel += FSearchTextBox_MouseWheel;
            
            FMappingRegistry = new MappingRegistry();
            FMappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            FHierarchyViewer.Registry = FMappingRegistry;
            
            FHDEHost.WindowSelectionChanged += FHDEHost_WindowSelectionChanged;
            //defer setting the active patch window as
            //this will trigger the initial WindowSelectionChangeCB
            //which will want to access this windows caption which is not yet available
            SynchronizationContext.Current.Post((object state) => FHDEHost_WindowSelectionChanged(FHDEHost, new WindowEventArgs(FHDEHost.ActivePatchWindow)), null);
            
            FTagsPin = tagsPin;
            FTagsPin.Changed += FTagsPin_Changed;
            
            FNodeFilter = new NodeFilter();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.FSearchTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.FNodeCountLabel = new System.Windows.Forms.Label();
            this.FHierarchyViewer = new VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer();
            this.FTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(252, 17);
            this.panel1.TabIndex = 7;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.FSearchTextBox);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(250, 15);
            this.panel3.TabIndex = 11;
            // 
            // FSearchTextBox
            // 
            this.FSearchTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.FSearchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FSearchTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FSearchTextBox.Location = new System.Drawing.Point(2, 0);
            this.FSearchTextBox.MinimumSize = new System.Drawing.Size(0, 17);
            this.FSearchTextBox.Name = "FSearchTextBox";
            this.FSearchTextBox.Size = new System.Drawing.Size(248, 17);
            this.FSearchTextBox.TabIndex = 13;
            this.FSearchTextBox.TextChanged += new System.EventHandler(this.FSearchTextBoxTextChanged);
            this.FSearchTextBox.MouseLeave += new System.EventHandler(this.FSearchTextBoxMouseLeave);
            this.FSearchTextBox.MouseEnter += new System.EventHandler(this.FSearchTextBoxMouseEnter);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(2, 15);
            this.panel2.TabIndex = 11;
            // 
            // FNodeCountLabel
            // 
            this.FNodeCountLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.FNodeCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FNodeCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FNodeCountLabel.Location = new System.Drawing.Point(0, 256);
            this.FNodeCountLabel.Name = "FNodeCountLabel";
            this.FNodeCountLabel.Size = new System.Drawing.Size(252, 17);
            this.FNodeCountLabel.TabIndex = 8;
            this.FNodeCountLabel.Text = "Matching Nodes: ";
            this.FNodeCountLabel.Visible = false;
            // 
            // FHierarchyViewer
            // 
            this.FHierarchyViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FHierarchyViewer.Location = new System.Drawing.Point(0, 17);
            this.FHierarchyViewer.Name = "FHierarchyViewer";
            this.FHierarchyViewer.ShowLinks = false;
            this.FHierarchyViewer.ShowRoot = false;
            this.FHierarchyViewer.Size = new System.Drawing.Size(252, 239);
            this.FHierarchyViewer.TabIndex = 9;
            this.FHierarchyViewer.MouseDoubleClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerDoubleClick);
            this.FHierarchyViewer.MouseClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerClick);
            this.FHierarchyViewer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FHierarchyViewerKeyPress);
            // 
            // FTooltip
            // 
            this.FTooltip.BackColor = System.Drawing.Color.Gray;
            this.FTooltip.ForeColor = System.Drawing.Color.White;
            // 
            // FinderPluginNode
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Controls.Add(this.FHierarchyViewer);
            this.Controls.Add(this.FNodeCountLabel);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Name = "FinderPluginNode";
            this.Size = new System.Drawing.Size(252, 273);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
        }
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolTip FTooltip;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer FHierarchyViewer;
        private System.Windows.Forms.Label FNodeCountLabel;
        private System.Windows.Forms.TextBox FSearchTextBox;
        private System.Windows.Forms.Panel panel1;
        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!FDisposed)
            {
                if(disposing)
                {
                    // Dispose managed resources.
                    FSearchTextBox.ContextMenu.Popup -= FSearchTextBox_ContextMenu_Popup;
                    FSearchTextBox.MouseWheel -= FSearchTextBox_MouseWheel;
                    FHDEHost.WindowSelectionChanged -= FHDEHost_WindowSelectionChanged;
                    FTagsPin.Changed -= FTagsPin_Changed;
                    
                    //                    if (FSearchResult != null)
                    //                        FSearchResult.Dispose();
                    
                    ActivePatchNode = null;
                    
                    this.FSearchTextBox.TextChanged -= this.FSearchTextBoxTextChanged;
                    
                    this.FHierarchyViewer.MouseDoubleClick -= this.FHierarchyViewerDoubleClick;
                    this.FHierarchyViewer.MouseClick -= this.FHierarchyViewerClick;
                    this.FHierarchyViewer.KeyPress -= this.FHierarchyViewerKeyPress;
                    this.FHierarchyViewer.Dispose();
                    this.FHierarchyViewer = null;
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
                
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }
        
        #endregion constructor/destructor
        
        private INode2 FActivePatchNode;
        private INode2 ActivePatchNode
        {
            get
            {
                return FActivePatchNode;
            }
            set
            {
                if (FActivePatchNode != null)
                {
                    if (FActivePatchNode.Parent != null)
                        FActivePatchNode.Parent.Removed -= HandleFActivePatchParentRemoved;
                }
                
                FActivePatchNode = value;
                
                if (FActivePatchNode != null)
                {
                    if (FActivePatchNode.Parent != null)
                        FActivePatchNode.Parent.Removed += HandleFActivePatchParentRemoved;
                }
            }
        }
        
        void FTagsPin_Changed(IDiffSpread<string> spread)
        {
            FSearchTextBox.Text = spread[0];
        }
        
        #region IWindowSelectionListener
        void FHDEHost_WindowSelectionChanged(object sender, WindowEventArgs args)
        {
            var window = args.Window;
            
            if (window == FActiveWindow)
                return;
            
            FActiveWindow = window;
            var windowType = window.WindowType;
            var updateActiveWindow = false;
            
            if (windowType == WindowType.Module || windowType == WindowType.Patch)
            {
                if (window != FActivePatchWindow)
                    SetActivePatch(window);
                else
                    updateActiveWindow = true;
            }
            else
            {
                if (FHDEHost.ActivePatchWindow == null)
                {
                    ClearSearch();
                    ActivePatchNode = null;
                }
                else if (FActivePatchWindow != FHDEHost.ActivePatchWindow)
                    SetActivePatch(FHDEHost.ActivePatchWindow);
                
                updateActiveWindow = true;
            }
            
            if (updateActiveWindow && FNodeView != null)
            {
                FNodeView.SetActiveWindow(FActiveWindow);
            }
        }
        
        private void SetActivePatch(IWindow2 patch)
        {
            ActivePatchNode = patch.Node;
            FActivePatchWindow = patch;
            
            //the hosts window may be null if the plugin is created hidden on startup
            if (FPluginHost.Window != null)
                FPluginHost.Window.Caption = FActivePatchNode.NodeInfo.Systemname;
            
            UpdateSearch();
        }

        void HandleFActivePatchParentRemoved(IViewableCollection collection, object item)
        {
            var childNode = item as INode2;
            //if active patch is being deleted detach view
            if (childNode == FActivePatchNode)
            {
                if (FActivePatchNode.Parent != null)
                    FActivePatchNode.Parent.Removed -= HandleFActivePatchParentRemoved;
                
                FActivePatchNode = null;
                FActivePatchWindow = null;
                FActiveWindow = null;
                
                if (!FNodeFilter.ScopeIsGlobal)
                    ClearSearch();
            }
        }
        #endregion IWindowSelectionListener
        
        #region Search
        private void ClearSearch()
        {
            if (FNodeView != null)
            {
                FNodeView.Dispose();
                FNodeView = null;
            }
        }
        
        private void UpdateSearch()
        {
            string query = FSearchTextBox.Text.ToLower();
            
            FHierarchyViewer.BeginUpdate();
            try
            {
                ClearSearch();
                
                if (NodeFilter.IsGlobalSearchScope(query))
                {
                    FNodeView = FNodeFilter.UpdateFilter(query, FHDEHost.RootNode);
                    FHierarchyViewer.ShowRoot = false;
                }
                else
                {
                    FNodeView = FNodeFilter.UpdateFilter(query, FActivePatchNode);
                    FHierarchyViewer.ShowRoot = true;
                }
                
                FNodeView.SetActiveWindow(FActiveWindow);
                FHierarchyViewer.Input = FNodeView;
            }
            finally
            {
                FHierarchyViewer.EndUpdate();
            }
        }
        #endregion Search
        
        #region GUI events
        void FSearchTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdateSearch();
            
            //save tags in config pin
            FTagsPin[0] = FSearchTextBox.Text;
        }
        
        void FSearchTextBox_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            FHierarchyViewer.Focus();
        }

        void FSearchTextBox_ContextMenu_Popup(object sender, EventArgs e)
        {
            FSearchTextBox.Text = "";
        }
        
        void FSearchTextBoxMouseEnter(object sender, EventArgs e)
        {
            string tip = "Separate tags by <space>:\n\n";
            
            tip += "<\t Search globally in the whole node graph\n";
            tip += ">\t Search in patches within the active patch\n";
            tip += "----\n";
            tip += "#\t Search in Node IDs\n";
            tip += "l\t Search in Labels (descriptive names)\n";
            tip += "----\n";
            tip += "n\t Native nodes\n";
            tip += "m\t Modules\n";
            tip += "p\t vvvv Plugins\n";
            tip += "d\t vvvv Dynamic Plugins\n";
            tip += "x\t Effects\n";
            tip += "f\t Freeframes Plugins\n";
            tip += "v\t VST Plugins\n";
            tip += "a\t all Addons\n";
            tip += "i\t IOBoxes (Pins of Patches/Modules)\n";
            tip += "s\t Send/Receive Nodes\n";
            tip += "/\t Comments\n";
            tip += "t\t Patches\n";
            tip += "r\t Red (missing) Nodes\n";
            tip += "b\t Boygrouped Nodes\n";
            tip += "w\t Windows";

            FTooltip.Show(tip, FSearchTextBox, new Point(0, FSearchTextBox.Height));
        }
        
        void FSearchTextBoxMouseLeave(object sender, EventArgs e)
        {
            FTooltip.Hide(FSearchTextBox);
        }
        
        void FHierarchyViewerClick(IModelMapper sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sender == null)
            {
                if (e.Button == MouseButtons.Middle)
                    FHierarchyViewer.ViewAll();
            }
            else
            {
                var nodeView = sender.Model as NodeView;
                if (e.Button == MouseButtons.Left && nodeView != null)
                {
                    nodeView.Selected = true;
                    FHDEHost.SelectNodes(new INode2[1] { nodeView.Node});
                    
                    if (FNodeView != null && !FNodeFilter.ScopeIsLocal)
                    {
                        if (sender.CanMap<ICamera>())
                        {
                            var camera = sender.Map<ICamera>();
                            var parent = nodeView.Parent;
                            if (parent == null)
                            {
                                parent = FNodeView;
                            }
                            
                            camera.View(parent);
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right && nodeView != null)
                {
                    if (nodeView.Node == FActivePatchNode)
                    {
                        FHDEHost.ShowEditor(FActivePatchNode.Parent);
                    }
                    else
                    {
                        OpenPatch(nodeView.Node);
                    }
                }
            }
        }
        
        void FHierarchyViewerDoubleClick(IModelMapper sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sender != null)
            {
                var nodeView = sender.Model as NodeView;
                
                if (nodeView != null)
                {
                    if (e.Button == MouseButtons.Left)
                        OpenParentAndSelectNode(nodeView.Node);
                }
            }
        }
        
        void FHierarchyViewerKeyPress(object sender, KeyPressEventArgs e)
        {
            FSearchTextBox.Focus();
            if (e.KeyChar == (char) Keys.Back)
                FSearchTextBox.Text = "";
            else
            {
                FSearchTextBox.Text += (e.KeyChar).ToString();
                FSearchTextBox.Select(FSearchTextBox.Text.Length, 1);
            }
        }
        #endregion GUI events
        
        private void OpenPatch(INode2 node)
        {
            if (node == null)
                FHDEHost.ShowEditor(FActivePatchNode.Parent);
            else if (node.HasPatch || node.HasCode)
                FHDEHost.ShowEditor(node);
            else if (node.HasGUI)
                FHDEHost.ShowGUI(node);
            //else
            //    OpenParentAndSelectNode(node);
        }
        
        private void OpenParentAndSelectNode(INode2 node)
        {
            if (node != FHDEHost.RootNode)
            {
                FHDEHost.ShowEditor(node.Parent);
                FHDEHost.SelectNodes(new INode2[1]{node});
            }
        }
    }
}
