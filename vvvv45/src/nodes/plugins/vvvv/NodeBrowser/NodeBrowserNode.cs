#region usings
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Collections;
using VVVV.Core.View;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes.NodeBrowser
{
    [PluginInfo(Name = "NodeBrowser",
                Category = "VVVV",
                Shortcut = "Ctrl+N",
                Author = "vvvv group",
                Help = "The NodeInfo Browser",
                InitialBoxWidth = 200,
                InitialBoxHeight = 250,
                InitialWindowWidth = 340,
                InitialWindowHeight = 550,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class NodeBrowserPluginNode: UserControl, INodeBrowser, IPartImportsSatisfiedNotification
    {
        #region field declaration
        
        //the hosts
        public IHDEHost HDEHost
        {
            get;
            private set;
        }
        
        [Import]
        public INodeBrowserHost NodeBrowserHost
        {
            get;
            set;
        }
        
        [Import]
        public ILogger Logger
        {
            get;
            set;
        }
        
        private bool FIsStandalone;
        public bool IsStandalone
        {
            get
            {
                return FIsStandalone;
            }
            set
            {
                FIsStandalone = value;
                
                if (FIsStandalone)
                {
                    HDEHost.WindowSelectionChanged += HandleWindowSelectionChanged;
                }
                else
                {
                    HDEHost.WindowSelectionChanged -= HandleWindowSelectionChanged;
                }
            }
        }
        
        internal NodeCollection FNodeCollection;
        
        public INodeInfoFactory NodeInfoFactory
        {
            get;
            set;
        }
        
        public CategoryFilterPanel CategoryFilter
        {
        	get {return FCategoryFilterPanel;}
        }
        
        internal IWindow2 CurrentPatchWindow
        {
            get
            {
                if (HDEHost != null)
                    return HDEHost.ActivePatchWindow;
                else
                    return null;
            }
        }

        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        //further fields
        public string CurrentDir
        {
            get
            {
                if (CurrentPatchWindow != null)
                {
                    var node = CurrentPatchWindow.Node;
                    var nodeInfo = node.NodeInfo;
                    var systemname = nodeInfo.Systemname;
                    var filename = nodeInfo.Filename;
                    
                    if (Path.IsPathRooted(filename))
                        return Path.GetDirectoryName(filename);
                    else
                        return string.Empty;
                }
                else
                    return string.Empty;
            }
        }
        private string FInitialText;
        #endregion field declaration
        
        #region constructor/destructor
        //alternative constructor for standalone use
        public NodeBrowserPluginNode()
        {
            DefaultConstructor();
        }
        
        [ImportingConstructor]
        public NodeBrowserPluginNode(IHDEHost host, INodeInfoFactory nodeInfoFactory, NodeCollection nodeCollection)
        {
            HDEHost = host;            
            NodeInfoFactory = nodeInfoFactory;
            FNodeCollection = nodeCollection;
            IsStandalone = true;
            
            DefaultConstructor();
        }

        private void DefaultConstructor()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FClonePanel.Closed += HandleClonePanelClosed;

            FClonePanel.Dock = DockStyle.Fill;
            FCategoryPanel.Dock = DockStyle.Fill;
            FNodeTagPanel.Dock = DockStyle.Fill;
            FGirlpowerTagPanel.Dock = DockStyle.Fill;
            FCategoryFilterPanel.Dock = DockStyle.Fill;

            FClonePanel.NodeBrowser = this;
            FNodeTagPanel.NodeBrowser = this;
            FCategoryPanel.NodeBrowser = this;
            FCategoryFilterPanel.NodeBrowser = this;
            
            FCategoryFilterPanel.OnFilterChanged += FCategoryFilterPanel_OnFilterChanged;
            
            FTagsTextBox.ContextMenu = new ContextMenu();
            FTagsTextBox.MouseWheel += FTagsTextBoxMouseWheel;
            
            FNodeTagPanel.TagsTextBox = FTagsTextBox;
        }

        void FCategoryFilterPanel_OnFilterChanged()
        {
        	UpdatePanels();
        }
        
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
					if (HDEHost != null)
					   HDEHost.WindowSelectionChanged -= HandleWindowSelectionChanged;
					
					FTagsTextBox.MouseWheel -= FTagsTextBoxMouseWheel;
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
                
                //nothing to declare
                
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }
        
        private void InitializeComponent()
        {
        	this.components = new System.ComponentModel.Container();
        	this.FClonePanel = new VVVV.Nodes.NodeBrowser.ClonePanel();
        	this.FNodeTagPanel = new VVVV.Nodes.NodeBrowser.TagPanel();
        	this.FCategoryPanel = new VVVV.Nodes.NodeBrowser.CategoryPanel();
        	this.FTopPanel = new System.Windows.Forms.Panel();
        	this.FTagsTextBox = new System.Windows.Forms.TextBox();
        	this.FTagButton = new System.Windows.Forms.Button();
        	this.FCategoryButton = new System.Windows.Forms.Button();
        	this.FGirlpowerButton = new System.Windows.Forms.Button();
        	this.FFilterButton = new System.Windows.Forms.Button();
        	this.FCategoryFilterPanel = new VVVV.Nodes.NodeBrowser.CategoryFilterPanel();
        	this.FGirlpowerTagPanel = new VVVV.Nodes.NodeBrowser.TagPanel();
        	this.FToolTip = new System.Windows.Forms.ToolTip(this.components);
        	this.FTopPanel.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// FClonePanel
        	// 
        	this.FClonePanel.BackColor = System.Drawing.Color.Silver;
        	this.FClonePanel.Location = new System.Drawing.Point(241, 82);
        	this.FClonePanel.Name = "FClonePanel";
        	this.FClonePanel.NodeBrowser = null;
        	this.FClonePanel.Padding = new System.Windows.Forms.Padding(8);
        	this.FClonePanel.Size = new System.Drawing.Size(295, 269);
        	this.FClonePanel.TabIndex = 0;
        	this.FClonePanel.Visible = false;
        	// 
        	// FNodeTagPanel
        	// 
        	this.FNodeTagPanel.AllowDragDrop = true;
        	this.FNodeTagPanel.AndTags = true;
        	this.FNodeTagPanel.Location = new System.Drawing.Point(16, 41);
        	this.FNodeTagPanel.Name = "FNodeTagPanel";
        	this.FNodeTagPanel.NodeBrowser = null;
        	this.FNodeTagPanel.Size = new System.Drawing.Size(120, 115);
        	this.FNodeTagPanel.TabIndex = 1;
        	this.FNodeTagPanel.TagsTextBox = null;
        	this.FNodeTagPanel.OnPanelChange += new VVVV.Nodes.NodeBrowser.PanelChangeHandler(this.HandleOnPanelChange);
        	this.FNodeTagPanel.OnCreateNode += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleCreateNode);
        	this.FNodeTagPanel.OnShowNodeReference += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowNodeReference);
        	this.FNodeTagPanel.OnShowHelpPatch += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowHelpPatch);
        	this.FNodeTagPanel.OnCreateNodeFromString += new VVVV.Nodes.NodeBrowser.CreateNodeFromStringHandler(this.HandleCreateNodeFromString);
        	// 
        	// FCategoryPanel
        	// 
        	this.FCategoryPanel.Location = new System.Drawing.Point(16, 162);
        	this.FCategoryPanel.Name = "FCategoryPanel";
        	this.FCategoryPanel.NodeBrowser = null;
        	this.FCategoryPanel.Size = new System.Drawing.Size(120, 96);
        	this.FCategoryPanel.TabIndex = 2;
        	this.FCategoryPanel.Visible = false;
        	this.FCategoryPanel.OnCreateNode += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleCreateNode);
        	this.FCategoryPanel.OnShowNodeReference += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowNodeReference);
        	this.FCategoryPanel.OnShowHelpPatch += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowHelpPatch);
        	// 
        	// FTopPanel
        	// 
        	this.FTopPanel.BackColor = System.Drawing.Color.Silver;
        	this.FTopPanel.Controls.Add(this.FTagsTextBox);
        	this.FTopPanel.Controls.Add(this.FTagButton);
        	this.FTopPanel.Controls.Add(this.FCategoryButton);
        	this.FTopPanel.Controls.Add(this.FGirlpowerButton);
        	this.FTopPanel.Controls.Add(this.FFilterButton);
        	this.FTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
        	this.FTopPanel.Location = new System.Drawing.Point(0, 0);
        	this.FTopPanel.Name = "FTopPanel";
        	this.FTopPanel.Size = new System.Drawing.Size(599, 21);
        	this.FTopPanel.TabIndex = 3;
        	// 
        	// FTagsTextBox
        	// 
        	this.FTagsTextBox.AcceptsTab = true;
        	this.FTagsTextBox.BackColor = System.Drawing.Color.Silver;
        	this.FTagsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FTagsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FTagsTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.FTagsTextBox.Location = new System.Drawing.Point(0, 0);
        	this.FTagsTextBox.Multiline = true;
        	this.FTagsTextBox.Name = "FTagsTextBox";
        	this.FTagsTextBox.Size = new System.Drawing.Size(515, 21);
        	this.FTagsTextBox.TabIndex = 4;
        	this.FTagsTextBox.TabStop = false;
        	this.FTagsTextBox.TextChanged += new System.EventHandler(this.FTagsTextBoxTextChanged);
        	this.FTagsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FTagsTextBoxKeyDown);
        	this.FTagsTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FTagsTextBoxMouseUp);
        	// 
        	// FTagButton
        	// 
        	this.FTagButton.BackColor = System.Drawing.Color.Silver;
        	this.FTagButton.Dock = System.Windows.Forms.DockStyle.Right;
        	this.FTagButton.Enabled = false;
        	this.FTagButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FTagButton.Location = new System.Drawing.Point(515, 0);
        	this.FTagButton.Name = "FTagButton";
        	this.FTagButton.Size = new System.Drawing.Size(21, 21);
        	this.FTagButton.TabIndex = 5;
        	this.FTagButton.Text = "T";
        	this.FTagButton.UseVisualStyleBackColor = false;
        	this.FTagButton.Click += new System.EventHandler(this.TopButtonClick);
        	this.FTagButton.MouseEnter += new System.EventHandler(this.TopButtonEnter);
        	this.FTagButton.MouseLeave += new System.EventHandler(this.TopButtonLeave);
        	// 
        	// FCategoryButton
        	// 
        	this.FCategoryButton.BackColor = System.Drawing.Color.Silver;
        	this.FCategoryButton.Dock = System.Windows.Forms.DockStyle.Right;
        	this.FCategoryButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.FCategoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FCategoryButton.Location = new System.Drawing.Point(536, 0);
        	this.FCategoryButton.Name = "FCategoryButton";
        	this.FCategoryButton.Size = new System.Drawing.Size(21, 21);
        	this.FCategoryButton.TabIndex = 2;
        	this.FCategoryButton.Text = "C";
        	this.FCategoryButton.UseVisualStyleBackColor = false;
        	this.FCategoryButton.Click += new System.EventHandler(this.TopButtonClick);
        	this.FCategoryButton.MouseEnter += new System.EventHandler(this.TopButtonEnter);
        	this.FCategoryButton.MouseLeave += new System.EventHandler(this.TopButtonLeave);
        	// 
        	// FGirlpowerButton
        	// 
        	this.FGirlpowerButton.BackColor = System.Drawing.Color.Silver;
        	this.FGirlpowerButton.Dock = System.Windows.Forms.DockStyle.Right;
        	this.FGirlpowerButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.FGirlpowerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FGirlpowerButton.Location = new System.Drawing.Point(557, 0);
        	this.FGirlpowerButton.Name = "FGirlpowerButton";
        	this.FGirlpowerButton.Size = new System.Drawing.Size(21, 21);
        	this.FGirlpowerButton.TabIndex = 1;
        	this.FGirlpowerButton.Text = "G";
        	this.FGirlpowerButton.UseVisualStyleBackColor = false;
        	this.FGirlpowerButton.Visible = false;
        	this.FGirlpowerButton.Click += new System.EventHandler(this.TopButtonClick);
        	this.FGirlpowerButton.MouseEnter += new System.EventHandler(this.TopButtonEnter);
        	this.FGirlpowerButton.MouseLeave += new System.EventHandler(this.TopButtonLeave);
        	// 
        	// FFilterButton
        	// 
        	this.FFilterButton.BackColor = System.Drawing.Color.Silver;
        	this.FFilterButton.Dock = System.Windows.Forms.DockStyle.Right;
        	this.FFilterButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.FFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FFilterButton.Location = new System.Drawing.Point(578, 0);
        	this.FFilterButton.Name = "FFilterButton";
        	this.FFilterButton.Size = new System.Drawing.Size(21, 21);
        	this.FFilterButton.TabIndex = 0;
        	this.FFilterButton.Text = "F";
        	this.FFilterButton.UseVisualStyleBackColor = false;
        	this.FFilterButton.Click += new System.EventHandler(this.TopButtonClick);
        	this.FFilterButton.MouseEnter += new System.EventHandler(this.TopButtonEnter);
        	this.FFilterButton.MouseLeave += new System.EventHandler(this.TopButtonLeave);
        	// 
        	// FCategoryFilterPanel
        	// 
        	this.FCategoryFilterPanel.AutoScroll = true;
        	this.FCategoryFilterPanel.BackColor = System.Drawing.Color.Silver;
        	this.FCategoryFilterPanel.Location = new System.Drawing.Point(37, 407);
        	this.FCategoryFilterPanel.Name = "FCategoryFilterPanel";
        	this.FCategoryFilterPanel.NodeBrowser = null;
        	this.FCategoryFilterPanel.Size = new System.Drawing.Size(240, 69);
        	this.FCategoryFilterPanel.TabIndex = 4;
        	// 
        	// FGirlpowerTagPanel
        	// 
        	this.FGirlpowerTagPanel.AllowDragDrop = false;
        	this.FGirlpowerTagPanel.AndTags = false;
        	this.FGirlpowerTagPanel.Location = new System.Drawing.Point(15, 264);
        	this.FGirlpowerTagPanel.Name = "FGirlpowerTagPanel";
        	this.FGirlpowerTagPanel.NodeBrowser = null;
        	this.FGirlpowerTagPanel.Size = new System.Drawing.Size(121, 103);
        	this.FGirlpowerTagPanel.TabIndex = 5;
        	this.FGirlpowerTagPanel.TagsTextBox = null;
        	// 
        	// NodeBrowserPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.Silver;
        	this.Controls.Add(this.FGirlpowerTagPanel);
        	this.Controls.Add(this.FCategoryFilterPanel);
        	this.Controls.Add(this.FClonePanel);
        	this.Controls.Add(this.FCategoryPanel);
        	this.Controls.Add(this.FNodeTagPanel);
        	this.Controls.Add(this.FTopPanel);
        	this.DoubleBuffered = true;
        	this.Name = "NodeBrowserPluginNode";
        	this.Size = new System.Drawing.Size(599, 479);
        	this.FTopPanel.ResumeLayout(false);
        	this.FTopPanel.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolTip FToolTip;
        private VVVV.Nodes.NodeBrowser.TagPanel FGirlpowerTagPanel;
        private VVVV.Nodes.NodeBrowser.CategoryFilterPanel FCategoryFilterPanel;
        private System.Windows.Forms.Button FFilterButton;
        private System.Windows.Forms.Button FGirlpowerButton;
        private System.Windows.Forms.Button FCategoryButton;
        private System.Windows.Forms.Button FTagButton;
        private System.Windows.Forms.TextBox FTagsTextBox;
        private System.Windows.Forms.Panel FTopPanel;
        private VVVV.Nodes.NodeBrowser.CategoryPanel FCategoryPanel;
        private VVVV.Nodes.NodeBrowser.TagPanel FNodeTagPanel;
        private VVVV.Nodes.NodeBrowser.ClonePanel FClonePanel;
        #endregion constructor/destructor
        
        private uint FLastTimestamp;
        private IWindow2 FLastPatchWindow;
        
        private void RedrawIfNeeded()
        {
            bool isRedrawNeeded = NodeInfoFactory.Timestamp != FLastTimestamp || !CurrentPatchWindow.Equals(FLastPatchWindow);
            if (isRedrawNeeded)
            {
            	UpdatePanels();
            }
            
            FLastTimestamp = NodeInfoFactory.Timestamp;            
            FLastPatchWindow = CurrentPatchWindow;
        }
        
        private void UpdatePanels()
        {
        	FCategoryFilterPanel.Update();
        	
        	if (FNodeTagPanel.Visible)
                FNodeTagPanel.Redraw();
            else
                FNodeTagPanel.PendingRedraw = true;
            
            if (FCategoryPanel.Visible)
                FCategoryPanel.Redraw();
            else
                FCategoryPanel.PendingRedraw = true;
        }
        
        void HandleOnPanelChange(NodeBrowserPage page, INodeInfo nodeInfo)
        {
            switch (page)
            {
                case NodeBrowserPage.NodeTags:
                    {
            			FTagButton.Enabled = false;
        				FCategoryButton.Enabled = true;
		        		FGirlpowerButton.Enabled = true;
        				FFilterButton.Enabled = true;   
        		
        				FNodeTagPanel.Visible = true;
                        FCategoryPanel.Visible = false;
                        FClonePanel.Visible = false;
                        FGirlpowerTagPanel.Visible = false;
                        FCategoryFilterPanel.Visible = false;
        				
        				FTagsTextBox.ReadOnly = false;
        				FTagsTextBox.Text = "";        				
                        break;
                    }
                case NodeBrowserPage.NodeCategories:
                    {
            			FTagButton.Enabled = true;
        				FCategoryButton.Enabled = false;
		        		FGirlpowerButton.Enabled = true;
        				FFilterButton.Enabled = true;
        				
        				FNodeTagPanel.Visible = false;
                        FCategoryPanel.Visible = true;
                        FClonePanel.Visible = false;
                        FGirlpowerTagPanel.Visible = false;
                        FCategoryFilterPanel.Visible = false;
        				
        				FTagsTextBox.Text = "Browse by category";
        				FTagsTextBox.ReadOnly = true;       				
                        break;
                    }
                case NodeBrowserPage.Clone:
                    {
                        var path = CurrentDir;
                        
                        if (nodeInfo.Factory != null && !string.IsNullOrEmpty(path))
                            path = path.ConcatPath(nodeInfo.Factory.JobStdSubPath);
                        else
                            path = "choose a directory to clone to...";
                        FClonePanel.Initialize(nodeInfo, path);
                        
                        FNodeTagPanel.Visible = false;
                        FCategoryPanel.Visible = false;
                        FClonePanel.Visible = true;
                        FGirlpowerTagPanel.Visible = false;
                        FCategoryFilterPanel.Visible = false;
                        
                        FTagsTextBox.Text = "Clone node";
        				FTagsTextBox.ReadOnly = true;
                        break;
                    }
            	case NodeBrowserPage.Girlpower:
                    {
            			FTagButton.Enabled = true;
        				FCategoryButton.Enabled = true;
		        		FGirlpowerButton.Enabled = false;
        				FFilterButton.Enabled = true;
        				
        				FNodeTagPanel.Visible = false;
                        FCategoryPanel.Visible = false;
                        FClonePanel.Visible = false;
                        FGirlpowerTagPanel.Visible = true;
                        FCategoryFilterPanel.Visible = false;
        				
        				FTagsTextBox.ReadOnly = false;
        				FTagsTextBox.Text = "";        				
                        break;
                    }
            		case NodeBrowserPage.Filter:
                    {
            			FTagButton.Enabled = true;
        				FCategoryButton.Enabled = true;
		        		FGirlpowerButton.Enabled = true;
        				FFilterButton.Enabled = false;
        				
        				FNodeTagPanel.Visible = false;
                        FCategoryPanel.Visible = false;
                        FClonePanel.Visible = false;
                        FGirlpowerTagPanel.Visible = false;
                        FCategoryFilterPanel.Visible = true;
        				
        				FTagsTextBox.Text = "Deselect categories to hide in the browser";
        				FTagsTextBox.ReadOnly = true;
                        break;
                    }
            }
            
            RedrawIfNeeded();
        }

        void HandleCreateNode(INodeInfo nodeInfo)
        {
            NodeBrowserHost.CreateNode(nodeInfo);
        }
        
        void HandleCreateNodeFromString(string text)
        {
            if (FInitialText != text)
                NodeBrowserHost.CreateComment(text);
            else
                NodeBrowserHost.CreateNode(null);
        }
        
        void HandleShowNodeReference(INodeInfo nodeInfo)
        {
            HDEHost.ShowNodeReference(nodeInfo);
        }
        
        void HandleShowHelpPatch(INodeInfo nodeInfo)
        {
            HDEHost.ShowHelpPatch(nodeInfo);
        }
        
        void HandleClonePanelClosed(INodeInfo nodeInfo, string Name, string Category, string Version, string path)
        {
            if (nodeInfo != null)
                NodeBrowserHost.CloneNode(nodeInfo, path, Name, Category, Version);
            
            HandleOnPanelChange(NodeBrowserPage.NodeTags, null);
        }
        
        #region INodeBrowser
        public void Initialize(string text)
        {
            IsStandalone = false;
            FNodeTagPanel.NodeBrowser = this;
            FInitialText = text;
            HandleOnPanelChange(NodeBrowserPage.NodeTags, null);
            FNodeTagPanel.Initialize(FInitialText);
        }
        
        public new void DragDrop(bool allow)
        {
            FNodeTagPanel.AllowDragDrop = allow;
        }
        
        public void AfterShow()
        {
            RedrawIfNeeded();
            
            FNodeTagPanel.AfterShow();
        }
        
        public void BeforeHide(out string comment)
        {
            if (string.IsNullOrEmpty(FInitialText) && FNodeTagPanel.Visible)
                comment = FNodeTagPanel.CommentText;
            else
                comment = "";
            
            FNodeTagPanel.BeforeHide();
            FCategoryPanel.BeforeHide();
        }
        #endregion INodeBrowser
        
        void HandleWindowSelectionChanged(object sender, WindowEventArgs args)
        {
            var windowtype = args.Window.WindowType;
            
            if (windowtype == WindowType.Patch || windowtype == WindowType.Module)
            {
                RedrawIfNeeded();
            }
        }
        
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                if (FClonePanel.Visible)
                    FClonePanel.SelectNextControl(FClonePanel.ActiveControl, true, true, true, true);
                else
                {
                    FNodeTagPanel.AndTags = !FNodeTagPanel.AndTags;
                    FNodeTagPanel.Redraw();
                }
                return true;
            }
            else if ((keyData == (Keys.Tab | Keys.Shift)) && (FClonePanel.Visible))
            {
                FClonePanel.SelectNextControl(FClonePanel.ActiveControl, false, true, true, true);
                return true;
            }
            else if (keyData == Keys.Escape && FClonePanel.Visible)
            {
                HandleOnPanelChange(NodeBrowserPage.NodeTags, null);
                return true;
            }
            else
                return base.ProcessDialogKey(keyData);
        }
        
        public void OnImportsSatisfied()
        {
            RedrawIfNeeded();
        }
        
        void TopButtonClick(object sender, EventArgs e)
        {
        	if (sender == FTagButton)
        		HandleOnPanelChange(NodeBrowserPage.NodeTags, null);
        	else if (sender == FCategoryButton)
        		HandleOnPanelChange(NodeBrowserPage.NodeCategories, null);
        	else if (sender == FGirlpowerButton)
        		HandleOnPanelChange(NodeBrowserPage.Girlpower, null);
			else if (sender == FFilterButton)
        		HandleOnPanelChange(NodeBrowserPage.Filter, null);			
        }
        
        #region TagsTextBox
        void FTagsTextBoxTextChanged(object sender, EventArgs e)
        {
        	var CLineHeight = 13;

        	if (FNodeTagPanel.Visible)
        	{
        		FNodeTagPanel.DoTextChanged();
        		FTopPanel.Height = FNodeTagPanel.TextBoxHeight;
        	}
//          else if (!FGirlpowerPanel.Visible)
//            	FGirlpowerTagPanel.TextChanged();
        }
        
        void FTagsTextBoxKeyDown(object sender, KeyEventArgs e)
        {
        	if (FNodeTagPanel.Visible)
                FNodeTagPanel.DoKeyDown(e);
//          else if (!FGirlpowerPanel.Visible)
//            	FGirlpowerTagPanel.DoKeyDown(FTagsTextBox, e);
        }
        
        void FTagsTextBoxMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        	if (FNodeTagPanel.Visible)
                FNodeTagPanel.DoMouseWheel(FTagsTextBox, e);
