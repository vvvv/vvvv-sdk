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
	public partial class IPControl: UserControl
	{
		[DllImport("iphlpapi.dll")]
		public static extern int SendARP(UInt32 DestIP, UInt32 SrcIP, [Out] byte[] pMacAddr, ref uint PhyAddrLen);
		private const int UPDATETIMEOUT = 5;
		private const string UNGROUPED = "ungrouped";
		private const string ALL = ".All";
		private const string ONLINE = ".Online";
		private const string OFFLINE = ".Offline";
		
		public event ButtonHandler OnVNCButton;
		public event ButtonHandler OnEXPButton;
		public event ButtonHandler OnXButton;
		public event ButtonUpHandler OnIPSelectButton;
		
		private DateTime FLastPingTime;
		private DateTime FLastWatchTime;
		private byte[] FMACBytes = new byte[6];
		private string FIP;
		public string IP
		{
			get{return FIP;}
		}
		
		private string FHostName;
		public string HostName
		{
			get{return FHostName;}
			set
			{
				FHostName = value;
				HostNameLabel.Text = FHostName;
			}
		}
		
		private string FMacAddress;
		public string MacAddress
		{
			get{return FMacAddress;}
			set
			{
				FMacAddress = value;
				if (! string.IsNullOrEmpty(FMacAddress))
				{
					char s = '-';
					string[] mac = FMacAddress.Split(s);
					for (int i=0; i<6; i++)
						FMACBytes[i] = Convert.ToByte(mac[i], 16);
				}
				MacIPLabel.Text = FIP + "  -  " + FMacAddress;
			}
		}
		
		private List<string> FGroups = new List<string>();
		public string Groups
		{
			get
			{
				string groups = "";
				for (int i=0; i<FGroups.Count; i++)
				{
					groups += FGroups[i];
					if (i < FGroups.Count-1)
						groups += "; ";
				}
				return groups;
			}
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
				Font tmpFont = new Font(HostNameLabel.Font, FontStyle.Bold);
				if (FIsSelected)
					HostNameLabel.Font = tmpFont;
				else
				{
					HostNameLabel.Font = new Font(HostNameLabel.Font, FontStyle.Regular);
					AppPanel.BackColor = Color.DarkRed;
				}
				MacIPLabel.Font = HostNameLabel.Font;
			}
		}
		
		public bool IsLocalHost
		{
			get{return IPAddress.IsLoopback(IPAddress.Parse(IP));}
		}
		
		private Process FProcess;
		
		private bool FDeleteMe;
		public bool DeleteMe
		{
			get{return FDeleteMe;}
		}
		
		public IPControl(string IP)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FIP = IP;
			MacIPLabel.Text = FIP;
		}
		
		public void AddGroups(string Groups)
		{
			if (Groups != "")
			{
				string[] groups;
				char s = ';';
				groups = Groups.Split(s);
				for (int i=0; i< groups.Length; i++)
					if (!FGroups.Contains(groups[i]))
					FGroups.Add(groups[i]);
			}
			
			if (FGroups.Count > 0)
				XButton.Enabled = false;
			
			FDeleteMe = false;
		}
		
		public void RemoveGroup(string Group)
		{
			if (FGroups.Contains(Group))
			{
				FGroups.Remove(Group);
				if (FGroups.Count == 0)
					FDeleteMe = true;
			}
		}
		
		public bool IsPartOfGroup(string Group)
		{
			if (Group == UNGROUPED)
				return FGroups.Count == 0;
			else if (Group == ALL)
				return true;
			else if (Group == ONLINE)
				return IsOnline;
			else if (Group == OFFLINE)
				return !IsOnline;
			else
				return FGroups.Contains(Group);
		}
		
		void VNCButtonClick(object sender, EventArgs e)
		{
			OnVNCButton.Invoke(FIP);
		}
		
		void EXPButtonClick(object sender, EventArgs e)
		{
			OnEXPButton.Invoke(FIP);
		}
		
		void XButtonClick(object sender, EventArgs e)
		{
			OnXButton.Invoke(FIP);
		}
		
		void IPLabelMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				IsSelected = true;
			if (e.Button == MouseButtons.Right)
				IsSelected = false;
			HostNameLabel.Capture = false;
			MacIPLabel.Capture = false;
		}
		
		void IPLabelMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				IsSelected = true;
			if (e.Button == MouseButtons.Right)
				IsSelected = false;
		}
		
		void IPLabelMouseUp(object sender, MouseEventArgs e)
		{
			OnIPSelectButton.Invoke(this);
		}
		
		public bool LocalProcessIsResponding(string ProcessName)
		{
			if (FProcess == null)
			{
				//find process and save a local reference
				Process[] processList;
				if (IPAddress.IsLoopback(IPAddress.Parse(IP)))
					processList = Process.GetProcesses();
				else
					processList = Process.GetProcesses(IP);
				
				foreach (Process p in processList)
				{
					if (p.ProcessName.ToLower() == ProcessName.ToLower())
					{
						FProcess = p;
						break;
					}
				}
			}
			
			if (FProcess == null)
			{
				AppIsOnline = false;
				return false;
			}
			else
			{
				FProcess.Refresh();
				//not available for remote processes
				try
				{
					AppIsOnline = FProcess.Responding;
					if (!AppIsOnline)
						FProcess.Kill();
				}
				catch
				{
					AppIsOnline = false;
					FProcess = null;
				}
				return AppIsOnline;
			}
		}
		
		public void WakeOnLan()
		{
			//code from: http://dotnet-snippets.de/dns/c-wake-on-lan-SID608.aspx
			//WOL packet is sent via broadcast address typically to ports 0, 7 or 9 (according to wikipedia)
			UdpClient client = new UdpClient();
			client.Connect(IPAddress.Broadcast, 0);

			//WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
			byte[] packet = new byte[17*6];

			//Trailer of 6 times 0xFF.
			for (int i = 0; i < 6; i++)
				packet[i] = 0xFF;

			//Body of magic packet contains 16 times the MAC address.
			for (int i = 1; i <= 16; i++)
				for (int j = 0; j < 6; j++)
				packet[i*6 + j] = FMACBytes[j];

			//Send WOL packet.
			client.Send(packet, packet.Length);
			client = null;
		}
		
		public bool NeedsWatchUpdate()
		{
			DateTime dt = DateTime.UtcNow;
			if (dt.Subtract(FLastWatchTime).Seconds < UPDATETIMEOUT)
				return false;
			else
			{
				FLastWatchTime = dt;
				return true;
			}
		}
		
		public void UpdateOnlineState()
		{
			DateTime dt = DateTime.UtcNow;
			if (dt.Subtract(FLastPingTime).Seconds < UPDATETIMEOUT)
				return;
			
			FLastPingTime = dt;
			
			Ping pingSender = new Ping();
			PingOptions options = new PingOptions();

			// Use the default Ttl value which is 128,
			// but change the fragmentation behavior.
			options.DontFragment = true;

			// Create a buffer of 32 bytes of data to be transmitted.
			string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
			byte[] buffer = Encoding.ASCII.GetBytes(data);
			int timeout = 120;
			PingReply reply = pingSender.Send(FIP, timeout, buffer, options);
			
			FIsOnline = reply.Status == IPStatus.Success;
			
			if (FIsOnline)
				OnlinePanel.BackColor = Color.DarkGreen;
			else
			{
				OnlinePanel.BackColor = Color.DarkRed;
				AppIsOnline = false;
			}
		}
		
		public void UpdateMACAddress()
		{
			if ((FIsSelected) && (FIsOnline))
			{
				//didn't get that code to work:
				//http://www.java2s.com/Code/CSharp/Network/GetMacAddress.htm
				
				//this is using wmi, but hardly works..
				//http://social.msdn.microsoft.com/forums/en-US/netfxnetcom/thread/2b125a0e-f67d-476f-b8a0-a21c99279d5b/
				
				//so now using SendARP via dllimport
				IPAddress addr = IPAddress.Parse(FIP);
				byte[] mac = new byte[6];
				uint length = (uint) mac.Length;
				
				//http://social.microsoft.com/Forums/en-US/vblanguage/thread/d4967e05-9914-49c4-9c9b-53fe8d52fee0/
				SendARP((UInt32)addr.Address, 0, mac, ref length);
				
				MacAddress = BitConverter.ToString(mac, 0, (int) length);
				
				//also read out hostname..takes quite some time
				HostName = System.Net.Dns.GetHostEntry(FIP).HostName;
			}
		}
	}
}
