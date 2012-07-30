#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "KeyMatch", 
	            Category = "String",
	            Help = "Detects pressed keys when connected with a Keyboard Node. Use the inspector to specify the keys to check.",
	            AutoEvaluate = true,
				Tags = "")]
	#endregion PluginInfo
	public class KeyMatchNode: IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		enum KeyMode {Press, Toggle, UpOnly, DownOnly, DownUp};
		
		#region fields & pins
		[Config("Key Match", IsSingle = true)]
		IDiffSpread<string> FKeyMatch;
		
		[Input("Input", IsSingle = true)]
		IDiffSpread<KeyboardState> FInput;
		
		[Input("Reset Toggle", IsSingle = true, IsBang = true)]
		ISpread<bool> FReset;
		
		[Input("Key Mode", IsSingle = true)]
		ISpread<KeyMode> FKeyMode;

		[Import()]
		ILogger FLogger;
		
		[Import()]
		IIOFactory FIOFactory;
 		
		Dictionary<string, ISpread<bool>> FOutputs = new Dictionary<string, ISpread<bool>>();
		Dictionary<string, bool> FLastFrame = new Dictionary<string, bool>();
		#endregion fields & pins
		
		public void OnImportsSatisfied()
		{
			FKeyMatch.Changed += KeyMatchChangedCB;
		}
		
		void KeyMatchChangedCB(IDiffSpread<string> sender)
		{
			var keys = FKeyMatch[0].Split(',').ToList().Select(s => s.Trim());
		
			//add new pins
			foreach (var key in keys)
			{
				var lowerKey = key.ToLower();
				if (!string.IsNullOrWhiteSpace(key) && !FOutputs.ContainsKey(lowerKey))
				{
					var outAttr = new OutputAttribute(key);
					outAttr.IsSingle = true;
					var spread = FIOFactory.CreateSpread<bool>(outAttr, true);
					FOutputs.Add(lowerKey, spread);
					FLastFrame.Add(lowerKey, false);
				}
			}
			
			//remove obsolete pins
			var toDelete = new List<string>();
			foreach (var key in FOutputs.Keys)
			{
				if (!keys.Contains(key))
					toDelete.Add(key);	
			}
			
			foreach (var pin in toDelete)
			{
				FOutputs.Remove(pin);
				FLastFrame.Remove(pin);
				FLogger.Log(LogType.Debug, "removed: " + pin);
			}
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//reset outputs
			if ((FKeyMode[0] == KeyMode.Press) || (FKeyMode[0] == KeyMode.DownOnly) || (FKeyMode[0] == KeyMode.UpOnly) || (FKeyMode[0] == KeyMode.DownUp) || FReset[0])
				for (int i = 0; i < FOutputs.Count; i++)
				{
					var key = FOutputs.ElementAt(i).Key;
					FOutputs[key][0] = false;
				}	
			
			//set active outputs
			if (FInput[0] != null && FInput.IsChanged)
			{
				var currentKeys = FInput[0].KeyCodes.Select(k => k.ToString().ToLower());
				
				foreach (var key in FLastFrame.Keys)
				{
					switch (FKeyMode[0])
					{
						case KeyMode.Press:
						{
							FOutputs[key][0] = currentKeys.Contains(key);
							break;
						}
						case KeyMode.DownOnly:
						{
							if (!FLastFrame[key] && currentKeys.Contains(key))
								FOutputs[key][0] = true;
							break;
						}	
						case KeyMode.UpOnly:
						{
							if (FLastFrame[key] && !currentKeys.Contains(key))
								FOutputs[key][0] = true;
							break;
						}	
						case KeyMode.DownUp:
						{
								if ((!FLastFrame[key] && currentKeys.Contains(key)) || (FLastFrame[key] && !currentKeys.Contains(key)))
								FOutputs[key][0] = true;
							break;
						}
						case KeyMode.Toggle:
						{
							if (!FLastFrame[key] && currentKeys.Contains(key))
								FOutputs[key][0] = !FOutputs[key][0];
							break;
						}
					}
				}

				//save last frames state
				for (int i = 0; i < FLastFrame.Count; i++)
				{
					var key = FLastFrame.ElementAt(i).Key;
					FLastFrame[key] = currentKeys.Contains(key);
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "RadioKeyMatch", 
	            Category = "String",
	            Help = "Similiar to KeyMatch, but does not create a output pin for each key to check, but returns the index of the pressed key on its output pin.",
	            AutoEvaluate = true,
				Tags = "")]
	#endregion PluginInfo
	public class RadioKeyMatchNode: IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IDiffSpread<KeyboardState> FInput;

		[Input("Key Match")]
		IDiffSpread<string> FKeyMatch;
		
		[Output("Output")]
		ISpread<int> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			
			if (FInput[0] != null)
				if (FInput.IsChanged || FKeyMatch.IsChanged)
				{
					for (int i = 0; i < SpreadMax; i++)
					{
						if (FInput[i].KeyCodes.Count > 0)
						{
							var matcher = FKeyMatch[i].Split(',').ToList().Select(s => s.ToLower().Trim()).ToList();
							var key = FInput[i].KeyCodes[0].ToString().ToLower();
							var index = matcher.IndexOf(key);
							if (index >= 0)
								FOutput[i] = index;
						}
					}
				}
		}
	}
}
