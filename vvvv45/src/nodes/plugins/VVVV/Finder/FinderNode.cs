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

#endregion usings

namespace VVVV.Nodes.Finder
{
    public enum SearchScope {Local, Downstream, Global};
    
    public struct Filter
    {
        public SearchScope Scope;
        public bool SendReceive;
        public bool Comments;
        public bool Labels;
        public bool Effects;
        public bool Freeframes;
        public bool Modules;
        public bool Plugins;
        public bool IONodes;
        public bool Natives;
        public bool VSTs;
        public bool Patches;
        public bool Unknowns;
        public bool Boygrouped;
        public bool Addons;
        public bool Windows;
        public bool IDs;
        private List<string> FTags;
        public List<string> Tags
        {
            get {return FTags;}
            set
            {
                FTags = value;
                
                if (FTags.Contains("g"))
                {
                    Scope = SearchScope.Global;
                    FTags.Remove("g");
                }
                else if (FTags.Contains("d"))
                {
                    Scope = SearchScope.Downstream;
                    FTags.Remove("d");
                }
                if (FTags.Contains("s"))
                {
                    SendReceive = true;
                    FTags.Remove("s");
                }
                if (FTags.Contains("/"))
                {
                    Comments = true;
                    FTags.Remove("/");
                }
                if (FTags.Contains("l"))
                {
                    Labels = true;
                    FTags.Remove("l");
                }
                if (FTags.Contains("x"))
                {
                    Effects = true;
                    FTags.Remove("x");
                }
                if (FTags.Contains("f"))
                {
                    Freeframes = true;
                    FTags.Remove("f");
                }
                if (FTags.Contains("m"))
                {
                    Modules = true;
                    FTags.Remove("m");
                }
                if (FTags.Contains("p"))
                {
                    Plugins = true;
                    FTags.Remove("p");
                }
                if (FTags.Contains("i"))
                {
                    IONodes = true;
                    FTags.Remove("i");
                }
                if (FTags.Contains("n"))
                {
                    Natives = true;
                    FTags.Remove("n");
                }
                if (FTags.Contains("v"))
                {
                    VSTs = true;
                    FTags.Remove("v");
                }
                if (FTags.Contains("t"))
                {
                    Patches = true;
                    FTags.Remove("t");
                }
                if (FTags.Contains("r"))
                {
                    Unknowns = true;
                    FTags.Remove("r");
                }
                if (FTags.Contains("a"))
                {
                    Addons = true;
                    FTags.Remove("a");
                }
                if (FTags.Contains("b"))
                {
                    Boygrouped = true;
                    FTags.Remove("b");
                }
                if (FTags.Contains("w"))
                {
                    Windows = true;
                    FTags.Remove("w");
                }
                if (FTags.Contains("#"))
                {
                    IDs = true;
                    FTags.Remove("#");
                }
                
                for (int i = 0; i < FTags.Count; i++)
                    FTags[i] = FTags[i].Trim((char) 160);
            }
        }
        
        public bool QuickTagsUsed()
        {
            return SendReceive || Comments || Labels || IONodes || Natives || Modules || Effects || Freeframes || VSTs || Plugins || Patches || Unknowns || Addons || Boygrouped || Windows || IDs;
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
        private List<PatchNode> FPlainResultList = new List<PatchNode>();
        
        private IWindow2 FActivePatchWindow;
        private IWindow2 FActiveWindow;
        private PatchNode FSearchResult;
        
        private Filter FFilter;
        private int FSearchIndex;
        
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
            this.FSearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FSearchTextBoxKeyDown);
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
                    
                    if (FSearchResult != null)
                        FSearchResult.Dispose();
                    
                    ActivePatchNode = null;
                    
                    this.FSearchTextBox.TextChanged -= this.FSearchTextBoxTextChanged;
                    this.FSearchTextBox.KeyDown -= this.FSearchTextBoxKeyDown;
                    
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
            
            if (updateActiveWindow && FSearchResult != null)
            {
                FSearchResult.SetActiveWindow(FActiveWindow);
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

        void HandleFActivePatchParentRemoved (IViewableCollection collection, object item)
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
                
                if (FFilter.Scope != SearchScope.Global)
                    ClearSearch();
            }
        }
        #endregion IWindowSelectionListener
        
        #region Search
        private void ClearSearch()
        {
            if (FSearchResult != null)
                FSearchResult.Dispose();
			FSearchResult = null;
            FPlainResultList.Clear();
            FSearchIndex = 0;
        }
        
