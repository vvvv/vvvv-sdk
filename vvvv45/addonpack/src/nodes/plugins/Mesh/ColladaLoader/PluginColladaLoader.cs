#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

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

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using ColladaSlimDX.ColladaDocument;
using ColladaSlimDX.ColladaPipeline;
using ColladaSlimDX.ColladaModel;
using ColladaSlimDX.Utils;

using SlimDX;
using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class PluginColladaLoader: IPlugin, IDisposable, IColladaModelNodeIO
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IStringIn FFileNameInput;
    	
    	//config pin declaration
    	private IEnumConfig FCsSourceTypeConfig;
    	private IEnumConfig FCsTargetTypeConfig;
    	private IEnumConfig FUpAxisSourceConfig;
    	private IEnumConfig FUpAxisTargetConfig;
    	private IEnumConfig FRightAxisSourceConfig;
    	private IEnumConfig FRightAxisTargetConfig;
    	private IValueConfig FMeterSourceConfig;
    	private IValueConfig FMeterTargetConfig;
    	
    	//output pin declaration
    	private INodeOut FColladaModelOutput;
    	private IStringOut FInfoOutput;
    	
    	private Document FColladaDocument;
    	private Model FColladaModel;
    	private List<string> FInfo;
    	private bool FInfoNeedsUpdate = false;
    	private int FCsTypeSource = 0;
    	private int FCsTypeTarget = 0;
    	private int FUpAxisSource = 0;
    	private int FUpAxisTarget = 0;
    	private int FRightAxisSource = 0;
    	private int FRightAxisTarget = 0;
    	private double FDistanceUnitSource = 0;
    	private double FDistanceUnitTarget = 0;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginColladaLoader()
        {
			//the nodes constructor
			FInfo = new List<string>();
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
        			FColladaDocument = null;
        			FColladaModel = null;
        			FInfo = null;
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "PluginColladaLoader is being deleted");
        		
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
        ~PluginColladaLoader()
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
					FPluginInfo.Name = "ColladaFile";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9.Geometry";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Loads a COLLADA *.dae file.";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "Collada,dae,load,read";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "https://collada.org/public_forum/viewtopic.php?t=676";
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
	    	FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out FFileNameInput);
			FFileNameInput.SetSubType("", true);
			
			//create configuration inputs
			FHost.CreateEnumConfig("Coordinate system of source", TSliceMode.Single, TPinVisibility.True, out FCsSourceTypeConfig);
			FCsSourceTypeConfig.SetSubType("CoordType");
			FHost.UpdateEnum("CoordType", "Default", new string[] { "Default", "LeftHanded", "RightHanded" });
			FHost.CreateEnumConfig("Source up axis", TSliceMode.Single, TPinVisibility.True, out FUpAxisSourceConfig);
			FUpAxisSourceConfig.SetSubType("Axis");
			FHost.CreateEnumConfig("Source right axis", TSliceMode.Single, TPinVisibility.True, out FRightAxisSourceConfig);
			FRightAxisSourceConfig.SetSubType("Axis");
			FHost.CreateValueConfig("Source distance unit in meter", 1, null, TSliceMode.Single, TPinVisibility.True, out FMeterSourceConfig);
			FMeterSourceConfig.SetSubType(0, double.MaxValue, 0.1, FDistanceUnitSource, false, false, false);
			
			FHost.CreateEnumConfig("Coordinate system of target", TSliceMode.Single, TPinVisibility.True, out FCsTargetTypeConfig);
			FCsTargetTypeConfig.SetSubType("CoordType");
			FHost.CreateEnumConfig("Target up axis", TSliceMode.Single, TPinVisibility.True, out FUpAxisTargetConfig);
			FUpAxisTargetConfig.SetSubType("Axis");
			FHost.UpdateEnum("Axis", "Default", new string[] { "Default", "X", "Y", "Z", "-X", "-Y", "-Z" });
			FHost.CreateEnumConfig("Target right axis", TSliceMode.Single, TPinVisibility.True, out FRightAxisTargetConfig);
			FRightAxisTargetConfig.SetSubType("Axis");
			FHost.CreateValueConfig("Target distance unit in meter", 1, null, TSliceMode.Single, TPinVisibility.True, out FMeterTargetConfig);
			FMeterTargetConfig.SetSubType(0, double.MaxValue, 0.1, FDistanceUnitTarget, false, false, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateNodeOutput("COLLADA Model", TSliceMode.Dynamic, TPinVisibility.True, out FColladaModelOutput);
	    	FColladaModelOutput.SetSubType(new Guid[1]{ColladaModelNodeIO.GUID}, ColladaModelNodeIO.FriendlyName);
	    	FColladaModelOutput.SetInterface(this);
	    	
	    	FHost.CreateStringOutput("Info", TSliceMode.Dynamic, TPinVisibility.True, out FInfoOutput);
	    	
	    	COLLADAUtil.Logger = new LoggerWrapper(FHost);
        } 

        #endregion pin creation
        
        #region IColladaModelNodeIO
		public void GetSlice(int Index, out Model ColladaModel)
		{
			ColladaModel = FColladaModel;
		}
		
        #endregion
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
    		if (Input == FCsSourceTypeConfig)
				FCsSourceTypeConfig.GetOrd(0, out FCsTypeSource);
    		else if (Input == FCsTargetTypeConfig)
				FCsTargetTypeConfig.GetOrd(0, out FCsTypeTarget);
    		else if(Input == FMeterSourceConfig)
				FMeterSourceConfig.GetValue(0, out FDistanceUnitSource);
    		else if(Input == FMeterTargetConfig)
				FMeterTargetConfig.GetValue(0, out FDistanceUnitTarget);
    		else if (Input == FUpAxisSourceConfig)
    			FUpAxisSourceConfig.GetOrd(0, out FUpAxisSource);
    		else if (Input == FUpAxisTargetConfig)
    			FUpAxisTargetConfig.GetOrd(0, out FUpAxisTarget);
    		else if (Input == FRightAxisSourceConfig)
    			FRightAxisSourceConfig.GetOrd(0, out FRightAxisSource);
    		else if (Input == FRightAxisTargetConfig)
    			FRightAxisTargetConfig.GetOrd(0, out FRightAxisTarget);
    		
    		ConfigurateModel();
        }
        
        private void ConfigurateModel()
        {
        	if (FColladaDocument == null) return;
        	if (FColladaModel == null) return;
        	
        	CoordinateSystem csSource = new CoordinateSystem(FColladaDocument.CoordinateSystem);
        	CoordinateSystem csTarget = new CoordinateSystem(CoordinateSystemType.LeftHanded);
        	
        	if (FCsTypeSource == 1)
        		csSource.Type = CoordinateSystemType.LeftHanded;
        	else if (FCsTypeSource == 2)
        		csSource.Type = CoordinateSystemType.RightHanded;
        	
        	if (FCsTypeTarget == 1)
        		csTarget.Type = CoordinateSystemType.LeftHanded;
        	else if (FCsTypeTarget == 2)
        		csTarget.Type = CoordinateSystemType.RightHanded;
        	
        	if (FUpAxisSource > 0)
        		csSource.Up = GetVectorForAxis(FUpAxisSource);
        	if (FUpAxisTarget > 0)
        		csTarget.Up = GetVectorForAxis(FUpAxisTarget);
        	if (FRightAxisSource > 0)
        		csSource.Right = GetVectorForAxis(FRightAxisSource);
        	if (FRightAxisTarget > 0)
        		csTarget.Right = GetVectorForAxis(FRightAxisTarget);
        	
        	if (FDistanceUnitSource > 0)
        		csSource.Meter = (float) FDistanceUnitSource;
        	if (FDistanceUnitTarget > 0)
        		csTarget.Meter = (float) FDistanceUnitTarget;
        	
        	FColladaModel.CoordinateSystemSource = csSource;
        	FColladaModel.CoordinateSystemTarget = csTarget;
        	
        	GenerateInfoStrings();
        }
        
        private Vector3 GetVectorForAxis(int axis)
        {
    		if (axis == 1)
    			return new Vector3(1f, 0f, 0f);
    		else if (axis == 2)
    			return new Vector3(0f, 1f, 0f);
    		else if (axis == 3)
    			return new Vector3(0f, 0f, 1f);
    		else if (axis == 4)
    			return new Vector3(-1f, 0f, 0f);
    		else if (axis == 5)
    			return new Vector3(0f, -1f, 0f);
    		else
    			return new Vector3(0f, 0f, -1f);
        }
        
        private string VectorToString(Vector3 v)
        {
        	if (v.X == 1) return "X";
        	if (v.Y == 1) return "Y";
        	if (v.Z == 1) return "Z";
        	if (v.X == -1) return "-X";
        	if (v.Y == -1) return "-Y";
        	return "-Z";
        }
        
        private void GenerateInfoStrings()
        {
        	FInfoNeedsUpdate = true;
        	FInfo.Clear();
        	
        	if (FColladaModel == null)
        	{
        		return;
        	}
        	
			FInfo.Add("Source up axis: " + VectorToString(FColladaModel.CoordinateSystemSource.Up));
			FInfo.Add("Source right axis: " + VectorToString(FColladaModel.CoordinateSystemSource.Right));
			FInfo.Add("Source in axis: " + VectorToString(FColladaModel.CoordinateSystemSource.Inward));
			FInfo.Add("Source distance unit in meter: " + FColladaModel.CoordinateSystemSource.Meter);
			FInfo.Add("Target up axis: " + VectorToString(FColladaModel.CoordinateSystemTarget.Up));
			FInfo.Add("Target right axis: " + VectorToString(FColladaModel.CoordinateSystemTarget.Right));
			FInfo.Add("Target in axis: " + VectorToString(FColladaModel.CoordinateSystemTarget.Inward));
			FInfo.Add("Target distance unit in meter: " + FColladaModel.CoordinateSystemTarget.Meter);
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {     	
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FFileNameInput.PinIsChanged)
			{
        		FColladaDocument = null;
        		FColladaModel = null;
        		FColladaModelOutput.SliceCount = 0;
        		FInfoOutput.SliceCount = 0;
        		
        		string filename;
    			FFileNameInput.GetString(0, out filename);
    			if (filename.Length == 0) 
    			{
    				FColladaModelOutput.SliceCount = 0;
    				return;
    			}
    			
    			Log(TLogType.Message, "Loading " + filename);
    			try
				{
					FColladaDocument = new Document(filename);
					Conditioner.ConvexTriangulator(FColladaDocument);
					// not necessary anymore
					//Conditioner.Reindexor(colladaDocument);
					FColladaModel = new Model(FColladaDocument);
					ConfigurateModel();
					
					Log(TLogType.Message, filename + " loaded.");
				}
				catch (Exception e)
				{
					Log(TLogType.Error, e.Message);
					Log(TLogType.Debug, e.StackTrace);
				}
				
				FColladaModelOutput.SliceCount = 1;
			}    
        	
        	if (FInfoNeedsUpdate)
        	{
        		FInfoNeedsUpdate = false;
        		FInfoOutput.SliceCount = FInfo.Count;
					for (int i = 0; i< FInfo.Count; i++)
						FInfoOutput.SetString(i, FInfo[i]);
        	}
        		
        }
             
        #endregion mainloop  
        
        #region helper functions
		private void Log(TLogType logType, string message)
		{
			FHost.Log(logType, "ColladaLoader: " + message);
		}
        #endregion
	}
}
