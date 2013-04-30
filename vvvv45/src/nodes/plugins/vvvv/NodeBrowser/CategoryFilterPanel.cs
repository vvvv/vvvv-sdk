using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;

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
				cb.MouseDown += CheckBox_MouseDown;
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
		
		void CheckBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			var box = sender as CheckBox;
			box.Checked = !box.Checked;

			if (e.Button == MouseButtons.Left)
			{
				//un/check all other categories as well that start with this + Dot, like EX9. or DX11.
				foreach (var categoryName in FCategories.Keys)
					if (categoryName.StartsWith(box.Text + "."))
						FCategories[categoryName].Checked = box.Checked;  
			}
			
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
			var xmlDoc = new XmlDocument();
			var settings = xmlDoc.CreateElement("SETTINGS");
			var nodeCategories = xmlDoc.CreateElement("NODECATEGORIES");
			xmlDoc.AppendChild(settings);
			settings.AppendChild(nodeCategories);
			
			int hiddenCount = 0;
	        foreach (var categoryName in FCategories.Keys)
	        	if (!CategoryVisible(categoryName))
	        {
	        	var category = xmlDoc.CreateElement("CATEGORY");
	        	var attr = xmlDoc.CreateAttribute("name");
	        	attr.Value = categoryName;
	        	category.Attributes.Append(attr);
	        	attr = xmlDoc.CreateAttribute("visible");
	        	attr.Value = "false";
	        	category.Attributes.Append(attr);
	        	
	        	nodeCategories.AppendChild(category);	  
				hiddenCount++;
	        }
	
	        using (var saveFile = new StreamWriter(savePath + @"\.vvvv"))
	        {
	            saveFile.Write(xmlDoc.OuterXml);
	        }
	        
	        FHiddenCategoryCountLabel.Text = "Hidden Categories: " + hiddenCount.ToString();
		}
		
		private void LoadFilter()
		{
			FHiddenCategories.Clear();
			
			var loadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			
			try
	        {
				var xmlDoc = new XmlDocument();
	            xmlDoc.Load(loadPath + @"\.vvvv");
	            
	            var hiddenCategories = xmlDoc.SelectNodes("/SETTINGS/NODECATEGORIES/CATEGORY");
	            foreach (XmlElement cat in hiddenCategories)
	            	FHiddenCategories.Add(cat.GetAttributeNode("name").Value);
	        }
	        catch (Exception e)
	        {
	            
	        }
	        
	        foreach (var cb in FCategories)
	        	cb.Value.Checked = !FHiddenCategories.Contains(cb.Key);
	        
	        FHiddenCategoryCountLabel.Text = "Hidden Categories: " + FHiddenCategories.Count.ToString();
		}		
		
		void CategoryFilterPanelVisibleChanged(object sender, EventArgs e)
		{
            CheckboxPanel.Focus();
		}
	}
}
