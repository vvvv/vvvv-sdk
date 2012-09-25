using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Threading;


using HighPerfTimer = MLib.Timer;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.OSC;

namespace Hoster
{
	public partial class PluginPage
	{
		const int FPinWidth = 70;
		const int FSliceHeight = 15;

		private TPluginHost FPluginHost;
		private List<NumericUpDown> FSliceCountControls;
		private bool FPinsChanged;
		private OSCTransmitter FOSCTransmitter;
		private OSCReceiver FOSCReceiver;
		private bool FListening;
		private Thread FThread;
		private string FOSCAddress;
		private bool FLoading = true;
		private int FSplitterDistance = 500;

		public string NodeInfoName
		{
			get{return FPluginHost.NodeInfoName;}
		}
		
		private bool FDebug;
		public bool Debug
		{
			set
			{
				InputsPanel.Visible = value;
				OutputsPanel.Visible = value;
				SliceCountsPanel.Visible = value;				
				FDebug = value;
			}	
			get{return FDebug;}
		}
		
		private bool FOSC;
		public bool OSC
		{
			set
			{
				OSCPanel.Visible = value;
				FOSC = value;
			}	
			get{return FOSC;}
		}
		
		public PluginPage()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			//the actual pluginhost
			FPluginHost = new TPluginHost();
			FPluginHost.OnPinCountChanged += new PinCountChangedHandler(PinCountChangedCB);
			FPluginHost.OnBeforeEvaluate += new EvaluateHandler(BeforeEvaluateCB);
			FPluginHost.OnAfterEvaluate += new EvaluateHandler(AfterEvaluateCB);
			FPluginHost.OnLog += new LogHandler(LogCB);
			FPluginHost.FPS = (int) FrameRateIO.Value;
			
			InputsPanel.Visible = false;
			InputsPanel.PinList = FPluginHost.Inputs;
			
			FSliceCountControls = new List<NumericUpDown>();
			
			OutputsPanel.Visible = false;
			OutputsPanel.PinList = FPluginHost.Outputs;
			
			//initialize UDP/OSC
			FOSCAddress = "/" + OSCMessageIO.Text;
			try
			{
				FOSCTransmitter = new OSCTransmitter(TargetHostIO.Text, (int) TargetPortIO.Value);
				FOSCTransmitter.Connect();
			}
			catch (Exception e)
			{
				DebugLog.Items.Add("UDP: failed to open port " + (int) TargetPortIO.Value );
				DebugLog.Items.Add("UDP: " + e.Message);
			}
			
			try
			{
				FOSCReceiver = new OSCReceiver((int) ReceivePortIO.Value);
				StartListeningOSC();
			}
			catch (Exception e)
			{
				DebugLog.Items.Add("UDP: failed to open port " + (int) ReceivePortIO.Value);
				DebugLog.Items.Add("UDP: " + e.Message);
			}
		}
		
		private void DisposePanel()
		{
			StopListeningOSC();
			FPluginHost.ReleasePlugin();
			FPluginHost = null;
		}
		
		#region OSC
		private void StartListeningOSC()
		{
			FListening = true;
			FThread = new Thread(new ThreadStart(ListenToOSC));
			FThread.Start();
		}
		
		private void StopListeningOSC()
		{
			FListening = false;
			if (FOSCReceiver != null)
				FOSCReceiver.Close();

			FOSCReceiver = null;
		}

		private void ListenToOSC()
		{
			while(FListening)
			{
				try
				{
					OSCPacket packet = FOSCReceiver.Receive();
					if (packet!=null)
					{
						if (packet.IsBundle())
						{
							ArrayList messages = packet.Values;
							for (int i=0; i<messages.Count; i++)
								ProcessOSCMessage((OSCMessage)messages[i]);
						}
						else
							ProcessOSCMessage((OSCMessage)packet);
					}
					else
						DebugLog.Items.Add("UDP: null packet received!");
				}
				catch (Exception e)
				{
					DebugLog.Items.Add("UDP: " + e.Message);
				}
			}
		}
		
