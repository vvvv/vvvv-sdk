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
 * replace IPControls with ordinary ListView
 * consistent log-messages
 * expect ping (for watchdog)
 * remoting of RemoterSA
 * log to disk
 * email notification of failures
 * allow processes to be started with processaffinity
 * show local ip(s)
 * monitoring (cpu, memory, running time...)
 * mediensteuerungs simulation: connects to port/ip udp or tcp and can send strings
 */

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition, inheriting from UserControl for the GUI stuff
	public class Remoter: UserControl, IPlugin
	{
		public enum TIPCLiveUpdate {Off, AllOnline, AllOffline};
		
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//input pin declaration
		private IStringConfig FSettingsInput;
		private IStringConfig FIPListInput;
		
		//output pin declaration
		private IValueOut FOnlineOutput;
		private IValueOut FAppIsOnlineOutput;
		
		//further fields
		private const string UNGROUPED = "ungrouped";
		private string FPsToolsPath;
		private string FVNCPath;
		private string FMirrorPath;
		private int FOnlineCheckID = 0;
		private bool FLoading = true;

		private List<IPControl> FIPSortList = new List<IPControl>();
		private BindingList<ProcessControl> FProcesses = new BindingList<ProcessControl>();
		private BindingList<string> FGroups = new BindingList<string>();
		
		private List<TaskControl> FTasks = new List<TaskControl>();
		private VVVV.Nodes.Remoter.TIPCLiveUpdate FIPCLiveUpdate;
		
		private System.Net.Sockets.TcpClient FTCPClient;
		private System.Net.Sockets.NetworkStream FTCPStream;
		
		private XmlDocument FSettings;
		private int FSplitterDistance = 300;
		
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
			
			RemoteProcessPathDrop.DataSource = FProcesses;
			RemoteProcessPathDrop.DisplayMember = "ProcessAndArguments";
			
			GroupFilterDropDown.DataSource = FGroups;
			FGroups.Add(".All");
			FGroups.Add(".Online");
			FGroups.Add(".Offline");
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
			this.OnlineWorker = new System.ComponentModel.BackgroundWorker();
			this.WatchWorker = new System.ComponentModel.BackgroundWorker();
			this.SplitContainer = new System.Windows.Forms.SplitContainer();
			this.LeftTabControl = new System.Windows.Forms.TabControl();
			this.IPPage = new System.Windows.Forms.TabPage();
			this.IPListPanel = new System.Windows.Forms.Panel();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.ClearSelectionButton = new System.Windows.Forms.Button();
			this.InvertSelectionButton = new System.Windows.Forms.Button();
			this.AddVisibleToSelectionButton = new System.Windows.Forms.Button();
			this.RemoveVisibleFromSelectionButton = new System.Windows.Forms.Button();
			this.DeleteSelectedIPsButton = new System.Windows.Forms.Button();
			this.GroupFilterDropDown = new System.Windows.Forms.ComboBox();
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.SelectAllGroupsButton = new System.Windows.Forms.Button();
			this.RemoveAllGroupsButton = new System.Windows.Forms.Button();
			this.InvertGroupSelectionButton = new System.Windows.Forms.Button();
			this.ClearGroupSelectionButton = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.RightTabControl = new System.Windows.Forms.TabControl();
			this.CommandsPage = new System.Windows.Forms.TabPage();
			this.TasksPanel = new System.Windows.Forms.Panel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.RemoteProcessPathDrop = new System.Windows.Forms.ComboBox();
			this.TaskAddButton = new System.Windows.Forms.Button();
			this.TaskDescriptionEdit = new System.Windows.Forms.TextBox();
			this.MirrorTestCheckBox = new System.Windows.Forms.CheckBox();
			this.MirrorButton = new System.Windows.Forms.Button();
			this.ShutdownButton = new System.Windows.Forms.Button();
			this.RebootButton = new System.Windows.Forms.Button();
			this.WakeOnLanButton = new System.Windows.Forms.Button();
			this.MACAddressButton = new System.Windows.Forms.Button();
			this.KillButton = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.StartButton = new System.Windows.Forms.Button();
			this.SettingsPage = new System.Windows.Forms.TabPage();
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
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel13 = new System.Windows.Forms.Panel();
			this.panel14 = new System.Windows.Forms.Panel();
			this.button1 = new System.Windows.Forms.Button();
			this.panel15 = new System.Windows.Forms.Panel();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.panel16 = new System.Windows.Forms.Panel();
			this.button5 = new System.Windows.Forms.Button();
			this.panel17 = new System.Windows.Forms.Panel();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.LeftTabControl.SuspendLayout();
			this.IPPage.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.LeftTopPanel.SuspendLayout();
			this.GroupPage.SuspendLayout();
			this.panel9.SuspendLayout();
			this.panel8.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.CommandsPage.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SettingsPage.SuspendLayout();
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
			this.panel14.SuspendLayout();
			this.SuspendLayout();
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
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SplitContainer.Location = new System.Drawing.Point(0, 0);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.LeftTabControl);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.RightTabControl);
			this.SplitContainer.Size = new System.Drawing.Size(694, 491);
			this.SplitContainer.SplitterDistance = 409;
			this.SplitContainer.TabIndex = 22;
			this.SplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainerSplitterMoved);
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
			this.LeftTabControl.Size = new System.Drawing.Size(409, 491);
			this.LeftTabControl.TabIndex = 4;
			this.LeftTabControl.SelectedIndexChanged += new System.EventHandler(this.LeftTabControlSelectedIndexChanged);
			// 
			// IPPage
			// 
			this.IPPage.BackColor = System.Drawing.SystemColors.Control;
			this.IPPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.IPPage.Controls.Add(this.IPListPanel);
			this.IPPage.Controls.Add(this.tableLayoutPanel5);
			this.IPPage.Controls.Add(this.GroupFilterDropDown);
			this.IPPage.Controls.Add(this.LeftTopPanel);
			this.IPPage.Location = new System.Drawing.Point(4, 25);
			this.IPPage.Name = "IPPage";
			this.IPPage.Padding = new System.Windows.Forms.Padding(3);
			this.IPPage.Size = new System.Drawing.Size(401, 462);
			this.IPPage.TabIndex = 0;
			this.IPPage.Text = "IPs";
			// 
			// IPListPanel
			// 
			this.IPListPanel.AutoScroll = true;
			this.IPListPanel.BackColor = System.Drawing.SystemColors.Control;
			this.IPListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IPListPanel.Location = new System.Drawing.Point(3, 44);
			this.IPListPanel.Name = "IPListPanel";
			this.IPListPanel.Size = new System.Drawing.Size(393, 388);
			this.IPListPanel.TabIndex = 5;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 6;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.88812F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.24865F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.79679F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.88812F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.17833F));
			this.tableLayoutPanel5.Controls.Add(this.label5, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.ClearSelectionButton, 1, 0);
			this.tableLayoutPanel5.Controls.Add(this.InvertSelectionButton, 3, 0);
			this.tableLayoutPanel5.Controls.Add(this.AddVisibleToSelectionButton, 2, 0);
			this.tableLayoutPanel5.Controls.Add(this.RemoveVisibleFromSelectionButton, 3, 0);
			this.tableLayoutPanel5.Controls.Add(this.DeleteSelectedIPsButton, 5, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 432);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(393, 25);
			this.tableLayoutPanel5.TabIndex = 8;
			// 
			// label5
			// 
			this.label5.BackColor = System.Drawing.SystemColors.Control;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(0, 0);
			this.label5.Margin = new System.Windows.Forms.Padding(0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 25);
			this.label5.TabIndex = 8;
			this.label5.Text = "Selection";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ClearSelectionButton
			// 
			this.ClearSelectionButton.BackColor = System.Drawing.SystemColors.Control;
			this.ClearSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClearSelectionButton.Location = new System.Drawing.Point(55, 0);
			this.ClearSelectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.ClearSelectionButton.Name = "ClearSelectionButton";
			this.ClearSelectionButton.Size = new System.Drawing.Size(46, 25);
			this.ClearSelectionButton.TabIndex = 9;
			this.ClearSelectionButton.Text = "Clear";
			this.ClearSelectionButton.UseVisualStyleBackColor = false;
			this.ClearSelectionButton.Click += new System.EventHandler(this.ClearSelectionButtonClick);
			// 
			// InvertSelectionButton
			// 
			this.InvertSelectionButton.BackColor = System.Drawing.SystemColors.Control;
			this.InvertSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvertSelectionButton.Location = new System.Drawing.Point(292, 0);
			this.InvertSelectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.InvertSelectionButton.Name = "InvertSelectionButton";
			this.InvertSelectionButton.Size = new System.Drawing.Size(46, 25);
			this.InvertSelectionButton.TabIndex = 12;
			this.InvertSelectionButton.Text = "Invert";
			this.InvertSelectionButton.UseVisualStyleBackColor = false;
			this.InvertSelectionButton.Click += new System.EventHandler(this.InvertSelectionButtonClick);
			// 
			// AddVisibleToSelectionButton
			// 
			this.AddVisibleToSelectionButton.BackColor = System.Drawing.SystemColors.Control;
			this.AddVisibleToSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddVisibleToSelectionButton.Location = new System.Drawing.Point(101, 0);
			this.AddVisibleToSelectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.AddVisibleToSelectionButton.Name = "AddVisibleToSelectionButton";
			this.AddVisibleToSelectionButton.Size = new System.Drawing.Size(81, 25);
			this.AddVisibleToSelectionButton.TabIndex = 10;
			this.AddVisibleToSelectionButton.Text = "Add Visible";
			this.AddVisibleToSelectionButton.UseVisualStyleBackColor = false;
			this.AddVisibleToSelectionButton.Click += new System.EventHandler(this.AddVisibleToSelectionButtonClick);
			// 
			// RemoveVisibleFromSelectionButton
			// 
			this.RemoveVisibleFromSelectionButton.BackColor = System.Drawing.SystemColors.Control;
			this.RemoveVisibleFromSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemoveVisibleFromSelectionButton.Location = new System.Drawing.Point(182, 0);
			this.RemoveVisibleFromSelectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.RemoveVisibleFromSelectionButton.Name = "RemoveVisibleFromSelectionButton";
			this.RemoveVisibleFromSelectionButton.Size = new System.Drawing.Size(110, 25);
			this.RemoveVisibleFromSelectionButton.TabIndex = 11;
			this.RemoveVisibleFromSelectionButton.Text = "Remove Visible";
			this.RemoveVisibleFromSelectionButton.UseVisualStyleBackColor = false;
			this.RemoveVisibleFromSelectionButton.Click += new System.EventHandler(this.RemoveVisibleFromSelectionButtonClick);
			// 
			// DeleteSelectedIPsButton
			// 
			this.DeleteSelectedIPsButton.BackColor = System.Drawing.SystemColors.Control;
			this.DeleteSelectedIPsButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeleteSelectedIPsButton.Location = new System.Drawing.Point(338, 0);
			this.DeleteSelectedIPsButton.Margin = new System.Windows.Forms.Padding(0);
			this.DeleteSelectedIPsButton.Name = "DeleteSelectedIPsButton";
			this.DeleteSelectedIPsButton.Size = new System.Drawing.Size(55, 25);
			this.DeleteSelectedIPsButton.TabIndex = 13;
			this.DeleteSelectedIPsButton.Text = "Delete";
			this.DeleteSelectedIPsButton.UseVisualStyleBackColor = false;
			this.DeleteSelectedIPsButton.Click += new System.EventHandler(this.DeleteButtonClick);
			// 
			// GroupFilterDropDown
			// 
			this.GroupFilterDropDown.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupFilterDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.GroupFilterDropDown.FormattingEnabled = true;
			this.GroupFilterDropDown.Items.AddRange(new object[] {
									".All",
									".Offline",
									".Online"});
			this.GroupFilterDropDown.Location = new System.Drawing.Point(3, 23);
			this.GroupFilterDropDown.Name = "GroupFilterDropDown";
			this.GroupFilterDropDown.Size = new System.Drawing.Size(393, 21);
			this.GroupFilterDropDown.Sorted = true;
			this.GroupFilterDropDown.TabIndex = 6;
			this.GroupFilterDropDown.SelectedIndexChanged += new System.EventHandler(this.GroupFilterDropDownSelectedIndexChanged);
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Controls.Add(this.NewIPEdit);
			this.LeftTopPanel.Controls.Add(this.AddIPButton);
			this.LeftTopPanel.Controls.Add(this.VNCButton);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LeftTopPanel.Location = new System.Drawing.Point(3, 3);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = new System.Drawing.Size(393, 20);
			this.LeftTopPanel.TabIndex = 1;
			// 
			// NewIPEdit
			// 
			this.NewIPEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewIPEdit.Location = new System.Drawing.Point(0, 0);
			this.NewIPEdit.Name = "NewIPEdit";
			this.NewIPEdit.Size = new System.Drawing.Size(331, 20);
			this.NewIPEdit.TabIndex = 0;
			this.NewIPEdit.Text = "192.168.0.1";
			this.NewIPEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NewIPEditKeyPress);
			// 
			// AddIPButton
			// 
			this.AddIPButton.BackColor = System.Drawing.SystemColors.Control;
			this.AddIPButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddIPButton.Location = new System.Drawing.Point(331, 0);
			this.AddIPButton.Name = "AddIPButton";
			this.AddIPButton.Size = new System.Drawing.Size(20, 20);
			this.AddIPButton.TabIndex = 1;
			this.AddIPButton.Text = "+";
			this.AddIPButton.UseVisualStyleBackColor = false;
			this.AddIPButton.Click += new System.EventHandler(this.AddIPButtonClick);
			// 
			// VNCButton
			// 
			this.VNCButton.BackColor = System.Drawing.SystemColors.Control;
			this.VNCButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.VNCButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VNCButton.Location = new System.Drawing.Point(351, 0);
			this.VNCButton.Name = "VNCButton";
			this.VNCButton.Size = new System.Drawing.Size(42, 20);
			this.VNCButton.TabIndex = 2;
			this.VNCButton.Text = "VNC";
			this.VNCButton.UseVisualStyleBackColor = false;
			this.VNCButton.Click += new System.EventHandler(this.VNCButtonClick);
			// 
			// GroupPage
			// 
			this.GroupPage.BackColor = System.Drawing.SystemColors.Control;
			this.GroupPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GroupPage.Controls.Add(this.GroupListPanel);
			this.GroupPage.Controls.Add(this.panel9);
			this.GroupPage.Controls.Add(this.panel8);
			this.GroupPage.Location = new System.Drawing.Point(4, 25);
			this.GroupPage.Name = "GroupPage";
			this.GroupPage.Padding = new System.Windows.Forms.Padding(3);
			this.GroupPage.Size = new System.Drawing.Size(401, 462);
			this.GroupPage.TabIndex = 1;
			this.GroupPage.Text = "Groups";
			// 
			// GroupListPanel
			// 
			this.GroupListPanel.AutoScroll = true;
			this.GroupListPanel.BackColor = System.Drawing.SystemColors.Control;
			this.GroupListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupListPanel.Location = new System.Drawing.Point(3, 23);
			this.GroupListPanel.Name = "GroupListPanel";
			this.GroupListPanel.Size = new System.Drawing.Size(393, 409);
			this.GroupListPanel.TabIndex = 6;
			// 
			// panel9
			// 
			this.panel9.BackColor = System.Drawing.SystemColors.Control;
			this.panel9.Controls.Add(this.NewGroupEdit);
			this.panel9.Controls.Add(this.AddGroupButton);
			this.panel9.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel9.Location = new System.Drawing.Point(3, 3);
			this.panel9.Name = "panel9";
			this.panel9.Size = new System.Drawing.Size(393, 20);
			this.panel9.TabIndex = 5;
			// 
			// NewGroupEdit
			// 
			this.NewGroupEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewGroupEdit.Location = new System.Drawing.Point(0, 0);
			this.NewGroupEdit.Name = "NewGroupEdit";
			this.NewGroupEdit.Size = new System.Drawing.Size(373, 20);
			this.NewGroupEdit.TabIndex = 2;
			this.NewGroupEdit.Text = "N\'Sync";
			this.NewGroupEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NewIPEditKeyPress);
			// 
			// AddGroupButton
			// 
			this.AddGroupButton.BackColor = System.Drawing.SystemColors.Control;
			this.AddGroupButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddGroupButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AddGroupButton.Location = new System.Drawing.Point(373, 0);
			this.AddGroupButton.Name = "AddGroupButton";
			this.AddGroupButton.Size = new System.Drawing.Size(20, 20);
			this.AddGroupButton.TabIndex = 3;
			this.AddGroupButton.Text = "+";
			this.AddGroupButton.UseVisualStyleBackColor = false;
			this.AddGroupButton.Click += new System.EventHandler(this.AddGroupButtonClick);
			// 
			// panel8
			// 
			this.panel8.Controls.Add(this.tableLayoutPanel1);
			this.panel8.Controls.Add(this.label8);
			this.panel8.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel8.Location = new System.Drawing.Point(3, 432);
			this.panel8.Name = "panel8";
			this.panel8.Size = new System.Drawing.Size(393, 25);
			this.panel8.TabIndex = 4;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.Controls.Add(this.SelectAllGroupsButton, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.RemoveAllGroupsButton, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.InvertGroupSelectionButton, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.ClearGroupSelectionButton, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(55, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(338, 25);
			this.tableLayoutPanel1.TabIndex = 10;
			// 
			// SelectAllGroupsButton
			// 
			this.SelectAllGroupsButton.BackColor = System.Drawing.SystemColors.Control;
			this.SelectAllGroupsButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectAllGroupsButton.Location = new System.Drawing.Point(84, 0);
			this.SelectAllGroupsButton.Margin = new System.Windows.Forms.Padding(0);
			this.SelectAllGroupsButton.Name = "SelectAllGroupsButton";
			this.SelectAllGroupsButton.Size = new System.Drawing.Size(84, 25);
			this.SelectAllGroupsButton.TabIndex = 7;
			this.SelectAllGroupsButton.Text = "All";
			this.SelectAllGroupsButton.UseVisualStyleBackColor = false;
			this.SelectAllGroupsButton.Click += new System.EventHandler(this.SelectAllGroupsButtonClick);
			// 
			// RemoveAllGroupsButton
			// 
			this.RemoveAllGroupsButton.BackColor = System.Drawing.SystemColors.Control;
			this.RemoveAllGroupsButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemoveAllGroupsButton.Location = new System.Drawing.Point(252, 0);
			this.RemoveAllGroupsButton.Margin = new System.Windows.Forms.Padding(0);
			this.RemoveAllGroupsButton.Name = "RemoveAllGroupsButton";
			this.RemoveAllGroupsButton.Size = new System.Drawing.Size(86, 25);
			this.RemoveAllGroupsButton.TabIndex = 6;
			this.RemoveAllGroupsButton.Text = "Delete";
			this.RemoveAllGroupsButton.UseVisualStyleBackColor = false;
			this.RemoveAllGroupsButton.Click += new System.EventHandler(this.RemoveAllGroupsButtonClick);
			// 
			// InvertGroupSelectionButton
			// 
			this.InvertGroupSelectionButton.BackColor = System.Drawing.SystemColors.Control;
			this.InvertGroupSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvertGroupSelectionButton.Location = new System.Drawing.Point(168, 0);
			this.InvertGroupSelectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.InvertGroupSelectionButton.Name = "InvertGroupSelectionButton";
			this.InvertGroupSelectionButton.Size = new System.Drawing.Size(84, 25);
			this.InvertGroupSelectionButton.TabIndex = 5;
			this.InvertGroupSelectionButton.Text = "Invert";
			this.InvertGroupSelectionButton.UseVisualStyleBackColor = false;
			this.InvertGroupSelectionButton.Click += new System.EventHandler(this.InvertGroupSelectionButtonClick);
			// 
			// ClearGroupSelectionButton
			// 
			this.ClearGroupSelectionButton.BackColor = System.Drawing.SystemColors.Control;
			this.ClearGroupSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClearGroupSelectionButton.Location = new System.Drawing.Point(0, 0);
			this.ClearGroupSelectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.ClearGroupSelectionButton.Name = "ClearGroupSelectionButton";
			this.ClearGroupSelectionButton.Size = new System.Drawing.Size(84, 25);
			this.ClearGroupSelectionButton.TabIndex = 2;
			this.ClearGroupSelectionButton.Text = "Clear";
			this.ClearGroupSelectionButton.UseVisualStyleBackColor = false;
			this.ClearGroupSelectionButton.Click += new System.EventHandler(this.ClearGroupSelectionButtonClick);
			// 
			// label8
			// 
			this.label8.BackColor = System.Drawing.SystemColors.Control;
			this.label8.Dock = System.Windows.Forms.DockStyle.Left;
			this.label8.Location = new System.Drawing.Point(0, 0);
			this.label8.Margin = new System.Windows.Forms.Padding(0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(55, 25);
			this.label8.TabIndex = 9;
			this.label8.Text = "Selection";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.RightTabControl.Controls.Add(this.CommandsPage);
			this.RightTabControl.Controls.Add(this.SettingsPage);
			this.RightTabControl.Controls.Add(this.SimulatorPage);
			this.RightTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightTabControl.Location = new System.Drawing.Point(0, 0);
			this.RightTabControl.Name = "RightTabControl";
			this.RightTabControl.SelectedIndex = 0;
			this.RightTabControl.Size = new System.Drawing.Size(281, 491);
			this.RightTabControl.TabIndex = 0;
			// 
			// CommandsPage
			// 
			this.CommandsPage.BackColor = System.Drawing.SystemColors.Control;
			this.CommandsPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CommandsPage.Controls.Add(this.TasksPanel);
			this.CommandsPage.Controls.Add(this.tableLayoutPanel3);
			this.CommandsPage.Location = new System.Drawing.Point(4, 25);
			this.CommandsPage.Name = "CommandsPage";
			this.CommandsPage.Padding = new System.Windows.Forms.Padding(3);
			this.CommandsPage.Size = new System.Drawing.Size(273, 462);
			this.CommandsPage.TabIndex = 1;
			this.CommandsPage.Text = "Commands";
			// 
			// TasksPanel
			// 
			this.TasksPanel.BackColor = System.Drawing.SystemColors.Control;
			this.TasksPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TasksPanel.Location = new System.Drawing.Point(3, 180);
			this.TasksPanel.Name = "TasksPanel";
			this.TasksPanel.Size = new System.Drawing.Size(265, 277);
			this.TasksPanel.TabIndex = 0;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 3;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel3.Controls.Add(this.RemoteProcessPathDrop, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.TaskAddButton, 2, 5);
			this.tableLayoutPanel3.Controls.Add(this.TaskDescriptionEdit, 0, 5);
			this.tableLayoutPanel3.Controls.Add(this.MirrorTestCheckBox, 3, 4);
			this.tableLayoutPanel3.Controls.Add(this.MirrorButton, 0, 4);
			this.tableLayoutPanel3.Controls.Add(this.ShutdownButton, 2, 3);
			this.tableLayoutPanel3.Controls.Add(this.RebootButton, 1, 3);
			this.tableLayoutPanel3.Controls.Add(this.WakeOnLanButton, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.MACAddressButton, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.KillButton, 2, 1);
			this.tableLayoutPanel3.Controls.Add(this.button3, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.StartButton, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 6;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(265, 177);
			this.tableLayoutPanel3.TabIndex = 0;
			// 
			// RemoteProcessPathDrop
			// 
			this.tableLayoutPanel3.SetColumnSpan(this.RemoteProcessPathDrop, 3);
			this.RemoteProcessPathDrop.Dock = System.Windows.Forms.DockStyle.Top;
			this.RemoteProcessPathDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RemoteProcessPathDrop.Location = new System.Drawing.Point(3, 3);
			this.RemoteProcessPathDrop.Name = "RemoteProcessPathDrop";
			this.RemoteProcessPathDrop.Size = new System.Drawing.Size(259, 21);
			this.RemoteProcessPathDrop.TabIndex = 30;
			this.RemoteProcessPathDrop.SelectedIndexChanged += new System.EventHandler(this.SettingsChanged);
			// 
			// TaskAddButton
			// 
			this.TaskAddButton.BackColor = System.Drawing.SystemColors.Control;
			this.TaskAddButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaskAddButton.Location = new System.Drawing.Point(179, 153);
			this.TaskAddButton.Name = "TaskAddButton";
			this.TaskAddButton.Size = new System.Drawing.Size(83, 21);
			this.TaskAddButton.TabIndex = 29;
			this.TaskAddButton.Text = "Add Task";
			this.TaskAddButton.UseVisualStyleBackColor = false;
			this.TaskAddButton.Click += new System.EventHandler(this.TaskAddButtonClick);
			// 
			// TaskDescriptionEdit
			// 
			this.tableLayoutPanel3.SetColumnSpan(this.TaskDescriptionEdit, 2);
			this.TaskDescriptionEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaskDescriptionEdit.Location = new System.Drawing.Point(3, 153);
			this.TaskDescriptionEdit.Name = "TaskDescriptionEdit";
			this.TaskDescriptionEdit.Size = new System.Drawing.Size(170, 20);
			this.TaskDescriptionEdit.TabIndex = 28;
			// 
			// MirrorTestCheckBox
			// 
			this.MirrorTestCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.MirrorTestCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MirrorTestCheckBox.Location = new System.Drawing.Point(179, 123);
			this.MirrorTestCheckBox.Name = "MirrorTestCheckBox";
			this.MirrorTestCheckBox.Size = new System.Drawing.Size(83, 24);
			this.MirrorTestCheckBox.TabIndex = 27;
			this.MirrorTestCheckBox.Text = "Test Only";
			this.MirrorTestCheckBox.UseVisualStyleBackColor = false;
			this.MirrorTestCheckBox.Click += new System.EventHandler(this.MirrorTestCheckBoxClick);
			// 
			// MirrorButton
			// 
			this.MirrorButton.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel3.SetColumnSpan(this.MirrorButton, 2);
			this.MirrorButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MirrorButton.Location = new System.Drawing.Point(3, 123);
			this.MirrorButton.Name = "MirrorButton";
			this.MirrorButton.Size = new System.Drawing.Size(170, 24);
			this.MirrorButton.TabIndex = 26;
			this.MirrorButton.Text = "Mirror Now";
			this.MirrorButton.UseVisualStyleBackColor = false;
			this.MirrorButton.Click += new System.EventHandler(this.MirrorButtonClick);
			// 
			// ShutdownButton
			// 
			this.ShutdownButton.BackColor = System.Drawing.SystemColors.Control;
			this.ShutdownButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShutdownButton.Location = new System.Drawing.Point(179, 93);
			this.ShutdownButton.Name = "ShutdownButton";
			this.ShutdownButton.Size = new System.Drawing.Size(83, 24);
			this.ShutdownButton.TabIndex = 25;
			this.ShutdownButton.Text = "Shutdown";
			this.ShutdownButton.UseVisualStyleBackColor = false;
			this.ShutdownButton.Click += new System.EventHandler(this.ShutdownButtonClick);
			// 
			// RebootButton
			// 
			this.RebootButton.BackColor = System.Drawing.SystemColors.Control;
			this.RebootButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RebootButton.Location = new System.Drawing.Point(91, 93);
			this.RebootButton.Name = "RebootButton";
			this.RebootButton.Size = new System.Drawing.Size(82, 24);
			this.RebootButton.TabIndex = 24;
			this.RebootButton.Text = "Reboot";
			this.RebootButton.UseVisualStyleBackColor = false;
			this.RebootButton.Click += new System.EventHandler(this.RebootButtonClick);
			// 
			// WakeOnLanButton
			// 
			this.WakeOnLanButton.BackColor = System.Drawing.SystemColors.Control;
			this.WakeOnLanButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WakeOnLanButton.Location = new System.Drawing.Point(3, 93);
			this.WakeOnLanButton.Name = "WakeOnLanButton";
			this.WakeOnLanButton.Size = new System.Drawing.Size(82, 24);
			this.WakeOnLanButton.TabIndex = 23;
			this.WakeOnLanButton.Text = "WakeOnLan";
			this.WakeOnLanButton.UseVisualStyleBackColor = false;
			this.WakeOnLanButton.Click += new System.EventHandler(this.WakeOnLanButtonClick);
			// 
			// MACAddressButton
			// 
			this.MACAddressButton.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel3.SetColumnSpan(this.MACAddressButton, 3);
			this.MACAddressButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MACAddressButton.Location = new System.Drawing.Point(3, 63);
			this.MACAddressButton.Name = "MACAddressButton";
			this.MACAddressButton.Size = new System.Drawing.Size(259, 24);
			this.MACAddressButton.TabIndex = 21;
			this.MACAddressButton.Text = "Get a MAC + HostName";
			this.MACAddressButton.UseVisualStyleBackColor = false;
			this.MACAddressButton.Click += new System.EventHandler(this.MACAddressButtonClick);
			// 
			// KillButton
			// 
			this.KillButton.BackColor = System.Drawing.SystemColors.Control;
			this.KillButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.KillButton.Location = new System.Drawing.Point(179, 33);
			this.KillButton.Name = "KillButton";
			this.KillButton.Size = new System.Drawing.Size(83, 24);
			this.KillButton.TabIndex = 20;
			this.KillButton.Text = "Kill";
			this.KillButton.UseVisualStyleBackColor = false;
			this.KillButton.Click += new System.EventHandler(this.KillButtonClick);
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.SystemColors.Control;
			this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button3.Location = new System.Drawing.Point(91, 33);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(82, 24);
			this.button3.TabIndex = 18;
			this.button3.Text = "Restart";
			this.button3.UseVisualStyleBackColor = false;
			this.button3.Click += new System.EventHandler(this.RestartButtonClick);
			// 
			// StartButton
			// 
			this.StartButton.BackColor = System.Drawing.SystemColors.Control;
			this.StartButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StartButton.Location = new System.Drawing.Point(3, 33);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(82, 24);
			this.StartButton.TabIndex = 15;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = false;
			this.StartButton.Click += new System.EventHandler(this.StartButtonClick);
			// 
			// SettingsPage
			// 
			this.SettingsPage.AutoScroll = true;
			this.SettingsPage.BackColor = System.Drawing.SystemColors.Control;
			this.SettingsPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SettingsPage.Controls.Add(this.MirrorBox);
			this.SettingsPage.Controls.Add(this.VNCBox);
			this.SettingsPage.Controls.Add(this.PsToolsBox);
			this.SettingsPage.Location = new System.Drawing.Point(4, 25);
			this.SettingsPage.Name = "SettingsPage";
			this.SettingsPage.Padding = new System.Windows.Forms.Padding(3);
			this.SettingsPage.Size = new System.Drawing.Size(273, 462);
			this.SettingsPage.TabIndex = 0;
			this.SettingsPage.Text = "Settings";
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
			this.MirrorBox.Location = new System.Drawing.Point(3, 161);
			this.MirrorBox.Name = "MirrorBox";
			this.MirrorBox.Size = new System.Drawing.Size(265, 145);
			this.MirrorBox.TabIndex = 2;
			this.MirrorBox.TabStop = false;
			this.MirrorBox.Text = "Mirror";
			// 
			// IgnorePattern
			// 
			this.IgnorePattern.Dock = System.Windows.Forms.DockStyle.Top;
			this.IgnorePattern.Location = new System.Drawing.Point(3, 123);
			this.IgnorePattern.Name = "IgnorePattern";
			this.IgnorePattern.Size = new System.Drawing.Size(259, 20);
			this.IgnorePattern.TabIndex = 23;
			this.IgnorePattern.Text = "*.v4p; *~.xml";
			this.IgnorePattern.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsKeyPress);
			// 
			// label7
			// 
			this.label7.Dock = System.Windows.Forms.DockStyle.Top;
			this.label7.Location = new System.Drawing.Point(3, 108);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(259, 15);
			this.label7.TabIndex = 54;
			this.label7.Text = "Ignore Pattern";
			// 
			// TargetPath
			// 
			this.TargetPath.Dock = System.Windows.Forms.DockStyle.Top;
			this.TargetPath.Location = new System.Drawing.Point(3, 88);
			this.TargetPath.Name = "TargetPath";
			this.TargetPath.Size = new System.Drawing.Size(259, 20);
			this.TargetPath.TabIndex = 22;
			this.TargetPath.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsKeyPress);
			// 
			// label6
			// 
			this.label6.Dock = System.Windows.Forms.DockStyle.Top;
			this.label6.Location = new System.Drawing.Point(3, 73);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(259, 15);
			this.label6.TabIndex = 52;
			this.label6.Text = "Target Path";
			// 
			// SourcePath
			// 
			this.SourcePath.Dock = System.Windows.Forms.DockStyle.Top;
			this.SourcePath.Location = new System.Drawing.Point(3, 53);
			this.SourcePath.Name = "SourcePath";
			this.SourcePath.Size = new System.Drawing.Size(259, 20);
			this.SourcePath.TabIndex = 21;
			this.SourcePath.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsKeyPress);
			// 
			// label4
			// 
			this.label4.Dock = System.Windows.Forms.DockStyle.Top;
			this.label4.Location = new System.Drawing.Point(3, 38);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(259, 15);
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
			this.MirrorPathPanel.Size = new System.Drawing.Size(259, 22);
			this.MirrorPathPanel.TabIndex = 20;
			// 
			// MirrorPathLabel
			// 
			this.MirrorPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MirrorPathLabel.Location = new System.Drawing.Point(43, 0);
			this.MirrorPathLabel.Name = "MirrorPathLabel";
			this.MirrorPathLabel.Size = new System.Drawing.Size(216, 22);
			this.MirrorPathLabel.TabIndex = 48;
			this.MirrorPathLabel.Text = "\\Mirror";
			this.MirrorPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MirrorPathButton
			// 
			this.MirrorPathButton.Dock = System.Windows.Forms.DockStyle.Left;
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
			this.VNCBox.Location = new System.Drawing.Point(3, 98);
			this.VNCBox.Name = "VNCBox";
			this.VNCBox.Size = new System.Drawing.Size(265, 63);
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
			this.VNCPathPanel.Size = new System.Drawing.Size(259, 22);
			this.VNCPathPanel.TabIndex = 10;
			// 
			// VNCPathLabel
			// 
			this.VNCPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VNCPathLabel.Location = new System.Drawing.Point(43, 0);
			this.VNCPathLabel.Name = "VNCPathLabel";
			this.VNCPathLabel.Size = new System.Drawing.Size(216, 22);
			this.VNCPathLabel.TabIndex = 48;
			this.VNCPathLabel.Text = "\\UltraVNC";
			this.VNCPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// VNCPathButton
			// 
			this.VNCPathButton.Dock = System.Windows.Forms.DockStyle.Left;
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
			this.VNCPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsKeyPress);
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
			this.PsToolsBox.Size = new System.Drawing.Size(265, 95);
			this.PsToolsBox.TabIndex = 0;
			this.PsToolsBox.TabStop = false;
			this.PsToolsBox.Text = "PsTools";
			// 
			// AddProcessButton
			// 
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
			this.PsToolsProcessPanel.Location = new System.Drawing.Point(3, 82);
			this.PsToolsProcessPanel.Name = "PsToolsProcessPanel";
			this.PsToolsProcessPanel.Size = new System.Drawing.Size(259, 10);
			this.PsToolsProcessPanel.TabIndex = 42;
			// 
			// PsToolsPathPanel
			// 
			this.PsToolsPathPanel.Controls.Add(this.PsToolsPathLabel);
			this.PsToolsPathPanel.Controls.Add(this.PsToolsPathButton);
			this.PsToolsPathPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PsToolsPathPanel.Location = new System.Drawing.Point(3, 16);
			this.PsToolsPathPanel.Name = "PsToolsPathPanel";
			this.PsToolsPathPanel.Size = new System.Drawing.Size(259, 22);
			this.PsToolsPathPanel.TabIndex = 0;
			// 
			// PsToolsPathLabel
			// 
			this.PsToolsPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PsToolsPathLabel.Location = new System.Drawing.Point(43, 0);
			this.PsToolsPathLabel.Name = "PsToolsPathLabel";
			this.PsToolsPathLabel.Size = new System.Drawing.Size(216, 22);
			this.PsToolsPathLabel.TabIndex = 48;
			this.PsToolsPathLabel.Text = "\\PsTools";
			this.PsToolsPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PsToolsPathButton
			// 
			this.PsToolsPathButton.Dock = System.Windows.Forms.DockStyle.Left;
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
			this.PsToolsUsername.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsKeyPress);
			// 
			// PsToolsPassword
			// 
			this.PsToolsPassword.Location = new System.Drawing.Point(111, 61);
			this.PsToolsPassword.Name = "PsToolsPassword";
			this.PsToolsPassword.PasswordChar = '*';
			this.PsToolsPassword.Size = new System.Drawing.Size(79, 20);
			this.PsToolsPassword.TabIndex = 3;
			this.PsToolsPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsKeyPress);
			// 
			// SimulatorPage
			// 
			this.SimulatorPage.BackColor = System.Drawing.SystemColors.Control;
			this.SimulatorPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SimulatorPage.Controls.Add(this.SimulatorListBox);
			this.SimulatorPage.Controls.Add(this.panel7);
			this.SimulatorPage.Controls.Add(this.panel6);
			this.SimulatorPage.Location = new System.Drawing.Point(4, 25);
			this.SimulatorPage.Name = "SimulatorPage";
			this.SimulatorPage.Padding = new System.Windows.Forms.Padding(3);
			this.SimulatorPage.Size = new System.Drawing.Size(273, 462);
			this.SimulatorPage.TabIndex = 2;
			this.SimulatorPage.Text = "Simulator";
			// 
			// SimulatorListBox
			// 
			this.SimulatorListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimulatorListBox.FormattingEnabled = true;
			this.SimulatorListBox.Location = new System.Drawing.Point(3, 45);
			this.SimulatorListBox.Name = "SimulatorListBox";
			this.SimulatorListBox.Size = new System.Drawing.Size(265, 407);
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
			this.panel7.Size = new System.Drawing.Size(265, 20);
			this.panel7.TabIndex = 5;
			// 
			// SimulatorStringEdit
			// 
			this.SimulatorStringEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimulatorStringEdit.Location = new System.Drawing.Point(0, 0);
			this.SimulatorStringEdit.Name = "SimulatorStringEdit";
			this.SimulatorStringEdit.Size = new System.Drawing.Size(245, 20);
			this.SimulatorStringEdit.TabIndex = 0;
			this.SimulatorStringEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SimulatorStringEditKeyPress);
			// 
			// AddSimulatorStringButton
			// 
			this.AddSimulatorStringButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AddSimulatorStringButton.Location = new System.Drawing.Point(245, 0);
			this.AddSimulatorStringButton.Name = "AddSimulatorStringButton";
			this.AddSimulatorStringButton.Size = new System.Drawing.Size(20, 20);
			this.AddSimulatorStringButton.TabIndex = 1;
			this.AddSimulatorStringButton.Text = "+";
			this.AddSimulatorStringButton.UseVisualStyleBackColor = true;
			this.AddSimulatorStringButton.Click += new System.EventHandler(this.AddSimulatorStringButtonClick);
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
			this.panel6.Size = new System.Drawing.Size(265, 22);
			this.panel6.TabIndex = 4;
			// 
			// SimulatorIPEdit
			// 
			this.SimulatorIPEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimulatorIPEdit.Location = new System.Drawing.Point(99, 0);
			this.SimulatorIPEdit.Name = "SimulatorIPEdit";
			this.SimulatorIPEdit.Size = new System.Drawing.Size(53, 20);
			this.SimulatorIPEdit.TabIndex = 4;
			this.SimulatorIPEdit.Click += new System.EventHandler(this.SimulatorIPEditTextChanged);
			// 
			// SimulatorPortUpDown
			// 
			this.SimulatorPortUpDown.Dock = System.Windows.Forms.DockStyle.Right;
			this.SimulatorPortUpDown.Location = new System.Drawing.Point(152, 0);
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
			this.SimulatorPortUpDown.Click += new System.EventHandler(this.SimulatorPortUpDownValueChanged);
			// 
			// SimulatorConnectButton
			// 
			this.SimulatorConnectButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.SimulatorConnectButton.Location = new System.Drawing.Point(208, 0);
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
			// panel3
			// 
			this.panel3.Location = new System.Drawing.Point(57, 230);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(170, 194);
			this.panel3.TabIndex = 53;
			// 
			// panel13
			// 
			this.panel13.BackColor = System.Drawing.Color.Gray;
			this.panel13.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel13.Location = new System.Drawing.Point(3, 149);
			this.panel13.Name = "panel13";
			this.panel13.Size = new System.Drawing.Size(271, 8);
			this.panel13.TabIndex = 54;
			// 
			// panel14
			// 
			this.panel14.Controls.Add(this.button1);
			this.panel14.Controls.Add(this.panel15);
			this.panel14.Controls.Add(this.checkBox1);
			this.panel14.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel14.Location = new System.Drawing.Point(3, 125);
			this.panel14.Name = "panel14";
			this.panel14.Size = new System.Drawing.Size(271, 24);
			this.panel14.TabIndex = 49;
			// 
			// button1
			// 
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(0, 0);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(185, 24);
			this.button1.TabIndex = 24;
			this.button1.Text = "Mirror Now";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// panel15
			// 
			this.panel15.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel15.Location = new System.Drawing.Point(185, 0);
			this.panel15.Name = "panel15";
			this.panel15.Size = new System.Drawing.Size(11, 24);
			this.panel15.TabIndex = 27;
			// 
			// checkBox1
			// 
			this.checkBox1.Dock = System.Windows.Forms.DockStyle.Right;
			this.checkBox1.Location = new System.Drawing.Point(196, 0);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(75, 24);
			this.checkBox1.TabIndex = 25;
			this.checkBox1.Text = "Test Only";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// panel16
			// 
			this.panel16.BackColor = System.Drawing.Color.Gray;
			this.panel16.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel16.Location = new System.Drawing.Point(3, 117);
			this.panel16.Name = "panel16";
			this.panel16.Size = new System.Drawing.Size(271, 8);
			this.panel16.TabIndex = 48;
			// 
			// button5
			// 
			this.button5.Dock = System.Windows.Forms.DockStyle.Top;
			this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button5.Location = new System.Drawing.Point(3, 62);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(271, 25);
			this.button5.TabIndex = 20;
			this.button5.Text = "Get a MAC + HostName";
			this.button5.UseVisualStyleBackColor = true;
			// 
			// panel17
			// 
			this.panel17.BackColor = System.Drawing.Color.Gray;
			this.panel17.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel17.Location = new System.Drawing.Point(3, 54);
			this.panel17.Name = "panel17";
			this.panel17.Size = new System.Drawing.Size(271, 8);
			this.panel17.TabIndex = 48;
			// 
			// comboBox1
			// 
			this.comboBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.Location = new System.Drawing.Point(3, 3);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(271, 21);
			this.comboBox1.TabIndex = 13;
			// 
			// Remoter
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.SplitContainer);
			this.DoubleBuffered = true;
			this.Name = "Remoter";
			this.Size = new System.Drawing.Size(694, 491);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			this.LeftTabControl.ResumeLayout(false);
			this.IPPage.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.GroupPage.ResumeLayout(false);
			this.panel9.ResumeLayout(false);
			this.panel9.PerformLayout();
			this.panel8.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.RightTabControl.ResumeLayout(false);
			this.CommandsPage.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.SettingsPage.ResumeLayout(false);
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
			this.panel14.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button ClearGroupSelectionButton;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
		private System.Windows.Forms.ComboBox RemoteProcessPathDrop;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Panel panel17;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Panel panel16;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Panel panel15;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Panel panel14;
		private System.Windows.Forms.Panel panel13;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.TextBox TaskDescriptionEdit;
		private System.Windows.Forms.Panel TasksPanel;
		private System.Windows.Forms.Button AddVisibleToSelectionButton;
		private System.Windows.Forms.Button DeleteSelectedIPsButton;
		private System.Windows.Forms.Button ClearSelectionButton;
		private System.Windows.Forms.Button RemoveVisibleFromSelectionButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button TaskAddButton;
		private System.Windows.Forms.TabControl RightTabControl;
		private System.Windows.Forms.SplitContainer SplitContainer;
		private System.Windows.Forms.Button VNCButton;
		private System.Windows.Forms.Panel PsToolsProcessPanel;
		private System.Windows.Forms.Button AddProcessButton;
		private System.Windows.Forms.CheckBox MirrorTestCheckBox;
		private System.ComponentModel.BackgroundWorker OnlineWorker;
		private System.ComponentModel.BackgroundWorker WatchWorker;
		
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
		private System.Windows.Forms.TabPage CommandsPage;
		private System.Windows.Forms.TabPage SettingsPage;
		private System.Windows.Forms.TabPage SimulatorPage;
		private System.Windows.Forms.Button MACAddressButton;
		private System.Windows.Forms.Button WakeOnLanButton;
		private System.Windows.Forms.Button InvertSelectionButton;
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
		private System.Windows.Forms.Button AddIPButton;
		private System.Windows.Forms.TextBox NewIPEdit;
		private System.Windows.Forms.Panel LeftTopPanel;
		private System.Windows.Forms.TextBox VNCPassword;
		private System.Windows.Forms.Label VNCPathLabel;
		private System.Windows.Forms.Button VNCPathButton;
		private System.Windows.Forms.GroupBox PsToolsBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox VNCBox;
		private System.Windows.Forms.GroupBox MirrorBox;
		private System.Windows.Forms.Panel IPListPanel;
		private System.Windows.Forms.Button ShutdownButton;
		private System.Windows.Forms.Button RebootButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.Button KillButton;
		private System.Windows.Forms.Button PsToolsPathButton;
		private System.Windows.Forms.Label PsToolsPathLabel;
		private System.Windows.Forms.FolderBrowserDialog FFolderBrowserDialog;
		
		void VNCButtonClick(object sender, EventArgs e)
		{
			string filename = FVNCPath + "\\vncviewer.exe";
			if (System.IO.File.Exists(filename))
				Execute(filename, FVNCPath, NewIPEdit.Text + " /password " + VNCPassword.Text, false, false);
		}
		
		void SplitContainerSplitterMoved(object sender, SplitterEventArgs e)
		{
			//not calling savesettings directly here, as this is also called while loading
			SettingsChanged(sender, e);
		}
		
		void SettingsKeyPress(object sender, KeyPressEventArgs e)
		{
			SaveSettings();
		}
		#endregion GUI
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create config inputs
			//first the IPs
			FHost.CreateStringConfig("IPs", TSliceMode.Dynamic, TPinVisibility.True, out FIPListInput);
			FIPListInput.SetSubType("127.0.0.1", false);

			//settings are depending on IPs already there when loading
			FHost.CreateStringConfig("Settings", TSliceMode.Single, TPinVisibility.True, out FSettingsInput);
			FSettingsInput.SetSubType("", false);
			//FSettingsInput.SliceCount = 1;

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
						
						//funny construction for backwards compatibility...
						try
						{
							if (ip_mac.Length == 5)
								AddIP(ip_mac[0], ip_mac[1], Convert.ToBoolean(ip_mac[2]), ip_mac[3], ip_mac[4]);
							else if (ip_mac.Length == 4)
								AddIP(ip_mac[0], ip_mac[1], false, ip_mac[3], "");
						}
						catch (IndexOutOfRangeException e)
						{
							AddIP(ip_mac[0], "", false, "", "");
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
			if (FLoading)
			{
				SplitContainer.SplitterDistance = FSplitterDistance;
				FLoading = false;
			}
			
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
		
		private void AddIP(string IP, string MAC, bool Selected, string Groups, string HostName)
		{
			IPAddress ipa;
			if (!IPAddress.TryParse(IP, out ipa))
			{
				FHost.Log(TLogType.Error, "Not a valid IP address");
				return;
			}
			
			IPControl ip = null;
			for (int i=0; i<IPListPanel.Controls.Count; i++)
			{
				if ((IPListPanel.Controls[i] as IPControl).IP == IP)
				{
					ip = IPListPanel.Controls[i] as IPControl;
					break;
				}
			}
			
			if (ip == null)
			{
				//add ip only if its not yet there
				ip = new IPControl(IP);
				ip.MacAddress = MAC;
				ip.HostName = HostName;
				ip.IsSelected = Selected;
				ip.Parent = IPListPanel;
				ip.Dock = DockStyle.Top;
				ip.BringToFront();
				ip.OnVNCButton += new ButtonHandler(VNCButtonHandlerCB);
				ip.OnEXPButton += new ButtonHandler(EXPButtonHandlerCB);
				ip.OnXButton += new ButtonHandler(IPXButtonHandlerCB);
				ip.OnIPSelectButton += new ButtonUpHandler(IPSelectButtonHandlerCB);
			}
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
						AddIP(baseip + i.ToString(), "", false, Group, "");
					}
				}
				catch
				{
					FHost.Log(TLogType.Error, "Not a valid IP address");
				}
			}
			else
				AddIP(NewIP, "", false, Group, "");
			
			SortIPs();
			UpdateIPListInput();
			IPListPanel.ResumeLayout();
		}
		
		private void UpdateIPListInput()
		{
			FIPListInput.SliceCount = IPListPanel.Controls.Count;
			int i=0;
			foreach (IPControl ipc in IPListPanel.Controls)
			{
				FIPListInput.SetString(i, ipc.IP + "|" + ipc.MacAddress + "|" + ipc.IsSelected.ToString() + "|" + ipc.Groups + "|" + ipc.HostName);
				i++;
			}
			
			FOnlineOutput.SliceCount = IPListPanel.Controls.Count;
			FAppIsOnlineOutput.SliceCount = IPListPanel.Controls.Count;
		}
		
		void AddIPButtonClick(object sender, EventArgs e)
		{
			DoAddIP(NewIPEdit.Text.Trim(), "");
		}
		
		void DeleteButtonClick(object sender, EventArgs e)
		{
			for (int i=IPListPanel.Controls.Count-1; i>=0; i--)
				if ((IPListPanel.Controls[i] as IPControl).IsSelected
				    && (IPListPanel.Controls[i] as IPControl).Groups == "")
					IPListPanel.Controls.RemoveAt(i);
			
			UpdateIPListInput();
		}
		
		
		void NewIPEditKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
				DoAddIP(NewIPEdit.Text.Trim(), "");
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
			gc.IsSelected = false;
			gc.Parent = GroupListPanel;
			gc.Dock = DockStyle.Top;
			gc.BringToFront();
			gc.OnXButton += new ButtonHandler(GroupXButtonHandlerCB);
			gc.OnGroupSelectButton += new ButtonUpHandler(GroupSelectButtonHandlerCB);
			gc.OnGroupChanged += new GroupChangedHandler(GroupChangedHandlerCB);
			
			gc.AddIP(IP);
			FGroups.Add(Group);
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
			FGroups.Remove(OldGroupName);
			FGroups.Add(Group.GroupName);
			
			//parse this groupcontrols ips and add them
			foreach(string s in IPs)
				DoAddIP(s, Group.GroupName);
			
			//delete remaining ips that are still marked for being deleted
			for (int i = IPListPanel.Controls.Count-1; i>=0; i--)
				if ((IPListPanel.Controls[i] as IPControl).DeleteMe)
					IPListPanel.Controls.RemoveAt(i);
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
			{
				(IPListPanel.Controls[i] as IPControl).RemoveGroup(Group);
				if ((IPListPanel.Controls[i] as IPControl).DeleteMe)
					IPListPanel.Controls.RemoveAt(i);
			}

			//remove from filter dropdown
			FGroups.Remove(Group);
			
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
			
			foreach(GroupControl gc in GroupListPanel.Controls)
				gc.IsSelected = true;
			
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
		
		void ClearGroupSelectionButtonClick(object sender, EventArgs e)
		{
			if (GroupListPanel.Controls.Count == 0)
				return;
			
			foreach(GroupControl gc in GroupListPanel.Controls)
				gc.IsSelected = false;
			
			UpdateIPSelectionsFromGroups();
		}
		#endregion IPGroups
		
		#region commands
		private void WatchLogCB(object Sender, ProgressChangedEventArgs e)
		{
			FHost.Log(TLogType.Warning, (string) e.UserState);
		}
		
		private string ExecutePsToolCommand(string Host, int ProcessID, TPsToolCommand Command, int Timeout)
		{
			if (PsToolsUsername.Text == "")
				return "Username for remote PC is not specified. See Settings!";
			if (PsToolsPassword.Text == "")
				return "Password for remote PC is not specified. See Settings!";
			if (FPsToolsPath == "")
				return "Path to PsTools is not specified. See Settings!";
			
			string filename = FPsToolsPath + "\\";
			string workingdir = "";
			string arguments = "\\\\" + Host + " -u " + PsToolsUsername.Text + " -p " + PsToolsPassword.Text;
			
			switch (Command)
			{
				case TPsToolCommand.Execute:
					{
						filename += "psexec.exe";
						if (Timeout > 0)
							arguments += " -n " + Timeout + " -i -d \"" + FProcesses[ProcessID].Process + "\" " + FProcesses[ProcessID].Arguments;
						else
							arguments += " -i -d \"" + FProcesses[ProcessID].Process + "\" " + FProcesses[ProcessID].Arguments;
						workingdir = System.IO.Path.GetDirectoryName(FProcesses[ProcessID].Process);
						break;
					}
				case TPsToolCommand.Kill:
					{
						filename += "pskill.exe";
						arguments += " -t " + System.IO.Path.GetFileNameWithoutExtension(FProcesses[ProcessID].Process);
						break;
					}
				case TPsToolCommand.Watch:
					{
						filename += "pslist.exe";
						arguments += " -m " + System.IO.Path.GetFileNameWithoutExtension(FProcesses[ProcessID].Process);
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
			
			string result = "";
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
		
		private void RunTask(TPsToolCommand Command, List<IPControl> IPs, int ProcessID, int Timeout)
		{
			foreach(IPControl ipc in IPs)
				if (ipc.IsOnline)
			{
				string result = ExecutePsToolCommand(ipc.IP, ProcessID, Command, Timeout);
				if (result.Length > 1)
					FHost.Log(TLogType.Error, result);
			}
		}
		
		private List<IPControl> SelectedIPs()
		{
			List<IPControl> selectedIPs = new List<IPControl>();
			foreach (IPControl ipc in IPListPanel.Controls)
				if (ipc.IsSelected)
					selectedIPs.Add(ipc);
			
			return selectedIPs;
		}
		
		void StartButtonClick(object sender, EventArgs e)
		{
			RunTask(TPsToolCommand.Execute, SelectedIPs(), RemoteProcessPathDrop.SelectedIndex, 0);
		}
		
		void RestartButtonClick(object sender, EventArgs e)
		{
			RunTask(TPsToolCommand.Kill, SelectedIPs(), RemoteProcessPathDrop.SelectedIndex, 0);
			RunTask(TPsToolCommand.Execute, SelectedIPs(), RemoteProcessPathDrop.SelectedIndex, 0);
		}
		
		void KillButtonClick(object sender, EventArgs e)
		{
			RunTask(TPsToolCommand.Kill, SelectedIPs(), RemoteProcessPathDrop.SelectedIndex, 0);
		}
		
		void RebootButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
					ExecutePsToolCommand(ipc.IP, -1, TPsToolCommand.Reboot, 0);
		}
		
		void ShutdownButtonClick(object sender, EventArgs e)
		{
			foreach(IPControl ipc in IPListPanel.Controls)
				if ((ipc.IsSelected) && (ipc.IsOnline))
					ExecutePsToolCommand(ipc.IP, -1, TPsToolCommand.Shutdown, 0);
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
				
				UpdateGroupsOnlineState();
				
				System.Threading.Thread.Sleep(5);
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
			//go through all tasks and see if they want to be watched
			while(true)
			{
				foreach (TaskControl task in FTasks)
				{
					if (task.Watch)
					{
						List<IPControl> groupIPs = IPsOfGroup(task.GroupID);
						foreach (IPControl ipc in groupIPs)
						{
							if (ipc.NeedsWatchUpdate() && (ipc.IsOnline))
							{
								string processName = System.IO.Path.GetFileNameWithoutExtension(FProcesses[task.ProcessID].Process);
								
								//for local processes use: Process.Responding
								if (ipc.IsLocalHost)
									ipc.AppIsOnline = ipc.LocalProcessIsResponding(processName);
								else
									//for remote processes use:
								{
									//this only detects processes that have vanished, not hanging ones!
									string result = ExecutePsToolCommand(ipc.IP, task.ProcessID, TPsToolCommand.Watch, 0);
									if (result.Contains("Error: "))
									{
										ipc.AppIsOnline = false;
										WatchWorker.ReportProgress(0, result);
									}
									else if (result.Contains("process " + System.IO.Path.GetFileNameWithoutExtension(processName) + " was not found"))
									{
										ipc.AppIsOnline = false;
									}
									else if (result.Contains("Failed"))
									{
										ipc.AppIsOnline = false;
										WatchWorker.ReportProgress(0, "Failed to watch remote process on: " + ipc.IP);
									}
									else
										ipc.AppIsOnline = true;
								}
								
								if (!ipc.AppIsOnline)
									switch (task.WatchMode)
								{
									case TWatchMode.Restart:
										{
											string result = ExecutePsToolCommand(ipc.IP, task.ProcessID, TPsToolCommand.Execute, task.Timeout);
											if (!(result.Length > 1))
												ipc.AppIsOnline = true;
											break;
										}
									case TWatchMode.Reboot:
										{
											ExecutePsToolCommand(ipc.IP, task.ProcessID, TPsToolCommand.Reboot, 0);
											break;
										}
								}
							}
							
							UpdateGroupsAppOnlineState();
							System.Threading.Thread.Sleep(5);
						}
					}
				}
				System.Threading.Thread.Sleep(5);
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
			XmlNodeList processes, selections, tasks;
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
				//processes
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
				
				//commands
				FSettings.LoadXml(Settings); //not sure why need to load here again
				tool = FSettings.SelectSingleNode(@"REMOTER/COMMANDS");
				attr = tool.Attributes.GetNamedItem("RemoteProcessPath") as XmlAttribute;
				RemoteProcessPathDrop.SelectedIndex = int.Parse(attr.Value);
				
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
				
				//splitter
				FSettings.LoadXml(Settings); //not sure why need to load here again
				
				tool = FSettings.SelectSingleNode(@"REMOTER/GUI");
				attr = tool.Attributes.GetNamedItem("Splitter") as XmlAttribute;
				FSplitterDistance = int.Parse(attr.Value);
				
				//tasks
				FSettings.LoadXml(Settings); //not sure why need to load here again
				tasks = FSettings.SelectNodes(@"REMOTER/TASKS/TASK");
				string description;
				bool watch;
				int group, proc, timeout;
				TWatchMode watchmode;
				foreach (XmlNode task in tasks)
				{
					attr = task.Attributes.GetNamedItem("Description") as XmlAttribute;
					description = attr.Value;
					
					attr = task.Attributes.GetNamedItem("Group") as XmlAttribute;
					group = int.Parse(attr.Value);
					
					attr = task.Attributes.GetNamedItem("Process") as XmlAttribute;
					proc = int.Parse(attr.Value);
					
					attr = task.Attributes.GetNamedItem("Timeout") as XmlAttribute;
					timeout = int.Parse(attr.Value);
					
					attr = task.Attributes.GetNamedItem("Watch") as XmlAttribute;
					watch = bool.Parse(attr.Value);
					
					attr = task.Attributes.GetNamedItem("WatchMode") as XmlAttribute;
					watchmode = (TWatchMode) Enum.Parse(typeof(TWatchMode), attr.Value);
					
					AddTask(description, group, proc, timeout, watch, watchmode);
				}
			}
			catch
			{
				FHost.Log(TLogType.Warning, "Failed loading Remoter settings.");
			}
		}
		
		private void SaveSettings()
		{
			//gather all settings as one XML string
			XmlNode main, tool, process, task;
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
			attr.Value = RemoteProcessPathDrop.SelectedIndex.ToString();
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
			
			//splitter
			tool = FSettings.CreateElement("GUI");
			main.AppendChild(tool);
			attr = FSettings.CreateAttribute("Splitter");
			attr.Value = SplitContainer.SplitterDistance.ToString();
			tool.Attributes.Append(attr);
			
			//tasks
			tool = FSettings.CreateElement("TASKS");
			main.AppendChild(tool);
			
			foreach(TaskControl tc in TasksPanel.Controls)
			{
				task = FSettings.CreateElement("TASK");
				tool.AppendChild(task);
				
				attr = FSettings.CreateAttribute("Description");
				attr.Value = tc.Description;
				task.Attributes.Append(attr);
				
				attr = FSettings.CreateAttribute("Group");
				attr.Value = tc.GroupID.ToString();
				task.Attributes.Append(attr);
				
				attr = FSettings.CreateAttribute("Process");
				attr.Value = tc.ProcessID.ToString();
				task.Attributes.Append(attr);
				
				attr = FSettings.CreateAttribute("Timeout");
				attr.Value = tc.Timeout.ToString();
				task.Attributes.Append(attr);
				
				attr = FSettings.CreateAttribute("Watch");
				attr.Value = tc.Watch.ToString();
				task.Attributes.Append(attr);
				
				attr = FSettings.CreateAttribute("WatchMode");
				attr.Value = tc.WatchMode.ToString();
				task.Attributes.Append(attr);
			}
			
			
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
		
		#region process
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
			
			FProcesses.Add(pc);
		}
		
		private void AdaptPsToolsProcessPanelHeight()
		{
			PsToolsProcessPanel.Height = PsToolsProcessPanel.Controls.Count * 25;
			PsToolsBox.Height = PsToolsProcessPanel.Height + 85;
		}
		
		private void ProcessXButtonHandlerCB(UserControl Control)
		{
			PsToolsProcessPanel.Controls.Remove(Control);
			FProcesses.Remove(Control as ProcessControl);
			
			AdaptPsToolsProcessPanelHeight();
			
			SaveSettings();
		}
		
		private void ProcessChangedHandlerCB()
		{
			SaveSettings();
		}
		
		#endregion process
		
		#region selections
		void ClearSelectionButtonClick(object sender, EventArgs e)
		{
			if (IPListPanel.Controls.Count == 0)
				return;
			
			foreach(IPControl ipc in IPListPanel.Controls)
				ipc.IsSelected = false;
			
			UpdateIPListInput();
		}
		
		void AddVisibleToSelectionButtonClick(object sender, EventArgs e)
		{
			if (IPListPanel.Controls.Count == 0)
				return;
			
			foreach(IPControl ipc in IPListPanel.Controls)
				if (ipc.Visible)
					ipc.IsSelected = true;
			
			UpdateIPListInput();
		}
		
		void RemoveVisibleFromSelectionButtonClick(object sender, EventArgs e)
		{
			if (IPListPanel.Controls.Count == 0)
				return;
			
			foreach(IPControl ipc in IPListPanel.Controls)
				if (ipc.Visible)
					ipc.IsSelected = false;
			
			UpdateIPListInput();
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
		#endregion selections
		
		#region tasks
		void TaskAddButtonClick(object sender, EventArgs e)
		{
			AddTask(TaskDescriptionEdit.Text.Trim(), 0, 0, 10, false, TWatchMode.Off);
			
			SaveSettings();
		}
		
		void TaskNameEditKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
				AddTask(TaskDescriptionEdit.Text.Trim(), 0, 0, 10, false, TWatchMode.Off);
			
			SaveSettings();
		}
		
		private void DeleteTaskHandler(UserControl Control)
		{
			FTasks.Remove(Control as TaskControl);
			TasksPanel.Controls.Remove(Control);
			SaveSettings();
		}
		
		private List<IPControl> IPsOfGroup(int GroupID)
		{
			List<IPControl> groupIPs = new List<IPControl>();
			foreach (IPControl ipc in IPListPanel.Controls)
				if (ipc.IsPartOfGroup(FGroups[GroupID]))
					groupIPs.Add(ipc);
			
			return groupIPs;
		}
		
		private void ExecuteTaskHandler(UserControl Control)
		{
			TaskControl task = Control as TaskControl;
			
			List<IPControl> groupIPs = IPsOfGroup(task.GroupID);
			
			switch (task.TaskType)
			{
				case TTaskType.Start:
					{
						RunTask(TPsToolCommand.Execute, groupIPs, task.ProcessID, task.Timeout);
						break;
					}
				case TTaskType.Restart:
					{
						RunTask(TPsToolCommand.Kill, groupIPs, task.ProcessID, task.Timeout);
						RunTask(TPsToolCommand.Execute, groupIPs, task.ProcessID, task.Timeout);
						break;
					}
				case TTaskType.Kill:
					{
						RunTask(TPsToolCommand.Kill, groupIPs, task.ProcessID, task.Timeout);
						break;
					}
			}
		}
		
		private void SaveTaskHandler(UserControl Control)
		{
			SaveSettings();
		}
		
		private void AddTask(string Description, int Group, int Process, int Timeout, bool Watch, TWatchMode WatchMode)
		{
			TaskControl task = new TaskControl();
			task.Description = Description;
			task.Watch = Watch;
			task.WatchMode = WatchMode;
			
			task.GroupDrop.DataSource = FGroups;
			task.ProcessDrop.DataSource = FProcesses;
			task.ProcessDrop.DisplayMember = "ProcessAndArguments";
			
			task.OnXButton += new ButtonUpHandler(DeleteTaskHandler);
			task.OnExecute += new ButtonUpHandler(ExecuteTaskHandler);
			task.OnSave += new ButtonUpHandler(SaveTaskHandler);
			
			task.GroupID = Group;
			task.ProcessID = Process;
			task.Timeout = Timeout;
			
			FTasks.Add(task);
			TasksPanel.Controls.Add(task);
			task.Dock = DockStyle.Top;
			task.BringToFront();
		}
		#endregion tasks
	}

	public delegate void ButtonHandler(string IP);
	public delegate void ButtonUpHandler(UserControl Control);
	public delegate void GroupChangedHandler(GroupControl Group, string OldGroupName, List<string> IPs);

	public enum TTaskType {Start, Restart, Kill}
	public enum TPsToolCommand {Execute, Kill, Watch, WatchExecute, Reboot, Shutdown};
	public enum TWatchMode {Off, Restart, Reboot};
}
