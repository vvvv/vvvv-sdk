#region licence/info

//////project name
//Timeliner

//////description
//a gui to arrange keyframes of different types (value, color, string..)
//in time

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.Timeliner;
using VVVV.Utils.ManagedVCL;

namespace VVVV.Nodes
{
	public class TimelinerPlugin: TopControl, IPlugin
	{
		#region field declaration
		
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		public const int FHeaderWidth = 128;
		public static System.Globalization.NumberFormatInfo GNumberFormat = new System.Globalization.NumberFormatInfo();
		private XmlDocument FSettings;
		
		// INPUT PINS
		///////////////////////
		private IValueIn FPlayInput;
		private IValueIn FSpeedInput;
		private IValueIn FSetTime;
		private IValueFastIn FTimeInput;
		
		// CONFIG PINS
		///////////////////////
		private IStringConfig FPinSettings;
		private IStringConfig FGUISettings;
		private IValueConfig FTranslateInput;
		private IValueConfig FScaleInput;

		// OUTPUT PINS
		///////////////////////
		private IValueOut FTimeOut;
		private IValueOut FPlayingOut;
		private IValueOut FSeekingOut;
		private IValueOut FScratchingOut;

		// VAIRABLES
		///////////////////////
		private bool FDoSetTime;
		private bool FFirstFrame = true;
		private bool FTransformationChanged = true;
		private bool FBlockConfigurate = false;
		private ArrayList FPinSettingsList = new ArrayList();
		private bool FAutomataCreatedViaGUI = false;
		
		// LISTS
		///////////////////////
		private System.Collections.Generic.List<VVVV.Nodes.Timeliner.TLBasePin> FOutputPins = new List<TLBasePin>();

		// CLASSES
		///////////////////////
		private TLRulerPin FTopRuler;
		private TLAutomataPin FAutomata;
		private TLTransformer FTransformer = new TLTransformer();
		private TLTime FTimer = new TLTime();
		
