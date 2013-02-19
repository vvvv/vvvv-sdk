#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "DirectInput", Category = "Devices", Help = "Access mouse data using DirectInput", Tags = "")]
	#endregion PluginInfo
	public class DevicesDirectInputNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Window Handle", DefaultValue = -1, IsSingle = true)]
		ISpread<int> FHandle;
		[Input("Foreground", DefaultValue = 0, IsSingle = true)]
		ISpread<bool> FFrg;
		[Input("Exclusive", DefaultValue = 0, IsSingle = true)]
		ISpread<bool> FExclusive;
		[Input("Reinitialize", DefaultValue = 0, IsSingle = true)]
		ISpread<bool> FInit;

		[Output("Mouse Position XYW")]
		ISpread<int> FPos;
		[Output("Mouse Buttons")]
		ISpread<bool> FButton;
		[Output("Keyboard Output")]
		ISpread<string> FKeysCon;
		[Output("Keyboard Spread")]
		ISpread<string> FKeys;
		[Output("Key's HashCode")]
		ISpread<int> FKeyCode;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
		
		[DllImport("C:\\Windows\\System32\\user32.dll")]
		public static extern IntPtr GetForegroundWindow();
		
		private Device mouse;
		private Device keyb;
		
		private void InitDevice() {
			mouse = new Device(SystemGuid.Mouse);
			keyb = new Device(SystemGuid.Keyboard);
			if (mouse == null) FLogger.Log(LogType.Debug, "No mouse found.");
			if (keyb == null) FLogger.Log(LogType.Debug, "No keyboard found.");
			IntPtr handle = (FHandle[0]==-1) ? GetForegroundWindow() : new IntPtr(FHandle[0]);
			if(FFrg[0] && (!FExclusive[0])) {
				mouse.SetCooperativeLevel(
					handle,
					CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive
				);
				keyb.SetCooperativeLevel(
					handle,
					CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive
				);
			}
			if(FFrg[0] && FExclusive[0]) {
				mouse.SetCooperativeLevel(
					handle,
					CooperativeLevelFlags.Foreground | CooperativeLevelFlags.Exclusive
				);
				keyb.SetCooperativeLevel(
					handle,
					CooperativeLevelFlags.Foreground | CooperativeLevelFlags.Exclusive
				);
			}
			if(!FFrg[0]) {
				mouse.SetCooperativeLevel(
					handle,
					CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive
				);
				keyb.SetCooperativeLevel(
					handle,
					CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive
				);
			}
			mouse.Acquire();
			keyb.Acquire();
		}
		
		[ImportingConstructor]
		public DevicesDirectInputNode() {
			mouse = new Device(SystemGuid.Mouse);
			mouse.Properties.AxisModeAbsolute = false;
			mouse.Acquire();
			keyb = new Device(SystemGuid.Keyboard);
			keyb.Acquire();
		}
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			IntPtr hwnd = new IntPtr(FHandle[0]);
			if(FInit[0]) InitDevice();
			if(mouse == null) FLogger.Log(LogType.Debug, "No mouse found.");
			if((FFrg[0] && (hwnd==GetForegroundWindow())) || (!FFrg[0]))
			{
				MouseState state = mouse.CurrentMouseState;
				KeyboardState kstate = keyb.GetCurrentKeyboardState();
				FPos.SliceCount = 3;
				FPos[0] = state.X;
				FPos[1] = state.Y;
				FPos[2] = state.Z;
				
				byte[] buttons = state.GetMouseButtons();
				FButton.SliceCount = buttons.Length;
				for(int i=0; i < buttons.Length; i++) {
					FButton[i] = buttons[i]!=0;
				}

		    	//Capture pressed keys.
				string info = "";
				int j = 0;
			    foreach(Key k in keyb.GetPressedKeys()) j++;
				FKeys.SliceCount = j;
				FKeyCode.SliceCount = j; j=0;
			    foreach(Key k in keyb.GetPressedKeys())
    			{
	    			FKeyCode[j] = k.GetHashCode();
    				// formatting for better "compatibility" with Keyboard (System Global)
    				// every key name will be placed in <> brackets except numbers and letters
        			FKeys[j] = "<" + k.ToString().ToUpper() + ">";
	    			if(FKeyCode[j] == 0x02) FKeys[j] = "1";
    				if(FKeyCode[j] == 0x03) FKeys[j] = "2";
    				if(FKeyCode[j] == 0x04) FKeys[j] = "3";
    				if(FKeyCode[j] == 0x05) FKeys[j] = "4";
    				if(FKeyCode[j] == 0x06) FKeys[j] = "5";
	    			if(FKeyCode[j] == 0x07) FKeys[j] = "6";
    				if(FKeyCode[j] == 0x08) FKeys[j] = "7";
    				if(FKeyCode[j] == 0x09) FKeys[j] = "8";
    				if(FKeyCode[j] == 0x0a) FKeys[j] = "9";
    				if(FKeyCode[j] == 0x0b) FKeys[j] = "0";
	    			if(
    					(FKeyCode[j] == 0x10) ||
    					(FKeyCode[j] == 0x11) ||
    					(FKeyCode[j] == 0x12) ||
    					(FKeyCode[j] == 0x13) ||
	    				(FKeyCode[j] == 0x14) ||
    					(FKeyCode[j] == 0x15) ||
    					(FKeyCode[j] == 0x16) ||
    					(FKeyCode[j] == 0x17) ||
    					(FKeyCode[j] == 0x18) ||
    					(FKeyCode[j] == 0x19) ||
	    				(FKeyCode[j] == 0x1e) ||
    					(FKeyCode[j] == 0x1f) ||
    					(FKeyCode[j] == 0x20) ||
    					(FKeyCode[j] == 0x21) ||
    					(FKeyCode[j] == 0x22) ||
    					(FKeyCode[j] == 0x23) ||
	    				(FKeyCode[j] == 0x24) ||
    					(FKeyCode[j] == 0x25) ||
    					(FKeyCode[j] == 0x26) ||
    					(FKeyCode[j] == 0x2c) ||
    					(FKeyCode[j] == 0x2d) ||
    					(FKeyCode[j] == 0x2e) ||
	    				(FKeyCode[j] == 0x2f) ||
    					(FKeyCode[j] == 0x30) ||
    					(FKeyCode[j] == 0x31) ||
    					(FKeyCode[j] == 0x32)
    				) FKeys[j] = k.ToString();
	    			info += FKeys[j];
    				j++;
    			}
				FKeysCon.SliceCount = 1;
				FKeysCon[0] = info;
			}
		}
	}
}