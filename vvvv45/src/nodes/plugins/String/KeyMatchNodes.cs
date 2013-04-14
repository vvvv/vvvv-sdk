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
using System.Diagnostics;
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
		enum KeyMode {Press, Toggle, UpOnly, DownOnly, DownUp, RepeatedEvent};
		
		#region fields & pins
#pragma warning disable 0649
        [Config("Key Match", IsSingle = true)]
        IDiffSpread<string> FKeyMatch;

        [Input("Keyboard", IsSingle = true)]
        IDiffSpread<KeyboardState> FInput;

        [Input("Reset Toggle", IsSingle = true, IsBang = true)]
        ISpread<bool> FReset;

        [Input("Key Mode", IsSingle = true)]
        ISpread<KeyMode> FKeyMode;

        [Import()]
        IIOFactory FIOFactory; 
#pragma warning restore

        #endregion fields & pins

        class OutputInfo
        {
            public IIOContainer<ISpread<bool>> Container;
            public ISpread<bool> Output { get { return Container.IOObject; } }
            public string Key;
            public string KeyToLower;
            public bool PressedLastFrame;
            public int LastTime;
        }

        List<OutputInfo> FOutputInfos = new List<OutputInfo>();

		public void OnImportsSatisfied()
		{
			FKeyMatch.Changed += KeyMatchChangedCB;
		}

        string FormatNicely(string s)
        {
            Debug.Assert(s.Length > 0);

            if (s.Length == 1)
                return s[0].ToString().ToUpper();
            else
                return s[0].ToString().ToUpper() + s.Substring(1);
        }

		void KeyMatchChangedCB(IDiffSpread<string> sender)
		{
            var keys = FKeyMatch[0].Split(',').ToList().Select(s => s.Trim()).Where(s => s.Length > 0).Select(s => FormatNicely(s)).ToArray();
		
			//add new pins
			foreach (var key in keys)
			{
				var lowerKey = key.ToLower();
                if (!string.IsNullOrWhiteSpace(key) && !FOutputInfos.Any(info => info.Key == key))
				{
					var outAttr = new OutputAttribute(key);
					outAttr.IsSingle = true;
                    var outputInfo = new OutputInfo()
                    {
                        Key = key,
                        KeyToLower = lowerKey,
                        Container = FIOFactory.CreateIOContainer<ISpread<bool>>(outAttr),
                    };
                    FOutputInfos.Add(outputInfo);
				}
			}
			
			//remove obsolete pins
			foreach (var outputInfo in FOutputInfos.ToArray())
				if (!keys.Contains(outputInfo.Key))
                {
                    FOutputInfos.Remove(outputInfo);
                    outputInfo.Container.Dispose();
			    }
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//reset outputs
			if (FReset[0])
				foreach (var outputInfo in FOutputInfos)
					outputInfo.Output[0] = false;

            var keyboardState = FInput[0];
            
            //set active outputs
			if (keyboardState != null)
			{
				var currentKeys = keyboardState.KeyCodes.Select(k => k.ToString().ToLower());
                var keyMode = FKeyMode[0];

                foreach (var outputInfo in FOutputInfos)
				{
                    var key = outputInfo.KeyToLower;
                    var output = outputInfo.Output;
                    var lastFrame = outputInfo.PressedLastFrame;
                    var thisFrame = currentKeys.Contains(key) || (key.Length == 1 ? keyboardState.KeyChars.Contains(key[0]) : false);
                    
                    switch (keyMode)
					{
						case KeyMode.Press:
						{
                            output[0] = thisFrame;
							break;
						}
						case KeyMode.DownOnly:
						{
                            output[0] = !lastFrame && thisFrame;
							break;
						}	
						case KeyMode.UpOnly:
						{
                            output[0] = lastFrame && !thisFrame;
                            break;
						}	
						case KeyMode.DownUp:
						{
                            output[0] = lastFrame != thisFrame;
                            break;
						}
						case KeyMode.Toggle:
						{
                            if (!lastFrame && thisFrame)
                                output[0] = !output[0];
							break;
						}
                        case KeyMode.RepeatedEvent:
                        {
                            output[0] = thisFrame && (!lastFrame || keyboardState.Time != outputInfo.LastTime);
                            break;
                        }
                    }
                    
                    // save pressed key state per output for next frame
                    outputInfo.PressedLastFrame = thisFrame;
                    outputInfo.LastTime = keyboardState.Time;
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
#pragma warning disable 0649
        [Input("Keyboard")]
        IDiffSpread<KeyboardState> FInput;

        [Input("Key Match")]
        IDiffSpread<string> FKeyMatch;

        [Output("Output")]
        ISpread<int> FOutput;
#pragma warning restore
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