		private void ProcessOSCMessage(OSCMessage message)
		{
			string address = message.Address;
			ArrayList args = message.Values;
			
			char[] s = {'/'};
			string[] path = address.Split(s);
			
			if (path[1] == OSCMessageIO.Text)
				foreach (IPluginIn p in FPluginHost.Inputs)
			{
				if (p.Name == path[2])
				{
					string spread = "";
					for (int i=0; i<args.Count; i++)
					{
						spread += args[i]+",";
					}
					char[] t = {','};
					spread = spread.TrimEnd(t);
					
					if (args[0] is float)
						spread = spread.Replace(',', '.');
					
					if (args.Count > 0)
						(p as TBasePin).SpreadAsString = spread;
				}
			}
		}
		#endregion OSC
		
		public void LoadPlugin(string Path, string ClassName)
		{
			FPluginHost.LoadPlugin(Path, ClassName, true);
			
			//get the plugins window handle
			if (FPluginHost.Plugin is IWin32Window)
			{
				IntPtr handle = (FPluginHost.Plugin as IWin32Window).Handle;
				
				//dock the plugins windowhandle to the hosts window
				PluginPanel.Controls.Add(System.Windows.Forms.Control.FromHandle(handle));
				
				//make the plugin fill the hosts plugin-area
				if ((int)handle > 0)
					System.Windows.Forms.Control.FromHandle(handle).Dock = DockStyle.Fill;
			}
		}
		
		void LogCB(string LogMessage)
		{
			if (DebugLog.InvokeRequired)
				DebugLog.Invoke(new LogHandler(LogCB), new Object[] {LogMessage});
			else
			{
				string [] loglines = LogMessage.Split(new char[1]{'\n'});
				DebugLog.Items.AddRange(loglines);
				DebugLog.TopIndex = DebugLog.Items.Count-1;
			}
		}
		
		void BeforeEvaluateCB()
		{
			if (FLoading)
			{
				SplitPanelContainer.SplitterDistance = FSplitterDistance;
				FLoading = false;
			}
			//validate all intputs
			//setting spreadcount on inputs
			int idx = 0;
			bool adjustpinpanels = false;
			foreach(TBasePin pin in FPluginHost.Inputs)
			{
				if (pin.Visibility != TPinVisibility.False)
				{
					if (pin.SliceCountIsChanged)
					{
						//update updown control
						FSliceCountControls[idx].Value = pin.SliceCount;
						adjustpinpanels = true;
					}
					else
					{
						int ctrlSC = (int) Math.Max(0, FSliceCountControls[idx].Value);
						if (pin.SliceCount != ctrlSC)
						{
							pin.SliceCount = ctrlSC;
							adjustpinpanels = true;
						}
					}
					idx++;
				}
			}
			
			if (adjustpinpanels)
			{
				AdjustInputsPanel();
				AdjustOutputsPanel();
			}
			
			InputsPanel.SetInputs();
		}
		
