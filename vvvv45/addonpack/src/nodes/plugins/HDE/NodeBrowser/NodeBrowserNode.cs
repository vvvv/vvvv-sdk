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
        CategoryModel FCategoryModel = new CategoryModel();
        AlphabetModel FAlphabetModel = new AlphabetModel();
        List<string> FAwesomeList = new List<string>();
        Dictionary<string, INodeInfo> FNodeDict = new Dictionary<string, INodeInfo>();
        private bool FAndTags = true;
        private int FSelectedLine = -1;
        private int FHoverLine = -1;
        private string FManualEntry = "";
        private int FScrolledLine = 0;
        private int FAwesomeWidth = 200;
        private bool FCtrlPressed = false;
        private int FVisibleLines = 16;
        private string FPath;
        private ToolTip FToolTip = new ToolTip();
        
        
        #endregion field declaration
        
        #region constructor/destructor
        public NodeBrowserPluginNode()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            textBoxTags.ContextMenu = new ContextMenu();
            textBoxTags.MouseWheel += new MouseEventHandler(TextBoxTagsMouseWheel);
            
            FToolTip.BackColor = Color.DarkGray;
            FToolTip.ForeColor = Color.White;
            FToolTip.ShowAlways = true;
            FToolTip.Popup += new PopupEventHandler(ToolTipPopupHandler);
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
                    
                    //the nodes author: your sign
                    FPluginInfo.Author = "vvvv group";
                    //describe the nodes function
                    FPluginInfo.Help = "The NodeInfo Browser";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "tag";
                    
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
        	this.tabControlMain = new System.Windows.Forms.TabControl();
        	this.tabAwesome = new System.Windows.Forms.TabPage();
        	this.richTextBox = new System.Windows.Forms.RichTextBox();
        	this.textBoxTags = new System.Windows.Forms.TextBox();
        	this.labelNodeCount = new System.Windows.Forms.Label();
        	this.tabCategory = new System.Windows.Forms.TabPage();
        	this.categoryTreeViewer = new VVVV.HDE.Viewer.PanelTreeViewer();
        	this.tabControlMain.SuspendLayout();
        	this.tabAwesome.SuspendLayout();
        	this.tabCategory.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// tabControlMain
        	// 
        	this.tabControlMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
        	this.tabControlMain.Controls.Add(this.tabAwesome);
        	this.tabControlMain.Controls.Add(this.tabCategory);
        	this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.tabControlMain.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.tabControlMain.ItemSize = new System.Drawing.Size(75, 15);
        	this.tabControlMain.Location = new System.Drawing.Point(0, 0);
        	this.tabControlMain.Margin = new System.Windows.Forms.Padding(0);
        	this.tabControlMain.Name = "tabControlMain";
        	this.tabControlMain.Padding = new System.Drawing.Point(4, 1);
        	this.tabControlMain.SelectedIndex = 0;
        	this.tabControlMain.Size = new System.Drawing.Size(325, 520);
        	this.tabControlMain.TabIndex = 0;
        	this.tabControlMain.SelectedIndexChanged += new System.EventHandler(this.TabControlMainSelectedIndexChanged);
        	// 
        	// tabAwesome
        	// 
        	this.tabAwesome.BackColor = System.Drawing.Color.LightGray;
        	this.tabAwesome.Controls.Add(this.richTextBox);
        	this.tabAwesome.Controls.Add(this.textBoxTags);
        	this.tabAwesome.Controls.Add(this.labelNodeCount);
        	this.tabAwesome.Location = new System.Drawing.Point(4, 19);
        	this.tabAwesome.Margin = new System.Windows.Forms.Padding(0);
        	this.tabAwesome.Name = "tabAwesome";
        	this.tabAwesome.Size = new System.Drawing.Size(317, 497);
        	this.tabAwesome.TabIndex = 2;
        	this.tabAwesome.Text = "By Tags";
        	this.tabAwesome.UseVisualStyleBackColor = true;
        	// 
        	// richTextBox
        	// 
        	this.richTextBox.BackColor = System.Drawing.Color.LightGray;
        	this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.richTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
        	this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.richTextBox.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.richTextBox.Location = new System.Drawing.Point(0, 20);
        	this.richTextBox.Name = "richTextBox";
        	this.richTextBox.ReadOnly = true;
        	this.richTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
        	this.richTextBox.Size = new System.Drawing.Size(317, 463);
        	this.richTextBox.TabIndex = 1;
        	this.richTextBox.Text = "";
        	this.richTextBox.VScroll += new System.EventHandler(this.RichTextBoxVScroll);
        	this.richTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseMove);
        	this.richTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
        	// 
        	// textBoxTags
        	// 
        	this.textBoxTags.AcceptsTab = true;
        	this.textBoxTags.BackColor = System.Drawing.Color.LightGray;
        	this.textBoxTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxTags.Dock = System.Windows.Forms.DockStyle.Top;
        	this.textBoxTags.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.textBoxTags.Location = new System.Drawing.Point(0, 0);
        	this.textBoxTags.Multiline = true;
        	this.textBoxTags.Name = "textBoxTags";
        	this.textBoxTags.Size = new System.Drawing.Size(317, 20);
        	this.textBoxTags.TabIndex = 0;
        	this.textBoxTags.TabStop = false;
        	this.textBoxTags.TextChanged += new System.EventHandler(this.TextBoxTagsTextChanged);
        	this.textBoxTags.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxTagsKeyDown);
        	this.textBoxTags.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxTagsKeyUp);
        	this.textBoxTags.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextBoxTagsMouseDown);
        	// 
        	// labelNodeCount
        	// 
        	this.labelNodeCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.labelNodeCount.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.labelNodeCount.Location = new System.Drawing.Point(0, 483);
        	this.labelNodeCount.Name = "labelNodeCount";
        	this.labelNodeCount.Size = new System.Drawing.Size(317, 14);
        	this.labelNodeCount.TabIndex = 2;
        	this.labelNodeCount.Text = "labelNodeCount";
        	// 
        	// tabCategory
        	// 
        	this.tabCategory.AutoScroll = true;
        	this.tabCategory.Controls.Add(this.categoryTreeViewer);
        	this.tabCategory.Location = new System.Drawing.Point(4, 19);
        	this.tabCategory.Name = "tabCategory";
        	this.tabCategory.Padding = new System.Windows.Forms.Padding(3);
        	this.tabCategory.Size = new System.Drawing.Size(317, 497);
        	this.tabCategory.TabIndex = 1;
        	this.tabCategory.Text = "By Category";
        	this.tabCategory.UseVisualStyleBackColor = true;
        	// 
        	// categoryTreeViewer
        	// 
        	this.categoryTreeViewer.AutoScroll = true;
        	this.categoryTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.categoryTreeViewer.Location = new System.Drawing.Point(3, 3);
        	this.categoryTreeViewer.Name = "categoryTreeViewer";
        	this.categoryTreeViewer.ShowRoot = true;
        	this.categoryTreeViewer.Size = new System.Drawing.Size(311, 491);
        	this.categoryTreeViewer.TabIndex = 1;
        	// 
        	// NodeBrowserPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.LightGray;
        	this.Controls.Add(this.tabControlMain);
        	this.DoubleBuffered = true;
        	this.Name = "NodeBrowserPluginNode";
        	this.Size = new System.Drawing.Size(325, 520);
        	this.tabControlMain.ResumeLayout(false);
        	this.tabAwesome.ResumeLayout(false);
        	this.tabAwesome.PerformLayout();
        	this.tabCategory.ResumeLayout(false);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label labelNodeCount;
        private System.Windows.Forms.TabPage tabAwesome;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.TextBox textBoxTags;
        private VVVV.HDE.Viewer.PanelTreeViewer categoryTreeViewer;
        private System.Windows.Forms.TabPage tabCategory;
        private System.Windows.Forms.TabControl tabControlMain;
        
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
            
            //create the fallback container, which contains default mappings for all
            //the interfaces the viewer model uses.
            var uc = new UnityContainer();
            uc.AddNewExtension<ViewerModelContainerExtension>();
            
            //now create a child container, which knows how to map the HDE model.
            var cc = uc.CreateChildContainer();
            cc.AddNewExtension<NodeBrowserModelProviderExtension>();
            
            //create AdapterFactoryContentProvider and hand it to the treeViewer
            var cp = new UnityContentProvider(cc);
            categoryTreeViewer.SetContentProvider(cp);
            
            //create AdapterFactoryLabelProvider and hand it to the treeViewer
            var lp = new UnityLabelProvider(cc);
            categoryTreeViewer.SetLabelProvider(lp);
            
            //hand model root over to viewers
            categoryTreeViewer.SetRoot(FCategoryModel);
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
            tabControlMain.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(text))
                FManualEntry = text.Trim();
            else
                FManualEntry = "";
            textBoxTags.Text = FManualEntry;
            textBoxTags.SelectAll();
            
            FSelectedLine = -1;
            FHoverLine = -1;
            
            RedrawAwesomeBar();
        }
        
        public void FocusAwesombar()
        {
            textBoxTags.Focus();
        }
        
        private void CreateNode()
        {
            FToolTip.Hide(richTextBox);
            
            string text = textBoxTags.Text.Trim();
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
                    FNodeBrowserHost.CreateComment(textBoxTags.Text);
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
                    tags += " " + nodeAuthor;
                string key;
                if (!string.IsNullOrEmpty(nodeInfo.Tags))
                    key = nodeInfo.Username + " [" + tags + "]";
                else
                    key = nodeInfo.Username;
                
                FAwesomeList.Add(key);
                FNodeDict[key] = nodeInfo;
                
                Size s = TextRenderer.MeasureText(key, new Font("Verdana", 7), new Size(1, 1));
                FAwesomeWidth = Math.Max(FAwesomeWidth, s.Width);
                
                //insert the nodeInfo into the data model
                FCategoryModel.Add(nodeInfo);
                FAlphabetModel.Add(nodeInfo);
            }
        }
        
        public void NodeInfoRemovedCB(INodeInfo nodeInfo)
        {
            string key = nodeInfo.Username + " [" + nodeInfo.Tags + "]";
            FNodeDict.Remove(key);
            FAwesomeList.Remove(key);
            
            FCategoryModel.Remove(nodeInfo);
            FAlphabetModel.Remove(nodeInfo);
        }
        #endregion INodeInfoListener

        #region TextBoxTags
        void TextBoxTagsTextChanged(object sender, EventArgs e)
        {
            textBoxTags.Height = Math.Max(20, textBoxTags.Lines.Length * 13 + 7);
            //saving the last manual entry for recovery when stepping through list and reaching index -1 again
            if (FSelectedLine == -1)
            {
                FManualEntry = textBoxTags.Text;
                RedrawAwesomeBar();
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
            {
                FToolTip.Hide(richTextBox);
                FNodeBrowserHost.CreateNode(null);
            }
            else if ((textBoxTags.Lines.Length == 1) && (e.KeyCode == Keys.Down))
            {
                FHoverLine += 1;
                if (FHoverLine == richTextBox.Lines.Length)
                {
                    ResetToManualEntry();
                    FHoverLine = -1;
                }
                textBoxTags.SelectionStart = textBoxTags.Text.Length;

                FSelectedLine = FHoverLine;
                RedrawAwesomeSelection(true);
            }
            else if ((textBoxTags.Lines.Length == 1) && (e.KeyCode == Keys.Up))
            {
                if (FHoverLine == -1)
                    FHoverLine = richTextBox.Lines.Length - 1;
                else
                {
                    FHoverLine -= 1;
                    if (FHoverLine == -1)
                        ResetToManualEntry();
                }
                textBoxTags.SelectionStart = textBoxTags.Text.Length;

                FSelectedLine = FHoverLine;
                RedrawAwesomeSelection(true);
            }
            else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
            {
                if (FSelectedLine != -1)
                {
                    FSelectedLine = -1;
                    FHoverLine = -1;
                    textBoxTags.SelectionStart = textBoxTags.Text.Length;
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
                tabControlMain.SelectedIndex = 1;
            else
            {
                FSelectedLine = -1;
                FHoverLine = -1;
                RedrawAwesomeSelection(true);
            }
        }
        
        void TextBoxTagsMouseWheel(object sender, MouseEventArgs e)
        {
            //clear old selection
            richTextBox.SelectionBackColor = Color.LightGray;
            
            int scrollCount = 1;
            if (FCtrlPressed)
                scrollCount = 3;
            
            //adjust the line supposed to be in view
            if (e.Delta < 0)
                FScrolledLine = Math.Min(richTextBox.Lines.Length - 1, FScrolledLine + scrollCount);
            else if (e.Delta > 0)
                FScrolledLine = Math.Max(0, FScrolledLine - scrollCount);
            
            //set the caret to the beginning of this line
            richTextBox.SelectionStart = richTextBox.GetFirstCharIndexFromLine(FScrolledLine);
            
            //scroll to the caret
            richTextBox.ScrollToCaret();
            
            //mark hoverline
            FHoverLine = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(e.Location));
        }
        
        private void ResetToManualEntry()
        {
            textBoxTags.Text = FManualEntry;
            textBoxTags.SelectionStart = FManualEntry.Length;
        }
        #endregion TextBoxTags
        
        #region RichTextBox
        void RichTextBoxMouseDown(object sender, MouseEventArgs e)
        {
            FSelectedLine = FHoverLine;
            textBoxTags.Text = richTextBox.Lines[FSelectedLine].Trim();
            CreateNode();
        }
        
        void RichTextBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (richTextBox.Lines.Length == 0)
                return;
            
            int newHoverLine = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(e.Location));
            if (FHoverLine != newHoverLine)
            {
                FHoverLine = newHoverLine;
                INodeInfo ni = FNodeDict[richTextBox.Lines[newHoverLine].Trim()];
                string tip = "";
                if (!string.IsNullOrEmpty(ni.ShortCut))
                    tip = "(" + ni.ShortCut + ")" ;
                if (!string.IsNullOrEmpty(ni.Help))
                    tip += ni.Help;
                if (!string.IsNullOrEmpty(ni.Warnings))
                    tip += "\n Warnings:\n" + ni.Warnings;
                if (!string.IsNullOrEmpty(ni.Bugs))
                    tip += "\n Bugs:\n" + ni.Bugs;
                if ((!string.IsNullOrEmpty(ni.Author)) && (ni.Author != "vvvv group"))
                    tip += "\n Author:\n" + ni.Author;
                if (!string.IsNullOrEmpty(ni.Credits))
                    tip += "\n Credits:\n" + ni.Credits;
                //FToolTip.
                if (!string.IsNullOrEmpty(tip))
                    FToolTip.Show(tip, richTextBox, 0, e.Y + 30);
                else
                    FToolTip.Hide(richTextBox);
                RedrawAwesomeSelection(false);
            }
        }
        
        private void RedrawAwesomeBar()
        {
            richTextBox.Clear();

            List<string> sub;
            string text = textBoxTags.Text.ToLower().Trim();
            string[] tags = text.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            
            bool sort = true;
            
            if (string.IsNullOrEmpty(text))
                sub = FAwesomeList;
            else if (text.IndexOf('.') == 0)
            {
                sub = new List<string>();
                
                foreach (string p in System.IO.Directory.GetFiles(FPath))
                    sub.Add(Path.GetFileName(p));
                
                sub = sub.FindAll(delegate(string node)
                                               {
                                                   node = node.ToLower();
                                                   bool containsAll = true;
                                                   string t;
                                                   foreach (string tag in tags)
                                                   {
                                                       t = tag.Replace(".", "");
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
                sort = false;
            }
            else
            {
                if (FAndTags)
                    sub = FAwesomeList.FindAll(delegate(string node)
                                               {
                                                   node = node.ToLower();
                                                   node = node.Replace('é', 'e');
                                                   bool containsAll = true;
                                                   foreach (string tag in tags)
                                                   {
                                                       if (!node.Contains(tag))
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
                else
                    sub = FAwesomeList.FindAll(delegate(string node)
                                               {
                                                   node = node.ToLower();
                                                   node = node.Replace('é', 'e');
                                                   foreach (string tag in tags)
                                                   {
                                                       if (node.Contains(tag))
                                                           return true;
                                                   }
                                                   return false;
                                               });
                
            }
            if (sort)
                sub.Sort(delegate(string s1, string s2)
                         {
                             //create a weighting index depending on the indices the tags appear in the nodenames
                             //earlier appearance counts more
                             int w1 = int.MaxValue, w2 = int.MaxValue;
                             foreach (string tag in tags)
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
                                 
                                 //special sorting for categories
                                 if (cat1.Contains("Value"))
                                     v1 = 9;
                                 else if (cat1.Contains("Spreads"))
                                     v1 = 8;
                                 else if (cat1.ToUpper().Contains("2D"))
                                     v1 = 7;
                                 else if (cat1.ToUpper().Contains("3D"))
                                     v1 = 6;
                                 else if (cat1.ToUpper().Contains("4D"))
                                     v1 = 5;
                                 else if (cat1.Contains("Animation"))
                                     v1 = 4;
                                 else if (cat1.Contains("String"))
                                     v1 = 3;
                                 else if (cat1.Contains("Color"))
                                     v1 = 2;
                                 else
                                     v1 = 0;
                                 
                                 if (cat2.Contains("Value"))
                                     v2 = 9;
                                 else if (cat2.Contains("Spreads"))
                                     v2 = 8;
                                 else if (cat2.ToUpper().Contains("2D"))
                                     v2 = 7;
                                 else if (cat2.ToUpper().Contains("3D"))
                                     v2 = 6;
                                 else if (cat2.ToUpper().Contains("4D"))
                                     v2 = 5;
                                 else if (cat2.Contains("Animation"))
                                     v2 = 4;
                                 else if (cat2.Contains("String"))
                                     v2 = 3;
                                 else if (cat2.Contains("Color"))
                                     v2 = 2;
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
            
            string n;
            char[] bolded;
            string rtf = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Verdana;}}\viewkind4\uc1\pard\f0\fs17 ";
            
            System.Text.StringBuilder sb;
            foreach (string s in sub)
            {
                //all comparison is case-in-sensitive
                n = s.ToLower();
                bolded = n.ToCharArray();
                foreach (string tag in tags)
                {
                    //
                    string t = tag.Replace(".", "");
                    //t = tag;
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
                sb = new System.Text.StringBuilder();
                for (int i=0; i<s.Length; i++)
                    if (bolded[i] == '°')
                        sb.Append("\\b " + s[i] + "\\b0 ");
                    else
                        sb.Append(s[i]);
                
                n = sb.ToString();
                rtf += n.PadRight(200) + "\\par ";
            }
            rtf += "}";

            richTextBox.Rtf = rtf;
            FScrolledLine = 0;
            
            labelNodeCount.Text = "Selected nodes: " + sub.Count.ToString();
        }
        
        private void RedrawAwesomeSelection(bool updateTags)
        {
            //clear old selection
            richTextBox.SelectionBackColor = Color.LightGray;

            if (FHoverLine > -1)
            {
                //if FHoverLine not visible
                //then scroll to the caret
                string sel;
                int topVisibleLine = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(new Point(1, 1)));
                int bottomVisibleLine = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(new Point(1, richTextBox.Height-1)));
                
                if (FHoverLine > bottomVisibleLine)
                {
                    sel = richTextBox.Lines[Math.Max(0, FHoverLine - FVisibleLines + 1)];
                    richTextBox.SelectionStart = richTextBox.Text.IndexOf(sel);
                    richTextBox.ScrollToCaret();
                }
                else if (FHoverLine < topVisibleLine)
                {
                    sel = richTextBox.Lines[FHoverLine];
                    richTextBox.SelectionStart = richTextBox.Text.IndexOf(sel);
                    richTextBox.ScrollToCaret();
                }
                
                //now select the line
                sel = richTextBox.Lines[FHoverLine];
                richTextBox.SelectionStart = richTextBox.Text.IndexOf(sel);
                richTextBox.SelectionLength = sel.Length;
                richTextBox.SelectionBackColor = Color.WhiteSmoke;
                if (updateTags)
                    textBoxTags.Text = sel.Trim();
            }
        }
        
        void RichTextBoxVScroll(object sender, EventArgs e)
        {
            FScrolledLine = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(new Point(0, 0)));
        }
        #endregion RichTextBox
        
        protected override bool ProcessDialogKey(Keys keyData)
        {
            base.ProcessDialogKey(keyData);
            if (keyData == Keys.Tab)
            {
                FAndTags = !FAndTags;
                RedrawAwesomeBar();
                return true;
            }
            else
                return false;
        }
        
        void TabControlMainSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedIndex == 0)
                textBoxTags.Focus();
        }
    }
}