        private void UpdateSearch()
        {
            if (FHDEHost.Root == null)
                return;
            
            string query = FSearchTextBox.Text.ToLower();
            query += (char) 160;
            
            FFilter = new Filter();
            var tags = query.Split(new char[1]{' '}).ToList();
            for (int i = tags.Count-1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(tags[i].Trim()))
                    tags.RemoveAt(i);
            }
            FFilter.Tags = tags;
            FHierarchyViewer.ShowLinks = FFilter.SendReceive;
            
            FHierarchyViewer.BeginUpdate();
            try
            {
                ClearSearch();
                
                switch (FFilter.Scope)
                {
                    case SearchScope.Global:
                        {
                            FSearchResult = new PatchNode(FHDEHost.RootNode, FFilter, true, true);
                            FHierarchyViewer.ShowRoot = false;
                            break;
                        }
                    case SearchScope.Local:
                        {
                            FSearchResult = new PatchNode(FActivePatchNode, FFilter, true, false);
                            FHierarchyViewer.ShowRoot = true;
                            break;
                        }
                    case SearchScope.Downstream:
                        {
                            FSearchResult = new PatchNode(FActivePatchNode, FFilter, true, true);
                            FHierarchyViewer.ShowRoot = true;
                            break;
                        }
                }
                
                FSearchResult.SetActiveWindow(FActiveWindow);

                FHierarchyViewer.Input = FSearchResult;

                FNodeCountLabel.Text = "Matching Nodes: " + FPlainResultList.Count.ToString();
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
        
        void FSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (FPlainResultList.Count == 0)
                return;
            
            if (e.KeyCode == Keys.F3 || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                FPlainResultList[FSearchIndex].Selected = false;
                if (e.Shift || e.KeyCode == Keys.Up)
                {
                    FSearchIndex -= 1;
                    if (FSearchIndex < 0)
                        FSearchIndex = FPlainResultList.Count - 1;
                }
                else
                    FSearchIndex = (FSearchIndex + 1) % FPlainResultList.Count;
                
                FPlainResultList[FSearchIndex].Selected = true;
                
                //select the node
                FHDEHost.SelectNodes(new INode2[1]{FPlainResultList[FSearchIndex].Node});
            }
            else if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                OpenPatch(FPlainResultList[FSearchIndex].Node);
            }
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
            
            tip += "g\t Search globally in the whole node graph\n";
            tip += "d\t Search in patches downstream of the active patch\n";
            tip += "----\n";
            tip += "n\t Nativ nodes\n";
            tip += "m\t Modules\n";
            tip += "p\t vvvv Plugins\n";
            tip += "x\t Effects\n";
            tip += "f\t Freeframes Plugins\n";
            tip += "v\t VST Plugins\n";
            tip += "a\t all Addons\n";
            tip += "i\t IOBoxes (Pins of Patches/Modules)\n";
            tip += "s\t Send/Receive Nodes\n";
            tip += "/\t Comments\n";
            tip += "l\t Labels (descriptive names)\n";
            tip += "t\t Patches\n";
            tip += "r\t Red (missing) Nodes\n";
            tip += "b\t Boygrouped Nodes\n";
            tip += "#\t Node IDs\n";
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
            else if (e.Button == MouseButtons.Left && sender.Model != null)
            {
                (sender.Model as PatchNode).Selected = true;
                FHDEHost.SelectNodes(new INode2[1]{(sender.Model as PatchNode).Node});
                
                //only fit view to selected node if not in local scope
                if (FFilter.Scope != SearchScope.Local)
                    if (sender.CanMap<ICamera>())
                {
                    var parent = (sender.Model as PatchNode).Node.Parent;
                    if (parent == null)
                        parent = FHDEHost.RootNode;
                    
                    sender.Map<ICamera>().View(FSearchResult.FindNode(parent));
                }
            }
            else if (e.Button == MouseButtons.Right && sender.Model != null)
            {
                if ((sender.Model as PatchNode).Node == FActivePatchNode)
                    FHDEHost.ShowEditor(FActivePatchNode.Parent);
                else
                    OpenPatch((sender.Model as PatchNode).Node);
            }
        }
        
        void FHierarchyViewerDoubleClick(IModelMapper sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                OpenParentAndSelectNode((sender.Model as PatchNode).Node);
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
            FHDEHost.ShowEditor(node.Parent);
            FHDEHost.SelectNodes(new INode2[1]{node});
        }
    }
}
