using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;

using HighPerfTimer = MLib.Timer;

using VVVV.PluginInterfaces.V1;

namespace Hoster
{
	public delegate void PinCountChangedHandler();
	public delegate void EvaluateHandler();
	public delegate void LogHandler(string LogMessage);
	
	public class TPluginHost: IPluginHost
	{
		private Timer FTimer = new Timer();
		private bool FPinCountChanged;
		private double FStartTime;
		
		private List<TBasePin> FInputs;
		public List<TBasePin> Inputs
		{
			get {return FInputs;}
		}
		
		private List<TBasePin> FOutputs;
		public List<TBasePin> Outputs
		{
			get {return FOutputs;}
		}
		
		private IPlugin FHostedPlugin;
		public IPlugin Plugin
		{
			get {return FHostedPlugin;}
		}
		
		public int FPS
		{
			set {FTimer.Interval = (int) (1000 / value);}
		}

		private string FNodeName;
		public string NodeName
		{
			get {return FNodeName;}
		}
		
		private string FNodeInfoName;
		public string NodeInfoName
		{
			get {return FNodeInfoName;}
		}
		
		public event PinCountChangedHandler OnPinCountChanged;
		public event EvaluateHandler OnBeforeEvaluate;
		public event EvaluateHandler OnAfterEvaluate;
		public event LogHandler OnLog;
		
		public TPluginHost()
		{
			//the hosts lists of pins
			FInputs = new List<TBasePin>();
			FOutputs = new List<TBasePin>();
			
			FTimer.Interval = (int) (1000 / 30);
			FTimer.Tick += new System.EventHandler(TimerTick);
		}
		
		~TPluginHost()
		{
			FTimer = null;
		}
		
		public string LoadPlugin(string Path, string ClassName, bool DoRun)
		{
			string result = "OK";
			
			try
			{
				System.Reflection.Assembly plugindll = System.Reflection.Assembly.LoadFrom(Path);
				Type plugin;
				
				//retrieve the nodoinfo
				foreach (System.Type objType in plugindll.GetTypes())
				{
					//Only look at public, non abstract types
					if (objType.IsPublic && !objType.IsAbstract)
					{
						//See if this type implements our interface
						plugin = objType.GetInterface("VVVV.PluginInterfaces.V1.IPlugin");
						if (plugin != null)
						{
							PropertyInfo pi = objType.GetProperty("PluginInfo");
							object a = pi.GetValue(null, null);
							IPluginInfo info = (IPluginInfo) a;
			
							FNodeInfoName = info.Name;
							break;
						}
					}
				}
				FNodeName = System.IO.Path.GetFileName(Path) + "|" + ClassName;
				
				FHostedPlugin = plugindll.CreateInstance(ClassName) as IPlugin;
				
				//hand the host over to the plugin
				FHostedPlugin.SetPluginHost(this);
				
				if (DoRun)
					OnPinCountChanged();
			}
			catch (Exception e)
			{
				result = "ERROR: " + e.Message;
			}
			
			//save starttime
			HighPerfTimer.Update();
			FStartTime = HighPerfTimer.Ticks / 1000D;
			
			if ((FHostedPlugin != null) && (DoRun))
				FTimer.Enabled = true;
			
			return result;
		}
		
		public void ReleasePlugin()
		{
			FTimer.Enabled = false;
			
			if (FHostedPlugin is IDisposable)
				(FHostedPlugin as IDisposable).Dispose();
			FHostedPlugin = null;
			
			FInputs = null;
			FOutputs = null;
		}
		
		#region IPluginHost
		public void Log(TLogType LogType, string Message)
		{
			if (OnLog != null)
			{
				HighPerfTimer.Update();
				long runningtime = (long) (HighPerfTimer.Ticks / 1000D - FStartTime);
				long sec = runningtime % 60;
				long min = runningtime % 3600 / 60;
				long hour = runningtime % 216000 / 3600;
				string time = hour.ToString("d2") + ":" + min.ToString("d2") + ":" + sec.ToString("d2");
				
				switch (LogType)
				{
						case TLogType.Debug: OnLog(time + "    " + Message); break;
						case TLogType.Message: OnLog(time + " -  " + Message); break;
						case TLogType.Warning: OnLog(time + " *  " + Message); break;
						case TLogType.Error: OnLog(time + " ERR  " + Message); break;
				}
			}
		}
		
		public void GetHostPath(out string Path)
		{
			Path = Application.ExecutablePath;
		}
		
		public void GetNodePath(bool UseDescriptiveNames, out string Path)
		{
			Path = "/";
		}
		
		public void UpdateEnum(string EnumName, string Default, string[] EnumEntries)
		{
			
		}
		
		public void GetEnumEntryCount(string EnumName, out int EntryCount)
		{
			EntryCount = 0;	
		}
		
		public void GetEnumEntry(string EnumName, int Index, out string EntryName)
		{
			EntryName = "";
		}
		
		public void Evaluate()
		{
		    //TODO: not implemented 
		}
		
		private void AddPin(TBasePin Pin)
		{
			if ((Pin.Direction == TPinDirection.Configuration) || (Pin.Direction == TPinDirection.Input))
				FInputs.Add(Pin);
			else
				FOutputs.Add(Pin);
			
			FPinCountChanged = true;
		}
		
