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
using VVVV.Core.Collections;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes.NodeBrowser
{
    enum NodeBrowserPage {ByCategory, ByTags};
    
    [PluginInfo(Name = "NodeBrowser",
                Category = "HDE",
                Shortcut = "Ctrl+N",
                Author = "vvvv group",
                Help = "The NodeInfo Browser",
                InitialBoxWidth = 100,
                InitialBoxHeight = 200,
                InitialWindowWidth = 300,
                InitialWindowHeight = 500,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class NodeBrowserPluginNode: UserControl, INodeBrowser, INodeInfoListener
    {
        #region field declaration
        
        //the hosts
        private IHDEHost FHDEHost;
        [Import]
        public INodeBrowserHost NodeBrowserHost {get; set;}
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        //further fields
        private bool FNeedsUpdate = true;
        private bool FAllowDragDrop = true;
        private bool FAllowUpdate = true;
        CategoryList FCategoryList = new CategoryList();
        Dictionary<string, string> FCategoryDict = new Dictionary<string, string>();
        List<string> FNodeList = new List<string>();
        List<string> FSelectionList = new List<string>();
        List<string> FRTFSelectionList = new List<string>();
        private string[] FTags = new string[0];
        Dictionary<string, INodeInfo> FNodeDict = new Dictionary<string, INodeInfo>();
        private bool FAndTags = true;
        private int FSelectedLine = -1;
        private int FHoverLine = -1;
        private Point FLastMouseHoverLocation = new Point(0, 0);
        private string FManualEntry = "";
        private bool FCtrlPressed = false;
        private int FVisibleLines = 16;
        private string FPath;
        private ToolTip FToolTip = new ToolTip();
        private bool FShowHover = false;
        private int FNodeFilter;
        
        private Color CLabelColor = Color.FromArgb(255, 154, 154, 154);
        private Color CHoverColor = Color.FromArgb(255, 216, 216, 216);
        private const string CRTFHeader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Verdana;}}\viewkind4\uc1\pard\f0\fs17 ";
        private const int CLineHeight = 13;
        
        private int FScrolledLine;
        private int ScrolledLine
        {
            get {return FScrolledLine;}
            set
            {
                FScrolledLine = Math.Max(0, Math.Min(FScrollBar.Maximum - FVisibleLines + FScrollBar.LargeChange - 3, value));
                FScrollBar.Value = FScrolledLine;
                UpdateRichTextBox();
            }
        }
        #endregion field declaration
        
        #region constructor/destructor
        //alternative constructor for standalone use
        public NodeBrowserPluginNode()
        {
            DefaultConstructor();
        }
        
        [ImportingConstructor]
        public NodeBrowserPluginNode(IHDEHost host)
        {
            DefaultConstructor();
            
            //register nodeinfolisteners at hdehost
            FHDEHost = host;
            FHDEHost.AddListener(this);
        }
        
        private void DefaultConstructor()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FTagsTextBox.ContextMenu = new ContextMenu();
            FTagsTextBox.MouseWheel += new MouseEventHandler(TextBoxTagsMouseWheel);
            
            FToolTip.BackColor = CLabelColor;
            FToolTip.ForeColor = Color.White;
            FToolTip.ShowAlways = false;
            FToolTip.Popup += new PopupEventHandler(ToolTipPopupHandler);
            
            SelectPage(NodeBrowserPage.ByTags);
            
            FCategoryDict.Add("2d", "Geometry in 2d, like connecting lines, calculating coordinates etc.");
            FCategoryDict.Add("3d", "Geometry in 3d.");
            FCategoryDict.Add("4d", "");
            FCategoryDict.Add("Animation", "Things which will animate over time and therefore have an internal state; Generate motion, smooth and filter motion, record and store values. FlipFlops and other Logic nodes.");
            FCategoryDict.Add("Astronomy", "Everything having to do with the Earth and the Universe; Current Time, calculation of earth, moon and sun’s parameters.");
            FCategoryDict.Add("Boolean", "Logic Operators.");
            FCategoryDict.Add("Color", "Working with color, color addition, subtraction, blending, color models etc.");
            FCategoryDict.Add("Debug", "Displaying system status information in various undocumented formats.");
            FCategoryDict.Add("Devices", "Control external devices, and get data from them.");
            FCategoryDict.Add("Differential", "Create ultra smooth motions by working with position and velocity at the same time.");
            FCategoryDict.Add("DShow9", "Audio and Video playback and effects based on Microsofts DirectShow Framework.");
            FCategoryDict.Add("DX9", "DirectX9 based rendering system");
            FCategoryDict.Add("Enumerations", "Work with enumerated data types");
            FCategoryDict.Add("EX9", "The DirectX9 based rendering system made more Explicit. So geometry generation is separated from geometry display in the shader.");
            FCategoryDict.Add("File", "Operations on the file system. Read, write, copy, delete, parse files etc.");
            FCategoryDict.Add("Flash", "Everything related to rendering Flash content.");
            FCategoryDict.Add("GDI", "Old school simple rendering system. Simple nodes for didactical use and lowtek graphics.");
            FCategoryDict.Add("HTML", "Nodes making use of HTML strings local or on the internet");
            FCategoryDict.Add("Network", "Internet functionality like HTTP, IRC, UDP, TCP, ...");
            FCategoryDict.Add("Node", "Operations on the generic so called node pins.");
            FCategoryDict.Add("ODE", "The Open Dynamics Engine for physical behaviour.");
            FCategoryDict.Add("Quaternion", "Work with Quaternion vectors for rotations.");
            FCategoryDict.Add("Spectral", "Operations for reducing value spreads to some few values. Summing, Averaging etc.");
            FCategoryDict.Add("Spreads", "Operations creating value spreads out of few key values. Also spread operations.");
            FCategoryDict.Add("String", "String functions, appending, searching, sorting, string spread and spectral operations.");
            FCategoryDict.Add("System", "Control of built in hardware, like mouse, keyboard, sound card mixer, power management etc.");
            FCategoryDict.Add("Transforms", "Nodes for creating and manipulating 3d-transformations.");
            FCategoryDict.Add("TTY", "Old school tty console rendering system for printing out status and debug messages.");
            FCategoryDict.Add("Value", "Everything dealing with numercial values: Mathematical operations, ...");
            FCategoryDict.Add("VVVV", "Everything directly related to the running vvvv instance: Command line parameters, Event outputs, Quit command, ...");
            FCategoryDict.Add("Windows", "Control Windows´ Windows, Desktop Icons etc.");
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
            mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            
            FCategoryTreeViewer.Registry = mappingRegistry;
            FCategoryTreeViewer.Input = FCategoryList;
            
            UpdateOutput();
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
        
        private void InitializeComponent()
        {
        	this.FTagPanel = new VVVV.Nodes.DoubleBufferedPanel();
        	this.FRichTextBox = new System.Windows.Forms.RichTextBox();
        	this.FNodeTypePanel = new VVVV.Nodes.DoubleBufferedPanel();
        	this.FNodeCountLabel = new System.Windows.Forms.Label();
        	this.FScrollBar = new System.Windows.Forms.VScrollBar();
        	this.FTagsTextBox = new System.Windows.Forms.TextBox();
        	this.FCategoryPanel = new System.Windows.Forms.Panel();
        	this.FCategoryTreeViewer = new VVVV.HDE.Viewer.WinFormsViewer.TreeViewer();
        	this.FTopLabel = new System.Windows.Forms.Label();
        	this.FTagPanel.SuspendLayout();
        	this.FCategoryPanel.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// FTagPanel
        	// 
        	this.FTagPanel.Controls.Add(this.FRichTextBox);
        	this.FTagPanel.Controls.Add(this.FNodeTypePanel);
        	this.FTagPanel.Controls.Add(this.FNodeCountLabel);
        	this.FTagPanel.Controls.Add(this.FScrollBar);
        	this.FTagPanel.Controls.Add(this.FTagsTextBox);
        	this.FTagPanel.Location = new System.Drawing.Point(3, 33);
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
        	this.FRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
        	this.FRichTextBox.Size = new System.Drawing.Size(107, 405);
        	this.FRichTextBox.TabIndex = 2;
        	this.FRichTextBox.TabStop = false;
        	this.FRichTextBox.Text = "";
        	this.FRichTextBox.WordWrap = false;
        	this.FRichTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseUp);
        	this.FRichTextBox.Resize += new System.EventHandler(this.FRichTextBoxResize);
        	this.FRichTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
        	this.FRichTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
        	// 
        	// FNodeTypePanel
        	// 
        	this.FNodeTypePanel.Dock = System.Windows.Forms.DockStyle.Left;
        	this.FNodeTypePanel.Location = new System.Drawing.Point(0, 20);
        	this.FNodeTypePanel.Name = "FNodeTypePanel";
        	this.FNodeTypePanel.Size = new System.Drawing.Size(20, 405);
        	this.FNodeTypePanel.TabIndex = 4;
        	this.FNodeTypePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.FNodeTypePanelPaint);
        	this.FNodeTypePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
        	this.FNodeTypePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
        	this.FNodeTypePanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseUp);
        	// 
        	// FNodeCountLabel
        	// 
        	this.FNodeCountLabel.BackColor = System.Drawing.Color.Silver;
        	this.FNodeCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FNodeCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.FNodeCountLabel.Location = new System.Drawing.Point(0, 425);
        	this.FNodeCountLabel.Name = "FNodeCountLabel";
        	this.FNodeCountLabel.Size = new System.Drawing.Size(127, 15);
        	this.FNodeCountLabel.TabIndex = 5;
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
        	this.FTagsTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FTagsTextBoxMouseUp);
        	// 
        	// FCategoryPanel
        	// 
        	this.FCategoryPanel.Controls.Add(this.FCategoryTreeViewer);
        	this.FCategoryPanel.Controls.Add(this.FTopLabel);
        	this.FCategoryPanel.Location = new System.Drawing.Point(165, 33);
        	this.FCategoryPanel.Name = "FCategoryPanel";
        	this.FCategoryPanel.Size = new System.Drawing.Size(159, 439);
        	this.FCategoryPanel.TabIndex = 5;
        	// 
        	// FCategoryTreeViewer
        	// 
        	this.FCategoryTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FCategoryTreeViewer.FlatStyle = true;
        	this.FCategoryTreeViewer.Location = new System.Drawing.Point(0, 15);
        	this.FCategoryTreeViewer.Name = "FCategoryTreeViewer";
        	this.FCategoryTreeViewer.ShowLines = false;
        	this.FCategoryTreeViewer.ShowPlusMinus = false;
        	this.FCategoryTreeViewer.ShowRoot = false;
        	this.FCategoryTreeViewer.ShowRootLines = false;
        	this.FCategoryTreeViewer.ShowTooltip = true;
        	this.FCategoryTreeViewer.Size = new System.Drawing.Size(159, 424);
        	this.FCategoryTreeViewer.TabIndex = 1;
        	this.FCategoryTreeViewer.Click += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.NodeBrowserPluginNode_Click);
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
        	this.DoubleBuffered = true;
        	this.Name = "NodeBrowserPluginNode";
        	this.Size = new System.Drawing.Size(325, 520);
        	this.FTagPanel.ResumeLayout(false);
        	this.FTagPanel.PerformLayout();
        	this.FCategoryPanel.ResumeLayout(false);
        	this.ResumeLayout(false);
        }

        
        private VVVV.HDE.Viewer.WinFormsViewer.TreeViewer FCategoryTreeViewer;
        private VVVV.Nodes.DoubleBufferedPanel FNodeTypePanel;
        private System.Windows.Forms.VScrollBar FScrollBar;
        private System.Windows.Forms.Label FNodeCountLabel;
        private System.Windows.Forms.Label FTopLabel;
        private System.Windows.Forms.Panel FCategoryPanel;
        private System.Windows.Forms.TextBox FTagsTextBox;
        private RichTextBox FRichTextBox;
        private VVVV.Nodes.DoubleBufferedPanel FTagPanel;
        
        #endregion constructor/destructor
        
        #region INodeBrowser
        public void Initialize(string path, string text)
        {
            FNeedsUpdate |= FPath != path;
            
            FPath = path;
            
            if (!string.IsNullOrEmpty(text))
                FManualEntry = text.Trim();
            else
                FManualEntry = "";
            FTagsTextBox.Text = FManualEntry;
            FTagsTextBox.SelectAll();
            
            FSelectedLine = -1;
            FHoverLine = -1;
            
            //init view
            if (FNeedsUpdate)
            {
                UpdateOutput();
                FCategoryTreeViewer.Reload();
                FNeedsUpdate = false;
            }
        }
        
        public void AfterShow()
        {
            FTagsTextBox.Focus();
        }
        
        public void BeforeHide()
        {
            FToolTip.Hide(FRichTextBox);
        }
        
        public void DragDrop(bool allow)
        {
            FAllowDragDrop = allow;
        }
        
        private void CreateNode()
        {
            string text = FTagsTextBox.Text.Trim();
            try
            {
                INodeInfo selNode = FNodeDict[text];
                NodeBrowserHost.CreateNode(selNode);
            }
            catch
            {
                if ((text.Contains(".v4p")) || (text.Contains(".fx")) || (text.Contains(".dll")))
                    NodeBrowserHost.CreateNodeFromFile(FPath + text);
                else
                    NodeBrowserHost.CreateComment(FTagsTextBox.Text);
            }
        }
        #endregion INodeBrowser
        
        #region INodeInfoListener
        private string NodeInfoToKey(INodeInfo nodeInfo)
        {
            string tags = nodeInfo.Tags;
            if ((!string.IsNullOrEmpty(nodeInfo.Author)) && (nodeInfo.Author != "vvvv group"))
                tags += ", " + nodeInfo.Author;

            if (!string.IsNullOrEmpty(nodeInfo.Tags))
                return nodeInfo.Username + " [" + tags + "]";
            else
                return nodeInfo.Username;
        }
        
        public void NodeInfoAddedCB(INodeInfo nodeInfo)
        {
            string nodeVersion = nodeInfo.Version;

            //don't include legacy nodes in the list
            if ((string.IsNullOrEmpty(nodeVersion)) || (!nodeVersion.ToLower().Contains("legacy")))
            {
                string key = NodeInfoToKey(nodeInfo);
                
                FNodeList.Add(key);
                FNodeDict[key] = nodeInfo;
                
                //insert nodeInfo to FCategoryList
                bool added = false;
                foreach (CategoryEntry ce in FCategoryList)
                    if (ce.Name == nodeInfo.Category)
                {
                    ce.Add(nodeInfo);
                    added = true;
                    break;
                }
                
                //category not yet present. create a new one
                if (!added)
                {
                    string description;
                    if (FCategoryDict.ContainsKey(nodeInfo.Category))
                        description = FCategoryDict[nodeInfo.Category];
                    else
                        description = "";
                    
                    var catEntry = new CategoryEntry(nodeInfo.Category, description);
                    catEntry.Add(nodeInfo);
                    FCategoryList.Add(catEntry);
                }
                
                FNeedsUpdate = true;
            }
        }
        
        public void NodeInfoUpdatedCB(INodeInfo nodeInfo)
        {
            string oldkey = "";
            string newkey = NodeInfoToKey(nodeInfo);
            //find the old key that is associated with this nodeinfo
            foreach(var infokey in FNodeDict)
                if (infokey.Value == nodeInfo)
            {
                oldkey = infokey.Key;
                break;
            }
            
            //re-add the same nodeinfo with the new key
            var ni = FNodeDict[oldkey];
            FNodeDict.Remove(oldkey);
            FNodeDict.Add(newkey, ni);
            
            FNodeList.Remove(oldkey);
            FNodeList.Add(newkey);
            
            FNeedsUpdate = true;
        }
        
        public void NodeInfoRemovedCB(INodeInfo nodeInfo)
        {
            string key = NodeInfoToKey(nodeInfo);
            FNodeDict.Remove(key);
            FNodeList.Remove(key);

            CategoryEntry catEntry = null;
            foreach (CategoryEntry ce in FCategoryList)
                if (ce.Name == nodeInfo.Category)
            {
                ce.Remove(nodeInfo);
                catEntry = ce;
                break;
            }
            
            if ((catEntry != null) && (catEntry.Count == 0))
                FCategoryList.Remove(catEntry);
            
            FNeedsUpdate = true;
        }
        #endregion INodeInfoListener

        #region TagView
        #region TextBoxTags
        void TextBoxTagsTextChanged(object sender, EventArgs e)
        {
            FTagsTextBox.Height = Math.Max(20, FTagsTextBox.Lines.Length * CLineHeight + 7);
            
            FHoverLine = -1;
            FShowHover = false;
            
            if (FAllowUpdate)
            {
                //saving the last manual entry for recovery when stepping through list and reaching index -1 again
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
                NodeBrowserHost.CreateNode(null);
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
                
                FShowHover = true;
                RedrawAwesomeSelection(true);
                ShowToolTip();
            }
            else if ((FTagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Up))
            {
                FHoverLine -= 1;
                //if we are now < -1 -> jump to last entry
                if (FHoverLine < -1)
                {
                    FHoverLine = Math.Min(FSelectionList.Count, FVisibleLines) - 1;
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
                
                FShowHover = true;
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
            FSelectedLine = -1;
            FHoverLine = -1;
            FShowHover = false;
            
            RedrawAwesomeSelection(true);
        }
        
        void FTagsTextBoxMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SelectPage(NodeBrowserPage.ByCategory);
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
            FRichTextBox.SelectionStart = FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)+1;
            FTagsTextBox.Focus();
            
            //as plugin in its own window
            if (FAllowDragDrop)
            {
                string systemname = FNodeDict[username].Systemname;
                FTagsTextBox.DoDragDrop(systemname, DragDropEffects.All);
                return;
            }
            
            //popped up on doubleclick
            if (e.Button == MouseButtons.Left)
            {
                FSelectedLine = FHoverLine;
                FTagsTextBox.Text = username;
                CreateNode();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                NodeBrowserHost.ShowNodeReference(FNodeDict[username]);
            }
            else
            {
                NodeBrowserHost.ShowHelpPatch(FNodeDict[username]);
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
            //hack: called only to re-focus active patch
            //after this mouseup set the focus to the already hidden NodeBrowser window
            NodeBrowserHost.CreateComment("");
            
            FTagsTextBox.Focus();
        }
        
        private void ShowToolTip()
        {
            if (FHoverLine == -1)
                return;
            
            string key = FRichTextBox.Lines[FHoverLine].Trim();
            if (FNodeDict.ContainsKey(key))
            {
                INodeInfo ni = FNodeDict[key];

                int y = FRichTextBox.GetPositionFromCharIndex(FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)).Y;
                string tip = "";
                if (!string.IsNullOrEmpty(ni.Shortcut))
                    tip = "(" + ni.Shortcut + ") " ;
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
        
        private List<string> GetLocalNodes()
        {
            var files = new List<string>();
            if (FPath != null)
            {
                foreach (string p in System.IO.Directory.GetFiles(FPath, "*.dll"))
                    files.Add(Path.GetFileName(p));
                foreach (string p in System.IO.Directory.GetFiles(FPath, "*.v4p"))
                    files.Add(Path.GetFileName(p));
                foreach (string p in System.IO.Directory.GetFiles(FPath, "*.fx"))
                    files.Add(Path.GetFileName(p));
            }
            return files;
        }
        
        private void FilterNodesByTags()
        {
            string text = FTagsTextBox.Text.ToLower().Trim();
            
            FSelectionList.Clear();
            FSelectionList.AddRange(FNodeList);
            
            //add local nodes
            FSelectionList.AddRange(GetLocalNodes());

            //show natives only
            if (FNodeFilter == (int) TNodeType.Native)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Native;});
            //show patches only
            else if (FNodeFilter == (int) TNodeType.Patch)
                FSelectionList = GetLocalNodes();
            //show modules only
            else if (FNodeFilter == (int) TNodeType.Module)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Module;});
            //show effects only
            else if (FNodeFilter == (int) TNodeType.Effect)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Effect;});
            //show freeframes only
            else if (FNodeFilter == (int) TNodeType.Freeframe)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Freeframe;});
            //show plugins only
            else if (FNodeFilter == (int) TNodeType.Plugin)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Plugin;});
            //show dynamics only
            else if (FNodeFilter == (int) TNodeType.Dynamic)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.Dynamic;});
            //show vsts only
            else if (FNodeFilter == (int) TNodeType.VST)
                FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == TNodeType.VST;});

            FSelectionList = ExtractSubList(FSelectionList);
            
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
                                    
                                    //names may be filenames, they always win
                                    bool s1IsFile = false;
                                    bool s2IsFile = false;
                                    
                                    if ((s1.IndexOf(".v4p") > 0) || (s1.IndexOf(".dll") > 0) || (s1.IndexOf(".fx") > 0))
                                        s1IsFile = true;
                                    if ((s2.IndexOf(".v4p") > 0) || (s2.IndexOf(".dll") > 0) || (s2.IndexOf(".fx") > 0))
                                        s2IsFile = true;
                                    
                                    string name1 = s1;
                                    string name2 = s2;
                                    
                                    if (!s1IsFile)
                                        name1 = s1.Substring(0, s1.IndexOf('('));
                                    if (!s2IsFile)
                                        name2 = s2.Substring(0, s2.IndexOf('('));
                                    
                                    //compare only the nodenames
                                    int comp = name1.CompareTo(name2);
                                    
                                    //if names are equal
                                    if (comp == 0)
                                    {
                                        //compare categories
                                        string cat1, cat2;
                                        if (s1IsFile)
                                            cat1 = Path.GetExtension(name1);
                                        else
                                            cat1 = s1.Substring(s1.IndexOf('(')).Trim(new char[2]{'(', ')'});
                                        
                                        if (s2IsFile)
                                            cat2 = Path.GetExtension(name2);
                                        else
                                            cat2 = s2.Substring(s2.IndexOf('(')).Trim(new char[2]{'(', ')'});
                                        
                                        int v1, v2;
                                        
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
            
            FNodeCountLabel.Text = "Matching Nodes: " + FSelectionList.Count.ToString();
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
                else if ((FTags[0] == ".") || (FTags[0] == "V") || (FTags[0] == "V4") || (FTags[0] == "V4P"))
                    FNodeFilter = (int) TNodeType.Patch;
                else if (FTags[0] == "M")
                    FNodeFilter = (int) TNodeType.Module;
                else if ((FTags[0] == "F") || (FTags[0] == "FF"))
                    FNodeFilter = (int) TNodeType.Freeframe;
                else if ((FTags[0] == "X") || (FTags[0] == "FX"))
                    FNodeFilter = (int) TNodeType.Effect;
                else if (FTags[0] == "P")
                    FNodeFilter = (int) TNodeType.Plugin;
                else if (FTags[0] == "D")
                    FNodeFilter = (int) TNodeType.Dynamic;
                else if ((FTags[0] == "VS") || (FTags[0] == "VST"))
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

            if (FHoverLine > -1)
            {
                //draw current selection
                string sel = FRichTextBox.Lines[FHoverLine];
                FRichTextBox.SelectionStart = FRichTextBox.Text.IndexOf(sel);
                FRichTextBox.SelectionLength = sel.Length;
                FRichTextBox.SelectionBackColor = CHoverColor;
                if (updateTags)
                {
                    FAllowUpdate = false;
                    FTagsTextBox.Text = sel.Trim();
                    FAllowUpdate = true;
                    FTagsTextBox.SelectionStart = FTagsTextBox.Text.Length;
                }
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
                int y = (i * CLineHeight) + 4;
                
                if ((FHoverLine == i) && (FShowHover))
                        using (SolidBrush b = new SolidBrush(CHoverColor))
                            e.Graphics.FillRectangle(b, new Rectangle(0, y-4, 21, CLineHeight));
                
                if (FNodeDict.ContainsKey(FSelectionList[index].Trim()))
                {
                    TNodeType nodeType = FNodeDict[FSelectionList[index].Trim()].Type;
                    
                    using (SolidBrush b = new SolidBrush(Color.Black))
                        switch (nodeType)
                    {
                        case TNodeType.Module:
                            {
                                e.Graphics.DrawString("M", FRichTextBox.Font, b, 5, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Plugin:
                            {
                                e.Graphics.DrawString("P", FRichTextBox.Font, b, 6, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Dynamic:
                            {
                                e.Graphics.DrawString("D", FRichTextBox.Font, b, 6, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Freeframe:
                            {
                                e.Graphics.DrawString("FF", FRichTextBox.Font, b, 4, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.Effect:
                            {
                                e.Graphics.DrawString("FX", FRichTextBox.Font, b, 4, y-3, StringFormat.GenericDefault);
                                break;
                            }
                        case TNodeType.VST:
                            {
                                e.Graphics.DrawString("VST", FRichTextBox.Font, b, 4, y-3, StringFormat.GenericDefault);
                                break;
                            }
                    }
                }
                else
                    using (SolidBrush b = new SolidBrush(Color.Black))
                        e.Graphics.DrawString("V", FRichTextBox.Font, b, 5, y-3, StringFormat.GenericDefault);
            }
        }
        
        void FRichTextBoxResize(object sender, EventArgs e)
        {
            FVisibleLines = FRichTextBox.Height / CLineHeight;
            UpdateOutput();
        }
        #endregion RichTextBox
        
        #region CategoryView
        void NodeBrowserPluginNode_Click(IModelMapper sender, MouseEventArgs e)
        {
            if (sender.Model is NodeInfoEntry)
            {
                if (e.Button == MouseButtons.Left)
                {
                    FTagsTextBox.Text = NodeInfoToKey((sender.Model as NodeInfoEntry).NodeInfo);
                    CreateNode();
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    NodeBrowserHost.ShowNodeReference(FNodeDict[(sender.Model as NodeInfoEntry).Name]);
                }
                else
                {
                    NodeBrowserHost.ShowHelpPatch(FNodeDict[(sender.Model as NodeInfoEntry).Name]);
                }
            }
            else
            {
                if (FCategoryTreeViewer.IsExpanded(sender.Model))
                    FCategoryTreeViewer.Collapse(sender.Model, false);
                else if (e.Button == MouseButtons.Left)
                    FCategoryTreeViewer.Solo(sender.Model);
                else if (e.Button == MouseButtons.Right)
                    FCategoryTreeViewer.Expand(sender.Model, false);
            }
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
                        FCategoryTreeViewer.Focus();
                        FTopLabel.Text = "-> Browse by Tags";
                        break;
                    }
                case NodeBrowserPage.ByTags:
                    {
                        FCategoryPanel.Hide();
                        FCategoryTreeViewer.HideToolTip();
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
