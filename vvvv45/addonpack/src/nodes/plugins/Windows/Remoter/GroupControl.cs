using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace VVVV.Nodes
{
	public partial class GroupControl: UserControl
	{
		public event ButtonHandler OnXButton;
		public event ButtonUpHandler OnGroupSelectButton;
		public event GroupChangedHandler OnGroupChanged;
		private const string UNGROUPED = "ungrouped";
		
		private bool FEditing = false;
		
		private string FGroupName;
		public string GroupName
		{
			get{return FGroupName;}
		}
	
		private List<IPControl> FIPControls = new List<IPControl>();
		public List<IPControl> IPControls
		{
			get{return FIPControls;}
		}
		
		private bool FIsOnline = false;
		public bool IsOnline
		{
			get{return FIsOnline;}
			set
			{
				FIsOnline = value;
				if (FIsOnline)
					OnlinePanel.BackColor = Color.DarkGreen;
				else
					OnlinePanel.BackColor = Color.DarkRed;
			}
		}
		
		private bool FAppIsOnline = false;
		public bool AppIsOnline
		{
			get{return FAppIsOnline;}
			set
			{
				FAppIsOnline = value;
				if (FAppIsOnline)
					AppPanel.BackColor = Color.DarkGreen;
				else
					AppPanel.BackColor = Color.DarkRed;
			}
		}
		
		private bool FIsSelected = false;
		public bool IsSelected
		{
			get{return FIsSelected;}
			set
			{
				FIsSelected = value;
				if (FIsSelected)
					GroupLabel.Font = new Font(GroupLabel.Font, FontStyle.Bold);
				else
				{
					GroupLabel.Font = new Font(GroupLabel.Font, FontStyle.Regular);
					AppPanel.BackColor = Color.DarkRed;
				}
			}
		}

		public GroupControl(string GroupName)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FGroupName = GroupName;
			GroupLabel.Text = FGroupName;
			GroupNameEdit.Text = FGroupName;
			
			if (FGroupName == UNGROUPED)
			{
				XButton.Enabled = false;
				GroupNameEdit.Enabled = false;
				IPsEdit.Enabled = false;
			}
			
			Height = 32;
		}
		
		public void AddIP(IPControl IP)
		{
			if (!FIPControls.Contains(IP) && (IP != null))
				FIPControls.Add(IP);
		}
		
		void XButtonClick(object sender, EventArgs e)
		{
			OnXButton.Invoke(FGroupName);
		}
		
		void GroupLabelMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				IsSelected = true;
			if (e.Button == MouseButtons.Right)
				IsSelected = false;
		}
		
		void GroupLabelMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				IsSelected = true;
			if (e.Button == MouseButtons.Right)
				IsSelected = false;
		}
		
		void GroupLabelMouseUp(object sender, MouseEventArgs e)
		{
			OnGroupSelectButton.Invoke(this);
		}
		
		void EditButtonClick(object sender, EventArgs e)
		{
			if (FEditing)
			{
				this.Height = 32;
				EditButton.Text = "E";
				
				if (FGroupName != UNGROUPED)
				{
					FIPControls.Clear();
					List<string> ips = new List<string>();
					foreach(string s in IPsEdit.Lines)
						ips.Add(s);
					
					string oldname = FGroupName;
					GroupLabel.Text = GroupNameEdit.Text;
					FGroupName = GroupNameEdit.Text;
					
					OnGroupChanged.Invoke(this, oldname, ips);
				}
			}
			else
			{
				this.Height = 52 + (FIPControls.Count + 1) * 20;
				string[] ips = new string[FIPControls.Count];
				for (int i=0; i<ips.Length; i++)
					ips[i] = FIPControls[i].IP;
				IPsEdit.Lines = ips;
				EditButton.Text = "S";
			}
			
			FEditing = !FEditing;
		}
	}
}