		void AfterEvaluateCB()
		{
			if (EnableOSCCheckBox.Checked)
			{
				//double time;
				//GetCurrentTime(out time);
				OSCBundle bundle = new OSCBundle();
				
				//send outputs as OSC
				for (int i=0; i<FPluginHost.Outputs.Count; i++)
				{
					string pinname = FPluginHost.Outputs[i].Name;
					// pinname = pinname.Replace(" ", "_");
					OSCMessage message = new OSCMessage(FOSCAddress + "/" + pinname);
					
					if (FPluginHost.Outputs[i] is TValuePin)
					{
						double val;
						for (int j=0; j<(FPluginHost.Outputs[i] as TValuePin).SliceCount; j++)
						{
							(FPluginHost.Outputs[i] as TValuePin).GetValue(j, out val);
							message.Append((float) val);
						}
					}
					else if (FPluginHost.Outputs[i] is TStringPin)
					{
						string str;
						for (int j=0; j<(FPluginHost.Outputs[i] as TStringPin).SliceCount; j++)
						{
							(FPluginHost.Outputs[i] as TStringPin).GetString(j, out str);
							message.Append(str);
						}
					}
					else if (FPluginHost.Outputs[i] is TColorPin)
					{
						RGBAColor col;
						for (int j=0; j<(FPluginHost.Outputs[i] as TColorPin).SliceCount; j++)
						{
							(FPluginHost.Outputs[i] as TColorPin).GetColor(j, out col);
							message.Append(col.ToString());
						}
					}
					
					bundle.Append(message);
				}
				
				try
				{
					if (FOSCTransmitter != null)
						FOSCTransmitter.Send(bundle);
				}
				catch (Exception ex)
				{
					DebugLog.Items.Add("UDP: " + ex.Message);
				}
			}

			bool redraw = false;
			for (int i=0; i<FPluginHost.Inputs.Count; i++)
			{
				redraw |= (FPluginHost.Inputs[i] as TBasePin).PinIsChanged;
				FPinsChanged |= (FPluginHost.Inputs[i] as TBasePin).SliceCountIsChanged;
				(FPluginHost.Inputs[i] as TBasePin).Invalidate();
			}
			if (redraw)
				InputsPanel.Invalidate();
			
			redraw = false;
			for (int i=0; i<FPluginHost.Outputs.Count; i++)
			{
				redraw |= (FPluginHost.Outputs[i] as TBasePin).PinIsChanged;
				FPinsChanged |= (FPluginHost.Outputs[i] as TBasePin).SliceCountIsChanged;
				(FPluginHost.Outputs[i] as TBasePin).Invalidate();
			}
			if (redraw)
				OutputsPanel.Invalidate();
		}
		
		private void PinCountChangedCB()
		{
			//slicecounts
			FSliceCountControls.Clear();
			SliceCountsPanel.Controls.Clear();
			NumericUpDown ud;

			int idx = 0;
			foreach (TBasePin pin in FPluginHost.Inputs)
			{
				if (pin.Visibility != TPinVisibility.False)
				{
					ud = new NumericUpDown();
					ud.Width = FPinWidth;
					ud.Left = ud.Width * idx;
					ud.Minimum = 0;
					ud.Maximum = int.MaxValue;
					ud.Value = Math.Max(0, pin.SliceCount);
					ud.BorderStyle = BorderStyle.None;
					SliceCountsPanel.Controls.Add(ud);
					FSliceCountControls.Add(ud);
					idx++;
				}
			}
			
			InputsPanel.UpdateVisiblePinList();
			OutputsPanel.UpdateVisiblePinList();
			
			//adjust inputpins areas height
			AdjustInputsPanel();
			
			//adjust outputpins areas height
			AdjustOutputsPanel();

			//adjust plugin areas height
			AdjustPluginPanel();
		}
		
