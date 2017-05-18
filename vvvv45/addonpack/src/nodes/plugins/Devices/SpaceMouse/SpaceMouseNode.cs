using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using VVVV.Core.Logging;

using VVVV.PluginInterfaces.V2;

using Vector3D = VVVV.Utils.VMath.Vector3D;
using SpaceVector3D = TDx.TDxInput.Vector3D;

namespace VVVV.Nodes
{

	#region PluginInfo
	[PluginInfo(Name = "SpaceMouse",
	            Category = "Devices",
	            Help = "Offers support for 3DConnexion USB Devices. Needs 3DConnexion driver installed: http://www.3dconnexion.com/support/downloads.php",
	            Tags = "navigator, 3DConnexion",
                Bugs =  "Button state output is not reliable, seem like it is mishandled by 3DConnexions Application Makros. Also, the node may only exist once, multiple instances won't work.",
                Credits = "velcrome")]
	#endregion PluginInfo	
    public class SpaceMousePlugin: IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
	{
        [Output("Position")]
        public ISpread<Vector3D> FPositionOut;

        [Output("Rotation")]
        public ISpread<Vector3D> FRotationOut;

        [Output("Angle", IsSingle = true)]
        public ISpread<double> FAngleOut;

        [Output("Buttons", IsBang = true)]
        public ISpread<bool> FKeyboardOut;

        [Output("Device Type")]
        public ISpread<int> FDeviceOut;

        [Import]
        public ILogger FLogger;

		private TDx.TDxInput.Sensor FSensor;
		private TDx.TDxInput.Keyboard FKeyboard;
		private TDx.TDxInput.Device FDevice;
        private int FDeviceType;

        public void OnImportsSatisfied()
        {
            try
            {
                FDevice = new TDx.TDxInput.Device();
                FSensor = FDevice.Sensor;
                FKeyboard = FDevice.Keyboard;

                // Add the event handlers
                FDevice.DeviceChange += DeviceChange;

                // Connect everything up
                FDevice.Connect();
                FDeviceType = FDevice.Type;
            }
            catch (COMException e)
            {
                FLogger.Log(LogType.Error, "SpaceMouse: " + e.Message);
            }
        }

		public void Dispose()
		{
			FSensor = null;
			FKeyboard = null;
            if (FDevice != null)
            {
                FDevice.DeviceChange -= DeviceChange;
                FDevice.Disconnect();
            }
		}
		
		public void Evaluate(int spreadMax)
		{
            if (FDevice == null)
                return;

            FPositionOut[0] = new Vector3D(FSensor.Translation.X, FSensor.Translation.Y, FSensor.Translation.Z) / 3000.0;
            FRotationOut[0] = new Vector3D(FSensor.Rotation.X, FSensor.Rotation.Y, FSensor.Rotation.Z);
            FAngleOut[0] = FSensor.Rotation.Angle / 3000;

		    FKeyboardOut.SliceCount = FKeyboard.Keys;
			for (int i=0; i<FKeyboard.Keys; i++)
                FKeyboardOut[i] = FKeyboard.IsKeyDown(i + 1);

            FDeviceOut[0] = FDeviceType;
		}

        void DeviceChange(int reserved)
        {
            FDeviceType = FDevice.Type;
        }
    }
}
