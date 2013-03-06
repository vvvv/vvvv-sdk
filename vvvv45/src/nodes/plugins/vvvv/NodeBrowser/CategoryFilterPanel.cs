using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of CategoryFilterPanel.
	/// </summary>
	public partial class CategoryFilterPanel : UserControl
	{
		private Dictionary<string, CheckBox> FCategories = new Dictionary<string, CheckBox>();
		private List<string> FHiddenCategories = new List<string>();
		
		public Action OnFilterChanged;
		
		internal bool PendingRedraw
        {
            get;
            set;
        }
		
		public CategoryFilterPanel()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		public NodeBrowserPluginNode NodeBrowser
        {
            get;
            set;
        }

		public void Update()
		{
			this.SuspendLayout();
			CheckboxPanel.SuspendLayout();

			CheckboxPanel.Controls.Clear();
			FCategories.Clear();
			
			//get a list of all current categories
			var categories = new List<string>();
			var nodeInfos = NodeBrowser.NodeInfoFactory.NodeInfos.Where(ni => ni.Ignore == false && ni.Type != NodeType.Patch && ni.Type != NodeType.Text);
            foreach (var nodeInfo in nodeInfos)
            {
            	if (!categories.Contains(nodeInfo.Category))
            		categories.Add(nodeInfo.Category);
            }
            
            categories.Sort();
			
            //for each category make a checkbox
			foreach (var category in categories)
			{
				var cb = new CheckBox();
				cb.Text = category;
				cb.Click += CheckBox_Click;
				cb.Dock = DockStyle.Top;
				cb.Checked = true;
				CheckboxPanel.Controls.Add(cb);
				
				cb.BringToFront();
				
				FCategories.Add(category, cb);
			}
			
			LoadFilter();
			
			CheckboxPanel.ResumeLayout();
			this.ResumeLayout();
		}
		
		void CheckBox_Click(object sender, EventArgs e)
		{
			SaveFilter();
			
			if (OnFilterChanged != null)
				OnFilterChanged();
		}
		
		public bool CategoryVisible(string category)
		{
			if (FCategories.ContainsKey(category))
				return FCategories[category].Checked;
			else
				return true;
		}
		
		private void SaveFilter()
		{
			var savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        	var sb = new StringBuilder();

	        foreach (var category in FCategories.Keys)
	        	if (!CategoryVisible(category))
	        	    sb.AppendLine(category);
	
	        using (var outfile = new StreamWriter(savePath + @"\.vvvv"))
	        {
	            outfile.Write(sb.ToString());
	        }
		}
		
		private void LoadFilter()
		{
			FHiddenCategories.Clear();
			
			var savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			
			try
	        {
	            using (StreamReader sr = new StreamReader(savePath + @"\.vvvv"))
	            {
	            	while (sr.Peek() >= 0) 
	                    FHiddenCategories.Add(sr.ReadLine());
	            }
	        }
	        catch (Exception e)
	        {
	            
	        }
	        
	        foreach (var cb in FCategories)
	        	cb.Value.Checked = !FHiddenCategories.Contains(cb.Key);
		}		
		
		void CategoryFilterPanelVisibleChanged(object sender, EventArgs e)
		{
            CheckboxPanel.Focus();
		}
	}
}
