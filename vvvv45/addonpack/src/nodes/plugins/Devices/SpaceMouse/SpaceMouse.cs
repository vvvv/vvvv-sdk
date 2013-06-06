#region licence/info

//////project name
//SpaceMouse

//////description
//driver for 3DConnexions SpaceNavigator

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
//TDx.TDxInput as shipping with the 3DConnexion driver

//////initial author
//velcrome
//////update by
//joreg: modelled after source provided via: http://www.3dconnexion.com/forum/viewtopic.php?p=2213#2213

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using TDx.TDxInput;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class SpaceMousePlugin: IPlugin, IDisposable
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
   		private bool FDisposed = false;
   		
		//output pin declaration
		private IValueOut FPositionPin;
		private IValueOut FRotationPin;
		private IValueOut FAnglePin;
		private IValueOut FKeyboardPin;
		private IValueOut FDeviceTypePin;

		private delegate void SetDeviceTextCallback();
		private delegate void SetMotionTextCallback();
		private delegate void SetKeyTextCallback(int keyCode);

		private TDx.TDxInput.Vector3D FTranslation = new TDx.TDxInput.Vector3DClass();
		private	TDx.TDxInput.AngleAxis FRotation = new TDx.TDxInput.AngleAxisClass();
		private int FDeviceType;
		
		private TDx.TDxInput.Sensor FSensor;
		private TDx.TDxInput.Keyboard FKeyboard;
		private TDx.TDxInput.Device FDevice;
		
		//a list that holds the state for every button
		private Dictionary<int, double> FButtonStates = new Dictionary<int, double>();
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public SpaceMousePlugin()
		{
			try
			{
				FDevice = new TDx.TDxInput.Device();
				FSensor = FDevice.Sensor;
				FKeyboard = FDevice.Keyboard;

				// Add the event handlers
				FDevice.DeviceChange += new TDx.TDxInput._ISimpleDeviceEvents_DeviceChangeEventHandler(DeviceChange);

				//Connect everything up
				FDevice.Connect();
			}
			catch (COMException e)
			{
				Console.Error.WriteLine("SpaceMouse: " + e.Message);
			}
		}
		
		// Implementing IDisposable's Dispose method.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FSensor = null;
					FKeyboard = null;
					if (FDevice != null)
						FDevice.Disconnect();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				if (FHost != null)
					FHost.Log(TLogType.Debug, "SpaceMouse is being deleted");
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~SpaceMousePlugin()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
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
				Info.Name = "SpaceMouse";
				Info.Category = "Devices";
				Info.Version = "";
				Info.Help = "Offers support for 3DConnexion USB Devices. Needs 3DConnexion driver installed: http://www.3dconnexion.com/support/downloads.php";
				Info.Bugs = "Button state output is not reliable. The node may only exist once, multiple instances won't work.";
				Info.Credits = "velcrome (for the initial version)";
				Info.Warnings = "";
				
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
			//assign host
			FHost = Host;

			//create inputs

			//create outputs
			FHost.CreateValueOutput("Position", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionPin);
			FPositionPin.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0,0,0, false, false, false);
			FPositionPin.SliceCount = 1;
			
			FHost.CreateValueOutput("Rotation", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FRotationPin);
			FRotationPin.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0,0,0, false, false, false);
			FRotationPin.SliceCount = 1;
			
			FHost.CreateValueOutput("Angle", 1, null, TSliceMode.Single, TPinVisibility.True, out FAnglePin);
			FAnglePin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

			FHost.CreateValueOutput("Buttons", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FKeyboardPin);
			FKeyboardPin.SetSubType(0, 1, 1, 0, false, true, true);

			FHost.CreateValueOutput("Device Type", 1, null, TSliceMode.Single, TPinVisibility.True, out FDeviceTypePin);
			FDeviceTypePin.SetSubType(0, int.MaxValue, 1, 0, false, false, true);
			
			DeviceChange(0);
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			FPositionPin.SetValue3D(0, FSensor.Translation.X, FSensor.Translation.Y, FSensor.Translation.Z);
			FRotationPin.SetValue3D(0, FSensor.Rotation.X, FSensor.Rotation.Y, FSensor.Rotation.Z);
			FAnglePin.SetValue(0, FSensor.Rotation.Angle);

			for (int i=0; i<FKeyboard.Keys; i++)
			{
				if (FKeyboard.IsKeyDown(i+1))
					FKeyboardPin.SetValue(i, 1);
				else
					FKeyboardPin.SetValue(i, 0);					
			}
			
			FDeviceTypePin.SetValue(0, FDeviceType);
		}
		#endregion mainloop
		
		void DeviceChange(int reserved)
		{
			FDeviceType = FDevice.Type;
			
			int key;
			FButtonStates.Clear();
			FKeyboardPin.SliceCount = FKeyboard.Keys;
			for (key = 1; key <= FKeyboard.Keys; key++)
				FButtonStates.Add(key, 0);
		}
	}
}

