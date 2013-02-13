#region usings
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel.Composition;

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
            FTagPanel.Dock = DockStyle.Fill;
            
            FClonePanel.NodeBrowser = this;
            FTagPanel.NodeBrowser = this;
            FCategoryPanel.NodeBrowser = this;
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
            this.FClonePanel = new VVVV.Nodes.NodeBrowser.ClonePanel();
            this.FTagPanel = new VVVV.Nodes.NodeBrowser.TagPanel();
            this.FCategoryPanel = new VVVV.Nodes.NodeBrowser.CategoryPanel();
            this.SuspendLayout();
            // 
            // FClonePanel
            // 
            this.FClonePanel.BackColor = System.Drawing.Color.Silver;
            this.FClonePanel.Location = new System.Drawing.Point(241, 82);
            this.FClonePanel.Name = "FClonePanel";
            this.FClonePanel.NodeBrowser = null;
            this.FClonePanel.Padding = new System.Windows.Forms.Padding(8);
            this.FClonePanel.Size = new System.Drawing.Size(250, 300);
            this.FClonePanel.TabIndex = 0;
            this.FClonePanel.Visible = false;
            // 
            // FTagPanel
            // 
            this.FTagPanel.AllowDragDrop = true;
            this.FTagPanel.AndTags = true;
            this.FTagPanel.Location = new System.Drawing.Point(17, 24);
            this.FTagPanel.Name = "FTagPanel";
            this.FTagPanel.NodeBrowser = null;
            this.FTagPanel.Size = new System.Drawing.Size(120, 115);
            this.FTagPanel.TabIndex = 1;
            this.FTagPanel.OnCreateNode += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleCreateNode);
            this.FTagPanel.OnPanelChange += new VVVV.Nodes.NodeBrowser.PanelChangeHandler(this.HandleOnPanelChange);
            this.FTagPanel.OnShowHelpPatch += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowHelpPatch);
            this.FTagPanel.OnCreateNodeFromString += new VVVV.Nodes.NodeBrowser.CreateNodeFromStringHandler(this.HandleCreateNodeFromString);
            this.FTagPanel.OnShowNodeReference += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowNodeReference);
            // 
            // FCategoryPanel
            // 
            this.FCategoryPanel.Location = new System.Drawing.Point(17, 161);
            this.FCategoryPanel.Name = "FCategoryPanel";
            this.FCategoryPanel.Size = new System.Drawing.Size(119, 85);
            this.FCategoryPanel.TabIndex = 2;
            this.FCategoryPanel.Visible = false;
            this.FCategoryPanel.OnCreateNode += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleCreateNode);
            this.FCategoryPanel.OnPanelChange += new VVVV.Nodes.NodeBrowser.PanelChangeHandler(this.HandleOnPanelChange);
            this.FCategoryPanel.OnShowHelpPatch += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowHelpPatch);
            this.FCategoryPanel.OnShowNodeReference += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.HandleShowNodeReference);
            // 
            // NodeBrowserPluginNode
            // 
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.FCategoryPanel);
            this.Controls.Add(this.FTagPanel);
            this.Controls.Add(this.FClonePanel);
            this.DoubleBuffered = true;
            this.Name = "NodeBrowserPluginNode";
            this.Size = new System.Drawing.Size(599, 479);
            this.ResumeLayout(false);
        }
        private VVVV.Nodes.NodeBrowser.CategoryPanel FCategoryPanel;
        private VVVV.Nodes.NodeBrowser.TagPanel FTagPanel;
        private VVVV.Nodes.NodeBrowser.ClonePanel FClonePanel;
        #endregion constructor/destructor
        
        private uint FLastTimestamp;
        private IWindow2 FLastPatchWindow;
        
        private void RedrawIfNeeded()
        {
            bool isRedrawNeeded = NodeInfoFactory.Timestamp != FLastTimestamp || !CurrentPatchWindow.Equals(FLastPatchWindow);
            if (isRedrawNeeded)
            {
                if (FTagPanel.Visible)
                {
                    FTagPanel.Redraw();
                }
                else
                {
                    FTagPanel.PendingRedraw = true;
                }
                
                if (FCategoryPanel.Visible)
                {
                    FCategoryPanel.Redraw();
                }
                else
                {
                    FCategoryPanel.PendingRedraw = true;
                }
            }
            
            FLastTimestamp = NodeInfoFactory.Timestamp;            
            FLastPatchWindow = CurrentPatchWindow;
        }
        
        void HandleOnPanelChange(NodeBrowserPage page, INodeInfo nodeInfo)
        {
            switch (page)
            {
                case NodeBrowserPage.ByCategory:
                    {
                        FClonePanel.Visible = false;
                        FTagPanel.Visible = false;
                        
                        FCategoryPanel.Visible = true;
                        break;
                    }
                case NodeBrowserPage.ByTags:
                    {
                        FClonePanel.Visible = false;
                        FCategoryPanel.Visible = false;
                        
                        FTagPanel.Visible = true;
                        break;
                    }
                case NodeBrowserPage.Clone:
                    {
                        FTagPanel.Visible = false;
                        FCategoryPanel.Visible = false;
                        
                        FClonePanel.Visible = true;
                        
                        var path = CurrentDir;
                        
                        if (nodeInfo.Factory != null && !string.IsNullOrEmpty(path))
                            path = path.ConcatPath(nodeInfo.Factory.JobStdSubPath);
                        else
                            path = "choose a directory to clone to...";
                        FClonePanel.Initialize(nodeInfo, path);
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
            
            HandleOnPanelChange(NodeBrowserPage.ByTags, null);
        }
        
        #region INodeBrowser
        public void Initialize(string text)
        {
            IsStandalone = false;
            FTagPanel.NodeBrowser = this;
            FInitialText = text;
            FTagPanel.Initialize(FInitialText);
            HandleOnPanelChange(NodeBrowserPage.ByTags, null);
        }
        
        public new void DragDrop(bool allow)
        {
            FTagPanel.AllowDragDrop = allow;
        }
        
        public void AfterShow()
        {
            RedrawIfNeeded();
            
            FTagPanel.AfterShow();
        }
        
        public void BeforeHide(out string comment)
        {
            if (string.IsNullOrEmpty(FInitialText))
                comment = FTagPanel.CommentText;
            else
                comment = "";
            
            FTagPanel.BeforeHide();
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
                    FTagPanel.AndTags = !FTagPanel.AndTags;
                    FTagPanel.Redraw();
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
                HandleOnPanelChange(NodeBrowserPage.ByTags, null);
                return true;
            }
            else
                return base.ProcessDialogKey(keyData);
        }
        
        public void OnImportsSatisfied()
        {
            RedrawIfNeeded();
        }
    }

    public enum NodeBrowserPage {ByCategory, ByTags, Clone};
    public delegate void PanelChangeHandler(NodeBrowserPage page, INodeInfo nodeInfo);
    public delegate void CreateNodeHandler(INodeInfo nodeInfo);
    public delegate void CreateNodeFromStringHandler(string text);
}
