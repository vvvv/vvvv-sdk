#region usings
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using VVVV.PluginInterfaces.V2;
using Ookii.Dialogs;
#endregion usings

namespace VVVV.Nodes.NodeBrowser
{
    public delegate void ClonePanelEventHandler(INodeInfo nodeInfo, string Name, string Category, string Version, string path);
    
    public partial class ClonePanel : UserControl
    {
        Dictionary<string, INodeInfo> FSystemNameDict = new Dictionary<string, INodeInfo>();
        
        public event ClonePanelEventHandler Closed;
        private INodeInfo FCloneInfo;
        
        public NodeBrowserPluginNode NodeBrowser {get; set;}
        
        public ClonePanel()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
        }
                
        void FNameTextBoxTextChanged(object sender, EventArgs e)
        {
        	CheckNodeName();
        }
        
        void FCategoryTextBoxTextChanged(object sender, EventArgs e)
        {
        	CheckNodeName();
        }
        
        void FVersionTextBoxTextChanged(object sender, EventArgs e)
        {
        	CheckNodeName();
        }
        
        public void Add(INodeInfo nodeInfo)
		{
			FSystemNameDict[nodeInfo.Systemname] = nodeInfo;
		}
		
        public void Update(INodeInfo nodeInfo)
        {
        /*	string oldSysKey = "";
        	foreach(var infokey in FSystemNameDict)
                if (infokey.Value == nodeInfo)
            {
                oldSysKey = infokey.Key;
                break;
            }
            
            FSystemNameDict.Remove(oldSysKey);
            FSystemNameDict.Add(ni.Systemname, ni);
        */}
        
		public void Remove(INodeInfo nodeInfo)
		{
            FSystemNameDict.Remove(nodeInfo.Systemname);
		}
        
        public void Initialize(INodeInfo nodeInfo, string path)
        {
            FCloneInfo = nodeInfo;
            FNameTextBox.Text = FCloneInfo.Name;
            FCategoryTextBox.Text = FCloneInfo.Category;
            FVersionTextBox.Text = FCloneInfo.Version;
            FPathTextBox.Text = path;
            
            if (nodeInfo.Type == NodeType.Effect)
            {
            	FCategoryTextBox.Enabled = false;
            	FVersionTextBox.Enabled = false;
            }
            else
            {
            	FCategoryTextBox.Enabled = true;
            	FVersionTextBox.Enabled = true;
            }
            
            FNameTextBox.SelectAll();
            FNameTextBox.Focus();
            
            CheckNodeName();
        }
        
        private void CheckNodeName()
        {
        	var name = FNameTextBox.Text.Trim();
        	var category = FCategoryTextBox.Text.Trim();
        	var version = FVersionTextBox.Text.Trim();
        	
        	if (string.IsNullOrEmpty(name) || name.Contains(" "))
        	{
        		FCloneButton.Enabled = false;
        		return;
        	}
        	
        	if (string.IsNullOrEmpty(category) || category.Contains(" "))
        	{
        		FCloneButton.Enabled = false;
        		return;
        	}
        	
        	if (!string.IsNullOrEmpty(version) && version.Contains(" "))
        	{
        		FCloneButton.Enabled = false;
        		return;
        	}
        	
        	if (!Path.IsPathRooted(FPathTextBox.Text))
        	{
        		FCloneButton.Enabled = false;
        		return;
        	}
        	    
            string systemName = name + " (";
            if (string.IsNullOrEmpty(version))
                systemName += category + ")";
            else
                systemName += category + " " + version + ")";
            
            var nodeinfos = NodeBrowser.NodeInfoFactory.NodeInfos.ToList();
            var nodeinfo = nodeinfos.Find(delegate(INodeInfo ni) {return ni.Systemname == systemName;});
            if (nodeinfo != null)
                FCloneButton.Enabled = false;
            else
                FCloneButton.Enabled = true;
        }
        
        void FCloneButtonClick(object sender, EventArgs e)
        {
        	Closed(FCloneInfo, FNameTextBox.Text.Trim(), FCategoryTextBox.Text.Trim(), FVersionTextBox.Text.Trim(), FPathTextBox.Text.Trim());
        }
        
        void FCancelButtonClick(object sender, EventArgs e)
        {
        	Closed(null, string.Empty, string.Empty, string.Empty, string.Empty);
        }
        
        void FCancelButtonKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
                Closed(null, string.Empty, string.Empty, string.Empty, string.Empty);
        }
        
        void FCloneButtonKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
        	    Closed(FCloneInfo, FNameTextBox.Text.Trim(), FCategoryTextBox.Text.Trim(), FVersionTextBox.Text.Trim(), FPathTextBox.Text.Trim());
        }
        
        void ClonePanelVisibleChanged(object sender, EventArgs e)
        {
            FNameTextBox.Focus();
        }
        
        void FPathButtonClick(object sender, EventArgs e)
        {
            using (var dialog = new VistaFolderBrowserDialog())
            {
                if (Directory.Exists(FPathTextBox.Text))
                    dialog.SelectedPath = FPathTextBox.Text;

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
        	        var path = dialog.SelectedPath;
        	        if (FCloneInfo.Type == NodeType.Effect)
        	        {
        		        if (!path.EndsWith("effects"))
        			        path = Path.Combine(path, "effects");
        	        }
                    else if (FCloneInfo.Type == NodeType.VL)
                    {
                        if (!path.EndsWith("vl"))
                            path = Path.Combine(path, "vl");
                    }
        	        else if (!path.EndsWith("plugins"))
        		        path = Path.Combine(path, "plugins");
        			
        	        FPathTextBox.Text = path;
        	        CheckNodeName();
                }
            }
        }
    }
}
