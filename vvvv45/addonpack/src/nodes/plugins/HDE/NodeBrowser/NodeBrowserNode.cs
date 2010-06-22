#region licence/info

//////project name
//vvvv plugin template with gui

//////description
//basic vvvv plugin template with gui.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using Microsoft.Practices.Unity;

using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Viewer.Model;

//the vvvv node namespace
namespace VVVV.Nodes.NodeBrowser
{
    enum NodeBrowserPage {ByCategory, ByTags};
    
    //class definition, inheriting from UserControl for the GUI stuff
    public class NodeBrowserPluginNode: UserControl, IHDEPlugin, INodeInfoListener, INodeBrowser
    {
        #region field declaration
        
        //the hosts
        private IPluginHost FPluginHost;
        private IHDEHost FHDEHost;
        private INodeBrowserHost FNodeBrowserHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        //further fields
        NodeListModel FCategoryModel = new NodeListModel();
        List<string> FNodeList = new List<string>();
        List<string> FSelectionList;
        List<string> FRTFSelectionList = new List<string>();
        private string[] FTags = new string[0];
        Dictionary<string, INodeInfo> FNodeDict = new Dictionary<string, INodeInfo>();
        private bool FAndTags = true;
        private int FSelectedLine = -1;
        private int FHoverLine = -1;
        private Point FLastMouseHoverLocation = new Point(0, 0);
        private string FManualEntry = "";
        private int FAwesomeWidth = 200;
        private bool FCtrlPressed = false;
        private int FVisibleLines = 16;
        private string FPath;
        private ToolTip FToolTip = new ToolTip();
        private bool FShowHover = false;
        private int FNodeFilter;
        
