using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
    public delegate void CloneInfoEventHandler(INodeInfo nodeInfo, string Name, string Category, string Version);
    
    public partial class CloneInfo : UserControl
    {
        public Dictionary<string, INodeInfo> NodeDict {get; set;}
        public event CloneInfoEventHandler Closed;
        private INodeInfo FCloneInfo;
        
        public CloneInfo()
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
        
        public void Initialize(INodeInfo nodeInfo)
        {
            FCloneInfo = nodeInfo;
            FNameTextBox.Text = FCloneInfo.Name;
            FCategoryTextBox.Text = FCloneInfo.Category;
            FVersionTextBox.Text = FCloneInfo.Version;
            FNameTextBox.SelectAll();
            FNameTextBox.Focus();
        }
        
        private void CheckNodeName()
        {
            string systemName = FNameTextBox.Text.Trim() + " (";
            if (string.IsNullOrEmpty(FVersionTextBox.Text.Trim()))
                systemName += FCategoryTextBox.Text.Trim() + ")";
            else
                systemName += FCategoryTextBox.Text.Trim() + " " + FVersionTextBox.Text.Trim() + ")";
            
            if (NodeDict.ContainsKey(systemName))
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
    }
}