		//called by the plugin the host can create a pin
		public void CreateValueInput(string Name, int Dimension, string[] DimensionNames,  TSliceMode SliceMode, TPinVisibility Visibility, out IValueIn Pin)
		{
			Pin = new TValuePin(this, Name, Dimension, DimensionNames, TPinDirection.Input, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		//called by the plugin the host can create a pin
		public void CreateValueFastInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueFastIn Pin)
		{
			Pin = new TValuePin(this, Name, Dimension, DimensionNames, TPinDirection.Input, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		//called by the plugin the host can create a pin
		public void CreateValueConfig(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueConfig Pin)
		{
			Pin = new TValuePin(this, Name, Dimension, DimensionNames, TPinDirection.Configuration, new TOnConfigurate(ConfigurateCB), SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		//called by the plugin the host can create a pin
		public void CreateValueOutput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueOut Pin)
		{
			Pin = new TValuePin(this, Name, Dimension, DimensionNames, TPinDirection.Output, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateStringInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringIn Pin)
		{
			Pin = new TStringPin(this, Name, TPinDirection.Input, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateStringConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringConfig Pin)
		{
			Pin = new TStringPin(this, Name, TPinDirection.Input, new TOnConfigurate(ConfigurateCB), SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateStringOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringOut Pin)
		{
			Pin = new TStringPin(this, Name, TPinDirection.Output, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}

		public void CreateTransformInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformIn Pin)
		{
			Pin = new TTransformInPin(this, Name, SliceMode, Visibility);
		}
		
		public void CreateTransformOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformOut Pin)
		{
			Pin = new TTransformOutPin(this, Name, SliceMode, Visibility);
		}
		
		public void CreateColorInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorIn Pin)
		{
			Pin = new TColorPin(this, Name, TPinDirection.Input, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateColorConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorConfig Pin)
		{
			Pin = new TColorPin(this, Name, TPinDirection.Configuration, new TOnConfigurate(ConfigurateCB), SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateColorOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorOut Pin)
		{
			Pin = new TColorPin(this, Name, TPinDirection.Output, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateEnumInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumIn Pin)
		{
			Pin = new TEnumPin(this, Name, TPinDirection.Input, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateEnumConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumConfig Pin)
		{
			Pin = new TEnumPin(this, Name, TPinDirection.Configuration, new TOnConfigurate(ConfigurateCB), SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateEnumOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumOut Pin)
		{
			Pin = new TEnumPin(this, Name, TPinDirection.Output, null, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateNodeInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out INodeIn Pin)
		{
			Pin = new TNodePin(this, Name, TPinDirection.Input, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateNodeOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out INodeOut Pin)
		{
			Pin = new TNodePin(this, Name, TPinDirection.Output, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateMeshOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IDXMeshOut Pin)
		{
			Pin = new TMeshPin(this, Name, TPinDirection.Output, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateLayerOutput(string Name, TPinVisibility Visibility, out IDXLayerIO Pin)
		{
			Pin = new TLayerPin(this, Name, TPinDirection.Output, TSliceMode.Single, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateTextureOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IDXTextureOut Pin)
		{
			Pin = new TTexturePin(this, Name, TPinDirection.Output, TSliceMode.Single, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateRenderStateInput(TSliceMode SliceMode, TPinVisibility Visibility, out IDXRenderStateIn Pin)
		{
			Pin = new TStatePin(this, TPinDirection.Input, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateSamplerStateInput(TSliceMode SliceMode, TPinVisibility Visibility, out IDXSamplerStateIn Pin)
		{
			Pin = new TStatePin(this, TPinDirection.Input, SliceMode, Visibility);
			
			AddPin(Pin as TBasePin);
		}
		
		public void CreateRawInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IRawIn Pin)
		{
		    //TODO: not implemented
		    Pin = null;
		}

		public void CreateRawOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IRawOut Pin)
		{
		    //TODO: not implemented
		    Pin = null;
		}
		
		//called by the plugin the host can delete a pin
		public void DeletePin(IPluginIO Pin)
		{
			if (FInputs.Contains(Pin as TBasePin))
				FInputs.Remove(Pin as TBasePin);
			else if (FOutputs.Contains(Pin as TBasePin))
				FOutputs.Remove(Pin as TBasePin);
			
			FPinCountChanged = true;
		}
		
		public void GetCurrentTime(out double CurrentTime)
		{
			HighPerfTimer.Update();
			CurrentTime = HighPerfTimer.Ticks / 1000D;  //DateTime.UtcNow.ToOADate();
		}
		
		public void ConfigurateCB(IPluginConfig Input)
		{
			FHostedPlugin.Configurate(Input);
			
			//pins may have been added/deleted during configuration
			if (FPinCountChanged) //pins added/deleted or any slicecount is changed
			{
				OnPinCountChanged();
				FPinCountChanged = false;
			}
		}
		#endregion IPluginHost
		
		//the hosts mainloop
		public void TimerTick(object sender, System.EventArgs e)
		{
			if (FPinCountChanged) //pins added/deleted or any slicecount is changed
			{
				OnPinCountChanged();
				FPinCountChanged = false;
			}
			
			OnBeforeEvaluate();
			
			//compute nodes SpreadCount
			int nodeSpreadCount = 0;
			foreach(TBasePin pin in Inputs)
			{
				if (pin.SliceCount == 0) //if any pin has spreadcount of 0 then spreadcount = 0
				{
					nodeSpreadCount = 0;
					break;
				}
				else
					nodeSpreadCount = Math.Max(nodeSpreadCount, pin.SliceCount);
			}
			
			try
			{
				FHostedPlugin.Evaluate(nodeSpreadCount);
			}
			catch (Exception ex)
			{
				Log(TLogType.Error, ex.Message);
			}
			
			OnAfterEvaluate();
		}
	}
}
