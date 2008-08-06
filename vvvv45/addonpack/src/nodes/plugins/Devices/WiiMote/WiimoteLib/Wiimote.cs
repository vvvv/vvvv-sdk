//////////////////////////////////////////////////////////////////////////////////
//	Wiimote.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Collections.Generic;

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
		public event WiimoteChangedEventHandler WiimoteChanged;

		/// <summary>
		/// Event raised when an extension is inserted or removed
		/// </summary>
		public event WiimoteExtensionChangedEventHandler WiimoteExtensionChanged;

		// VID = Nintendo, PID = Wiimote
		private const int VID = 0x057e;
		private const int PID = 0x0306;

		// sure, we could find this out the hard way using HID, but trust me, it's 22
		private const int REPORT_LENGTH = 22;

		/// <summary>
		/// The report format in which the Wiimote should return data
		/// </summary>
		public enum InputReport : byte
		{
			/// <summary>
			/// Status report
			/// </summary>
			Status				= 0x20,
			/// <summary>
			/// Read data from memory location
			/// </summary>
			ReadData			= 0x21,
			/// <summary>
			/// Button data only
			/// </summary>
			Buttons				= 0x30,
			/// <summary>
			/// Button and accelerometer data
			/// </summary>
			ButtonsAccel		= 0x31,
			/// <summary>
			/// IR sensor and accelerometer data
			/// </summary>
			IRAccel				= 0x33,
			/// <summary>
			/// Button and extension controller data
			/// </summary>
			ButtonsExtension	= 0x34,
			/// <summary>
			/// Extension and accelerometer data
			/// </summary>
			ExtensionAccel		= 0x35,
			/// <summary>
			/// IR sensor, extension controller and accelerometer data
			/// </summary>
			IRExtensionAccel	= 0x37,
		};

		// Wiimote output commands
		private enum OutputReport : byte
		{
			None			= 0x00,
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

		// read/write handle to the device
		private SafeFileHandle mHandle;

		// a pretty .NET stream to read/write from/to
		private FileStream mStream;

		// report buffer
		private readonly byte[] mBuff = new byte[REPORT_LENGTH];

		// read data buffer
		private byte[] mReadBuff;

		// current state of controller
		private readonly WiimoteState mWiimoteState = new WiimoteState();

		// event for read data processing
		private readonly AutoResetEvent mReadDone = new AutoResetEvent(false);

		// use a different method to write reports
		private bool mAltWriteMethod;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Wiimote()
		{
		}

		/// <summary>
		/// Connect to a Wiimote paired to the PC via Bluetooth
		/// </summary>
		public void Connect()
		{
			int index = 0;
			bool found = false;
			Guid guid;

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
				if (IntPtr.Size == 8)
					diDetail.cbSize = 8;
				else
					diDetail.cbSize = 5;

				// actually get the detail struct
				if(HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref diDetail, size, out size, IntPtr.Zero))
				{
					Debug.WriteLine(index + " " + diDetail.DevicePath + " " + Marshal.GetLastWin32Error());

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
							Debug.WriteLine("Found it!");
							found = true;

							// create a nice .NET FileStream wrapping the handle above
							mStream = new FileStream(mHandle, FileAccess.ReadWrite, REPORT_LENGTH, true);

							// start an async read operation on it
							BeginAsyncRead();

							// read the calibration info from the controller
							try
							{
								ReadCalibration();
							}
							catch
							{
								// if we fail above, try the alternate HID writes
								mAltWriteMethod = true;
								ReadCalibration();
							}

							// force a status check to get the state of any extensions plugged in at startup
							GetStatus();

							break;
						}
						else
						{
							// otherwise this isn't the controller, so close up the file handle
							mHandle.Close();
						}
					}
				}
				else
				{
					// failed to get the detail struct
					throw new Exception("SetupDiGetDeviceInterfaceDetail failed on index " + index);
				}

				// move to the next device
				index++;
			}

			// clean up our list
			HIDImports.SetupDiDestroyDeviceInfoList(hDevInfo);

			// if we didn't find a Wiimote, throw an exception
			if(!found)
				throw new Exception("Wiimote not found in HID device list.");
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
			if(mStream.CanRead)
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
				if(ParseInput(buff))
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
		private bool ParseInput(byte[] buff)
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
					ParseExtension(DecryptBuffer(buff), 4);
					break;
				case InputReport.ExtensionAccel:
					ParseButtons(buff);
					ParseAccel(buff);
					ParseExtension(DecryptBuffer(buff), 6);
					break;
				case InputReport.IRExtensionAccel:
					ParseButtons(buff);
					ParseAccel(buff);
					ParseIR(buff);
					ParseExtension(DecryptBuffer(buff), 16);
					break;
				case InputReport.Status:
					ParseButtons(buff);
					mWiimoteState.Battery = buff[6];

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
							// start reading again
							BeginAsyncRead();

							InitializeExtension();
						}
						else
							mWiimoteState.ExtensionType = ExtensionType.None;

						if(WiimoteExtensionChanged != null)
							WiimoteExtensionChanged(this, new WiimoteExtensionChangedEventArgs(mWiimoteState.ExtensionType, mWiimoteState.Extension));
					}

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
		/// Handles setting up an extension when plugged.  Currenlty only support the Nunchuk.
		/// </summary>
		private void InitializeExtension()
		{
			WriteData(REGISTER_EXTENSION_INIT, 0x00);

			byte[] buff = ReadData(REGISTER_EXTENSION_TYPE, 2);

			if(buff[0] == (byte)ExtensionType.Nunchuk && buff[1] == (byte)ExtensionType.Nunchuk)
				mWiimoteState.ExtensionType = ExtensionType.Nunchuk;
			else if(buff[0] == (byte)ExtensionType.ClassicController && buff[1] == (byte)ExtensionType.ClassicController)
				mWiimoteState.ExtensionType = ExtensionType.ClassicController;
			else if(buff[0] == 0xff)	// partially inserted case...reset back to nothing inserted
			{
				mWiimoteState.Extension = false;
				mWiimoteState.ExtensionType = ExtensionType.None;
				return;
			}
			else
				throw new Exception("Unknown extension controller found: " + buff[0]);

			buff = DecryptBuffer(ReadData(REGISTER_EXTENSION_CALIBRATION, 16));

			switch(mWiimoteState.ExtensionType)
			{
				case ExtensionType.Nunchuk:
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
			mWiimoteState.AccelState.RawX = buff[3];
			mWiimoteState.AccelState.RawY = buff[4];
			mWiimoteState.AccelState.RawZ = buff[5];

			mWiimoteState.AccelState.X = (float)((float)mWiimoteState.AccelState.RawX - mWiimoteState.AccelCalibrationInfo.X0) /
				((float)mWiimoteState.AccelCalibrationInfo.XG - mWiimoteState.AccelCalibrationInfo.X0);
			mWiimoteState.AccelState.Y = (float)((float)mWiimoteState.AccelState.RawY - mWiimoteState.AccelCalibrationInfo.Y0) /
				((float)mWiimoteState.AccelCalibrationInfo.YG - mWiimoteState.AccelCalibrationInfo.Y0);
			mWiimoteState.AccelState.Z = (float)((float)mWiimoteState.AccelState.RawZ - mWiimoteState.AccelCalibrationInfo.Z0) /
				((float)mWiimoteState.AccelCalibrationInfo.ZG - mWiimoteState.AccelCalibrationInfo.Z0);
		}

		/// <summary>
		/// Parse IR data from report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseIR(byte[] buff)
		{
			mWiimoteState.IRState.RawX1 = buff[6]  | ((buff[8] >> 4) & 0x03) << 8;
			mWiimoteState.IRState.RawY1 = buff[7]  | ((buff[8] >> 6) & 0x03) << 8;

			switch(mWiimoteState.IRState.Mode)
			{
				case IRMode.Basic:
					mWiimoteState.IRState.RawX2 = buff[9]  | ((buff[8] >> 0) & 0x03) << 8;
					mWiimoteState.IRState.RawY2 = buff[10] | ((buff[8] >> 2) & 0x03) << 8;

					mWiimoteState.IRState.Size1 = 0x00;
					mWiimoteState.IRState.Size2 = 0x00;

					mWiimoteState.IRState.Found1 = !(buff[6] == 0xff && buff[7] == 0xff);
					mWiimoteState.IRState.Found2 = !(buff[9] == 0xff && buff[10] == 0xff);
					break;
				case IRMode.Extended:
					mWiimoteState.IRState.RawX2 = buff[9]  | ((buff[11] >> 4) & 0x03) << 8;
					mWiimoteState.IRState.RawY2 = buff[10] | ((buff[11] >> 6) & 0x03) << 8;
					mWiimoteState.IRState.RawX3 = buff[12] | ((buff[14] >> 4) & 0x03) << 8;
					mWiimoteState.IRState.RawY3 = buff[13] | ((buff[14] >> 6) & 0x03) << 8;
					mWiimoteState.IRState.RawX4 = buff[15] | ((buff[17] >> 4) & 0x03) << 8;
					mWiimoteState.IRState.RawY4 = buff[16] | ((buff[17] >> 6) & 0x03) << 8;

					mWiimoteState.IRState.Size1 = buff[8] & 0x0f;
					mWiimoteState.IRState.Size2 = buff[11] & 0x0f;
					mWiimoteState.IRState.Size3 = buff[14] & 0x0f;
					mWiimoteState.IRState.Size4 = buff[17] & 0x0f;

					mWiimoteState.IRState.Found1 = !(buff[6] == 0xff && buff[7] == 0xff && buff[8] == 0xff);
					mWiimoteState.IRState.Found2 = !(buff[9] == 0xff && buff[10] == 0xff && buff[11] == 0xff);
					mWiimoteState.IRState.Found3 = !(buff[12] == 0xff && buff[13] == 0xff && buff[14] == 0xff);
					mWiimoteState.IRState.Found4 = !(buff[15] == 0xff && buff[16] == 0xff && buff[17] == 0xff);
					break;
			}

			mWiimoteState.IRState.X1 = (float)(mWiimoteState.IRState.RawX1 / 1023.5f);
			mWiimoteState.IRState.X2 = (float)(mWiimoteState.IRState.RawX2 / 1023.5f);
			mWiimoteState.IRState.X3 = (float)(mWiimoteState.IRState.RawX3 / 1023.5f);
			mWiimoteState.IRState.X4 = (float)(mWiimoteState.IRState.RawX4 / 1023.5f);

			mWiimoteState.IRState.Y1 = (float)(mWiimoteState.IRState.RawY1 / 767.5f);
			mWiimoteState.IRState.Y2 = (float)(mWiimoteState.IRState.RawY2 / 767.5f);
			mWiimoteState.IRState.Y3 = (float)(mWiimoteState.IRState.RawY3 / 767.5f);
			mWiimoteState.IRState.Y4 = (float)(mWiimoteState.IRState.RawY4 / 767.5f);

			if(mWiimoteState.IRState.Found1 && mWiimoteState.IRState.Found2)
			{
				mWiimoteState.IRState.RawMidX = (mWiimoteState.IRState.RawX2 + mWiimoteState.IRState.RawX1) / 2;
				mWiimoteState.IRState.RawMidY = (mWiimoteState.IRState.RawY2 + mWiimoteState.IRState.RawY1) / 2;
				
				mWiimoteState.IRState.MidX = (mWiimoteState.IRState.X2 + mWiimoteState.IRState.X1) / 2.0f;
				mWiimoteState.IRState.MidY = (mWiimoteState.IRState.Y2 + mWiimoteState.IRState.Y1) / 2.0f;
			}
			else
				mWiimoteState.IRState.MidX = mWiimoteState.IRState.MidY = 0.0f;
		}

		/// <summary>
		/// Parse data from an extension.  Nunchuk/Classic Controller support only.
		/// </summary>
		/// <param name="buff">Data buffer</param>
		/// <param name="offset">Offset into data buffer</param>
		private void ParseExtension(byte[] buff, int offset)
		{
			switch(mWiimoteState.ExtensionType)
			{
				case ExtensionType.Nunchuk:
					mWiimoteState.NunchukState.RawX = buff[offset];
					mWiimoteState.NunchukState.RawY = buff[offset + 1];
					mWiimoteState.NunchukState.AccelState.RawX = buff[offset + 2];
					mWiimoteState.NunchukState.AccelState.RawY = buff[offset + 3];
					mWiimoteState.NunchukState.AccelState.RawZ = buff[offset + 4];

					mWiimoteState.NunchukState.C = (buff[offset + 5] & 0x02) == 0;
					mWiimoteState.NunchukState.Z = (buff[offset + 5] & 0x01) == 0;

					mWiimoteState.NunchukState.AccelState.X = (float)((float)mWiimoteState.NunchukState.AccelState.RawX - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0) /
						((float)mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.XG - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.X0);
					mWiimoteState.NunchukState.AccelState.Y = (float)((float)mWiimoteState.NunchukState.AccelState.RawY - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0) /
						((float)mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.YG - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Y0);
					mWiimoteState.NunchukState.AccelState.Z = (float)((float)mWiimoteState.NunchukState.AccelState.RawZ - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0) /
						((float)mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.ZG - mWiimoteState.NunchukState.CalibrationInfo.AccelCalibration.Z0);

					if(mWiimoteState.NunchukState.CalibrationInfo.MaxX != 0x00)
						mWiimoteState.NunchukState.X = (float)((float)mWiimoteState.NunchukState.RawX - mWiimoteState.NunchukState.CalibrationInfo.MidX) /
							((float)mWiimoteState.NunchukState.CalibrationInfo.MaxX - mWiimoteState.NunchukState.CalibrationInfo.MinX);

					if(mWiimoteState.NunchukState.CalibrationInfo.MaxY != 0x00)
						mWiimoteState.NunchukState.Y = (float)((float)mWiimoteState.NunchukState.RawY - mWiimoteState.NunchukState.CalibrationInfo.MidY) /
							((float)mWiimoteState.NunchukState.CalibrationInfo.MaxY - mWiimoteState.NunchukState.CalibrationInfo.MinY);

					break;
				case ExtensionType.ClassicController:
					mWiimoteState.ClassicControllerState.RawXL = (byte)(buff[offset] & 0x3f);
					mWiimoteState.ClassicControllerState.RawYL = (byte)(buff[offset + 1] & 0x3f);
					mWiimoteState.ClassicControllerState.RawXR = (byte)((buff[offset + 2] >> 7) | (buff[offset + 1] & 0xc0) >> 5 | (buff[offset] & 0xc0) >> 3);
					mWiimoteState.ClassicControllerState.RawYR = (byte)(buff[offset + 2] & 0x1f);

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
						mWiimoteState.ClassicControllerState.XL = (float)((float)mWiimoteState.ClassicControllerState.RawXL - mWiimoteState.ClassicControllerState.CalibrationInfo.MidXL) /
							(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXL - mWiimoteState.ClassicControllerState.CalibrationInfo.MinXL);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL != 0x00)
						mWiimoteState.ClassicControllerState.YL = (float)((float)mWiimoteState.ClassicControllerState.RawYL - mWiimoteState.ClassicControllerState.CalibrationInfo.MidYL) /
							(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYL - mWiimoteState.ClassicControllerState.CalibrationInfo.MinYL);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR != 0x00)
						mWiimoteState.ClassicControllerState.XR = (float)((float)mWiimoteState.ClassicControllerState.RawXR - mWiimoteState.ClassicControllerState.CalibrationInfo.MidXR) /
							(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxXR - mWiimoteState.ClassicControllerState.CalibrationInfo.MinXR);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR != 0x00)
						mWiimoteState.ClassicControllerState.YR = (float)((float)mWiimoteState.ClassicControllerState.RawYR - mWiimoteState.ClassicControllerState.CalibrationInfo.MidYR) /
							(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxYR - mWiimoteState.ClassicControllerState.CalibrationInfo.MinYR);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL != 0x00)
						mWiimoteState.ClassicControllerState.TriggerL = (mWiimoteState.ClassicControllerState.RawTriggerL) /
							(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerL - mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerL);

					if(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR != 0x00)
						mWiimoteState.ClassicControllerState.TriggerR = (mWiimoteState.ClassicControllerState.RawTriggerR) /
							(float)(mWiimoteState.ClassicControllerState.CalibrationInfo.MaxTriggerR - mWiimoteState.ClassicControllerState.CalibrationInfo.MinTriggerR);

					break;
			}
		}

		/// <summary>
		/// Parse data returned from a read report
		/// </summary>
		/// <param name="buff">Data buffer</param>
		private void ParseReadData(byte[] buff)
		{
			if((buff[3] & 0x08) != 0)
				throw new Exception("Error reading data from Wiimote: Bytes do not exist.");
			else if((buff[3] & 0x07) != 0)
				throw new Exception("Error reading data from Wiimote: Attempt to read from write-only registers.");
			else
			{
				int size = buff[3] >> 4;
				Array.Copy(buff, 6, mReadBuff, 0, size+1);
			}

			// set the event so the other thread will continue
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
		private void ReadCalibration()
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
		/// Set Wiimote reporting mode
		/// </summary>
		/// <param name="type">Report type</param>
		/// <param name="continuous">Continuous data</param>
		public void SetReportType(InputReport type, bool continuous)
		{
			switch(type)
			{
				case InputReport.IRAccel:
					EnableIR(IRMode.Extended);
					break;
				case InputReport.IRExtensionAccel:
					EnableIR(IRMode.Basic);
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
		}

		/// <summary>
		/// Turn on the IR sensor
		/// </summary>
		/// <param name="mode">The data report mode</param>
		private void EnableIR(IRMode mode)
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
			WriteData(REGISTER_IR_SENSITIVITY_1, 9, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00, 0xc0});
			WriteData(REGISTER_IR_SENSITIVITY_2, 2, new byte[] {0x40, 0x00});
			WriteData(REGISTER_IR_MODE, (byte)mode);
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
			else
				mStream.Write(mBuff, 0, REPORT_LENGTH);

			Thread.Sleep(100);
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

			mBuff[0] = (byte)OutputReport.ReadMemory;
			mBuff[1] = (byte)(((address & 0xff000000) >> 24) | GetRumbleBit());
			mBuff[2] = (byte)((address & 0x00ff0000)  >> 16);
			mBuff[3] = (byte)((address & 0x0000ff00)  >>  8);
			mBuff[4] = (byte)(address & 0x000000ff);

			mBuff[5] = (byte)((size & 0xff00) >> 8);
			mBuff[6] = (byte)(size & 0xff);

			WriteReport();

			if(!mReadDone.WaitOne(1000, false))
				throw new Exception("Error reading data from Wiimote...is it connected?");

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

			Thread.Sleep(100);
		}

		/// <summary>
		/// Current Wiimote state
		/// </summary>
		public WiimoteState WiimoteState
		{
			get { return mWiimoteState; }
		}

		///<summary>
		/// Force the PC to write to the Wiimote in an alternate way.  This should no longer be used.
		///</summary>
		[Obsolete("The Connect method will determine the proper write method at runtime.  Only set this if you really want to force it.")]
		public bool AltWriteMethod
		{
			get { return mAltWriteMethod; }
			set { mAltWriteMethod = value; }
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

		/// <summary>
		/// Alternative Method to find multiple remotes by Paamayim
		/// http://www.wiimoteproject.com/wiimote-and-bluetooth-connectivity/two-wiimotes-at-a-time-strange-behaviour-t244.0.html
		/// 
		/// Added manually!!
		/// </summary>
		public static List<Wiimote> GetConnectedWiimotes() {
			List<Wiimote> motes = new List<Wiimote>();

			int index = 0;
			bool found = false;
			Guid guid;

			// get the GUID of the HID class
			HIDImports.HidD_GetHidGuid(out guid);

			// get a handle to all devices that are part of the HID class
			// Fun fact:  DIGCF_PRESENT worked on my machine just fine.  I reinstalled Vista, and now it no longer finds the Wiimote with that parameter enabled...
			IntPtr hDevInfo = HIDImports.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, HIDImports.DIGCF_DEVICEINTERFACE);// | HIDImports.DIGCF_PRESENT);

			// create a new interface data struct and initialize its size
			HIDImports.SP_DEVICE_INTERFACE_DATA diData = new HIDImports.SP_DEVICE_INTERFACE_DATA();
			diData.cbSize = Marshal.SizeOf(diData);

			// get a device interface to a single device (enumerate all devices)
			while (HIDImports.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, index, ref diData)) {
				UInt32 size;

				// get the buffer size for this device detail instance (returned in the size parameter)
				HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

				// create a detail struct and set its size
				HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = new HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA();

				// yeah, yeah...well, see, on Win x86, cbSize must be 5 for some reason.  On x64, apparently 8 is what it wants.
				// someday I should figure this out.  Thanks to Paul Miller on this...
				if (IntPtr.Size == 8)
					diDetail.cbSize = 8;
				else
					diDetail.cbSize = 5;

				// actually get the detail struct
				if (HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref diDetail, size, out size, IntPtr.Zero)) {
					Debug.WriteLine(index + " " + diDetail.DevicePath + " " + Marshal.GetLastWin32Error());

					// open a read/write handle to our device using the DevicePath returned
					SafeFileHandle mHandle = HIDImports.CreateFile(diDetail.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);

					// create an attributes struct and initialize the size
					HIDImports.HIDD_ATTRIBUTES attrib = new HIDImports.HIDD_ATTRIBUTES();
					attrib.Size = Marshal.SizeOf(attrib);

					// get the attributes of the current device
					if (HIDImports.HidD_GetAttributes(mHandle.DangerousGetHandle(), ref attrib)) {
						// if the vendor and product IDs match up
						if (attrib.VendorID == VID && attrib.ProductID == PID) {
							Debug.WriteLine("Found it!");
							found = true;

							Wiimote mote = new Wiimote();
							mote.mStream = new FileStream(mHandle, FileAccess.ReadWrite, REPORT_LENGTH, true);
							mote.mHandle = mHandle;

							// start an async read operation on it
							mote.BeginAsyncRead();

							// read the calibration info from the controller
							try {
								mote.ReadCalibration();
							} catch {
								// if we fail above, try the alternate HID writes
								mote.mAltWriteMethod = true;
								mote.ReadCalibration();
							}


							// force a status check to get the state of any extensions plugged in at startup
							mote.GetStatus();

							motes.Add(mote);
						} else {
							// otherwise this isn't the controller, so close up the file handle
							mHandle.Close();
						}
					}
				} else {
					// failed to get the detail struct
					throw new Exception("SetupDiGetDeviceInterfaceDetail failed on index " + index);
				}

				index++;
			}

			// clean up our list
			HIDImports.SetupDiDestroyDeviceInfoList(hDevInfo);

			// if we didn't find a Wiimote, throw an exception
			if (!found)
				throw new Exception("Wiimote not found in HID device list.");

			return motes;
		}
		#endregion
	}
}
