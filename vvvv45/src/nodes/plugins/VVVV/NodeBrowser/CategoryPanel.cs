using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of CategoryPanel.
	/// </summary>
	public partial class CategoryPanel : UserControl
	{
		public event PanelChangeHandler OnPanelChange;
		public event CreateNodeHandler OnCreateNode;
		public event CreateNodeHandler OnShowNodeReference;
		public event CreateNodeHandler OnShowHelpPatch;
		
		Dictionary<string, string> FCategoryDict = new Dictionary<string, string>();
        CategoryList FCategoryList = new CategoryList();  
        
        public bool NeedsUpdate {get; set;}
		
		public CategoryPanel()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FCategoryDict.Add("2d", "Geometry in 2d, like connecting lines, calculating coordinates etc.");
            FCategoryDict.Add("3d", "Geometry in 3d.");
            FCategoryDict.Add("4d", "");
            FCategoryDict.Add("Animation", "Things which will animate over time and therefore have an internal state; Generate motion, smooth and filter motion, record and store values. FlipFlops and other Logic nodes.");
            FCategoryDict.Add("Astronomy", "Everything having to do with the Earth and the Universe; Current Time, calculation of earth, moon and sun’s parameters.");
            FCategoryDict.Add("Boolean", "Logic Operators.");
            FCategoryDict.Add("Color", "Working with color, color addition, subtraction, blending, color models etc.");
            FCategoryDict.Add("Debug", "Displaying system status information in various undocumented formats.");
            FCategoryDict.Add("Devices", "Control external devices, and get data from them.");
            FCategoryDict.Add("Differential", "Create ultra smooth motions by working with position and velocity at the same time.");
            FCategoryDict.Add("DShow9", "Audio and Video playback and effects based on Microsofts DirectShow Framework.");
            FCategoryDict.Add("DX9", "DirectX9 based rendering system");
            FCategoryDict.Add("Enumerations", "Work with enumerated data types");
            FCategoryDict.Add("EX9", "The DirectX9 based rendering system made more Explicit. So geometry generation is separated from geometry display in the shader.");
            FCategoryDict.Add("File", "Operations on the file system. Read, write, copy, delete, parse files etc.");
            FCategoryDict.Add("Flash", "Everything related to rendering Flash content.");
            FCategoryDict.Add("GDI", "Old school simple rendering system. Simple nodes for didactical use and lowtek graphics.");
            FCategoryDict.Add("HTML", "Nodes making use of HTML strings local or on the internet");
            FCategoryDict.Add("Network", "Internet functionality like HTTP, IRC, UDP, TCP, ...");
            FCategoryDict.Add("Node", "Operations on the generic so called node pins.");
            FCategoryDict.Add("ODE", "The Open Dynamics Engine for physical behaviour.");
            FCategoryDict.Add("Quaternion", "Work with Quaternion vectors for rotations.");
            FCategoryDict.Add("Spectral", "Operations for reducing value spreads to some few values. Summing, Averaging etc.");
            FCategoryDict.Add("Spreads", "Operations creating value spreads out of few key values. Also spread operations.");
            FCategoryDict.Add("String", "String functions, appending, searching, sorting, string spread and spectral operations.");
            FCategoryDict.Add("System", "Control of built in hardware, like mouse, keyboard, sound card mixer, power management etc.");
            FCategoryDict.Add("Transforms", "Nodes for creating and manipulating 3d-transformations.");
            FCategoryDict.Add("TTY", "Old school tty console rendering system for printing out status and debug messages.");
            FCategoryDict.Add("Value", "Everything dealing with numercial values: Mathematical operations, ...");
            FCategoryDict.Add("VVVV", "Everything directly related to the running vvvv instance: Command line parameters, Event outputs, Quit command, ...");
            FCategoryDict.Add("Windows", "Control Windows´ Windows, Desktop Icons etc.");
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            mappingRegistry.RegisterDefaultMapping<IParent, DefaultParentProvider>();
            mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            
            FCategoryTreeViewer.Registry = mappingRegistry;
            FCategoryTreeViewer.Input = FCategoryList;
		}
		
		public void Redraw()
		{
		    FCategoryTreeViewer.Reload();
		}
		
		public void Add(INodeInfo nodeInfo)
		{
		    if (nodeInfo.Type == NodeType.Patch || nodeInfo.Type == NodeType.Text)
		        return;
		    
			//insert nodeInfo to FCategoryList
            bool added = false;
            foreach (CategoryEntry ce in FCategoryList)
                if (ce.Name == nodeInfo.Category)
            {
                ce.Add(nodeInfo);
                added = true;
                break;
            }
            
            //category not yet present. create a new one
            if (!added)
            {
                string description;
                if (FCategoryDict.ContainsKey(nodeInfo.Category))
                    description = FCategoryDict[nodeInfo.Category];
                else
                    description = "";
                
                var catEntry = new CategoryEntry(nodeInfo.Category, description);
                catEntry.Add(nodeInfo);
                FCategoryList.Add(catEntry);
            }
            
            NeedsUpdate = true;
		}
		
		public void Update(INodeInfo nodeInfo)
		{
			NeedsUpdate = true;
		}
		
		public void Remove(INodeInfo nodeInfo)
		{
			CategoryEntry catEntry = null;
            foreach (CategoryEntry ce in FCategoryList)
                if (ce.Name == nodeInfo.Category)
            {
                ce.Remove(nodeInfo);
                catEntry = ce;
                break;
            }
            
            if ((catEntry != null) && (catEntry.Count == 0))
                FCategoryList.Remove(catEntry);
            
            NeedsUpdate = true;
		}

        public void BeforeHide()
        {
            FCategoryTreeViewer.HideToolTip();
        }
        
        void FTopLabelClick(object sender, EventArgs e)
        {
            OnPanelChange(NodeBrowserPage.ByTags, null);
        }
		
        public void Update()
		{
			FCategoryTreeViewer.HideToolTip();
			FCategoryTreeViewer.Focus();
			if (NeedsUpdate)
			{
			    FCategoryTreeViewer.Reload();
			    NeedsUpdate = false;
			}
		}
        
		void CategoryPanelVisibleChanged(object sender, EventArgs e)
		{
			FCategoryTreeViewer.HideToolTip();
			FCategoryTreeViewer.Focus();
		}
		
		void FCategoryTreeViewerMouseDown(IModelMapper sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (sender.Model is NodeInfoEntry)
            {
                if (e.Button == MouseButtons.Left)
                    OnCreateNode((sender.Model as NodeInfoEntry).NodeInfo);
                else if (e.Button == MouseButtons.Middle)
                    OnShowNodeReference((sender.Model as NodeInfoEntry).NodeInfo);
                else
                    OnShowHelpPatch((sender.Model as NodeInfoEntry).NodeInfo);
            }
            else
            {
                if (FCategoryTreeViewer.IsExpanded(sender.Model))
                {
                    switch (e.Button)
                    {
                            case MouseButtons.Left: FCategoryTreeViewer.Collapse(sender.Model, false); break;
                            case MouseButtons.Right: FCategoryTreeViewer.Collapse(sender.Model, false); break;
                            case MouseButtons.Middle: FCategoryTreeViewer.Collapse(FCategoryTreeViewer.Input, true); break;
                    }
                }
                else
                {
                    switch (e.Button)
                    {
                            case MouseButtons.Left: FCategoryTreeViewer.Solo(sender.Model); break;
                            case MouseButtons.Right: FCategoryTreeViewer.Expand(sender.Model, false); break;
                            case MouseButtons.Middle: FCategoryTreeViewer.Expand(FCategoryTreeViewer.Input, true); break;
                    }
                }
            }
		}
	}
}
