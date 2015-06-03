using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using VVVV.Core.Logging;

using VVVV.PluginInterfaces.V2;

using Vector3D = VVVV.Utils.VMath.Vector3D;
using SpaceVector3D = TDx.TDxInput.Vector3D;

//the vvvv node namespace
namespace VVVV.Nodes
{

	#region PluginInfo
	[PluginInfo(Name = "SpaceMouse",
	            Category = "Devices",
	            Help = "Offers support for 3DConnexion USB Devices. Needs 3DConnexion driver installed: http://www.3dconnexion.com/support/downloads.php",
	            Tags = "Space Navigator 3DConnexion",
                Bugs =  "Button state output is not reliable, seem like it is mishandled by 3DConnexions Application Makros. Also, the node may only exist once, multiple instances won't work.",
                Credits = "velcrome")]
	#endregion PluginInfo	
    public class SpaceMouseNode: IPluginEvaluate, IDisposable
	{
		#region field declaration
		
        [Output("Position")]
        public ISpread<Vector3D> FPositionOut;

        [Output("Rotation")]
        public ISpread<Vector3D> FRotationOut;

        [Output("Angle", IsSingle = true)]
        public ISpread<double> FAngleOut;

        [Output("Buttons", IsBang = true)]
        public ISpread<bool> FKeyboardOut;

        [Output("DeviceType")]
        public ISpread<int> FDeviceOut;

        [Import()]
        public ILogger FLogger;


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
        // Track whether Dispose has been called.
        private bool FDisposed = false;
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public SpaceMouseNode()
		{
			try
			{
				FDevice = new TDx.TDxInput.Device();
				FSensor = FDevice.Sensor;
				FKeyboard = FDevice.Keyboard;

				//Connect everything up
				FDevice.Connect();
			}
			catch (COMException e)
			{
				FLogger.Log(LogType.Error, "SpaceMouse: " + e.Message);
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
				
				FLogger.Log(LogType.Debug, "SpaceMouse is being deleted");
				
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
		~SpaceMouseNode()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}
		
		#endregion constructor/destructor
		
		
		#region mainloop

		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{

		    SpreadMax = 1;
            FPositionOut.SliceCount = SpreadMax;
		    FRotationOut.SliceCount = 1;
		    FAngleOut.SliceCount = 1;

            FPositionOut[0] = new Vector3D( FSensor.Translation.X, FSensor.Translation.Y, FSensor.Translation.Z) / 3000.0;
            FRotationOut[0] = new Vector3D(FSensor.Rotation.X, FSensor.Rotation.Y, FSensor.Rotation.Z);

            FAngleOut[0] = FSensor.Rotation.Angle / 3000;


		    FKeyboardOut.SliceCount = FKeyboard.Keys;

			for (int i=0; i<FKeyboard.Keys; i++)
			{
                FKeyboardOut[i] = FKeyboard.IsKeyDown(i + 1);
			}
			
			FDeviceOut[0] = FDeviceType;
		}
		#endregion mainloop
	}
}

