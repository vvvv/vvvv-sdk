#region usings
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.Practices.Unity;

using VVVV.Core;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.HDE.Viewer;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.HDE.Viewer.WinFormsViewer;
#endregion usings

namespace VVVV.Nodes.Finder
{
    public enum SearchScope {Global, Local, Downstream};
    
    [PluginInfo(Name = "Finder",
                Category = "HDE",
                Shortcut = "Ctrl+F",
                Author = "vvvv group",
                Help = "Finds Nodes, Comments and Send/Receive channels.",
                InitialBoxWidth = 200,
                InitialBoxHeight = 100,
                InitialWindowWidth = 420,
                InitialWindowHeight = 630,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class FinderPluginNode: UserControl, IPluginHDE, IWindowSelectionListener
    {
        #region field declaration
        
        //the host (mandatory)
        private IHDEHost FHDEHost;
        private List<PatchNode> FPlainSearchList = new List<PatchNode>();
        private SearchScope FSearchScope = SearchScope.Local;
        
        private IWindow FActiveWindow;
        private PatchNode FActivePatchNode;
        private PatchNode FRoot;
        
        private bool FSendReceive;
        private bool FComments;
        private bool FLabels;
        private bool FEffects;
        private bool FFreeframes;
        private bool FModules;
        private bool FPlugins;
        private bool FIONodes;
        private bool FNatives;
        private bool FVSTs;
        private bool FTexts;
        private bool FUnknowns;
        private bool FBoygrouped;
        private bool FAddons;
        
        private List<string> FTags;
        private int FSearchIndex;
        
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //further fields
        System.Collections.Generic.List<INode> FNodes = new List<INode>();
        
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public FinderPluginNode(IHDEHost host)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FHDEHost = host;
            
            FSearchTextBox.ContextMenu = new ContextMenu();
            FSearchTextBox.ContextMenu.Popup += new EventHandler(FSearchTextBox_ContextMenu_Popup);
            
            
            INode root;
            FHDEHost.GetRoot(out root);
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            
            FHierarchyViewer.Registry = mappingRegistry;
            FRoot = new PatchNode(root);
            FRoot.Added += new CollectionDelegate(root_Added);
            //FHierarchyViewer.Input = FRoot;
            
            //this will trigger the initial WindowSelectionChangeCB
            FHDEHost.AddListener(this);
        }

        void FSearchTextBox_ContextMenu_Popup(object sender, EventArgs e)
        {
            FSearchTextBox.Text = "";
        }

        void root_Added(IViewableCollection collection, object item)
        {
            FHierarchyViewer.Reload();
        }
        
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.FSelectedPatchLabel = new System.Windows.Forms.Label();
            this.FNodeCountLabel = new System.Windows.Forms.Label();
            this.FHierarchyViewer = new VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.FSearchTextBox = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.FSelectedPatchLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(252, 34);
            this.panel1.TabIndex = 7;
            // 
            // FSelectedPatchLabel
            // 
            this.FSelectedPatchLabel.AutoEllipsis = true;
            this.FSelectedPatchLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.FSelectedPatchLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FSelectedPatchLabel.Location = new System.Drawing.Point(0, 0);
            this.FSelectedPatchLabel.Name = "FSelectedPatchLabel";
            this.FSelectedPatchLabel.Size = new System.Drawing.Size(250, 17);
            this.FSelectedPatchLabel.TabIndex = 9;
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
            // 
            // FHierarchyViewer
            // 
            this.FHierarchyViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FHierarchyViewer.Location = new System.Drawing.Point(0, 34);
            this.FHierarchyViewer.Name = "FHierarchyViewer";
            this.FHierarchyViewer.Size = new System.Drawing.Size(252, 222);
            this.FHierarchyViewer.TabIndex = 9;
            this.FHierarchyViewer.Click += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerClick);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.FSearchTextBox);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 17);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(250, 15);
            this.panel3.TabIndex = 11;
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
            this.FSearchTextBox.TextChanged += new System.EventHandler(this.FFindTextBoxTextChanged);
            this.FSearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FSearchTextBoxKeyDown);
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
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label FSelectedPatchLabel;
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
                    FHDEHost.RemoveListener(this);
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
        
        #region IWindowSelectionListener
        public void WindowSelectionChangeCB(IWindow window)
        {
            var windowType = window.GetWindowType();
            
            if (windowType == WindowType.Module || windowType == WindowType.Patch)
            {
                if (window != FActiveWindow)
                {
                    if (FActivePatchNode != null)
                        FActivePatchNode.UnSubscribe();
                    
                    FSelectedPatchLabel.Text = window.Caption;
                    FActivePatchNode = new PatchNode(window.GetNode());
                    UpdateSearch();
                    
                    FActiveWindow = window;
                }
            }
            //todo: mark current patch like "you are here"
        }
        #endregion IWindowSelectionListener
        
        public void UpdateView()
        {
            FHierarchyViewer.Reload();
        }
        /*
        private INode FindParent(INode parent, INode target)
        {
            INode[] children = parent.GetChildren();
            
            if (children != null)
            {
                foreach(INode child in children)
                {
                    if (child == target)
                        return parent;
                    else
                    {
                        INode p = FindParent(child, target);
                        if (p != null)
                            return p;
                    }
                }
                return null;
            }
            else
                return null;
        }
         */
        #region Search
        void FFindTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdateSearch();
        }
        
        private void AddNodesByTag(PatchNode searchResult, PatchNode sourceTree)
        {
            //go through child nodes of sourceTree recursively and see if any contains the tag
            foreach (PatchNode node in sourceTree)
            {
                //now first go downstream recursively
                //to see if this pn is needed in the hierarchy to hold any matching downstream nodes
                //create a dummy to attach possible matching downstream nodes
                var parent = new PatchNode(null);
                parent.Node = node.Node;
                AddNodesByTag(parent, node);
                
                if (parent.Count > 0 || CheckForInclusion(parent))
                {
                    searchResult.Add(parent);
                    FPlainSearchList.Add(parent);
                }
            }
        }
        
        private bool CheckForInclusion(PatchNode node)
        {
            bool include = false;
            var quickTagsUsed = FSendReceive || FComments || FLabels || FIONodes || FNatives || FModules || FEffects || FFreeframes || FVSTs || FPlugins || FTexts || FUnknowns || FAddons || FBoygrouped;
            
            if (FTags.Count == 0)
            {
                if (quickTagsUsed)
                {
                    include = FSendReceive && !string.IsNullOrEmpty(node.SRChannel);
                    include |= FComments && node.Name.StartsWith("//");
                    include |= FLabels && !string.IsNullOrEmpty(node.DescriptiveName);
                    include |= FIONodes && node.IsIONode;
                    include |= FNatives && node.NodeType == NodeType.Native;
                    include |= FModules && node.NodeType == NodeType.Module;
                    include |= FEffects && node.NodeType == NodeType.Effect;
                    include |= FFreeframes && node.NodeType == NodeType.Freeframe;
                    include |= FVSTs && node.NodeType == NodeType.VST;
                    include |= FPlugins && (node.NodeType == NodeType.Plugin || node.NodeType == NodeType.Dynamic);
                    include |= FTexts && node.NodeType == NodeType.Text;
                    include |= FUnknowns && node.IsMissing;
                    include |= FBoygrouped && node.IsBoygrouped;
                    include |= FAddons && (node.NodeType != NodeType.Native && node.NodeType != NodeType.Text && node.NodeType != NodeType.Patch);
                }
                else
                    include = true;
            }
            else
            {
                if (FSendReceive && !string.IsNullOrEmpty(node.SRChannel))
                {
                    var inc = true;
                    var channel = node.SRChannel.ToLower();
                    
                    foreach (string tag in FTags)
                        inc = inc && channel.Contains(tag);
                    include |= inc;
                }
                if (FComments && !string.IsNullOrEmpty(node.Comment))
                {
                    var inc = true;
                    var comment = node.Comment.ToLower();
                    
                    foreach (string tag in FTags)
                        inc = inc && comment.Contains(tag);
                    include |= inc;
                }
                if (FIONodes && node.IsIONode)
                {
                    var inc = true;
                    var dname = node.DescriptiveName.ToLower();
                    
                    foreach (string tag in FTags)
                        inc = inc && dname.Contains(tag);
                    include |= inc;
                }
                if (FLabels && !string.IsNullOrEmpty(node.DescriptiveName))
                {
                    var inc = true;
                    var dname = node.DescriptiveName.ToLower();
                    
                    foreach (string tag in FTags)
                        inc = inc && dname.Contains(tag);
                    include |= inc;
                }
                if ((FEffects && node.NodeType == NodeType.Effect)
                    || (FModules && node.NodeType == NodeType.Module)
                    || (FPlugins && (node.NodeType == NodeType.Plugin || node.NodeType == NodeType.Dynamic))
                    || (FFreeframes && node.NodeType == NodeType.Freeframe)
                    || (FNatives && node.NodeType == NodeType.Native)
                    || (FVSTs && node.NodeType == NodeType.VST)
                    || (FTexts && node.NodeType == NodeType.Text)
                    || (FUnknowns && node.IsMissing)
                    || (FBoygrouped && node.IsBoygrouped)
                    || (FAddons && (node.NodeType != NodeType.Native && node.NodeType != NodeType.Text && node.NodeType != NodeType.Patch)))
                {
                    var inc = true;
                    var name = node.Name.ToLower();
                    
                    foreach (string tag in FTags)
                        inc = inc && name.Contains(tag);
                    include |= inc;
                }
                
                //if non of the one-character tags is chosen
                if (!quickTagsUsed)
                {
                    var inc = true;
                    var name = node.Name.ToLower();
                    
                    foreach (string tag in FTags)
                        inc = inc && name.Contains(tag);
                    include |= inc;
                }
            }
            
            if (include)
                node.SetTags(FTags);
            
            return include;
        }
        
        private void UpdateSearch()
        {
            string query = FSearchTextBox.Text.ToLower();
            
            query += (char) 160;
            //if the last character
            
            var searchResult = new PatchNode(null);
            FPlainSearchList.Clear();
            FSearchIndex = 0;
            
            //check for tags in query:
            //g: global
            //b: below local
            //default scope: local
            FSearchScope = SearchScope.Local;
            FTags = query.Split(new char[1]{' '}).ToList();
            
            if (FTags.Contains("g"))
            {
                FSearchScope = SearchScope.Global;
                FTags.Remove("g");
            }
            else if (FTags.Contains("d"))
            {
                FSearchScope = SearchScope.Downstream;
                FTags.Remove("d");
            }
            
            FSendReceive = FComments = FLabels = FEffects = FFreeframes = FModules = FPlugins = FIONodes = FNatives = FVSTs = FAddons = FUnknowns = FTexts = FBoygrouped = false;
            //s: send/receive channels
            //c: comments
            //d: descriptive names
            if (FTags.Contains("s"))
            {
                FSendReceive = true;
                FTags.Remove("s");
            }
            if (FTags.Contains("//"))
            {
                FComments = true;
                FTags.Remove("//");
            }
            if (FTags.Contains("l"))
            {
                FLabels = true;
                FTags.Remove("l");
            }
            if (FTags.Contains("x"))
            {
                FEffects = true;
                FTags.Remove("x");
            }
            if (FTags.Contains("f"))
            {
                FFreeframes = true;
                FTags.Remove("f");
            }
            if (FTags.Contains("m"))
            {
                FModules = true;
                FTags.Remove("m");
            }
            if (FTags.Contains("p"))
            {
                FPlugins = true;
                FTags.Remove("p");
            }
            if (FTags.Contains("i"))
            {
                FIONodes = true;
                FTags.Remove("i");
            }
            if (FTags.Contains("n"))
            {
                FNatives = true;
                FTags.Remove("n");
            }
            if (FTags.Contains("v"))
            {
                FVSTs = true;
                FTags.Remove("v");
            }
            if (FTags.Contains("t"))
            {
                FTexts = true;
                FTags.Remove("t");
            }
            if (FTags.Contains("r"))
            {
                FUnknowns = true;
                FTags.Remove("r");
            }
            if (FTags.Contains("a"))
            {
                FAddons = true;
                FTags.Remove("a");
            }
            if (FTags.Contains("b"))
            {
                FBoygrouped = true;
                FTags.Remove("b");
            }
            
            FTags[FTags.Count-1] = FTags[FTags.Count-1].Trim((char) 160);
            
            while (FTags.Contains(" "))
                FTags.Remove(" ");
            if (FTags.Contains(""))
                FTags.Remove("");
            
            switch (FSearchScope)
            {
                case SearchScope.Global:
                    {
                        //go through child nodes of FRoot recursively and see if any contains the tag
                        AddNodesByTag(searchResult, FRoot);
                        break;
                    }
                case SearchScope.Local:
                    {
                        //go through child nodes of FActivePatch and see if any contains the tag
                        foreach (PatchNode pn in FActivePatchNode)
                            if (CheckForInclusion(pn))
                        {
                            var node = new PatchNode(null);
                            node.Node = pn.Node;
                            searchResult.Add(node);
                            FPlainSearchList.Add(node);
                        }
                        break;
                    }
                case SearchScope.Downstream:
                    {
                        //go through child nodes of FActivePatch recursively and see if any contains the tag
                        AddNodesByTag(searchResult, FActivePatchNode);
                        break;
                    }
            }
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            
            FHierarchyViewer.Registry = mappingRegistry;
            FHierarchyViewer.Input = searchResult;

            FNodeCountLabel.Text = "Matching Nodes: " + FPlainSearchList.Count.ToString();
        }
        #endregion Search
        
        void FSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                FPlainSearchList[FSearchIndex].Selected = false;
                if (e.Shift)
                {
                    FSearchIndex -= 1;
                    if (FSearchIndex < 0)
                        FSearchIndex = FPlainSearchList.Count - 1;
                }
                else
                    FSearchIndex = (FSearchIndex + 1) % FPlainSearchList.Count;
                
                FPlainSearchList[FSearchIndex].Selected = true;
                
                //open the patch this node is in
                //FGraphViewerHost.ShowPatchOfNode(FPlainSearchList[FSearchIndex].Node);
                
                //select the node
                FHDEHost.SelectNodes(new INode[1]{FPlainSearchList[FSearchIndex].Node});
                
                //refocus in order to allow further next search in case a patch was opened
                //FSearchTextBox.Focus();
                
                FHierarchyViewer.Redraw();
            }
        }
        
        void FHierarchyViewerClick(IModelMapper sender, MouseEventArgs e)
        {
            (sender.Model as PatchNode).Selected = true;
            FHDEHost.SelectNodes(new INode[1]{(sender.Model as PatchNode).Node});
            
            if (sender.CanMap<ICamera>())
            {
                if (e.Button == 0)
                    sender.Map<ICamera>().View(sender.Model);
                else if ((int)e.Button == 1)
                    sender.Map<ICamera>().ViewAll();
                else if ((int)e.Button == 2)
                    sender.Map<ICamera>().ViewParent(sender.Model);
            }
        }
    }
}
