using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.NodeBrowser
{
    public partial class TagPanel : UserControl
    {
        public event PanelChangeHandler OnPanelChange;
        public event CreateNodeHandler OnCreateNode;
        public event CreateNodeHandler OnShowNodeReference;
        public event CreateNodeHandler OnShowHelpPatch;
        public event CreateNodeFromStringHandler OnCreateNodeFromString;
        
        private int FVisibleLines = 20;
        private Color CLabelColor = Color.FromArgb(255, 154, 154, 154);
        private Color CHoverColor = Color.FromArgb(255, 216, 216, 216);
        private const string CRTFHeader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Verdana;}\deftab300}\viewkind4\uc1\pard\f0\fs17 ";
        private int CLineHeight = 13; //dip-ified in constructor!
        private int CLineLength = 200; //dip-ified in constructor!
        private int FHoverLine;
        private List<string> FTags;
        private Point FLastMouseHoverLocation = new Point(0, 0);
        private int FNodeFilter;
        private bool FShowInternal;

        private List<INodeInfo> FSelectionList = new List<INodeInfo>();
        private List<string> FRTFSelectionList = new List<string>();
        private readonly Regex FVVVVGroupRegex = new Regex(@"vvvv\s+group", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        private List<string> FCategoryPriorities = new List<string>(new string[] { "DSHOW9", "OCTONION", "QUATERNION", "FLASH", "GDI", "TTY", "SVG", "TRANSFORM", "COLOR", "DX9", "EX9.GEOMETRY", "EX9.TEXTURE", "EX9", "DX11.LAYER", "DX11.GEOMETRY", "EX9.TEXTUREFX", "EX9.TEXTURE", "DX11", "RAW", "STRING", "FILE", "ANIMATION", "4D", "3D", "2D", "SPREADS", "VALUE" });
        
        private NodeBrowserPluginNode FNodeBrowser;
        public NodeBrowserPluginNode NodeBrowser
        {
            get
            {
                return FNodeBrowser;
            }
            set
            {
                if (FNodeBrowser != null)
                {
                    this.FRichTextBox.Resize -= this.HandleRichTextBoxResize;
                }
                
                FNodeBrowser = value;
                
                if (FNodeBrowser != null && FNodeBrowser.IsStandalone)
                {
                    this.FRichTextBox.Resize += this.HandleRichTextBoxResize;
                }
            }
        }
        
        public bool AndTags
        {
            get;
            set;
        }
        
        public bool AllowDragDrop
        {
            get;
            set;
        }
        
        public TextBox TagsTextBox
        {
            get;
            set;
        }
        
        internal bool PendingRedraw
        {
            get;
            set;
        }
        
        private int FScrolledLine;
        private int ScrolledLine
        {
            get
            {
                return FScrolledLine;
            }
            set
            {
                FScrolledLine = Math.Max(0, Math.Min(FSelectionList.Count - FVisibleLines, value));
                FScrollBar.Value = FScrolledLine;
                UpdateRichTextBox();
            }
        }
        
        public string CommentText
        {
            get
            {
                return TagsTextBox.Text.Trim();
            }
        }
        
        public int TextBoxHeight
        {
            get
            {
            	return  Math.Max(1, TagsTextBox.Lines.Length) * CLineHeight + DIPY(7);
            }
        }
        
        private float FDPIX, FDPIY;
        private int DIPX(int pixel)
        {
        	return (int) Math.Round(pixel * FDPIX);
        }
        
        private int DIPY(int pixel)
        {
        	return (int) Math.Round(pixel * FDPIY);
        }
        
        public TagPanel()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            FToolTip.BackColor = CLabelColor;
            FToolTip.ForeColor = Color.White;
            FToolTip.ShowAlways = false;
            FToolTip.Popup += ToolTipPopupHandler;
            
            FRichTextBox.MouseWheel += DoMouseWheel;
            
            using (var g = this.CreateGraphics())
            {
            	FDPIX = g.DpiX / 96.0f;
				FDPIY = g.DpiY / 96.0f;           	
            }
            
            CLineHeight = DIPY(13);
            CLineLength = DIPX(200);
        }
        
        private void ToolTipPopupHandler(object sender, PopupEventArgs e)
        {
        	e.ToolTipSize = new Size(Math.Min(e.ToolTipSize.Width, DIPX(300)), e.ToolTipSize.Height);
        }
        
        private string NodeInfoToDisplayName(INodeInfo nodeInfo)
        {
            string tags = nodeInfo.Tags;
            if ((!string.IsNullOrEmpty(nodeInfo.Author)) && (!FVVVVGroupRegex.IsMatch(nodeInfo.Author)))
                if (string.IsNullOrEmpty(tags))
                    tags = nodeInfo.Author;
                else
                    tags += ", " + nodeInfo.Author;

            if (!string.IsNullOrEmpty(tags))
                return nodeInfo.Username + " [" + tags + "]";
            else
                return nodeInfo.Username;
        }
        
        public void Initialize(string text)
        {
            if (string.IsNullOrEmpty(text))
                TagsTextBox.Text = "";
            else
                TagsTextBox.Text = text.Trim();

            TagsTextBox.SelectAll();
            
            FHoverLine = -1;
            ScrolledLine = 0;
            
            //            if (NeedsUpdate)
            //                Redraw();
            RedrawSelection();
        }
        
        public void AfterShow()
        {
            this.FRichTextBox.Resize += this.HandleRichTextBoxResize;
            TagsTextBox.Focus();
        }
        
        public void BeforeHide()
        {
        	//reset text to "" before removing resizeHandler in order to get FVisible lines computed correctly
            TagsTextBox.Text = "";
        	this.FRichTextBox.Resize -= this.HandleRichTextBoxResize;
            FToolTip.Hide(FRichTextBox);
        }
        
        void CreateNodeFromHoverLine()
        {
            try
            {
                var selNode = FSelectionList[FHoverLine + ScrolledLine];
                
                //now that we create a text make sure not to create a comment as well
                TagsTextBox.Text = "";
                
                if ((Control.ModifierKeys == Keys.Control) && (selNode.Type == NodeType.Dynamic || selNode.Type == NodeType.Effect || selNode.Type == NodeType.VL || selNode.Type == NodeType.Module))
                    OnPanelChange(NodeBrowserPage.Clone, selNode);
                else
                    OnCreateNode(selNode);
            }
            catch
            {
                OnCreateNodeFromString(TagsTextBox.Text.Trim());
            }
        }
        
        #region TagsTextBox
        public void DoTextChanged()
        {
            //saving the last manual entry for recovery when stepping through list and reaching index -1 again
            FToolTip.Hide(FRichTextBox);
            
            Redraw();
            
            if (FRichTextBox.Lines.Length > 0)
                FHoverLine = 0;
            else
                FHoverLine = -1;
            
            RedrawSelection();
        }

        public void DoKeyDown(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
            {
                if (!e.Shift)
                    CreateNodeFromHoverLine();
            }
            else if (e.KeyCode == Keys.Escape)
                OnCreateNode(null);
            else if ((TagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Down))
            {
                FHoverLine += 1;
                //if this is exceeding the FSelectionList.Count -> jump to line 0
                if (FHoverLine + ScrolledLine >= FSelectionList.Count)
                {
                    FHoverLine = 0;
                    ScrolledLine = 0;
                }
                //if this is exceeding the currently visible lines -> scroll down a line
                else if (FHoverLine >= FVisibleLines)
                {
                    ScrolledLine += 1;
                    FHoverLine = FVisibleLines - 1;
                }
                
                RedrawSelection();
                ShowToolTip(0);
            }
            else if ((TagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Up))
            {
                FHoverLine -= 1;
                //if this is exceeding the currently visible lines -> scroll up a line
                if ((FHoverLine == -1) && (ScrolledLine > 0))
                {
                    ScrolledLine -= 1;
                    FHoverLine = 0;
                }
                //if we are now < 0 -> jump to last entry
                else if (FHoverLine < 0)
                {
                    FHoverLine = Math.Min(FSelectionList.Count, FVisibleLines) - 1;
                    ScrolledLine = FSelectionList.Count;
                }
                
                RedrawSelection();
                ShowToolTip(0);
            }
            else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
            {
                if (FHoverLine != -1)
                {
                    FHoverLine = -1;
                    TagsTextBox.SelectionStart = TagsTextBox.Text.Length;
                    RedrawSelection();
                }
            }
            else if ((e.Control) && (e.KeyCode == Keys.A))
            {
                TagsTextBox.SelectAll();
            }
        }
        
        public void DoMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //clear old selection
            FRichTextBox.SelectionBackColor = Color.Silver;
            
            int scrollCount = 1;
            if (Control.ModifierKeys == Keys.Control)
                scrollCount = 3;
            
            //adjust the line supposed to be in view
            if (e.Delta < 0)
                ScrolledLine += scrollCount;
            else if (e.Delta > 0)
                ScrolledLine -= scrollCount;
            
            if (ScrolledLine < 0)
                return;
            
            RedrawSelection();
        }
        #endregion TextBoxTags
        
        #region RichTextBox
        void RichTextBoxMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (FHoverLine < 0 || FHoverLine >= FRichTextBox.Lines.Length) return;
            
            string username = FRichTextBox.Lines[FHoverLine].Trim();
            FRichTextBox.SelectionStart = FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)+1;
            TagsTextBox.Focus();
            
            //as plugin in its own window
            if (AllowDragDrop)
            {
                var selNode = FSelectionList[FHoverLine + ScrolledLine];
                TagsTextBox.DoDragDrop(string.Format("{0}||{1}", selNode.Systemname, selNode.Filename), DragDropEffects.All);
                return;
            }
            //else popped up on doubleclick
            else if (e.Button == MouseButtons.Left)
                CreateNodeFromHoverLine();
            else
            {
                try
                {
                    var selNode = FSelectionList[FHoverLine + ScrolledLine];
                    if (e.Button == MouseButtons.Middle)
                        OnShowNodeReference(selNode);
                    else
                    {
                    	TagsTextBox.Text = "";
                        OnShowHelpPatch(selNode);
                    }
                }
                catch //username is a filename..do nothing
                {}
            }
        }
        
        void RichTextBoxMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (FRichTextBox.Lines.Length == 0)
                return;
            
            var charIndex = FRichTextBox.GetCharIndexFromPosition(e.Location);
            int newHoverLine = FRichTextBox.GetLineFromCharIndex(charIndex);
            
            //avoid some flicker
            if (newHoverLine != FHoverLine)
