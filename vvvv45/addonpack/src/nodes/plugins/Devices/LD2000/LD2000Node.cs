using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace LD2000
{
	/// <summary>
	/// Allows vvvv to communicate with Lasershow Designer 2000.
	/// </summary>
	public class LD2000Node : IPlugin, IDisposable
	{
		#region structs
		
		struct DisplayPin {
			public IValueIn Pin;
			public string Name;
			public int ArgPos;
			public int Min;
			public int Max;
			public int Default;
		}
		
		#endregion
		
    	#region field declaration
    	
    	// The host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
   		
   		private Dictionary<int, int> pointFlagMap = new Dictionary<int, int>();
   		private FrameEx workingFrame;
   		private Point[] pointBuffer;
   		private DisplayPin[] FVDPins;
   		private DisplayPin[] FSkewPins;
   		
   		private bool ldRunning = false;
   		private int ldStatus = 0;
   		private int ldVersion = 0;
   		private int ldMaxFrames = 64000;
   		private int ldMaxPoints = 6000;
   		private int ldMaxBuffer = 8192;
   		private int ldUndoFrames = 0;
   		
   		private IValueFastIn FXIn;
   		private IValueFastIn FYIn;
   		private IColorIn FColorIn;
   		private IValueFastIn FCornerIn;
   		private IValueFastIn FTravelBlankIn;
   		private IValueFastIn FFrameSizeIn;
   		private IValueFastIn FFrameIn;
   		private IValueFastIn FFrameScanRateIn;
   		private IValueFastIn FZoneIn;
   		private IValueFastIn FAnimationCountIn;
   		private IValueFastIn FIsVectorFrameIn;
   		private IValueIn FWorkingTrackIn;
   		/*
   		private IValueIn FActiveScannerIn;
   		*/
   		private IValueIn FDoWriteIn;
   		
   		private IValueConfig FScanRateIn;
   		
   		private IValueOut FXOut;
   		private IValueOut FYOut;
   		private IColorOut FColorOut;
   		private IValueOut FCornerOut;
   		private IValueOut FFrameSizeOut;
   		private IValueOut FFrameScanRateOut;
   		private IValueOut FZoneOut;
   		private IValueOut FAnimationCountOut;
   		private IValueOut FIsVectorFrameOut;
   		private IStringOut FStatusOut;
   		
   		private const string STATUS_MSG_NOT_RUNNING = "LD2000 not running.";
   		
   		private const int FLAG_CORNER = 1;
   		private const int FLAG_TRAVELBLANK = 2;
   		
   		private const int MAX_SCANNER_CODE = 1073741824; // 2^30

    	#endregion field declaration
		
    	#region constructor/destructor
    	
        public LD2000Node()
        {
			//the nodes constructor
			workingFrame = new FrameEx();
			pointBuffer = new Point[ldMaxPoints];
			
			// vector display pins
			FVDPins = new DisplayPin[7];
			int i = 0;
	    	FVDPins[i].Name = "Line Beginning Blanked Points";
	    	FVDPins[i].ArgPos = 4;
	    	FVDPins[i].Min = 1;
	    	FVDPins[i].Max = 8;
	    	FVDPins[i++].Default = 3;
	    	FVDPins[i].Name = "Line Beginning Anchor Points";
	    	FVDPins[i].ArgPos = 0;
	    	FVDPins[i].Min = 1;
	    	FVDPins[i].Max = 8;
	    	FVDPins[i++].Default = 2;
	    	FVDPins[i].Name = "Line Corner Anchor Points";
	    	FVDPins[i].ArgPos = 1;
	    	FVDPins[i].Min = 2;
	    	FVDPins[i].Max = 10;
	    	FVDPins[i++].Default = 3;
	    	FVDPins[i].Name = "Line Ending Blanked Points";
	    	FVDPins[i].ArgPos = 5;
	    	FVDPins[i].Min = 1;
	    	FVDPins[i].Max = 8;
	    	FVDPins[i++].Default = 3;
	    	FVDPins[i].Name = "Line Ending Anchor Points";
	    	FVDPins[i].ArgPos = 2;
	    	FVDPins[i].Min = 1;
	    	FVDPins[i].Max = 8;
	    	FVDPins[i++].Default = 2;
	    	FVDPins[i].Name = "Point Spacing Blanked Lines";
	    	FVDPins[i].ArgPos = 6;
	    	FVDPins[i].Min = 100;
	    	FVDPins[i].Max = 4000;
	    	FVDPins[i++].Default = 500;
	    	FVDPins[i].Name = "Point Spacing Visible Lines";
	    	FVDPins[i].ArgPos = 3;
	    	FVDPins[i].Min = 100;
	    	FVDPins[i].Max = 4000;
	    	FVDPins[i++].Default = 250;
	    	
	    	FSkewPins = new DisplayPin[1];
	    	i = 0;
	    	FSkewPins[i].Name = "Color/Blanking Shift";
	    	FSkewPins[i].ArgPos = 0;
	    	FSkewPins[i].Min = 0;
	    	FSkewPins[i].Max = 20;
	    	FSkewPins[i++].Default = 0;
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
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
        		StopLD();
	        	
        		FHost.Log(TLogType.Debug, "LD2000Node is being deleted.");
        		
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
        ~LD2000Node()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
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
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "LD2000";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Devices";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Controls the LD2000 device.";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "ld2000, laser";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
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
        
        #region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateValueFastInput("X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FXIn);
	    	FXIn.SetSubType(-1.0, 1.0, 0.01, 0, false, false, false);
	    	FHost.CreateValueFastInput("Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FYIn);
	    	FYIn.SetSubType(-1.0, 1.0, 0.01, 0, false, false, false);
	    	FHost.CreateColorInput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorIn);
	    	FColorIn.SetSubType(VColor.White, false);
	    	FHost.CreateValueFastInput("Corner", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCornerIn);
	    	FCornerIn.SetSubType(int.MinValue, int.MaxValue, 1.0, 0.0, false, false, true);
	    	FHost.CreateValueFastInput("Travel Blank", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTravelBlankIn);
	    	FTravelBlankIn.SetSubType(int.MinValue, int.MaxValue, 1.0, 0.0, false, false, true);
	    	FHost.CreateValueFastInput("Frame Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFrameSizeIn);
	    	FFrameSizeIn.SetSubType(int.MinValue, int.MaxValue, 1.0, -1.0, false, false, true);
	    	FHost.CreateValueFastInput("Frame", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFrameIn);
	    	FFrameIn.SetSubType(1.0, int.MaxValue, 1.0, 1.0, false, false, true);
	    	FHost.CreateValueFastInput("Projection Zone", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FZoneIn);
	    	FZoneIn.SetSubType(1.0, 30.0, 1.0, 1.0, false, false, true);
	    	FHost.CreateValueFastInput("Frame Scan Rate", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFrameScanRateIn);
	    	FFrameScanRateIn.SetSubType(-130.0, int.MaxValue, 1.0, 100.0, false, false, true);
	    	FHost.CreateValueFastInput("Animation Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAnimationCountIn);
	    	FAnimationCountIn.SetSubType(0.0, int.MaxValue, 1.0, 0.0, false, false, true);
	    	FHost.CreateValueFastInput("Is Vector Frame", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsVectorFrameIn);
	    	FIsVectorFrameIn.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);
	    	
	    	CreatePins(FVDPins);
	    	CreatePins(FSkewPins);
	    	
	    	FHost.CreateValueInput("Working Track", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FWorkingTrackIn);
	    	FWorkingTrackIn.SetSubType(-1.0, int.MaxValue, 1.0, 1.0, false, false, true);
	    	/*
	    	FHost.CreateValueInput("Active Scanner", 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FActiveScannerIn);
	    	FActiveScannerIn.SetSubType(-1.0, int.MaxValue, 1.0, -1.0, false, false, true);
	    	*/
	    	FHost.CreateValueInput("Do Write", 1, null, TSliceMode.Single, TPinVisibility.True, out FDoWriteIn);
	    	FDoWriteIn.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
			
			//create configuration inputs
			FHost.CreateValueConfig("Scan Rate", 1, null, TSliceMode.Single, TPinVisibility.True, out FScanRateIn);
			FScanRateIn.SetSubType(25.0, 130000.0, 1.0, 30000.0, false, false, true);
	    	
	    	//create outputs	
	    	FHost.CreateValueOutput("X", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FXOut);
	    	FXOut.SetSubType(-1.0, 1.0, 0.01, 0, false, false, false);
	    	FHost.CreateValueOutput("Y", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FYOut);
	    	FYOut.SetSubType(-1.0, 1.0, 0.01, 0, false, false, false);
	    	FHost.CreateColorOutput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorOut);
	    	FColorOut.SetSubType(VColor.White, false);
	    	FHost.CreateValueOutput("Corner", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCornerOut);
	    	FCornerOut.SetSubType(int.MinValue, int.MaxValue, 1.0, 0.0, false, false, true);
	    	FHost.CreateValueOutput("Frame Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFrameSizeOut);
	    	FFrameSizeOut.SetSubType(int.MinValue, int.MaxValue, 1.0, -1.0, false, false, true);
	    	FHost.CreateValueOutput("Frame Scan Rate", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FFrameScanRateOut);
	    	FFrameScanRateOut.SetSubType(-130.0, int.MaxValue, 1.0, 100.0, false, false, true);
	    	FHost.CreateValueOutput("Projection Zone", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FZoneOut);
	    	FZoneOut.SetSubType(1.0, 30.0, 1.0, 1.0, false, false, true);
	    	FHost.CreateValueOutput("Animation Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAnimationCountOut);
	    	FAnimationCountOut.SetSubType(0.0, int.MaxValue, 1.0, 0.0, false, false, true);
	    	FHost.CreateValueOutput("Is Vector Frame", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsVectorFrameOut);
	    	FIsVectorFrameOut.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    	FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out FStatusOut);
	    	
	    	RunLD();
        } 

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	if (!ldRunning) 
        	{
        		PrintStatusMessage(STATUS_MSG_NOT_RUNNING);
        		return;
        	}
        	
        	if (Input == FScanRateIn)
        	{
        		double scanRate;
        		FScanRateIn.GetValue(0, out scanRate);
        		
        		int desiredPPS = (int) scanRate;
        		int actualPPS = desiredPPS;
        		LD.DisplayFreq3(desiredPPS, desiredPPS, desiredPPS, ref actualPPS, ref actualPPS, ref actualPPS);
        	}
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        unsafe public void Evaluate(int SpreadMax)
        {     	
        	if (!ldRunning) 
        	{
        		return;
        	} else {
        		PrintStatusMessage("");
        	}
        	
        	double tmp;
        	double* px, py;
        	int v, countx, county;
        	int frameOffset = 0;
        	int pointCount = 0;
        	RGBAColor color;
        	int[] frames = new int[FFrameIn.SliceCount];
        	
        	for (int i = 0; i < frames.Length; i++)
        	{
        		FFrameIn.GetValue(i, out tmp);
        		frames[i] = VMath.Zmod((int) tmp, ldMaxFrames);
        	}
        	
        	FDoWriteIn.GetValue(0, out tmp);
        	if (tmp > 0)
        	{
        		pointCount = Math.Max(Math.Max(FXIn.SliceCount, FYIn.SliceCount), FColorIn.SliceCount);
        		
        		// Setup a mapping of point and its special flags.
        		pointFlagMap.Clear();
        		int pointId;
        		for (int j = 0; j < FCornerIn.SliceCount; j++)
        		{
        			FCornerIn.GetValue(j, out tmp);
        			pointId = VMath.Zmod((int) tmp, pointCount);
        			if (pointFlagMap.ContainsKey(pointId))
        				pointFlagMap[pointId] |= FLAG_CORNER;
        			else
        				pointFlagMap[pointId] = FLAG_CORNER;
        		}
        		for (int j = 0; j < FTravelBlankIn.SliceCount; j++)
        		{
        			FTravelBlankIn.GetValue(j, out tmp);
        			pointId = VMath.Zmod((int) tmp, pointCount);
        			if (pointFlagMap.ContainsKey(pointId))
        				pointFlagMap[pointId] |= FLAG_TRAVELBLANK;
        			else
        				pointFlagMap[pointId] = FLAG_TRAVELBLANK;
        		}
        		
        		for (int j = 0; j < frames.Length; j++)
        		{
        			LD.SetWorkingFrame(frames[j]);
        			
        			// Get the frame size for this frame
        			FFrameSizeIn.GetValue(j, out tmp);
        			int frameSize = (int) tmp;
        			if (frameSize < 0)
        			{
        				frameSize = pointCount / Math.Abs(frameSize);
        			}
        			if (frameSize > pointBuffer.Length)
        			{
        				PrintStatusMessage("Too many points. Maximum number of points for each frame is: " + pointBuffer.Length);
        				frameSize = pointBuffer.Length;
        			}
        			
        			// Create binSize many points
        			workingFrame = new FrameEx();
	        		workingFrame.NumPoints = frameSize;
	        		
	        		FXIn.GetValuePointer(out countx, out px);
	        		FYIn.GetValuePointer(out county, out py);
	        		
	        		int flag;
	        		for (int i = 0; i < frameSize; i++)
		        	{
	        			pointId = i + frameOffset;
	        			
		        		pointBuffer[i] = new Point();
		        		v = (int) (*(px + (pointId % countx)) * 8000);
		        		pointBuffer[i].X = v;
		        		
		        		v = (int) (*(py + (pointId % county)) * 8000);
		        		pointBuffer[i].Y = v;
		        		
		        		FColorIn.GetColor(pointId, out color);
		        		//swapping r'n'b 
		        		var r = color.R;
		        		color.R = color.B;
		        		color.B = r;
		        		pointBuffer[i].Color = color.Color.ToArgb();
		        		
		        		if (pointFlagMap.TryGetValue(pointId, out flag))
		        		{
		        			if ((flag & FLAG_CORNER) > 0)
		        				pointBuffer[i].VOtype = LD.PT_CORNER;
		        			if ((flag & FLAG_TRAVELBLANK) > 0)
		        			{
		        				pointBuffer[i].VOtype |= LD.PT_TRAVELBLANK;
		        				pointBuffer[i].Color = 0;
		        			}
		        		}
		        	}
	        		
	        		// Get projection zone for this frame
	        		FZoneIn.GetValue(j, out tmp);
	        		workingFrame.PreferredProjectionZone = VMath.Zmod((int) (tmp - 1), 30);
	        		
	        		// Is this a vector orientated frame?
	        		FIsVectorFrameIn.GetValue(j, out tmp);
	        		workingFrame.VectorFlag = tmp > 0 ? 1 : 0;
	        		
	        		// Scan rate for this frame
	        		FFrameScanRateIn.GetValue(j, out tmp);
	        		workingFrame.ScanRate = (int) tmp;
	        		
	        		// Set animation count
	        		FAnimationCountIn.GetValue(j, out tmp);
	        		workingFrame.AnimationCount = (int) tmp;
	        		
        			LD.WriteFrameFastEx(ref workingFrame, ref pointBuffer[0]);
	        		
	        		frameOffset += frameSize;
        		}
        	} else {
        		List<int> corners = new List<int>();
        		
        		for (int j = 0; j < frames.Length; j++)
        		{
        			LD.SetWorkingFrame(frames[j]);
        			
        			int numPoints = 0;
        			LD.ReadNumPoints(ref numPoints);
        			
        			pointCount += numPoints;
        		}
        		
        		FXOut.SliceCount = pointCount;
        		FYOut.SliceCount = pointCount;
        		FColorOut.SliceCount = pointCount;
        		FFrameSizeOut.SliceCount = frames.Length;
        		FFrameScanRateOut.SliceCount = frames.Length;
        		FIsVectorFrameOut.SliceCount = frames.Length;
        		FZoneOut.SliceCount = frames.Length;
        		FAnimationCountOut.SliceCount = frames.Length;
        		
        		for (int j = 0; j < frames.Length; j++)
        		{
        			LD.SetWorkingFrame(frames[j]);
        			
        			int frameSize = 0;
        			LD.ReadNumPoints(ref frameSize);
	        		LD.ReadFrameEx(ref workingFrame, ref pointBuffer[0]);
        			
	        		for (int i = 0; i < frameSize; i++)
	        		{
	        			int slice = i + frameOffset;
	        			FXOut.SetValue(slice, ((double) pointBuffer[i].X) / 8000.0);
	        			FYOut.SetValue(slice, ((double) pointBuffer[i].Y) / 8000.0);
	        			RGBAColor c = new RGBAColor();
	        			c.Color = System.Drawing.Color.FromArgb(pointBuffer[i].Color);
	        			FColorOut.SetColor(slice, c);
	        			
	        			// Handle vector orientated flags
	        			if ((pointBuffer[i].VOtype & LD.PT_CORNER) > 0)
	        			{
	        				corners.Add(slice);
	        			}
	        		}
	        		
	        		FCornerOut.SliceCount = corners.Count;
	        		for (int i = 0; i < corners.Count; i++)
	        			FCornerOut.SetValue(i, corners[i]);
	        		
	        		FFrameSizeOut.SetValue(j, frameSize);
	        		FFrameScanRateOut.SetValue(j, workingFrame.ScanRate);
	        		FIsVectorFrameOut.SetValue(j, workingFrame.VectorFlag != 0 ? 1.0 : 0.0);
	        		FZoneOut.SetValue(j, workingFrame.PreferredProjectionZone + 1);
	        		FAnimationCountOut.SetValue(j, workingFrame.AnimationCount);
	        		
	        		frameOffset += frameSize;
        		}
        	}
        	
        	bool vdChanged = PinsChanged(FVDPins);
        	bool skewChanged = PinsChanged(FSkewPins);
        	/*
        	bool scannerChanged = FActiveScannerIn.PinIsChanged;
        	if (scannerChanged)
        	{
        		// Blank out all scanners
        		LD.SetWorkingScanners(-1);
        		LD.DisplayFrame(0);
        	}
        	int scannerFrameCount = Math.Max(frames.Length, FActiveScannerIn.SliceCount);
        	*/
        	int scannerFrameCount = Math.Max(frames.Length, FWorkingTrackIn.SliceCount);
        	for (int i = 0; i < scannerFrameCount; i++)
        	{
        		/*
        		FActiveScannerIn.GetValue(i, out tmp);
        		int scannerCode = (int) tmp;
        		scannerCode = VMath.Clamp(scannerCode, -1, MAX_SCANNER_CODE);
        		if (scannerCode == 0) scannerCode = 1; // 0 is illegal
        		LD.SetWorkingScanners(scannerCode);
        		*/
        		FWorkingTrackIn.GetValue(i, out tmp);
        		int trackCode = (int) tmp;
        		LD.SetWorkingTracks(trackCode);
	        		
        		LD.DisplayFrame(frames[i % frames.Length]);
        		if (vdChanged)
        		{
        			int[] arg = PinsAsArgumentList(FVDPins, i);
        			LD.DisplayObjectSettings(arg[0], arg[1], arg[2], arg[3], arg[4], arg[5], arg[6]);
        		}
        		if (skewChanged)
        		{
        			int[] arg = PinsAsArgumentList(FSkewPins, i);
        			LD.DisplaySkew(4, arg[0], 0);
        		}
        	}
        	
        	/*
        	LD.SetWorkingScanners(-1);
        	*/
        	LD.DisplayUpdate();
        }
             
        #endregion mainloop 
        
        #region helper functions
        
        private void PrintStatusMessage(string msg)
        {
        	FStatusOut.SetString(0, msg);
        }
        
        private void CreatePins(DisplayPin[] pins)
        {
        	for (int i = 0; i < pins.Length; i++)
	    	{
	    		FHost.CreateValueInput(pins[i].Name, 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out pins[i].Pin);
	    		pins[i].Pin.SetSubType(pins[i].Min, pins[i].Max, 1.0, pins[i].Default, false, false, true);
	    	}
        }
        
        private bool PinsChanged(DisplayPin[] pins)
        {
        	for (int i = 0; i < pins.Length; i++)
        	{
        		if (pins[i].Pin.PinIsChanged) return true;
        	}
        	return false;
        }
        
        private int[] PinsAsArgumentList(DisplayPin[] pins, int index)
        {
        	int[] result = new int[pins.Length];
        	double tmp;
        	
        	for (int i = 0; i < result.Length; i++)
        	{
        		pins[i].Pin.GetValue(index, out tmp);
        		int v = (int) tmp;
        		v = VMath.Clamp(v, pins[i].Min, pins[i].Max);
        		result[pins[i].ArgPos] = v;
        	}
        	
        	return result;
        }
        
        #endregion
        
        #region ld2000
        
        private void RunLD() 
        {
        	try
        	{
        		if (ldRunning) return;

        		LD.InitialQMCheck(ref ldStatus);
        		if (ldStatus != LD.LDSTATUS_OK)
        		{
        			PrintStatusMessage("QMCheck failed.");
        			return;
        		}
        		
        		LD.BeginSessionEx(ref ldVersion,
        		                  ref ldMaxFrames,
        		                  ref ldMaxPoints,
        		                  ref ldMaxBuffer,
        		                  ref ldUndoFrames,
        		                  ref ldStatus);
        		if (ldStatus != LD.LDSTATUS_OK)
        		{
        			PrintStatusMessage("BeginSessionEx failed.");
        			return;
        		}
        		
        		LD.SetWorkingScanners(-1);
        		LD.SetWorkingTracks(1);
        		LD.SetWorkingFrame(1);
        		
        		ldRunning = true;
        	}
        	catch (Exception)
        	{
        		ldRunning = false;
        	}
        }
        
        private void StopLD() 
        {
        	if (!ldRunning) return;
        	
  			LD.SetWorkingScanners(-1); // 'Set all scanners
			LD.SetWorkingTracks(-1);   // 'Set all tracks
			LD.DisplayFrame(0);        // 'Basically, we make it so that anything that is being displayed now, on any scanner is blanked out
			LD.SetWorkingTracks(1);    // 'always exit with this set to 1 to avoid bugs
			LD.DisplayUpdate();
        }
        
        #endregion
	}
}
