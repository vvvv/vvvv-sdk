using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
    /// <summary>
    /// Description of CategoryControl.
    /// </summary>
    public partial class CategoryControl : UserControl
    {
        private bool FCollapsed = true;
        
        public string Category
        {
            get;
            private set;
        }
        
        public int UncollapsedHeight
        {
            get;
            private set;
        }
        
        public int NodeCount
        {
            get {return NodeListBox.Items.Count;}
        }
        
        public CategoryControl(string category)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            Category = category;
            CategoryLabel.Text = category;
            Height = CategoryLabel.Height;
        }
        
        public void Add(INodeInfo nodeInfo)
        {
            NodeListBox.Items.Add(nodeInfo.Username);
            UpdateNodeListBoxHeight();
        }
        
        public void Remove(INodeInfo nodeInfo)
        {
            NodeListBox.Items.Remove(nodeInfo.Username);
            UpdateNodeListBoxHeight();
        }
        
        private void UpdateNodeListBoxHeight()
        {
            UncollapsedHeight = CategoryLabel.Height + NodeListBox.Items.Count * NodeListBox.ItemHeight + 5;
            
            if (FCollapsed)
                Height = CategoryLabel.Height;
            else
                Height = UncollapsedHeight;
        }
        
        void CategoryLabelClick(object sender, EventArgs e)
        {
        	FCollapsed = !FCollapsed;
        	UpdateNodeListBoxHeight();            
        }
    }
}
