#region licence/info

//////project name
//Remoter

//////description
//a gui to remote PCs

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using System.ComponentModel;


using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

/*todos
 * do things in threads
 * consistent log-messages
 * expect ping (for watchdog)
 * remoting of RemoterSA
 * log to disk
 * email notification of failures
 * allow processes to be started with processaffinity
 * show local ip(s)
 * show computer names
 * monitoring (cpu, memory, running time...)
 * mediensteuerungs simulation: connects to port/ip udp or tcp and can send strings
 */

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition, inheriting from UserControl for the GUI stuff
	public class Remoter: UserControl, IPlugin
	{
		public enum TPsToolCommand {Execute, Kill, Watch, WatchExecute, Reboot, Shutdown};
		public enum TWatchMode {Off, Restart, Reboot};
		public enum TIPCLiveUpdate {Off, AllOnline, AllOffline};
		private const string UNGROUPED = "ungrouped";
		
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//input pin declaration
		private IValueIn FMyValueInput;
		private IStringConfig FSettingsInput;
		private IStringConfig FIPListInput;
		
		//output pin declaration
		private IValueOut FOnlineOutput;
		private IValueOut FAppIsOnlineOutput;
		
		//further fields
		private string FPsToolsPath;
		private string FVNCPath;
		private string FMirrorPath;
		private int FOnlineCheckID = 0;
		private int FWatchCheckID = 0;
		private bool FLoading = true;
		private int FWatchProcessID = 0;
		private string FWatchProcessName;
		private List<IPControl> FIPSortList = new List<IPControl>();
		private VVVV.Nodes.Remoter.TIPCLiveUpdate FIPCLiveUpdate;
		
		private System.Net.Sockets.TcpClient FTCPClient;
		private System.Net.Sockets.NetworkStream FTCPStream;
		
		private VVVV.Nodes.Remoter.TWatchMode FWatchMode = TWatchMode.Off;
		
		private XmlDocument FSettings;
		
		#endregion field declaration
		
		#region constructor/destructor
		public Remoter()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			FSettings = new XmlDocument();
			
			GroupFilterDropDown.SelectedIndex = 0;
			
			OnlineWorker.RunWorkerAsync();
			WatchWorker.RunWorkerAsync();
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FSettings = null;
					//FTCPClient.Close();
					//FTCPClient = null;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Remoter";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Windows";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "A GUI to remote PCs";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(200, 100);
					//define the nodes initial size in window-mode
					FPluginInfo.InitialWindowSize = new Size(400, 300);
					//define the nodes initial component mode
					FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;
			}
		}
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return true;}
		}
		
		#endregion node name and infos
		
		#region GUI
		private void InitializeComponent()
		{
			this.FFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.panel1 = new System.Windows.Forms.Panel();
			this.Simulator = new System.Windows.Forms.TabControl();
			this.CommandsPage = new System.Windows.Forms.TabPage();
			this.WatchProcessPathComboBox = new System.Windows.Forms.ComboBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.WatchModeReboot = new System.Windows.Forms.RadioButton();
			this.WatchModeRestart = new System.Windows.Forms.RadioButton();
			this.WatchModeOff = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.WatchProcessCheckbox = new System.Windows.Forms.CheckBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel10 = new System.Windows.Forms.Panel();
			this.MirrorButton = new System.Windows.Forms.Button();
			this.panel12 = new System.Windows.Forms.Panel();
			this.MirrorTestCheckBox = new System.Windows.Forms.CheckBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this.ShutdownButton = new System.Windows.Forms.Button();
			this.RebootButton = new System.Windows.Forms.Button();
			this.WakeOnLanButton = new System.Windows.Forms.Button();
			this.MACAddressButton = new System.Windows.Forms.Button();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.panel5 = new System.Windows.Forms.Panel();
			this.RemoteProcessPathComboBox = new System.Windows.Forms.ComboBox();
			this.KillButton = new System.Windows.Forms.Button();
			this.RestartButton = new System.Windows.Forms.Button();
			this.StartButton = new System.Windows.Forms.Button();
			this.SettingsPage = new System.Windows.Forms.TabPage();
			this.RemotingBox = new System.Windows.Forms.GroupBox();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.MirrorBox = new System.Windows.Forms.GroupBox();
			this.IgnorePattern = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.TargetPath = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.SourcePath = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.MirrorPathPanel = new System.Windows.Forms.Panel();
			this.MirrorPathLabel = new System.Windows.Forms.Label();
			this.MirrorPathButton = new System.Windows.Forms.Button();
			this.VNCBox = new System.Windows.Forms.GroupBox();
			this.VNCPathPanel = new System.Windows.Forms.Panel();
			this.VNCPathLabel = new System.Windows.Forms.Label();
			this.VNCPathButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.VNCPassword = new System.Windows.Forms.TextBox();
			this.PsToolsBox = new System.Windows.Forms.GroupBox();
			this.AddProcessButton = new System.Windows.Forms.Button();
			this.PsToolsProcessPanel = new System.Windows.Forms.Panel();
			this.PsToolsPathPanel = new System.Windows.Forms.Panel();
			this.PsToolsPathLabel = new System.Windows.Forms.Label();
			this.PsToolsPathButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.PsToolsUsername = new System.Windows.Forms.TextBox();
			this.PsToolsPassword = new System.Windows.Forms.TextBox();
			this.SimulatorPage = new System.Windows.Forms.TabPage();
			this.SimulatorListBox = new System.Windows.Forms.ListBox();
			this.panel7 = new System.Windows.Forms.Panel();
			this.SimulatorStringEdit = new System.Windows.Forms.TextBox();
			this.AddSimulatorStringButton = new System.Windows.Forms.Button();
			this.panel6 = new System.Windows.Forms.Panel();
			this.SimulatorIPEdit = new System.Windows.Forms.TextBox();
			this.SimulatorPortUpDown = new System.Windows.Forms.NumericUpDown();
			this.SimulatorConnectButton = new System.Windows.Forms.Button();
			this.SimulatorTCPCheckBox = new System.Windows.Forms.RadioButton();
			this.SimulatorUDPCheckBox = new System.Windows.Forms.RadioButton();
			this.LeftPanel = new System.Windows.Forms.Panel();
			this.LeftTabControl = new System.Windows.Forms.TabControl();
			this.IPPage = new System.Windows.Forms.TabPage();
			this.IPListPanel = new System.Windows.Forms.Panel();
			this.GroupFilterDropDown = new System.Windows.Forms.ComboBox();
			this.LeftBottomPanel = new System.Windows.Forms.Panel();
			this.InvertSelectionButton = new System.Windows.Forms.Button();
			this.SelectAllButton = new System.Windows.Forms.Button();
			this.RemoveAllButton = new System.Windows.Forms.Button();
			this.LeftTopPanel = new System.Windows.Forms.Panel();
			this.NewIPEdit = new System.Windows.Forms.TextBox();
			this.AddIPButton = new System.Windows.Forms.Button();
			this.VNCButton = new System.Windows.Forms.Button();
			this.GroupPage = new System.Windows.Forms.TabPage();
			this.GroupListPanel = new System.Windows.Forms.Panel();
			this.panel9 = new System.Windows.Forms.Panel();
			this.NewGroupEdit = new System.Windows.Forms.TextBox();
			this.AddGroupButton = new System.Windows.Forms.Button();
			this.panel8 = new System.Windows.Forms.Panel();
			this.InvertGroupSelectionButton = new System.Windows.Forms.Button();
			this.SelectAllGroupsButton = new System.Windows.Forms.Button();
			this.RemoveAllGroupsButton = new System.Windows.Forms.Button();
			this.OnlineWorker = new System.ComponentModel.BackgroundWorker();
			this.WatchWorker = new System.ComponentModel.BackgroundWorker();
			this.panel1.SuspendLayout();
			this.Simulator.SuspendLayout();
			this.CommandsPage.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel10.SuspendLayout();
			this.SettingsPage.SuspendLayout();
			this.RemotingBox.SuspendLayout();
			this.MirrorBox.SuspendLayout();
			this.MirrorPathPanel.SuspendLayout();
			this.VNCBox.SuspendLayout();
			this.VNCPathPanel.SuspendLayout();
			this.PsToolsBox.SuspendLayout();
			this.PsToolsPathPanel.SuspendLayout();
			this.SimulatorPage.SuspendLayout();
			this.panel7.SuspendLayout();
			this.panel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimulatorPortUpDown)).BeginInit();
			this.LeftPanel.SuspendLayout();
			this.LeftTabControl.SuspendLayout();
			this.IPPage.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.LeftTopPanel.SuspendLayout();
			this.GroupPage.SuspendLayout();
			this.panel9.SuspendLayout();
			this.panel8.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.panel1.Controls.Add(this.Simulator);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(245, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(347, 378);
			this.panel1.TabIndex = 20;
			// 
			// Simulator
			// 
			this.Simulator.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.Simulator.Controls.Add(this.CommandsPage);
			this.Simulator.Controls.Add(this.SettingsPage);
			this.Simulator.Controls.Add(this.SimulatorPage);
			this.Simulator.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Simulator.Location = new System.Drawing.Point(0, 0);
			this.Simulator.Name = "Simulator";
			this.Simulator.SelectedIndex = 0;
			this.Simulator.Size = new System.Drawing.Size(347, 378);
			this.Simulator.TabIndex = 0;
			// 
			// CommandsPage
			// 
			this.CommandsPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.CommandsPage.Controls.Add(this.WatchProcessPathComboBox);
			this.CommandsPage.Controls.Add(this.panel2);
			this.CommandsPage.Controls.Add(this.panel3);
			this.CommandsPage.Controls.Add(this.panel10);
			this.CommandsPage.Controls.Add(this.panel4);
			this.CommandsPage.Controls.Add(this.ShutdownButton);
			this.CommandsPage.Controls.Add(this.RebootButton);
			this.CommandsPage.Controls.Add(this.WakeOnLanButton);
			this.CommandsPage.Controls.Add(this.MACAddressButton);
			this.CommandsPage.Controls.Add(this.checkBox1);
			this.CommandsPage.Controls.Add(this.panel5);
			this.CommandsPage.Controls.Add(this.RemoteProcessPathComboBox);
			this.CommandsPage.Controls.Add(this.KillButton);
			this.CommandsPage.Controls.Add(this.RestartButton);
			this.CommandsPage.Controls.Add(this.StartButton);
			this.CommandsPage.Location = new System.Drawing.Point(4, 25);
			this.CommandsPage.Name = "CommandsPage";
			this.CommandsPage.Padding = new System.Windows.Forms.Padding(3);
			this.CommandsPage.Size = new System.Drawing.Size(339, 349);
			this.CommandsPage.TabIndex = 1;
			this.CommandsPage.Text = "Commands";
			// 
			// WatchProcessPathComboBox
			// 
			this.WatchProcessPathComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.WatchProcessPathComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.WatchProcessPathComboBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.WatchProcessPathComboBox.Location = new System.Drawing.Point(3, 281);
			this.WatchProcessPathComboBox.Name = "WatchProcessPathComboBox";
			this.WatchProcessPathComboBox.Size = new System.Drawing.Size(333, 21);
			this.WatchProcessPathComboBox.TabIndex = 33;
			this.WatchProcessPathComboBox.SelectedIndexChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.WatchModeReboot);
			this.panel2.Controls.Add(this.WatchModeRestart);
			this.panel2.Controls.Add(this.WatchModeOff);
			this.panel2.Controls.Add(this.label5);
			this.panel2.Controls.Add(this.WatchProcessCheckbox);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(3, 256);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(333, 25);
			this.panel2.TabIndex = 46;
			// 
			// WatchModeReboot
			// 
			this.WatchModeReboot.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchModeReboot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.WatchModeReboot.Location = new System.Drawing.Point(245, 0);
			this.WatchModeReboot.Name = "WatchModeReboot";
			this.WatchModeReboot.Size = new System.Drawing.Size(80, 25);
			this.WatchModeReboot.TabIndex = 32;
			this.WatchModeReboot.TabStop = true;
			this.WatchModeReboot.Text = "reboot PC";
			this.WatchModeReboot.UseVisualStyleBackColor = true;
			this.WatchModeReboot.Click += new System.EventHandler(this.WatchModeClick);
			// 
			// WatchModeRestart
			// 
			this.WatchModeRestart.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchModeRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.WatchModeRestart.Location = new System.Drawing.Point(187, 0);
			this.WatchModeRestart.Name = "WatchModeRestart";
			this.WatchModeRestart.Size = new System.Drawing.Size(58, 25);
			this.WatchModeRestart.TabIndex = 31;
			this.WatchModeRestart.TabStop = true;
			this.WatchModeRestart.Text = "restart";
			this.WatchModeRestart.UseVisualStyleBackColor = true;
			this.WatchModeRestart.Click += new System.EventHandler(this.WatchModeClick);
			// 
			// WatchModeOff
			// 
			this.WatchModeOff.Checked = true;
			this.WatchModeOff.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchModeOff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.WatchModeOff.Location = new System.Drawing.Point(124, 0);
			this.WatchModeOff.Name = "WatchModeOff";
			this.WatchModeOff.Size = new System.Drawing.Size(63, 25);
			this.WatchModeOff.TabIndex = 30;
			this.WatchModeOff.TabStop = true;
			this.WatchModeOff.Text = "nothing";
			this.WatchModeOff.UseVisualStyleBackColor = true;
			this.WatchModeOff.Click += new System.EventHandler(this.WatchModeClick);
			// 
			// label5
			// 
			this.label5.Dock = System.Windows.Forms.DockStyle.Left;
			this.label5.Location = new System.Drawing.Point(19, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(105, 25);
			this.label5.TabIndex = 41;
			this.label5.Text = "on lost process do:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// WatchProcessCheckbox
			// 
			this.WatchProcessCheckbox.Dock = System.Windows.Forms.DockStyle.Left;
			this.WatchProcessCheckbox.Location = new System.Drawing.Point(0, 0);
			this.WatchProcessCheckbox.Name = "WatchProcessCheckbox";
			this.WatchProcessCheckbox.Size = new System.Drawing.Size(19, 25);
			this.WatchProcessCheckbox.TabIndex = 42;
			this.WatchProcessCheckbox.Text = "checkBox3";
			this.WatchProcessCheckbox.UseVisualStyleBackColor = true;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.Gray;
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(3, 248);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(333, 8);
			this.panel3.TabIndex = 47;
			// 
			// panel10
			// 
			this.panel10.Controls.Add(this.MirrorButton);
			this.panel10.Controls.Add(this.panel12);
			this.panel10.Controls.Add(this.MirrorTestCheckBox);
			this.panel10.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel10.Location = new System.Drawing.Point(3, 224);
			this.panel10.Name = "panel10";
			this.panel10.Size = new System.Drawing.Size(333, 24);
			this.panel10.TabIndex = 49;
			// 
			// MirrorButton
			// 
			this.MirrorButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MirrorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MirrorButton.Location = new System.Drawing.Point(0, 0);
			this.MirrorButton.Name = "MirrorButton";
			this.MirrorButton.Size = new System.Drawing.Size(247, 24);
			this.MirrorButton.TabIndex = 24;
			this.MirrorButton.Text = "Mirror Now";
			this.MirrorButton.UseVisualStyleBackColor = true;
			this.MirrorButton.Click += new System.EventHandler(this.MirrorButtonClick);
			// 
			// panel12
			// 
			this.panel12.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel12.Location = new System.Drawing.Point(247, 0);
			this.panel12.Name = "panel12";
			this.panel12.Size = new System.Drawing.Size(11, 24);
			this.panel12.TabIndex = 27;
			// 
			// MirrorTestCheckBox
			// 
			this.MirrorTestCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.MirrorTestCheckBox.Location = new System.Drawing.Point(258, 0);
			this.MirrorTestCheckBox.Name = "MirrorTestCheckBox";
			this.MirrorTestCheckBox.Size = new System.Drawing.Size(75, 24);
			this.MirrorTestCheckBox.TabIndex = 25;
			this.MirrorTestCheckBox.Text = "Test Only";
			this.MirrorTestCheckBox.UseVisualStyleBackColor = true;
			this.MirrorTestCheckBox.Click += new System.EventHandler(this.MirrorTestCheckBoxClick);
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.Gray;
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(3, 216);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(333, 8);
			this.panel4.TabIndex = 48;
			// 
			// ShutdownButton
			// 
			this.ShutdownButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShutdownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ShutdownButton.Location = new System.Drawing.Point(3, 189);
			this.ShutdownButton.Name = "ShutdownButton";
			this.ShutdownButton.Size = new System.Drawing.Size(333, 27);
			this.ShutdownButton.TabIndex = 23;
			this.ShutdownButton.Text = "Shutdown";
			this.ShutdownButton.UseVisualStyleBackColor = true;
			this.ShutdownButton.Click += new System.EventHandler(this.ShutdownButtonClick);
			// 
			// RebootButton
			// 
			this.RebootButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.RebootButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RebootButton.Location = new System.Drawing.Point(3, 162);
			this.RebootButton.Name = "RebootButton";
			this.RebootButton.Size = new System.Drawing.Size(333, 27);
			this.RebootButton.TabIndex = 22;
			this.RebootButton.Text = "Reboot";
			this.RebootButton.UseVisualStyleBackColor = true;
			this.RebootButton.Click += new System.EventHandler(this.RebootButtonClick);
			// 
			// WakeOnLanButton
			// 
			this.WakeOnLanButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.WakeOnLanButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.WakeOnLanButton.Location = new System.Drawing.Point(3, 135);
			this.WakeOnLanButton.Name = "WakeOnLanButton";
			this.WakeOnLanButton.Size = new System.Drawing.Size(333, 27);
			this.WakeOnLanButton.TabIndex = 21;
			this.WakeOnLanButton.Text = "WakeOnLan";
			this.WakeOnLanButton.UseVisualStyleBackColor = true;
			this.WakeOnLanButton.Click += new System.EventHandler(this.WakeOnLanButtonClick);
			// 
			// MACAddressButton
			// 
			this.MACAddressButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.MACAddressButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MACAddressButton.Location = new System.Drawing.Point(3, 113);
			this.MACAddressButton.Name = "MACAddressButton";
			this.MACAddressButton.Size = new System.Drawing.Size(333, 22);
			this.MACAddressButton.TabIndex = 20;
			this.MACAddressButton.Text = "Get a MAC";
			this.MACAddressButton.UseVisualStyleBackColor = true;
			this.MACAddressButton.Click += new System.EventHandler(this.MACAddressButtonClick);
			// 
			// checkBox1
			// 
			this.checkBox1.Enabled = false;
			this.checkBox1.Location = new System.Drawing.Point(6, 309);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(85, 24);
			this.checkBox1.TabIndex = 34;
			this.checkBox1.Text = "expect ping";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.Gray;
			this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel5.Location = new System.Drawing.Point(3, 105);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(333, 8);
			this.panel5.TabIndex = 48;
			// 
			// RemoteProcessPathComboBox
			// 
			this.RemoteProcessPathComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.RemoteProcessPathComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.RemoteProcessPathComboBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.RemoteProcessPathComboBox.Location = new System.Drawing.Point(3, 84);
			this.RemoteProcessPathComboBox.Name = "RemoteProcessPathComboBox";
			this.RemoteProcessPathComboBox.Size = new System.Drawing.Size(333, 21);
			this.RemoteProcessPathComboBox.TabIndex = 13;
			this.RemoteProcessPathComboBox.SelectedIndexChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// KillButton
			// 
			this.KillButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.KillButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.KillButton.Location = new System.Drawing.Point(3, 57);
			this.KillButton.Name = "KillButton";
			this.KillButton.Size = new System.Drawing.Size(333, 27);
			this.KillButton.TabIndex = 12;
			this.KillButton.Text = "Kill";
			this.KillButton.UseVisualStyleBackColor = true;
			this.KillButton.Click += new System.EventHandler(this.KillButtonClick);
			// 
			// RestartButton
			// 
			this.RestartButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.RestartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RestartButton.Location = new System.Drawing.Point(3, 30);
			this.RestartButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.RestartButton.Name = "RestartButton";
			this.RestartButton.Size = new System.Drawing.Size(333, 27);
			this.RestartButton.TabIndex = 11;
			this.RestartButton.Text = "Restart";
			this.RestartButton.UseVisualStyleBackColor = true;
			this.RestartButton.Click += new System.EventHandler(this.RestartButtonClick);
			// 
			// StartButton
			// 
			this.StartButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.StartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.StartButton.Location = new System.Drawing.Point(3, 3);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(333, 27);
			this.StartButton.TabIndex = 10;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButtonClick);
			// 
			// SettingsPage
			// 
			this.SettingsPage.AutoScroll = true;
			this.SettingsPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.SettingsPage.Controls.Add(this.RemotingBox);
			this.SettingsPage.Controls.Add(this.MirrorBox);
			this.SettingsPage.Controls.Add(this.VNCBox);
			this.SettingsPage.Controls.Add(this.PsToolsBox);
			this.SettingsPage.Location = new System.Drawing.Point(4, 25);
			this.SettingsPage.Name = "SettingsPage";
			this.SettingsPage.Padding = new System.Windows.Forms.Padding(3);
			this.SettingsPage.Size = new System.Drawing.Size(339, 349);
			this.SettingsPage.TabIndex = 0;
			this.SettingsPage.Text = "Settings";
			// 
			// RemotingBox
			// 
			this.RemotingBox.Controls.Add(this.radioButton1);
			this.RemotingBox.Controls.Add(this.radioButton2);
			this.RemotingBox.Controls.Add(this.checkBox2);
			this.RemotingBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.RemotingBox.Location = new System.Drawing.Point(3, 338);
			this.RemotingBox.Name = "RemotingBox";
			this.RemotingBox.Size = new System.Drawing.Size(317, 50);
			this.RemotingBox.TabIndex = 3;
			this.RemotingBox.TabStop = false;
			this.RemotingBox.Text = "Remoting";
			// 
			// radioButton1
			// 
			this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioButton1.Location = new System.Drawing.Point(181, 19);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(52, 24);
			this.radioButton1.TabIndex = 32;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "TCP";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.Checked = true;
			this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioButton2.Location = new System.Drawing.Point(123, 19);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(52, 24);
			this.radioButton2.TabIndex = 31;
			this.radioButton2.TabStop = true;
			this.radioButton2.Text = "UDP";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// checkBox2
			// 
			this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox2.Location = new System.Drawing.Point(3, 19);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(117, 24);
			this.checkBox2.TabIndex = 30;
			this.checkBox2.Text = "Enable remoting";
			this.checkBox2.UseVisualStyleBackColor = true;
			// 
			// MirrorBox
			// 
			this.MirrorBox.Controls.Add(this.IgnorePattern);
			this.MirrorBox.Controls.Add(this.label7);
			this.MirrorBox.Controls.Add(this.TargetPath);
			this.MirrorBox.Controls.Add(this.label6);
			this.MirrorBox.Controls.Add(this.SourcePath);
			this.MirrorBox.Controls.Add(this.label4);
			this.MirrorBox.Controls.Add(this.MirrorPathPanel);
			this.MirrorBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MirrorBox.Location = new System.Drawing.Point(3, 193);
			this.MirrorBox.Name = "MirrorBox";
			this.MirrorBox.Size = new System.Drawing.Size(317, 145);
			this.MirrorBox.TabIndex = 2;
			this.MirrorBox.TabStop = false;
			this.MirrorBox.Text = "Mirror";
			// 
			// IgnorePattern
			// 
			this.IgnorePattern.Dock = System.Windows.Forms.DockStyle.Top;
			this.IgnorePattern.Location = new System.Drawing.Point(3, 123);
			this.IgnorePattern.Name = "IgnorePattern";
			this.IgnorePattern.Size = new System.Drawing.Size(311, 20);
			this.IgnorePattern.TabIndex = 23;
			this.IgnorePattern.Text = "*.v4p; *~.xml";
			this.IgnorePattern.TextChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// label7
			// 
			this.label7.Dock = System.Windows.Forms.DockStyle.Top;
			this.label7.Location = new System.Drawing.Point(3, 108);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(311, 15);
			this.label7.TabIndex = 54;
			this.label7.Text = "Ignore Pattern";
			// 
			// TargetPath
			// 
			this.TargetPath.Dock = System.Windows.Forms.DockStyle.Top;
			this.TargetPath.Location = new System.Drawing.Point(3, 88);
			this.TargetPath.Name = "TargetPath";
			this.TargetPath.Size = new System.Drawing.Size(311, 20);
			this.TargetPath.TabIndex = 22;
			this.TargetPath.TextChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// label6
			// 
			this.label6.Dock = System.Windows.Forms.DockStyle.Top;
			this.label6.Location = new System.Drawing.Point(3, 73);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(311, 15);
			this.label6.TabIndex = 52;
			this.label6.Text = "Target Path";
			// 
			// SourcePath
			// 
			this.SourcePath.Dock = System.Windows.Forms.DockStyle.Top;
			this.SourcePath.Location = new System.Drawing.Point(3, 53);
			this.SourcePath.Name = "SourcePath";
			this.SourcePath.Size = new System.Drawing.Size(311, 20);
			this.SourcePath.TabIndex = 21;
			this.SourcePath.TextChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// label4
			// 
			this.label4.Dock = System.Windows.Forms.DockStyle.Top;
			this.label4.Location = new System.Drawing.Point(3, 38);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(311, 15);
			this.label4.TabIndex = 50;
			this.label4.Text = "Source Path";
			// 
			// MirrorPathPanel
			// 
			this.MirrorPathPanel.Controls.Add(this.MirrorPathLabel);
			this.MirrorPathPanel.Controls.Add(this.MirrorPathButton);
			this.MirrorPathPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MirrorPathPanel.Location = new System.Drawing.Point(3, 16);
			this.MirrorPathPanel.Name = "MirrorPathPanel";
			this.MirrorPathPanel.Size = new System.Drawing.Size(311, 22);
			this.MirrorPathPanel.TabIndex = 20;
			// 
			// MirrorPathLabel
			// 
			this.MirrorPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MirrorPathLabel.Location = new System.Drawing.Point(43, 0);
			this.MirrorPathLabel.Name = "MirrorPathLabel";
			this.MirrorPathLabel.Size = new System.Drawing.Size(268, 22);
			this.MirrorPathLabel.TabIndex = 48;
			this.MirrorPathLabel.Text = "\\Mirror";
			this.MirrorPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MirrorPathButton
			// 
			this.MirrorPathButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.MirrorPathButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MirrorPathButton.Location = new System.Drawing.Point(0, 0);
			this.MirrorPathButton.Name = "MirrorPathButton";
			this.MirrorPathButton.Size = new System.Drawing.Size(43, 22);
			this.MirrorPathButton.TabIndex = 20;
			this.MirrorPathButton.Text = "Path";
			this.MirrorPathButton.UseVisualStyleBackColor = true;
			this.MirrorPathButton.Click += new System.EventHandler(this.MirrorPathButtonClick);
			// 
			// VNCBox
			// 
			this.VNCBox.Controls.Add(this.VNCPathPanel);
			this.VNCBox.Controls.Add(this.label3);
			this.VNCBox.Controls.Add(this.VNCPassword);
			this.VNCBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.VNCBox.Location = new System.Drawing.Point(3, 130);
			this.VNCBox.Name = "VNCBox";
			this.VNCBox.Size = new System.Drawing.Size(317, 63);
			this.VNCBox.TabIndex = 1;
			this.VNCBox.TabStop = false;
			this.VNCBox.Text = "VNC";
			// 
			// VNCPathPanel
			// 
			this.VNCPathPanel.Controls.Add(this.VNCPathLabel);
			this.VNCPathPanel.Controls.Add(this.VNCPathButton);
			this.VNCPathPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.VNCPathPanel.Location = new System.Drawing.Point(3, 16);
			this.VNCPathPanel.Name = "VNCPathPanel";
			this.VNCPathPanel.Size = new System.Drawing.Size(311, 22);
			this.VNCPathPanel.TabIndex = 10;
			// 
			// VNCPathLabel
			// 
			this.VNCPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VNCPathLabel.Location = new System.Drawing.Point(43, 0);
			this.VNCPathLabel.Name = "VNCPathLabel";
			this.VNCPathLabel.Size = new System.Drawing.Size(268, 22);
			this.VNCPathLabel.TabIndex = 48;
			this.VNCPathLabel.Text = "\\UltraVNC";
			this.VNCPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// VNCPathButton
			// 
			this.VNCPathButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.VNCPathButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.VNCPathButton.Location = new System.Drawing.Point(0, 0);
			this.VNCPathButton.Name = "VNCPathButton";
			this.VNCPathButton.Size = new System.Drawing.Size(43, 22);
			this.VNCPathButton.TabIndex = 10;
			this.VNCPathButton.Text = "Path";
			this.VNCPathButton.UseVisualStyleBackColor = true;
			this.VNCPathButton.Click += new System.EventHandler(this.VNCPathButtonClick);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(46, 41);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 16);
			this.label3.TabIndex = 43;
			this.label3.Text = "Password";
			// 
			// VNCPassword
			// 
			this.VNCPassword.Location = new System.Drawing.Point(111, 38);
			this.VNCPassword.Name = "VNCPassword";
			this.VNCPassword.PasswordChar = '*';
			this.VNCPassword.Size = new System.Drawing.Size(79, 20);
			this.VNCPassword.TabIndex = 11;
			this.VNCPassword.TextChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// PsToolsBox
			// 
			this.PsToolsBox.Controls.Add(this.AddProcessButton);
			this.PsToolsBox.Controls.Add(this.PsToolsProcessPanel);
			this.PsToolsBox.Controls.Add(this.PsToolsPathPanel);
			this.PsToolsBox.Controls.Add(this.label2);
			this.PsToolsBox.Controls.Add(this.label1);
			this.PsToolsBox.Controls.Add(this.PsToolsUsername);
			this.PsToolsBox.Controls.Add(this.PsToolsPassword);
			this.PsToolsBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.PsToolsBox.Location = new System.Drawing.Point(3, 3);
			this.PsToolsBox.Name = "PsToolsBox";
			this.PsToolsBox.Size = new System.Drawing.Size(317, 127);
			this.PsToolsBox.TabIndex = 0;
			this.PsToolsBox.TabStop = false;
			this.PsToolsBox.Text = "PsTools";
			// 
			// AddProcessButton
			// 
			this.AddProcessButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddProcessButton.Location = new System.Drawing.Point(3, 63);
			this.AddProcessButton.Name = "AddProcessButton";
			this.AddProcessButton.Size = new System.Drawing.Size(25, 20);
			this.AddProcessButton.TabIndex = 43;
			this.AddProcessButton.Text = "+";
			this.AddProcessButton.UseVisualStyleBackColor = true;
			this.AddProcessButton.Click += new System.EventHandler(this.AddProcessButtonClick);
			// 
			// PsToolsProcessPanel
			// 
			this.PsToolsProcessPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PsToolsProcessPanel.Location = new System.Drawing.Point(3, 85);
			this.PsToolsProcessPanel.Name = "PsToolsProcessPanel";
			this.PsToolsProcessPanel.Size = new System.Drawing.Size(311, 39);
			this.PsToolsProcessPanel.TabIndex = 42;
			// 
			// PsToolsPathPanel
			// 
			this.PsToolsPathPanel.Controls.Add(this.PsToolsPathLabel);
			this.PsToolsPathPanel.Controls.Add(this.PsToolsPathButton);
			this.PsToolsPathPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PsToolsPathPanel.Location = new System.Drawing.Point(3, 16);
			this.PsToolsPathPanel.Name = "PsToolsPathPanel";
			this.PsToolsPathPanel.Size = new System.Drawing.Size(311, 22);
			this.PsToolsPathPanel.TabIndex = 0;
			// 
			// PsToolsPathLabel
			// 
			this.PsToolsPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PsToolsPathLabel.Location = new System.Drawing.Point(43, 0);
			this.PsToolsPathLabel.Name = "PsToolsPathLabel";
			this.PsToolsPathLabel.Size = new System.Drawing.Size(268, 22);
			this.PsToolsPathLabel.TabIndex = 48;
			this.PsToolsPathLabel.Text = "\\PsTools";
			this.PsToolsPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PsToolsPathButton
			// 
			this.PsToolsPathButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.PsToolsPathButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.PsToolsPathButton.Location = new System.Drawing.Point(0, 0);
			this.PsToolsPathButton.Name = "PsToolsPathButton";
			this.PsToolsPathButton.Size = new System.Drawing.Size(43, 22);
			this.PsToolsPathButton.TabIndex = 1;
			this.PsToolsPathButton.Text = "Path";
			this.PsToolsPathButton.UseVisualStyleBackColor = true;
			this.PsToolsPathButton.Click += new System.EventHandler(this.PsToolsPathButtonClick);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(46, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 17);
			this.label2.TabIndex = 41;
			this.label2.Text = "Password";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(46, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 20);
			this.label1.TabIndex = 40;
			this.label1.Text = "Username";
			// 
			// PsToolsUsername
			// 
			this.PsToolsUsername.Location = new System.Drawing.Point(111, 38);
			this.PsToolsUsername.Name = "PsToolsUsername";
			this.PsToolsUsername.Size = new System.Drawing.Size(79, 20);
			this.PsToolsUsername.TabIndex = 2;
			this.PsToolsUsername.TextChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// PsToolsPassword
			// 
			this.PsToolsPassword.Location = new System.Drawing.Point(111, 61);
			this.PsToolsPassword.Name = "PsToolsPassword";
			this.PsToolsPassword.PasswordChar = '*';
			this.PsToolsPassword.Size = new System.Drawing.Size(79, 20);
			this.PsToolsPassword.TabIndex = 3;
			this.PsToolsPassword.TextChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// SimulatorPage
			// 
			this.SimulatorPage.Controls.Add(this.SimulatorListBox);
			this.SimulatorPage.Controls.Add(this.panel7);
			this.SimulatorPage.Controls.Add(this.panel6);
			this.SimulatorPage.Location = new System.Drawing.Point(4, 25);
			this.SimulatorPage.Name = "SimulatorPage";
			this.SimulatorPage.Padding = new System.Windows.Forms.Padding(3);
			this.SimulatorPage.Size = new System.Drawing.Size(339, 349);
			this.SimulatorPage.TabIndex = 2;
			this.SimulatorPage.Text = "Simulator";
			this.SimulatorPage.UseVisualStyleBackColor = true;
			// 
			// SimulatorListBox
			// 
			this.SimulatorListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimulatorListBox.FormattingEnabled = true;
			this.SimulatorListBox.Location = new System.Drawing.Point(3, 47);
			this.SimulatorListBox.Name = "SimulatorListBox";
			this.SimulatorListBox.Size = new System.Drawing.Size(333, 290);
			this.SimulatorListBox.TabIndex = 3;
			this.SimulatorListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SimulatorListBoxMouseUp);
			// 
			// panel7
			// 
			this.panel7.Controls.Add(this.SimulatorStringEdit);
			this.panel7.Controls.Add(this.AddSimulatorStringButton);
			this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel7.Location = new System.Drawing.Point(3, 25);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(333, 22);
			this.panel7.TabIndex = 5;
			// 
			// SimulatorStringEdit
			// 
			this.SimulatorStringEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimulatorStringEdit.Location = new System.Drawing.Point(0, 0);
			this.SimulatorStringEdit.Name = "SimulatorStringEdit";
			this.SimulatorStringEdit.Size = new System.Drawing.Size(313, 20);
			this.SimulatorStringEdit.TabIndex = 0;
			this.SimulatorStringEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SimulatorStringEditKeyPress);
			// 
			// AddSimulatorStringButton
			// 
			this.AddSimulatorStringButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddSimulatorStringButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddSimulatorStringButton.Location = new System.Drawing.Point(313, 0);
			this.AddSimulatorStringButton.Name = "AddSimulatorStringButton";
			this.AddSimulatorStringButton.Size = new System.Drawing.Size(20, 22);
			this.AddSimulatorStringButton.TabIndex = 1;
			this.AddSimulatorStringButton.Text = "+";
			this.AddSimulatorStringButton.UseVisualStyleBackColor = true;
			this.AddSimulatorStringButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AddSimulatorStringButtonClick);
			// 
			// panel6
			// 
			this.panel6.Controls.Add(this.SimulatorIPEdit);
			this.panel6.Controls.Add(this.SimulatorPortUpDown);
			this.panel6.Controls.Add(this.SimulatorConnectButton);
			this.panel6.Controls.Add(this.SimulatorTCPCheckBox);
			this.panel6.Controls.Add(this.SimulatorUDPCheckBox);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel6.Location = new System.Drawing.Point(3, 3);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(333, 22);
			this.panel6.TabIndex = 4;
			// 
			// SimulatorIPEdit
			// 
			this.SimulatorIPEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimulatorIPEdit.Location = new System.Drawing.Point(99, 0);
			this.SimulatorIPEdit.Name = "SimulatorIPEdit";
			this.SimulatorIPEdit.Size = new System.Drawing.Size(121, 20);
			this.SimulatorIPEdit.TabIndex = 4;
			this.SimulatorIPEdit.TextChanged += new System.EventHandler(this.SimulatorIPEditTextChanged);
			// 
			// SimulatorPortUpDown
			// 
			this.SimulatorPortUpDown.Dock = System.Windows.Forms.DockStyle.Right;
			this.SimulatorPortUpDown.Location = new System.Drawing.Point(220, 0);
			this.SimulatorPortUpDown.Maximum = new decimal(new int[] {
			                                               	65535,
			                                               	0,
			                                               	0,
			                                               	0});
			this.SimulatorPortUpDown.Name = "SimulatorPortUpDown";
			this.SimulatorPortUpDown.Size = new System.Drawing.Size(56, 20);
			this.SimulatorPortUpDown.TabIndex = 3;
			this.SimulatorPortUpDown.Value = new decimal(new int[] {
			                                             	44444,
			                                             	0,
			                                             	0,
			                                             	0});
			this.SimulatorPortUpDown.ValueChanged += new System.EventHandler(this.SimulatorPortUpDownValueChanged);
			// 
			// SimulatorConnectButton
			// 
			this.SimulatorConnectButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.SimulatorConnectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SimulatorConnectButton.Location = new System.Drawing.Point(276, 0);
			this.SimulatorConnectButton.Name = "SimulatorConnectButton";
			this.SimulatorConnectButton.Size = new System.Drawing.Size(57, 22);
			this.SimulatorConnectButton.TabIndex = 5;
			this.SimulatorConnectButton.Text = "Connect";
			this.SimulatorConnectButton.UseVisualStyleBackColor = true;
			this.SimulatorConnectButton.Click += new System.EventHandler(this.SimulatorConnectButtonClick);
			// 
			// SimulatorTCPCheckBox
			// 
			this.SimulatorTCPCheckBox.Checked = true;
			this.SimulatorTCPCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.SimulatorTCPCheckBox.Location = new System.Drawing.Point(51, 0);
			this.SimulatorTCPCheckBox.Name = "SimulatorTCPCheckBox";
			this.SimulatorTCPCheckBox.Size = new System.Drawing.Size(48, 22);
			this.SimulatorTCPCheckBox.TabIndex = 1;
			this.SimulatorTCPCheckBox.TabStop = true;
			this.SimulatorTCPCheckBox.Text = "TCP";
			this.SimulatorTCPCheckBox.UseVisualStyleBackColor = true;
			// 
			// SimulatorUDPCheckBox
			// 
			this.SimulatorUDPCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.SimulatorUDPCheckBox.Location = new System.Drawing.Point(0, 0);
			this.SimulatorUDPCheckBox.Name = "SimulatorUDPCheckBox";
			this.SimulatorUDPCheckBox.Size = new System.Drawing.Size(51, 22);
			this.SimulatorUDPCheckBox.TabIndex = 0;
			this.SimulatorUDPCheckBox.Text = "UDP";
			this.SimulatorUDPCheckBox.UseVisualStyleBackColor = true;
			// 
			// LeftPanel
			// 
			this.LeftPanel.AutoScroll = true;
			this.LeftPanel.Controls.Add(this.LeftTabControl);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = new System.Drawing.Point(0, 0);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = new System.Drawing.Size(245, 378);
			this.LeftPanel.TabIndex = 21;
			// 
			// LeftTabControl
			// 
			this.LeftTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.LeftTabControl.Controls.Add(this.IPPage);
			this.LeftTabControl.Controls.Add(this.GroupPage);
			this.LeftTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftTabControl.Location = new System.Drawing.Point(0, 0);
			this.LeftTabControl.Name = "LeftTabControl";
			this.LeftTabControl.SelectedIndex = 0;
			this.LeftTabControl.Size = new System.Drawing.Size(245, 378);
			this.LeftTabControl.TabIndex = 3;
			this.LeftTabControl.SelectedIndexChanged += new System.EventHandler(this.LeftTabControlSelectedIndexChanged);
			// 
			// IPPage
			// 
			this.IPPage.Controls.Add(this.IPListPanel);
			this.IPPage.Controls.Add(this.GroupFilterDropDown);
			this.IPPage.Controls.Add(this.LeftBottomPanel);
			this.IPPage.Controls.Add(this.LeftTopPanel);
			this.IPPage.Location = new System.Drawing.Point(4, 25);
			this.IPPage.Name = "IPPage";
			this.IPPage.Padding = new System.Windows.Forms.Padding(3);
			this.IPPage.Size = new System.Drawing.Size(237, 349);
			this.IPPage.TabIndex = 0;
			this.IPPage.Text = "IPs";
			this.IPPage.UseVisualStyleBackColor = true;
			// 
			// IPListPanel
			// 
			this.IPListPanel.AutoScroll = true;
			this.IPListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IPListPanel.Location = new System.Drawing.Point(3, 44);
			this.IPListPanel.Name = "IPListPanel";
			this.IPListPanel.Size = new System.Drawing.Size(231, 277);
			this.IPListPanel.TabIndex = 5;
			// 
			// GroupFilterDropDown
			// 
			this.GroupFilterDropDown.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupFilterDropDown.FormattingEnabled = true;
			this.GroupFilterDropDown.Items.AddRange(new object[] {
									".All",
									".Offline",
									".Online"});
			this.GroupFilterDropDown.Location = new System.Drawing.Point(3, 23);
			this.GroupFilterDropDown.Name = "GroupFilterDropDown";
			this.GroupFilterDropDown.Size = new System.Drawing.Size(231, 21);
			this.GroupFilterDropDown.Sorted = true;
			this.GroupFilterDropDown.TabIndex = 6;
			this.GroupFilterDropDown.SelectedIndexChanged += new System.EventHandler(this.GroupFilterDropDownSelectedIndexChanged);
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Controls.Add(this.InvertSelectionButton);
			this.LeftBottomPanel.Controls.Add(this.SelectAllButton);
			this.LeftBottomPanel.Controls.Add(this.RemoveAllButton);
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LeftBottomPanel.Location = new System.Drawing.Point(3, 321);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = new System.Drawing.Size(231, 25);
			this.LeftBottomPanel.TabIndex = 3;
			// 
			// InvertSelectionButton
			// 
			this.InvertSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvertSelectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.InvertSelectionButton.Location = new System.Drawing.Point(60, 0);
			this.InvertSelectionButton.Name = "InvertSelectionButton";
			this.InvertSelectionButton.Size = new System.Drawing.Size(93, 25);
			this.InvertSelectionButton.TabIndex = 4;
			this.InvertSelectionButton.Text = "Invert Selection";
			this.InvertSelectionButton.UseVisualStyleBackColor = true;
			this.InvertSelectionButton.Click += new System.EventHandler(this.InvertSelectionButtonClick);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.SelectAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SelectAllButton.Location = new System.Drawing.Point(0, 0);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = new System.Drawing.Size(60, 25);
			this.SelectAllButton.TabIndex = 1;
			this.SelectAllButton.Text = "Select all";
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButtonClick);
			// 
			// RemoveAllButton
			// 
			this.RemoveAllButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RemoveAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RemoveAllButton.Location = new System.Drawing.Point(153, 0);
			this.RemoveAllButton.Name = "RemoveAllButton";
			this.RemoveAllButton.Size = new System.Drawing.Size(78, 25);
			this.RemoveAllButton.TabIndex = 0;
			this.RemoveAllButton.Text = "Remove all IPs";
			this.RemoveAllButton.UseVisualStyleBackColor = true;
			this.RemoveAllButton.Click += new System.EventHandler(this.RemoveAllButtonClick);
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Controls.Add(this.NewIPEdit);
			this.LeftTopPanel.Controls.Add(this.AddIPButton);
			this.LeftTopPanel.Controls.Add(this.VNCButton);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LeftTopPanel.Location = new System.Drawing.Point(3, 3);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = new System.Drawing.Size(231, 20);
			this.LeftTopPanel.TabIndex = 1;
			// 
			// NewIPEdit
			// 
			this.NewIPEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewIPEdit.Location = new System.Drawing.Point(0, 0);
			this.NewIPEdit.Name = "NewIPEdit";
			this.NewIPEdit.Size = new System.Drawing.Size(169, 20);
			this.NewIPEdit.TabIndex = 0;
			this.NewIPEdit.Text = "192.168.0.1";
			this.NewIPEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NewIPEditKeyPress);
			// 
			// AddIPButton
			// 
			this.AddIPButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddIPButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddIPButton.Location = new System.Drawing.Point(169, 0);
			this.AddIPButton.Name = "AddIPButton";
			this.AddIPButton.Size = new System.Drawing.Size(20, 20);
			this.AddIPButton.TabIndex = 1;
			this.AddIPButton.Text = "+";
			this.AddIPButton.UseVisualStyleBackColor = true;
			this.AddIPButton.Click += new System.EventHandler(this.AddIPButtonClick);
			// 
			// VNCButton
			// 
			this.VNCButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.VNCButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.VNCButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VNCButton.Location = new System.Drawing.Point(189, 0);
			this.VNCButton.Name = "VNCButton";
			this.VNCButton.Size = new System.Drawing.Size(42, 20);
			this.VNCButton.TabIndex = 2;
			this.VNCButton.Text = "VNC";
			this.VNCButton.UseVisualStyleBackColor = true;
			this.VNCButton.Click += new System.EventHandler(this.VNCButtonClick);
			// 
			// GroupPage
			// 
			this.GroupPage.Controls.Add(this.GroupListPanel);
			this.GroupPage.Controls.Add(this.panel9);
			this.GroupPage.Controls.Add(this.panel8);
			this.GroupPage.Location = new System.Drawing.Point(4, 25);
			this.GroupPage.Name = "GroupPage";
			this.GroupPage.Padding = new System.Windows.Forms.Padding(3);
			this.GroupPage.Size = new System.Drawing.Size(237, 349);
			this.GroupPage.TabIndex = 1;
			this.GroupPage.Text = "Groups";
			this.GroupPage.UseVisualStyleBackColor = true;
			// 
			// GroupListPanel
			// 
			this.GroupListPanel.AutoScroll = true;
			this.GroupListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupListPanel.Location = new System.Drawing.Point(3, 28);
			this.GroupListPanel.Name = "GroupListPanel";
			this.GroupListPanel.Size = new System.Drawing.Size(231, 293);
			this.GroupListPanel.TabIndex = 6;
			// 
			// panel9
			// 
			this.panel9.Controls.Add(this.NewGroupEdit);
			this.panel9.Controls.Add(this.AddGroupButton);
			this.panel9.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel9.Location = new System.Drawing.Point(3, 3);
			this.panel9.Name = "panel9";
			this.panel9.Size = new System.Drawing.Size(231, 25);
			this.panel9.TabIndex = 5;
			// 
			// NewGroupEdit
			// 
			this.NewGroupEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewGroupEdit.Location = new System.Drawing.Point(0, 0);
			this.NewGroupEdit.Name = "NewGroupEdit";
			this.NewGroupEdit.Size = new System.Drawing.Size(211, 20);
			this.NewGroupEdit.TabIndex = 2;
			this.NewGroupEdit.Text = "N\'Sync";
			this.NewGroupEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NewGroupEditKeyPress);
			// 
			// AddGroupButton
			// 
			this.AddGroupButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddGroupButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddGroupButton.Location = new System.Drawing.Point(211, 0);
			this.AddGroupButton.Name = "AddGroupButton";
			this.AddGroupButton.Size = new System.Drawing.Size(20, 25);
			this.AddGroupButton.TabIndex = 3;
			this.AddGroupButton.Text = "+";
			this.AddGroupButton.UseVisualStyleBackColor = true;
			this.AddGroupButton.Click += new System.EventHandler(this.AddGroupButtonClick);
			// 
			// panel8
			// 
			this.panel8.Controls.Add(this.InvertGroupSelectionButton);
			this.panel8.Controls.Add(this.SelectAllGroupsButton);
			this.panel8.Controls.Add(this.RemoveAllGroupsButton);
			this.panel8.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel8.Location = new System.Drawing.Point(3, 321);
			this.panel8.Name = "panel8";
			this.panel8.Size = new System.Drawing.Size(231, 25);
			this.panel8.TabIndex = 4;
			// 
			// InvertGroupSelectionButton
			// 
			this.InvertGroupSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvertGroupSelectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.InvertGroupSelectionButton.Location = new System.Drawing.Point(60, 0);
			this.InvertGroupSelectionButton.Name = "InvertGroupSelectionButton";
			this.InvertGroupSelectionButton.Size = new System.Drawing.Size(93, 25);
			this.InvertGroupSelectionButton.TabIndex = 4;
			this.InvertGroupSelectionButton.Text = "Invert Selection";
			this.InvertGroupSelectionButton.UseVisualStyleBackColor = true;
			this.InvertGroupSelectionButton.Click += new System.EventHandler(this.InvertGroupSelectionButtonClick);
			// 
			// SelectAllGroupsButton
			// 
			this.SelectAllGroupsButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.SelectAllGroupsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SelectAllGroupsButton.Location = new System.Drawing.Point(0, 0);
			this.SelectAllGroupsButton.Name = "SelectAllGroupsButton";
			this.SelectAllGroupsButton.Size = new System.Drawing.Size(60, 25);
			this.SelectAllGroupsButton.TabIndex = 1;
			this.SelectAllGroupsButton.Text = "Select all";
			this.SelectAllGroupsButton.UseVisualStyleBackColor = true;
			this.SelectAllGroupsButton.Click += new System.EventHandler(this.SelectAllGroupsButtonClick);
			// 
			// RemoveAllGroupsButton
			// 
			this.RemoveAllGroupsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RemoveAllGroupsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.RemoveAllGroupsButton.Location = new System.Drawing.Point(153, 0);
			this.RemoveAllGroupsButton.Name = "RemoveAllGroupsButton";
			this.RemoveAllGroupsButton.Size = new System.Drawing.Size(78, 25);
			this.RemoveAllGroupsButton.TabIndex = 0;
			this.RemoveAllGroupsButton.Text = "Remove all IPs";
			this.RemoveAllGroupsButton.UseVisualStyleBackColor = true;
			this.RemoveAllGroupsButton.Click += new System.EventHandler(this.RemoveAllGroupsButtonClick);
			// 
			// OnlineWorker
			// 
			this.OnlineWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OnlineWorkerDoWork);
			// 
			// WatchWorker
			// 
			this.WatchWorker.WorkerReportsProgress = true;
			this.WatchWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.WatchWorkerDoWork);
			this.WatchWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.WatchLogCB);
			// 
			// Remoter
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.LeftPanel);
			this.DoubleBuffered = true;
			this.Name = "Remoter";
			this.Size = new System.Drawing.Size(592, 378);
			this.panel1.ResumeLayout(false);
			this.Simulator.ResumeLayout(false);
			this.CommandsPage.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel10.ResumeLayout(false);
			this.SettingsPage.ResumeLayout(false);
			this.RemotingBox.ResumeLayout(false);
			this.MirrorBox.ResumeLayout(false);
			this.MirrorBox.PerformLayout();
			this.MirrorPathPanel.ResumeLayout(false);
			this.VNCBox.ResumeLayout(false);
			this.VNCBox.PerformLayout();
			this.VNCPathPanel.ResumeLayout(false);
			this.PsToolsBox.ResumeLayout(false);
			this.PsToolsBox.PerformLayout();
			this.PsToolsPathPanel.ResumeLayout(false);
			this.SimulatorPage.ResumeLayout(false);
			this.panel7.ResumeLayout(false);
			this.panel7.PerformLayout();
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimulatorPortUpDown)).EndInit();
			this.LeftPanel.ResumeLayout(false);
			this.LeftTabControl.ResumeLayout(false);
			this.IPPage.ResumeLayout(false);
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.GroupPage.ResumeLayout(false);
			this.panel9.ResumeLayout(false);
			this.panel9.PerformLayout();
			this.panel8.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.CheckBox WatchProcessCheckbox;
		private System.Windows.Forms.Button VNCButton;
		private System.Windows.Forms.Panel PsToolsProcessPanel;
		private System.Windows.Forms.Panel panel12;
		private System.Windows.Forms.Button AddProcessButton;
		private System.Windows.Forms.ComboBox WatchProcessPathComboBox;
		private System.Windows.Forms.ComboBox RemoteProcessPathComboBox;
		private System.Windows.Forms.CheckBox MirrorTestCheckBox;
		private System.Windows.Forms.Panel panel10;
		private System.ComponentModel.BackgroundWorker OnlineWorker;
		private System.ComponentModel.BackgroundWorker WatchWorker;
		private System.Windows.Forms.Label label5;
		
		private System.Windows.Forms.Button InvertGroupSelectionButton;
		private System.Windows.Forms.Button SelectAllGroupsButton;
		private System.Windows.Forms.Button RemoveAllGroupsButton;
		private System.Windows.Forms.ComboBox GroupFilterDropDown;
		private System.Windows.Forms.Panel GroupListPanel;
		private System.Windows.Forms.TextBox NewGroupEdit;
		private System.Windows.Forms.Panel panel8;
		private System.Windows.Forms.Button AddGroupButton;
		private System.Windows.Forms.Panel panel9;
		private System.Windows.Forms.TabPage GroupPage;
		private System.Windows.Forms.TabPage IPPage;
		private System.Windows.Forms.TabControl LeftTabControl;
		private System.Windows.Forms.ListBox MCListBox;
		private System.Windows.Forms.NumericUpDown MCPortUpDown;
		private System.Windows.Forms.TextBox MCStringEdit;
		private System.Windows.Forms.Button AddMCString;
		private System.Windows.Forms.RadioButton MCTCPCheckBox;
		private System.Windows.Forms.RadioButton MCUDPCheckBox;
		private System.Windows.Forms.Button AddSimulatorStringButton;
		private System.Windows.Forms.Button SimulatorConnectButton;
		private System.Windows.Forms.RadioButton SimulatorUDPCheckBox;
		private System.Windows.Forms.RadioButton SimulatorTCPCheckBox;
		private System.Windows.Forms.TextBox SimulatorStringEdit;
		private System.Windows.Forms.NumericUpDown SimulatorPortUpDown;
		private System.Windows.Forms.ListBox SimulatorListBox;
		private System.Windows.Forms.TextBox SimulatorIPEdit;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Panel panel7;
		private System.Windows.Forms.TabControl Simulator;
		private System.Windows.Forms.TabPage CommandsPage;
		private System.Windows.Forms.TabPage SettingsPage;
		private System.Windows.Forms.TabPage SimulatorPage;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.RadioButton WatchModeOff;
		private System.Windows.Forms.Button MACAddressButton;
		private System.Windows.Forms.Button WakeOnLanButton;
		private System.Windows.Forms.Button InvertSelectionButton;
		private System.Windows.Forms.Button SelectAllButton;
		private System.Windows.Forms.Panel MirrorPathPanel;
		private System.Windows.Forms.Panel VNCPathPanel;
		private System.Windows.Forms.Panel PsToolsPathPanel;
		private System.Windows.Forms.TextBox PsToolsPassword;
		private System.Windows.Forms.TextBox PsToolsUsername;
		private System.Windows.Forms.Label MirrorPathLabel;
		private System.Windows.Forms.Button MirrorPathButton;
		private System.Windows.Forms.Button MirrorButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox SourcePath;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox TargetPath;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox IgnorePattern;
		private System.Windows.Forms.Panel LeftPanel;
		private System.Windows.Forms.Button AddIPButton;
		private System.Windows.Forms.TextBox NewIPEdit;
		private System.Windows.Forms.Panel LeftTopPanel;
		private System.Windows.Forms.Button RemoveAllButton;
		private System.Windows.Forms.Panel LeftBottomPanel;
		private System.Windows.Forms.TextBox VNCPassword;
		private System.Windows.Forms.Label VNCPathLabel;
		private System.Windows.Forms.Button VNCPathButton;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.GroupBox PsToolsBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox VNCBox;
		private System.Windows.Forms.GroupBox MirrorBox;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.GroupBox RemotingBox;
		private System.Windows.Forms.Panel IPListPanel;
		private System.Windows.Forms.RadioButton WatchModeReboot;
		private System.Windows.Forms.RadioButton WatchModeRestart;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button ShutdownButton;
		private System.Windows.Forms.Button RebootButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.Button RestartButton;
		private System.Windows.Forms.Button KillButton;
		private System.Windows.Forms.Button PsToolsPathButton;
		private System.Windows.Forms.Label PsToolsPathLabel;
		private System.Windows.Forms.FolderBrowserDialog FFolderBrowserDialog;
		#endregion GUI
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create config inputs
			FHost.CreateStringConfig("Settings", TSliceMode.Single, TPinVisibility.True, out FSettingsInput);
			FSettingsInput.SliceCount = 1;
			
			//create inputs
			FHost.CreateValueInput("Value Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMyValueInput);
			FMyValueInput.SetSubType(0, 1, 1, 0, false, false, false);
			
			FHost.CreateStringConfig("IPs", TSliceMode.Dynamic, TPinVisibility.True, out FIPListInput);
			FIPListInput.SetSubType("127.0.0.1", false);

			//create outputs
			FHost.CreateValueOutput("IP Online", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOnlineOutput);
			FOnlineOutput.SetSubType(0, 1, 1, 0, false, false, false);
			
			FHost.CreateValueOutput("App Online", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAppIsOnlineOutput);
			FAppIsOnlineOutput.SetSubType(0, 1, 1, 0, false, false, false);
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			if (FLoading)
			{
				if (Input == FSettingsInput)
				{
					string settings;
					try
					{
						FSettingsInput.GetString(0, out settings);
						LoadSettings(settings);
					}catch{}
				}
				else if (Input == FIPListInput)
				{
					string ipmac;
					string[] ip_mac;
					for (int i=0; i<Input.SliceCount; i++)
					{
						FIPListInput.GetString(i, out ipmac);
						char s = '|';
						ip_mac = ipmac.Split(s);
						try
						{
							AddIP(ip_mac[0], ip_mac[1], Convert.ToBoolean(ip_mac[2]), ip_mac[3]);
						}
						catch (IndexOutOfRangeException e)
						{
							AddIP(ip_mac[0], "", false, "");
						}
					}
					
					FOnlineOutput.SliceCount = IPListPanel.Controls.Count;
					FAppIsOnlineOutput.SliceCount = IPListPanel.Controls.Count;
					SortIPs();
				}
			}
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			if ((FLoading) && (PsToolsProcessPanel.Controls.Count == 0))
			{
				AddProcess("notepad.exe", "");
				UpdateProcessComboBoxes();
			}
			
			FLoading = false;
			
			int i = IPListPanel.Controls.Count-1;
			foreach (IPControl ipc in IPListPanel.Controls)
			{
				FOnlineOutput.SetValue(i, Convert.ToDouble(ipc.IsOnline));
				FAppIsOnlineOutput.SetValue(i, Convert.ToDouble(ipc.AppIsOnline));
				i--;
			}
		}
		
		#endregion mainloop
		
		#region IPs
		private void SortIPs()
		{
			IPListPanel.SuspendLayout();
			FIPSortList.Clear();
			foreach (IPControl ipc in IPListPanel.Controls)
				if (ipc != null)
				FIPSortList.Add(ipc);
			
			if (FIPSortList.Count > 0)
				FIPSortList.Sort(delegate(IPControl ipc1, IPControl ipc2)
				                 {
				                 	string[] ip1 = ipc1.IP.Split('.');
				                 	string[] ip2 = ipc2.IP.Split('.');
				                 	
				                 	//UInt32 i1 = (UInt32) (byte.Parse(ip1[3]) + (byte.Parse(ip1[2]) + (1 << 8)) + (byte.Parse(ip1[1]) + (1 << 16)) + (byte.Parse(ip1[0]) + (1 << 24)));
				                 	//UInt32 i2 = (UInt32) (byte.Parse(ip2[3]) + (byte.Parse(ip2[2]) + (1 << 8)) + (byte.Parse(ip2[1]) + (1 << 16)) + (byte.Parse(ip2[0]) + (1 << 24)));
				                 	
				                 	//int i1 = byte.Parse(ip1[0]) + 255*3 + byte.Parse(ip1[1] + 255*2 +
				                 	Decimal i1 = (byte.Parse(ip1[0]) << 24) + (byte.Parse(ip1[1]) << 16) + (byte.Parse(ip1[2]) << 8) + byte.Parse(ip1[3]);
				                 	Decimal i2 = (byte.Parse(ip2[0]) << 24) + (byte.Parse(ip2[1]) << 16) + (byte.Parse(ip2[2]) << 8) + byte.Parse(ip2[3]);
				                 	if (i1==i2)
				                 		return 0;
				                 	else if (i1>i2)
				                 		return 1;
				                 	else
				                 		return -1;
				                 });
			
			for (int i=0; i<FIPSortList.Count; i++)
				FIPSortList[i].BringToFront();
			
			IPListPanel.ResumeLayout();
		}
		
		private void IPSelectButtonHandlerCB(UserControl Control)
		{
			NewIPEdit.Text = (Control as IPControl).IP;
			UpdateIPListInput();
		}
		
		private void IPXButtonHandlerCB(string IP)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
			{
				if (ipc.IP == IP)
				{
					IPListPanel.Controls.Remove(ipc);
					foreach(GroupControl gc in GroupListPanel.Controls)
					{
						if (gc.GroupName == UNGROUPED)
						{
							gc.IPControls.Remove(ipc);
							if (gc.IPControls.Count == 0)
								GroupListPanel.Controls.Remove(gc);
							break;
						}
					}
					break;
				}
			}
			
			UpdateIPListInput();
		}
		
		private void VNCButtonHandlerCB(string IP)
		{
			string filename = FVNCPath + "\\vncviewer.exe";
			if (System.IO.File.Exists(filename))
				Execute(filename, FVNCPath, IP + " /password " + VNCPassword.Text, false, false);
		}
		
		private void EXPButtonHandlerCB(string IP)
		{
			Execute("explorer.exe", "\\\\"+IP + TargetPath.Text, "\\\\"+IP + TargetPath.Text, false, false);
		}
		
		private void AddIP(string IP, string MAC, bool Selected, string Groups)
		{
			IPAddress ipa;
			if (!IPAddress.TryParse(IP, out ipa))
			{
				FHost.Log(TLogType.Error, "Not a valid IP address");
				return;
			}
			
			for (int i=0; i<IPListPanel.Controls.Count; i++)
			{
				if ((IPListPanel.Controls[i] as IPControl).IP == IP)
					return;
			}
			
			//add ip only if its not only there
			IPControl ip = new IPControl(IP);
			ip.MacAddress = MAC;
			ip.IsSelected = Selected;
			ip.Parent = IPListPanel;
			ip.Dock = DockStyle.Top;
			ip.BringToFront();
			ip.OnVNCButton += new ButtonHandler(VNCButtonHandlerCB);
			ip.OnEXPButton += new ButtonHandler(EXPButtonHandlerCB);
			ip.OnXButton += new ButtonHandler(IPXButtonHandlerCB);
			ip.OnIPSelectButton += new ButtonUpHandler(IPSelectButtonHandlerCB);
			
			ip.AddGroups(Groups);
			
			string[] groups;
			char s = ';';
			groups = Groups.Split(s);
			
			for (int i=0; i<groups.Length; i++)
				if (groups[i] == "")
				AddGroup(UNGROUPED, ip);
			else
				AddGroup(groups[i], ip);
		}
		
		private void DoAddIP(string NewIP, string Group)
		{
			IPListPanel.SuspendLayout();
			if (NewIP.Contains("-"))
			{
				int dotpos = NewIP.LastIndexOf('.');
				int dashpos = NewIP.LastIndexOf('-');
				string baseip = NewIP.Substring(0, dotpos+1);
				string first = NewIP.Substring(dotpos+1, dashpos-dotpos-1);
				string last = NewIP.Substring(dashpos+1);
				
				try
				{
					byte from = Convert.ToByte(first);
					byte to = Convert.ToByte(last);
					for (int i=from; i<=to; i++)
					{
						AddIP(baseip + i.ToString(), "", false, Group);
					}
				}
				catch
				{
					FHost.Log(TLogType.Error, "Not a valid IP address");
				}
			}
			else
				AddIP(NewIP, "", false, Group);
			
			UpdateIPListInput();
			IPListPanel.ResumeLayout();
		}
		
		private void UpdateIPListInput()
		{
			FIPListInput.SliceCount = IPListPanel.Controls.Count;
			int i=0;
			foreach (IPControl ipc in IPListPanel.Controls)
			{
				FIPListInput.SetString(i, ipc.IP + "|" + ipc.MacAddress + "|" + ipc.IsSelected.ToString() + "|" + ipc.Groups);
				i++;
			}
			
			FOnlineOutput.SliceCount = IPListPanel.Controls.Count;
			FAppIsOnlineOutput.SliceCount = IPListPanel.Controls.Count;
		}
		
		void AddIPButtonClick(object sender, EventArgs e)
		{
			DoAddIP(NewIPEdit.Text.Trim(), "");
		}
		
		void RemoveAllButtonClick(object sender, EventArgs e)
		{
			IPListPanel.Controls.Clear();
			
			UpdateIPListInput();
		}
		
		void NewIPEditKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
				DoAddIP(NewIPEdit.Text.Trim(), "");
		}
		
		void SelectAllButtonClick(object sender, EventArgs e)
		{
			if (IPListPanel.Controls.Count == 0)
				return;
			
			bool selected = !(IPListPanel.Controls[0] as IPControl).IsSelected;
			foreach(IPControl ipc in IPListPanel.Controls)
				ipc.IsSelected = selected;
			
			UpdateIPListInput();
		}
		
		void InvertSelectionButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				ipc.IsSelected = !ipc.IsSelected;
			
			UpdateIPListInput();
		}
		#endregion IPs
		
		#region IPGroups
		void AddGroupButtonClick(object sender, EventArgs e)
		{
			DoAddGroup();
		}
		
		private void AddGroup(string Group, IPControl IP)
		{
			for (int i=0; i<GroupListPanel.Controls.Count; i++)
			{
				if ((GroupListPanel.Controls[i] as GroupControl).GroupName == Group)
				{
					(GroupListPanel.Controls[i] as GroupControl).AddIP(IP);
					return;
				}
			}
			
			//add group only if its not yet there
			GroupControl gc = new GroupControl(Group);
			gc.Height = 30;
			gc.IsSelected = false;
			gc.Parent = GroupListPanel;
			gc.Dock = DockStyle.Top;
			gc.BringToFront();
			gc.OnXButton += new ButtonHandler(GroupXButtonHandlerCB);
			gc.OnGroupSelectButton += new ButtonUpHandler(GroupSelectButtonHandlerCB);
			gc.OnGroupChanged += new GroupChangedHandler(GroupChangedHandlerCB);
			
			gc.AddIP(IP);
			GroupFilterDropDown.Items.Add(Group);
		}
		
		private void DoAddGroup()
		{
			string newgroup = NewGroupEdit.Text.Trim();
			
			AddGroup(newgroup, null);
		}
		
		void NewGroupEditKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
				DoAddGroup();
		}
		
		private void GroupSelectButtonHandlerCB(UserControl Control)
		{
			UpdateIPSelectionsFromGroups();
		}
		
		private void UpdateIPSelectionsFromGroups()
		{
			foreach(IPControl ipc in IPListPanel.Controls)
			{
				ipc.IsSelected = false;
				foreach(GroupControl gc in GroupListPanel.Controls)
					if (gc.IsSelected)
					ipc.IsSelected |= ipc.IsPartOfGroup(gc.GroupName);
			}
		}
		
		private void GroupChangedHandlerCB(GroupControl Group, string OldGroupName, List<string> IPs)
		{
			//the name may have changed
			//ips may have been added/removed
			
			//remove the old groupname of all ips
			//note: ips may be deleted at this stage, when they are only part of one group
			foreach(IPControl ipc in IPListPanel.Controls)
				ipc.RemoveGroup(OldGroupName);
			
			//since the groups name may have changed, remove the oldname and add the newname again
			GroupFilterDropDown.Items.Remove(OldGroupName);
			GroupFilterDropDown.Items.Add(Group.GroupName);
			
			//parse this groupcontrols ips add them
			foreach(string s in IPs)
				DoAddIP(s, Group.GroupName);
		}
		
		private void GroupXButtonHandlerCB(string Group)
		{
			RemoveGroup(Group);
		}
		
		private void RemoveGroup(string Group)
		{
			if (Group == UNGROUPED)
				return;
			
			//go through all ips and remove this group
			//and delete ip if it was only in this group
			for (int i=IPListPanel.Controls.Count-1; i>=0; i--)
				(IPListPanel.Controls[i] as IPControl).RemoveGroup(Group);

			//remove from filter dropdown
			GroupFilterDropDown.Items.Remove(Group);
			
			//remove this groups control
			for (int i=0; i<GroupListPanel.Controls.Count; i++)
			{
				if ((GroupListPanel.Controls[i] as GroupControl).GroupName == Group)
				{
					GroupListPanel.Controls.RemoveAt(i);
					return;
				}
			}
		}
		
		void GroupFilterDropDownSelectedIndexChanged(object sender, EventArgs e)
		{
			IPListPanel.SuspendLayout();
			SortIPs();
			
			FIPCLiveUpdate = TIPCLiveUpdate.Off;
			switch (GroupFilterDropDown.SelectedIndex)
			{
				case 0:
					{
						foreach(IPControl ipc in IPListPanel.Controls)
							ipc.Visible = true;
						break;
					}
				case 1:
					{
						foreach(IPControl ipc in IPListPanel.Controls)
							ipc.Visible = !ipc.IsOnline;
						FIPCLiveUpdate = TIPCLiveUpdate.AllOffline;
						break;
					}
				case 2:
					{
						foreach(IPControl ipc in IPListPanel.Controls)
							ipc.Visible = ipc.IsOnline;
						FIPCLiveUpdate = TIPCLiveUpdate.AllOnline;
						break;
					}
				default:
					{
						foreach(IPControl ipc in IPListPanel.Controls)
							ipc.Visible = ipc.IsPartOfGroup(GroupFilterDropDown.Items[GroupFilterDropDown.SelectedIndex].ToString());
						break;
					}
			}
			IPListPanel.ResumeLayout();
		}
		
		void LeftTabControlSelectedIndexChanged(object sender, EventArgs e)
		{
			if (LeftTabControl.SelectedIndex == 1) //groups
			{
				foreach(IPControl ipc in IPListPanel.Controls)
					ipc.IsSelected = false;
				foreach(GroupControl gc in GroupListPanel.Controls)
					gc.IsSelected = false;
			}
			else
			{
				SortIPs();
			}
		}
		
		void SelectAllGroupsButtonClick(object sender, EventArgs e)
		{
			if (GroupListPanel.Controls.Count == 0)
				return;
			
			bool selected = !(GroupListPanel.Controls[0] as GroupControl).IsSelected;
			
			foreach(GroupControl gc in GroupListPanel.Controls)
				gc.IsSelected = selected;
			
			UpdateIPSelectionsFromGroups();
		}
		
		void InvertGroupSelectionButtonClick(object sender, EventArgs e)
		{
			foreach(GroupControl gc in GroupListPanel.Controls)
				gc.IsSelected = !gc.IsSelected;
			
			UpdateIPSelectionsFromGroups();
		}
		
		void RemoveAllGroupsButtonClick(object sender, EventArgs e)
		{
			for (int i=GroupListPanel.Controls.Count-1; i>=0; i--)
				RemoveGroup((GroupListPanel.Controls[i] as GroupControl).GroupName);
		}
		#endregion IPGroups
		
		#region commands
		private void WatchLogCB(object Sender, ProgressChangedEventArgs e)
		{
			FHost.Log(TLogType.Warning, (string) e.UserState);
		}
		
		private string ExecutePsToolCommand(TPsToolCommand Command, string Host)
		{
			if (PsToolsUsername.Text == "")
				return "Error: Username for remote PC is not specified. See Settings!";
			if (PsToolsPassword.Text == "")
				return "Error: Password for remote PC is not specified. See Settings!";
			if (FPsToolsPath == "")
				return "Error: Path to PsTools is not specified. See Settings!";
			
			string filename = FPsToolsPath + "\\";
			string workingdir = "";
			string arguments = "\\\\" + Host + " -u " + PsToolsUsername.Text + " -p " + PsToolsPassword.Text;
			ProcessControl pc;
			
			switch (Command)
			{
				case TPsToolCommand.Execute:
					{
						filename += "psexec.exe";
						pc = PsToolsProcessPanel.Controls[RemoteProcessPathComboBox.SelectedIndex] as ProcessControl;
						arguments += " -i 0 -d \"" + pc.Process + "\" " + pc.Arguments;
						workingdir = System.IO.Path.GetDirectoryName(pc.Process);
						break;
					}
				case TPsToolCommand.Kill:
					{
						filename += "pskill.exe";
						pc = PsToolsProcessPanel.Controls[RemoteProcessPathComboBox.SelectedIndex] as ProcessControl;
						arguments += " " + System.IO.Path.GetFileNameWithoutExtension(pc.Process);
						break;
					}
				case TPsToolCommand.Watch:
					{
						filename += "pslist.exe";
						pc = PsToolsProcessPanel.Controls[FWatchProcessID] as ProcessControl;
						arguments += " -m " + System.IO.Path.GetFileNameWithoutExtension(pc.Process);
						break;
					}
				case TPsToolCommand.WatchExecute:
					{
						filename += "psexec.exe";
						pc = PsToolsProcessPanel.Controls[FWatchProcessID] as ProcessControl;
						arguments += " -i 0 -d \"" + pc.Process + "\" " + pc.Arguments;
						workingdir = System.IO.Path.GetDirectoryName(pc.Process);
						break;
					}
				case TPsToolCommand.Reboot:
					{
						filename += "psshutdown.exe";
						arguments += " -f -r -t 0";
						break;
					}
				case TPsToolCommand.Shutdown:
					{
						filename += "psshutdown.exe";
						arguments += " -f -s -t 0";
						break;
					}
			}
			
			string result = "";
			if (System.IO.File.Exists(filename))
				result = Execute(filename, workingdir, arguments, true, false);
			else
				result = "Error: File not found: " + filename;
			
			return result;
		}
		
		private string Execute(string Filename, string WorkingDir, string Arguments, bool RedirectStandardOutput, bool OutputImmediately)
		{
			Process proc = new Process();
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = RedirectStandardOutput;
			proc.StartInfo.RedirectStandardError = RedirectStandardOutput;
			proc.StartInfo.UseShellExecute = false;
			//proc.EnableRaisingEvents = true;
			proc.StartInfo.FileName = Filename;
			proc.StartInfo.Arguments = Arguments;
			proc.StartInfo.WorkingDirectory = WorkingDir;
			if (OutputImmediately)
			{
				proc.OutputDataReceived += new DataReceivedEventHandler(ConsoleOutputHandler);
				proc.ErrorDataReceived += new DataReceivedEventHandler(ConsoleOutputHandler);
				//proc.o += new DataReceivedEventHandler(ConsoleOutputHandler);
			}
			
			string result ="";
			try
			{
				proc.Start();
				
				//this would block Remoter while a VNC viewer is running
				if (RedirectStandardOutput)
				{
					//
					if (OutputImmediately)
					{
						proc.BeginOutputReadLine();
						proc.BeginErrorReadLine();
					}
					else
					{	
						System.IO.StreamReader sOut;
						sOut = proc.StandardOutput;
						do
						{
							if (!OutputImmediately)
								result += sOut.ReadLine() + "\n";
						}
						while(!sOut.EndOfStream); //!proc.HasExited);
						sOut.Close();
					}
				}
				
				proc.Close();
			}
			catch (Exception e)
			{
				result = "Error executing: " + Filename + ": " + e.Message;
			}
			
			return result;
		}
		
		private void ConsoleOutputHandler(object sendingProcess,
		                                         DataReceivedEventArgs outLine)
		{
			FHost.Log(TLogType.Message, outLine.Data);
		}
		
		private void StartProcess()
		{
			//also tried to start process remotely via WMI
			//but it turns out processes started like this cannot show their gui
			//at least not without an additional hack:
			//http://social.msdn.microsoft.com/Forums/en-US/Vsexpressvb/thread/9a5a7884-eeab-4784-bcba-05348719a55c/
			//so for now stay with psexec.exe
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
			{
				string result = ExecutePsToolCommand(TPsToolCommand.Execute, ipc.IP);
				if (string.IsNullOrEmpty(result))
					FHost.Log(TLogType.Error, result);
			}
		}
		
		private void KillProcess()
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
			{
				string result = ExecutePsToolCommand(TPsToolCommand.Kill, ipc.IP);
				
				if (result != "")
					FHost.Log(TLogType.Error, result);
			}
		}
		
		void StartButtonClick(object sender, EventArgs e)
		{
			StartProcess();
		}
		
		void RestartButtonClick(object sender, EventArgs e)
		{
			KillProcess();
			StartProcess();
		}
		
		void KillButtonClick(object sender, EventArgs e)
		{
			KillProcess();
		}
		
		void RebootButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
				ExecutePsToolCommand(TPsToolCommand.Reboot, ipc.IP);
		}
		
		void ShutdownButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
				ExecutePsToolCommand(TPsToolCommand.Shutdown, ipc.IP);
		}
		
		void MirrorButtonClick(object sender, EventArgs e)
		{
			string arguments, ignorepattern = "";
			
			ignorepattern = IgnorePattern.Text.TrimEnd(";".ToCharArray());
			
			string[] ignores = ignorepattern.Split(';');
			ignorepattern = "";
			for (int i=0; i<ignores.Length; i++)
				if (!string.IsNullOrEmpty(ignores[i]))
				ignorepattern += " -if=" + ignores[i].Trim();
			
			string testonly = "";
			if (MirrorTestCheckBox.Checked)
				testonly = " -d";
			
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
			{
				arguments = "\"" + SourcePath.Text + "\" \"\\\\" + ipc.IP + TargetPath.Text + "\"" + ignorepattern + testonly + " -sa";
				Execute(FMirrorPath + "\\mirror.exe", FMirrorPath, arguments, true, true);
			}
		}
		
		void WakeOnLanButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				if (ipc.IsSelected)
			{
				ipc.WakeOnLan();
			}
		}

		void MACAddressButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				ipc.UpdateMACAddress();
			
			UpdateIPListInput();
		}
		
		void WatchModeClick(object sender, EventArgs e)
		{
			if (sender == WatchModeOff)
				FWatchMode = TWatchMode.Off;
			else if (sender == WatchModeRestart)
				FWatchMode = TWatchMode.Restart;
			else if (sender == WatchModeReboot)
				FWatchMode = TWatchMode.Reboot;
			
			SaveSettings();
		}
		
		void OnlineWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while(true)
			{
				if (IPListPanel.Controls.Count > 0)
				{
					FOnlineCheckID = (FOnlineCheckID + 1) % IPListPanel.Controls.Count;
					IPControl ipc = IPListPanel.Controls[FOnlineCheckID] as IPControl;
					ipc.UpdateOnlineState();
					
					switch(FIPCLiveUpdate)
					{
						case TIPCLiveUpdate.AllOffline:
							{
								ipc.Visible = !ipc.IsOnline;
								break;
							}
						case TIPCLiveUpdate.AllOnline:
							{
								ipc.Visible = ipc.IsOnline;
								break;
							}
					}
				}
				else
					System.Threading.Thread.Sleep(500);
				
				UpdateGroupsOnlineState();
			}
		}
		
		private void UpdateGroupsOnlineState()
		{
			foreach(GroupControl gc in GroupListPanel.Controls)
			{
				bool online = gc.IPControls.Count > 0;
				foreach(IPControl ipc in gc.IPControls)
					online &= ipc.IsOnline;
				
				gc.IsOnline = online;
			}
		}
		
		void WatchWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while(true)
			{
				if ((IPListPanel.Controls.Count > 0) && (WatchProcessCheckbox.Checked))
				{
					FWatchCheckID = (FWatchCheckID + 1) % IPListPanel.Controls.Count;
					
					IPControl ipc = (IPListPanel.Controls[FWatchCheckID] as IPControl);
					
					if (ipc.NeedsWatchUpdate() && (ipc.IsSelected) && (ipc.IsOnline))
					{
						//if expect ping is on
						
						//else
						//for local processes use: Process.Responding
						if (ipc.IsLocalHost)
						{
							if (!ipc.LocalProcessIsResponding(WatchProcessPathComboBox.Text))
							{
								if (WatchModeRestart.Checked)
									ExecutePsToolCommand(TPsToolCommand.WatchExecute, ipc.IP);
								else if (WatchModeReboot.Checked)
									ExecutePsToolCommand(TPsToolCommand.Reboot, ipc.IP);
							}
						}
						//for remote processes use:
						else
						{
							//this only detects processes that have vanished, not hanging ones.
							string result = ExecutePsToolCommand(TPsToolCommand.Watch, ipc.IP);
							if (result.Contains("Error: "))
							{
								ipc.AppIsOnline = false;
								WatchWorker.ReportProgress(0, result);
							}
							else if (result.Contains("process " + System.IO.Path.GetFileNameWithoutExtension(FWatchProcessName) + " was not found"))
							{
								ipc.AppIsOnline = false;
								
								if (WatchModeRestart.Checked)
									ExecutePsToolCommand(TPsToolCommand.WatchExecute, ipc.IP);
								else if (WatchModeReboot.Checked)
									ExecutePsToolCommand(TPsToolCommand.Reboot, ipc.IP);
							}
							else if (result.Contains("Failed"))
							{
								ipc.AppIsOnline = false;
								WatchWorker.ReportProgress(0, "Failed to watch remote process on: " + ipc.IP);
							}
							else
								ipc.AppIsOnline = true;
						}
					}
				}
				else
					System.Threading.Thread.Sleep(500);
				
				UpdateGroupsAppOnlineState();
			}
		}
		
		private void UpdateGroupsAppOnlineState()
		{
			foreach(GroupControl gc in GroupListPanel.Controls)
			{
				bool online = gc.IPControls.Count > 0;
				foreach(IPControl ipc in gc.IPControls)
					online &= ipc.AppIsOnline;
				
				gc.AppIsOnline = online;
			}
		}
		#endregion commands
		
		#region settings
		void PsToolsPathButtonClick(object sender, EventArgs e)
		{
			FFolderBrowserDialog.ShowDialog();
			FPsToolsPath = FFolderBrowserDialog.SelectedPath;
			PsToolsPathLabel.Text = FPsToolsPath;
			
			SaveSettings();
		}
		
		void VNCPathButtonClick(object sender, EventArgs e)
		{
			FFolderBrowserDialog.ShowDialog();
			FVNCPath = FFolderBrowserDialog.SelectedPath;
			VNCPathLabel.Text = FVNCPath;
			
			SaveSettings();
		}
		
		void MirrorPathButtonClick(object sender, EventArgs e)
		{
			FFolderBrowserDialog.ShowDialog();
			FMirrorPath = FFolderBrowserDialog.SelectedPath;
			MirrorPathLabel.Text = FMirrorPath;
			
			SaveSettings();
		}
		
		void SettingsChanged(object sender, EventArgs e)
		{
			if (!FLoading)
				SaveSettings();
		}
		
		void MirrorTestCheckBoxClick(object sender, EventArgs e)
		{
			SaveSettings();
		}
		
		private void LoadSettings(string Settings)
		{
			XmlNode tool;
			XmlNodeList processes;
			XmlAttribute attr;
			
			try
			{
				FSettings.LoadXml(Settings);
				//pstools
				tool = FSettings.SelectSingleNode(@"REMOTER/PSTOOLS");
				attr = tool.Attributes.GetNamedItem("Path") as XmlAttribute;
				PsToolsPathLabel.Text = attr.Value;
				FPsToolsPath = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("Username") as XmlAttribute;
				PsToolsUsername.Text = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("Password") as XmlAttribute;
				PsToolsPassword.Text = attr.Value;
				
				FSettings.LoadXml(Settings); //not sure why need to load here again
				//vnc
				processes = FSettings.SelectNodes(@"REMOTER/PSTOOLS/PROCESS");
				string path, arguments;
				foreach (XmlNode process in processes)
				{
					attr = process.Attributes.GetNamedItem("Path") as XmlAttribute;
					path = attr.Value;
					
					attr = process.Attributes.GetNamedItem("Arguments") as XmlAttribute;
					arguments = attr.Value;
					
					AddProcess(path, arguments);
				}
				UpdateProcessComboBoxes();

				FSettings.LoadXml(Settings); //not sure why need to load here again
				//vnc
				tool = FSettings.SelectSingleNode(@"REMOTER/VNC");
				attr = tool.Attributes.GetNamedItem("Path") as XmlAttribute;
				VNCPathLabel.Text = attr.Value;
				FVNCPath = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("Password") as XmlAttribute;
				VNCPassword.Text = attr.Value;
				
				FSettings.LoadXml(Settings); //not sure why need to load here again
				//mirror
				tool = FSettings.SelectSingleNode(@"REMOTER/MIRROR");
				attr = tool.Attributes.GetNamedItem("Path") as XmlAttribute;
				MirrorPathLabel.Text = attr.Value;
				FMirrorPath = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("SourcePath") as XmlAttribute;
				SourcePath.Text = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("TargetPath") as XmlAttribute;
				TargetPath.Text = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("IgnorePattern") as XmlAttribute;
				IgnorePattern.Text = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("TestOnly") as XmlAttribute;
				if (attr != null)
					MirrorTestCheckBox.Checked = bool.Parse(attr.Value);
				
				FSettings.LoadXml(Settings); //not sure why need to load here again
				
				//commands
				tool = FSettings.SelectSingleNode(@"REMOTER/COMMANDS");
				attr = tool.Attributes.GetNamedItem("RemoteProcessPath") as XmlAttribute;
				RemoteProcessPathComboBox.SelectedIndex = int.Parse(attr.Value);
				
				attr = tool.Attributes.GetNamedItem("WatchMode") as XmlAttribute;
				FWatchMode = (TWatchMode) Enum.Parse(typeof(TWatchMode), attr.Value);
				switch (FWatchMode)
				{
						case TWatchMode.Off: WatchModeOff.Checked = true; break;
						case TWatchMode.Restart: WatchModeRestart.Checked = true; break;
						case TWatchMode.Reboot: WatchModeReboot.Checked = true; break;
				}
				
				attr = tool.Attributes.GetNamedItem("WatchProcessPath") as XmlAttribute;
				FWatchProcessID = int.Parse(attr.Value);
				WatchProcessPathComboBox.SelectedIndex = FWatchProcessID;
				ProcessControl pc = PsToolsProcessPanel.Controls[FWatchProcessID] as ProcessControl;
				FWatchProcessName = pc.Process;
				
				//simulator
				FSettings.LoadXml(Settings); //not sure why need to load here again

				tool = FSettings.SelectSingleNode(@"REMOTER/SIMULATOR");
				attr = tool.Attributes.GetNamedItem("TargetIP") as XmlAttribute;
				SimulatorIPEdit.Text = attr.Value;
				
				attr = tool.Attributes.GetNamedItem("TargetPort") as XmlAttribute;
				SimulatorPortUpDown.Value = Convert.ToDecimal(attr.Value);
				
				attr = tool.Attributes.GetNamedItem("Commands") as XmlAttribute;
				char[] s = new char[1];
				s[0] = ';';
				string[] commands = attr.Value.Split(s);
				SimulatorListBox.Items.Clear();
				SimulatorListBox.Items.AddRange(commands);
			}
			catch
			{
				FHost.Log(TLogType.Warning, "Failed loading Remoter settings.");
			}
		}
		
		private void SaveSettings()
		{
			//gather all settings as one XML string
			XmlNode main, tool, process;
			XmlAttribute attr;
			
			FSettings.RemoveAll();
			main = FSettings.CreateElement("REMOTER");
			//pstools
			tool = FSettings.CreateElement("PSTOOLS");
			main.AppendChild(tool);
			attr = FSettings.CreateAttribute("Path");
			attr.Value = PsToolsPathLabel.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("Username");
			attr.Value = PsToolsUsername.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("Password");
			attr.Value = PsToolsPassword.Text;
			tool.Attributes.Append(attr);
			
			foreach(ProcessControl pc in PsToolsProcessPanel.Controls)
			{
				process = FSettings.CreateElement("PROCESS");
				tool.AppendChild(process);
				
				attr = FSettings.CreateAttribute("Path");
				attr.Value = pc.Process;
				process.Attributes.Append(attr);
				
				attr = FSettings.CreateAttribute("Arguments");
				attr.Value = pc.Arguments;
				process.Attributes.Append(attr);
			}
			
			//vnc
			tool = FSettings.CreateElement("VNC");
			main.AppendChild(tool);
			attr = FSettings.CreateAttribute("Path");
			attr.Value = VNCPathLabel.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("Password");
			attr.Value = VNCPassword.Text;
			tool.Attributes.Append(attr);
			
			//mirror
			tool = FSettings.CreateElement("MIRROR");
			main.AppendChild(tool);
			attr = FSettings.CreateAttribute("Path");
			attr.Value = MirrorPathLabel.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("SourcePath");
			attr.Value = SourcePath.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("TargetPath");
			attr.Value = TargetPath.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("IgnorePattern");
			attr.Value = IgnorePattern.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("TestOnly");
			attr.Value = MirrorTestCheckBox.Checked.ToString();
			tool.Attributes.Append(attr);

			//remoting
			
			//commands
			tool = FSettings.CreateElement("COMMANDS");
			main.AppendChild(tool);
			attr = FSettings.CreateAttribute("RemoteProcessPath");
			attr.Value = RemoteProcessPathComboBox.SelectedIndex.ToString();
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("WatchMode");
			attr.Value = FWatchMode.ToString();
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("WatchProcessPath");
			FWatchProcessID = Math.Max(0, WatchProcessPathComboBox.SelectedIndex);
			ProcessControl pcc = PsToolsProcessPanel.Controls[FWatchProcessID] as ProcessControl;
			FWatchProcessName = pcc.Process;
			attr.Value = FWatchProcessID.ToString();
			tool.Attributes.Append(attr);
			
			//simulator
			tool = FSettings.CreateElement("SIMULATOR");
			main.AppendChild(tool);
			attr = FSettings.CreateAttribute("TargetIP");
			attr.Value = SimulatorIPEdit.Text;
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("TargetPort");
			attr.Value = SimulatorPortUpDown.Value.ToString();
			tool.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("Commands");
			string commands = "";
			for (int i=0; i<SimulatorListBox.Items.Count; i++)
				commands += SimulatorListBox.Items[i] + ";";
			
			char[] s = new char[1];
			s[0] = ';';
			commands = commands.TrimEnd(s);
			attr.Value = commands;
			tool.Attributes.Append(attr);
			
			//write to settingspin
			FSettingsInput.SetString(0, main.OuterXml);
		}
		#endregion settings
		
		#region simulator
		void SimulatorListBoxMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				string msg = SimulatorListBox.Items[SimulatorListBox.SelectedIndex].ToString();
				SimulatorStringEdit.Text = msg;
				
				if (FTCPStream != null)
				{
					char[] chars = msg.ToCharArray();
					byte[] bytes = new byte[chars.Length];
					for (int i=0; i<bytes.Length; i++)
						bytes[i] = Convert.ToByte(chars[i]);
					
					FTCPStream.Write(bytes, 0, bytes.Length);
				}
			}
			else if (e.Button == MouseButtons.Right)
				if (SimulatorListBox.SelectedIndex > -1)
				SimulatorListBox.Items.RemoveAt(SimulatorListBox.SelectedIndex);
		}
		
		private void UpdateSimulatorConnection()
		{
			IPAddress target;
			IPAddress.TryParse(SimulatorIPEdit.Text, out target);
			
			if (target != null)
			{
				if (SimulatorTCPCheckBox.Checked)
				{
					if (FTCPClient != null)
					{
						FTCPStream.Close();
						FTCPStream = null;
						FTCPClient.Close();
						FTCPClient = null;
					}
					
					FTCPClient = new System.Net.Sockets.TcpClient();
					FTCPClient.Connect(SimulatorIPEdit.Text, (int) SimulatorPortUpDown.Value);
					FTCPStream = FTCPClient.GetStream();
				}
			}
		}
		
		void AddSimulatorStringButtonClick(object sender, EventArgs e)
		{
			AddSimulatorString();
		}
		
		private void AddSimulatorString()
		{
			SimulatorListBox.Items.Add(SimulatorStringEdit.Text);
			SimulatorStringEdit.Text = "";
			SaveSettings();
		}
		
		void SimulatorConnectButtonClick(object sender, EventArgs e)
		{
			UpdateSimulatorConnection();
		}
		
		void SimulatorPortUpDownValueChanged(object sender, EventArgs e)
		{
			SaveSettings();
		}
		
		void SimulatorIPEditTextChanged(object sender, EventArgs e)
		{
			SaveSettings();
		}
		
		void SimulatorStringEditKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) 13)
				AddSimulatorString();
		}
		#endregion simulator
		
		void AddProcessButtonClick(object sender, EventArgs e)
		{
			AddProcess("", "");
		}
		
		private void AddProcess(string Path, string Arguments)
		{
			ProcessControl pc = new ProcessControl();
			pc.Process = Path;
			pc.Arguments = Arguments;
			
			pc.Dock = DockStyle.Top;
			pc.OnXButton += new ButtonUpHandler(ProcessXButtonHandlerCB);
			pc.OnProcessChanged += new ProcessChangedHandler(ProcessChangedHandlerCB);
			PsToolsProcessPanel.Controls.Add(pc);
			
			pc.Focus();
			
			AdaptPsToolsProcessPanelHeight();
		}
		
		private void AdaptPsToolsProcessPanelHeight()
		{
			PsToolsProcessPanel.Height = PsToolsProcessPanel.Controls.Count * 20;
			PsToolsBox.Height = PsToolsProcessPanel.Height + 85;
		}
		
		private void ProcessXButtonHandlerCB(UserControl Control)
		{
			PsToolsProcessPanel.Controls.Remove(Control);
			AdaptPsToolsProcessPanelHeight();
			
			SaveSettings();
		}
		
		private void UpdateProcessComboBoxes()
		{
			string process;
			WatchProcessPathComboBox.Items.Clear();
			RemoteProcessPathComboBox.Items.Clear();
			foreach(ProcessControl pc in PsToolsProcessPanel.Controls)
			{
				WatchProcessPathComboBox.Items.Add(pc.Process + " " + pc.Arguments);
				RemoteProcessPathComboBox.Items.Add(pc.Process + " " + pc.Arguments);
			}
		}
		
		private void ProcessChangedHandlerCB()
		{
			UpdateProcessComboBoxes();
			
			SaveSettings();
		}
		
		void VNCButtonClick(object sender, EventArgs e)
		{
			string filename = FVNCPath + "\\vncviewer.exe";
			if (System.IO.File.Exists(filename))
				Execute(filename, FVNCPath, NewIPEdit.Text + " /password " + VNCPassword.Text, false, false);
		}
	}

	public delegate void ButtonHandler(string IP);
	public delegate void ButtonUpHandler(UserControl Control);
	public delegate void GroupChangedHandler(GroupControl Group, string OldGroupName, List<string> IPs);
}
