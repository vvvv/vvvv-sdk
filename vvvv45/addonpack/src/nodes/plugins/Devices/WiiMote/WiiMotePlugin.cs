#region licence/info

//////project name
//WiiMote Port to VVVV

//////description
//WiiMote Port to VVV, using Brian Peeks C# library

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
// velcrome

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using WiimoteLib;
using System.Collections.Generic;


//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class PluginWiiMote: IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		private Wiimote FRemote ;
		private bool FWorking = false;
		private bool FInvalidate = true;
		private int FWiimoteID = 0;
		
		//input pin declaration
		//		CreateValueInput - Pins check for changes every frame, only calculate, when change happened
		//		CreateFastValueInput - Pins skip this check, use it in conjunction with some kind of "apply"-Pin
		

		//input pin declaration
		private IValueIn FPinInputRumble;
		private IValueIn FPinInputLED;
		private IValueIn FPinInputEnable;
		private IValueFastIn FPinInputCalibrationZeroG;
		private IValueFastIn FPinInputCalibrationOneG;
		private IValueIn FPinInputCalibrate;
		private IValueIn FPinInputMode;
		private int FIRMode = 2;
		
		private IValueIn FPinInputID;

		private IValueConfig FPinConfigExtension;
		private int FExtension = 0;
		
		
		//output pin declaration
		private IValueOut FPinOutputAccelleration;
		private IValueOut FPinOutputTilt;
		private IValueOut FPinOutputAvailable;
		
		private IValueOut FPinOutputCursor;
		private IValueOut FPinOutputButtons;
		private IValueOut FPinOutputControls;

		private IStringOut FPinOutputWorking;
		private string FMessage;
		
		private IValueOut FPinOutputBattery;
		private IStringOut FPinOutputExtensionFound;
		private IValueOut FPinOutputInfraredBlobs;

		// 		Extension
		private IValueOut FPinOutputExtAccelleration; // only nunchuk
		private IValueOut FPinOutputExtTilt; // only nunchuck

		private IValueOut FPinOutputExtJoystickLeft; // analog X, analog Y
		private IValueOut FPinOutputExtJoystickRight; // only classic, same as above

		private IValueOut FPinOutputExtCursor; // only classic: up down left right
		private IValueOut FPinOutputExtButtons; // classic: A B X Y nunchuk: C Z
		private IValueOut FPinOutputExtControls; // +, -, home

		private IValueOut FPinOutputExtControls2; // only classic: ZL, ZR, TriggerL, TriggerR

		
		#endregion field declaration
		
		#region constructor/destructor
		
		public PluginWiiMote()
		{
			FRemote = new Wiimote();
			
			//the nodes constructor
			//nothing to declare for this node
		}
		
		~PluginWiiMote()
		{
			FRemote.Dispose();
			//the nodes destructor
			//nothing to destruct
		}

		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				IPluginInfo Info = new PluginInfo();
				Info.Name = "WiiMote";
				Info.Category = "Devices";
				Info.Version = "";
				Info.Help = "Every ";
				Info.Bugs = "No Calibration for the Extensions implemented yet.";
				Info.Credits = "Plugin Port: velcrome@gmx.net \n" +
					"Basic Wiimotelib: http://www.brianpeek.com";
				Info.Warnings = "Not throughly tested yet";
				
				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
				//leave above as is
			}
		}

		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			FHost = Host;

			FHost.CreateValueInput("Wiimote ID", 1, null, TSliceMode.Dynamic,TPinVisibility.OnlyInspector,  out FPinInputID);
			FPinInputID.SetSubType(-1, int.MaxValue, 1, -1, false, false, true);

			FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single,TPinVisibility.True,  out FPinInputEnable);
			FPinInputEnable.SetSubType(0, 1, 1, 0, false, true, false);

			FHost.CreateValueInput("LED", 4, new string[4] {"0", "1", "2", "3"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinInputLED);
			FPinInputLED.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

			FHost.CreateValueInput("Rumble", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinInputRumble);
			FPinInputRumble.SetSubType(0, 1, 1, 0, false, true, false);

			FHost.CreateValueInput("Infrared Mode", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinInputMode);
			FPinInputMode.SetSubType(0, 3, 1, FIRMode, false, false, true);

			FHost.CreateValueFastInput("Calibration ZeroG", 3, new string[3]{"X", "Y", "Z"}, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPinInputCalibrationZeroG);
			FPinInputCalibrationZeroG.SetSubType3D(0, 0xFF, 1, 126, 126, 131, false, false, true);

			FHost.CreateValueFastInput("Calibration OneG", 3, new string[3]{"X", "Y", "Z"}, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPinInputCalibrationOneG);
			FPinInputCalibrationOneG.SetSubType3D(0, 0xFF, 1, 151, 151, 151, false, false, true);

			FHost.CreateValueInput("Calibrate", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPinInputCalibrate);
			FPinInputCalibrate.SetSubType(0, 1, 1, 0, true, false, false);
			
//			Enumeration would be better:
//			0 - no Extension
//			1 - Nunchuk
//			2 - Classic
			FHost.CreateValueConfig("Extension", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinConfigExtension);
			FPinConfigExtension.SetSubType(0, 2, 1, 0, false, false, true);

			//create outputs
			FHost.CreateValueOutput("Available Wiimotes", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPinOutputAvailable);
			FPinOutputAvailable.SetSubType(0, int.MaxValue, 1, 0, false, false, true);

			FHost.CreateStringOutput("Working", TSliceMode.Single, TPinVisibility.True, out FPinOutputWorking);
			FPinOutputWorking.SetSubType("Initializing", false);

			FHost.CreateStringOutput("Extension Found", TSliceMode.Single, TPinVisibility.True, out FPinOutputExtensionFound);
			FPinOutputExtensionFound.SetSubType("none", false);

			FHost.CreateValueOutput("Battery", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputBattery);
			FPinOutputBattery.SetSubType(double.MinValue, double.MaxValue, 0.0001, 0, false, false, false);

			FHost.CreateValueOutput("Cursor", 4, new string[4]{"Up", "Down", "Left", "Right"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputCursor);
			FPinOutputCursor.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

			FHost.CreateValueOutput("Buttons", 4, new string[4]{"A", "B", "1", "2"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputButtons);
			FPinOutputButtons.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

			FHost.CreateValueOutput("System", 3, new string[3]{"Plus", "Minus", "Home"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputControls);
			FPinOutputControls.SetSubType3D(0, 1, 1, 0, 0, 0, false, true, false);
			
			FHost.CreateValueOutput("Acceleration", 3, new string[3]{"X", "Y", "Z"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputAccelleration);
			FPinOutputAccelleration.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
			
			FHost.CreateValueOutput("Angle", 2, new string[2]{"Pitch", "Roll"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputTilt);
			FPinOutputTilt.SetSubType2D(double.MinValue, double.MaxValue, 0.0001, 0, 0, false, false, false);

			FHost.CreateValueOutput("Infrared", 4, new string[4]{"ID", "X", "Y", "Size"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputInfraredBlobs);
			FPinOutputInfraredBlobs.SetSubType4D(0, 1023, 1, 0, 0, 0, 0, false, false, true);

			Enable();
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			double ext;
			FPinConfigExtension.GetValue(0, out ext);

			int newExt = (int)Math.Ceiling(ext);
			if (newExt != FExtension) {
				if (FPinOutputExtAccelleration != null) FHost.DeletePin(FPinOutputExtAccelleration);
				if (FPinOutputExtTilt != null)FHost.DeletePin(FPinOutputExtTilt);
				if (FPinOutputExtButtons != null)FHost.DeletePin(FPinOutputExtButtons);
				if (FPinOutputExtCursor != null)FHost.DeletePin(FPinOutputExtCursor);
				if (FPinOutputExtControls!= null)FHost.DeletePin(FPinOutputExtControls);

				if (FPinOutputExtJoystickLeft != null)FHost.DeletePin(FPinOutputExtJoystickLeft);
				if (FPinOutputExtJoystickRight!= null)FHost.DeletePin(FPinOutputExtJoystickRight);
				if (FPinOutputExtControls2!= null)FHost.DeletePin(FPinOutputExtControls2);
				
			}
			
			FExtension = newExt;
			
			switch (FExtension) {
				case 1: //Nunchuck
					FHost.CreateValueOutput("NunchukButtons", 2, new string[2]{"C", "Z"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtButtons);
					FPinOutputExtButtons.SetSubType2D(0, 1, 1, 0, 0, false, true, false);
					
					FHost.CreateValueOutput("NunchukAccelleration", 3, new string[3]{"X", "Y", "Z"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtAccelleration);
					FPinOutputExtAccelleration.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("NunchukTilt", 2, new string[2]{"Pitch", "Roll"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtTilt);
					FPinOutputExtTilt.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("NunchukJoystick", 2, new string[2]{"Horizontal", "Vertical"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtJoystickLeft);
					FPinOutputExtJoystickLeft.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);

					
					break;
				case 2: // Classic
					FHost.CreateValueOutput("ClassicCursor", 4, new string[4]{"Up", "Down", "Left", "Right"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtCursor);
					FPinOutputExtCursor.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

					FHost.CreateValueOutput("ClassicButtons", 4, new string[4]{"A", "B", "X", "Y"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtButtons);
					FPinOutputExtButtons.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

					FHost.CreateValueOutput("ClassicSystem", 3, new string[3]{"Plus", "Minus", "Home"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtControls);
					FPinOutputExtControls.SetSubType3D(0, 1, 1, 0, 0, 0, false, true, false);
					
					FHost.CreateValueOutput("ClassicJoystickLeft", 2, new string[2]{"Horizontal", "Vertical"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtJoystickLeft);
					FPinOutputExtJoystickLeft.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("ClassicJoystickRight", 2, new string[2]{"Horizontal", "Vertical"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtJoystickRight);
					FPinOutputExtJoystickRight.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);

					FHost.CreateValueOutput("ClassicBackControl", 4, new string[4]{"Left", "Right", "Left", "Right"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtControls2);
					FPinOutputExtControls2.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

					/*
					FHost.CreateValueOutput("ClassicAccelleration", 3, new string[3]{"X", "Y", "Z"}, TSliceMode.Dynamic, TPinVisibility.True, out FExtAccelleration);
					FExtAccelleration.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("ClassicTilt", 2, new string[2]{"Roll", "Yaw"}, TSliceMode.Dynamic, TPinVisibility.True, out FExtTilt);
					FExtTilt.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);

					 */
					
					break;
				default: // None
					
					break;
			}
			
		}
		
		
		
		void OnInvalidatePlugin(object Sender, WiimoteChangedEventArgs Args)
		{
			FInvalidate = true;
		}
		
		void OnChangeReportType(object Sender, WiimoteExtensionChangedEventArgs Args) {
			if (Args.ExtensionType == ExtensionType.ClassicController) {
				FRemote.SetReportType(Wiimote.InputReport.IRExtensionAccel, true);
			}
			if (Args.ExtensionType == ExtensionType.Nunchuk) {
				FRemote.SetReportType(Wiimote.InputReport.IRExtensionAccel, true);
			}
			if (Args.ExtensionType == ExtensionType.None) {
				FRemote.SetReportType(Wiimote.InputReport.IRAccel, true);
			}
			
			FRemote.GetStatus();
			FInvalidate = true;
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		
		// WiiMote is not spreadable (mostly due to the IR thingy and by now also because of the extension architecture
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			if (FPinInputEnable.PinIsChanged || FPinInputID.PinIsChanged) {
				Enable();
				FInvalidate = true;
			}
			
			if (FWorking && (FPinInputMode.PinIsChanged || FPinInputEnable.PinIsChanged)) {
				double mode;
				FPinInputMode.GetValue(0, out mode);
				FIRMode  = (int)Math.Ceiling(mode);
				switch (FIRMode) {
						case 0:FRemote.WiimoteState.IRState.Mode = IRMode.Off;
						break;
						case 1:FRemote.WiimoteState.IRState.Mode = IRMode.Basic;
						break;
						case 2:FRemote.WiimoteState.IRState.Mode = IRMode.Extended;
						break;
						case 3:FRemote.WiimoteState.IRState.Mode = IRMode.Full;
						break;
						
				}
				FRemote.GetStatus();
				FInvalidate = true;
				
			}
			
			FPinOutputWorking.SetString(0, FWorking?"OK: "+FRemote.WiimoteState.IRState.Mode.ToString():FMessage);

			if (!FWorking) return;

			
			if (FPinInputRumble.PinIsChanged || FPinInputEnable.PinIsChanged) {
				double rumble;
				FPinInputRumble.GetValue(0, out rumble);
				FRemote.SetRumble(rumble==1d?true:false);
			}

			if (FPinInputLED.PinIsChanged || FPinInputEnable.PinIsChanged) {
				double[] dLed = new double[4];
				bool[] led = new bool[4];
				for (int i=0;i<4;i++) {
					FPinInputLED.GetValue(i, out dLed[i]);
					led[i] = (dLed[i]==1d)?true:false;
				}
				FRemote.SetLEDs(led[0], led[1], led[2], led[3]);
			}
			

//			careful with this... it will permanently override all factory calibration of your wiimote.
			if ((FPinInputEnable.PinIsChanged  || FPinInputCalibrate.PinIsChanged) && (FPinInputCalibrate.PinIsChanged)) {
				byte[,] data = new byte[3,2];
				double output;

				for (int i=0;i<3;i++) {
					FPinInputCalibrationOneG.GetValue(i, out output);
					data[i,0] = (byte) (output);
					FPinInputCalibrationZeroG.GetValue(i, out output);
					data[i,1] = (byte) (output);
				}
				
				double calibrate;
				FPinInputCalibrate.GetValue(0, out calibrate);
				
				if (calibrate == 1d) {
					FRemote.WiimoteState.AccelCalibrationInfo.X0 = data[0,0];
					FRemote.WiimoteState.AccelCalibrationInfo.Y0 = data[1,0];
					FRemote.WiimoteState.AccelCalibrationInfo.Z0 = data[2,0];
					FRemote.WiimoteState.AccelCalibrationInfo.XG = data[0,1];
					FRemote.WiimoteState.AccelCalibrationInfo.YG = data[1,1];
					FRemote.WiimoteState.AccelCalibrationInfo.ZG = data[2,1];
				}
				FInvalidate = true;
			}
			
			if (FInvalidate) {
				
				
				FPinOutputCursor.SliceCount = 1;
				FPinOutputCursor.SetValue(0, FRemote.WiimoteState.ButtonState.Up?1d:0d);
				FPinOutputCursor.SetValue(1, FRemote.WiimoteState.ButtonState.Down?1d:0d);
				FPinOutputCursor.SetValue(2, FRemote.WiimoteState.ButtonState.Left?1d:0d);
				FPinOutputCursor.SetValue(3, FRemote.WiimoteState.ButtonState.Right?1d:0d);

				FPinOutputControls.SliceCount = 1;
				FPinOutputControls.SetValue(0, FRemote.WiimoteState.ButtonState.Plus?1d:0d);
				FPinOutputControls.SetValue(1, FRemote.WiimoteState.ButtonState.Minus?1d:0d);
				FPinOutputControls.SetValue(2, FRemote.WiimoteState.ButtonState.Home?1d:0d);

				FPinOutputButtons.SliceCount = 1;
				FPinOutputButtons.SetValue(0, FRemote.WiimoteState.ButtonState.A?1d:0d);
				FPinOutputButtons.SetValue(1, FRemote.WiimoteState.ButtonState.B?1d:0d);
				FPinOutputButtons.SetValue(2, FRemote.WiimoteState.ButtonState.One?1d:0d);
				FPinOutputButtons.SetValue(3, FRemote.WiimoteState.ButtonState.Two?1d:0d);
				
				FPinOutputTilt.SliceCount = 1;
				FPinOutputTilt.SetValue2D(0, FRemote.WiimoteState.AccelState.X, FRemote.WiimoteState.AccelState.Y);

				double[] normalizedA = new double[3];
				normalizedA[0] = (double)((FRemote.WiimoteState.AccelState.RawX - 128))/128d;
				normalizedA[1] = (double)((FRemote.WiimoteState.AccelState.RawY- 128))/128d;
				normalizedA[2] = (double)((FRemote.WiimoteState.AccelState.RawZ- 128))/128d;
				FPinOutputAccelleration.SliceCount = 1;
				FPinOutputAccelleration.SetValue3D(0, normalizedA[0], normalizedA[1], normalizedA[2]);
				
				FPinOutputBattery.SetValue(0, (double)FRemote.WiimoteState.Battery/0xFF);
				

//			InfraRed
				
				int irCount = 0;
				if (FRemote.WiimoteState.IRState.Found1) irCount++;
				if (FRemote.WiimoteState.IRState.Found2) irCount++;
				if (FRemote.WiimoteState.IRState.Found3 && FIRMode == 2) irCount++;
				if (FRemote.WiimoteState.IRState.Found4 && FIRMode == 2) irCount++;

				FPinOutputInfraredBlobs.SliceCount = irCount;

				irCount = 0;
				if (FRemote.WiimoteState.IRState.Found1) {
					FPinOutputInfraredBlobs.SetValue4D(irCount, 0d, FRemote.WiimoteState.IRState.RawX1, FRemote.WiimoteState.IRState.RawY1, (FRemote.WiimoteState.IRState.Size1));
					irCount++;
				}
				if (FRemote.WiimoteState.IRState.Found2) {
					FPinOutputInfraredBlobs.SetValue4D(irCount, 1d, FRemote.WiimoteState.IRState.RawX2, FRemote.WiimoteState.IRState.RawY2, (FRemote.WiimoteState.IRState.Size2));
					irCount++;
				}
				if (FRemote.WiimoteState.IRState.Found3 && FIRMode == 2) {
					FPinOutputInfraredBlobs.SetValue4D(irCount, 2d, FRemote.WiimoteState.IRState.RawX3, FRemote.WiimoteState.IRState.RawY3, (FRemote.WiimoteState.IRState.Size3));
					irCount++;
				}
				if (FRemote.WiimoteState.IRState.Found4 && FIRMode == 2) {
					FPinOutputInfraredBlobs.SetValue4D(irCount, 3d, FRemote.WiimoteState.IRState.RawX4, FRemote.WiimoteState.IRState.RawY4, (FRemote.WiimoteState.IRState.Size4));
					irCount++;
				}

				FInvalidate = false;

				ExtensionType ext = FRemote.WiimoteState.ExtensionType;
				string enabledExt = FRemote.WiimoteState.Extension?" disabled":"";
				if (ext == ExtensionType.Nunchuk && FExtension == 1) {
					Nunchuk();
					enabledExt = " enabled";
				}

				if (ext == ExtensionType.ClassicController && FExtension == 2) {
					Classic();
					enabledExt = " enabled";
				}

				FPinOutputExtensionFound.SetString(0, ext.ToString()+enabledExt);
				
				
			}
		}

		private void Classic() {
			FPinOutputExtCursor.SliceCount = 1;
			FPinOutputExtCursor.SetValue(0, FRemote.WiimoteState.ClassicControllerState.ButtonState.Up?1d:0d);
			FPinOutputExtCursor.SetValue(1, FRemote.WiimoteState.ClassicControllerState.ButtonState.Down?1d:0d);
			FPinOutputExtCursor.SetValue(2, FRemote.WiimoteState.ClassicControllerState.ButtonState.Left?1d:0d);
			FPinOutputExtCursor.SetValue(3, FRemote.WiimoteState.ClassicControllerState.ButtonState.Right?1d:0d);

			FPinOutputExtControls.SliceCount = 1;
			FPinOutputExtControls.SetValue(0, FRemote.WiimoteState.ClassicControllerState.ButtonState.Plus?1d:0d);
			FPinOutputExtControls.SetValue(1, FRemote.WiimoteState.ClassicControllerState.ButtonState.Minus?1d:0d);
			FPinOutputExtControls.SetValue(2, FRemote.WiimoteState.ClassicControllerState.ButtonState.Home?1d:0d);

			FPinOutputExtControls.SliceCount = 1;
			FPinOutputExtControls2.SetValue(0, FRemote.WiimoteState.ClassicControllerState.ButtonState.ZL?1d:0d);
			FPinOutputExtControls2.SetValue(1, FRemote.WiimoteState.ClassicControllerState.ButtonState.ZR?1d:0d);
			FPinOutputExtControls2.SetValue(3, FRemote.WiimoteState.ClassicControllerState.ButtonState.TriggerL?1d:0d);
			FPinOutputExtControls2.SetValue(4, FRemote.WiimoteState.ClassicControllerState.ButtonState.TriggerR?1d:0d);

			FPinOutputExtButtons.SliceCount = 1;
			FPinOutputExtButtons.SetValue(0, FRemote.WiimoteState.ClassicControllerState.ButtonState.A?1d:0d);
			FPinOutputExtButtons.SetValue(1, FRemote.WiimoteState.ClassicControllerState.ButtonState.B?1d:0d);
			FPinOutputExtButtons.SetValue(2, FRemote.WiimoteState.ClassicControllerState.ButtonState.X?1d:0d);
			FPinOutputExtButtons.SetValue(3, FRemote.WiimoteState.ClassicControllerState.ButtonState.Y?1d:0d);
			
			FPinOutputExtJoystickLeft.SliceCount = 1;
			FPinOutputExtJoystickLeft.SetValue(0, FRemote.WiimoteState.ClassicControllerState.XL*2);
			FPinOutputExtJoystickLeft.SetValue(1, FRemote.WiimoteState.ClassicControllerState.YL*2);
			
			FPinOutputExtJoystickRight.SliceCount = 1;
			FPinOutputExtJoystickRight.SetValue(0, FRemote.WiimoteState.ClassicControllerState.XR*2);
			FPinOutputExtJoystickRight.SetValue(1, FRemote.WiimoteState.ClassicControllerState.YR*2);
		}
		
		private void Nunchuk() {
			FPinOutputExtButtons.SliceCount = 1;
			FPinOutputExtButtons.SetValue(0, FRemote.WiimoteState.NunchukState.C?1d:0d);
			FPinOutputExtButtons.SetValue(1, FRemote.WiimoteState.NunchukState.Z?1d:0d);

			FPinOutputExtTilt.SliceCount = 1;
			FPinOutputExtTilt.SetValue2D(0, FRemote.WiimoteState.NunchukState.AccelState.X, FRemote.WiimoteState.NunchukState.AccelState.Y);

			double[] normalizedA = new double[3];
			normalizedA[0] = (double)((FRemote.WiimoteState.NunchukState.AccelState.RawX - 128))/128d;
			normalizedA[1] = (double)((FRemote.WiimoteState.NunchukState.AccelState.RawY- 128))/128d;
			normalizedA[2] = (double)((FRemote.WiimoteState.NunchukState.AccelState.RawZ- 128))/128d;
			FPinOutputExtAccelleration.SliceCount = 1;
			FPinOutputExtAccelleration.SetValue3D(0, normalizedA[0], normalizedA[1], normalizedA[2]);
			
			FPinOutputExtJoystickLeft.SliceCount = 1;
			FPinOutputExtJoystickLeft.SetValue(0, FRemote.WiimoteState.NunchukState.X*2);
			FPinOutputExtJoystickLeft.SetValue(1, FRemote.WiimoteState.NunchukState.Y*2);
		}
		
		private void Enable() {
			double ext;
			FPinConfigExtension.GetValue(0, out ext);
			FExtension = (int)Math.Ceiling(ext);

			double enabled;
			FPinInputEnable.GetValue(0, out enabled);

			double id;
			FPinInputID.GetValue(0, out id);

			if (enabled==1d) {
				try
				{
					// if connected, disconnect first);
					if (FWorking && FWiimoteID!= (int)id) {
						FRemote.WiimoteChanged -= new WiimoteChangedEventHandler(OnInvalidatePlugin);
						FRemote.WiimoteExtensionChanged -= new WiimoteExtensionChangedEventHandler(OnChangeReportType);
						FRemote.Disconnect();
					}
					
					if (id < 0) {
						try {
							FRemote = new Wiimote();
							FRemote.Connect();
						} catch (Exception e) {
							FWorking = false;
							throw new Exception("No Wiimote detected. " + e.ToString());
						}
					} else {
//						Connect to all Wiimotes
						List<Wiimote> list = Wiimote.GetConnectedWiimotes();
						
						FPinOutputAvailable.SliceCount = list.Count;
						for (int i=0;i<list.Count;i++) {
							FPinOutputAvailable.SetValue(i, i);
						}

						try {
							FRemote = list[(int)id];
						}
						catch (Exception e) {
							FWorking = false;
							throw new Exception("No Wiimote with that ID detected. " + e.ToString());
						}
					}

					if (FRemote.WiimoteState.Extension == true) {
						FRemote.SetReportType(Wiimote.InputReport.IRExtensionAccel, true);
					} else {
						FRemote.SetReportType(Wiimote.InputReport.IRAccel, true);
					}
					FRemote.GetStatus();

					FRemote.SetLEDs(false, false, false, true); // light number vvvvour
					FRemote.WiimoteChanged += new WiimoteChangedEventHandler(OnInvalidatePlugin);
					FRemote.WiimoteExtensionChanged += new WiimoteExtensionChangedEventHandler(OnChangeReportType);
					FWorking = true;
					FMessage = "OK";
				}
				catch (Exception x)
				{
					FMessage = x.Message;
					FWorking = false;
				}
			}
			else {
				FWorking = false;
				FMessage = "Disabled";
				
				try
				{
					FRemote.SetRumble(false);
					FRemote.Disconnect();
				}
				catch (Exception x)
				{
					FMessage = x.Message;
					FWorking = false;
				}
			}
			
			
		}
		#endregion mainloop
	}
}
