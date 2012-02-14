#region licence/info

//////project name
//vvvv tablet plugin

//////description
//vvvv node for wintab graphics tablet

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# SharpDevelop / Visual Studio

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group (template)
//moti zilberman (plugin)

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;


using VBTablet;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

//the vvvv node namespace
namespace VVVV.Nodes
{

    //class definition
    public class TabletNode :  IPlugin, IDisposable
    {
        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
   		private bool FDisposed = false;

        //input pin declaration
        private IValueIn FPinInputEnable;
        private IValueIn FPinInputDigitizing;

        //output pin declaration
        private IValueOut FPinOutputX, FPinOutputY, FPinOutputPressure, FPinOutputCursor, FPinOutputSerialNo, FPinOutputCursorType,
            FPinOutputCursorSubtype,
            FPinOutputProximity, FPinOutputTilt, FPinOutputAzimuth, FPinOutputDimensions, FPinOutputButtons;
        private IStringOut FPinOutputCursorName;

        private IValueConfig FPinConfigDebug;

        private IValueOut FPinDebugButtons;

        private Tablet Tablet;

        #endregion field declaration

        #region constructor/destructor

        static string getMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }


        public TabletNode()
        {
            Random r = new Random();
            ctxName = getMd5Hash("VVVV" + r.Next().ToString() + r.Next().ToString());
            //the nodes constructor
            //CreateTablet();
            //            Connect();

        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~TabletNode()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
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
        
        protected virtual void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			// Dispose managed resources.
        			if (Tablet != null)
        			{
			     		Disconnect();
			     		Tablet = null;
        			}
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
				//..
        	}
        	FDisposed = true;
        }

        #endregion constructor/destructor

        #region node name and infos

        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                //fill out nodes info
                //see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
                if (FPluginInfo == null)
                {
                    FPluginInfo = new PluginInfo();
                    FPluginInfo.Name = "Tablet";							//use CamelCaps and no spaces
                    FPluginInfo.Category = "Devices";						//try to use an existing one
                    FPluginInfo.Version = "Wintab";						//versions are optional. leave blank if not needed
                    FPluginInfo.Help = "Grabs input from a tablet via the Wintab API";
                    FPluginInfo.Tags = "";
                    FPluginInfo.Bugs = "";
                    FPluginInfo.Credits = "Based on VBTablet.NET alpha";								//give credits to thirdparty code used
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                }
                return FPluginInfo;
                //leave above as is
            }
        }

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return true; }
        }

        #endregion node name and infos

        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs

            FHost.CreateValueConfig("Debug Pins", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPinConfigDebug);
            FPinConfigDebug.SetSubType(0, 1, 1, 0, false, true, false);

            FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinInputEnable);
            FPinInputEnable.SetSubType(0, 1, 1, 0, false, true, false);

            FHost.CreateValueInput("Exclusive", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinInputDigitizing);
            FPinInputDigitizing.SetSubType(0, 1, 1, 1, false, true, false);

            //create outputs	    	
            FHost.CreateValueOutput("X", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputX);
            FPinOutputX.SetSubType(-1, 1, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Y", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputY);
            FPinOutputY.SetSubType(-1, 1, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Pressure", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputPressure);
            FPinOutputPressure.SetSubType(0, 1, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Proximity", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputProximity);
            FPinOutputProximity.SetSubType(0, 1, 1, 0, false, true, false);

            FHost.CreateValueOutput("Tilt", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputTilt);
            FPinOutputTilt.SetSubType(-1, 1, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Azimuth", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputAzimuth);
            FPinOutputAzimuth.SetSubType(0, 1, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Cursor", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputCursor);
            FPinOutputCursor.SetSubType(int.MinValue, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueOutput("Cursor Serial No.", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputSerialNo);
            FPinOutputSerialNo.SetSubType(int.MinValue, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueOutput("Cursor Type", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputCursorType);
            FPinOutputCursorType.SetSubType(int.MinValue, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueOutput("Cursor Subtype", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinOutputCursorSubtype);
            FPinOutputCursorSubtype.SetSubType(int.MinValue, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateStringOutput("Cursor Name", TSliceMode.Single, TPinVisibility.True, out FPinOutputCursorName);
            FPinOutputCursorName.SetSubType("", false);

            FHost.CreateValueOutput("Dimensions", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputDimensions);
            FPinOutputDimensions.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            FHost.CreateValueOutput("Buttons", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPinOutputButtons);
            FPinOutputButtons.SetSubType(0, 1, 1, 0, false, true, true);

            if (Tablet != null)
	            Connect();
            Configurate(FPinConfigDebug);
        }

        #endregion pin creation

        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            if (Input == null)
                return;
            switch (Input.Name)
            {
                case "Debug Pins":
                    double pinConfigDebug;
                    FPinConfigDebug.GetValue(0, out pinConfigDebug);
                    if (pinConfigDebug == 1d)
                    {
                        if (FPinDebugButtons == null)
                        {

                            FHost.CreateValueOutput("Buttons (Debug)", 1, null, TSliceMode.Single, TPinVisibility.True, out FPinDebugButtons);
                            FPinDebugButtons.SetSubType(int.MinValue, int.MinValue, 1, 0, false, false, true);
                        }
                    }
                    else if (FPinDebugButtons != null)
                    {
                        FHost.DeletePin(FPinDebugButtons);
                        FPinDebugButtons = null;
                    }
                    break;
            }
        }

        void Enable()
        {
            if (CreateTablet())
            {
                if (!Tablet.Connected)
                    Connect();
                CreateContext(ref ctxName);
                Tablet.SelectContext(ref ctxName);
                Tablet.Context.Enabled = true;
            }
        }


        void Disable()
        {
            if (Tablet != null)
            {
                Tablet.SelectContext(ref ctxName);
                Tablet.Context.Enabled = false;
            }
        }

        double X, Y, NormalPressure, Tilt, Azimuth;
        int CursorNum;
        bool InContext = false;
        int Buttons, SerialNo, CursorType, CursorSubtype, NumButtons;
        string CursorName;

        void Tablet_PacketArrival(ref IntPtr ContextHandle, ref int Cursor, ref int X, ref int Y, ref int Z, ref int Buttons, ref int NormalPressure, ref int TangentPressure, ref int Azimuth, ref int Altitude, ref int Twist, ref int Pitch, ref int Roll, ref int Yaw, ref int PacketSerial, ref int PacketTime)
        {
            this.X = 2 * (double)X / Tablet.Context.OutputExtentX - 1;
            this.Y = 2 * (double)Y / Tablet.Context.OutputExtentY - 1;
            this.NormalPressure = (double)NormalPressure / Tablet.Device.NormalPressure.get_Max(false);
            this.Buttons = Buttons;
            this.Tilt = (900 - Math.Abs((double)Altitude)) / 3600;
            this.Azimuth = (double)Azimuth / 3600;
            this.CursorNum = Tablet.Cursor.Index = Cursor;
            this.NumButtons = Tablet.Cursor.NumButtons;
            this.SerialNo = Tablet.Cursor.PhysicalID;
            this.CursorType = Tablet.Cursor.PhysicalType & 0x0F06;
            this.CursorSubtype = (Tablet.Cursor.PhysicalType & 0xF0) >> 4;
            this.CursorName = Tablet.Cursor.Name;
        }

        bool digitizing = true;
        string ctxName;

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        bool CreateTablet()
        {
            if (Tablet == null)
            {
                try
                {
                    Tablet = new Tablet();
                    Tablet.PacketArrival += new Tablet.PacketArrivalEventHandler(Tablet_PacketArrival);
                    Tablet.ProximityChange += new Tablet.ProximityChangeEventHandler(Tablet_ProximityChange);
                }
                catch (Exception e)
                {
                    Tablet = null;
                    FHost.Log(TLogType.Error, e.Message);
                    return false;
                }
            }
            return true;
        }

        void Tablet_ProximityChange(ref bool InContext, ref bool IsPhysical, ref IntPtr ContextHandle, ref string ContextName)
        {
            this.InContext = InContext;
        }

        void CreateContext(ref string name)
        {
            if (Tablet.GetCtxHandleByName(ref name) == IntPtr.Zero)
            {
                Tablet.AddContext(name, ref digitizing);
                Tablet.SelectContext(ref name);
                Tablet.Context.Options.IsPenCtx = Tablet.Context.Options.IsSystemCtx = true;
                Tablet.Context.Options.NotifyCursorChange = true;

                Tablet.Connected = true;

                Tablet.Context.OutputExtentX = Tablet.Context.InputExtentX;
                Tablet.Context.OutputExtentY = Tablet.Context.InputExtentY;
                Tablet.Context.Update();
            }
        }

        UserControl hiddenControl = new UserControl();

        void Connect()
        {
            if (Tablet.Connected && (Tablet.GetCtxHandleByName(ref ctxName) != IntPtr.Zero))
                return;
            //Tablet.hWnd = Handle;
            Tablet.hWnd = hiddenControl.Handle;
            CreateContext(ref ctxName);
        }

        void Disconnect()
        {
            if (!Tablet.Connected)
                return;

            Tablet.SelectContext(ref ctxName);
            Tablet.Context.Update();

            Tablet.RemoveContext(ref ctxName);

            Tablet.Connected = false;

        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            double pinInputEnable, pinInputDigitizing;
            FPinInputEnable.GetValue(0, out pinInputEnable);
            FPinInputDigitizing.GetValue(0, out pinInputDigitizing);
            if (FPinInputEnable.PinIsChanged)
            {
                if (pinInputEnable == 1d)
                    Enable();
                else
                    Disable();
            }
            if (Tablet == null)
                return;
            if (FPinInputDigitizing.PinIsChanged)
            {
                digitizing = (pinInputDigitizing == 1d);
                if (pinInputEnable == 1d)
                    Disable();
                Disconnect();
                Connect();
                if (pinInputEnable == 1d)
                    Enable();
            }
            if ((pinInputEnable == 1d))
            {
                FPinOutputProximity.SetValue(0, InContext ? 1 : 0);
                FPinOutputX.SetValue(0, X);
                FPinOutputY.SetValue(0, Y);
                FPinOutputPressure.SetValue(0, NormalPressure);
                FPinOutputCursor.SetValue(0, CursorNum);
                FPinOutputSerialNo.SetValue(0, SerialNo);
                FPinOutputCursorType.SetValue(0, CursorType);
                FPinOutputCursorSubtype.SetValue(0, CursorSubtype);
                    FPinOutputButtons.SliceCount = NumButtons;
                    for (int i = 0; i < NumButtons; i++)
                        FPinOutputButtons.SetValue(i, ((Buttons & (1 << i)) != 0) ? 1 : 0);
                FPinOutputAzimuth.SetValue(0, Azimuth);
                FPinOutputTilt.SetValue(0, Tilt);
                if (FPinDebugButtons != null)
                    FPinDebugButtons.SetValue(0, NumButtons);
                FPinOutputDimensions.SliceCount = 2;
                FPinOutputDimensions.SetValue(0, Tablet.Context.InputExtentX);
                FPinOutputDimensions.SetValue(1, Tablet.Context.InputExtentY);
                FPinOutputCursorName.SetString(0, CursorName);
            }
        }

        #endregion mainloop
    }
}

    