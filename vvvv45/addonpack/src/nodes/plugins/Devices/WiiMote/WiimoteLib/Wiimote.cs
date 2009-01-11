//////////////////////////////////////////////////////////////////////////////////
//	Wiimote.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//  and http://www.codeplex.com/WiimoteLib
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace WiimoteLib
{
	/// <summary>
	/// Implementation of Wiimote
	/// </summary>
	public class Wiimote : IDisposable
	{
		/// <summary>
		/// Event raised when Wiimote state is changed
		/// </summary>
		public event EventHandler<WiimoteChangedEventArgs> WiimoteChanged;

		/// <summary>
		/// Event raised when an extension is inserted or removed
		/// </summary>
		public event EventHandler<WiimoteExtensionChangedEventArgs> WiimoteExtensionChanged;

		// VID = Nintendo, PID = Wiimote
		private const int VID = 0x057e;
		private const int PID = 0x0306;

		// sure, we could find this out the hard way using HID, but trust me, it's 22
		private const int REPORT_LENGTH = 22;

		// Wiimote output commands
		private enum OutputReport : byte
		{
			LEDs			= 0x11,
			Type			= 0x12,
			IR				= 0x13,
			Status			= 0x15,
			WriteMemory		= 0x16,
			ReadMemory		= 0x17,
			IR2				= 0x1a,
		};

		// Wiimote registers
		private const int REGISTER_IR				= 0x04b00030;
		private const int REGISTER_IR_SENSITIVITY_1	= 0x04b00000;
		private const int REGISTER_IR_SENSITIVITY_2	= 0x04b0001a;
		private const int REGISTER_IR_MODE			= 0x04b00033;

		private const int REGISTER_EXTENSION_INIT			= 0x04a40040;
		private const int REGISTER_EXTENSION_TYPE			= 0x04a400fe;
		private const int REGISTER_EXTENSION_CALIBRATION	= 0x04a40020;

		// length between board sensors
		private const int BSL = 43;

		// width between board sensors
		private const int BSW = 24;

		// read/write handle to the device
		private SafeFileHandle mHandle;

		// a pretty .NET stream to read/write from/to
		private FileStream mStream;

		// report buffer
		private readonly byte[] mBuff = new byte[REPORT_LENGTH];

		// read data buffer
		private byte[] mReadBuff;

		// address to read from
		private int mAddress;

		// size of requested read
		private short mSize;

		// current state of controller
		private readonly WiimoteState mWiimoteState = new WiimoteState();

		// event for read data processing
		private readonly AutoResetEvent mReadDone = new AutoResetEvent(false);

		// event for status report
		private readonly AutoResetEvent mStatusDone = new AutoResetEvent(false);

		// use a different method to write reports
		private bool mAltWriteMethod;

		// HID device path of this Wiimote
		private string mDevicePath = string.Empty;

		// unique ID
		private readonly Guid mID = Guid.NewGuid();

		// delegate used for enumerating found Wiimotes
		internal delegate bool WiimoteFoundDelegate(string devicePath);

		// kilograms to pounds
		private const float KG2LB = 2.20462262f;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Wiimote()
		{
		}

		internal Wiimote(string devicePath)
		{
			mDevicePath = devicePath;
		}

		/// <summary>
		/// Connect to the first-found Wiimote
		/// </summary>
		/// <exception cref="WiimoteNotFoundException">Wiimote not found in HID device list</exception>
		public void Connect()
		{
			if(string.IsNullOrEmpty(mDevicePath))
				FindWiimote(WiimoteFound);
			else
				OpenWiimoteDeviceHandle(mDevicePath);
		}

		internal static void FindWiimote(WiimoteFoundDelegate wiimoteFound)
		{
			int index = 0;
			bool found = false;
			Guid guid;
			SafeFileHandle mHandle;

			// get the GUID of the HID class
			HIDImports.HidD_GetHidGuid(out guid);

			// get a handle to all devices that are part of the HID class
			// Fun fact:  DIGCF_PRESENT worked on my machine just fine.  I reinstalled Vista, and now it no longer finds the Wiimote with that parameter enabled...
			IntPtr hDevInfo = HIDImports.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, HIDImports.DIGCF_DEVICEINTERFACE);// | HIDImports.DIGCF_PRESENT);

			// create a new interface data struct and initialize its size
			HIDImports.SP_DEVICE_INTERFACE_DATA diData = new HIDImports.SP_DEVICE_INTERFACE_DATA();
			diData.cbSize = Marshal.SizeOf(diData);

			// get a device interface to a single device (enumerate all devices)
			while(HIDImports.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, index, ref diData))
			{
				UInt32 size;

				// get the buffer size for this device detail instance (returned in the size parameter)
				HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

				// create a detail struct and set its size
				HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = new HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA();

				// yeah, yeah...well, see, on Win x86, cbSize must be 5 for some reason.  On x64, apparently 8 is what it wants.
				// someday I should figure this out.  Thanks to Paul Miller on this...
				diDetail.cbSize = (uint)(IntPtr.Size == 8 ? 8 : 5);

				// actually get the detail struct
				if(HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref diDetail, size, out size, IntPtr.Zero))
				{
					Debug.WriteLine(string.Format("{0}: {1} - {2}", index, diDetail.DevicePath, Marshal.GetLastWin32Error()));

					// open a read/write handle to our device using the DevicePath returned
					mHandle = HIDImports.CreateFile(diDetail.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);

					// create an attributes struct and initialize the size
					HIDImports.HIDD_ATTRIBUTES attrib = new HIDImports.HIDD_ATTRIBUTES();
					attrib.Size = Marshal.SizeOf(attrib);

					// get the attributes of the current device
					if(HIDImports.HidD_GetAttributes(mHandle.DangerousGetHandle(), ref attrib))
					{
						// if the vendor and product IDs match up
						if(attrib.VendorID == VID && attrib.ProductID == PID)
						{
							// it's a Wiimote
							Debug.WriteLine("Found one!");
							found = true;

							// fire the callback function...if the callee doesn't care about more Wiimotes, break out
							if(!wiimoteFound(diDetail.DevicePath))
								break;
						}
					}
					mHandle.Close();
				}
				else
				{
					// failed to get the detail struct
					throw new WiimoteException("SetupDiGetDeviceInterfaceDetail failed on index " + index);
				}

				// move to the next device
				index++;
			}

			// clean up our list
			HIDImports.SetupDiDestroyDeviceInfoList(hDevInfo);

			// if we didn't find a Wiimote, throw an exception
			if(!found)
				throw new WiimoteNotFoundException("No Wiimotes found in HID device list.");
		}

		private bool WiimoteFound(string devicePath)
		{
			mDevicePath = devicePath;

			// if we didn't find a Wiimote, throw an exception
			OpenWiimoteDeviceHandle(mDevicePath);

			return false;
		}

		private void OpenWiimoteDeviceHandle(string devicePath)
		{
			// open a read/write handle to our device using the DevicePath returned
			mHandle = HIDImports.CreateFile(devicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);

			// create an attributes struct and initialize the size
			HIDImports.HIDD_ATTRIBUTES attrib = new HIDImports.HIDD_ATTRIBUTES();
			attrib.Size = Marshal.SizeOf(attrib);

			// get the attributes of the current device
			if(HIDImports.HidD_GetAttributes(mHandle.DangerousGetHandle(), ref attrib))
			{
				// if the vendor and product IDs match up
				if(attrib.VendorID == VID && attrib.ProductID == PID)
				{
					// create a nice .NET FileStream wrapping the handle above
					mStream = new FileStream(mHandle, FileAccess.ReadWrite, REPORT_LENGTH, true);

					// start an async read operation on it
					BeginAsyncRead();

					// read the calibration info from the controller
					try
					{
						ReadWiimoteCalibration();
					}
					catch
					{
						// if we fail above, try the alternate HID writes
						mAltWriteMethod = true;
						ReadWiimoteCalibration();
					}

					// force a status check to get the state of any extensions plugged in at startup
					GetStatus();
				}
				else
				{
					// otherwise this isn't the controller, so close up the file handle
					mHandle.Close();				
					throw new WiimoteException("Attempted to open a non-Wiimote device.");
				}
			}
		}

		/// <summary>
		/// Disconnect from the controller and stop reading data from it
		/// </summary>
		public void Disconnect()
		{
			// close up the stream and handle
			if(mStream != null)
				mStream.Close();

			if(mHandle != null)
				mHandle.Close();
		}

		/// <summary>
		/// Start reading asynchronously from the controller
		/// </summary>
        private void BeginAsyncRead()
        {
			// if the stream is valid and ready
			if(mStream != null && mStream.CanRead)
			{
				// setup the read and the callback
				byte[] buff = new byte[REPORT_LENGTH];
				mStream.BeginRead(buff, 0, REPORT_LENGTH, new AsyncCallback(OnReadData), buff);
			}
        }

		/// <summary>
		/// Callback when data is ready to be processed
		/// </summary>
		/// <param name="ar">State information for the callback</param>
        private void OnReadData(IAsyncResult ar)
		{
			// grab the byte buffer
			byte[] buff = (byte[])ar.AsyncState;

			try
			{
				// end the current read
				mStream.EndRead(ar);

				// parse it
				if(ParseInputReport(buff))
				{
					// post an event
					if(WiimoteChanged != null)
						WiimoteChanged(this, new WiimoteChangedEventArgs(mWiimoteState));
				}

				// start reading again
				BeginAsyncRead();
			}
			catch(OperationCanceledException)
			{
				Debug.WriteLine("OperationCanceledException");
			}
		}

		/// <summary>
		/// Parse a report sent by the Wiimote
		/// </summary>
		/// <param name="buff">Data buffer to parse</param>
		/// <returns>Returns a boolean noting whether an event needs to be posted</returns>
		private bool ParseInputReport(byte[] buff)
		{
			InputReport type = (InputReport)buff[0];

			switch(type)
			{
				case InputReport.Buttons:
					ParseButtons(buff);
					break;
				case InputReport.ButtonsAccel:
					ParseButtons(buff);
					ParseAccel(buff);
					break;
				case InputReport.IRAccel:
					ParseButtons(buff);
					ParseAccel(buff);
					ParseIR(buff);
					break;
				case InputReport.ButtonsExtension:
					ParseButtons(buff);
					ParseExtension(buff, 3);
					break;
				case InputReport.ExtensionAccel:
					ParseButtons(buff);
					ParseAccel(buff);
					ParseExtension(buff, 6);
					break;
				case InputReport.IRExtensionAccel:
					ParseButtons(buff);
					ParseAccel(buff);
					ParseIR(buff);
					ParseExtension(buff, 16);
					break;
				case InputReport.Status:
					ParseButtons(buff);
					mWiimoteState.BatteryRaw = buff[6];
					mWiimoteState.Battery = (((100.0f * 48.0f * (float)((int)buff[6] / 48.0f))) / 192.0f);

					// get the real LED values in case the values from SetLEDs() somehow becomes out of sync, which really shouldn't be possible
					mWiimoteState.LEDState.LED1 = (buff[3] & 0x10) != 0;
					mWiimoteState.LEDState.LED2 = (buff[3] & 0x20) != 0;
					mWiimoteState.LEDState.LED3 = (buff[3] & 0x40) != 0;
					mWiimoteState.LEDState.LED4 = (buff[3] & 0x80) != 0;

					// extension connected?
					bool extension = (buff[3] & 0x02) != 0;
					Debug.WriteLine("Extension: " + extension);

					if(mWiimoteState.Extension != extension)
					{
						mWiimoteState.Extension = extension;

						if(extension)
						{
							InitializeExtension();
						}
						else
							mWiimoteState.ExtensionType = ExtensionType.None;

						// only fire the extension changed event if we have a real extension (i.e. not a balance board)
						if(WiimoteExtensionChanged != null && mWiimoteState.ExtensionType != ExtensionType.BalanceBoard)
							WiimoteExtensionChanged(this, new WiimoteExtensionChangedEventArgs(mWiimoteState.ExtensionType, mWiimoteState.Extension));
					}
					mStatusDone.Set();
					break;
				case InputReport.ReadData:
					ParseButtons(buff);
					ParseReadData(buff);
					break;
				default:
					Debug.WriteLine("Unknown report type: " + type.ToString("x"));
					return false;
			}

			return true;
		}

		/// <summary>
		/// Handles setting up an extension when plugged in
		/// </summary>
		private void InitializeExtension()
		{
			WriteData(REGISTER_EXTENSION_INIT, 0x00);

			// start reading again
			BeginAsyncRead();

			// doing a read too quickly after a write seems to kill the guitar controller
			Thread.Sleep(50);

			byte[] buff = ReadData(REGISTER_EXTENSION_TYPE, 2);
			int type = (int)buff[0] << 8 | buff[1];

			switch((ExtensionType)type)
			{
				case ExtensionType.None:
				case ExtensionType.ParitallyInserted:
					mWiimoteState.Extension = false;
					mWiimoteState.ExtensionType = ExtensionType.None;
					return;
				case ExtensionType.Nunchuk:
				case ExtensionType.ClassicController:
				case ExtensionType.Guitar:
				case ExtensionType.BalanceBoard:
					mWiimoteState.ExtensionType = (ExtensionType)type;
					this.SetReportType(InputReport.ButtonsExtension, true);
					break;
				default:
					throw new WiimoteException("Unknown extension controller found: " + type.ToString("x"));
			}

			switch(mWiimoteState.ExtensionType)
			{
				case ExtensionType.Nunchuk:
					buff = DecryptBuffer(ReadData(REGISTER_EXTENSION_CALIBRATION, 16));

					mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0 = buff[0];
					mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0 = buff[1];
					mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0 = buff[2];
					mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.XG = buff[4];
					mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.YG = buff[5];
					mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.ZG = buff[6];
					mWiimoteState.NunchukState.CalibrationInfo.MaxX = buff[8];
					mWiimoteState.NunchukState.CalibrationInfo.MinX = buff[9];
					mWiimoteState.NunchukState.CalibrationInfo.MidX = buff[10];
					mWiimoteState.NunchukState.CalibrationInfo.MaxY = buff[11];
					mWiimoteState.NunchukState.CalibrationInfo.MinY = buff[12];
					mWiimoteState.NunchukState.CalibrationInfo.MidY = buff[13];
					break;
				case ExtensionType.ClassicController:
					buff = DecryptBuffer(ReadData(REGISTER_EXTENSION_CALIBRATION, 16));

					mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL = (byte)(buff[0] >> 2);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MinXL = (byte)(buff[1] >> 2);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MidXL = (byte)(buff[2] >> 2);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL = (byte)(buff[3] >> 2);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MinYL = (byte)(buff[4] >> 2);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MidYL = (byte)(buff[5] >> 2);

					mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR = (byte)(buff[6] >> 3);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MinXR = (byte)(buff[7] >> 3);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MidXR = (byte)(buff[8] >> 3);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR = (byte)(buff[9] >> 3);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MinYR = (byte)(buff[10] >> 3);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MidYR = (byte)(buff[11] >> 3);

					// this doesn't seem right...
//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MinTriggerL = (byte)(buff[12] >> 3);
//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MaxTriggerL = (byte)(buff[14] >> 3);
//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MinTriggerR = (byte)(buff[13] >> 3);
//					mWiimoteState.ClassicControllerState.AccelCalibrationInfo.MaxTriggerR = (byte)(buff[15] >> 3);
					mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerL = 0;
					mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL = 31;
					mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerR = 0;
					mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR = 31;
					break;
				case ExtensionType.Guitar:
					// there appears to be no calibration data returned by the guitar controller
					break;
				case ExtensionType.BalanceBoard:
					buff = ReadData(REGISTER_EXTENSION_CALIBRATION, 32);

					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopRight =		(short)((short)buff[4] << 8 | buff[5]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomRight =	(short)((short)buff[6] << 8 | buff[7]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopLeft =		(short)((short)buff[8] << 8 | buff[9]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomLeft =	(short)((short)buff[10] << 8 | buff[11]);

					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopRight =		(short)((short)buff[12] << 8 | buff[13]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomRight =	(short)((short)buff[14] << 8 | buff[15]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopLeft =		(short)((short)buff[16] << 8 | buff[17]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomLeft =	(short)((short)buff[18] << 8 | buff[19]);

					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopRight =		(short)((short)buff[20] << 8 | buff[21]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomRight =	(short)((short)buff[22] << 8 | buff[23]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopLeft =		(short)((short)buff[24] << 8 | buff[25]);
					mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomLeft =	(short)((short)buff[26] << 8 | buff[27]);
					break;
			}
		}

		/// <summary>
		/// Decrypts data sent from the extension to the Wiimote
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/// <returns>Byte array containing decoded data</returns>
		private byte[] DecryptBuffer(byte[] buff)
		{
			for(int i = 0; i < buff.Length; i++)
				buff[i] = (byte)(((buff[i] ^ 0x17) + 0x17) & 0xff);

			return buff;
		}

		/// <summary>
		/// Parses a standard button report into the ButtonState struct
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseButtons(byte[] buff)
		{
			mWiimoteState.ButtonState.A		= (buff[2] & 0x08) != 0;
			mWiimoteState.ButtonState.B		= (buff[2] & 0x04) != 0;
			mWiimoteState.ButtonState.Minus	= (buff[2] & 0x10) != 0;
			mWiimoteState.ButtonState.Home	= (buff[2] & 0x80) != 0;
			mWiimoteState.ButtonState.Plus	= (buff[1] & 0x10) != 0;
			mWiimoteState.ButtonState.One	= (buff[2] & 0x02) != 0;
			mWiimoteState.ButtonState.Two	= (buff[2] & 0x01) != 0;
			mWiimoteState.ButtonState.Up	= (buff[1] & 0x08) != 0;
			mWiimoteState.ButtonState.Down	= (buff[1] & 0x04) != 0;
			mWiimoteState.ButtonState.Left	= (buff[1] & 0x01) != 0;
			mWiimoteState.ButtonState.Right	= (buff[1] & 0x02) != 0;
		}

		/// <summary>
		/// Parse accelerometer data
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseAccel(byte[] buff)
		{
			mWiimoteState.AccelState.RawValues.X = buff[3];
			mWiimoteState.AccelState.RawValues.Y = buff[4];
			mWiimoteState.AccelState.RawValues.Z = buff[5];

			mWiimoteState.AccelState.Values.X = (float)((float)mWiimoteState.AccelState.RawValues.X - mWiimoteState.AccelCalibrationInfo.X0) / 
											((float)mWiimoteState.AccelCalibrationInfo.XG - mWiimoteState.AccelCalibrationInfo.X0);
			mWiimoteState.AccelState.Values.Y = (float)((float)mWiimoteState.AccelState.RawValues.Y - mWiimoteState.AccelCalibrationInfo.Y0) /
											((float)mWiimoteState.AccelCalibrationInfo.YG - mWiimoteState.AccelCalibrationInfo.Y0);
			mWiimoteState.AccelState.Values.Z = (float)((float)mWiimoteState.AccelState.RawValues.Z - mWiimoteState.AccelCalibrationInfo.Z0) /
											((float)mWiimoteState.AccelCalibrationInfo.ZG - mWiimoteState.AccelCalibrationInfo.Z0);
		}

		/// <summary>
		/// Parse IR data from report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseIR(byte[] buff)
		{
			mWiimoteState.IRState.IRSensors[0].RawPosition.X = buff[6] | ((buff[8] >> 4) & 0x03) << 8;
			mWiimoteState.IRState.IRSensors[0].RawPosition.Y = buff[7] | ((buff[8] >> 6) & 0x03) << 8;

			switch(mWiimoteState.IRState.Mode)
			{
				case IRMode.Basic:
					mWiimoteState.IRState.IRSensors[1].RawPosition.X = buff[9]  | ((buff[8] >> 0) & 0x03) << 8;
					mWiimoteState.IRState.IRSensors[1].RawPosition.Y = buff[10] | ((buff[8] >> 2) & 0x03) << 8;

					mWiimoteState.IRState.IRSensors[0].Size = 0x00;
					mWiimoteState.IRState.IRSensors[1].Size = 0x00;

					mWiimoteState.IRState.IRSensors[0].Found = !(buff[6] == 0xff && buff[7] == 0xff);
					mWiimoteState.IRState.IRSensors[1].Found = !(buff[9] == 0xff && buff[10] == 0xff);
					break;
				case IRMode.Extended:
					mWiimoteState.IRState.IRSensors[1].RawPosition.X = buff[9]  | ((buff[11] >> 4) & 0x03) << 8;
					mWiimoteState.IRState.IRSensors[1].RawPosition.Y = buff[10] | ((buff[11] >> 6) & 0x03) << 8;
					mWiimoteState.IRState.IRSensors[2].RawPosition.X = buff[12] | ((buff[14] >> 4) & 0x03) << 8;
					mWiimoteState.IRState.IRSensors[2].RawPosition.Y = buff[13] | ((buff[14] >> 6) & 0x03) << 8;
					mWiimoteState.IRState.IRSensors[3].RawPosition.X = buff[15] | ((buff[17] >> 4) & 0x03) << 8;
					mWiimoteState.IRState.IRSensors[3].RawPosition.Y = buff[16] | ((buff[17] >> 6) & 0x03) << 8;

					mWiimoteState.IRState.IRSensors[0].Size = buff[8] & 0x0f;
					mWiimoteState.IRState.IRSensors[1].Size = buff[11] & 0x0f;
					mWiimoteState.IRState.IRSensors[2].Size = buff[14] & 0x0f;
					mWiimoteState.IRState.IRSensors[3].Size = buff[17] & 0x0f;

					mWiimoteState.IRState.IRSensors[0].Found = !(buff[6] == 0xff && buff[7] == 0xff && buff[8] == 0xff);
					mWiimoteState.IRState.IRSensors[1].Found = !(buff[9] == 0xff && buff[10] == 0xff && buff[11] == 0xff);
					mWiimoteState.IRState.IRSensors[2].Found = !(buff[12] == 0xff && buff[13] == 0xff && buff[14] == 0xff);
					mWiimoteState.IRState.IRSensors[3].Found = !(buff[15] == 0xff && buff[16] == 0xff && buff[17] == 0xff);
					break;
			}

			mWiimoteState.IRState.IRSensors[0].Position.X = (float)(mWiimoteState.IRState.IRSensors[0].RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensors[1].Position.X = (float)(mWiimoteState.IRState.IRSensors[1].RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensors[2].Position.X = (float)(mWiimoteState.IRState.IRSensors[2].RawPosition.X / 1023.5f);
			mWiimoteState.IRState.IRSensors[3].Position.X = (float)(mWiimoteState.IRState.IRSensors[3].RawPosition.X / 1023.5f);

			mWiimoteState.IRState.IRSensors[0].Position.Y = (float)(mWiimoteState.IRState.IRSensors[0].RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensors[1].Position.Y = (float)(mWiimoteState.IRState.IRSensors[1].RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensors[2].Position.Y = (float)(mWiimoteState.IRState.IRSensors[2].RawPosition.Y / 767.5f);
			mWiimoteState.IRState.IRSensors[3].Position.Y = (float)(mWiimoteState.IRState.IRSensors[3].RawPosition.Y / 767.5f);

			if(mWiimoteState.IRState.IRSensors[0].Found && mWiimoteState.IRState.IRSensors[1].Found)
			{
				mWiimoteState.IRState.RawMidpoint.X = (mWiimoteState.IRState.IRSensors[1].RawPosition.X + mWiimoteState.IRState.IRSensors[0].RawPosition.X) / 2;
				mWiimoteState.IRState.RawMidpoint.Y = (mWiimoteState.IRState.IRSensors[1].RawPosition.Y + mWiimoteState.IRState.IRSensors[0].RawPosition.Y) / 2;
		
				mWiimoteState.IRState.Midpoint.X = (mWiimoteState.IRState.IRSensors[1].Position.X + mWiimoteState.IRState.IRSensors[0].Position.X) / 2.0f;
				mWiimoteState.IRState.Midpoint.Y = (mWiimoteState.IRState.IRSensors[1].Position.Y + mWiimoteState.IRState.IRSensors[0].Position.Y) / 2.0f;
			}
			else
				mWiimoteState.IRState.Midpoint.X = mWiimoteState.IRState.Midpoint.Y = 0.0f;
		}

		/// <summary>
		/// Parse data from an extension controller
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/// <param name="offset">Offset into data buffer</param>
		private void ParseExtension(byte[] buff, int offset)
		{
			switch(mWiimoteState.ExtensionType)
			{
				case ExtensionType.Nunchuk:
					buff = DecryptBuffer(buff);
					mWiimoteState.NunchukState.RawJoystick.X = buff[offset];
					mWiimoteState.NunchukState.RawJoystick.Y = buff[offset + 1];
					mWiimoteState.NunchukState.AccelState.RawValues.X = buff[offset + 2];
					mWiimoteState.NunchukState.AccelState.RawValues.Y = buff[offset + 3];
					mWiimoteState.NunchukState.AccelState.RawValues.Z = buff[offset + 4];

					mWiimoteState.NunchukState.C = (buff[offset + 5] & 0x02) == 0;
					mWiimoteState.NunchukState.Z = (buff[offset + 5] & 0x01) == 0;

					mWiimoteState.NunchukState.AccelState.Values.X = (float)((float)mWiimoteState.NunchukState.AccelState.RawValues.X - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0) / 
													((float)mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.XG - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0);
					mWiimoteState.NunchukState.AccelState.Values.Y = (float)((float)mWiimoteState.NunchukState.AccelState.RawValues.Y - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0) /
													((float)mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.YG - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0);
					mWiimoteState.NunchukState.AccelState.Values.Z = (float)((float)mWiimoteState.NunchukState.AccelState.RawValues.Z - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0) /
													((float)mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.ZG - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0);

					if(mWiimoteState.NunchukState.CalibrationInfo.MaxX != 0x00)
						mWiimoteState.NunchukState.Joystick.X = (float)((float)mWiimoteState.NunchukState.RawJoystick.X - mWiimoteState.NunchukState.CalibrationInfo.MidX) / 
												((float)mWiimoteState.NunchukState.CalibrationInfo.MaxX - mWiimoteState.NunchukState.CalibrationInfo.MinX);

					if(mWiimoteState.NunchukState.CalibrationInfo.MaxY != 0x00)
						mWiimoteState.NunchukState.Joystick.Y = (float)((float)mWiimoteState.NunchukState.RawJoystick.Y - mWiimoteState.NunchukState.CalibrationInfo.MidY) / 
												((float)mWiimoteState.NunchukState.CalibrationInfo.MaxY - mWiimoteState.NunchukState.CalibrationInfo.MinY);

					break;

				case ExtensionType.ClassicController:
					buff = DecryptBuffer(buff);
					mWiimoteState.ClassicControllerState.RawJoystickL.X = (byte)(buff[offset] & 0x3f);
					mWiimoteState.ClassicControllerState.RawJoystickL.Y = (byte)(buff[offset + 1] & 0x3f);
					mWiimoteState.ClassicControllerState.RawJoystickR.X = (byte)((buff[offset + 2] >> 7) | (buff[offset + 1] & 0xc0) >> 5 | (buff[offset] & 0xc0) >> 3);
					mWiimoteState.ClassicControllerState.RawJoystickR.Y = (byte)(buff[offset + 2] & 0x1f);

					mWiimoteState.ClassicControllerState.RawTriggerL = (byte)(((buff[offset + 2] & 0x60) >> 2) | (buff[offset + 3] >> 5));
					mWiimoteState.ClassicControllerState.RawTriggerR = (byte)(buff[offset + 3] & 0x1f);

					mWiimoteState.ClassicControllerState.ButtonState.TriggerR	= (buff[offset + 4] & 0x02) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Plus		= (buff[offset + 4] & 0x04) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Home		= (buff[offset + 4] & 0x08) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Minus		= (buff[offset + 4] & 0x10) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.TriggerL	= (buff[offset + 4] & 0x20) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Down		= (buff[offset + 4] & 0x40) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Right		= (buff[offset + 4] & 0x80) == 0;

					mWiimoteState.ClassicControllerState.ButtonState.Up			= (buff[offset + 5] & 0x01) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Left		= (buff[offset + 5] & 0x02) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.ZR			= (buff[offset + 5] & 0x04) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.X			= (buff[offset + 5] & 0x08) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.A			= (buff[offset + 5] & 0x10) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.Y			= (buff[offset + 5] & 0x20) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.B			= (buff[offset + 5] & 0x40) == 0;
					mWiimoteState.ClassicControllerState.ButtonState.ZL			= (buff[offset + 5] & 0x80) == 0;

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL != 0x00)
						mWiimoteState.ClassicControllerState.JoystickL.X = (float)((float)mWiimoteState.ClassicControllerState.RawJoystickL.X - mWiimoteState.ClassicControllerState.CalibrationInfo.MidXL) / 
						(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL - mWiimoteState.ClassicControllerState.CalibrationInfo.MinXL);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL != 0x00)
						mWiimoteState.ClassicControllerState.JoystickL.Y = (float)((float)mWiimoteState.ClassicControllerState.RawJoystickL.Y - mWiimoteState.ClassicControllerState.CalibrationInfo.MidYL) / 
						(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL - mWiimoteState.ClassicControllerState.CalibrationInfo.MinYL);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR != 0x00)
						mWiimoteState.ClassicControllerState.JoystickR.X = (float)((float)mWiimoteState.ClassicControllerState.RawJoystickR.X - mWiimoteState.ClassicControllerState.CalibrationInfo.MidXR) / 
						(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR - mWiimoteState.ClassicControllerState.CalibrationInfo.MinXR);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR != 0x00)
						mWiimoteState.ClassicControllerState.JoystickR.Y = (float)((float)mWiimoteState.ClassicControllerState.RawJoystickR.Y - mWiimoteState.ClassicControllerState.CalibrationInfo.MidYR) / 
						(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR - mWiimoteState.ClassicControllerState.CalibrationInfo.MinYR);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL != 0x00)
						mWiimoteState.ClassicControllerState.TriggerL = (mWiimoteState.ClassicControllerState.RawTriggerL) / 
						(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL - mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerL);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR != 0x00)
						mWiimoteState.ClassicControllerState.TriggerR = (mWiimoteState.ClassicControllerState.RawTriggerR) / 
						(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR - mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerR);
					break;

				case ExtensionType.Guitar:
					buff = DecryptBuffer(buff);
				    mWiimoteState.GuitarState.ButtonState.Plus		= (buff[offset + 4] & 0x04) == 0;
				    mWiimoteState.GuitarState.ButtonState.Minus		= (buff[offset + 4] & 0x10) == 0;
				    mWiimoteState.GuitarState.ButtonState.StrumDown	= (buff[offset + 4] & 0x40) == 0;

				    mWiimoteState.GuitarState.ButtonState.StrumUp	= (buff[offset + 5] & 0x01) == 0;
				    mWiimoteState.GuitarState.ButtonState.Yellow	= (buff[offset + 5] & 0x08) == 0;
				    mWiimoteState.GuitarState.ButtonState.Green		= (buff[offset + 5] & 0x10) == 0;
				    mWiimoteState.GuitarState.ButtonState.Blue		= (buff[offset + 5] & 0x20) == 0;
				    mWiimoteState.GuitarState.ButtonState.Red		= (buff[offset + 5] & 0x40) == 0;
				    mWiimoteState.GuitarState.ButtonState.Orange	= (buff[offset + 5] & 0x80) == 0;

					// it appears the joystick values are only 6 bits
					mWiimoteState.GuitarState.RawJoystick.X			= (buff[offset + 0] & 0x3f);
					mWiimoteState.GuitarState.RawJoystick.Y			= (buff[offset + 1] & 0x3f);

					// and the whammy bar is only 4 bits
					mWiimoteState.GuitarState.RawWhammyBar			= (byte)(buff[offset + 3] & 0x0f);

					mWiimoteState.GuitarState.Joystick.X			= (float)(mWiimoteState.GuitarState.RawJoystick.X - 0x1f) / 0x3f;	// not fully accurate, but close
					mWiimoteState.GuitarState.Joystick.Y			= (float)(mWiimoteState.GuitarState.RawJoystick.Y - 0x1f) / 0x3f;	// not fully accurate, but close
					mWiimoteState.GuitarState.WhammyBar				= (float)(mWiimoteState.GuitarState.RawWhammyBar) / 0x0a;	// seems like there are 10 positions?
				    break;

				case ExtensionType.BalanceBoard:
					mWiimoteState.BalanceBoardState.SensorValuesRaw.TopRight =		(short)((short)buff[offset + 0] << 8 | buff[offset + 1]);
					mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomRight =	(short)((short)buff[offset + 2] << 8 | buff[offset + 3]);
					mWiimoteState.BalanceBoardState.SensorValuesRaw.TopLeft =		(short)((short)buff[offset + 4] << 8 | buff[offset + 5]);
					mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomLeft =	(short)((short)buff[offset + 6] << 8 | buff[offset + 7]);

					mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoardState.SensorValuesRaw.TopLeft, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopLeft, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopLeft, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopLeft);
					mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoardState.SensorValuesRaw.TopRight, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.TopRight, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.TopRight, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.TopRight);
					mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomLeft, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomLeft, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomLeft, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomLeft);
					mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight = GetBalanceBoardSensorValue(mWiimoteState.BalanceBoardState.SensorValuesRaw.BottomRight, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg0.BottomRight, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg17.BottomRight, mWiimoteState.BalanceBoardState.CalibrationInfo.Kg34.BottomRight);

					mWiimoteState.BalanceBoardState.SensorValuesLb.TopLeft = (mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft * KG2LB);
					mWiimoteState.BalanceBoardState.SensorValuesLb.TopRight = (mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight * KG2LB);
					mWiimoteState.BalanceBoardState.SensorValuesLb.BottomLeft = (mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft * KG2LB);
					mWiimoteState.BalanceBoardState.SensorValuesLb.BottomRight = (mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight * KG2LB);

					mWiimoteState.BalanceBoardState.WeightKg = (mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft + mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight + mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft + mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight) / 4.0f;
					mWiimoteState.BalanceBoardState.WeightLb = (mWiimoteState.BalanceBoardState.SensorValuesLb.TopLeft + mWiimoteState.BalanceBoardState.SensorValuesLb.TopRight + mWiimoteState.BalanceBoardState.SensorValuesLb.BottomLeft + mWiimoteState.BalanceBoardState.SensorValuesLb.BottomRight) / 4.0f;

					float Kx = (mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft + mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft) / (mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight + mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight);
					float Ky = (mWiimoteState.BalanceBoardState.SensorValuesKg.TopLeft + mWiimoteState.BalanceBoardState.SensorValuesKg.TopRight) / (mWiimoteState.BalanceBoardState.SensorValuesKg.BottomLeft + mWiimoteState.BalanceBoardState.SensorValuesKg.BottomRight);

					mWiimoteState.BalanceBoardState.CenterOfGravity.X = ((float)(Kx - 1) / (float)(Kx + 1)) * (float)(-BSL / 2);
					mWiimoteState.BalanceBoardState.CenterOfGravity.Y = ((float)(Ky - 1) / (float)(Ky + 1)) * (float)(-BSW / 2);
					break;
			}
		}

		private float GetBalanceBoardSensorValue(short sensor, short min, short mid, short max)
		{
			if(max == mid || mid == min)
				return 0;

			if(sensor < mid)
				return 68.0f * ((float)(sensor - min) / (mid - min));
			else
				return 68.0f * ((float)(sensor - mid) / (max - mid)) + 68.0f;
		}


		/// <summary>
		/// Parse data returned from a read report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseReadData(byte[] buff)
		{
			if((buff[3] & 0x08) != 0)
				throw new WiimoteException("Error reading data from Wiimote: Bytes do not exist.");

			if((buff[3] & 0x07) != 0)
				throw new WiimoteException("Error reading data from Wiimote: Attempt to read from write-only registers.");

			// get our size and offset from the report
			int size = (buff[3] >> 4) + 1;
			int offset = (buff[4] << 8 | buff[5]);

			// add it to the buffer
			Array.Copy(buff, 6, mReadBuff, offset - mAddress, size);

			// if we've read it all, set the event
			if(mAddress + mSize == offset + size)
				mReadDone.Set();
		}

		/// <summary>
		/// Returns whether rumble is currently enabled.
		/// </summary>
		/// <returns>Byte indicating true (0x01) or false (0x00)</returns>
		private byte GetRumbleBit()
		{
			return (byte)(mWiimoteState.Rumble ? 0x01 : 0x00);
		}

		/// <summary>
		/// Read calibration information stored on Wiimote
		/// </summary>
		private void ReadWiimoteCalibration()
		{
			// this appears to change the report type to 0x31
			byte[] buff = ReadData(0x0016, 7);

			mWiimoteState.AccelCalibrationInfo.X0 = buff[0];
			mWiimoteState.AccelCalibrationInfo.Y0 = buff[1];
			mWiimoteState.AccelCalibrationInfo.Z0 = buff[2];
			mWiimoteState.AccelCalibrationInfo.XG = buff[4];
			mWiimoteState.AccelCalibrationInfo.YG = buff[5];
			mWiimoteState.AccelCalibrationInfo.ZG = buff[6];
		}

		/// <summary>
		/// Set Wiimote reporting mode (if using an IR report type, IR sensitivity is set to WiiLevel3)
		/// </summary>
		/// <param name="type">Report type</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(InputReport type, bool continuous)
		{
			SetReportType(type, IRSensitivity.Maximum, continuous);
		}

		/// <summary>
		/// Set Wiimote reporting mode
		/// </summary>
		/// <param name="type">Report type</param>
		/// <param name="irSensitivity">IR sensitivity</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(InputReport type, IRSensitivity irSensitivity, bool continuous)
		{
			// only 1 report type allowed for the BB
			if(mWiimoteState.ExtensionType == ExtensionType.BalanceBoard)
				type = InputReport.ButtonsExtension;

			switch(type)
			{
				case InputReport.IRAccel:
					EnableIR(IRMode.Extended, irSensitivity);
					break;
				case InputReport.IRExtensionAccel:
					EnableIR(IRMode.Basic, irSensitivity);
					break;
				default:
					DisableIR();
					break;
			}

			ClearReport();
			mBuff[0] = (byte)OutputReport.Type;
			mBuff[1] = (byte)((continuous ? 0x04 : 0x00) | (byte)(mWiimoteState.Rumble ? 0x01 : 0x00));
			mBuff[2] = (byte)type;

			WriteReport();
		}

		/// <summary>
		/// Set the LEDs on the Wiimote
		/// </summary>
		/// <param name="led1">LED 1</param>
		/// <param name="led2">LED 2</param>
		/// <param name="led3">LED 3</param>
		/// <param name="led4">LED 4</param>
		public void SetLEDs(bool led1, bool led2, bool led3, bool led4)
		{
			mWiimoteState.LEDState.LED1 = led1;
			mWiimoteState.LEDState.LED2 = led2;
			mWiimoteState.LEDState.LED3 = led3;
			mWiimoteState.LEDState.LED4 = led4;

			ClearReport();

			mBuff[0] = (byte)OutputReport.LEDs;
			mBuff[1] =	(byte)(
						(led1 ? 0x10 : 0x00) |
						(led2 ? 0x20 : 0x00) |
						(led3 ? 0x40 : 0x00) |
						(led4 ? 0x80 : 0x00) |
						GetRumbleBit());

			WriteReport();
		}

		/// <summary>
		/// Set the LEDs on the Wiimote
		/// </summary>
		/// <param name="leds">The value to be lit up in base2 on the Wiimote</param>
		public void SetLEDs(int leds)
		{
			mWiimoteState.LEDState.LED1 = (leds & 0x01) > 0;
			mWiimoteState.LEDState.LED2 = (leds & 0x02) > 0;
			mWiimoteState.LEDState.LED3 = (leds & 0x04) > 0;
			mWiimoteState.LEDState.LED4 = (leds & 0x08) > 0;

			ClearReport();

			mBuff[0] = (byte)OutputReport.LEDs;
			mBuff[1] =	(byte)(
						((leds & 0x01) > 0 ? 0x10 : 0x00) |
						((leds & 0x02) > 0 ? 0x20 : 0x00) |
						((leds & 0x04) > 0 ? 0x40 : 0x00) |
						((leds & 0x08) > 0 ? 0x80 : 0x00) |
						GetRumbleBit());

			WriteReport();
		}

		/// <summary>
		/// Toggle rumble
		/// </summary>
		/// <param name="on">On or off</param>
		public void SetRumble(bool on)
		{
			mWiimoteState.Rumble = on;

			// the LED report also handles rumble
			SetLEDs(mWiimoteState.LEDState.LED1, 
					mWiimoteState.LEDState.LED2,
					mWiimoteState.LEDState.LED3,
					mWiimoteState.LEDState.LED4);
		}

		/// <summary>
		/// Retrieve the current status of the Wiimote and extensions.  Replaces GetBatteryLevel() since it was poorly named.
		/// </summary>
		public void GetStatus()
		{
			ClearReport();

			mBuff[0] = (byte)OutputReport.Status;
			mBuff[1] = GetRumbleBit();

			WriteReport();

			// signal the status report finished
			if(!mStatusDone.WaitOne(3000, false))
				throw new WiimoteException("Timed out waiting for status report");
		}

		/// <summary>
		/// Turn on the IR sensor
		/// </summary>
		/// <param name="mode">The data report mode</param>
		/// <param name="irSensitivity">IR sensitivity</param>
		private void EnableIR(IRMode mode, IRSensitivity irSensitivity)
		{
			mWiimoteState.IRState.Mode = mode;

			ClearReport();
			mBuff[0] = (byte)OutputReport.IR;
			mBuff[1] = (byte)(0x04 | GetRumbleBit());
			WriteReport();

			ClearReport();
			mBuff[0] = (byte)OutputReport.IR2;
			mBuff[1] = (byte)(0x04 | GetRumbleBit());
			WriteReport();

			WriteData(REGISTER_IR, 0x08);
			switch(irSensitivity)
			{
				case IRSensitivity.WiiLevel1:
					WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xfe});
					WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0xfd, 0x05});
					break;
				case IRSensitivity.WiiLevel2:
					WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xb4});
					WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0xb3, 0x04});
					break;
				case IRSensitivity.WiiLevel3:
					WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64});
					WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0x63, 0x03});
					break;
				case IRSensitivity.WiiLevel4:
					WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36});
					WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0x35, 0x03});
					break;
				case IRSensitivity.WiiLevel5:
					WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20});
					WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0x1, 0x03});
					break;
				case IRSensitivity.Maximum:
					WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x90, 0x00, 0x41});
					WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0x40, 0x00});
					break;
				default:
					throw new ArgumentOutOfRangeException("irSensitivity");
			}
			WriteData(REGISTER_IR_MODE, (byte)mode);
			WriteData(REGISTER_IR, 0x08);
		}

		/// <summary>
		/// Disable the IR sensor
		/// </summary>
		private void DisableIR()
		{
			mWiimoteState.IRState.Mode = IRMode.Off;

			ClearReport();
			mBuff[0] = (byte)OutputReport.IR;
			mBuff[1] = GetRumbleBit();
			WriteReport();

			ClearReport();
			mBuff[0] = (byte)OutputReport.IR2;
			mBuff[1] = GetRumbleBit();
			WriteReport();
		}

		/// <summary>
		/// Initialize the report data buffer
		/// </summary>
		private void ClearReport()
		{
			Array.Clear(mBuff, 0, REPORT_LENGTH);
		}

		/// <summary>
		/// Write a report to the Wiimote
		/// </summary>
		private void WriteReport()
		{
			if(mAltWriteMethod)
				HIDImports.HidD_SetOutputReport(this.mHandle.DangerousGetHandle(), mBuff, (uint)mBuff.Length);
			else if(mStream != null)
				mStream.Write(mBuff, 0, REPORT_LENGTH);

			Thread.Sleep(50);
		}

		/// <summary>
		/// Read data or register from Wiimote
		/// </summary>
		/// <param name="address">Address to read</param>
		/// <param name="size">Length to read</param>
		/// <returns>Data buffer</returns>
		public byte[] ReadData(int address, short size)
		{
			ClearReport();

			mReadBuff = new byte[size];
			mAddress = address & 0xffff;
			mSize = size;

			mBuff[0] = (byte)OutputReport.ReadMemory;
			mBuff[1] = (byte)(((address & 0xff000000) >> 24) | GetRumbleBit());
			mBuff[2] = (byte)((address & 0x00ff0000)  >> 16);
			mBuff[3] = (byte)((address & 0x0000ff00)  >>  8);
			mBuff[4] = (byte)(address & 0x000000ff);

			mBuff[5] = (byte)((size & 0xff00) >> 8);
			mBuff[6] = (byte)(size & 0xff);

			WriteReport();

			if(!mReadDone.WaitOne(1000, false))
				throw new WiimoteException("Error reading data from Wiimote...is it connected?");

			return mReadBuff;
		}

		/// <summary>
		/// Write a single byte to the Wiimote
		/// </summary>
		/// <param name="address">Address to write</param>
		/// <param name="data">Byte to write</param>
		public void WriteData(int address, byte data)
		{
			WriteData(address, 1, new byte[] { data });
		}

		/// <summary>
		/// Write a byte array to a specified address
		/// </summary>
		/// <param name="address">Address to write</param>
		/// <param name="size">Length of buffer</param>
		/// <param name="buff">Data buffer</param>
		
		public void WriteData(int address, byte size, byte[] buff)
		{
			ClearReport();

			mBuff[0] = (byte)OutputReport.WriteMemory;
			mBuff[1] = (byte)(((address & 0xff000000) >> 24) | GetRumbleBit());
			mBuff[2] = (byte)((address & 0x00ff0000)  >> 16);
			mBuff[3] = (byte)((address & 0x0000ff00)  >>  8);
			mBuff[4] = (byte)(address & 0x000000ff);
			mBuff[5] = size;
			Array.Copy(buff, 0, mBuff, 6, size);

			WriteReport();
		}

		/// <summary>
		/// Current Wiimote state
		/// </summary>
		public WiimoteState WiimoteState
		{
			get { return mWiimoteState; }
		}

		///<summary>
		/// Unique identifier for this Wiimote (not persisted across application instances)
		///</summary>
		public Guid ID
		{
			get { return mID; }
		}

		/// <summary>
		/// HID device path for this Wiimote (valid until Wiimote is disconnected)
		/// </summary>
		public string HIDDevicePath
		{
			get { return mDevicePath; }
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose Wiimote
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose wiimote
		/// </summary>
		/// <param name="disposing">Disposing?</param>
		protected virtual void Dispose(bool disposing)
		{
			// close up our handles
			if(disposing)
				Disconnect();
		}
		#endregion
	}

	/// <summary>
	/// Thrown when no Wiimotes are found in the HID device list
	/// </summary>
	[Serializable]
	public class WiimoteNotFoundException : ApplicationException
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public WiimoteNotFoundException()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		public WiimoteNotFoundException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		public WiimoteNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Streaming context</param>
		protected WiimoteNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	/// <summary>
	/// Represents errors that occur during the execution of the Wiimote library
	/// </summary>
	[Serializable]
	public class WiimoteException : ApplicationException
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public WiimoteException()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		public WiimoteException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		public WiimoteException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Streaming context</param>
		protected WiimoteException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}