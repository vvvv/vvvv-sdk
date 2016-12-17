using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using VVVV.PluginInterfaces.V1;

using TUIO.NET;

namespace TUIODecoder
{
    public class TUIODecoder : IPlugin, IDisposable
    {
        //DEBUG
        public static TUIODecoder instance;

        #region field declaration

        //the host (mandatory)
        public IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IStringIn FTUIOPacketInput;

        //output pin declaration
        private IValueOut FSessionIDOut;
        private IValueOut FClassIDOut;
        private IValueOut FTypeIDOut;
        private IValueOut FPosXOut;
        private IValueOut FPosYOut;
        private IValueOut FWidthOut;
        private IValueOut FHeightOut;
        private IValueOut FAreaOut;
        private IValueOut FAngleOut;
        private IValueOut FMovementXOut;
        private IValueOut FMovementYOut;
        private IValueOut FMotionAccelerationOut;
        private IValueOut FRotationAccelerationOut;
        private IValueOut FMotionSpeedOut;
        private IValueOut FRotationSpeedOut;

        // TUIO Client and stuff
        private TuioClient FTuioClient;

        #endregion field declaration

        #region constructor/destructor

        public TUIODecoder()
        {
            FTuioClient = new TuioClient();
            instance = this;
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
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Debug, "TUIODecoder is being deleted");

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
        ~TUIODecoder()
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
                //see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
                IPluginInfo Info = new PluginInfo();
                Info.Name = "TUIODecoder";							//use CamelCaps and no spaces
                Info.Category = "Network";						//try to use an existing one
                Info.Version = "1.0 Legacy";						//versions are optional. leave blank if not needed
                Info.Help = "Takes a TUIO command string (for example from an UDP client), \ndecodes it and returns the parsed command";
                Info.Bugs = "";
                Info.Credits = "Thanks to the \"TUIO C# Library\" from the reacTIVision project";								//give credits to thirdparty code used
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
            get { return false; }
        }

        #endregion

        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateStringInput("TUIO Packet String", TSliceMode.Single, TPinVisibility.True, out FTUIOPacketInput);
            FTUIOPacketInput.SetSubType("", false);

