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
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
   		
   		private Dictionary<int, int> pointFlagMap = new Dictionary<int, int>();
   		
   		private bool ldRunning = false;
   		private int ldStatus = 0;
   		private int ldVersion = 0;
   		private int ldMaxFrames = 6000;
   		private int ldMaxPoints = 6000;
   		private int ldMaxBuffer = 8192;
   		private int ldUndoFrames = 0;
   		
   		private IValueFastIn FXIn;
   		private IValueFastIn FYIn;
   		private IColorIn FColorIn;
   		private IValueFastIn FCornerIn;
   		private IValueFastIn FTravelBlankIn;
   		private IValueFastIn FBinSizeIn;
   		private IValueFastIn FFrameIn;
   		private IValueFastIn FZoneIn;
   		private IValueFastIn FIsVectorFrameIn;
   		private IValueIn FDoWriteIn;
   		private IValueConfig FScanRateIn;
   		private IStringOut FStatusOut;
   		
   		private const string STATUS_MSG_NOT_RUNNING = "LD2000 not running.";
   		private const int FLAG_CORNER = 1;
   		private const int FLAG_TRAVELBLANK = 2;

    	#endregion field declaration
		
    	#region constructor/destructor
    	
        public LD2000Node()
        {
			//the nodes constructor

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
	    	FHost.CreateValueFastInput("BinSize", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSizeIn);
	    	FBinSizeIn.SetSubType(int.MinValue, int.MaxValue, 1.0, -1.0, false, false, true);
	    	FHost.CreateValueFastInput("Frame", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFrameIn);
	    	FFrameIn.SetSubType(1.0, int.MaxValue, 1.0, 1.0, false, false, true);
	    	FHost.CreateValueFastInput("Projection Zone", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FZoneIn);
	    	FZoneIn.SetSubType(1.0, 30.0, 1.0, 1.0, false, false, true);
	    	FHost.CreateValueFastInput("Is Vector Frame", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsVectorFrameIn);
	    	FIsVectorFrameIn.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    	FHost.CreateValueInput("Do Write", 1, null, TSliceMode.Single, TPinVisibility.True, out FDoWriteIn);
	    	FDoWriteIn.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    	
			
			//create configuration inputs
			FHost.CreateValueConfig("Scan Rate", 1, null, TSliceMode.Single, TPinVisibility.True, out FScanRateIn);
			FScanRateIn.SetSubType(25.0, 130000.0, 1.0, 30000.0, false, false, true);
	    	
	    	//create outputs	
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
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (!ldRunning) 
        	{
        		PrintStatusMessage(STATUS_MSG_NOT_RUNNING);
        		return;
        	}
        	
        	double tmp;
        	double* px, py;
        	int v, countx, county;
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
        		int pointCount = Math.Max(Math.Max(FXIn.SliceCount, FYIn.SliceCount), FColorIn.SliceCount);
        		
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
        		
        		int binOffset = 0;
        		for (int j = 0; j < frames.Length; j++)
        		{
        			LD.SetWorkingFrame(frames[j]);
        			
        			// Get the bin size for this frame
        			FBinSizeIn.GetValue(j, out tmp);
        			int binSize = (int) tmp;
        			if (binSize < 0)
        			{
        				binSize = pointCount / Math.Abs(binSize);
        			}
        			
        			// Create binSize many points
        			LD.FrameEx frame = new LD.FrameEx();
	        		frame.NumPoints = binSize;
	        		
	        		LD.Point[] point = new LD.Point[frame.NumPoints];
	        		FXIn.GetValuePointer(out countx, out px);
	        		FYIn.GetValuePointer(out county, out py);
	        		
	        		int flag;
	        		for (int i = 0; i < point.Length; i++)
		        	{
	        			pointId = i + binOffset;
	        			
		        		point[i] = new LD.Point();
		        		v = (int) (*(px + (pointId % countx)) * 8000);
		        		point[i].X = v;
		        		
		        		v = (int) (*(py + (pointId % county)) * 8000);
		        		point[i].Y = v;
		        		
		        		FColorIn.GetColor(pointId, out color);
		        		point[i].Color = color.Color.ToArgb();
		        		
		        		if (pointFlagMap.TryGetValue(pointId, out flag))
		        		{
		        			if ((flag & FLAG_CORNER) > 0)
		        				point[i].VOtype = LD.PT_CORNER;
		        			if ((flag & FLAG_TRAVELBLANK) > 0)
		        				point[i].VOtype |= LD.PT_TRAVELBLANK;
		        		}
		        	}
	        		
	        		// Get projection zone for this frame
	        		FZoneIn.GetValue(j, out tmp);
	        		frame.PreferredProjectionZone = VMath.Zmod((int) (tmp -1), 30);
	        		
	        		// Is this a vector orientated frame?
	        		FIsVectorFrameIn.GetValue(j, out tmp);
	        		frame.VectorFlag = tmp > 0 ? 1 : 0;
	        		
	        		if (point.Length > 0)
	        			LD.WriteFrameFastEx(ref frame, ref point[0]);
	        		else 
	        		{
	        			LD.Point blackPoint = new LD.Point();
	        			LD.WriteFrameFastEx(ref frame, ref blackPoint);
	        		}
	        		
	        		binOffset += binSize;
        		}
        	}
        	
        	for (int i = 0; i < frames.Length; i++)
        	{
        		LD.DisplayFrame(frames[i]);
		    	LD.DisplayUpdate();
        	}
        }
             
        #endregion mainloop 
        
        #region helper functions
        
        private void PrintStatusMessage(string msg)
        {
        	FStatusOut.SetString(0, msg);
        }
        
        #endregion
        
        #region ld2000
        
        private void RunLD() 
        {
        	if (ldRunning) return;

        	LD.InitialQMCheck(ref ldStatus);
        	if (ldStatus != LD.LDSTATUS_OK) 
        	{
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
        		return;
        	}
        	
        	LD.SetWorkingScanners(-1);
  			LD.SetWorkingTracks(1);
  			LD.SetWorkingFrame(1);
  			
  			ldRunning = true;
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
