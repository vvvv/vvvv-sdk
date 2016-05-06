//use what you need
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.SkeletonInterfaces;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class GetJoint: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private INodeIn FSkeletonInput;
    	private IStringIn FJointNameInput;
    	
    	private IStringOut FParentNameOutput;
    	private IValueOut FJointIdOutput;
    	private ITransformOut FBaseTransformOutput;
    	private ITransformOut FAnimationTransformOutput;
    	
    	private Skeleton FSkeleton;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
        
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
	        	
        		FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");
        		
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
        ~GetJoint()
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
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "GetJoint";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "GetJoint";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "certainly";
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
            var guids = new System.Guid[1];
            guids[0] = SkeletonNodeIO.GUID;
            FHost.CreateNodeInput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonInput);
	    	FSkeletonInput.SetSubType(guids, "Skeleton");
	    	FHost.CreateStringInput("Joint Name", TSliceMode.Dynamic, TPinVisibility.True, out FJointNameInput);
	    	
	    	// create outputs
	    	FHost.CreateValueOutput("ID", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FJointIdOutput);
	    	FJointIdOutput.SetSubType(0, 500, 1, 0, false, false, true);
	    	FHost.CreateStringOutput("Parent Name", TSliceMode.Dynamic, TPinVisibility.True, out FParentNameOutput);
	    	FHost.CreateTransformOutput("Base Transform", TSliceMode.Dynamic, TPinVisibility.True, out FBaseTransformOutput);
	    	FHost.CreateTransformOutput("Animation Transform", TSliceMode.Dynamic, TPinVisibility.True, out FAnimationTransformOutput);
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
        	
        	if (FSkeletonInput.PinIsChanged)
        	{
        		if (FSkeletonInput.IsConnected)
        		{
	        		object currInterface;
	        		FSkeletonInput.GetUpstreamInterface(out currInterface);
	        		FSkeleton = (Skeleton)currInterface;
        		}
        		else
        			FSkeleton = null;
        	}
        	
        	if (FSkeletonInput.PinIsChanged || FJointNameInput.PinIsChanged)
        	{
        		if (FSkeleton!=null)
        		{
        			string jointName;
        			IJoint currJoint;
        			FParentNameOutput.SliceCount = FJointNameInput.SliceCount;
        			FBaseTransformOutput.SliceCount = FJointNameInput.SliceCount;
        			FAnimationTransformOutput.SliceCount = FJointNameInput.SliceCount;
        			for (int i=0; i<FJointNameInput.SliceCount; i++)
        			{
        				
        				FJointNameInput.GetString(i, out jointName);
        				if (FSkeleton.JointTable.ContainsKey(jointName))
        				{
	        				currJoint = (IJoint)FSkeleton.JointTable[jointName];
	        				if (currJoint.Parent!=null)
	        					FParentNameOutput.SetString(i, currJoint.Parent.Name);
	        				else
	        					FParentNameOutput.SetString(i, "");
	
	    					FJointIdOutput.SetValue(i, currJoint.Id);
	    					FBaseTransformOutput.SetMatrix(i, VMath.Rotate(currJoint.Rotation) * currJoint.BaseTransform);
	        				FAnimationTransformOutput.SetMatrix(i, currJoint.AnimationTransform);
        				}
        				else
        				{
        					FJointIdOutput.SetValue(i, -1);
        					FBaseTransformOutput.SetMatrix(i, VMath.IdentityMatrix);
	        				FAnimationTransformOutput.SetMatrix(i, VMath.IdentityMatrix);
        				}
        			}
        		}
        	}
        }
        
        #endregion mainloop

    }
}
