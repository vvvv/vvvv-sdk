/*
 * Created by SharpDevelop.
 * User: joreg
 * Date: 09.11.2009
 * Time: 13:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of SelectionControl.
	/// </summary>
	public partial class SelectionControl : UserControl
	{
		public event ButtonUpHandler OnXButton;
		public event ButtonUpHandler OnSelectionChanged;
		
		private bool FEditing = false;
		
		private string FSelectionName;
		public string SelectionName
		{
			get {return FSelectionName;}
			set
			{
				FSelectionName = value;
				UpdateSelectionLabel();
			}
		}
		
		private List<IPControl> FIPControls;
		public List<IPControl> IPControls
		{
			get {return FIPControls;}
			set {FIPControls = value;}
		}
		
		public string IPList
		{
			get
			{
				string list = "";
				foreach (IPControl ipc in FSelectedIPs)
					list += ipc.IP + ";";
				
				return list;
			}
			set
			{
				FSelectedIPs = new List<IPControl>();
				char s = ';';
				string[] ips = value.Split(s);
				IPControl ipc;
				
				for (int i=0; i<ips.Length; i++)
				{
					ipc = IPControls.Find(delegate(IPControl ip){return ip.IP == ips[i];});
					if (ipc != null)
						FSelectedIPs.Add(ipc);
				}
				
				UpdateSelectionLabel();
			}
		}
		
		private List<IPControl> FSelectedIPs;
		public List<IPControl> SelectedIPs
		{
			get {return FSelectedIPs;}
			set {FSelectedIPs = value;}
		}
		
		public SelectionControl()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			Height = 25;
		}
		
		void EditButtonClick(object sender, EventArgs e)
		{
			FEditing = !FEditing;
			
			if (FEditing)
			{
				EditButton.Text = "S";
				Height = 45;
				NameEdit.Text = FSelectionName;
			}
			else
			{
				EditButton.Text = "E";
				Height = 25;
				SelectionName = NameEdit.Text;
				OnSelectionChanged.Invoke(this);
			}
		}
		
		void DeleteButtonClick(object sender, EventArgs e)
		{
			OnXButton.Invoke(this);
		}
		
		void ShowSelectionButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in FIPControls)
				ipc.IsSelected = false;
			
			foreach(IPControl ipc in FSelectedIPs)
				ipc.IsSelected = true;
		}
		
		void TakeSelectionButtonClick(object sender, EventArgs e)
		{
			TakeSelection();
			OnSelectionChanged.Invoke(this);
		}
		
		public void TakeSelection()
		{
			FSelectedIPs = FIPControls.FindAll(delegate(IPControl ipc){return ipc.IsSelected;});
			UpdateSelectionLabel();
		}
		
		private void UpdateSelectionLabel()
		{
			if (FSelectedIPs == null)
				NameLabel.Text = FSelectionName + " (0)";
			else
				NameLabel.Text = FSelectionName + " (" + FSelectedIPs.Count.ToString() + ")";
		}
		
		public void RemoveIP(IPControl ipc)
		{
			FSelectedIPs.Remove(ipc);
		}
		
	}
}
