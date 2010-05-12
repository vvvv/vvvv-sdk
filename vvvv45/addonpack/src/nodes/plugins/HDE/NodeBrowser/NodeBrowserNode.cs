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
        private string FManualEntry = "";
        
        #endregion field declaration
        
        #region constructor/destructor
        public NodeBrowserPluginNode()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            tabControlMain.SelectedIndex = 2;
            textBoxTags.ContextMenu = new ContextMenu();
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
        	this.tabAlphabetical = new System.Windows.Forms.TabPage();
        	this.alphabetTreeViewer = new VVVV.HDE.Viewer.TreeViewer();
        	this.tabCategory = new System.Windows.Forms.TabPage();
        	this.categoryTreeViewer = new VVVV.HDE.Viewer.PanelTreeViewer();
        	this.tabAwesome = new System.Windows.Forms.TabPage();
        	this.richTextBox = new System.Windows.Forms.RichTextBox();
        	this.textBoxTags = new System.Windows.Forms.TextBox();
        	this.tabControlMain.SuspendLayout();
        	this.tabAlphabetical.SuspendLayout();
        	this.tabCategory.SuspendLayout();
        	this.tabAwesome.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// tabControlMain
        	// 
        	this.tabControlMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
        	this.tabControlMain.Controls.Add(this.tabAlphabetical);
        	this.tabControlMain.Controls.Add(this.tabCategory);
        	this.tabControlMain.Controls.Add(this.tabAwesome);
        	this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.tabControlMain.Location = new System.Drawing.Point(0, 0);
        	this.tabControlMain.Name = "tabControlMain";
        	this.tabControlMain.SelectedIndex = 0;
        	this.tabControlMain.Size = new System.Drawing.Size(325, 520);
        	this.tabControlMain.TabIndex = 0;
        	// 
        	// tabAlphabetical
        	// 
        	this.tabAlphabetical.AutoScroll = true;
        	this.tabAlphabetical.Controls.Add(this.alphabetTreeViewer);
        	this.tabAlphabetical.Location = new System.Drawing.Point(4, 25);
        	this.tabAlphabetical.Name = "tabAlphabetical";
        	this.tabAlphabetical.Padding = new System.Windows.Forms.Padding(3);
        	this.tabAlphabetical.Size = new System.Drawing.Size(317, 491);
        	this.tabAlphabetical.TabIndex = 0;
        	this.tabAlphabetical.Text = "Alphabetical";
        	this.tabAlphabetical.UseVisualStyleBackColor = true;
        	// 
        	// alphabetTreeViewer
        	// 
        	this.alphabetTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.alphabetTreeViewer.Location = new System.Drawing.Point(3, 3);
        	this.alphabetTreeViewer.Name = "alphabetTreeViewer";
        	this.alphabetTreeViewer.ShowRoot = false;
        	this.alphabetTreeViewer.Size = new System.Drawing.Size(311, 485);
        	this.alphabetTreeViewer.TabIndex = 0;
        	// 
        	// tabCategory
        	// 
        	this.tabCategory.AutoScroll = true;
        	this.tabCategory.Controls.Add(this.categoryTreeViewer);
        	this.tabCategory.Location = new System.Drawing.Point(4, 25);
        	this.tabCategory.Name = "tabCategory";
        	this.tabCategory.Padding = new System.Windows.Forms.Padding(3);
        	this.tabCategory.Size = new System.Drawing.Size(317, 491);
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
        	this.categoryTreeViewer.Size = new System.Drawing.Size(311, 485);
        	this.categoryTreeViewer.TabIndex = 0;
        	// 
        	// tabAwesome
        	// 
        	this.tabAwesome.Controls.Add(this.richTextBox);
        	this.tabAwesome.Controls.Add(this.textBoxTags);
        	this.tabAwesome.Location = new System.Drawing.Point(4, 25);
        	this.tabAwesome.Name = "tabAwesome";
        	this.tabAwesome.Padding = new System.Windows.Forms.Padding(3);
        	this.tabAwesome.Size = new System.Drawing.Size(317, 491);
        	this.tabAwesome.TabIndex = 2;
        	this.tabAwesome.Text = "AwesomeBar";
        	this.tabAwesome.UseVisualStyleBackColor = true;
        	// 
        	// richTextBox
        	// 
        	this.richTextBox.BackColor = System.Drawing.Color.LightGray;
        	this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.richTextBox.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.richTextBox.Location = new System.Drawing.Point(3, 24);
        	this.richTextBox.Name = "richTextBox";
        	this.richTextBox.ReadOnly = true;
        	this.richTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
        	this.richTextBox.Size = new System.Drawing.Size(311, 464);
        	this.richTextBox.TabIndex = 0;
        	this.richTextBox.Text = "";
        	this.richTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxMouseDown);
        	// 
        	// textBoxTags
        	// 
        	this.textBoxTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.textBoxTags.Dock = System.Windows.Forms.DockStyle.Top;
        	this.textBoxTags.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.textBoxTags.Location = new System.Drawing.Point(3, 3);
        	this.textBoxTags.Name = "textBoxTags";
        	this.textBoxTags.Size = new System.Drawing.Size(311, 21);
        	this.textBoxTags.TabIndex = 1;
        	this.textBoxTags.TextChanged += new System.EventHandler(this.TextBoxTagsTextChanged);
        	this.textBoxTags.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxTagsKeyDown);
        	this.textBoxTags.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextBoxTagsMouseDown);
        	// 
        	// NodeBrowserPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.tabControlMain);
        	this.Name = "NodeBrowserPluginNode";
        	this.Size = new System.Drawing.Size(325, 520);
        	this.tabControlMain.ResumeLayout(false);
        	this.tabAlphabetical.ResumeLayout(false);
        	this.tabCategory.ResumeLayout(false);
        	this.tabAwesome.ResumeLayout(false);
        	this.tabAwesome.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.TabPage tabAwesome;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.TextBox textBoxTags;
        private VVVV.HDE.Viewer.TreeViewer alphabetTreeViewer;
        private VVVV.HDE.Viewer.PanelTreeViewer categoryTreeViewer;
        private System.Windows.Forms.TabPage tabCategory;
        private System.Windows.Forms.TabPage tabAlphabetical;
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
            alphabetTreeViewer.SetContentProvider(cp);
            
            //create AdapterFactoryLabelProvider and hand it to the treeViewer
            var lp = new UnityLabelProvider(cc);
            categoryTreeViewer.SetLabelProvider(lp);
            alphabetTreeViewer.SetLabelProvider(lp);
            /*
            //create AdapterFactoryContextMenuProvider and hand it to the treeViewer
            var cmp = new UnityContextMenuProvider(cc);
            categoryTreeViewer.SetContextMenuProvider(cmp);
            alphabetTreeViewer.SetContextMenuProvider(cmp);
            
            //create AdapterFactoryDragDropProvider and hand it to the treeViewer
            var ddp = new UnityDragDropProvider(cc);
            categoryTreeViewer.SetDragDropProvider(ddp);
            alphabetTreeViewer.SetDragDropProvider(ddp);
             */
            
            
            
            
            /*    //create AdapterFactory and provider
            NodeListAdapterFactory af = new NodeListAdapterFactory();
            var cp = new AdapterFactoryContentProvider(af);
            var lp = new AdapterFactoryLabelProvider(af);
            //var ddp = new AdapterFactoryDragDropProvider(af);
            
            //hand providers over to viewers
            categoryTreeViewer.SetContentProvider(cp);
            categoryTreeViewer.SetLabelProvider(lp);
            //categoryTreeViewer.SetDragDropProvider(ddp);
            
            alphabetTreeViewer.SetContentProvider(cp);
            alphabetTreeViewer.SetLabelProvider(lp);
            //alphabetTreeViewer.SetDragDropProvider(ddp);
             */
            //hand model root over to viewers
            //categoryTreeViewer.SetRoot(FCategoryModel);
            //alphabetTreeViewer.ShowRoot = true;
            alphabetTreeViewer.SetRoot(FAlphabetModel);
        }

        public void SetNodeBrowserHost(INodeBrowserHost host)
        {
            FNodeBrowserHost = host;
        }
        
        public void Initialize(string Text)
        {
            tabControlMain.SelectedIndex = 2;
            textBoxTags.Text = Text;
            FManualEntry = Text;
            RedrawAwesomeBar();
        }
        #endregion initialization
        
        public void NodeInfoAddedCB(INodeInfo nodeInfo)
        {
            //FPluginHost.Log(TLogType.Debug, nodeInfo.Username);
            string key = nodeInfo.Username + " [" + nodeInfo.Tags + "]";
            FAwesomeList.Add(key);
            FNodeDict[key] = nodeInfo;
            
            //insert the nodeInfo into the data model
            FCategoryModel.Add(nodeInfo);
            //FPluginHost.Log(TLogType.Debug, "added to categories");
            FAlphabetModel.Add(nodeInfo);
            //FPluginHost.Log(TLogType.Debug, "added to alphabet");
            
            
            //FPluginHost.Log(TLogType.Debug, nodeInfo.Category);
            //the contentprovider will call its changed event to update the view
        }
        
        public void NodeInfoRemovedCB(INodeInfo nodeInfo)
        {
            string key = nodeInfo.Username + " [" + nodeInfo.Tags + "]";
            FNodeDict.Remove(key);
            FAwesomeList.Remove(key);
            
            FCategoryModel.Remove(nodeInfo);
            FAlphabetModel.Remove(nodeInfo);
        }

        void RedrawAwesomeBar()
        {
            richTextBox.Clear();

            List<string> sub;
            string text = textBoxTags.Text.ToLower().Trim();
            string[] tags = new string[0];
            
            if (string.IsNullOrEmpty(text))
                sub = FAwesomeList;
            else
            {
                tags = text.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);

                
                if (FAndTags)
                    sub = FAwesomeList.FindAll(delegate(string node)
                                               {
                                                   node = node.ToLower();
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
                                                   foreach (string tag in tags)
                                                   {
                                                       if (node.Contains(tag))
                                                           return true;
                                                   }
                                                   return false;
                                               });
                
            }
            sub.Sort(delegate(string s1, string s2){return s1.CompareTo(s2);});
            
            string n;
            string rtf = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Verdana;}}\viewkind4\uc1\pard\f0\fs17 ";
            
            foreach (string s in sub)
            {
                n = s;
                foreach (string tag in tags)
                    n = Regex.Replace(n, tag, "\\b $0\\b0 ", RegexOptions.IgnoreCase);

                rtf += n.PadRight(200) + "\\par ";
            }
            rtf += "}";

            richTextBox.Rtf = rtf;
        }
        
        void TextBoxTagsTextChanged(object sender, EventArgs e)
        {
            if (FSelectedLine == -1)
            {    
                FManualEntry = textBoxTags.Text;
                RedrawAwesomeBar();
            }
        }

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
        
        void TextBoxTagsKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
                CreateNode();
            else if (e.KeyCode == Keys.Escape)
                FNodeBrowserHost.CreateNode(null);
            else if (e.KeyCode == Keys.Down)
            {
                FSelectedLine = FSelectedLine + 1;
                if (FSelectedLine == richTextBox.Lines.Length)
                {
                    ResetToManualEntry();
                    FSelectedLine = -1;
                }
                textBoxTags.SelectionStart = textBoxTags.Text.Length;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (FSelectedLine == -1)
                    FSelectedLine = richTextBox.Lines.Length - 1;
                else 
                {
                    FSelectedLine -= 1;
                    if (FSelectedLine == -1)
                        ResetToManualEntry();
                }       
                textBoxTags.SelectionStart = textBoxTags.Text.Length;
            }
            else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
            {
                if (FSelectedLine != -1)
                {
                    FSelectedLine = -1;
                    textBoxTags.SelectionStart = textBoxTags.Text.Length;
                }                
            }
            
            RedrawAwesomeSelection();
        }
        
        void TextBoxTagsMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                tabControlMain.SelectedIndex = 1;
            else
            {
                FSelectedLine = -1;
                RedrawAwesomeSelection();
            }
        }
        
        private void RedrawAwesomeSelection()
        {
            if (FSelectedLine > -1)
            {
                richTextBox.SelectionBackColor = Color.LightGray;
                string sel = richTextBox.Lines[FSelectedLine];
                richTextBox.SelectionStart = richTextBox.Text.IndexOf(sel);
                richTextBox.SelectionLength = sel.Length;
                textBoxTags.Text = sel.Trim();
                richTextBox.SelectionBackColor = Color.WhiteSmoke;
            }
            else
                richTextBox.SelectionBackColor = Color.LightGray;
        }
        
        void RichTextBoxMouseDown(object sender, MouseEventArgs e)
        {
            FSelectedLine = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(e.Location));
            textBoxTags.Text = richTextBox.Lines[FSelectedLine];
            CreateNode();
        }
        
        private void CreateNode()
        {
            INodeInfo selNode = FNodeDict[textBoxTags.Text.Trim()];
            if (selNode != null)
                FNodeBrowserHost.CreateNode(selNode);
            else
                FNodeBrowserHost.CreateComment(textBoxTags.Text);
        }
        
        private void ResetToManualEntry()
        {
            textBoxTags.Text = FManualEntry;
            textBoxTags.SelectionStart = FManualEntry.Length;
        }
    }
}