		public void LoadFromXML(XmlNode plugin)
		{
			XmlNode conf;
			
			//load config
			conf = plugin.SelectSingleNode(@"CONFIG/TARGETHOST");
			if (conf != null)
				TargetHostIO.Text = conf.InnerText;
			
			conf = plugin.SelectSingleNode(@"CONFIG/TARGETPORT");
			if (conf != null)
				TargetPortIO.Text = conf.InnerText;
			
			conf = plugin.SelectSingleNode(@"CONFIG/RECEIVEPORT");
			if (conf != null)
				ReceivePortIO.Text = conf.InnerText;
			
			conf = plugin.SelectSingleNode(@"CONFIG/OSCADDRESS");
			if (conf != null)
				OSCMessageIO.Text = conf.InnerText;
			
			conf = plugin.SelectSingleNode(@"CONFIG/FRAMERATE");
			if (conf != null)
				FrameRateIO.Text = conf.InnerText;
			
			conf = plugin.SelectSingleNode(@"CONFIG/SHOWOSC");
			if (conf != null)
				OSC = bool.Parse(conf.InnerText);
			
			conf = plugin.SelectSingleNode(@"CONFIG/ENABLEOSC");
			if (conf != null)
				EnableOSCCheckBox.Checked = bool.Parse(conf.InnerText);
			
			conf = plugin.SelectSingleNode(@"CONFIG/DEBUG");
			if (conf != null)
				Debug = bool.Parse(conf.InnerText);
			
			conf = plugin.SelectSingleNode(@"CONFIG/SPLITTER");
			if (conf != null)
				FSplitterDistance = int.Parse(conf.InnerText);
			
			//load pins
			XmlNode node = plugin.SelectSingleNode(@"NODE");
			XmlNode xmlpin;
			if (node != null)
			{
				XmlAttribute attr;
				TBasePin pin;
				int idx = 0;
				
				for (int i=0; i<FPluginHost.Inputs.Count; i++) //cannot use foreach here because FInputs.count changes during loop, when pins get added
				{
					pin = FPluginHost.Inputs[i] as TBasePin;
					xmlpin = node.SelectSingleNode("PIN[@pinname='" + pin.Name + "']");
					if (xmlpin != null)
					{
						//hack: also set value on slicecountcontrols. improve: slicecountcontrols should be tied to pins
						if (pin.Visibility != TPinVisibility.False)
						{
							FSliceCountControls[idx].Value = pin.SliceCount;
							idx++;
						}
						
						attr = xmlpin.Attributes.GetNamedItem("values") as XmlAttribute;
						pin.SpreadAsString = attr.Value;
					}
				}
			}
		}
		
		public void SaveToXML(XmlDocument Document)
		{
			XmlNode plugin, conf, node, targethost, targetport, receiveport, address, fps, enableosc, showosc, debug, splitter;
			XmlNode host = Document.SelectSingleNode(@"//HOST");
			
			plugin = Document.CreateElement("PLUGIN");
			host.AppendChild(plugin);
			
			//configuration info
			conf = Document.CreateElement("CONFIG");
			plugin.AppendChild(conf);
			
			targethost = Document.CreateElement("TARGETHOST");
			targethost.InnerText = TargetHostIO.Text;
			conf.AppendChild(targethost);
			
			targetport = Document.CreateElement("TARGETPORT");
			targetport.InnerText = TargetPortIO.Text;
			conf.AppendChild(targetport);
			
			receiveport = Document.CreateElement("RECEIVEPORT");
			receiveport.InnerText = ReceivePortIO.Text;
			conf.AppendChild(receiveport);
			
			address = Document.CreateElement("OSCADDRESS");
			address.InnerText = OSCMessageIO.Text;
			conf.AppendChild(address);
			
			fps = Document.CreateElement("FRAMERATE");
			fps.InnerText = FrameRateIO.Text;
			conf.AppendChild(fps);
			
			enableosc = Document.CreateElement("ENABLEOSC");
			enableosc.InnerText = EnableOSCCheckBox.Checked.ToString();
			conf.AppendChild(enableosc);
			
			showosc = Document.CreateElement("SHOWOSC");
			showosc.InnerText = FOSC.ToString();
			conf.AppendChild(showosc);
			
			debug = Document.CreateElement("DEBUG");
			debug.InnerText = FDebug.ToString();
			conf.AppendChild(debug);
			
			splitter = Document.CreateElement("SPLITTER");
			splitter.InnerText = SplitPanelContainer.SplitterDistance.ToString();
			conf.AppendChild(splitter);
			
			//node info
			XmlNode pin;
			XmlAttribute attr;
			
			node = Document.CreateElement("NODE");
			plugin.AppendChild(node);
			attr = Document.CreateAttribute("nodename");
			attr.Value =  FPluginHost.NodeName;
			node.Attributes.Append(attr);
			
			for (int i=0; i<FPluginHost.Inputs.Count; i++)
			{
				pin = Document.CreateElement("PIN");
				attr = Document.CreateAttribute("pinname");
				attr.Value = FPluginHost.Inputs[i].Name;
				pin.Attributes.Append(attr);
				attr = Document.CreateAttribute("slicecount");
				attr.Value = (FPluginHost.Inputs[i] as TBasePin).SliceCount.ToString();
				pin.Attributes.Append(attr);
				attr = Document.CreateAttribute("values");
				attr.Value = (FPluginHost.Inputs[i] as TBasePin).SpreadAsString;
				pin.Attributes.Append(attr);
				node.AppendChild(pin);
			}
		}
		