//			if ((e.Location.X != FLastMouseHoverLocation.X) || (e.Location.Y != FLastMouseHoverLocation.Y))
            {
                FLastMouseHoverLocation = e.Location;
                FHoverLine = newHoverLine;
                ShowToolTip(e.X + DIPX(15));
                RedrawSelection();
            }
        }
        
        void RichTextBoxMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //if cloned via ctrl+click the self is now hidden
            //and we don't want the nodebrowser to vanish yet
            if (Visible)
            {
                //hack: called only to re-focus active patch
                //after this mouseup set the focus to the already hidden NodeBrowser window
                OnCreateNodeFromString("");
                
                TagsTextBox.Focus();
            }
        }
        
        private void ShowToolTip(int x)
        {
            var selectionIndex = FHoverLine + FScrolledLine;
            if (selectionIndex < 0 || selectionIndex >= FSelectionList.Count) return;
            
            var ni = FSelectionList[FHoverLine + ScrolledLine];

            int y = FRichTextBox.GetPositionFromCharIndex(FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)).Y;
            string tip = "";
            
            if (!string.IsNullOrEmpty(ni.Shortcut))
                tip = "(" + ni.Shortcut + ") " ;
            if (!string.IsNullOrEmpty(ni.Help))
            	tip += ni.Help.Trim();
            if (!string.IsNullOrEmpty(ni.Warnings))
                tip += "\n WARNINGS: " + ni.Warnings.Trim();
            if (!string.IsNullOrEmpty(ni.Bugs))
                tip += "\n BUGS: " + ni.Bugs.Trim();
            if ((!string.IsNullOrEmpty(ni.Author)) && (ni.Author != "vvvv group"))
                tip += "\n AUTHOR: " + ni.Author.Trim();
            if (!string.IsNullOrEmpty(ni.Credits))
                tip += "\n CREDITS: " + ni.Credits.Trim();
            if (ni.Type == NodeType.Dynamic || ni.Type == NodeType.Effect || ni.Type == NodeType.VL || ni.Type == NodeType.Module)
                tip += "\n Use CTRL+Enter or CTRL+Click to clone this node.";
            if (!string.IsNullOrEmpty(ni.Filename))
                tip += "\n\n" + ni.Filename;

            if (!string.IsNullOrEmpty(tip))
            	FToolTip.Show(tip.Trim(), FRichTextBox, x, y + DIPX(15));
            else
                FToolTip.Hide(FRichTextBox);
        }
        
        private IEnumerable<INodeInfo> ExtractSubList(IEnumerable<INodeInfo> nodeInfos)
        {
            if (FTags.Count == 0)
                return nodeInfos;
            else
            {
                var regex = new Regex(@"\[(.+)\]");
                return nodeInfos.Where((nodeInfo) =>
                                           {
                                               var displayName = NodeInfoToDisplayName(nodeInfo);
                                               //bezier hack
                                               displayName = displayName.Replace('é', 'e');

                                               //make all tags ToUpperFirst so they can be found
                                               var match = regex.Match(displayName);
                                               var upperedTags = match.Value.TrimStart('[').TrimEnd(']').Split(',').Select(x => x.Trim().ToUpperFirstInvariant());
                                               displayName = regex.Replace(displayName, string.Join(", ", upperedTags));

                                               var lowerDisplayName = displayName.ToLower();
                                               bool containsAll = true;
                                               string t = "";
                                               foreach (string tag in FTags)
                                               {
                                                   t = tag.ToLower();

                                                   var found = false;
                                                   if (FSwizzles.Contains(t))
                                                       found = displayName.ToLower().Contains(t);

                                                   if (!found && FSingleCharNodes.Contains(t))
                                                       found = displayName.ToLower().StartsWith(t);

                                                   if (!found)
                                                   {
                                                       //first we check case-insensitive if the displayname simply start with the given tag
                                                       found = displayName.ToLower().StartsWith(t);
                                                       if (!found)
                                                       {
                                                           //then we consider the tag a sub-term in the displayname 
                                                           //and therefore want it to start with a capital letter
                                                           //so that we don't just find any occurances 
                                                           //but only those were the tag appears as a proper camel-cased sub-term
                                                           
                                                           t = t.ToUpperFirstInvariant();
                                                           //first char matches case-sensitive, all later chars match insensitive
                                                           var pattern = "(" + Regex.Escape(t[0].ToString()) + "(?i)" + Regex.Escape(string.Join("", t.Skip(1))) + "(?-i))";
                                                           var rex = new Regex(pattern);
                                                           var matches = rex.Match(displayName);
                                                           found = matches.Length > 0;
                                                       }
                                                   }

                                                   if (found)
                                                   {
                                                       if (!AndTags)
                                                           break;
                                                   }
                                                   else
                                                       containsAll = false;

                                                   if ((AndTags) && (!containsAll))
                                                       break;
                                               }

                                               //todo: remove OR-tags case or refine it 
                                               if (((AndTags) && (containsAll)) || ((!AndTags) && (lowerDisplayName.Contains(t.ToLower()))))
                                                   return true;
                                               else
                                                   return false;
                                           });
            }
        }
        
        private bool IsAvailableInActivePatch(INodeInfo nodeInfo)
        {
            return IsAvailableInActivePatch(nodeInfo, true);
        }
        
        private bool IsAvailableInActivePatch(INodeInfo nodeInfo, bool lookInSearchPaths)
        {
            string filename = nodeInfo.Filename;
            if (string.IsNullOrEmpty(filename))
                return true;
            
            string dir = Path.GetDirectoryName(filename);
            if (string.IsNullOrEmpty(dir))
                return true;
            
            // Hack to make it work again 50
            var nodeCollection = NodeBrowser.FNodeCollection;
            if (nodeCollection == null)
                return true;
            
            if (nodeInfo != FCurrentPatchWindowNodeInfo)
            {
                if (!string.IsNullOrEmpty(FCurrentDir))
                    //available if local
                    if (dir.StartsWith(FCurrentDir))
                        return true;
                
                if (lookInSearchPaths && nodeInfo.Type != NodeType.Patch)
                {
                    //available if from any of the global paths
                    return nodeCollection.IsInUserDefinedSearchPath(nodeInfo.Factory, dir);
                }
            }
            return false;
        }
        
        private INodeInfo FCurrentPatchWindowNodeInfo;
        private string FCurrentDir;
        private void FilterNodesByTags()
        {
            if (NodeBrowser == null)
                return;
            
            //            FNeedsUpdate = false;
            
            FSelectionList.Clear();

            var nodeInfos = NodeBrowser.NodeInfoFactory.NodeInfos.Where(ni => ni.Ignore == false && NodeBrowser.CategoryFilter.CategoryVisible(ni.Category));
            if (FShowInternal)
                nodeInfos = nodeInfos.Where(ni => ni.Version.Contains("Internal"));
            else
                nodeInfos = nodeInfos.Where(ni => !ni.Version.Contains("Internal"));

            // Cache current patch window nodeinfo and current dir
            var currentPatchWindow = NodeBrowser.CurrentPatchWindow;
            FCurrentPatchWindowNodeInfo = currentPatchWindow != null ? currentPatchWindow.Node.NodeInfo : null;
            FCurrentDir = NodeBrowser.CurrentDir;
            
            if (FNodeFilter == -1)
                nodeInfos = nodeInfos.Where(nodeInfo => nodeInfo.Type == NodeType.Native || IsAvailableInActivePatch(nodeInfo));
            else if (FNodeFilter == -2)
                nodeInfos = nodeInfos.Where(nodeInfo => nodeInfo.Type != NodeType.Native && IsAvailableInActivePatch(nodeInfo, false));
            else if (FNodeFilter == (int) NodeType.Native)
                nodeInfos = nodeInfos.Where(nodeInfo => nodeInfo.Type == NodeType.Native);
            else
            {
                NodeType nodeType = (NodeType) FNodeFilter;
                nodeInfos = nodeInfos.Where(nodeInfo => nodeInfo.Type == nodeType && IsAvailableInActivePatch(nodeInfo));
            }
            
            FSelectionList = ExtractSubList(nodeInfos).ToList();
            
            FSelectionList.Sort(SortNodeInfo);

            if (FNodeCountLabel.InvokeRequired)
                FNodeCountLabel.Invoke(new MethodInvoker(() =>
                                                         {
                                                             FNodeCountLabel.Text = "Matching Nodes: " + FSelectionList.Count.ToString();
                                                         }));
            else
                //FCategoryTreeViewer.Reload();
                FNodeCountLabel.Text = "Matching Nodes: " + FSelectionList.Count.ToString();
        }
        
        private readonly Regex FCatRegExp = new Regex(@"\((.*)\)(.*)$");
        private readonly string[] FSwizzles = new string[8]{"xy", "xyz", "xz", "yz", "xyw", "xyzw", "xzw", "yzw"};
        private readonly string[] FSingleCharNodes = new string[3] { "i", "s", "r" };

        private int Weight(int lastWeight, string text, string tag)
        {
            var pos = text.IndexOf(tag);
            if (pos > -1)
            {
                //the following won't work well for swizzles, like: xyZ, Xyz
                //so simply exclude those
                if (FSwizzles.Contains(tag.ToLower()))
                    return Math.Min(lastWeight, pos);

                if (FSingleCharNodes.Contains(tag.ToLower()))
                    return Math.Min(lastWeight, pos);

                //do the following finegrained check only for tags found before the category/version/tags
                if (pos < text.IndexOf(" ("))
                {
                    //see if the tag is a complete subword in the nodename, like "Editor" in "PointEditorState"
                    //if so, that counts more
                    //it counts even more if that tag is also the last word in a nodename, like "Editor" in "PointEditor"
                    //so check if there is either: 
                    //- no character following
                    //- or at least two characters following where the first isUpper, the second isLower 
                    if (text.Length >= pos + tag.Length)
                        return (text[pos + tag.Length] == ' ') ? Math.Min(pos, 1) : pos;
                    else if (text.Length >= pos + tag.Length + 1)
                    {
                        var nextChar = pos + tag.Length;
                        return (char.IsUpper(text[nextChar]) && char.IsLower(text[nextChar + 1])) ? Math.Min(pos, 1) : pos;
                    }
                }

                //otherwise simply the position of the tag is the pos
                return Math.Min(lastWeight, pos);
            }            
            else
                return lastWeight;
        }

        private int SortNodeInfo(INodeInfo n1, INodeInfo n2)
        {
            var s1 = NodeInfoToDisplayName(n1).ToLower();
            var s2 = NodeInfoToDisplayName(n2).ToLower();
            
            //Workaround: Following code assumes s1 and s2 are either a filename
            //or include an opening parenthesis. Since node info rework there're
            //also systemnames without any parenthesis. So for now add one if missing.
            if (s1.IndexOf('(') < 0)
                s1 = s1 + "()";
            if (s2.IndexOf('(') < 0)
                s2 = s2 + "()";
            
            //create a weighting index depending on the indices the tags appear in the nodenames
            //earlier appearance counts more
            int w1 = int.MaxValue, w2 = int.MaxValue;
            string t = "";
            foreach (string tag in FTags)
            {
                t = tag.TrimStart(new char[1]{'.'});

                w1 = Weight(w1, s1, t);
                w2 = Weight(w2, s2, t);
            }
            
            if (w1 != w2)
            {
                if (w1 < w2)
                    return -1;
                else
                    return 1;
            }
            
            //if weights are equal, compare the nodenames
            string name1 = s1.Substring(0, s1.IndexOf('('));
            string name2 = name2 = s2.Substring(0, s2.IndexOf('('));
            
            //compare only the nodenames
            int comp = name1.CompareTo(name2);
            
            //if names are equal
            if (comp == 0)
            {
                //compare categories
                Match match = null;
                match = FCatRegExp.Match(s1);
                string cat1 = match.Success ? match.Groups[1].Value : string.Empty;
                match = FCatRegExp.Match(s2);
                string cat2 = match.Success ? match.Groups[1].Value : string.Empty;
                
                int v1, v2;
                
                //special sorting for categories
                v1 = FCategoryPriorities.IndexOf(cat1.Split(' ')[0].ToUpper());
                v2 = FCategoryPriorities.IndexOf(cat2.Split(' ')[0].ToUpper());
                
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
        }

        private void PrepareRTF()
        {
            string n;
            char[] bolded;
            
            var sb = new System.Text.StringBuilder();
            FRTFSelectionList.Clear();
            
            foreach (INodeInfo nodeInfo in FSelectionList)
            {
                //all comparison is case-in-sensitive
                var s = NodeInfoToDisplayName(nodeInfo);
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
                int markupChars = 0;
                sb.Remove(0, sb.Length);
                for (int i=0; i<s.Length; i++)
                    if (bolded[i] == '°')
                {
                    sb.Append("\\b " + s[i] + "\\b0 ");
                    markupChars += 7;
                }
                else
                    sb.Append(s[i]);
                
                n = sb.ToString();

                FRTFSelectionList.Add(GetNodeTypePrefix(nodeInfo) + "\t" + n.PadRight(CLineLength + markupChars) + "\\par ");
            }
        }

        string GetNodeTypePrefix(INodeInfo nodeInfo)
        {
            switch (nodeInfo.Type)
            {
                case NodeType.Native: return "";
                case NodeType.Module: return " m";
                case NodeType.Plugin: return " p";
                case NodeType.Dynamic: return " d";
                case NodeType.Freeframe: return " f";
                case NodeType.Effect: return " x";
                case NodeType.VST: return " a";
                case NodeType.VL: return " v";
                default: return " t";
            }
        }

        private void UpdateRichTextBox()
        {
            int maxLine = Math.Min(ScrolledLine + FVisibleLines, FRTFSelectionList.Count);
            var rtfBuilder = new StringBuilder(CRTFHeader);
            
            for (int i = ScrolledLine; i < maxLine; i++)
            {
                rtfBuilder.Append(FRTFSelectionList[i]);
            }
            
            //seems mono adds a \par here automatically, so remove one
            string rtf = rtfBuilder.ToString();
            rtf = rtf.TrimEnd(new char[5]{'\\', 'p', 'a', 'r', ' '});// + "}";

            if (FRichTextBox.InvokeRequired)
                FRichTextBox.Invoke(new MethodInvoker(() => { FRichTextBox.Rtf = rtf; }));
            else
                FRichTextBox.Rtf = rtf;
        }

        public void Redraw()
        {
            FShowInternal = false;

            string query = TagsTextBox.Text.ToLower();
            query += (char) 160;
            FTags = query.Split(new char[1]{' '}).ToList();
            
            FNodeFilter = -1;
            if (FTags.Contains("n"))
            {
                FNodeFilter = (int) NodeType.Native;
                FTags.Remove("n");
            }
            else if (FTags.Contains("t"))
            {
                FNodeFilter = (int) NodeType.Patch;
                FTags.Remove("t");
            }
            else if (FTags.Contains("m"))
            {
                FNodeFilter = (int) NodeType.Module;
                FTags.Remove("m");
            }
            else if (FTags.Contains("f"))
            {
                FNodeFilter = (int) NodeType.Freeframe;
                FTags.Remove("f");
            }
            else if (FTags.Contains("x"))
            {
                FNodeFilter = (int) NodeType.Effect;
                FTags.Remove("x");
            }
            else if (FTags.Contains("p"))
            {
                FNodeFilter = (int) NodeType.Plugin;
                FTags.Remove("p");
            }
            else if (FTags.Contains("d"))
            {
                FNodeFilter = (int) NodeType.Dynamic;
                FTags.Remove("d");
            }
            else if (FTags.Contains("a"))
            {
                FNodeFilter = (int) NodeType.VST;
                FTags.Remove("a");
            }
            else if (FTags.Contains("v"))
            {
                FNodeFilter = (int) NodeType.VL;
                FTags.Remove("v");
            }
            else if (FTags.Contains("."))
            {
                FNodeFilter = -2;
                FTags.Remove(".");
            }
            else if (FTags.Contains("i"))
            {
                FShowInternal = true;
                FTags.Remove("i");
            }

            //clean up the list
            FTags[FTags.Count-1] = FTags[FTags.Count-1].Trim((char) 160);
            while (FTags.Contains(" "))
                FTags.Remove(" ");
            if (FTags.Contains(""))
                FTags.Remove("");
            
            FilterNodesByTags();
            PrepareRTF();
            
            if (FScrollBar.InvokeRequired)
                FScrollBar.Invoke(new MethodInvoker(() =>
                                                    {
                                                        FScrollBar.Maximum = Math.Max(0, FSelectionList.Count - FVisibleLines + FScrollBar.LargeChange - 1);
                                                    }));
            else
                FScrollBar.Maximum = Math.Max(0, FSelectionList.Count - FVisibleLines + FScrollBar.LargeChange - 1);
            
            //calling UpdateRichTexBox()
            ScrolledLine = 0;
            
            PendingRedraw = false;
        }

        private void RedrawSelection()
        {
            //clear old selection
            FRichTextBox.SelectionBackColor = Color.Silver;

            if (FHoverLine > -1)
            {
                //draw current selection
                FRichTextBox.SelectionStart = FRichTextBox.GetFirstCharIndexFromLine(FHoverLine);
                FRichTextBox.SelectionLength = CLineLength;
                FRichTextBox.SelectionBackColor = CHoverColor;
            }
            
            //make sure the selection is also drawn in the NodeTypePanel
            FRichTextBox.Invalidate();
        }

        void FScrollBarValueChanged(object sender, EventArgs e)
        {
            FScrolledLine = FScrollBar.Value;
            UpdateRichTextBox();
            FToolTip.Hide(FRichTextBox);
        }

        void HandleRichTextBoxResize(object sender, EventArgs e)
        {
        	FVisibleLines = FRichTextBox.Height / CLineHeight;
            Redraw();
        }
        #endregion RichTextBox

        void TagPanelVisibleChanged(object sender, EventArgs e)
        {
        	//TagsTextBox not assigned during designtime
        	if (TagsTextBox != null)
        	{
	            TagsTextBox.Text = TagsTextBox.Text.Trim();
	            TagsTextBox.Focus();
        	}
            FToolTip.Hide(FRichTextBox);
            
            if (PendingRedraw)
            {
                Redraw();
            }
        }
        
        void FRichTextBoxMouseLeave(object sender, EventArgs e)
        {
            FToolTip.Hide(this);
        }
        
        void FNodeTypePanelMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //also trigger nodeinsert with click on nodetypepanel
            RichTextBoxMouseDown(sender, e);
        }
    }
}