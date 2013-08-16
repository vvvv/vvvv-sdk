#region licence/info

//////project name
//uEyeCam

//////description
//Native interface to uEye cameras by IDS-Imaging
//Writes images to shared memory

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.SharedMemory;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class uEyeCamPlugin: UserControl, IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//input pin declaration
		private IValueIn FEnablePin;
		private IValueIn FPixelClockPin;
		private IValueIn FFrameRatePin;
		private IValueIn FExposureTimePin;
		private IValueIn FFormatPin;
		private IValueIn FFlashModePin;
		private IValueIn FFlashDelayPin;
		private IValueIn FFlashDurationPin;
		
		private IStringIn FSharedNamePin;

		//output pin declaration
		private IValueOut FActualFrameRatePin;
		private IStringOut FInfoPin;

		//further fields
		private Segment FSegment;
		private uEye FuEyeCam;
		
		private const int IMAGE_COUNT = 1;
		private struct UEYEIMAGE
		{
			public IntPtr pMemory;
			public int MemID;
			public int nSeqNum;
		}
		private UEYEIMAGE[] FuEyeImages;
		private IntPtr FCurMem;

		private bool FDrawing;
		private int	FRenderMode = uEye.IS_RENDER_FIT_TO_WINDOW;
		private int FWidth, FHeight;
		
		private int FColorMode = uEye.IS_SET_CM_RGB24;
		private int FBytes = 3;
		private int FBits = 24;

		#endregion field declaration
		
		#region constructor/destructor
		public uEyeCamPlugin()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			// init our ueye object
			FuEyeCam = new uEye();
			// enable static messages ( no open camera is needed )
			FuEyeCam.EnableMessage(uEye.IS_NEW_DEVICE, this.Handle.ToInt32());
			FuEyeCam.EnableMessage(uEye.IS_DEVICE_REMOVAL, this.Handle.ToInt32());
			
			// init our image struct and alloc marshall pointers for the uEye memory
			FuEyeImages = new UEYEIMAGE[IMAGE_COUNT];
			int nLoop = 0;
			for (nLoop = 0; nLoop < IMAGE_COUNT; nLoop++)
			{
				FuEyeImages[nLoop].pMemory = Marshal.AllocCoTaskMem(4);	// create marshal object pointers
				FuEyeImages[nLoop].MemID	= 0;
				FuEyeImages[nLoop].nSeqNum	= 0;
			}
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					if (FSegment != null)
						FSegment.Dispose();
				}
				
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				FreeImages();
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}
		
		private void FreeImages()
		{
			if (FuEyeCam.IsOpen())
			{
				// release marshal object pointers
				int nLoop = 0;
				for (nLoop = 0; nLoop < IMAGE_COUNT; nLoop++)
					Marshal.FreeCoTaskMem(FuEyeImages[nLoop].pMemory);
				FuEyeCam.ExitCamera();
			}
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "uEyeCam";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Devices";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Native interface to uEye cameras by http://ids-imaging.de/";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "Based on Interfaceclass for uEye Camera family provided by http://ids-imaging.de";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(200, 100);
					//define the nodes initial size in window-mode
					FPluginInfo.InitialWindowSize = new Size(400, 300);
					//define the nodes initial component mode
					FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;
			}
		}
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return true;}
		}
		
		#endregion node name and infos
		
		private void InitializeComponent()
		{
			this.Preview = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.Preview)).BeginInit();
			this.SuspendLayout();
			// 
			// Preview
			// 
			this.Preview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Preview.Location = new System.Drawing.Point(0, 0);
			this.Preview.Name = "Preview";
			this.Preview.Size = new System.Drawing.Size(461, 296);
			this.Preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.Preview.TabIndex = 0;
			this.Preview.TabStop = false;
			// 
			// uEyeCamPlugin
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.Preview);
			this.DoubleBuffered = true;
			this.Name = "uEyeCamPlugin";
			this.Size = new System.Drawing.Size(461, 296);
			((System.ComponentModel.ISupportInitialize)(this.Preview)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.PictureBox Preview;
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs
			FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out FEnablePin);
			FEnablePin.SetSubType(0, 1, 1, 1, false, true, false);
			
			FHost.CreateValueInput("Format", 1, null, TSliceMode.Single, TPinVisibility.True, out FFormatPin);
			FFormatPin.SetSubType(0, 5, 1, 1, false, false, true);
			
			FHost.CreateValueInput("Pixel Clock", 1, null, TSliceMode.Single, TPinVisibility.True, out FPixelClockPin);
			FPixelClockPin.SetSubType(5, 30, 1, 30, false, false, false);
			
			FHost.CreateValueInput("Framerate", 1, null, TSliceMode.Single, TPinVisibility.True, out FFrameRatePin);
			FFrameRatePin.SetSubType(7.76, 52.18, 0.01, 52.18, false, false, false);
			
			FHost.CreateValueInput("Exposure Time", 1, null, TSliceMode.Single, TPinVisibility.True, out FExposureTimePin);
			FExposureTimePin.SetSubType(0.03, 19.099, 0.01, 5, false, false, false);
			
			FHost.CreateValueInput("Flash Mode", 1, null, TSliceMode.Single, TPinVisibility.True, out FFlashModePin);
			FFlashModePin.SetSubType(0, 10, 1, 0, false, false, true);
			
			FHost.CreateValueInput("Flash Delay", 1, null, TSliceMode.Single, TPinVisibility.True, out FFlashDelayPin);
			FFlashDelayPin.SetSubType(0, 20, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Flash Duration", 1, null, TSliceMode.Single, TPinVisibility.True, out FFlashDurationPin);
			FFlashDurationPin.SetSubType(0, 20, 0.01, 1, false, false, false);
			
			FHost.CreateStringInput("Shared Name", TSliceMode.Dynamic, TPinVisibility.True, out FSharedNamePin);
			FSharedNamePin.SetSubType("#vvvv", false);

			//create outputs
			FHost.CreateValueOutput("Actual Framerate", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FActualFrameRatePin);
			FActualFrameRatePin.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateStringOutput("Info", TSliceMode.Single, TPinVisibility.True, out FInfoPin);
			FInfoPin.SetSubType("", false);
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
			double val;
			double actualFPS=0;
			double actualEXP=0;
			bool needsinit = false;
			
			if (FEnablePin.PinIsChanged)
			{
				FEnablePin.GetValue(0, out val);
				if (val < 0.5)
				{
					if (FuEyeCam.IsOpen())
					{
						FuEyeCam.ExitCamera();
						FreeImages();
					}
				}
				else
				{
					needsinit = true;
				}				
			}
			
			if (FFormatPin.PinIsChanged)
			{
				int format;
				double f;
				
				FFormatPin.GetValue(0, out f);
				format = (int) f;
				
				switch (format)
				{
					case 0:
						{
							FColorMode = uEye.IS_SET_CM_RGB32;
							FBytes = 4;
							FBits = 32;
							break;
						}
					case 1:
						{
							FColorMode = uEye.IS_SET_CM_RGB24;
							FBytes = 3;
							FBits = 24;
							break;
						}
					case 2:
						{
							FColorMode = uEye.IS_SET_CM_RGB16;
							FBytes = 2;
							FBits = 16;
							break;
						}
					case 3:
						{
							FColorMode = uEye.IS_SET_CM_RGB15;
							FBytes = 2;
							FBits = 15;
							break;
						}
					case 4:
						{
							FColorMode = uEye.IS_SET_CM_BAYER;
							FBytes = 4; //??
							FBits = 32; //??
							break;
						}
					case 5:
						{
							FColorMode = uEye.IS_SET_CM_Y8;
							FBytes = 1;
							FBits = 8;
							break;
						}
				}
				
				if (FuEyeCam.IsOpen())
					needsinit = true;
			}
			
			if (needsinit)
				InitCam(0);
			
			if (FuEyeCam == null)
				return;
			
			if (FSharedNamePin.PinIsChanged)
			{
				if (FSegment != null)
					FSegment.Dispose();

				string name;
				FSharedNamePin.GetString(0, out name);

				FSegment = new Segment(name, SharedMemoryCreationFlag.Create, FWidth*FHeight*FBytes);
			}
			
			if (FPixelClockPin.PinIsChanged)
			{
				FPixelClockPin.GetValue(0, out val);
				FuEyeCam.SetPixelClock((int) val);
			}
			
			if (FFrameRatePin.PinIsChanged)
			{
				FFrameRatePin.GetValue(0, out val);
				FuEyeCam.SetFrameRate(val, ref actualFPS);
			}
			
			if (FExposureTimePin.PinIsChanged)
			{
				FExposureTimePin.GetValue(0, out val);
				FuEyeCam.SetExposureTime(val, ref actualEXP);
			}
			
			if (FFlashDelayPin.PinIsChanged || FFlashDurationPin.PinIsChanged)
			{
				FFlashDelayPin.GetValue(0, out val);
				double dur;
				FFlashDurationPin.GetValue(0, out dur);
				FuEyeCam.SetFlashDelay((int) val *1000, (int) dur*1000);
			}
			
			if (FFlashModePin.PinIsChanged)
			{
				FFlashModePin.GetValue(0, out val);
				FuEyeCam.SetFlashStrobe((int) val, 0);
			}
			
			FuEyeCam.GetFramesPerSecond(ref actualFPS);
			FActualFrameRatePin.SetValue(0, actualFPS);
		}
		
		#endregion mainloop
		
		void InitCam(int CamID)
		{
			//if opened before, close now
			if (FuEyeCam.IsOpen())
			{
				FuEyeCam.ExitCamera();
				FreeImages();
			}

			//open a camera
			if (FuEyeCam.InitCamera(CamID, Handle.ToInt32()) != uEye.IS_SUCCESS)
			{
				FHost.Log(TLogType.Error, "Failed to initialize uEyCam, ID: " +CamID);
				return;
			}

			// check for image size
			uEye.SENSORINFO sensorInfo = new uEye.SENSORINFO();
			FuEyeCam.GetSensorInfo( ref sensorInfo );
			int x = sensorInfo.nMaxWidth;
			int y = sensorInfo.nMaxHeight;
			FuEyeCam.SetImageSize(x, y);

			FWidth = x;
			FHeight = y;
			
			// alloc images
			FuEyeCam.ClearSequence();
			int nLoop = 0;
			for (nLoop = 0; nLoop < IMAGE_COUNT; nLoop++)
			{
				// alloc memory
				FuEyeCam.AllocImageMem(x, y, FBits, ref FuEyeImages[nLoop].pMemory, ref FuEyeImages[nLoop].MemID);
				// add our memory to the sequence
				FuEyeCam.AddToSequence(FuEyeImages[nLoop].pMemory, FuEyeImages[nLoop].MemID);
				// set sequence number
				FuEyeImages[nLoop].nSeqNum	= nLoop + 1;
			}

			FuEyeCam.SetColorMode(FColorMode);
			FuEyeCam.EnableMessage(uEye.IS_FRAME, this.Handle.ToInt32());
			
			UpdateInfos();

			// free image
			if (Preview.Image != null)
			{
				Preview.Image.Dispose();
				Preview.Image = null;
			}

			// capture a single image
			FuEyeCam.CaptureVideo(uEye.IS_WAIT);
			Refresh();
		}
		
		private void UpdateInfos()
		{
			string info;
			
			int ver = uEye.GetDLLVersion();
			info = String.Format("uEye SDK Version: {0}.{1}.{2}", (ver>>24), (ver>>16&0xff), (ver&0xffff));

			int nrOfCameras = 0;
			uEye.GetNumberOfCameras(ref nrOfCameras);
			info += "\n" +  String.Format("Connected cameras: {0}", nrOfCameras );

			// camera infos
			if (FuEyeCam.IsOpen())
			{
				// Sensorinfo
				uEye.SENSORINFO sensorInfo = new uEye.SENSORINFO();
				FuEyeCam.GetSensorInfo(ref sensorInfo);
				info += "\n" + "Sensor: " + sensorInfo.strSensorName;

				// Camerainfo
				uEye.CAMINFO cameraInfo = new uEye.CAMINFO();
				FuEyeCam.GetCameraInfo(ref cameraInfo);
				info += "\n" + "CameraInfo:";
				info += "\n" + "   SerNo: " + cameraInfo.SerNo;
				info += "\n" + "   Date: " + cameraInfo.Date;
				info += "\n" + "   Version: " + cameraInfo.Version;
				info += "\n" + String.Format("   Camera ID: {0}", cameraInfo.id);

				// Memory board query
				if (FuEyeCam.IsMemoryBoardConnected())
					info += "\n" + "Memoryboard connected";
				else
					info += "\n" + "No Memoryboard connected";
			}
			FInfoPin.SetString(0, info);
		}
		
		void DrawImage()
		{
			FDrawing = true;
			// draw current memory if a camera is opened
			if (FuEyeCam.IsOpen())
			{
				int num = 0;
				IntPtr pMem = new IntPtr();
				IntPtr pLast = new IntPtr();
				FuEyeCam.GetActSeqBuf(ref num, ref pMem, ref pLast);
				if (pLast.ToInt32() == 0)
				{
					FDrawing = false;
					return;
				}

				int nLastID = GetImageID(pLast);
				int nLastNum = GetImageNum(pLast);
				FuEyeCam.LockSeqBuf(nLastNum, pLast);

				FCurMem = pLast;		// remember current buffer for our tootip ctrl
				
				FuEyeCam.RenderBitmap(nLastID, Preview.Handle.ToInt32(), FRenderMode);
				
				if (FSegment != null)
				{
					FSegment.Lock();
					FSegment.CopyByteArrayToSharedMemory(FuEyeImages[0].pMemory, FWidth * FHeight * FBytes);
					FSegment.Unlock();
				}
				
				FuEyeCam.UnlockSeqBuf(nLastNum, pLast);
			}
			FDrawing = false;
		}

		// ------------------------  GetImageID -------------------------------
		//
		int GetImageID( IntPtr pBuffer )
		{
			// get image id for a given memory
			if ( !FuEyeCam.IsOpen() )
				return 0;

			int i = 0;
			for ( i=0; i<IMAGE_COUNT; i++)
				if ( FuEyeImages[i].pMemory == pBuffer )
				return FuEyeImages[i].MemID;
			return 0;
		}
		
		
		// ------------------------  GetImageNum -------------------------------
		//
		int GetImageNum( IntPtr pBuffer )
		{
			// get number of sequence for a given memory
			if ( !FuEyeCam.IsOpen() )
				return 0;

			int i = 0;
			for ( i=0; i<IMAGE_COUNT; i++)
				if ( FuEyeImages[i].pMemory == pBuffer )
				return FuEyeImages[i].nSeqNum;

			return 0;
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawImage();
		}
		
		
		// ------------------------  WndProc  -------------------------------
		//
		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
		protected override void WndProc(ref Message m)
		{
			// Listen for operating system messages
			switch (m.Msg)
			{
					// Ueye Message
				case uEye.IS_UEYE_MESSAGE:
					HandleUeyeMessage(m.WParam.ToInt32(), m.LParam.ToInt32());
					break;
			}
			base.WndProc(ref m);
		}

		// ------------------------  HandleUeyeMessage  -------------------------------
		//
		void HandleUeyeMessage( int wParam, int lParam )
		{
			switch (wParam)
			{
				case uEye.IS_FRAME:
					if (!FDrawing)
						DrawImage();
					break;

				case uEye.IS_DEVICE_REMOVAL:
				case uEye.IS_NEW_DEVICE:
					//UpdateInfos();
					break;
			}
		}

	}
}