		#region GUI
		void ClearButtonClick(object sender, EventArgs e)
		{
			DebugLog.Items.Clear();
		}
		
		void AdjustPluginPanel()
		{
			/*if (DebugCheckBox.Checked)
				PluginPanel.Height = this.ClientRectangle.Height - SliceCountsPanel.Height - InputsPanel.Height - OutputsPanel.Height - ConfigPanel.Height;
			else
				PluginPanel.Height = this.ClientRectangle.Height - ConfigPanel.Height;*/
		}
		
		void AdjustInputsPanel()
		{
			int maxcount = 0;
			foreach (TBasePin pin in FPluginHost.Inputs)
				if (pin.Visibility != TPinVisibility.False)
					maxcount = Math.Max(maxcount, pin.SliceCount * pin.Dimension);
			
			InputsPanel.Height = (maxcount + 1) * FSliceHeight + 2;
		}
		
		void AdjustOutputsPanel()
		{
			int maxcount = 0;
			foreach (TBasePin pin in FPluginHost.Outputs)
				if (pin.Visibility != TPinVisibility.False)
					maxcount = Math.Max(maxcount, pin.SliceCount * pin.Dimension);
			
			OutputsPanel.Height = Math.Min(200, (maxcount + 1) * FSliceHeight + 2);
			OutputsPanel.Invalidate();
		}

		void MainFormSizeChanged(object sender, System.EventArgs e)
		{
			AdjustPluginPanel();
		}
		
		void FrameRateIOValueChanged(object sender, System.EventArgs e)
		{
			FPluginHost.FPS = (int) (sender as NumericUpDown).Value;
		}
		
		void OSCMessageIOTextChanged(object sender, System.EventArgs e)
		{
			FOSCAddress = (sender as TextBox).Text;
		}
		
		void TargetPortIOValueChanged(object sender, System.EventArgs e)
		{
			try
			{
				if (FOSCTransmitter != null)
					FOSCTransmitter.Close();
				FOSCTransmitter = null;
				FOSCTransmitter = new OSCTransmitter(TargetHostIO.Text, (int) (sender as NumericUpDown).Value);
			}
			catch (Exception se)
			{
				DebugLog.Items.Add("UDP: " + se.Message);
			}
		}
		
		void ReceivePortIOValueChanged(object sender, System.EventArgs e)
		{
			try
			{
				StopListeningOSC();
				FOSCReceiver = new OSCReceiver((int) ReceivePortIO.Value);
				StartListeningOSC();
			}
			catch (Exception ex)
			{
				DebugLog.Items.Add("UDP: failed to open port: " + (int) ReceivePortIO.Value);
				DebugLog.Items.Add("UDP: " + ex.Message);
			}
		}
		
		void TargetHostIOTextChanged(object sender, System.EventArgs e)
		{
			try
			{
				if (FOSCTransmitter != null)
					FOSCTransmitter.Close();
				FOSCTransmitter = null;
				FOSCTransmitter = new OSCTransmitter((sender as TextBox).Text, (int) TargetPortIO.Value);
			}
			catch (Exception se)
			{
				DebugLog.Items.Add("UDP: " + se.Message);
			}
		}
		#endregion GUI
	}
}
