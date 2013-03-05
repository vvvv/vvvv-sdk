using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of CategoryFilterPanel.
	/// </summary>
	public partial class CategoryFilterPanel : UserControl
	{
		private Dictionary<string, CheckBox> FCategories = new Dictionary<string, CheckBox>();
		private List<string> FVisibleCategories = new List<string>();
		
		public CategoryFilterPanel()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		public void SetCategories(List<string> categories)
		{
			this.SuspendLayout();

			CheckboxPanel.Controls.Clear();
			FCategories.Clear();
			
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
			
			this.ResumeLayout();
		}
		
		void CheckBox_Click(object sender, EventArgs e)
		{
			SaveFilter();
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
	        	if (CategoryVisible(category))
	        	    sb.AppendLine(category);
	
	        using (var outfile = new StreamWriter(savePath + @"\.vvvv"))
	        {
	            outfile.Write(sb.ToString());
	        }
		}
		
		private void LoadFilter()
		{
			FVisibleCategories.Clear();
			
			var savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			
			try
	        {
	            using (StreamReader sr = new StreamReader(savePath + @"\.vvvv"))
	            {
	            	while (sr.Peek() >= 0) 
	                    FVisibleCategories.Add(sr.ReadLine());
	            }
	        }
	        catch (Exception e)
	        {
	            
	        }
	        
	        foreach (var cb in FCategories)
	        	cb.Value.Checked = FVisibleCategories.Contains(cb.Key);
		}		
	}
}