            //create outputs	    	
            FHost.CreateValueOutput("Session ID", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSessionIDOut);
            FSessionIDOut.SetSubType(0, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueOutput("Class ID", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FClassIDOut);
            FClassIDOut.SetSubType(0, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueOutput("Type", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTypeIDOut);
            FTypeIDOut.SetSubType(0, int.MaxValue, 1, 0, false, false, true);

            FHost.CreateValueOutput("Position X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPosXOut);
            FPosXOut.SetSubType(0, 1, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Position Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPosYOut);
            FPosYOut.SetSubType(0, 1, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Angle", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAngleOut);
            FAngleOut.SetSubType(0, Math.PI * 2, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Width", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FWidthOut);
            FPosYOut.SetSubType(0, 1, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Height", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHeightOut);
            FPosYOut.SetSubType(0, 1, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Area", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAreaOut);
            FPosYOut.SetSubType(0, 1, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Movement X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMovementXOut);
            FMovementXOut.SetSubType(float.MinValue, float.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Movement Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMovementYOut);
            FMovementYOut.SetSubType(float.MinValue, float.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Motion Acceleration", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMotionAccelerationOut);
            FMotionAccelerationOut.SetSubType(float.MinValue, float.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Rotation Acceleration", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRotationAccelerationOut);
            FRotationAccelerationOut.SetSubType(float.MinValue, float.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Motion Speed", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMotionSpeedOut);
            FMotionSpeedOut.SetSubType(float.MinValue, float.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Rotation Speed", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRotationSpeedOut);
            FRotationSpeedOut.SetSubType(float.MinValue, float.MaxValue, 0.0001, 0, false, false, false);

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
            //if any of the inputs has changed
            //recompute the outputs
            if (FTUIOPacketInput.PinIsChanged)
            {
                string currentPacket;
                FTUIOPacketInput.GetString(0, out currentPacket);
                if (currentPacket == "" || currentPacket == null) return;
                int tuioPosition = currentPacket.IndexOf("#bundle");
                if (tuioPosition == -1)
                    return;


                while ((tuioPosition = currentPacket.IndexOf("#bundle")) >= 0)
                {
                    int nextpos = currentPacket.IndexOf("#bundle", tuioPosition + 1);
                    string currentSeperatedPacket = "";
                    if (nextpos == -1)
                    {
                        currentSeperatedPacket = currentPacket;
                        currentPacket = "";
                    }
                    else
                    {
                        currentSeperatedPacket = currentPacket.Substring(tuioPosition, nextpos - tuioPosition);
                        currentPacket = currentPacket.Substring(nextpos);
                    }

                    VVVV.Utils.OSC.OSCPacket packet = VVVV.Utils.OSC.OSCPacket.Unpack(VVVV.Utils.OSC.OSCPacket.ASCIIEncoding8Bit.GetBytes(currentSeperatedPacket));
                    if (packet.IsBundle())
                    {
                        ArrayList messages = packet.Values;
                        for (int i = 0; i < messages.Count; i++)
                        {
                            FTuioClient.ProcessMessage((VVVV.Utils.OSC.OSCMessage)messages[i]);
                        }
                    }
                    else
                        FTuioClient.ProcessMessage((VVVV.Utils.OSC.OSCMessage)packet);
                }
            }
            List<TuioCursor> cursors = FTuioClient.getTuioCursors();
            List<TuioObject> objects = FTuioClient.getTuioObjects();
            List<TuioBlob> blobs = FTuioClient.getTuioBlobs();
            int slicecount = cursors.Count + objects.Count + blobs.Count;
            FSessionIDOut.SliceCount = slicecount;
            FClassIDOut.SliceCount = slicecount;
            FTypeIDOut.SliceCount = slicecount;
            FPosXOut.SliceCount = slicecount;
            FPosYOut.SliceCount = slicecount;
            FWidthOut.SliceCount = slicecount;
            FHeightOut.SliceCount = slicecount;
            FAreaOut.SliceCount = slicecount;
            FAngleOut.SliceCount = slicecount;
            FMovementXOut.SliceCount = slicecount;
            FMovementYOut.SliceCount = slicecount;
            FMotionAccelerationOut.SliceCount = slicecount;
            FRotationAccelerationOut.SliceCount = slicecount;
            FMotionSpeedOut.SliceCount = slicecount;
            FRotationSpeedOut.SliceCount = slicecount;

            int curindex = 0;
            for (int i = 0; i < cursors.Count; i++)
            {
                TuioCursor cur = cursors[i];
                FSessionIDOut.SetValue(curindex, cur.getSessionID());
                FClassIDOut.SetValue(curindex, cur.getFingerID());
                FTypeIDOut.SetValue(curindex, 0);
                FPosXOut.SetValue(curindex, cur.getPosition().getX() * 2 - 1);
                FPosYOut.SetValue(curindex, -cur.getPosition().getY() * 2 + 1);
                FAngleOut.SetValue(curindex, 0);
                FMovementXOut.SetValue(curindex, cur.getXSpeed());
                FMovementYOut.SetValue(curindex, cur.getYSpeed());
                FMotionAccelerationOut.SetValue(curindex, cur.getMotionAccel());
                FRotationAccelerationOut.SetValue(curindex, 0);
                FMotionSpeedOut.SetValue(curindex, cur.getMotionSpeed());
                FRotationSpeedOut.SetValue(curindex, 0);
                curindex++;
            }
            int objectOffset = 1000;
            for (int i = 0; i < objects.Count; i++)
            {
                TuioObject obj = objects[i];
                FSessionIDOut.SetValue(curindex, obj.getSessionID());
                FClassIDOut.SetValue(curindex, obj.getFiducialID());
                FTypeIDOut.SetValue(curindex,  1);
                FPosXOut.SetValue(curindex, obj.getPosition().getX() * 2 - 1);
                FPosYOut.SetValue(curindex, -obj.getPosition().getY() * 2 + 1);
                FAngleOut.SetValue(curindex, 1 - ((obj.getAngle()) / (Math.PI + Math.PI)));
                FMovementXOut.SetValue(curindex, obj.getXSpeed());
                FMovementYOut.SetValue(curindex, obj.getYSpeed());
                FMotionAccelerationOut.SetValue(curindex, obj.getMotionAccel());
                FRotationAccelerationOut.SetValue(curindex, obj.getRotationAccel());
                FMotionSpeedOut.SetValue(curindex, obj.getMotionSpeed());
                FRotationSpeedOut.SetValue(curindex, obj.getRotationSpeed());
                curindex++;
            }

            int blobOffset = 2000;
            for (int i = 0; i < blobs.Count; i++)
            {
                TuioBlob blb = blobs[i];
                FSessionIDOut.SetValue(curindex, blb.getSessionID());
                FClassIDOut.SetValue(curindex, blb.getBlobID());
                FTypeIDOut.SetValue(curindex, 2);
                FPosXOut.SetValue(curindex, blb.getPosition().getX() * 2 - 1);
                FPosYOut.SetValue(curindex, -blb.getPosition().getY() * 2 + 1);
                FWidthOut.SetValue(curindex, blb.getWidth());
                FHeightOut.SetValue(curindex, blb.getHeight());
                FAreaOut.SetValue(curindex, blb.getArea());
                FAngleOut.SetValue(curindex, 1 - ((blb.getAngle()) / (Math.PI + Math.PI)));
                FMovementXOut.SetValue(curindex, blb.getXSpeed());
                FMovementYOut.SetValue(curindex, blb.getYSpeed());
                FMotionAccelerationOut.SetValue(curindex, blb.getMotionAccel());
                FRotationAccelerationOut.SetValue(curindex, blb.getRotationAccel());
                FMotionSpeedOut.SetValue(curindex, blb.getMotionSpeed());
                FRotationSpeedOut.SetValue(curindex, blb.getRotationSpeed());
                curindex++;
            }

        }

        #endregion mainloop
    }
}
