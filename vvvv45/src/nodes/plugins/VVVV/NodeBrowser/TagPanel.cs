using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
		
		private int FVisibleLines = 16;
		private Color CLabelColor = Color.FromArgb(255, 154, 154, 154);
		private Color CHoverColor = Color.FromArgb(255, 216, 216, 216);
		private const string CRTFHeader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Verdana;}}\viewkind4\uc1\pard\f0\fs17 ";
		private const int CLineHeight = 13;
		private int FHoverLine;
		private List<string> FTags;
		private Point FLastMouseHoverLocation = new Point(0, 0);
		private int FNodeFilter;

		private List<INodeInfo> FSelectionList = new List<INodeInfo>();
		private List<string> FRTFSelectionList = new List<string>();
		
		public NodeBrowserPluginNode NodeBrowser {get; set;}
		public bool AndTags {get; set;}
		public bool AllowDragDrop {get; set;}
		private bool FNeedsUpdate;
		public bool NeedsUpdate
		{
			get {return FNeedsUpdate;}
			set
			{
				if (FNeedsUpdate != value)
				{
					if (Visible)
						FilterNodesByTags();
					FNeedsUpdate = value;
				}
			}
		}
		
		private int FScrolledLine;
		private int ScrolledLine
		{
			get {return FScrolledLine;}
			set
			{
				FScrolledLine = Math.Max(0, Math.Min(FSelectionList.Count - FVisibleLines, value));
				FScrollBar.Value = FScrolledLine;
				UpdateRichTextBox();
			}
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
			
			FTagsTextBox.ContextMenu = new ContextMenu();
			FTagsTextBox.MouseWheel += FTagsTextBoxMouseWheel;
			FRichTextBox.MouseWheel += FTagsTextBoxMouseWheel;
		}
		
		private void ToolTipPopupHandler(object sender, PopupEventArgs e)
		{
			e.ToolTipSize = new Size(Math.Min(e.ToolTipSize.Width, 300), e.ToolTipSize.Height);
		}
		
		private string NodeInfoToDisplayName(INodeInfo nodeInfo)
		{
			string tags = nodeInfo.Tags;
			if ((!string.IsNullOrEmpty(nodeInfo.Author)) && (nodeInfo.Author != "vvvv group"))
				tags += ", " + nodeInfo.Author;

			if (!string.IsNullOrEmpty(nodeInfo.Tags))
				return nodeInfo.Username + " [" + tags + "]";
			else
				return nodeInfo.Username;
		}
		
		public void Initialize(string text)
		{
			if (string.IsNullOrEmpty(text))
				FTagsTextBox.Text = "";
			else
				FTagsTextBox.Text = text.Trim();

			FTagsTextBox.SelectAll();
			
			FHoverLine = -1;
			ScrolledLine = 0;
			
			if (NeedsUpdate)
                Redraw();
			RedrawSelection();
		}
		
		public void AfterShow()
		{
			FTagsTextBox.Focus();
		}
		
		public void BeforeHide()
		{
			FToolTip.Hide(FRichTextBox);
		}
		
		void CreateNodeFromHoverLine()
		{
			try
			{
				var selNode = FSelectionList[FHoverLine + ScrolledLine];
				if ((Control.ModifierKeys == Keys.Control) && ((selNode.Type == NodeType.Dynamic) || (selNode.Type == NodeType.Effect)))
					OnPanelChange(NodeBrowserPage.Clone, selNode);
				else
					OnCreateNode(selNode);
			}
			catch
			{
				OnCreateNodeFromString(FTagsTextBox.Text.Trim());
			}
		}
		#region TagsTextBox
		void FTagsTextBoxTextChanged(object sender, EventArgs e)
		{
			FTagsTextBox.Height = Math.Max(20, FTagsTextBox.Lines.Length * CLineHeight + 7);
			
			//saving the last manual entry for recovery when stepping through list and reaching index -1 again
			FToolTip.Hide(FRichTextBox);
			
			Redraw();
			
			if (FRichTextBox.Lines.Length > 0)
				FHoverLine = 0;
			else
				FHoverLine = -1;
			
			RedrawSelection();
		}

		void FTagsTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
			{
				if (!e.Shift)
					CreateNodeFromHoverLine();
			}
			else if (e.KeyCode == Keys.Escape)
				OnCreateNode(null);
			else if ((FTagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Down))
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
				ShowToolTip();
			}
			else if ((FTagsTextBox.Lines.Length < 2) && (e.KeyCode == Keys.Up))
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
				ShowToolTip();
			}
			else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
			{
				if (FHoverLine != -1)
				{
					FHoverLine = -1;
					FTagsTextBox.SelectionStart = FTagsTextBox.Text.Length;
					RedrawSelection();
				}
			}
			else if ((e.Control) && (e.KeyCode == Keys.A))
			{
				FTagsTextBox.SelectAll();
			}
		}
		
		void FTagsTextBoxMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//do this in mouseup (not mousedown) for ContextMenu not throwing error
			if (e.Button == MouseButtons.Right)
			{
				OnPanelChange(NodeBrowserPage.ByCategory, null);
			}
		}
		
		void FTagsTextBoxMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
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
			string username = FRichTextBox.Lines[FHoverLine].Trim();
			FRichTextBox.SelectionStart = FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)+1;
			FTagsTextBox.Focus();
			
			//as plugin in its own window
			if (AllowDragDrop)
			{
				var selNode = FSelectionList[FHoverLine + ScrolledLine];
				FTagsTextBox.DoDragDrop(selNode.Systemname, DragDropEffects.All);
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
						OnShowHelpPatch(selNode);
				}
				catch //username is a filename..do nothing
				{}
			}
		}
		
		void RichTextBoxMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (FRichTextBox.Lines.Length == 0)
				return;
			
			int newHoverLine = FRichTextBox.GetLineFromCharIndex(FRichTextBox.GetCharIndexFromPosition(e.Location));
			
			//avoid some flicker
			if ((e.Location.X != FLastMouseHoverLocation.X) || (e.Location.Y != FLastMouseHoverLocation.Y))
			{
				FLastMouseHoverLocation = e.Location;
				FHoverLine = newHoverLine;
				if (sender != FNodeTypePanel)
				    ShowToolTip();
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
				
				FTagsTextBox.Focus();
			}
		}
		
		private void ShowToolTip()
		{
			if (FHoverLine == -1)
				return;
			
			var ni = FSelectionList[FHoverLine + ScrolledLine];

			int y = FRichTextBox.GetPositionFromCharIndex(FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)).Y;
			string tip = "";
			if (ni.Type == NodeType.Dynamic || ni.Type == NodeType.Effect)
				tip = "Use CTRL+Enter or CTRL+Click to clone this node.\n";
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
		
		private List<INodeInfo> ExtractSubList(List<INodeInfo> InputList)
		{
			if (FTags.Count == 0)
				return InputList;
			else
				return InputList.FindAll(delegate(INodeInfo nodeInfo)
				                         {
				                         	var displayName = nodeInfo.Username;
				                         	displayName = displayName.ToLower();
				                         	displayName = displayName.Replace('é', 'e');
				                         	bool containsAll = true;
				                         	string t = "";
				                         	foreach (string tag in FTags)
				                         	{
				                         		t = tag.ToLower();
				                         		t = t.TrimStart(new char[1]{'.'});
				                         		if (displayName.Contains(t))
				                         		{
				                         			if (!AndTags)
				                         				break;
				                         		}
				                         		else
				                         		{
				                         			containsAll = false;
				                         			break;
				                         		}
				                         	}
				                         	
				                         	if (((AndTags) && (containsAll)) || ((!AndTags) && (displayName.Contains(t))))
				                         		return true;
				                         	else
				                         		return false;
				                         });
		}
		
		private bool IsAvailableInActivePatch(INodeInfo nodeInfo, bool localOnly)
		{
            var fn = "";
            try
            {
                fn = Path.GetDirectoryName(nodeInfo.Filename);
            }
            catch
            {

            }
            
            if (NodeBrowser.CurrentPatchWindow == null || nodeInfo != NodeBrowser.CurrentPatchWindow.GetNode().GetNodeInfo())
			{
				if (!string.IsNullOrEmpty(NodeBrowser.CurrentDir))
					//available if local
					if (fn.StartsWith(NodeBrowser.CurrentDir))
						return true;
				
				if (!localOnly && NodeBrowser.NodeCollection != null)
					//available if from any of the global paths
					foreach (var sp in NodeBrowser.NodeCollection.Paths)
						if (fn.StartsWith(sp.Path))
							return true;
			}
			return false;
		}
		
		private void FilterNodesByTags()
		{
			if (NodeBrowser == null)
				return;
			
			FNeedsUpdate = false;
			
			FSelectionList.Clear();

			var nodeinfos = NodeBrowser.NodeInfoFactory.NodeInfos.ToList().FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Ignore == false;});

			//show natives only
			if (FNodeFilter == (int) NodeType.Native)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Native;});
			//show patches only
			else if (FNodeFilter == (int) NodeType.Patch)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Patch && IsAvailableInActivePatch(nodeInfo, true);});
			//show modules only
			else if (FNodeFilter == (int) NodeType.Module)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Module && IsAvailableInActivePatch(nodeInfo, false);});
			//show effects only
			else if (FNodeFilter == (int) NodeType.Effect)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Effect && IsAvailableInActivePatch(nodeInfo, false);});
			//show freeframes only
			else if (FNodeFilter == (int) NodeType.Freeframe)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Freeframe && IsAvailableInActivePatch(nodeInfo, false);});
			//show plugins only
			else if (FNodeFilter == (int) NodeType.Plugin)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Plugin && IsAvailableInActivePatch(nodeInfo, false);});
			//show dynamics only
			else if (FNodeFilter == (int) NodeType.Dynamic)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Dynamic && IsAvailableInActivePatch(nodeInfo, false);});
			//show vsts only
			else if (FNodeFilter == (int) NodeType.VST)
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.VST && IsAvailableInActivePatch(nodeInfo, false);});
			//show all but local addons not local to CurrentDir
			else
			{
				FSelectionList = nodeinfos.FindAll(delegate(INodeInfo nodeInfo)
				                                   {
				                                   	return (nodeInfo.Type == NodeType.Native)
				                                   		|| (nodeInfo.Type == NodeType.Patch && IsAvailableInActivePatch(nodeInfo, true))
				                                   		|| (nodeInfo.Type != NodeType.Native && nodeInfo.Type != NodeType.Patch && IsAvailableInActivePatch(nodeInfo, false));
				                                   });
			}
			
			FSelectionList = ExtractSubList(FSelectionList);
			
			FSelectionList.Sort(delegate(INodeInfo n1, INodeInfo n2)
			                    {
			                    	var s1 = NodeInfoToDisplayName(n1);
			                    	var s2 = NodeInfoToDisplayName(n2);
			                    	
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
			                    		if (s1.ToLower().IndexOf(t) > -1)
			                    			w1 = Math.Min(w1, s1.ToLower().IndexOf(t));
			                    		if (s2.ToLower().IndexOf(t) > -1)
			                    			w2 = Math.Min(w2, s2.ToLower().IndexOf(t));
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
			                    		string cat1 = s1.Substring(s1.IndexOf('(')).Trim(new char[2]{'(', ')'});
			                    		string cat2 = s2.Substring(s2.IndexOf('(')).Trim(new char[2]{'(', ')'});
			                    		
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
			
			if (FNodeCountLabel.InvokeRequired)
				FNodeCountLabel.Invoke(new MethodInvoker(() =>
				                                         {
				                                         	FNodeCountLabel.Text = "Matching Nodes: " + FSelectionList.Count.ToString();
				                                         }));
			else
				//FCategoryTreeViewer.Reload();
				FNodeCountLabel.Text = "Matching Nodes: " + FSelectionList.Count.ToString();
		}

		private void PrepareRTF()
		{
			string n;
			char[] bolded;
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
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
			
			if (FRichTextBox.InvokeRequired)
				FRichTextBox.Invoke(new MethodInvoker(() => { FRichTextBox.Rtf = rtf + "}"; }));
			else
				FRichTextBox.Rtf = rtf + "}";
			
			FNodeTypePanel.Invalidate();
		}

		public void Redraw()
		{
			string query = FTagsTextBox.Text.ToLower();
			query += (char) 160;
			FTags = query.Split(new char[1]{' '}).ToList();
			
			FNodeFilter = -1;
			if (FTags.Contains("n"))
			{
				FNodeFilter = (int) NodeType.Native;
				FTags.Remove("n");
			}
			else if (FTags.Contains("."))
			{
				FNodeFilter = (int) NodeType.Patch;
				FTags.Remove(".");
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
			else if (FTags.Contains("v"))
			{
				FNodeFilter = (int) NodeType.VST;
				FTags.Remove("v");
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
		}

		private void RedrawSelection()
		{
			//clear old selection
			FRichTextBox.SelectionBackColor = Color.Silver;

			if (FHoverLine > -1)
			{
				//draw current selection
				int offset = 0;
				for (int i = 0; i < FHoverLine; i++)
					offset += FRichTextBox.Lines[i].Length;
				
				FRichTextBox.SelectionStart = offset;
				FRichTextBox.SelectionLength = FRichTextBox.Lines[FHoverLine].Length;
				FRichTextBox.SelectionBackColor = CHoverColor;
			}
			
			//make sure the selection is also drawn in the NodeTypePanel
			FNodeTypePanel.Invalidate();
		}

		void FScrollBarValueChanged(object sender, EventArgs e)
		{
			FScrolledLine = FScrollBar.Value;
			UpdateRichTextBox();
			FToolTip.Hide(FRichTextBox);
		}

		void FNodeTypePanelPaint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(Color.Silver);
			
			int maxLine = Math.Min(FVisibleLines, FSelectionList.Count);
			for (int i = 0; i < maxLine; i++)
			{
				int index = Math.Min(i + ScrolledLine, FSelectionList.Count-1);
				int y = (i * CLineHeight) + 4;
				
				if (FHoverLine == i)
					using (SolidBrush b = new SolidBrush(CHoverColor))
						e.Graphics.FillRectangle(b, new Rectangle(0, y-4, 21, CLineHeight));
				
				var nodeType = FSelectionList[index].Type;
				{
					using (SolidBrush b = new SolidBrush(Color.Black))
						switch (nodeType)
					{
						case NodeType.Native:
							{
								break;
							}
						case NodeType.Module:
							{
								e.Graphics.DrawString("M", FRichTextBox.Font, b, 5, y-3, StringFormat.GenericDefault);
								break;
							}
						case NodeType.Plugin:
							{
								e.Graphics.DrawString("P", FRichTextBox.Font, b, 6, y-3, StringFormat.GenericDefault);
								break;
							}
						case NodeType.Dynamic:
							{
								e.Graphics.DrawString("D", FRichTextBox.Font, b, 6, y-3, StringFormat.GenericDefault);
								break;
							}
						case NodeType.Freeframe:
							{
								e.Graphics.DrawString("FF", FRichTextBox.Font, b, 4, y-3, StringFormat.GenericDefault);
								break;
							}
						case NodeType.Effect:
							{
								e.Graphics.DrawString("FX", FRichTextBox.Font, b, 4, y-3, StringFormat.GenericDefault);
								break;
							}
						case NodeType.VST:
							{
								e.Graphics.DrawString("V", FRichTextBox.Font, b, 4, y-3, StringFormat.GenericDefault);
								break;
							}
							// Added code:
						default:
							{
								e.Graphics.DrawString(".", FRichTextBox.Font, b, 5, y-3, StringFormat.GenericDefault);
								break;
							}
					}
				}
			}
		}

		void FRichTextBoxResize(object sender, EventArgs e)
		{
			FVisibleLines = FRichTextBox.Height / CLineHeight;
			Redraw();
		}
		#endregion RichTextBox

		void TagPanelVisibleChanged(object sender, EventArgs e)
		{
			FTagsTextBox.Text = FTagsTextBox.Text.Trim();
			FTagsTextBox.Focus();
			FToolTip.Hide(FRichTextBox);
			
			if (Visible && NeedsUpdate)
				FilterNodesByTags();
		}
		
		void FRichTextBoxMouseLeave(object sender, EventArgs e)
		{
		    FToolTip.Hide(this);
		}
	}
}