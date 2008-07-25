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

namespace VVVV.Nodes
{
	public class TimelinerPlugin: UserControl, IPlugin
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
		//private IValueOut FSeekOut;

		// VAIRABLES
		///////////////////////
		private bool FDoSetTime;
		private double FInputTime;
		private bool FFirstFrame = true;
		private bool FTransformationChanged = true;
		private bool FBlockConfigurate = false;

		private bool FAutomataCreatedViaGUI = false;
		
		// LISTS
		///////////////////////
		private System.Collections.Generic.List<VVVV.Nodes.Timeliner.TLBasePin> FOutputPins = new List<TLBasePin>();

		// CLASSES
		///////////////////////
		private TLRulerPin GTopRuler;
		private TLAutomataPin FAutomata;
		private TLTransformer GTransformer = new TLTransformer();
		private TLTime GTimer = new TLTime();
		
		private void InitializeComponent()
		{
			this.MainMenu = new System.Windows.Forms.Panel();
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
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainMenu
			// 
			this.MainMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
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
			// AutomataCheckBox
			// 
			this.AutomataCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.AutomataCheckBox.Location = new System.Drawing.Point(237, 0);
			this.AutomataCheckBox.Name = "AutomataCheckBox";
			this.AutomataCheckBox.Size = new System.Drawing.Size(34, 24);
			this.AutomataCheckBox.TabIndex = 8;
			this.AutomataCheckBox.Text = "A";
			this.AutomataCheckBox.UseVisualStyleBackColor = true;
			this.AutomataCheckBox.CheckedChanged += new System.EventHandler(this.AutomataCheckBoxCheckedChanged);
			// 
			// ColorButton
			// 
			this.ColorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ColorButton.Location = new System.Drawing.Point(389, 0);
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
			this.RulerButton.Location = new System.Drawing.Point(277, 0);
			this.RulerButton.Name = "RulerButton";
			this.RulerButton.Size = new System.Drawing.Size(31, 23);
			this.RulerButton.TabIndex = 6;
			this.RulerButton.Text = "+R";
			this.RulerButton.UseVisualStyleBackColor = true;
			this.RulerButton.Click += new System.EventHandler(this.PinButtonClick);
			// 
			// StringButton
			// 
			this.StringButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.StringButton.Location = new System.Drawing.Point(351, 0);
			this.StringButton.Name = "StringButton";
			this.StringButton.Size = new System.Drawing.Size(32, 23);
			this.StringButton.TabIndex = 4;
			this.StringButton.Text = "+S";
			this.StringButton.UseVisualStyleBackColor = true;
			this.StringButton.Click += new System.EventHandler(this.PinButtonClick);
			// 
			// ValueButton
			// 
			this.ValueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ValueButton.Location = new System.Drawing.Point(314, 0);
			this.ValueButton.Name = "ValueButton";
			this.ValueButton.Size = new System.Drawing.Size(31, 23);
			this.ValueButton.TabIndex = 3;
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
			this.InsertPreview.BackColor = System.Drawing.Color.Lime;
			this.InsertPreview.Location = new System.Drawing.Point(206, 111);
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
			this.PinHeaderPanel0.DragOver += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel0DragOver);
			this.PinHeaderPanel0.DragDrop += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel0DragDrop);
			this.PinHeaderPanel0.DragEnter += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel0DragEnter);
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
			this.PinHeaderPanel1.DragOver += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel1DragOver);
			this.PinHeaderPanel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel1DragDrop);
			this.PinHeaderPanel1.DragEnter += new System.Windows.Forms.DragEventHandler(this.PinHeaderPanel1DragEnter);
			// 
			// TimelinerPlugin
			// 
			this.Controls.Add(this.PinPanel);
			this.Controls.Add(this.MainMenu);
			this.Name = "TimelinerPlugin";
			this.Size = new System.Drawing.Size(688, 355);
			this.MainMenu.ResumeLayout(false);
			this.PinPanel.ResumeLayout(false);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Panel InsertPreview;
		private System.Windows.Forms.Panel PinHeaderPanel0;
		private System.Windows.Forms.Panel PinHeaderPanel1;
		private VVVV.Nodes.Timeliner.TLSliceArea SliceArea;
		private System.Windows.Forms.SplitContainer SplitContainer;
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

			this.BackColor = System.Drawing.Color.LightGray;

			MainMenu.BringToFront();
			PinPanel.BringToFront();
			
			//PinHeaderPanel0.MouseWheel += new MouseEventHandler(OnMouseWheel);
			GTransformer.OnTransformationChanged += new TransformationChangedHandler(TransformationChangedCB);
			
			GNumberFormat.NumberDecimalSeparator = ".";
			SliceArea.OutputPins = FOutputPins;
			SliceArea.Transformer = GTransformer;
			SliceArea.Timer = GTimer;
			SliceArea.SplitterPosition = 20;
			
			FSettings = new XmlDocument();
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
        			GTopRuler = null;
        			GTimer = null;
        			GTransformer = null;
        			
        			FHost.DeletePin(FTimeInput);
        			FTimeInput = null;
        			
        			FHost.DeletePin(FPlayInput);
        			FPlayInput = null;
        			
        			FHost.DeletePin(FTranslateInput);
        			FTranslateInput = null;
        			
        			FHost.DeletePin(FScaleInput);
        			FScaleInput = null;
        			
        			FHost.DeletePin(FSetTime);
        			FSetTime = null;
        			
        			FHost.DeletePin(FTimeOut);
        			FTimeOut = null;
        			
        			//FHost.DeletePin(FSeekOut);
        			//FSeekOut = null;
        			
        			FHost.DeletePin(FPinSettings);
        			FPinSettings = null;
        			
        			FHost.DeletePin(FGUISettings);
        			FGUISettings = null;
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
				//..
        	}
        	FDisposed = true;
        }
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		public static IPluginInfo PluginInfo
		{
			get
			{
				IPluginInfo Info = new PluginInfo();
				
				// PLUGIN INFORMATIONS
				///////////////////////
				Info.Name = "Timeliner";
				Info.Category = "Animation";
				Info.Version = "";
				Info.Help = "";
				Info.Bugs = "";
				Info.Credits = "";
				Info.Warnings = "";
				Info.InitialBoxSize = new Size(400, 200);
				Info.InitialWindowSize = new Size(600, 300);
				Info.InitialComponentMode = TComponentMode.InAWindow;
				
				// STACK TRACES
				/////////////// 
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
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
			AddPin(TLPinType.Ruler);
		}

		private void createInputPins()
		{
			// TIME IN
			///////////////////////
			FHost.CreateValueFastInput("Time In", 1, null, TSliceMode.Single, TPinVisibility.True, out FTimeInput);
			FTimeInput.SetSubType(Double.MinValue, Double.MaxValue, 0.01D, 0, false, false, false);
			
			// PLAY IN
			///////////////////////
			FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out FPlayInput);
			FPlayInput.SetSubType(0, 1, 1, 0, false, true, true);
			
			// LOOK AT IN
			///////////////////////
			FHost.CreateValueConfig("Translate", 1, null, TSliceMode.Single, TPinVisibility.True, out FTranslateInput);
			FTranslateInput.SetSubType(Double.MinValue, Double.MaxValue, 0.1, 0, false, false, false);
			
			// SPAN IN
			///////////////////////
			FHost.CreateValueConfig("Scale", 1, null, TSliceMode.Single, TPinVisibility.True, out FScaleInput);
			FScaleInput.SetSubType(0.1, 1000, 0.1, 50, false, false, false);
			
			// SET TIME IN
			///////////////////////
			FHost.CreateValueInput("Set Time", 1, null, TSliceMode.Single, TPinVisibility.True, out FSetTime);
			FSetTime.SetSubType(0, 1, 1, 0, false, false, true);

			// ONLY VISIBLE IN INSPECTOR
			////////////////////////////
			/// 
			FHost.CreateStringConfig("GUI Settings", TSliceMode.Dynamic, TPinVisibility.Hidden, out FGUISettings);
			FGUISettings.SliceCount = 0;
			FGUISettings.SetSubType("", false);
			
			FHost.CreateStringConfig("Pin Settings", TSliceMode.Dynamic, TPinVisibility.Hidden, out FPinSettings);
			FPinSettings.SliceCount = 0;
			FPinSettings.SetSubType("", false);
		}

		private void createOutputPins()
		{
			// TIME OUT
			///////////////////////
			FHost.CreateValueOutput("Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTimeOut);
			FTimeOut.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, false);
			
			//FHost.CreateValueOutput("Seek", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSeekOut);
			//FSeekOut.SetSubType(0, 1, 1, 0, true, false, true);
		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//create pins here according to valuepinnames slice-content
			System.Diagnostics.Debug.WriteLine("Configurate: " +Input.Name + " - SliceCount: " + Input.SliceCount);
			
			if (FBlockConfigurate) 
				return; 
			
			//this is a hack
			//only needed because vvvv calls configcallback after setting of every slice while SettingSpreadAsString onload
			//should better fill whole spread with saved values and then call configcallback
			string s;
			List<string> pinsettings = new List<string>();
			bool doublesettings = false;
			for (int i=0; i<FPinSettings.SliceCount; i++)
			{
				FPinSettings.GetString(i, out s);
				//System.Diagnostics.Debug.WriteLine("pinnames: " + s);
				if (pinsettings.IndexOf(s) >= 0)
				{
					doublesettings = true;
					break;
				}
				else
					pinsettings.Add(s);
			}
			
			//only go on if every settings slice is unique: as at least pin name must be unique in all pins!
			if ((doublesettings) || (FPinSettings.SliceCount == 0))
				return;
			//hack end.-
			
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
					GTimer.IsRunning = false;
				else
					GTimer.IsRunning = true;
			}
			
			//lookat
			if (FTransformationChanged)
			{
				FTranslateInput.GetValue(0, out dval);
				GTransformer.GTimeTranslate = dval;
				FScaleInput.GetValue(0, out dval);
				GTransformer.GTimeScale = dval;
				
				GTransformer.ApplyTransformation();
				
				FTransformationChanged = false;
				SliceArea.Invalidate();
			}
			
			//set time
			if (FSetTime.PinIsChanged)
			{
				FSetTime.GetValue(0, out dval);
				FDoSetTime = dval > 0.5;
			}
			
			if (FDoSetTime)
			{
				FTimeInput.GetValue(0, out dval);
				FInputTime = dval;
				GTimer.CurrentTime = FInputTime;
			}
			else
			{
				double hosttime;
				FHost.GetCurrentTime(out hosttime);
				GTimer.HostTime = hosttime;
			}
			
			//update time
			GTimer.Evaluate();
			FTimeOut.SetValue(0, GTimer.CurrentTime);
			
			SliceArea.Evaluate();
			
			//statetimes, statenames, stateexp
			
			//set outputs
			//FSeekOut.SetValue(0,0);
			
			for (int i = 0; i<FOutputPins.Count;i++)
			{
				FOutputPins[i].Evaluate(GTimer.CurrentTime);
			}
			
			if (FFirstFrame)
			{
				//draw whole slicearea after all keyframes have been loaded
				SliceArea.Refresh();
				FFirstFrame = false;
			}
		}
		
		#endregion mainloop

		private void GUISettingsChanged()
		{
			string settings;
			XmlNode guiSettings;
			XmlAttribute attr;
			
			FGUISettings.GetString(0, out settings);
			
			settings = settings.TrimEnd();
			if (settings == "")
				return;
			
			FSettings.LoadXml(settings);
			guiSettings = FSettings.SelectSingleNode(@"//SPLITTER");
			attr = guiSettings.Attributes.GetNamedItem("Position") as XmlAttribute;
			SplitContainer.SplitterDistance = Convert.ToInt32(attr.Value);
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
								newPin = new TLAutomataPin(FHost, GTransformer, FOutputPins.Count, pinSettings);
								FAutomata = (TLAutomataPin) newPin;
								GTimer.Automata = FAutomata;
								
								if (FAutomataCreatedViaGUI)
									FAutomata.InitializeWithLoop();
								
								AutomataCheckBox.Checked = true;
								break;
							}
						case TLPinType.Ruler:
							{
								newPin = new TLRulerPin(GTransformer, FOutputPins.Count, pinSettings, GTopRuler != null);
								if (GTopRuler == null)
									GTopRuler = (TLRulerPin) newPin;
								break;
							}
						case TLPinType.Value:
							{
								newPin = new TLValuePin(FHost, GTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.String:
							{
								newPin = new TLStringPin(FHost, GTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.Color:
							{
								newPin = new TLColorPin(FHost, GTransformer, FOutputPins.Count, pinSettings);
								break;
							}
						case TLPinType.Midi:
							{
								//newPin = new TLMidiPin(FHost, GTransformer, FOutputPins.Count, pinSettings);
								break;
							}
							
					}
					
					
					newPin.OnPinChanged += new PinHandler(PinChangedCB);
					newPin.OnRedraw += new PinHandler(RedrawCB);
					newPin.OnRemovePin += new PinHandler(RemovePinCB);

					int parent = -1;
					try
					{
						attr = pinSettings.Attributes.GetNamedItem("Parent") as XmlAttribute;
						parent = int.Parse(attr.Value);
					}
					catch
					{}
					
					if (parent == -1)
					{
						if (newPin is TLRulerPin || newPin is TLAutomataPin)
						{
							PinHeaderPanel0.Controls.Add(newPin);
							FOutputPins.Insert(PinHeaderPanel0.Controls.Count-1, newPin);
						}
						else
						{
							PinHeaderPanel1.Controls.Add(newPin);
							FOutputPins.Add(newPin);
						}
					}
					else
					{
						switch (parent)
						{
								case 0: 
								{
									PinHeaderPanel0.Controls.Add(newPin);
									FOutputPins.Insert(PinHeaderPanel0.Controls.Count-1, newPin);
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
			FPinSettings.SliceCount++;
			
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
			
			FPinSettings.SetString(FPinSettings.SliceCount-1, pin.OuterXml);
		}
		
		private void RemovePinCB(TLBasePin Pin)
		{
			if (Pin is TLRulerPin)
			{
				List<TLBasePin> rp = FOutputPins.FindAll(delegate(TLBasePin p) {return p is TLRulerPin;});
				if (rp.Count == 1)
					return;
			}
			
			int pinID = FOutputPins.IndexOf(Pin);
			MovePin(pinID, -1);
		}
		
		private void MovePin(int OldIdx, int NewIdx)
		{
			//call with NewIdx = -1 to remove the pin
			
			ArrayList lPinSettings = new ArrayList();
			string sliceSettings;
			for (int i=0; i<FPinSettings.SliceCount; i++)
			{
				FPinSettings.GetString(i, out sliceSettings);
				sliceSettings = sliceSettings.TrimEnd();
				lPinSettings.Add(sliceSettings);
			}
			
			string tmpSliceSettings = (string) lPinSettings[OldIdx];
			lPinSettings.RemoveAt(OldIdx);
			
			if (NewIdx >= 0)
				lPinSettings.Insert(NewIdx, tmpSliceSettings);
			
			FPinSettings.SliceCount = lPinSettings.Count;
			
			FBlockConfigurate = true;
			for (int i=0; i<FPinSettings.SliceCount; i++)
				FPinSettings.SetString(i, (string) lPinSettings[i]);
			FBlockConfigurate = false;
			
			for (int i=0; i<FOutputPins.Count; i++)
				FOutputPins[i].Order = i;
			
			Configurate(FPinSettings);
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
		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (m.Msg == 0x0101)
			{
				KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
				
				if (ke.KeyCode == Keys.Space)
				{
					GTimer.IsRunning = !GTimer.IsRunning;
					UpdatePlayButton();
					return true;
				}
				else if (ke.KeyCode == Keys.Back)
				{
					GTimer.IsRunning = false;
					GTimer.CurrentTime = 0;
					return true;
				}
				else if (ke.KeyCode == Keys.Home)
				{
					if (FAutomata != null)
						GTimer.CurrentTime = FAutomata.OutputSlices[0].KeyFrames[0].Time;
					else
						GTimer.CurrentTime = 0;
					return true;
				}
				else if (ke.KeyCode == Keys.End)
				{
					if (FAutomata != null)
						GTimer.CurrentTime = FAutomata.OutputSlices[0].KeyFrames[FAutomata.OutputSlices[0].KeyFrames.Count-2].Time;
					return true;
				}
				else if (ke.KeyCode == Keys.PageUp)
				{
					if (FAutomata != null)
						GTimer.CurrentTime = FAutomata.NextState.Time-0.0001;
					return true;
				}
				else if (ke.KeyCode == Keys.PageDown)
				{
					if (FAutomata != null)
						GTimer.CurrentTime = FAutomata.PreviousState.Time-0.0001;
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
			
			return false;
		}
		
		private void TransformationChangedCB(double Translation, double Scaling)
		{
			FTranslateInput.SetValue(0, Translation);
			FScaleInput.SetValue(0, Scaling);
			FTransformationChanged = true;
		}

		void StopButtonClick(object sender, EventArgs e)
		{
			GTimer.IsRunning = false;
			GTimer.CurrentTime = 0.0f;
			UpdatePlayButton();
		}
		
		void PlayButtonClick(object sender, EventArgs e)
		{
			GTimer.IsRunning = !GTimer.IsRunning;
			UpdatePlayButton();
		}
		
		void UpdatePlayButton()
		{
			if (GTimer.IsRunning)
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
			//	else if (sender == this.MidiButton)
			//		AddPin(TLPinType.Midi);
		}
		
		void AutomataCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (AutomataCheckBox.Checked)
			{
				FAutomataCreatedViaGUI = true;
				
				if (FAutomata == null)
					AddPin(TLPinType.Automata);
			}
			else
			{
				GTimer.Automata = null;
				RemovePinCB(FAutomata);
				FAutomata = null;
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
					PinHeaderPanel1.Controls.Remove(droppedPin);
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
					PinHeaderPanel0.Controls.Remove(droppedPin);
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
		
		void SplitContainerSplitterMoved(object sender, SplitterEventArgs e)
		{
			//save splitter position
			if (FSettings != null)
			{
				XmlNode splitter = FSettings.CreateElement("SPLITTER");
				XmlAttribute attr = FSettings.CreateAttribute("Position");
				attr.Value = SplitContainer.SplitterDistance.ToString();
				splitter.Attributes.Append(attr);
				
				FGUISettings.SliceCount = 1;
				FGUISettings.SetString(0, splitter.OuterXml);
			}
			
			SliceArea.SplitterPosition = SplitContainer.SplitterDistance;
			SliceArea.Refresh();
		}
	}
}