//          else if (!FGirlpowerPanel.Visible)
//            	FGirlpowerTagPanel.DoMouseWheel(FTagsTextBox, e);
        }
        #endregion
        
        void FTagsTextBoxMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        	if (e.Button == MouseButtons.Right)
        	{
        		if (FNodeTagPanel.Visible)
	                HandleOnPanelChange(NodeBrowserPage.NodeCategories, null);
	        	else if (FCategoryPanel.Visible)
	        		HandleOnPanelChange(NodeBrowserPage.Filter, null);
//	            else if (FGirlpowerTagPanel.Visible)
//	        		HandleOnPanelChange(NodeBrowserPage.Filter, null);
	        	else if (FCategoryFilterPanel.Visible)
	        		HandleOnPanelChange(NodeBrowserPage.NodeTags, null);
        	}
        }
        
        void TopButtonEnter(object sender, EventArgs e)
        {
        	var b = (sender as Button);
        	string text = "";
        	if (b == FTagButton)
        		text = "Browse nodes by tags";
        	else if (b == FCategoryButton)
        		text = "Browse nodes by category";
        	else if (b == FGirlpowerButton)
        		text = "Browse girlpower demos";
        	else if (b == FFilterButton)
        		text = "Set node category filter";
        	
        	FToolTip.Show(text, this, b.Left, b.Bottom + 10);
        }
        
        void TopButtonLeave(object sender, EventArgs e)
        {
        	FToolTip.Hide(this);
        }
    }

    public enum NodeBrowserPage {NodeTags, NodeCategories, Girlpower, Clone, Filter};
    public delegate void PanelChangeHandler(NodeBrowserPage page, INodeInfo nodeInfo);
    public delegate void CreateNodeHandler(INodeInfo nodeInfo);
    public delegate void CreateNodeFromStringHandler(string text);
}
