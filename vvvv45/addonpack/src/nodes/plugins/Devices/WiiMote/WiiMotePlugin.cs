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
	public class PluginWiiMote: IPlugin, IDisposable
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		private Wiimote FRemote ;
		private bool FWorking = false;
		private bool FInvalidate = true;
		private bool FUseMotionPlus = false;
		private String FWiimoteID = "";
		
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
		
		private IValueIn FPinForceReset;
		private IEnumIn FPinInputMode;
		private int FIRMode = 2;
		
		private IStringIn FPinInputID;

		private IEnumConfig FPinConfigExtension;
		private IValueConfig FPinMotionPlus;
		private int FExtension = 0;
		
		
		//output pin declaration
		private IValueOut FPinOutputAccelleration;
		private IValueOut FPinOutputTilt;
		private IStringOut FPinOutputAvailable;
		
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

		//	MotionPlus
		private IValueOut FPinMotionPlusOutputAccelleration;
		private IValueOut FPinMotionPlusOutputTilt;
		
		
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public PluginWiiMote()
		{
			FRemote = new Wiimote();
			
			//the nodes constructor
			//nothing to declare for this node
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		private bool FDisposed;
		private void Dispose(bool disposing)
		{
			if (!FDisposed)
			{
				if (disposing)
				{
					FRemote.Dispose();
					FRemote = null;
				}
				
				FDisposed = true;
			}
		}
		
		~PluginWiiMote()
		{
			Dispose(false);
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
				Info.Bugs = "No Calibration for the Extensions implemented yet. / VVVV Crushes if the Connection to the Controler is lost";
				Info.Credits = "Plugin Port: velcrome@gmx.net \n" +
					"Basic Wiimotelib: http://www.brianpeek.com";
				Info.Warnings = "Not throughly tested yet";
                Info.Tags = "Wii,Controler";

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

			FHost.CreateStringInput("Wiimote ID", TSliceMode.Dynamic,TPinVisibility.OnlyInspector,  out FPinInputID);
			FPinInputID.SetSubType("-1", false);

			FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single,TPinVisibility.True,  out FPinInputEnable);
			FPinInputEnable.SetSubType(0, 1, 1, 0, false, true, false);

			FHost.CreateValueInput("LED", 4, new string[4] {"0", "1", "2", "3"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinInputLED);
			FPinInputLED.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

			FHost.CreateValueInput("Rumble", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinInputRumble);
			FPinInputRumble.SetSubType(0, 1, 1, 0, false, true, false);

			FHost.UpdateEnum("WiimoteInfraredMode", "Extended", new string[4] {"Full", "Extended", "Basic", "Off"});
			
			FHost.CreateEnumInput("Infrared Mode", TSliceMode.Dynamic, TPinVisibility.True, out FPinInputMode);
			FPinInputMode.SetSubType("WiimoteInfraredMode");

			FHost.CreateValueFastInput("Calibration ZeroG", 3, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPinInputCalibrationZeroG);
			FPinInputCalibrationZeroG.SetSubType3D(0, 0xFF, 1, 126, 126, 131, false, false, true);

			FHost.CreateValueFastInput("Calibration OneG", 3, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPinInputCalibrationOneG);
			FPinInputCalibrationOneG.SetSubType3D(0, 0xFF, 1, 151, 151, 151, false, false, true);

			FHost.CreateValueInput("Calibrate", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPinInputCalibrate);
			FPinInputCalibrate.SetSubType(0, 1, 1, 0, true, false, false);
			
			FHost.CreateValueInput("Force Reset", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPinForceReset);
			FPinForceReset.SetSubType(0, 1, 1, 0, true, false, false);

			FHost.UpdateEnum("WiimoteExtensionEnum", "none", new string[5]{"none", "Nunchuk", "Classic", "BalanceBoard", "Guitar"});
			FHost.CreateEnumConfig("Extension", TSliceMode.Single, TPinVisibility.True, out FPinConfigExtension);
			FPinConfigExtension.SetSubType("WiimoteExtensionEnum");
			
//			FHost.CreateValueConfig("MotionPlus", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinMotionPlus);
//			FPinMotionPlus.SetSubType(0, 1, 1, 0, false, true, false);
			
			//create outputs
			FHost.CreateStringOutput("Available Wiimotes", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPinOutputAvailable);
			FPinOutputAvailable.SetSubType("none", false);

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
			
			FHost.CreateValueOutput("Acceleration", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputAccelleration);
			FPinOutputAccelleration.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
			
			FHost.CreateValueOutput("Angle", 3, new string[3]{"Pitch", "Roll", "Yaw"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputTilt);
			FPinOutputTilt.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);

			FHost.CreateValueOutput("Infrared", 3, new string[3]{"X", "Y", "Size"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputInfraredBlobs);
			FPinOutputInfraredBlobs.SetSubType3D(0, 1023, 1, 0, 0, 0, false, false, true);

			Enable();
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
/*
 * MotionPlus is included in the highly beta 1.8 version of brian peeks library
 * http://wiimotelib.codeplex.com/releases/view/30401
 * 
 * so far it only supports raw accelleration data, no tilt no nothing
 
 			double motionplus;
			FPinMotionPlus.GetValue(0, out motionplus);
			
			if ((motionplus==1d) != FUseMotionPlus) {
				FUseMotionPlus = (motionplus==1d);

				if (FPinMotionPlusOutputAccelleration != null) FHost.DeletePin(FPinMotionPlusOutputAccelleration);
//				if (FPinMotionPlusOutputTilt!= null)FHost.DeletePin(FPinMotionPlusOutputTilt);

				if (FUseMotionPlus) {
					FHost.CreateValueOutput("MotionPlusAccelleration", 3, new string[3]{"X", "Y", "Z"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinMotionPlusOutputAccelleration);
					FPinMotionPlusOutputAccelleration.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
//					FHost.CreateValueOutput("MotionPlusTilt", 3, new string[3]{"Pitch", "Roll", "Yaw"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinMotionPlusOutputTilt);
//					FPinMotionPlusOutputTilt.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
					FRemote.InitializeMotionPlus();
				} 
			}
*/			
			

			int newExt;
			FPinConfigExtension.GetOrd(0, out newExt);
			
			if (newExt != FExtension) {
				if (FPinOutputExtAccelleration != null) FHost.DeletePin(FPinOutputExtAccelleration);
				if (FPinOutputExtTilt != null)FHost.DeletePin(FPinOutputExtTilt);
				if (FPinOutputExtButtons != null)FHost.DeletePin(FPinOutputExtButtons);
				if (FPinOutputExtCursor != null)FHost.DeletePin(FPinOutputExtCursor);
				if (FPinOutputExtControls!= null)FHost.DeletePin(FPinOutputExtControls);

				if (FPinOutputExtJoystickLeft != null)FHost.DeletePin(FPinOutputExtJoystickLeft);
				if (FPinOutputExtJoystickRight!= null)FHost.DeletePin(FPinOutputExtJoystickRight);
				if (FPinOutputExtControls2!= null)FHost.DeletePin(FPinOutputExtControls2);
			
			FExtension = newExt;
			
			switch (FExtension) {
				case 1: //Nunchuk
					FHost.Log(TLogType.Debug, "Nunchuk");

					FHost.CreateValueOutput("Nunchuk Buttons", 2, new string[2]{"C", "Z"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtButtons);
					FPinOutputExtButtons.SetSubType2D(0, 1, 1, 0, 0, false, true, false);
					
					FHost.CreateValueOutput("Nunchuk Accelleration", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtAccelleration);
					FPinOutputExtAccelleration.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("Nunchuk Tilt", 3, new string[3]{"Pitch", "Roll", "Yaw"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtTilt);
					FPinOutputExtTilt.SetSubType3D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("Nunchuk Joystick", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtJoystickLeft);
					FPinOutputExtJoystickLeft.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);
					break;
				case 2: // Classic
					FHost.Log(TLogType.Debug, "Classic");

					FHost.CreateValueOutput("Classic Cursor", 4, new string[4]{"Up", "Down", "Left", "Right"}, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtCursor);
					FPinOutputExtCursor.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

					FHost.CreateValueOutput("Classic Buttons", 4, new string[4]{"A", "B", "X", "Y"}, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtButtons);
					FPinOutputExtButtons.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

					FHost.CreateValueOutput("Classic System", 3, new string[3]{"Plus", "Minus", "Home"}, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtControls);
					FPinOutputExtControls.SetSubType3D(0, 1, 1, 0, 0, 0, false, true, false);
					
					FHost.CreateValueOutput("Classic Joystick Left", 2, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtJoystickLeft);
					FPinOutputExtJoystickLeft.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("Classic Joystick Right", 2, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtJoystickRight);
					FPinOutputExtJoystickRight.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);

					FHost.CreateValueOutput("Classic Back Control", 4, new string[4]{"Left", "Right", "Left", "Right"}, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtControls2);
					FPinOutputExtControls2.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);
					
					break;
				case 3: //BalanceBoard
					FHost.Log(TLogType.Debug, "BalanceBoard");

					FHost.CreateValueOutput("Balance Board Weight (kg)", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtButtons);
					FPinOutputExtButtons.SetSubType(0, double.MaxValue, 1, 0, false, false, false);

                    FHost.CreateValueOutput("Balance Board Corners", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtAccelleration);
                    FPinOutputExtAccelleration.SetSubType(double.MinValue, double.MaxValue, 0.0001, 0, false, false, false);

					//FHost.CreateValueOutput("Balance Board Corners", 4, new string[4]{"TL", "TR", "BL", "BR"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtAccelleration);
					//FPinOutputExtAccelleration.SetSubType4D(double.MinValue, double.MaxValue, 0.0001, 0, 0, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("Balance Board Center Of Gravity", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtJoystickLeft);
					FPinOutputExtJoystickLeft.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);
					break;					
				case 4: // Guitar
					FHost.Log(TLogType.Debug, "Guitar");
					
					FHost.CreateValueOutput("Guitar Buttons", 4, new string[4]{"Plus", "Minus", "StrumUp", "StrumDown"}, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtButtons);
					FPinOutputExtButtons.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

					FHost.CreateValueOutput("Guitar Color Bar", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputExtControls);
					FPinOutputExtControls.SetSubType(0, 1, 1, 0, false, true, false);
					
					FHost.CreateValueOutput("Guitar Whammy Bar", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtJoystickLeft);
					FPinOutputExtJoystickLeft.SetSubType2D(0, 1, 0.0001, 0, 0, false, false, false);
					
					FHost.CreateValueOutput("Guitar Joystick", 2, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputExtJoystickRight);
					FPinOutputExtJoystickRight.SetSubType2D(-1, 1, 0.0001, 0, 0, false, false, false);

					break;
				default: // None
					FHost.Log(TLogType.Debug, "No Extension");

					break;
			}
			}
			FInvalidate = true;
		}
		
		
		void OnInvalidatePlugin(object Sender, WiimoteChangedEventArgs Args)
		{
			FInvalidate = true;
		}
		
		void OnChangeReportType(object Sender, WiimoteExtensionChangedEventArgs Args) {
			if (Args.ExtensionType == ExtensionType.ClassicController) {
				FRemote.SetReportType(InputReport.IRExtensionAccel, true);
			}
			if (Args.ExtensionType == ExtensionType.BalanceBoard) {
				FRemote.SetReportType(InputReport.IRExtensionAccel, true);
			}
			if (Args.ExtensionType == ExtensionType.Guitar) {
				FRemote.SetReportType(InputReport.IRExtensionAccel, true);
			}
			if (Args.ExtensionType == ExtensionType.Nunchuk) {
				FRemote.SetReportType(InputReport.IRExtensionAccel, true);
			}
//			if (Args.ExtensionType == ExtensionType.MotionPlus) {
//				FRemote.SetReportType(InputReport.IRExtensionAccel, true);
//			}
			if (Args.ExtensionType == ExtensionType.None) {
				FRemote.SetReportType(InputReport.IRAccel, true);
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
				FPinInputMode.GetOrd(0, out FIRMode);
				if (FIRMode == 0) FIRMode = 1;
				
				switch (FIRMode) {
						case 3:FRemote.WiimoteState.IRState.Mode = IRMode.Off;
						break;
						case 2:FRemote.WiimoteState.IRState.Mode = IRMode.Basic;
						break;
						case 1:FRemote.WiimoteState.IRState.Mode = IRMode.Extended;
						break;
						case 0:FRemote.WiimoteState.IRState.Mode = IRMode.Full;
						break;
						
				}
				FRemote.GetStatus();
				FInvalidate = true;
				
			}
			
			FPinOutputWorking.SetString(0, FWorking?"OK: "+FRemote.WiimoteState.IRState.Mode.ToString():FMessage);

			if (!FWorking) return;
			
			
			if (FPinForceReset.PinIsChanged) {
				double reset;
				FPinForceReset.GetValue(0, out reset);
				
				if (reset == 1d) {
					if (FRemote.WiimoteState.Extension) 
						FRemote.SetReportType(InputReport.IRExtensionAccel, true);
						else FRemote.SetReportType(InputReport.IRAccel, true);
					}
			}
			
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
			
			if (FInvalidate) 
			{
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
				FPinOutputTilt.SetValue3D(0, FRemote.WiimoteState.AccelState.Values.X, FRemote.WiimoteState.AccelState.Values.Y, FRemote.WiimoteState.AccelState.Values.Z);

				double[] normalizedA = new double[3];
				normalizedA[0] = (double)((FRemote.WiimoteState.AccelState.RawValues.X - 128))/128d;
				normalizedA[1] = (double)((FRemote.WiimoteState.AccelState.RawValues.Y- 128))/128d;
				normalizedA[2] = (double)((FRemote.WiimoteState.AccelState.RawValues.Z- 128))/128d;
				FPinOutputAccelleration.SliceCount = 1;
				FPinOutputAccelleration.SetValue3D(0, normalizedA[0], normalizedA[1], normalizedA[2]);
				
				FPinOutputBattery.SetValue(0, (double)FRemote.WiimoteState.Battery/0xFF);
				
				int irCount = 0;
				if (FIRMode == 0) irCount = 4; // Full
				if (FIRMode == 1) irCount = 4; // Extended
				if (FIRMode == 2) irCount = 2; // Basic
				if (FIRMode == 3) irCount = 0; // Of
				
				FPinOutputInfraredBlobs.SliceCount = irCount;

				for (int i = 0;i<irCount;i++) {
					FPinOutputInfraredBlobs.SetValue3D(i, FRemote.WiimoteState.IRState.IRSensors[i].RawPosition.X, FRemote.WiimoteState.IRState.IRSensors[i].RawPosition.Y, FRemote.WiimoteState.IRState.IRSensors[i].Size);
				}

//			MotionPlus
				if (FUseMotionPlus) {
//					MotionPlus();
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

				if (ext == ExtensionType.BalanceBoard && FExtension == 3) {
					BalanceBoard();
					enabledExt = " enabled";
				}				

				if (ext == ExtensionType.Guitar && FExtension == 4) {
					Guitar();
					enabledExt = " enabled";
				}
				FPinOutputExtensionFound.SetString(0, ext.ToString()+enabledExt);
				
				
			}
		}
		
/*		private void MotionPlus() {
			FPinMotionPlusOutputAccelleration.SliceCount = 1;

			double[] normalizedA = new double[3];
			normalizedA[0] = (double)(FRemote.WiimoteState.MotionPlusState.RawValues.X);
			normalizedA[1] = (double)(FRemote.WiimoteState.MotionPlusState.RawValues.Y);
			normalizedA[2] = (double)(FRemote.WiimoteState.MotionPlusState.RawValues.Z);
			
			FPinMotionPlusOutputAccelleration.SetValue3D(0, normalizedA[0], normalizedA[1], normalizedA[2]);
		}
*/
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
			FPinOutputExtJoystickLeft.SetValue(0, FRemote.WiimoteState.ClassicControllerState.RawJoystickL.X*2);
			FPinOutputExtJoystickLeft.SetValue(1, FRemote.WiimoteState.ClassicControllerState.RawJoystickL.Y*2);
			
			FPinOutputExtJoystickRight.SliceCount = 1;
			FPinOutputExtJoystickRight.SetValue(0, FRemote.WiimoteState.ClassicControllerState.RawJoystickR.X*2);
			FPinOutputExtJoystickRight.SetValue(1, FRemote.WiimoteState.ClassicControllerState.RawJoystickR.Y*2);
		}
		
		private void Nunchuk() {
			FPinOutputExtButtons.SliceCount = 1;
			FPinOutputExtButtons.SetValue(0, FRemote.WiimoteState.NunchukState.C?1d:0d);
			FPinOutputExtButtons.SetValue(1, FRemote.WiimoteState.NunchukState.Z?1d:0d);

			FPinOutputExtTilt.SliceCount = 1;
			FPinOutputExtTilt.SetValue3D(0, FRemote.WiimoteState.NunchukState.AccelState.Values.X, FRemote.WiimoteState.NunchukState.AccelState.Values.Y, FRemote.WiimoteState.NunchukState.AccelState.Values.Z);

			double[] normalizedA = new double[3];
			normalizedA[0] = (double)((FRemote.WiimoteState.NunchukState.AccelState.RawValues.X - 128))/128d;
			normalizedA[1] = (double)((FRemote.WiimoteState.NunchukState.AccelState.RawValues.Y - 128))/128d;
			normalizedA[2] = (double)((FRemote.WiimoteState.NunchukState.AccelState.RawValues.Z- 128))/128d;
			FPinOutputExtAccelleration.SliceCount = 1;
			FPinOutputExtAccelleration.SetValue3D(0, normalizedA[0], normalizedA[1], normalizedA[2]);
			
			FPinOutputExtJoystickLeft.SliceCount = 1;
			FPinOutputExtJoystickLeft.SetValue(0, FRemote.WiimoteState.NunchukState.Joystick.X*2);
			FPinOutputExtJoystickLeft.SetValue(1, FRemote.WiimoteState.NunchukState.Joystick.Y*2);
		}
		
		private void BalanceBoard () {
			FPinOutputExtButtons.SetValue(0, FRemote.WiimoteState.BalanceBoardState.WeightKg);

            FPinOutputExtAccelleration.SliceCount = 4;
            FPinOutputExtAccelleration.SetValue(0, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.TopLeft);
            FPinOutputExtAccelleration.SetValue(1, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.TopRight);
            FPinOutputExtAccelleration.SetValue(2, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft);
            FPinOutputExtAccelleration.SetValue(3, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.BottomRight);

			//FPinOutputExtAccelleration.SliceCount = 1;
			//FPinOutputExtAccelleration.SetValue4D(0, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.TopLeft, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.TopRight, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft, FRemote.WiimoteState.BalanceBoardState.SensorValuesKg.BottomRight);
			
			FPinOutputExtJoystickLeft.SliceCount = 1;
			FPinOutputExtJoystickLeft.SetValue2D(0, FRemote.WiimoteState.BalanceBoardState.CenterOfGravity.X*2, FRemote.WiimoteState.BalanceBoardState.CenterOfGravity.Y*2);
		}	
		
		private void Guitar () {
			FPinOutputExtButtons.SliceCount = 1;
			FPinOutputExtButtons.SetValue4D(0, FRemote.WiimoteState.GuitarState.ButtonState.Plus?1d:0d, FRemote.WiimoteState.GuitarState.ButtonState.Minus?1d:0d, FRemote.WiimoteState.GuitarState.ButtonState.StrumUp?1d:0d, FRemote.WiimoteState.GuitarState.ButtonState.StrumDown?1d:0d);
			
			FPinOutputExtControls.SliceCount = 5;
			FPinOutputExtAccelleration.SetValue(0, FRemote.WiimoteState.GuitarState.FretButtonState.Green?1d:0d);
			FPinOutputExtAccelleration.SetValue(1, FRemote.WiimoteState.GuitarState.FretButtonState.Red?1d:0d);
			FPinOutputExtAccelleration.SetValue(2, FRemote.WiimoteState.GuitarState.FretButtonState.Yellow?1d:0d);
			FPinOutputExtAccelleration.SetValue(3, FRemote.WiimoteState.GuitarState.FretButtonState.Blue?1d:0d);
			FPinOutputExtAccelleration.SetValue(4, FRemote.WiimoteState.GuitarState.FretButtonState.Orange?1d:0d);
			
			FPinOutputExtJoystickLeft.SliceCount = 1;
			FPinOutputExtJoystickLeft.SetValue(0, FRemote.WiimoteState.GuitarState.WhammyBar);

			FPinOutputExtJoystickRight.SliceCount = 1;
			FPinOutputExtJoystickLeft.SetValue2D(0, FRemote.WiimoteState.GuitarState.Joystick.X*2, FRemote.WiimoteState.GuitarState.Joystick.Y*2);
		}
		
		private void Enable() {
			FPinConfigExtension.GetOrd(0, out FExtension );

			double enabled;
			FPinInputEnable.GetValue(0, out enabled);

			String id;
			FPinInputID.GetString(0, out id);

			if (enabled==1d) {
				try
				{
					// if connected, disconnect first);
					if (FWorking && FWiimoteID!= id) {
						FRemote.WiimoteChanged -= OnInvalidatePlugin;
						FRemote.WiimoteExtensionChanged -= OnChangeReportType;
						FRemote.Disconnect();
					}
					
					WiimoteCollection wc = new WiimoteCollection();
					wc.FindAllWiimotes();
				
					FPinOutputAvailable.SliceCount = wc.Count;
					for (int i=0;i<wc.Count;i++) {
						FPinOutputAvailable.SetString(i, wc[(int)i].HIDDevicePath);
					}

					if (id == "-1" || id == "null") id = ""; // to keep compatibility with the older nodes.

					try {
						FRemote = null;
						if (id == "") FRemote = wc[(int)0];
							else for (int i=0;i<wc.Count;i++) {
								if (wc[(int)i].HIDDevicePath == id) FRemote = wc[(int)i];
						}						
						if (FRemote == null) {
							FWorking = false;
							throw new Exception("No Wiimote with that ID detected. " );
						}
					}
					catch (Exception e) {
						FWorking = false;
						throw new Exception("No Wiimote with that ID detected. " + e.ToString());
					}

					if (FRemote.WiimoteState.Extension == true) {
						FRemote.SetReportType(InputReport.IRExtensionAccel, true);
					} else {
						FRemote.SetReportType(InputReport.IRAccel, true);
					}
					FRemote.Connect();
					
					FRemote.SetLEDs(false, false, false, true); // light number vvvvour
					FRemote.WiimoteChanged += OnInvalidatePlugin;
					FRemote.WiimoteExtensionChanged += OnChangeReportType;
					FWorking = true;
					FMessage = "OK";

					
					if (FUseMotionPlus) {
//						FRemote.InitializeMotionPlus();
					}
					FRemote.GetStatus();

				}
				catch(WiimoteNotFoundException ex)
				{
					FMessage = "Wiimote not found error";
					FWorking = false;
				}
				catch(WiimoteException ex)
				{
					FMessage = "Wiimote error";
					FWorking = false;
				}
				catch (Exception ex)
				{
					FMessage = ex.Message;
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
			FInvalidate = true;
		}
		#endregion mainloop
	}
}
