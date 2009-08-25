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
		
		private bool FEditing = false;
		
		private string FGroupName;
		public string GroupName
		{
			get{return FGroupName;}
		}
		
		private List<string> FIPs = new List<string>();
		public List<string> IPs
		{
			get{return FIPs;}
		}
		
		private bool FIsOnline = false;
		public bool IsOnline
		{
			get{return FIsOnline;}
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
			
			if (FGroupName == "ungrouped")
			{
				XButton.Enabled = false;
				GroupNameEdit.Enabled = false;
				IPsEdit.Enabled = false;
			}
		}
		
		public void AddIP(string IP)
		{
			if (!FIPs.Contains(IP))
				FIPs.Add(IP);
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
				this.Height = 30;
				EditButton.Text = "E";
				
				FIPs.Clear();
				foreach(string s in IPsEdit.Lines)
					FIPs.Add(s);
				
				string oldname = FGroupName;
				GroupLabel.Text = GroupNameEdit.Text;
				FGroupName = GroupNameEdit.Text;
				
				OnGroupChanged.Invoke(this, oldname);
			}
			else
			{
				this.Height = 50 + (FIPs.Count + 1) * 20;
				IPsEdit.Lines = FIPs.ToArray();
				EditButton.Text = "S";
			}
			FEditing = !FEditing;
		}
	}
}
