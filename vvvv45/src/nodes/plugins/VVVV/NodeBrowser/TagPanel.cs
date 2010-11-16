using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
		private ToolTip FToolTip = new ToolTip();
		private string[] FTags = new string[0];
		private Point FLastMouseHoverLocation = new Point(0, 0);
		private int FNodeFilter;
		/*
		List<string> FSelectionList = new List<string>();
		 */
		List<INodeInfo> FSelectionList = new List<INodeInfo>();
		List<string> FRTFSelectionList = new List<string>();
//		List<string> FNodeList = new List<string>();
		Dictionary<INodeInfo, string> FNodeDict = new Dictionary<INodeInfo, string>();
		Dictionary<string, INodeInfo> FSystemNameToNodeDict = new Dictionary<string, INodeInfo>();
		NodeBrowserPluginNode FNodeBrowserNode;
		
		public bool AndTags {get; set;}
		public bool AllowDragDrop {get; set;}
		
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
		
		public TagPanel()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FToolTip.BackColor = CLabelColor;
			FToolTip.ForeColor = Color.White;
			FToolTip.ShowAlways = false;
			FToolTip.Popup += new PopupEventHandler(ToolTipPopupHandler);
			
			FTagsTextBox.ContextMenu = new ContextMenu();
			FTagsTextBox.MouseWheel += new MouseEventHandler(FTagsTextBoxMouseWheel);
			FRichTextBox.MouseWheel += new MouseEventHandler(FTagsTextBoxMouseWheel);
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
		
		public void Add(INodeInfo nodeInfo)
		{
			/*
			string key = NodeInfoToDisplayName(nodeInfo);
			
			FNodeList.Add(key);
			FNodeDict[key] = nodeInfo;
			 */
			
			if (!nodeInfo.Ignore)
			{
				string displayName = NodeInfoToDisplayName(nodeInfo);
				FNodeDict[nodeInfo] = displayName;
				
				INodeInfo oldNodeInfo = null;
				var systemname = nodeInfo.Systemname;
				if (FSystemNameToNodeDict.TryGetValue(systemname, out oldNodeInfo))
				{
					FNodeDict.Remove(oldNodeInfo);
				}
				FSystemNameToNodeDict[systemname] = nodeInfo;
			}
		}
		
		public void Update(INodeInfo nodeInfo)
		{
			/*
			string oldkey = "";
			string newkey = NodeInfoToDisplayName(nodeInfo);
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
			 */
			
			if (!nodeInfo.Ignore)
			{
				string displayName = NodeInfoToDisplayName(nodeInfo);
				FNodeDict[nodeInfo] = displayName;
			}
			else
			{
				FNodeDict.Remove(nodeInfo);
			}
		}
		
		public void Remove(INodeInfo nodeInfo)
		{
			/*
			string key = NodeInfoToDisplayName(nodeInfo);
			FNodeDict.Remove(key);
			FNodeList.Remove(key);
			 */
			
			FNodeDict.Remove(nodeInfo);
		}
		
		public void Initialize(NodeBrowserPluginNode nodeBrowserNode, string text)
		{
			FNodeBrowserNode = nodeBrowserNode;
			
			if (string.IsNullOrEmpty(text))
				FTagsTextBox.Text = "";
			else
				FTagsTextBox.Text = text.Trim();

			FTagsTextBox.SelectAll();
			
			FHoverLine = -1;
			ScrolledLine = 0;
			
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
			string text = "";
			try
			{
				/*
				text = FRichTextBox.Lines[FHoverLine].Trim();
				
				INodeInfo selNode = FNodeDict[text];
				 */
				
				var selNode = FSelectionList[FHoverLine + ScrolledLine];
				if ((Control.ModifierKeys == Keys.Control) && ((selNode.Type == NodeType.Dynamic) || (selNode.Type == NodeType.Effect)))
					OnPanelChange(NodeBrowserPage.Clone, selNode);
				else
					OnCreateNode(selNode);
			}
			catch
			{
				if ((text.EndsWith(".v4p")) || (text.EndsWith(".fx")) || (text.EndsWith(".dll")))
					OnCreateNodeFromString(text);
				else
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
		
		void FTagsTextBoxMouseUp(object sender, MouseEventArgs e)
		{
			//do this in mouseup (not mousedown) for ContextMenu not throwing error
			if (e.Button == MouseButtons.Right)
			{
				OnPanelChange(NodeBrowserPage.ByCategory, null);
			}
		}
		
		void FTagsTextBoxMouseWheel(object sender, MouseEventArgs e)
		{
			//clear old selection
			FRichTextBox.SelectionBackColor = Color.Silver;
			
			int scrollCount = 1;
			if (Control.ModifierKeys == Keys.Control)
				scrollCount = 3;
			
			//adjust the line supposed to be in view
			if (e.Delta < 0)
				ScrolledLine = Math.Min(FScrollBar.Maximum - FVisibleLines + FScrollBar.LargeChange - 3, ScrolledLine + scrollCount);
			else if (e.Delta > 0)
				ScrolledLine = Math.Max(0, ScrolledLine - scrollCount);
			
			if (ScrolledLine < 0)
				return;
			
			RedrawSelection();
		}
		#endregion TextBoxTags
		
		#region RichTextBox
		void RichTextBoxMouseDown(object sender, MouseEventArgs e)
		{
			string username = FRichTextBox.Lines[FHoverLine].Trim();
			FRichTextBox.SelectionStart = FRichTextBox.GetFirstCharIndexFromLine(FHoverLine)+1;
			FTagsTextBox.Focus();
			
			//as plugin in its own window
			if (AllowDragDrop)
			{
				/*
				var selNode = FNodeDict[username];
				string systemname = selNode.Systemname;
				
				FTagsTextBox.DoDragDrop(systemname, DragDropEffects.All);
				 */
				
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
					/*
					var selNode = FNodeDict[username];
					 */
					
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
				RedrawSelection();
			}
		}
		
		void RichTextBoxMouseUp(object sender, MouseEventArgs e)
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
			
			/*
			string key = FRichTextBox.Lines[FHoverLine].Trim();
			if (FNodeDict.ContainsKey(key))
			{
				INodeInfo ni = FNodeDict[key];
			 */
			
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
			
			/*
			}
			 */
		}
		
		/*
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
			                         		t = t.TrimStart(new char[1]{'.'});
			                         		if (node.Contains(t))
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
			                         	
			                         	if (((AndTags) && (containsAll)) || ((!AndTags) && (node.Contains(t))))
			                         		return true;
			                         	else
			                         		return false;
			                         });
		}
		 */
		
		private List<INodeInfo> ExtractSubList(List<INodeInfo> InputList)
		{
			return InputList.FindAll(delegate(INodeInfo nodeInfo)
			                         {
			                         	var displayName = FNodeDict[nodeInfo];
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
		
//		private List<string> GetLocalNodes()
//		{
//			var files = new List<string>();
//			if (System.IO.Path.IsPathRooted(FPathDir))
//			{
//				foreach (string p in System.IO.Directory.GetFiles(FPathDir, "*.v4p", SearchOption.TopDirectoryOnly))
//				{
//					//prevent patches from being created recursively
//					if (p != FPath)
//						files.Add(System.IO.Path.GetFileName(p));
//				}
//				foreach (string p in System.IO.Directory.GetFiles(FPathDir, "*.dll"))
//					files.Add(System.IO.Path.GetFileName(p));
//				foreach (string p in System.IO.Directory.GetFiles(FPathDir, "*.fx"))
//					files.Add(System.IO.Path.GetFileName(p));
//			}
//			return files;
//		}
		
		private void FilterNodesByTags()
		{
			string text = FTagsTextBox.Text.ToLower().Trim();
			
			FSelectionList.Clear();
			/*
			FSelectionList.AddRange(FNodeList);
			 */
			FSelectionList.AddRange(FNodeDict.Keys);
			
			//add local nodes
			/* should already be loaded by watcher.
			FSelectionList.AddRange(GetLocalNodes());
			 */
			
			/*
			//show natives only
			if (FNodeFilter == (int) NodeType.Native)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.Native;});
			//show patches only
			else if (FNodeFilter == (int) NodeType.Patch)
				FSelectionList = GetLocalNodes();
			//show modules only
			else if (FNodeFilter == (int) NodeType.Module)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.Module;});
			//show effects only
			else if (FNodeFilter == (int) NodeType.Effect)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.Effect;});
			//show freeframes only
			else if (FNodeFilter == (int) NodeType.Freeframe)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.Freeframe;});
			//show plugins only
			else if (FNodeFilter == (int) NodeType.Plugin)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.Plugin;});
			//show dynamics only
			else if (FNodeFilter == (int) NodeType.Dynamic)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.Dynamic;});
			//show vsts only
			else if (FNodeFilter == (int) NodeType.VST)
				FSelectionList = FNodeList.FindAll(delegate(string node){return FNodeDict[node].Type == NodeType.VST;});
			 */
			
			//show natives only
			if (FNodeFilter == (int) NodeType.Native)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Native;});
			//show patches only
			else if (FNodeFilter == (int) NodeType.Patch)
			{
				var currentDir = FNodeBrowserNode.CurrentDir;
				var currentNode = FNodeBrowserNode.CurrentPatchWindow.GetNode();
				var currentNodeInfo = currentNode.GetNodeInfo();
				
				FSelectionList = FSelectionList.FindAll(
					delegate(INodeInfo nodeInfo)
					{
						if (!string.IsNullOrEmpty(currentDir))
						{
							if (!string.IsNullOrEmpty(nodeInfo.Filename))
							{
								if (currentNodeInfo.Filename == nodeInfo.Filename)
									return false;
								
								var directory = System.IO.Path.GetDirectoryName(nodeInfo.Filename);
								return directory == currentDir;
							}
						}
						
						return false;
					});
			}
			//show modules only
			else if (FNodeFilter == (int) NodeType.Module)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Module;});
			//show effects only
			else if (FNodeFilter == (int) NodeType.Effect)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Effect;});
			//show freeframes only
			else if (FNodeFilter == (int) NodeType.Freeframe)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Freeframe;});
			//show plugins only
			else if (FNodeFilter == (int) NodeType.Plugin)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Plugin;});
			//show dynamics only
			else if (FNodeFilter == (int) NodeType.Dynamic)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.Dynamic;});
			//show vsts only
			else if (FNodeFilter == (int) NodeType.VST)
				FSelectionList = FSelectionList.FindAll(delegate(INodeInfo nodeInfo){return nodeInfo.Type == NodeType.VST;});

			FSelectionList = ExtractSubList(FSelectionList);
			
			/*
			FSelectionList.Sort(delegate(string s1, string s2)
			                    {
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
			                    			cat1 = System.IO.Path.GetExtension(name1);
			                    		else
			                    			cat1 = s1.Substring(s1.IndexOf('(')).Trim(new char[2]{'(', ')'});
			                    		
			                    		if (s2IsFile)
			                    			cat2 = System.IO.Path.GetExtension(name2);
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
			 */
			FSelectionList.Sort(delegate(INodeInfo n1, INodeInfo n2)
			                    {
			                    	var s1 = FNodeDict[n1];
			                    	var s2 = FNodeDict[n2];
			                    	
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
			                    			cat1 = System.IO.Path.GetExtension(name1);
			                    		else
			                    			cat1 = s1.Substring(s1.IndexOf('(')).Trim(new char[2]{'(', ')'});
			                    		
			                    		if (s2IsFile)
			                    			cat2 = System.IO.Path.GetExtension(name2);
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
			/*
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
			 */
			
			foreach (INodeInfo nodeInfo in FSelectionList)
			{
				//all comparison is case-in-sensitive
				var s = FNodeDict[nodeInfo];
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
			string text = FTagsTextBox.Text.Trim();
			FTags = text.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			
			FNodeFilter = -1;
			
			if (FTags.Length > 0)
			{
				if (FTags[0] == "N")
					FNodeFilter = (int) NodeType.Native;
				else if ((FTags[0].StartsWith(".")) || (FTags[0] == "V4") || (FTags[0] == "V4P"))
					FNodeFilter = (int) NodeType.Patch;
				else if (FTags[0] == "M")
					FNodeFilter = (int) NodeType.Module;
				else if ((FTags[0] == "F") || (FTags[0] == "FF"))
					FNodeFilter = (int) NodeType.Freeframe;
				else if ((FTags[0] == "X") || (FTags[0] == "FX"))
					FNodeFilter = (int) NodeType.Effect;
				else if (FTags[0] == "P")
					FNodeFilter = (int) NodeType.Plugin;
				else if (FTags[0] == "D")
					FNodeFilter = (int) NodeType.Dynamic;
				else if ((FTags[0] == "V") || (FTags[0] == "VS") || (FTags[0] == "VST"))
					FNodeFilter = (int) NodeType.VST;
			}
			
			if (FNodeFilter >= 0)
			{
				//remove first entry from FTags if it doesn't start with .
				if (!FTags[0].StartsWith("."))
				{
					string[] restTags = new string[Math.Max(0, FTags.Length-1)];
					for (int i = 1; i < FTags.Length; i++)
					{
						restTags[i - 1] = FTags[i];
					}
					FTags = restTags;
				}
			}
			
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
				/*string sel = FRichTextBox.Lines[FHoverLine];
				FRichTextBox.SelectionStart = FRichTextBox.Text.IndexOf(sel);
				FRichTextBox.SelectionLength = sel.Length;
				 */
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
				int index = i + ScrolledLine;
				int y = (i * CLineHeight) + 4;
				
				if (FHoverLine == i)
					using (SolidBrush b = new SolidBrush(CHoverColor))
						e.Graphics.FillRectangle(b, new Rectangle(0, y-4, 21, CLineHeight));
				
				/*
				if (FNodeDict.ContainsKey(FSelectionList[index].Trim()))
				{
					NodeType nodeType = FNodeDict[FSelectionList[index].Trim()].Type;
				 */
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
				/*
				else
					using (SolidBrush b = new SolidBrush(Color.Black))
						e.Graphics.DrawString("V", FRichTextBox.Font, b, 5, y-3, StringFormat.GenericDefault);
				 */
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
		}
	}
}