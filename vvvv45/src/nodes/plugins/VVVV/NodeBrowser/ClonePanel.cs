using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.NodeBrowser
{
    public delegate void ClonePanelEventHandler(INodeInfo nodeInfo, string Name, string Category, string Version);
    
    public partial class ClonePanel : UserControl
    {
    	public event PanelChangeHandler OnPanelChange;
    	
        Dictionary<string, INodeInfo> FSystemNameDict = new Dictionary<string, INodeInfo>();
        
        public event ClonePanelEventHandler Closed;
        private INodeInfo FCloneInfo;
        
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
        
        public void Initialize(INodeInfo nodeInfo)
        {
            FCloneInfo = nodeInfo;
            FNameTextBox.Text = FCloneInfo.Name;
            FCategoryTextBox.Text = FCloneInfo.Category;
            FVersionTextBox.Text = FCloneInfo.Version;
            
            if (nodeInfo.Type == NodeType.Effect)
            {
            	FCategoryTextBox.Enabled = false;
            	FVersionTextBox.Enabled = false;
            }
            
            FNameTextBox.SelectAll();
            FNameTextBox.Focus();
            
            CheckNodeName();
        }
        
        private void CheckNodeName()
        {
            string systemName = FNameTextBox.Text.Trim() + " (";
            if (string.IsNullOrEmpty(FVersionTextBox.Text.Trim()))
                systemName += FCategoryTextBox.Text.Trim() + ")";
            else
                systemName += FCategoryTextBox.Text.Trim() + " " + FVersionTextBox.Text.Trim() + ")";
            
            if (FSystemNameDict.ContainsKey(systemName))
                FCloneButton.Enabled = false;
            else
                FCloneButton.Enabled = true;
        }
        
        void FCloneButtonClick(object sender, EventArgs e)
        {
            Closed(FCloneInfo, FNameTextBox.Text.Trim(), FCategoryTextBox.Text.Trim(), FVersionTextBox.Text.Trim());
        }
        
        void FCancelButtonClick(object sender, EventArgs e)
        {
        	Closed(null, "", "", "");
        }
        
        void FCancelButtonKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
                Closed(null, "", "", "");
        }
        
        void FCloneButtonKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
        	    Closed(FCloneInfo, FNameTextBox.Text.Trim(), FCategoryTextBox.Text.Trim(), FVersionTextBox.Text.Trim());
        }
        
        void ClonePanelVisibleChanged(object sender, EventArgs e)
        {
            FNameTextBox.Focus();
        }
    }
}