		private void InitializeComponent()
		{
            this.MainMenu = new System.Windows.Forms.Panel();
            this.TimeBarModeBox = new System.Windows.Forms.ComboBox();
            this.ToggleCollapseButton = new System.Windows.Forms.Button();
            this.MidiButton = new System.Windows.Forms.Button();
            this.WavButton = new System.Windows.Forms.Button();
            this.AutomataCheckBox = new System.Windows.Forms.CheckBox();
            this.ColorButton = new System.Windows.Forms.Button();
            this.RulerButton = new System.Windows.Forms.Button();
            this.StringButton = new System.Windows.Forms.Button();
            this.ValueButton = new System.Windows.Forms.Button();
            this.PlayButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.PinPanel = new System.Windows.Forms.Panel();
            this.InsertPreview = new System.Windows.Forms.Panel();
            this.SliceArea = new VVVV.Nodes.Timeliner.TLSliceArea();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.PinHeaderPanel0 = new System.Windows.Forms.Panel();
            this.PinHeaderPanel1 = new System.Windows.Forms.Panel();
            this.MainMenu.SuspendLayout();
            this.PinPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.MainMenu.Controls.Add(this.TimeBarModeBox);
            this.MainMenu.Controls.Add(this.ToggleCollapseButton);
            this.MainMenu.Controls.Add(this.MidiButton);
            this.MainMenu.Controls.Add(this.WavButton);
            this.MainMenu.Controls.Add(this.AutomataCheckBox);
            this.MainMenu.Controls.Add(this.ColorButton);
            this.MainMenu.Controls.Add(this.RulerButton);
            this.MainMenu.Controls.Add(this.StringButton);
            this.MainMenu.Controls.Add(this.ValueButton);
            this.MainMenu.Controls.Add(this.PlayButton);
            this.MainMenu.Controls.Add(this.StopButton);
            this.MainMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
		    this.MainMenu.Size = new System.Drawing.Size(688, 25);
            this.MainMenu.TabIndex = 0;
            // 
            // TimeBarModeBox
            // 
            this.TimeBarModeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TimeBarModeBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TimeBarModeBox.FormattingEnabled = true;
            this.TimeBarModeBox.Items.AddRange(new object[] {
            "Manual",
            "Jump",
            "Follow"});
		    this.TimeBarModeBox.Location = new System.Drawing.Point(454, 2);
            this.TimeBarModeBox.Name = "TimeBarModeBox";
		    this.TimeBarModeBox.Size = new System.Drawing.Size(93, 21);
            this.TimeBarModeBox.TabIndex = 10;
            this.TimeBarModeBox.SelectionChangeCommitted += new System.EventHandler(this.TimeBarModeBoxSelectionChangeCommitted);
            // 
            // ToggleCollapseButton
            // 
            this.ToggleCollapseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.ToggleCollapseButton.Location = new System.Drawing.Point(110, 0);
            this.ToggleCollapseButton.Name = "ToggleCollapseButton";
		    this.ToggleCollapseButton.Size = new System.Drawing.Size(40, 23);
            this.ToggleCollapseButton.TabIndex = 2;
            this.ToggleCollapseButton.Text = "v | >";
            this.ToggleCollapseButton.UseVisualStyleBackColor = true;
            this.ToggleCollapseButton.Click += new System.EventHandler(this.ToggleCollapseButtonClick);
            // 
            // MidiButton
            // 
            this.MidiButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.MidiButton.Location = new System.Drawing.Point(374, 0);
            this.MidiButton.Name = "MidiButton";
		    this.MidiButton.Size = new System.Drawing.Size(34, 23);
            this.MidiButton.TabIndex = 8;
            this.MidiButton.Text = "+M";
            this.MidiButton.UseVisualStyleBackColor = true;
            this.MidiButton.Click += new System.EventHandler(this.PinButtonClick);
            // 
            // WavButton
            // 
            this.WavButton.Enabled = false;
            this.WavButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.WavButton.Location = new System.Drawing.Point(414, 0);
            this.WavButton.Name = "WavButton";
		    this.WavButton.Size = new System.Drawing.Size(34, 23);
            this.WavButton.TabIndex = 9;
            this.WavButton.Text = "+W";
            this.WavButton.UseVisualStyleBackColor = true;
            this.WavButton.Visible = false;
            this.WavButton.Click += new System.EventHandler(this.PinButtonClick);
            // 
            // AutomataCheckBox
            // 
            this.AutomataCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.AutomataCheckBox.Location = new System.Drawing.Point(185, 0);
            this.AutomataCheckBox.Name = "AutomataCheckBox";
		    this.AutomataCheckBox.Size = new System.Drawing.Size(34, 24);
            this.AutomataCheckBox.TabIndex = 3;
            this.AutomataCheckBox.Text = "A";
            this.AutomataCheckBox.UseVisualStyleBackColor = true;
            this.AutomataCheckBox.CheckedChanged += new System.EventHandler(this.AutomataCheckBoxCheckedChanged);
            // 
            // ColorButton
            // 
            this.ColorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.ColorButton.Location = new System.Drawing.Point(337, 0);
            this.ColorButton.Name = "ColorButton";
		    this.ColorButton.Size = new System.Drawing.Size(31, 23);
            this.ColorButton.TabIndex = 7;
            this.ColorButton.Text = "+C";
            this.ColorButton.UseVisualStyleBackColor = true;
            this.ColorButton.Click += new System.EventHandler(this.PinButtonClick);
            // 
            // RulerButton
            // 
            this.RulerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.RulerButton.Location = new System.Drawing.Point(225, 0);
            this.RulerButton.Name = "RulerButton";
		    this.RulerButton.Size = new System.Drawing.Size(31, 23);
            this.RulerButton.TabIndex = 4;
            this.RulerButton.Text = "+R";
            this.RulerButton.UseVisualStyleBackColor = true;
            this.RulerButton.Click += new System.EventHandler(this.PinButtonClick);
            // 
            // StringButton
            // 
            this.StringButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.StringButton.Location = new System.Drawing.Point(299, 0);
            this.StringButton.Name = "StringButton";
		    this.StringButton.Size = new System.Drawing.Size(32, 23);
            this.StringButton.TabIndex = 6;
            this.StringButton.Text = "+S";
            this.StringButton.UseVisualStyleBackColor = true;
            this.StringButton.Click += new System.EventHandler(this.PinButtonClick);
            // 
            // ValueButton
            // 
            this.ValueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.ValueButton.Location = new System.Drawing.Point(262, 0);
            this.ValueButton.Name = "ValueButton";
		    this.ValueButton.Size = new System.Drawing.Size(31, 23);
            this.ValueButton.TabIndex = 5;
            this.ValueButton.Text = "+V";
            this.ValueButton.UseVisualStyleBackColor = true;
            this.ValueButton.Click += new System.EventHandler(this.PinButtonClick);
            // 
            // PlayButton
            // 
            this.PlayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.PlayButton.Location = new System.Drawing.Point(46, 0);
            this.PlayButton.Name = "PlayButton";
		    this.PlayButton.Size = new System.Drawing.Size(40, 23);
            this.PlayButton.TabIndex = 1;
            this.PlayButton.Text = "Play";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButtonClick);
            // 
            // StopButton
            // 
            this.StopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopButton.Location = new System.Drawing.Point(0, 0);
            this.StopButton.Name = "StopButton";
		    this.StopButton.Size = new System.Drawing.Size(40, 23);
            this.StopButton.TabIndex = 0;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButtonClick);
            // 
            // PinPanel
            // 
            this.PinPanel.AutoScroll = true;
            this.PinPanel.Controls.Add(this.InsertPreview);
            this.PinPanel.Controls.Add(this.SliceArea);
            this.PinPanel.Controls.Add(this.SplitContainer);
            this.PinPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		    this.PinPanel.Location = new System.Drawing.Point(0, 25);
            this.PinPanel.Name = "PinPanel";
		    this.PinPanel.Size = new System.Drawing.Size(688, 330);
            this.PinPanel.TabIndex = 1;
            // 
            // InsertPreview
            // 
            this.InsertPreview.BackColor = System.Drawing.Color.Black;
		    this.InsertPreview.Location = new System.Drawing.Point(156, 17);
            this.InsertPreview.Name = "InsertPreview";
		    this.InsertPreview.Size = new System.Drawing.Size(150, 2);
            this.InsertPreview.TabIndex = 4;
            this.InsertPreview.Visible = false;
            // 
            // SliceArea
            // 
            this.SliceArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.SliceArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SliceArea.ForeColor = System.Drawing.SystemColors.Window;
		    this.SliceArea.Location = new System.Drawing.Point(150, 0);
            this.SliceArea.MouseState = VVVV.Nodes.Timeliner.TLMouseState.msIdle;
            this.SliceArea.Name = "SliceArea";
		    this.SliceArea.Size = new System.Drawing.Size(538, 330);
            this.SliceArea.TabIndex = 3;
            // 
            // SplitContainer
            // 
            this.SplitContainer.BackColor = System.Drawing.Color.Black;
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Left;
            this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.AutoScroll = true;
            this.SplitContainer.Panel1.Controls.Add(this.PinHeaderPanel0);
            this.SplitContainer.Panel1MinSize = 20;
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.AutoScroll = true;
            this.SplitContainer.Panel2.Controls.Add(this.PinHeaderPanel1);
		    this.SplitContainer.Size = new System.Drawing.Size(150, 330);
		    this.SplitContainer.SplitterDistance = 20;
		    this.SplitContainer.SplitterWidth = 5;
            this.SplitContainer.TabIndex = 2;
            this.SplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainerSplitterMoved);
            // 
            // PinHeaderPanel0
            // 
            this.PinHeaderPanel0.AllowDrop = true;
            this.PinHeaderPanel0.AutoScroll = true;
            this.PinHeaderPanel0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.PinHeaderPanel0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PinHeaderPanel0.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.PinHeaderPanel0.Location = new System.Drawing.Point(0, 0);
            this.PinHeaderPanel0.Name = "PinHeaderPanel0";
		    this.PinHeaderPanel0.Size = new System.Drawing.Size(150, 20);
            this.PinHeaderPanel0.TabIndex = 0;
            this.PinHeaderPanel0.Tag = "0";
            this.PinHeaderPanel0.DragDrop += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel0DragDrop);
            this.PinHeaderPanel0.DragEnter += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel0DragEnter);
            this.PinHeaderPanel0.DragOver += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel0DragOver);
            // 
            // PinHeaderPanel1
            // 
            this.PinHeaderPanel1.AllowDrop = true;
            this.PinHeaderPanel1.AutoScroll = true;
            this.PinHeaderPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.PinHeaderPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PinHeaderPanel1.Location = new System.Drawing.Point(0, 0);
            this.PinHeaderPanel1.Name = "PinHeaderPanel1";
		    this.PinHeaderPanel1.Size = new System.Drawing.Size(150, 305);
            this.PinHeaderPanel1.TabIndex = 1;
            this.PinHeaderPanel1.Tag = "1";
            this.PinHeaderPanel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel1DragDrop);
            this.PinHeaderPanel1.DragEnter += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel1DragEnter);
            this.PinHeaderPanel1.DragOver += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel1DragOver);
            // 
            // TimelinerPlugin
            // 
		    this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PinPanel);
            this.Controls.Add(this.MainMenu);
            this.Name = "TimelinerPlugin";
		    this.Size = new System.Drawing.Size(688, 355);
            this.MainMenu.ResumeLayout(false);
            this.PinPanel.ResumeLayout(false);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.ComboBox TimeBarModeBox;
		private System.Windows.Forms.Button ToggleCollapseButton;
		private System.Windows.Forms.SplitContainer SplitContainer;
		private System.Windows.Forms.Button MidiButton;
		private System.Windows.Forms.Button WavButton;
		private System.Windows.Forms.Panel InsertPreview;
		private System.Windows.Forms.Panel PinHeaderPanel0;
		private System.Windows.Forms.Panel PinHeaderPanel1;
		private VVVV.Nodes.Timeliner.TLSliceArea SliceArea;
		private System.Windows.Forms.CheckBox AutomataCheckBox;
		
		private System.Windows.Forms.Button RulerButton;
		private System.Windows.Forms.Button ColorButton;
		private System.Windows.Forms.Panel MainMenu;
		private System.Windows.Forms.Panel PinPanel;
		private System.Windows.Forms.Button StringButton;
		private System.Windows.Forms.Button ValueButton;
		private System.Windows.Forms.Button PlayButton;
		private System.Windows.Forms.Button StopButton;
		
		#endregion field creation
		
		#region constructor/destructor
		
		public TimelinerPlugin()
		{
			InitializeComponent();

            using (var g = this.CreateGraphics())
            {
                var dpiFactor = g.DpiY / 96.0f;
                SplitContainer.SplitterDistance = (int) Math.Round(30 * dpiFactor);
            }

			this.BackColor = System.Drawing.Color.LightGray;

			MainMenu.BringToFront();
			PinPanel.BringToFront();
			
			//PinHeaderPanel0.MouseWheel += new MouseEventHandler(OnMouseWheel);
			FTransformer.OnTransformationChanged += new TransformationChangedHandler(TransformationChangedCB);
			
			GNumberFormat.NumberDecimalSeparator = ".";
			SliceArea.OutputPins = FOutputPins;
			SliceArea.Transformer = FTransformer;
			SliceArea.Timer = FTimer;
			SliceArea.SplitterPosition = 20;
			
			FSettings = new XmlDocument();
			
			FTimer.Transformer = FTransformer;
			TimeBarModeBox.SelectedIndex = 0;
		}
		
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FAutomata = null;
					FTopRuler = null;
					FTimer = null;
					FTransformer = null;

				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				//..
			}
			FDisposed = true;
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		private static PluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Timeliner";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Animation";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "A gui to arrange keyframes of different types (value, color, string..)";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "Keyframe, Score";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "MidiPin uses MIDIToolkit by Leslie Sanford: http://www.codeproject.com/KB/audio-video/MIDIToolkit.aspx";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any kown usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(400, 200);
					//define the nodes initial size in window-mode
					FPluginInfo.InitialWindowSize = new Size(600, 300);
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
			get	{return true;}
		}
		
		#endregion node name and infos

		#region pin creation
		
		public void SetPluginHost(IPluginHost Host)
		{
			FHost = Host;
			
			// CREATE INPUT PINS
			///////////////////////
			createInputPins();
			
			// CREATE OUTPUT PINS
			///////////////////////
			createOutputPins();
			
			//add one Ruler for sure
			FPinSettingsList.Add("<PIN Name=\"Ruler0\" Type=\"Ruler\" SliceCount=\"0\" Height=\"40\" Parent=\"0\" />");
			Configurate(FPinSettings);
		}

		private void createInputPins()
		{
			FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out FPlayInput);
			FPlayInput.SetSubType(0, 1, 1, 0, false, true, true);
			FPlayInput.Order = -99999;
			
			FHost.CreateValueInput("Speed", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FSpeedInput);
			FSpeedInput.SetSubType(Double.MinValue, Double.MaxValue, 0.01D, 1, false, false, false);
			FSpeedInput.Order = -99998;
			
			FHost.CreateValueFastInput("Time In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTimeInput);
			FTimeInput.SetSubType(Double.MinValue, Double.MaxValue, 0.01D, 0, false, false, false);
			FTimeInput.Order = -99997;
			
			FHost.CreateValueInput("Set Time", 1, null, TSliceMode.Single, TPinVisibility.True, out FSetTime);
			FSetTime.SetSubType(0, 1, 1, 0, false, true, true);
			FSetTime.Order = -99996;
			
			FHost.CreateValueConfig("Translate", 1, null, TSliceMode.Single, TPinVisibility.True, out FTranslateInput);
			FTranslateInput.SetSubType(Double.MinValue, Double.MaxValue, 0.1, 0, false, false, false);
			FTranslateInput.Order = -99995;
			
			FHost.CreateValueConfig("Scale", 1, null, TSliceMode.Single, TPinVisibility.True, out FScaleInput);
			FScaleInput.SetSubType(0.1, 1000, 0.1, 50, false, false, false);
			FScaleInput.Order = -99994;
			
			// ONLY VISIBLE IN INSPECTOR
			FHost.CreateStringConfig("GUI Settings", TSliceMode.Dynamic, TPinVisibility.Hidden, out FGUISettings);
			//FGUISettings.SliceCount = 0;
			FGUISettings.SetSubType("", false);
			FGUISettings.Order = -99993;
			
			FHost.CreateStringConfig("Pin Settings", TSliceMode.Dynamic, TPinVisibility.Hidden, out FPinSettings);
			FPinSettings.SetSubType("<PIN Name=\"Ruler0\" Type=\"Ruler\" SliceCount=\"0\" Height=\"40\" Parent=\"0\" />", false);
			FPinSettings.Order = -99992;
		}

		private void createOutputPins()
		{
			// TIME OUT
			///////////////////////
			FHost.CreateValueOutput("Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTimeOut);
			FTimeOut.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, false);
			FTimeOut.Order = -99999;
			
			FHost.CreateValueOutput("Playing", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPlayingOut);
			FPlayingOut.SetSubType(0, 1, 1, 0, true, false, true);
			FPlayingOut.Order = -99998;
			
			FHost.CreateValueOutput("Seeking", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FSeekingOut);
			FSeekingOut.SetSubType(0, 1, 1, 0, true, false, true);
			FSeekingOut.Order = -99997;
			
			FHost.CreateValueOutput("Scratching", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FScratchingOut);
			FScratchingOut.SetSubType(0, 1, 1, 0, true, false, true);
			FScratchingOut.Order = -99996;
		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//create pins here according to PinSettings content
			
			if (FBlockConfigurate)
				return;
			
			//FHost.Log(TLogType.Debug, "Configurate: " +Input.Name + " - SliceCount: " + Input.SliceCount);
			
			//this is a hack
			//only needed because vvvv calls configcallback after setting of every slice while SettingSpreadAsString onload
			//should better fill whole spread with saved values and then call configcallback
			//so here we check that all pinsettings refer to a different PinName
			//if there are doublepinnames, the current call seems to be triggered before all slices have been
			//used to compare whole settings string before (now comparing only pinnames) which failed,
			//as settings can include optional attributes..

			string s;
			List<string> pinList = new List<string>();
			XmlNode pinSettings;
			XmlAttribute attr;
			bool doublesettings = false;
			for (int i=0; i<FPinSettings.SliceCount; i++)
			{
				FPinSettings.GetString(i, out s);
				FSettings.LoadXml(s);
				pinSettings = FSettings.SelectSingleNode(@"//PIN");
				attr = pinSettings.Attributes.GetNamedItem("Name") as XmlAttribute;

				if (pinList.IndexOf(attr.Value) >= 0)
				{
					doublesettings = true;
					break;
				}
				else
					pinList.Add(attr.Value);
			}
			
			//only go on if every settings slice is unique: as at least pin name must be unique in all pins!
			if ((doublesettings) || (FPinSettings.SliceCount == 0))
				return;
			//hack end.-
			
			//FHost.Log(TLogType.Debug, "Configurate: " +Input.Name + " - FirstFrame: " + FFirstFrame);
			
			if (Input == FGUISettings)
				GUISettingsChanged();
			else if (Input == FPinSettings)
				PinSettingsChanged();
			else //could also be a message for one of the other pins...
				for (int i = 0;i<FOutputPins.Count;i++)
			{
				FOutputPins[i].Configurate(Input, FFirstFrame);
			}
		}
		
		public void Evaluate(int SpreadMax)
		{
			//check inputs
			double dval;
			
			//data in
			/*	if (FDataInput.PinIsChanged)
        	{
        		FDataInput.GetValue(0, out dval);
        		if (TLGlobalStates.Instance.IsRecord)
        		{
        			for (int i = 0; i< FOutputPins.Count;i++)
        			{
        				if (FOutputPins[i].Type==0)
        				{
        					FOutputPins[i].OutputSlices[0].AddKeyFrame(GTopRuler.CurrentTime,dval,1);
        				}
        			}
        		}
        		FData = (double) dval;
        	}*/
			
			//play
			if (FPlayInput.PinIsChanged)
			{
				FPlayInput.GetValue(0, out dval);
				
				if(dval == 0)
					FTimer.IsRunning = false;
				else
					FTimer.IsRunning = true;
			}
			
			//lookat
			//this is only for loading saved data in the first frame
			if (FTransformationChanged)
			{
				FTranslateInput.GetValue(0, out dval);
				FTransformer.GTimeTranslate = dval;
				FScaleInput.GetValue(0, out dval);
				FTransformer.GTimeScale = dval;
				
				FTransformer.ApplyTransformation();
				
				FTransformationChanged = false;
				SliceArea.Invalidate();
			}
			
			//set time
			if (FSetTime.PinIsChanged)
			{
				FSetTime.GetValue(0, out dval);
				FDoSetTime = dval > 0.5;
			}
			
			var isScratching = SliceArea.MouseState == TLMouseState.msDraggingTimeBar;

			if (isScratching)
			{
				//this is handled in SliceArea.MouseDown
			}
			else if (FDoSetTime)
			{
				FTimer.TimeCount = Math.Min(FOutputPins.Count, FTimeInput.SliceCount);
				for (int i=0; i<FTimeInput.SliceCount; i++)
				{
					FTimeInput.GetValue(i, out dval);
					FTimer.SetTime(i, dval);
				}
			}
			else
			{
				double hosttime;
				FTimer.TimeCount = 1;
				FHost.GetCurrentTime(out hosttime);
				FTimer.HostTime = hosttime;
				double speed;
				FSpeedInput.GetValue(0, out speed);
				FTimer.Speed = speed;
			}
			
			//update time
			FSeekingOut.SetValue(0, System.Convert.ToDouble(FTimer.IsSeeking));
			FScratchingOut.SetValue(0, System.Convert.ToDouble(SliceArea.MouseState == TLMouseState.msDraggingTimeBar));
			FTimer.Evaluate();
			FTimeOut.SliceCount = FTimer.TimeCount;
			for (int i=0; i<FTimer.TimeCount; i++)
				FTimeOut.SetValue(0, FTimer.GetTime(i));

			if (FTimer.IsRunning)
			{
				var pixThresh = SliceArea.Width / 20;
				if (TimeBarModeBox.Text == "Jump")
				{
					var timeAsX = FTimer.GetTimeAsX(0);
					
					//if timebar not in view, bring into view
					if ((timeAsX < pixThresh) || (timeAsX > SliceArea.Width - pixThresh))
					{
						//compute timeoffset
						var currentStart = FTransformer.XPosToTime(0);
						var targetStart = FTimer.GetTime(0);
						var offTime = targetStart - currentStart;
						//convert to pixels
						var offX = offTime * FTransformer.GTimeScale - pixThresh;
						FTransformer.TranslateTime(-offX);
						
						FTransformer.ApplyTransformation();
						SliceArea.Invalidate();
					}
				}
				else if (TimeBarModeBox.Text == "Follow")
				{
					//scroll so that timebar is in center
					//compute timeoffset
					var currentStart = FTransformer.XPosToTime(0);
					var targetStart = FTimer.GetTime(0) - (FTopRuler.VisibleTimeRange / 2) / FTransformer.GTimeScale;
					var offTime = targetStart - currentStart;
					//convert to pixels
					var offX = offTime * FTransformer.GTimeScale;
					FTransformer.TranslateTime(-offX);
					
					FTransformer.ApplyTransformation();
					SliceArea.Invalidate();
				}
			}
			FPlayingOut.SetValue(0, System.Convert.ToDouble(FTimer.IsRunning));
			
			int index = 0;
			foreach (TLBasePin p in FOutputPins)
			{
				if (p is TLRulerPin)
					p.Evaluate(FTimer.GetTime(0));
				else if (!(p is TLAutomataPin)) //as automata is already evaluated above
					p.Evaluate(FTimer.GetTime(index++));
			}
			/*	for (int i = 0; i<FOutputPins.Count;i++)
			{
				FOutputPins[i].Evaluate(GTimer.GetTime(i));
			}*/

			SliceArea.Evaluate();

			FTimer.InvalidateTimes();
			if (FFirstFrame)
			{
				//draw whole slicearea after all keyframes have been loaded
				SliceArea.Refresh();
				FFirstFrame = false;
				
				//for backwards compatibility update pinsettings once after startup:
				UpdatePinSettingsFromActualPinLayout();
				
				foreach (TLBasePin pin in FOutputPins)
					if (pin is TLPin)
						(pin as TLPin).UpdateSliceSpecificSettings();
			}
		}
		#endregion mainloop

		private void GUISettingsChanged()
		{
			if (FGUISettings.SliceCount == 0)
				return;
			
			TimeBarModeBox.SelectedItem = GetGUIParameter("TIMEBAR", "Mode", "Manual");
			SplitContainer.SplitterDistance = int.Parse(GetGUIParameter("SPLITTER", "Position", "20"));
		}
		
		private void PinSettingsChanged()
		{
			string settings, pinName;
			TLPinType pinType;
			int currentID;
			bool found;
			TLBasePin tmpPin;
			XmlNode pinSettings;
			XmlAttribute attr;
			
			int i;
			for (i=0; i<FPinSettings.SliceCount; i++)
			{
				currentID = 0;
				found = false;
				settings = "";
				FPinSettings.GetString(i, out settings);
				
				settings = settings.TrimEnd();
				if (settings == "")
					return;
				
				FSettings.LoadXml(settings);
				pinSettings = FSettings.SelectSingleNode(@"//PIN");
				attr = pinSettings.Attributes.GetNamedItem("Name") as XmlAttribute;
				pinName = attr.Value;
				
				foreach (TLBasePin bp in FOutputPins)
				{
					if (bp.Name == pinName)
					{
						found = true;
						break;
					}
					currentID++;
				}
				
				if (found)	//if list of OutputPins contains a pin with this pinname, then move it to this place
				{
					if (currentID != i)
					{
						tmpPin = FOutputPins[currentID];
						FOutputPins.Remove(tmpPin);
						FOutputPins.Insert(i, tmpPin);
					}
				}
				else //if list of OutputPins does not contain a pin with this pin name create it and insert it here
				{
					TLBasePin newPin = null;
					attr = pinSettings.Attributes.GetNamedItem("Type") as XmlAttribute;
					pinType = (TLPinType) Enum.Parse(typeof(TLPinType), attr.Value);
					
					switch (pinType)
					{
						case TLPinType.Automata:
							{
								newPin = new TLAutomataPin(FHost, FTransformer, FOutputPins.Count, pinSettings);
								FAutomata = (TLAutomataPin) newPin;
								FTimer.Automata = FAutomata;
								
								if (FAutomataCreatedViaGUI)
									FAutomata.InitializeWithLoop();
								
								AutomataCheckBox.Checked = true;
								break;
							}
						case TLPinType.Ruler:
							{
								newPin = new TLRulerPin(FTransformer, FOutputPins.Count, pinSettings, FTopRuler != null);
								if (FTopRuler == null)
									FTopRuler = (TLRulerPin) newPin;
								
								(newPin as TLRulerPin).Timer = FTimer;
								break;
							}
						case TLPinType.Value:
							{
								newPin = new TLValuePin(FHost, FTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.String:
							{
								newPin = new TLStringPin(FHost, FTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.Color:
							{
								newPin = new TLColorPin(FHost, FTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.Midi:
							{
								newPin = new TLMidiPin(FHost, FTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.Wave:
							{
								//newPin = new TLWavPin(FHost, GTransformer, FOutputPins.Count, pinSettings);
								break;
							}
					}
					
					newPin.OnPinChanged += new PinHandler(PinChangedCB);
					newPin.OnRedraw += new PinHandler(RedrawCB);
					newPin.OnRemovePin += new PinHandler(RemovePinCB);

					int parent;
					try
					{
						attr = pinSettings.Attributes.GetNamedItem("Parent") as XmlAttribute;
						parent = int.Parse(attr.Value);
						
						switch (parent)
						{
							case 0:
								{
									PinHeaderPanel0.Controls.Add(newPin);
									FOutputPins.Insert(Math.Max(0, PinHeaderPanel0.Controls.Count-1), newPin);
									break;
								}
							case 1:
								{
									PinHeaderPanel1.Controls.Add(newPin);
									FOutputPins.Add(newPin);
									break;
								}
						}
					}
					catch
					{
						if (newPin is TLRulerPin || newPin is TLAutomataPin)
						{
							PinHeaderPanel0.Controls.Add(newPin);
							FOutputPins.Insert(Math.Max(0, PinHeaderPanel0.Controls.Count-1), newPin);
						}
						else
						{
							PinHeaderPanel1.Controls.Add(newPin);
							FOutputPins.Add(newPin);
						}
					}
					
					for (int j=0; j<FOutputPins.Count; j++)
						FOutputPins[j].Order = j;

					newPin.Dock = DockStyle.Top;
					newPin.BringToFront();
				}
			}
			
			//remove all remaining pins
			for (int j=FOutputPins.Count-1; j==i; j--)
			{
				tmpPin = FOutputPins[j];
				FOutputPins.RemoveAt(j);
				if (PinHeaderPanel0.Controls.Contains(tmpPin))
					PinHeaderPanel0.Controls.Remove(tmpPin);
				else
					PinHeaderPanel1.Controls.Remove(tmpPin);
				tmpPin.DestroyPins();
				tmpPin = null;
			}
			
			SliceArea.Invalidate();
		}
		
		private void AddPin(TLPinType PinType)
		{
			XmlAttribute attr;
			XmlNode pin = FSettings.CreateElement("PIN");
			attr = FSettings.CreateAttribute("Name");
			attr.Value = GetUniqueDefaultName(PinType.ToString());
			pin.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("Type");
			attr.Value = PinType.ToString();
			pin.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("SliceCount");
			attr.Value = "1";
			pin.Attributes.Append(attr);
			
			UpdatePinSettingsList();
			
			if ((FPinSettingsList.Count == 0) || (!(PinType == TLPinType.Ruler) || (PinType == TLPinType.Automata)))
				FPinSettingsList.Add(pin.OuterXml);
			else
				FPinSettingsList.Insert(Math.Max(0, PinHeaderPanel0.Controls.Count-1), pin.OuterXml);
			
			UpdatePinSettings();
		}
		
		private void RemovePinCB(TLBasePin Pin)
		{
			if (MessageBox.Show("You sure?", "Deleting Pin: " + Pin.Name, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (Pin is TLRulerPin)
				{
					List<TLBasePin> rp = FOutputPins.FindAll(delegate(TLBasePin p) { return p is TLRulerPin; });
					if (rp.Count == 1)
						return;
				}
				
				int pinID = FOutputPins.IndexOf(Pin);
				MovePin (pinID, -1);
			}
		}
		
		private void UpdatePinSettingsFromActualPinLayout()
		{
			//get an arraylist fresh from the current pinsettings
			FPinSettingsList.Clear();
			for (int i=0; i<FOutputPins.Count; i++)
				FPinSettingsList.Add(FOutputPins[i].Settings.OuterXml);
			
			UpdatePinSettings();
		}
		
		private void UpdatePinSettingsList()
		{
			//get an arraylist fresh from the current pinsettings
			FPinSettingsList.Clear();
			string sliceSettings;
			for (int i=0; i<FPinSettings.SliceCount; i++)
			{
				FPinSettings.GetString(i, out sliceSettings);
				sliceSettings = sliceSettings.TrimEnd();
				if (sliceSettings != string.Empty)
					FPinSettingsList.Add(sliceSettings);
			}
		}
		
		private void UpdatePinSettings()
		{
			//write the arraylist back to the PinSettings pin
			FPinSettings.SliceCount = FPinSettingsList.Count;
			
			FBlockConfigurate = true;
			for (int i=0; i<FPinSettings.SliceCount; i++)
				FPinSettings.SetString(i, (string) FPinSettingsList[i]);
			FBlockConfigurate = false;
			
			Configurate(FPinSettings);
		}
		
		//call with NewIdx = -1 to remove the pin
		private void MovePin(int OldIdx, int NewIdx)
		{
			UpdatePinSettingsList();
			
			string tmpSliceSettings = (string) FPinSettingsList[OldIdx];
			FPinSettingsList.RemoveAt(OldIdx);
			
			if (NewIdx >= 0)
				FPinSettingsList.Insert(NewIdx, tmpSliceSettings);
			
			UpdatePinSettings();

			for (int i=0; i<FOutputPins.Count; i++)
				FOutputPins[i].Order = i;
		}
		
		private void PinChangedCB(TLBasePin Pin)
		{
			int idx = FOutputPins.IndexOf(Pin);
			
			//XmlNode pin = FSettings.CreateElement("PIN");
			FPinSettings.SetString(idx, FOutputPins[idx].Settings.OuterXml);
			
			SliceArea.Refresh();
		}
		
		private void RedrawCB(TLBasePin P)
		{
			SliceArea.Invalidate(new Rectangle(0, P.Top, SliceArea.Width, P.Height));
		}
		
		private string GetUniqueDefaultName(string TypeName)
		{
			List<string> pinnames = new List<string>();
			
			//create a temporary list of all current pinnames
			foreach (TLBasePin bp in FOutputPins)
				pinnames.Add(bp.Name);
			
			//create a new default name until this one is not already taken
			int id = 0;
			string newname;
			do
			{
				newname = TypeName + id.ToString();
				id++;
			}
			while (pinnames.Contains(newname));
			
			return newname;
		}
		
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (e.Y - MainMenu.Height < SplitContainer.SplitterDistance)
			{
				//not sure why, but need to scroll twice here for desired effect
				PinHeaderPanel0.VerticalScroll.Value = Math.Min(PinHeaderPanel0.VerticalScroll.Maximum, Math.Max(0, PinHeaderPanel0.VerticalScroll.Value - e.Delta/5));
				PinHeaderPanel0.VerticalScroll.Value = Math.Min(PinHeaderPanel0.VerticalScroll.Maximum, Math.Max(0, PinHeaderPanel0.VerticalScroll.Value - e.Delta/5));
			}
			else
			{
				//not sure why, but need to scroll twice here for desired effect
				PinHeaderPanel1.VerticalScroll.Value = Math.Min(PinHeaderPanel1.VerticalScroll.Maximum, Math.Max(0, PinHeaderPanel1.VerticalScroll.Value - e.Delta/5));
				PinHeaderPanel1.VerticalScroll.Value = Math.Min(PinHeaderPanel1.VerticalScroll.Maximum, Math.Max(0, PinHeaderPanel1.VerticalScroll.Value - e.Delta/5));
			}
		}
		
		//http://msdn2.microsoft.com/en-us/library/system.windows.forms.control.processcmdkey.aspx
		//This method is only called when the control is hosted in a Windows Forms application or as an ActiveX control.
		//seems to work in standalone and docked to a vvvv-window though.
		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			return base.ProcessCmdKey(ref m, keyData);
		}
		
		protected override bool ProcessKeyPreview(ref Message m)
		{
			const int WM_KEYDOWN = 0x100;
			//const int WM_KEYUP = 0x101;
			
			if (m.Msg == WM_KEYDOWN)
			{
				//FHost.Log(TLogType.Debug, "keydown");
				KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
				
				if (ke.KeyCode == Keys.Space)
				{
					FTimer.IsRunning = !FTimer.IsRunning;
					UpdatePlayButton();
					return true;
				}
				else if (ke.KeyCode == Keys.Back)
				{
					FTimer.IsRunning = false;
					FTimer.SetTime(0, 0);
					return true;
				}
				else if (ke.KeyCode == Keys.Home)
				{
					if (FAutomata != null)
						FTimer.SetTime(0, FAutomata.OutputSlices[0].KeyFrames[0].Time);
					else
						FTimer.SetTime(0, 0);
					return true;
				}
				else if (ke.KeyCode == Keys.End)
				{
					if (FAutomata != null)
						FTimer.SetTime(0, FAutomata.OutputSlices[0].KeyFrames[FAutomata.OutputSlices[0].KeyFrames.Count-2].Time);
					return true;
				}
				else if (ke.KeyCode == Keys.PageUp)
				{
					if (FAutomata != null)
						FTimer.SetTime(0, FAutomata.NextState.Time-TLTime.MinTimeStep);
					return true;
				}
				else if (ke.KeyCode == Keys.PageDown)
				{
					if (FAutomata != null)
						FTimer.SetTime(0, FAutomata.PreviousState.Time-TLTime.MinTimeStep);
					return true;
				}
				else if (ke.KeyCode == Keys.Delete)
				{
					foreach (TLBasePin bp in FOutputPins)
						SliceArea.DeleteSelectedKeyFrames(bp);
					return true;
				}
				else if (ke.KeyCode == Keys.L && ke.Control)
				{
					SliceArea.TimeAlignSelectedKeyFrames();
					return true;
				}
				else if (ke.KeyCode == Keys.A && ke.Control && !ke.Shift)
				{
					SliceArea.SelectAll(true);
					return true;
				}
				else if (ke.KeyCode == Keys.A && ke.Control && ke.Shift)
				{
					SliceArea.SelectAllOfPin();
					return true;
				}
				else if (ke.KeyCode == Keys.A && ke.Shift)
				{
					SliceArea.SelectAllOfSlice();
					return true;
				}
				//copy/paste/undo/selectall
			}
			
			bool r = base.ProcessKeyPreview(ref m);
			
			//FHost.Log(TLogType.Debug, r.ToString());
			
			return r;
		}
		
		protected override bool ProcessDialogKey(Keys keyData)
		{
			//FHost.Log(TLogType.Debug, "dialogkey");
			return false;
		}
		
		private void TransformationChangedCB(double Translation, double Scaling)
		{
			FTranslateInput.SetValue(0, Math.Round(Translation, 4));
			FScaleInput.SetValue(0, Math.Round(Scaling, 4));
		}

		void StopButtonClick(object sender, EventArgs e)
		{
			FTimer.IsRunning = false;
			FTimer.SetTime(0, 0);
			UpdatePlayButton();
		}
		
		void PlayButtonClick(object sender, EventArgs e)
		{
			FTimer.IsRunning = !FTimer.IsRunning;
			UpdatePlayButton();
		}
		
		void UpdatePlayButton()
		{
			if (FTimer.IsRunning)
				PlayButton.Text = "| |";
			else
				PlayButton.Text = "Play";
		}
		
		void PinButtonClick(object sender, EventArgs e)
		{
			if (sender == this.RulerButton)
				AddPin(TLPinType.Ruler);
			else if (sender == this.ValueButton)
				AddPin(TLPinType.Value);
			else if (sender == this.StringButton)
				AddPin(TLPinType.String);
			else if (sender == this.ColorButton)
				AddPin(TLPinType.Color);
			else if (sender == this.MidiButton)
				AddPin(TLPinType.Midi);
			else if (sender == this.WavButton)
				AddPin(TLPinType.Wave);
		}
		
		void AutomataCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (AutomataCheckBox.Checked)
			{
				FAutomataCreatedViaGUI = true;
				
				if (FAutomata == null)
				{
					AddPin (TLPinType.Automata);
					//pinsettings are added as lastslice
					//calling a movepin now to move the pinsettings of automatapin to its default position=1;
					MovePin (FOutputPins.Count - 1, PinHeaderPanel0.Controls.Count - 1);
					if (SplitContainer.SplitterDistance < FAutomata.Top + FAutomata.Height)
						SplitContainer.SplitterDistance += FAutomata.Height;
				}
			}
			else
			{
				var tempTop = FAutomata.Top;
				var tempHeight = FAutomata.Height;
				RemovePinCB(FAutomata);
				
				if (FOutputPins.IndexOf(FAutomata) == -1)
				{
					FAutomata = null;
					FTimer.Automata = null;
					
					if (SplitContainer.SplitterDistance >= tempTop + tempHeight)
						SplitContainer.SplitterDistance -= tempHeight;
				}
				else
					AutomataCheckBox.CheckState = CheckState.Checked;
			}
		}

		int DropToId(Panel PinPanel, Point MousePos)
		{
			Point p = this.PointToClient(MousePos);
			TLBasePin bp = FOutputPins.Find(delegate (TLBasePin pin){return (pin.Top < p.Y && pin.Top + pin.Height > p.Y);});
			
			if (bp == null)
				return 0;
			
			if (p.Y < bp.Top + bp.Height/2)
				return PinPanel.Controls.IndexOf(bp) + 1;
			else
				return PinPanel.Controls.IndexOf(bp);
		}
		
		void PinHeaderPanel0DragEnter(object sender, DragEventArgs e)
		{
			InsertPreview.Show();
		}
		
		void PinHeaderPanel0DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All;

			int id = DropToId(PinHeaderPanel0, new Point(e.X, e.Y - MainMenu.Height));
			if (id >= PinHeaderPanel0.Controls.Count)
				InsertPreview.Location = new Point(0, PinHeaderPanel0.Controls[id-1].Top);
			else
				InsertPreview.Location = new Point(0, PinHeaderPanel0.Controls[id].Bottom);
		}
		
		void PinHeaderPanel0DragDrop(object sender, DragEventArgs e)
		{
			this.SuspendLayout();
			if (e.Data.GetDataPresent(DataFormats.Serializable))
			{
				int id = DropToId(PinHeaderPanel0, new Point(e.X, e.Y - MainMenu.Height));
				TLBasePin droppedPin = (TLBasePin) e.Data.GetData(DataFormats.Serializable);
				
				if (droppedPin.Parent == PinHeaderPanel1)
				{
					//PinHeaderPanel1.Controls.Remove(droppedPin);
					PinHeaderPanel0.Controls.Add(droppedPin);
				}
				else //move in same panel
				{
					int oldID = PinHeaderPanel0.Controls.IndexOf(droppedPin);
					if (oldID < id)
						id -= 1;
				}
				
				PinHeaderPanel0.Controls.SetChildIndex(droppedPin, id);
				
				//old index of pin in FOutputPins list
				int oldIdx = FOutputPins.IndexOf(droppedPin);
				
				//new index in list
				int newIdx = PinHeaderPanel0.Controls.Count - id - 1;
				
				MovePin(oldIdx, newIdx);
			}
			InsertPreview.Hide();
			this.ResumeLayout(true);
		}
		
		void PinHeaderPanel1DragEnter(object sender, DragEventArgs e)
		{
			InsertPreview.Show();
		}
		
		void PinHeaderPanel1DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All;

			int id = DropToId(PinHeaderPanel1, new Point(e.X, e.Y - MainMenu.Height));
			if (id >= PinHeaderPanel1.Controls.Count)
				InsertPreview.Location = new Point(0, PinHeaderPanel1.Controls[id-1].Top + SplitContainer.SplitterRectangle.Top + 5);
			else
				InsertPreview.Location = new Point(0, PinHeaderPanel1.Controls[id].Bottom + SplitContainer.SplitterRectangle.Top + 5);
		}
		
		void PinHeaderPanel1DragDrop(object sender, DragEventArgs e)
		{
			this.SuspendLayout();
			if (e.Data.GetDataPresent(DataFormats.Serializable))
			{
				int id = DropToId(PinHeaderPanel1, new Point(e.X, e.Y - MainMenu.Height));
				TLBasePin droppedPin = (TLBasePin) e.Data.GetData(DataFormats.Serializable);
				
				if (droppedPin.Parent == PinHeaderPanel0)
				{
					//PinHeaderPanel0.Controls.Remove(droppedPin);
					PinHeaderPanel1.Controls.Add(droppedPin);
				}
				else //move in same panel
				{
					int oldID = PinHeaderPanel1.Controls.IndexOf(droppedPin);
					if (oldID < id)
						id -= 1;
				}

				PinHeaderPanel1.Controls.SetChildIndex(droppedPin, id);
				
				//old index of pin in FOutputPins list
				int oldIdx = FOutputPins.IndexOf(droppedPin);
				
				//new index in list
				int newIdx = PinHeaderPanel0.Controls.Count + PinHeaderPanel1.Controls.Count - id - 1;
				
				MovePin(oldIdx, newIdx);
			}
			InsertPreview.Hide();
			this.ResumeLayout(true);
		}
		
		private void SetGUIParameter(string tag, string attribute, string value)
		{
            if (FGUISettings == null) //while still starting up
                return; 

			//load current settings
			string s;
			var splitterPos = "0";
			FGUISettings.GetString(0, out s);
			if (string.IsNullOrEmpty(s))
				FSettings.RemoveAll();
			else
			{
				FSettings.LoadXml(s);
				
				//check for legacy settings (SPLITTER was root element)
				
				if (FSettings.DocumentElement.Name == "SPLITTER")
				{
					splitterPos = FSettings.DocumentElement.Attributes.GetNamedItem("Position").Value;
					FSettings.RemoveAll();
				}
			}
			
			//create element if it doesn't exist
			var guiSettings = FSettings.SelectSingleNode(@"//GUI");
			if (guiSettings == null)
			{
				guiSettings = FSettings.CreateElement("GUI");
				FSettings.AppendChild(guiSettings);
			}
			
			//add legacy splitter setting if it was there before
			if (splitterPos != "0")
			{
				var e = FSettings.CreateElement("SPLITTER");
				guiSettings.AppendChild(e);
				var a = FSettings.CreateAttribute("Position");
				a.Value = splitterPos;
				e.Attributes.SetNamedItem(a);
			}
			
			var element = guiSettings.SelectSingleNode(tag);
			if (element == null)
			{
				element = FSettings.CreateElement(tag);
				guiSettings.AppendChild(element);
			}
			
			//set given attribute
			var attr = FSettings.CreateAttribute(attribute);
			attr.Value = value;
			element.Attributes.SetNamedItem(attr);
			
			//write settings back to pin
			FBlockConfigurate = true;
			FGUISettings.SliceCount = 1;
			FGUISettings.SetString(0, guiSettings.OuterXml);
			FBlockConfigurate = false;
		}
		
		private string GetGUIParameter(string tag, string attribute, string defaultValue)
		{
			//load current settings
			string s;
			FGUISettings.GetString(0, out s);
			if (string.IsNullOrEmpty(s))
				return defaultValue;
			else
				FSettings.LoadXml(s);
			
			//select element
			var element = FSettings.SelectSingleNode(@"/GUI/" + tag);
			if (element == null)
				return defaultValue;
			
			var attr = element.Attributes.GetNamedItem(attribute) as XmlAttribute;
			return attr.Value;
		}
		
		void SplitContainerSplitterMoved(object sender, SplitterEventArgs e)
		{
			SetGUIParameter("SPLITTER", "Position", SplitContainer.SplitterDistance.ToString());

			SliceArea.SplitterPosition = SplitContainer.SplitterDistance;
			SliceArea.Refresh();
			
			foreach (var p in PinHeaderPanel1.Controls)
				(p as TLPin).UpdateKeyFrameAreas();
		}
		
		void ToggleCollapseButtonClick(object sender, EventArgs e)
		{
			int count = 0;
			foreach(TLBasePin pin in FOutputPins)
				if (pin.Collapsed)
					count++;
			
			bool collapsed = count > FOutputPins.Count/2;
			foreach(TLBasePin pin in FOutputPins)
				pin.Collapsed = !collapsed;
		}
		
		void TimeBarModeBoxSelectionChangeCommitted(object sender, EventArgs e)
		{
			SetGUIParameter("TIMEBAR", "Mode", TimeBarModeBox.SelectedItem.ToString());
		}
	}
}