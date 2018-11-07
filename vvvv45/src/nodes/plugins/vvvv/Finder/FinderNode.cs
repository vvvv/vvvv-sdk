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
    [PluginInfo(Name = "Finder",
                Category = "VVVV",
                Shortcut = "Ctrl+F",
                Author = "vvvv group",
                Help = "Finds Nodes, Comments, Send/Receive channels and more.",
                InitialBoxWidth = 200,
                InitialBoxHeight = 250,
                InitialWindowWidth = 340,
                InitialWindowHeight = 550,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class FinderPluginNode: UserControl, IPluginBase
    {
        #region field declaration
        private IDiffSpread<string> FTagsPin;
        
        private IPluginHost2 FPluginHost;
        private IHDEHost FHDEHost;
        private MappingRegistry FMappingRegistry;
        
        private NodeView FNodeView;
        
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

            //adapt to dpi-scaling
            using (var g = this.CreateGraphics())
            {
                var dpiFactor = g.DpiY / 96.0f;
                panel1.Height = (int)(FSearchTextBox.Height + 4 * dpiFactor);
            }
            
            FHDEHost = host;
            FPluginHost = pluginHost;
            
            FSearchTextBox.ContextMenu = new ContextMenu();
            FSearchTextBox.ContextMenu.Popup += FSearchTextBox_ContextMenu_Popup;
            FSearchTextBox.MouseWheel += FSearchTextBox_MouseWheel;
            
            FMappingRegistry = new MappingRegistry();
            FMappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            FHierarchyViewer.Registry = FMappingRegistry;
            
            FTagsPin = tagsPin;
            FTagsPin.Changed += HandleTagsPinChanged;
            
            FNodeFilter = new NodeFilter();
            FNodeView = FNodeFilter.UpdateFilter(string.Empty, FHDEHost.RootNode, ModuleCheckBox.Checked);
            
            FHDEHost.WindowSelectionChanged += HandleWindowSelectionChanged;
            //defer setting the active patch window as
            //this will trigger the initial WindowSelectionChangeCB
            //which will want to access this windows caption which is not yet available
            SynchronizationContext.Current.Post((object state) => HandleWindowSelectionChanged(FHDEHost, new WindowEventArgs(FHDEHost.ActivePatchWindow)), null);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.FSearchTextBox = new System.Windows.Forms.TextBox();
            this.ModuleCheckBox = new System.Windows.Forms.CheckBox();
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
            this.panel3.Controls.Add(this.ModuleCheckBox);
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
            this.FSearchTextBox.Size = new System.Drawing.Size(213, 17);
            this.FSearchTextBox.TabIndex = 13;
            this.FSearchTextBox.TextChanged += new System.EventHandler(this.FSearchTextBoxTextChanged);
            this.FSearchTextBox.MouseEnter += new System.EventHandler(this.FSearchTextBoxMouseEnter);
            this.FSearchTextBox.MouseLeave += new System.EventHandler(this.FSearchTextBoxMouseLeave);
            // 
            // ModuleCheckBox
            // 
            this.ModuleCheckBox.AutoSize = true;
            this.ModuleCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.ModuleCheckBox.Location = new System.Drawing.Point(215, 0);
            this.ModuleCheckBox.Name = "ModuleCheckBox";
            this.ModuleCheckBox.Size = new System.Drawing.Size(35, 15);
            this.ModuleCheckBox.TabIndex = 14;
            this.ModuleCheckBox.Text = "M";
            this.FTooltip.SetToolTip(this.ModuleCheckBox, "Search in Modules");
            this.ModuleCheckBox.UseVisualStyleBackColor = true;
            this.ModuleCheckBox.CheckedChanged += new System.EventHandler(this.ModuleCheckBoxCheckedChanged);
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
            this.FHierarchyViewer.MouseClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerClick);
            this.FHierarchyViewer.MouseDoubleClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerDoubleClick);
            this.FHierarchyViewer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FHierarchyViewerKeyPress);
            // 
            // FTooltip
            // 
            this.FTooltip.BackColor = System.Drawing.Color.Gray;
            this.FTooltip.ForeColor = System.Drawing.Color.White;
            // 
            // FinderPluginNode
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private System.Windows.Forms.CheckBox ModuleCheckBox;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolTip FTooltip;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer FHierarchyViewer;
        private System.Windows.Forms.Label FNodeCountLabel;
        private System.Windows.Forms.TextBox FSearchTextBox;
        private System.Windows.Forms.Panel panel1;
        
        protected override void Dispose(bool disposing)
        {
            if(!FDisposed)
            {
                if(disposing)
                {
                    // Dispose managed resources.
                    FSearchTextBox.ContextMenu.Popup -= FSearchTextBox_ContextMenu_Popup;
                    FSearchTextBox.MouseWheel -= FSearchTextBox_MouseWheel;
                    FHDEHost.WindowSelectionChanged -= HandleWindowSelectionChanged;
                    FTagsPin.Changed -= HandleTagsPinChanged;
                    
                    //                    if (FSearchResult != null)
                    //                        FSearchResult.Dispose();
                    
                    ActivePatchNode = null;
                    
                    this.FSearchTextBox.TextChanged -= this.FSearchTextBoxTextChanged;
                    
                    // Shutdown viewer layer
                    this.FHierarchyViewer.MouseDoubleClick -= this.FHierarchyViewerDoubleClick;
                    this.FHierarchyViewer.MouseClick -= this.FHierarchyViewerClick;
                    this.FHierarchyViewer.KeyPress -= this.FHierarchyViewerKeyPress;
                    this.FHierarchyViewer.Dispose();
                    this.FHierarchyViewer = null;
                    
                    // Shutdown view layer
                    FNodeView.Dispose();
                }
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
                        FActivePatchNode.Parent.Removed -= HandleActivePatchParentRemoved;
                }
                
                FActivePatchNode = value;
                
                if (FActivePatchNode != null)
                {
                    if (FActivePatchNode.Parent != null)
                        FActivePatchNode.Parent.Removed += HandleActivePatchParentRemoved;
                }
            }
        }
        
        void HandleTagsPinChanged(IDiffSpread<string> spread)
        {
            FSearchTextBox.Text = spread[0];
        }
        
        void HandleWindowSelectionChanged(object sender, WindowEventArgs args)
        {
            var window = args.Window;
            if (window == null) return; // Might happen during node list creation
            
            switch (window.WindowType)
            {
                case WindowType.Patch:
                case WindowType.Module:
                    ActivePatchNode = window.Node;
                    //the hosts window may be null if the plugin is created hidden on startup
                    if (FPluginHost.Window != null)
                        FPluginHost.Window.Caption = FActivePatchNode.NodeInfo.Systemname;
                    
                    //only redraw if in local scope
                    if (!FNodeFilter.ScopeIsGlobal)
                        UpdateView();
                    break;
            }
            
            FNodeView.SetActiveWindow(window);
        }

        void HandleActivePatchParentRemoved(IViewableCollection collection, object item)
        {
            var childNode = item as INode2;
            //if active patch is being deleted detach view
            if (childNode == ActivePatchNode)
            {
                ActivePatchNode = childNode.Parent;
                
                if (!FNodeFilter.ScopeIsGlobal)
                    UpdateView();
            }
        }
        
        private void UpdateView()
        {
            string query = FSearchTextBox.Text.ToLower();
            
            FHierarchyViewer.BeginUpdate();
            try
            {
                FNodeView.Dispose();
                
                if (NodeFilter.IsGlobalSearchScope(query))
                {
                    FNodeView = FNodeFilter.UpdateFilter(query, FHDEHost.RootNode, ModuleCheckBox.Checked);
                    FHierarchyViewer.ShowRoot = false;
                }
                else
                {
                    FNodeView = FNodeFilter.UpdateFilter(query, FActivePatchNode, false);
                    FHierarchyViewer.ShowRoot = true;
                }
                
                FHierarchyViewer.Input = FNodeView;
            }
            finally
            {
                FHierarchyViewer.EndUpdate();
            }
        }
        
        #region GUI events
        void FSearchTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdateView();
            
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
            tip += "v\t VL Plugins\n";
            tip += "x\t Effects\n";
            tip += "f\t Freeframes Plugins\n";
            tip += "a\t VST Plugins\n";
            tip += "i\t IOBoxes (Pins of Patches/Modules)\n";
            tip += "e\t exposed IOBoxes\n";
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
        
        void FHierarchyViewerClick(ModelMapper sender, System.Windows.Forms.MouseEventArgs e)
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
                    if (FActivePatchNode.Window != null && FActivePatchNode.Window.IsVisible)
                    {
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
        
        void FHierarchyViewerDoubleClick(ModelMapper sender, System.Windows.Forms.MouseEventArgs e)
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
		void ModuleCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			UpdateView();
		}
    }
}