        private Color CLabelColor = Color.FromArgb(255, 154, 154, 154);
        private Color CHoverColor = Color.FromArgb(255, 216, 216, 216);
        private const string CRTFHeader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Verdana;}}\viewkind4\uc1\pard\f0\fs17 ";
        
        private int FScrolledLine;
        private int ScrolledLine
        {
            get {return FScrolledLine;}
            set
            {
                FScrolledLine = Math.Min(FScrollBar.Maximum - FVisibleLines + FScrollBar.LargeChange - 3, Math.Max(0, value));
                FScrollBar.Value = FScrolledLine;
                UpdateRichTextBox();
            }
        }
        #endregion field declaration
        
        #region constructor/destructor
        public NodeBrowserPluginNode()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FTagsTextBox.ContextMenu = new ContextMenu();
            FTagsTextBox.MouseWheel += new MouseEventHandler(TextBoxTagsMouseWheel);
            
            FToolTip.BackColor = CLabelColor;
            FToolTip.ForeColor = Color.White;
            FToolTip.ShowAlways = true;
            FToolTip.Popup += new PopupEventHandler(ToolTipPopupHandler);
            
            SelectPage(NodeBrowserPage.ByTags);
        }
        
        private void ToolTipPopupHandler(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(Math.Min(e.ToolTipSize.Width, 300), e.ToolTipSize.Height);
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
                    FHDEHost.RemoveListener(this);
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
        
        #endregion constructor/destructor
        
        #region node name and infos
        
        //provide node infos
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();
                    
                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "NodeBrowser";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HDE";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";
                    
                    FPluginInfo.ShortCut = "Ctrl+N";
                    
                    //the nodes author: your sign
                    FPluginInfo.Author = "vvvv group";
                    //describe the nodes function
                    FPluginInfo.Help = "The NodeInfo Browser";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";
                    
                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";
                    
                    //define the nodes initial size in box-mode
                    FPluginInfo.InitialBoxSize = new Size(100, 200);
                    //define the nodes initial size in window-mode
                    FPluginInfo.InitialWindowSize = new Size(300, 500);
                    //define the nodes initial component mode
                    FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
                    
                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }
        
        #endregion node name and infos
        
        private void InitializeComponent()
        {
            this.FNodeCountLabel = new System.Windows.Forms.Label();
            this.FTagPanel = new System.Windows.Forms.Panel();
            this.FRichTextBox = new System.Windows.Forms.RichTextBox();
            this.FNodeTypePanel = new VVVV.Nodes.DoubleBufferedPanel();
            this.FScrollBar = new System.Windows.Forms.VScrollBar();
            this.FTagsTextBox = new System.Windows.Forms.TextBox();
            this.FCategoryPanel = new System.Windows.Forms.Panel();
            this.CategoryTreeViewer = new VVVV.HDE.Viewer.TreeViewer();
            this.FTopLabel = new System.Windows.Forms.Label();
            this.FTagPanel.SuspendLayout();
            this.FCategoryPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // FNodeCountLabel
            // 
            this.FNodeCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FNodeCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FNodeCountLabel.Location = new System.Drawing.Point(0, 506);
            this.FNodeCountLabel.Name = "FNodeCountLabel";
            this.FNodeCountLabel.Size = new System.Drawing.Size(325, 14);
            this.FNodeCountLabel.TabIndex = 3;
            // 
            // FTagPanel
            // 
            this.FTagPanel.Controls.Add(this.FRichTextBox);
            this.FTagPanel.Controls.Add(this.FNodeTypePanel);
            this.FTagPanel.Controls.Add(this.FScrollBar);
            this.FTagPanel.Controls.Add(this.FTagsTextBox);
            this.FTagPanel.Location = new System.Drawing.Point(4, 33);
            this.FTagPanel.Name = "FTagPanel";
            this.FTagPanel.Size = new System.Drawing.Size(144, 440);
            this.FTagPanel.TabIndex = 4;
            // 
            // FRichTextBox
            // 
            this.FRichTextBox.BackColor = System.Drawing.Color.Silver;
            this.FRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FRichTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.FRichTextBox.DetectUrls = false;
            this.FRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FRichTextBox.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FRichTextBox.Location = new System.Drawing.Point(20, 20);
            this.FRichTextBox.Name = "FRichTextBox";
            this.FRichTextBox.ReadOnly = true;
            this.FRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Horizontal;
            this.FRichTextBox.Size = new System.Drawing.Size(107, 420);
            this.FRichTextBox.TabIndex = 2;
            this.FRichTextBox.TabStop = false;
            this.FRichTextBox.Text = "";
            this.FRichTextBox.WordWrap = false;
            this.FRichTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseUp);
            this.FRichTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
            this.FRichTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
            // 
            // FNodeTypePanel
            // 
            this.FNodeTypePanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.FNodeTypePanel.Location = new System.Drawing.Point(0, 20);
            this.FNodeTypePanel.Name = "FNodeTypePanel";
            this.FNodeTypePanel.Size = new System.Drawing.Size(20, 420);
            this.FNodeTypePanel.TabIndex = 4;
            this.FNodeTypePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.FNodeTypePanelPaint);
            // 
            // FScrollBar
            // 
            this.FScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.FScrollBar.Location = new System.Drawing.Point(127, 20);
            this.FScrollBar.Name = "FScrollBar";
            this.FScrollBar.Size = new System.Drawing.Size(17, 420);
            this.FScrollBar.TabIndex = 3;
            this.FScrollBar.Value = 100;
            this.FScrollBar.ValueChanged += new System.EventHandler(this.FScrollBarValueChanged);
            // 
            // FTagsTextBox
            // 
            this.FTagsTextBox.AcceptsTab = true;
            this.FTagsTextBox.BackColor = System.Drawing.Color.Silver;
            this.FTagsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FTagsTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FTagsTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FTagsTextBox.Location = new System.Drawing.Point(0, 0);
            this.FTagsTextBox.Multiline = true;
            this.FTagsTextBox.Name = "FTagsTextBox";
            this.FTagsTextBox.Size = new System.Drawing.Size(144, 20);
            this.FTagsTextBox.TabIndex = 1;
            this.FTagsTextBox.TabStop = false;
            this.FTagsTextBox.TextChanged += new System.EventHandler(this.TextBoxTagsTextChanged);
            this.FTagsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxTagsKeyDown);
            this.FTagsTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxTagsKeyUp);
            this.FTagsTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextBoxTagsMouseDown);
            // 
            // FCategoryPanel
            // 
            this.FCategoryPanel.Controls.Add(this.CategoryTreeViewer);
            this.FCategoryPanel.Controls.Add(this.FTopLabel);
            this.FCategoryPanel.Location = new System.Drawing.Point(165, 33);
            this.FCategoryPanel.Name = "FCategoryPanel";
            this.FCategoryPanel.Size = new System.Drawing.Size(159, 439);
            this.FCategoryPanel.TabIndex = 5;
            // 
            // CategoryTreeViewer
            // 
            this.CategoryTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CategoryTreeViewer.FlatStyle = false;
            this.CategoryTreeViewer.Location = new System.Drawing.Point(0, 15);
            this.CategoryTreeViewer.Name = "CategoryTreeViewer";
            this.CategoryTreeViewer.ShowLines = false;
            this.CategoryTreeViewer.ShowPlusMinus = false;
            this.CategoryTreeViewer.ShowRoot = false;
            this.CategoryTreeViewer.ShowRootLines = false;
            this.CategoryTreeViewer.ShowTooltip = true;
            this.CategoryTreeViewer.Size = new System.Drawing.Size(159, 424);
            this.CategoryTreeViewer.TabIndex = 1;
            this.CategoryTreeViewer.LeftClick += new System.EventHandler(this.CategoryTreeViewerLeftClick);
            // 
            // FTopLabel
            // 
            this.FTopLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(154)))), ((int)(((byte)(154)))));
            this.FTopLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FTopLabel.Location = new System.Drawing.Point(0, 0);
            this.FTopLabel.Name = "FTopLabel";
            this.FTopLabel.Size = new System.Drawing.Size(159, 15);
            this.FTopLabel.TabIndex = 7;
            this.FTopLabel.Text = "Click here to browse by tags";
            this.FTopLabel.Click += new System.EventHandler(this.FTopLabelClick);
            // 
            // NodeBrowserPluginNode
            // 
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.FCategoryPanel);
            this.Controls.Add(this.FTagPanel);
            this.Controls.Add(this.FNodeCountLabel);
            this.Name = "NodeBrowserPluginNode";
            this.Size = new System.Drawing.Size(325, 520);
            this.FTagPanel.ResumeLayout(false);
            this.FTagPanel.PerformLayout();
            this.FCategoryPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private VVVV.Nodes.DoubleBufferedPanel FNodeTypePanel;
        private System.Windows.Forms.VScrollBar FScrollBar;
        private System.Windows.Forms.Label FNodeCountLabel;
        private System.Windows.Forms.Label FTopLabel;
        private System.Windows.Forms.Panel FCategoryPanel;
        private System.Windows.Forms.TextBox FTagsTextBox;
        private RichTextBox FRichTextBox;
        private System.Windows.Forms.Panel FTagPanel;
        private VVVV.HDE.Viewer.TreeViewer CategoryTreeViewer;
        
        #region initialization
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost host)
        {
            FPluginHost = host;
        }
        
        public void SetHDEHost(IHDEHost host)
        {
            //assign host
            FHDEHost = host;
            
            //register nodeinfolisteners at hdehost
            FHDEHost.AddListener(this);
            
            //now create a child container, which knows how to map the HDE model.
            var cc = FHDEHost.UnityContainer.CreateChildContainer();
            cc.AddNewExtension<NodeBrowserModelContainerExtension>();
            
            //create a IContentProvider and hand it to the treeViewer
            var cp = new UnityContentProvider(cc);
            CategoryTreeViewer.SetContentProvider(cp);
            
            //create ILabelProvider and hand it to the treeViewer
            var lp = new UnityLabelProvider(cc);
            CategoryTreeViewer.SetLabelProvider(lp);
            
            //hand model root over to viewers
            CategoryTreeViewer.SetRoot(FCategoryModel);
        }

        #endregion initialization
        
        #region INodeBrowser
        public void SetNodeBrowserHost(INodeBrowserHost host)
        {
            FNodeBrowserHost = host;
        }
        
        public void Initialize(string path, string text, out int width)
        {
            FPath = path;
            width = FAwesomeWidth;
            
            if (!string.IsNullOrEmpty(text))
                FManualEntry = text.Trim();
            else
                FManualEntry = "";
            FTagsTextBox.Text = FManualEntry;
            FTagsTextBox.SelectAll();
            
            FSelectedLine = -1;
            FHoverLine = -1;
            
            //init view
            UpdateOutput();
        }
        
        public void AfterShow()
        {
            FTagsTextBox.Focus();
        }
        
        public void BeforeHide()
        {
            FToolTip.Hide(FRichTextBox);
        }
        
        private void CreateNode()
        {
            string text = FTagsTextBox.Text.Trim();
            try
            {
                INodeInfo selNode = FNodeDict[text];
                FNodeBrowserHost.CreateNode(selNode);
            }
            catch
            {
                if ((text.Contains(".v4p")) || (text.Contains(".fx")) || (text.Contains(".dll")))
                    FNodeBrowserHost.CreateNodeFromFile(FPath + text);
                else
                    FNodeBrowserHost.CreateComment(FTagsTextBox.Text);
            }
        }
        #endregion INodeBrowser
        
        #region INodeInfoListener
        public void NodeInfoAddedCB(INodeInfo nodeInfo)
        {
            string nodeVersion = nodeInfo.Version;
            string nodeAuthor = nodeInfo.Author;
            string nodeTags = nodeInfo.Tags;

            //don't include legacy nodes in the list
            if ((string.IsNullOrEmpty(nodeVersion)) || (!nodeVersion.ToLower().Contains("legacy")))
            {
                string tags = nodeTags;
                if (nodeAuthor != "vvvv group")
                    tags += ", " + nodeAuthor;
                string key;
                if (!string.IsNullOrEmpty(nodeInfo.Tags))
                    key = nodeInfo.Username + " [" + tags + "]";
                else
                    key = nodeInfo.Username;
                
                FNodeList.Add(key);
                FNodeDict[key] = nodeInfo;
                
                Size s = TextRenderer.MeasureText(key, FRichTextBox.Font, new Size(1, 1));
                FAwesomeWidth = Math.Max(FAwesomeWidth, s.Width);
                
                //insert nodeInfo to NodeListModel
                var nodeInfoEntry = FHDEHost.UnityContainer.BuildUp(new NodeInfoEntry(nodeInfo));
                CategoryEntry catEntry;
                if (FCategoryModel.Contains(nodeInfoEntry.Category))
                {
                    catEntry = FCategoryModel.GetCategoryEntry(nodeInfoEntry.Category);
                    catEntry.Add(nodeInfoEntry);
                }
                else
                {
                    catEntry = FHDEHost.UnityContainer.BuildUp(new CategoryEntry(nodeInfoEntry.Category));
                    catEntry.Add(nodeInfoEntry);
                    FCategoryModel.Add(catEntry);
                }
            }
        }
        
        public void NodeInfoRemovedCB(INodeInfo nodeInfo)
        {
            string key = nodeInfo.Username + " [" + nodeInfo.Tags + "]";
            FNodeDict.Remove(key);
            FNodeList.Remove(key);
            
            CategoryEntry catEntry = FCategoryModel.GetCategoryEntry(nodeInfo.Category);
            //catEntry.no
            
            //FHDEHost.UnityContainer.te
            
            UpdateOutput();
        }
        #endregion INodeInfoListener

        #region TagView
        #region TextBoxTags
        void TextBoxTagsTextChanged(object sender, EventArgs e)
        {
            FTagsTextBox.Height = Math.Max(20, FTagsTextBox.Lines.Length * 13 + 7);
            //saving the last manual entry for recovery when stepping through list and reaching index -1 again
            if (FHoverLine == -1)
            {
                FManualEntry = FTagsTextBox.Text;
                FToolTip.Hide(FRichTextBox);
                
                UpdateOutput();
            }
        }

        void TextBoxTagsKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
            {
                if (!e.Shift)
                    CreateNode();
            }
            else if (e.KeyCode == Keys.Escape)
                FNodeBrowserHost.CreateNode(null);
            else if ((FTagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Down))
            {
                FHoverLine += 1;
                //if this is exceeding the FSelectionList.Count -> reset to manually entered tags
                if (FHoverLine + ScrolledLine >= FSelectionList.Count)
                    ResetToManualEntry();
                //if this is exceeding the currently visible lines -> scroll down a line
                else if (FHoverLine >= FVisibleLines)
                {
                    ScrolledLine += 1;
                    FHoverLine = FVisibleLines - 1;
                }
                
                RedrawAwesomeSelection(true);
                
                ShowToolTip();
            }
            else if ((FTagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Up))
            {
                FHoverLine -= 1;
                //if we are now < -1 -> jump to last entry
                if (FHoverLine < -1)
                {
                    FHoverLine = FVisibleLines - 1;
                    ScrolledLine = FSelectionList.Count;
                }
                //if we are now at -1 -> reset to manually entered tags
                else if ((FHoverLine == -1) && (ScrolledLine == 0))
                  ResetToManualEntry();  
                //if this is exceeding the currently visible lines -> scroll up a line
                else if ((FHoverLine == -1) && (ScrolledLine > 0))
                {
                    ScrolledLine -= 1;
                    FHoverLine = 0;
                }
                //set caret to end of tagline
               // FTagsTextBox.SelectionStart = FTagsTextBox.Text.Length;

                //FSelectedLine = FHoverLine;
                RedrawAwesomeSelection(true);
                ShowToolTip();
            }
            else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
            {
                if (FHoverLine != -1)
                {
                    FSelectedLine = -1;
                    FHoverLine = -1;
                    FTagsTextBox.SelectionStart = FTagsTextBox.Text.Length;
                    RedrawAwesomeSelection(true);
                }
            }
            else if (e.Control)
                FCtrlPressed = true;
        }
        
        void TextBoxTagsKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Control) || (e.KeyCode == Keys.ControlKey))
                FCtrlPressed = false;
        }
        
        void TextBoxTagsMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                SelectPage(NodeBrowserPage.ByCategory);
            else
            {
                FSelectedLine = -1;
                FHoverLine = -1;
                FShowHover = false;
                
                RedrawAwesomeSelection(true);
            }
        }
        
        void TextBoxTagsMouseWheel(object sender, MouseEventArgs e)
        {
            //clear old selection
            FRichTextBox.SelectionBackColor = Color.Silver;
            
            int scrollCount = 1;
            if (FCtrlPressed)
                scrollCount = 3;
            
            //adjust the line supposed to be in view
            if (e.Delta < 0)
                ScrolledLine = Math.Min(FScrollBar.Maximum - FVisibleLines + FScrollBar.LargeChange - 3, ScrolledLine + scrollCount);
            else if (e.Delta > 0)
                ScrolledLine = Math.Max(0, ScrolledLine - scrollCount);
            
            if (ScrolledLine < 0)
                return;
            
            FShowHover = true;
            RedrawAwesomeSelection(false);
        }
        
        private void ResetToManualEntry()
        {
            FTagsTextBox.Text = FManualEntry;
            FTagsTextBox.SelectionStart = FManualEntry.Length;
            FHoverLine = -1;
            ScrolledLine = 0;
            
        }
        #endregion TextBoxTags
        
        void RichTextBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (FHoverLine < 0)
                return;
            
            string username = FRichTextBox.Lines[FHoverLine].Trim();
            if (e.Button == MouseButtons.Left)
            {
                FSelectedLine = FHoverLine;
                FTagsTextBox.Text = username;
                CreateNode();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                FNodeBrowserHost.ShowNodeReference(FNodeDict[username]);
            }
            else
            {
                FNodeBrowserHost.ShowHelpPatch(FNodeDict[username]);
            }
        }
        
        void RichTextBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (FRichTextBox.Lines.Length == 0)
                return;
            
            int newHoverLine = FRichTextBox.GetLineFromCharIndex(FRichTextBox.GetCharIndexFromPosition(e.Location));
            
            //avoid some flicker
            if ((e.Location.X != FLastMouseHoverLocation.X) || (e.Location.Y != FLastMouseHoverLocation.Y))
            {
                FLastMouseHoverLocation = e.Location;
                FHoverLine = newHoverLine;
                ShowToolTip();
                RedrawAwesomeSelection(false);
            }
            
            FShowHover = true;
        }
        
        void RichTextBoxMouseUp(object sender, MouseEventArgs e)
        {
            FRichTextBox.Focus();
        }
        
        private void ShowToolTip()
        {
            string key = FRichTextBox.Lines[FHoverLine].Trim();
            if (FNodeDict.ContainsKey(key))
            {
                INodeInfo ni = FNodeDict[key];

                int y = FRichTextBox.GetPositionFromCharIndex(FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)).Y;
                string tip = "";
                if (!string.IsNullOrEmpty(ni.ShortCut))
                    tip = "(" + ni.ShortCut + ") " ;
                if (!string.IsNullOrEmpty(ni.Help))
                    tip += ni.Help;
                if (!string.IsNullOrEmpty(ni.Warnings))
                    tip += "\n WARNINGS: " + ni.Warnings;
                if (!string.IsNullOrEmpty(ni.Bugs))
                    tip += "\n BUGS: " + ni.Bugs;
                if ((!string.IsNullOrEmpty(ni.Author)) && (ni.Author != "vvvv group"))
                    tip += "\n AUTHOR: " + ni.Author;
                if (!string.IsNullOrEmpty(ni.Credits))
                    tip += "\n CREDITS: " + ni.Credits;
                if (!string.IsNullOrEmpty(tip))
                    FToolTip.Show(tip, FRichTextBox, 0, y + 30);
                else
                    FToolTip.Hide(FRichTextBox);
            }
        }
        
        private List<string> ExtractSubList(List<string> InputList)
        {
            return InputList.FindAll(delegate(string node)
                                     {
                                         node = node.ToLower();
                                         node = node.Replace('é', 'e');
                                         bool containsAll = true;
                                         string t = "";
                                         foreach (string tag in FTags)
                                         {
                                             t = tag.ToLower();
                                             if (node.Contains(t))
                                             {
                                                 if (!FAndTags)
                                                     break;
                                             }
                                             else
                                             {
                                                 containsAll = false;
                                                 break;
                                             }
                                         }
                                         
                                         if (((FAndTags) && (containsAll)) || ((!FAndTags) && (node.Contains(t))))
                                             return true;
                                         else
                                             return false;
                                     });
        }
        
        private void FilterNodesByTags()
        {
            bool sort = true;
            string text = FTagsTextBox.Text.ToLower().Trim();
            
            if (string.IsNullOrEmpty(text))
                FSelectionList = FNodeList;
            //show directory
            else if (text.IndexOf('.') == 0)
            {
                if (FPath != null)
                {
                    foreach (string p in System.IO.Directory.GetFiles(FPath))
                        FSelectionList.Add(Path.GetFileName(p));
                    
                    FSelectionList = FSelectionList.FindAll(delegate(string node)
                                                            {
                                                                node = node.ToLower();
                                                                bool containsAll = true;
                                                                string t;
                                                                foreach (string tag in FTags)
                                                                {
                                                                    t = tag.ToLower();
                                                                    t = t.Replace(".", "");
                                                                    if (!node.Contains(t))
                                                                    {
                                                                        containsAll = false;
                                                                        break;
                                                                    }
                                                                }
                                                                
                                                                if (containsAll)
                                                                    return true;
                                                                else
                                                                    return false;
                                                            });
                }
                sort = false;
            }
            //show natives only
            else if (FNodeFilter == (int) TNodeType.Native)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Native;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //show modules only
            else if (FNodeFilter == (int) TNodeType.Patch)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Patch;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //show effects only
            else if (FNodeFilter == (int) TNodeType.Effect)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Effect;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //show freeframes only
            else if (FNodeFilter == (int) TNodeType.Freeframe)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Freeframe;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //show plugins only
            else if (FNodeFilter == (int) TNodeType.Plugin)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Plugin;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //show dynamics only
            else if (FNodeFilter == (int) TNodeType.Dynamic)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Dynamic;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //show vsts only
            else if (FNodeFilter == (int) TNodeType.VST)
            {
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.VST;});
                FSelectionList = ExtractSubList(FSelectionList);
            }
            //default behavior
            else
            {
                FSelectionList = ExtractSubList(FNodeList);
            }
            if (sort)
                FSelectionList.Sort(delegate(string s1, string s2)
                                    {
                                        //create a weighting index depending on the indices the tags appear in the nodenames
                                        //earlier appearance counts more
                                        int w1 = int.MaxValue, w2 = int.MaxValue;
                                        foreach (string tag in FTags)
                                        {
                                            if (s1.ToLower().IndexOf(tag) > -1)
                                                w1 = Math.Min(w1, s1.ToLower().IndexOf(tag));
                                            if (s2.ToLower().IndexOf(tag) > -1)
                                                w2 = Math.Min(w2, s2.ToLower().IndexOf(tag));
                                        }
                                        
                                        if (w1 != w2)
                                        {
                                            if (w1 < w2)
                                                return -1;
                                            else
                                                return 1;
                                        }
                                        
                                        //if weights are equal, compare the nodenames
                                        
                                        //compare only the nodenames
                                        string name1 = s1.Substring(0, s1.IndexOf('('));
                                        string name2 = s2.Substring(0, s2.IndexOf('('));
                                        int comp = name1.CompareTo(name2);
                                        
                                        //if names are equal
                                        if (comp == 0)
                                        {
                                            //compare categories
                                            string cat1 = s1.Substring(s1.IndexOf('(')).Trim(new char[2]{'(', ')'});
                                            string cat2 = s2.Substring(s2.IndexOf('(')).Trim(new char[2]{'(', ')'});
                                            int v1, v2;
                                            
                                            
                                            //System.Diagnostics.Debug.WriteLine(cat1 + " - " + cat2);
                                            
                                            //special sorting for categories
                                            if (cat1.Contains("Value"))
                                                v1 = 99;
                                            else if (cat1.Contains("Spreads"))
                                                v1 = 98;
                                            else if (cat1.ToUpper().Contains("2D"))
                                                v1 = 97;
                                            else if (cat1.ToUpper().Contains("3D"))
                                                v1 = 96;
                                            else if (cat1.ToUpper().Contains("4D"))
                                                v1 = 95;
                                            else if (cat1.Contains("Animation"))
                                                v1 = 94;
                                            else if (cat1.Contains("EX9"))
                                                v1 = 93;
                                            else if (cat1.Contains("TTY"))
                                                v1 = 92;
                                            else if (cat1.Contains("GDI"))
                                                v1 = 91;
                                            else if (cat1.Contains("Flash"))
                                                v1 = 90;
                                            else if (cat1.Contains("String"))
                                                v1 = 89;
                                            else if (cat1.Contains("Color"))
                                                v1 = 88;
                                            else
                                                v1 = 0;
                                            
                                            if (cat2.Contains("Value"))
                                                v2 = 99;
                                            else if (cat2.Contains("Spreads"))
                                                v2 = 98;
                                            else if (cat2.ToUpper().Contains("2D"))
                                                v2 = 97;
                                            else if (cat2.ToUpper().Contains("3D"))
                                                v2 = 96;
                                            else if (cat2.ToUpper().Contains("4D"))
                                                v2 = 95;
                                            else if (cat2.Contains("Animation"))
                                                v2 = 94;
                                            else if (cat2.Contains("EX9"))
                                                v2 = 93;
                                            else if (cat2.Contains("TTY"))
                                                v2 = 92;
                                            else if (cat2.Contains("GDI"))
                                                v2 = 91;
                                            else if (cat2.Contains("Flash"))
                                                v2 = 90;
                                            else if (cat2.Contains("String"))
                                                v2 = 89;
                                            else if (cat2.Contains("Color"))
                                                v2 = 88;
                                            else
                                                v2 = 0;
                                            
                                            if (v1 > v2)
                                                return -1;
                                            else if (v1 < v2)
                                                return 1;
                                            else //categories are the same, compare versions
                                            {
                                                if ((cat1.Length > cat2.Length) && (cat1.Contains(cat2)))
                                                    return 1;
                                                else if ((cat2.Length > cat1.Length) && (cat2.Contains(cat1)))
                                                    return -1;
                                                else
                                                    return cat1.CompareTo(cat2);
                                            }
                                        }
                                        else
                                            return comp;
                                    });
            
            FNodeCountLabel.Text = "Selected nodes: " + FSelectionList.Count.ToString();
        }
        
        private void PrepareRTF()
        {
            string n;
            char[] bolded;
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            FRTFSelectionList.Clear();
            foreach (string s in FSelectionList)
            {
                //all comparison is case-in-sensitive
                n = s.ToLower();
                bolded = n.ToCharArray();
                foreach (string tag in FTags)
                {
                    string t = tag.Replace(".", "");
                    t = t.ToLower();
                    if (!string.IsNullOrEmpty(t))
                    {
                        //in the bolded char[] mark all matching characters as ° for later being rendered as bold
                        int start = 0;
                        while (n.IndexOf(t, start) >= 0)
                        {
                            int pos = n.IndexOf(t, start);
                            for (int i=pos; i<pos + t.Length; i++)
                                bolded[i] = '°';
                            start = pos+1;
                        }
                    }
                }
                
                //now recreate the string including bold markups
                sb.Remove(0, sb.Length);
                for (int i=0; i<s.Length; i++)
                    if (bolded[i] == '°')
                        sb.Append("\\b " + s[i] + "\\b0 ");
                    else
                        sb.Append(s[i]);
                
                n = sb.ToString();
                FRTFSelectionList.Add(n.PadRight(200) + "\\par ");
            }
        }
        
        private void UpdateRichTextBox()
        {
            string rtf = CRTFHeader;
            int maxLine = Math.Min(ScrolledLine + FVisibleLines, FRTFSelectionList.Count);
            
            for (int i = ScrolledLine; i < maxLine; i++)
            {
                rtf += FRTFSelectionList[i];
            }
            
            FRichTextBox.Rtf = rtf + "}";
            
            FNodeTypePanel.Invalidate();
        }
        
        private void UpdateOutput()
        {
            string text = FTagsTextBox.Text.Trim();
            FTags = text.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            
            FNodeFilter = -1;
            
            if (FTags.Length > 0)
            {
                if (FTags[0] == "N")
                    FNodeFilter = (int) TNodeType.Native;
                else if (FTags[0] == "M")
                    FNodeFilter = (int) TNodeType.Patch;
                else if (FTags[0] == "F")
                    FNodeFilter = (int) TNodeType.Freeframe;
                else if (FTags[0] == "X")
                    FNodeFilter = (int) TNodeType.Effect;
                else if (FTags[0] == "P")
                    FNodeFilter = (int) TNodeType.Plugin;
                else if (FTags[0] == "D")
                    FNodeFilter = (int) TNodeType.Dynamic;
                else if (FTags[0] == "V")
                    FNodeFilter = (int) TNodeType.VST;
            }
            
            if (FNodeFilter >= 0)
            {
                //remove first entry from FTags
                string[] restTags = new string[Math.Max(0, FTags.Length-1)];
                for (int i = 1; i < FTags.Length; i++)
                {
                    restTags[i - 1] = FTags[i];
                }
                FTags = restTags;
            }
            
            FilterNodesByTags();
            PrepareRTF();
            
            FScrollBar.Maximum = Math.Max(0, FSelectionList.Count - FVisibleLines + FScrollBar.LargeChange - 1);
            ScrolledLine = 0;
        }
        
        private void RedrawAwesomeSelection(bool updateTags)
        {
            //clear old selection
            FRichTextBox.SelectionBackColor = Color.Silver;

            //draw current selection
            string sel = FRichTextBox.Lines[FHoverLine];
            FRichTextBox.SelectionStart = FRichTextBox.Text.IndexOf(sel);
            FRichTextBox.SelectionLength = sel.Length;
            FRichTextBox.SelectionBackColor = CHoverColor;
            if (updateTags)
            {
                FTagsTextBox.Text = sel.Trim();
                FTagsTextBox.SelectionStart = FTagsTextBox.Text.Length;
            }

            //make sure the selection is also drawn in the NodeTypePanel
            FNodeTypePanel.Invalidate();
        }
        
        protected override bool ProcessDialogKey(Keys keyData)
        {
            base.ProcessDialogKey(keyData);
            if (keyData == Keys.Tab)
            {
                FAndTags = !FAndTags;
                UpdateOutput();
                return true;
            }
            else
                return false;
        }
        
        void FScrollBarValueChanged(object sender, EventArgs e)
        {
            FScrolledLine = FScrollBar.Value;
            UpdateRichTextBox();
            FToolTip.Hide(FRichTextBox);
            FShowHover = false;
        }
        
        void FNodeTypePanelPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Silver);
            
            int maxLine = Math.Min(FVisibleLines, FSelectionList.Count);
            for (int i = 0; i < maxLine; i++)
            {
                int index = i + ScrolledLine;
                if (FNodeDict.ContainsKey(FSelectionList[index].Trim()))
                {
                    int y = (i * 13) + 4;
                    TNodeType nodeType = FNodeDict[FSelectionList[index].Trim()].Type;
                    
                    if ((FHoverLine == i) && (FShowHover))
                        using (SolidBrush b = new SolidBrush(CHoverColor))
                            e.Graphics.FillRectangle(b, new Rectangle(0, y-4, 21, 13));
                    
                    switch (nodeType)
                    {
                        case TNodeType.Patch:
                            {
                                e.Graphics.DrawString("M", FRichTextBox.Font, new SolidBrush(Color.Black), 5, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Plugin:
                            {
                                e.Graphics.DrawString("P", FRichTextBox.Font, new SolidBrush(Color.Black), 6, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Dynamic:
                            {
                                e.Graphics.DrawString("dyn", FRichTextBox.Font, new SolidBrush(Color.Black), 2, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Freeframe:
                            {
                                e.Graphics.DrawString("FF", FRichTextBox.Font, new SolidBrush(Color.Black), 4, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Effect:
                            {
                                e.Graphics.DrawString("FX", FRichTextBox.Font, new SolidBrush(Color.Black), 4, y-3, StringFormat.GenericDefault);
                                break;
                            }
                    }
                }
            }
        }
        #endregion RichTextBox
        
        #region CategoryView
        void CategoryTreeViewerLeftClick(object sender, EventArgs e)
        {
            if (sender is NodeInfoEntry)
            {
                FTagsTextBox.Text = (sender as NodeInfoEntry).Username;
                CreateNode();
            }
            /* else if (e.Button == MouseButtons.Middle)
            {
                FNodeBrowserHost.ShowNodeReference(FNodeDict[username]);
            }
            else
            {
                FNodeBrowserHost.ShowHelpPatch(FNodeDict[username]);
            }
            }*/
            else if (CategoryTreeViewer.IsExpanded(sender))
                CategoryTreeViewer.Collapse(sender, false);
            else
                CategoryTreeViewer.Solo(sender);
        }
        
        void FTopLabelClick(object sender, EventArgs e)
        {
            SelectPage(NodeBrowserPage.ByTags);
        }
        #endregion CategoryView
        
        private void SelectPage(NodeBrowserPage page)
        {
            switch (page)
            {
                case NodeBrowserPage.ByCategory:
                    {
                        FToolTip.Hide(FRichTextBox);
                        FTagPanel.Hide();
                        FCategoryPanel.Dock = DockStyle.Fill;
                        FCategoryPanel.BringToFront();
                        FCategoryPanel.Show();
                        FTopLabel.Text = "-> Browse by Tags";
                        break;
                    }
                case NodeBrowserPage.ByTags:
                    {
                        FCategoryPanel.Hide();
                        FTagPanel.Dock = DockStyle.Fill;
                        FTagPanel.BringToFront();
                        FTagPanel.Show();
                        FTagsTextBox.Focus();
                        FTopLabel.Text = "-> Browse by Categories";
                        break;
                    }
            }
        }
    }
}
