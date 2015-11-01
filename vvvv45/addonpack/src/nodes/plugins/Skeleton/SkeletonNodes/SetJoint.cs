//use what you need
using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.SkeletonInterfaces;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class SetJoint: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private INodeIn FSkeletonInput;
    	private IStringIn FJointNameInput;
    	private IStringIn FParentNameInput;
    	private IValueIn FConstraintsInput;
    	private ITransformIn FBaseTransformInput;
    	private ITransformIn FAnimationTransformInput;
    	//private IValueIn FRotationInput;
    	
    	private INodeOut FSkeletonOutput;
    	
    	private Skeleton inputSkeleton;
    	private Skeleton s;
    	private List<string> jointNames;
    	
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
        ~SetJoint()
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
					FPluginInfo.Name = "SetJoint";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "SetJoint";
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

            var guids = new System.Guid[1];
            guids[0] = SkeletonNodeIO.GUID;

            //create inputs
            FHost.CreateNodeInput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonInput);
	    	FSkeletonInput.SetSubType(guids, "Skeleton");
	    	FHost.CreateStringInput("Parent Name", TSliceMode.Dynamic, TPinVisibility.True, out FParentNameInput);
	    	FHost.CreateValueInput("Constraints", 2, null, TSliceMode.Dynamic, TPinVisibility.False, out FConstraintsInput);
	    	FConstraintsInput.SetSubType2D(-1.0, 1.0, 0.01, -1.0, 1.0, false, false, false);
	    	FHost.CreateTransformInput("Base Transform", TSliceMode.Dynamic, TPinVisibility.True, out FBaseTransformInput);
	    	FHost.CreateTransformInput("Animation Transform", TSliceMode.Dynamic, TPinVisibility.True, out FAnimationTransformInput);
	    	FHost.CreateStringInput("Joint Name", TSliceMode.Dynamic, TPinVisibility.True, out FJointNameInput);
	    	
	    	// create outputs
	    	FHost.CreateNodeOutput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonOutput);
	    	FSkeletonOutput.SetSubType(guids, "Skeleton");
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
        	
        	bool recalculate = false;
        	
        	if (FJointNameInput.PinIsChanged)
        	{
        		jointNames = new List<string>();
        		string jointName;
        		for (int i=0; i<FJointNameInput.SliceCount; i++)
        		{
        			FJointNameInput.GetString(i, out jointName);
        			jointNames.Add(jointName);
        		}
        		recalculate = true;
        	}
        	
        	if (FSkeletonInput.PinIsChanged || recalculate)
        	{
        		if (FSkeletonInput.IsConnected)
        		{
	        		object currInterface;
	        		FSkeletonInput.GetUpstreamInterface(out currInterface);
	        		s = (Skeleton)currInterface;
	        		if (inputSkeleton==null || !s.Uid.Equals(inputSkeleton.Uid))
	        		{
	        			inputSkeleton = (Skeleton)s.DeepCopy();
	        		}
	        		else
	        		{
	        			foreach (KeyValuePair<string, IJoint> pair in s.JointTable)
	        			{
	        				if (!jointNames.Exists(delegate(string name) {return name==pair.Key;}))
	        				{
		        				inputSkeleton.JointTable[pair.Key].BaseTransform = pair.Value.BaseTransform;
		        				inputSkeleton.JointTable[pair.Key].AnimationTransform = pair.Value.AnimationTransform;
	        				}
	        				inputSkeleton.JointTable[pair.Key].Constraints = pair.Value.Constraints;
	        			}
	        		}
        		}
        		else
        			inputSkeleton = null;
        	}
        	
        	if (FSkeletonInput.PinIsChanged || FAnimationTransformInput.PinIsChanged || FBaseTransformInput.PinIsChanged || FParentNameInput.PinIsChanged || FConstraintsInput.PinIsChanged || recalculate)
        	{
        		if (inputSkeleton!=null)
        		{
	        		IJoint currJoint;
	        		for (int i=0; i<jointNames.Count; i++)
	        		{
	        			currJoint = inputSkeleton.JointTable[jointNames[i]];
	        			Matrix4x4 currAnimationT;
	        			Matrix4x4 currBaseT;
	        			string currParentName;
	        			if (currJoint!=null)
	        			{
	        				if (FAnimationTransformInput.IsConnected)
	        				{
		        				FAnimationTransformInput.GetMatrix(i, out currAnimationT);
		        				currJoint.AnimationTransform = currAnimationT;
	        				}
	        				else
	        					currJoint.AnimationTransform = s.JointTable[currJoint.Name].AnimationTransform;
	        				
	        				if (FBaseTransformInput.IsConnected)
	        				{
		        				FBaseTransformInput.GetMatrix(i, out currBaseT);
		        				currJoint.BaseTransform = currBaseT;
	        				}
	        				else
	        					currJoint.BaseTransform = s.JointTable[currJoint.Name].BaseTransform;
	        				
	        				/*if (FRotationInput.IsConnected)
	        				{
	        					currRotation = new Vector3D(0);
	        					FRotationInput.GetValue3D(i, out currRotation.x, out currRotation.y, out currRotation.z);
	        					//currJoint.Rotation = currRotation;
	        					
	        				}
	        				*/
	        				
	        				/*if (FConstraintsInput.IsConnected)
	        				{
	        					currConstraints = new List<Vector2D>();
	        					Vector2D currConstraint;
	        					for (int j=0; j<3; j++)
	        					{
	        						currConstraint = new Vector2D(0);
	        						FConstraintsInput.GetValue2D(i*3, out currConstraint.x, out currConstraint.y);
	        						currConstraints.Add(currConstraint);
	        					}
	        					currJoint.Constraints = currConstraints;
	        				}
	        				*/
	        				
	        				FParentNameInput.GetString(i, out currParentName);
	        				if (currParentName!=null && (FParentNameInput.SliceCount>1 || !string.IsNullOrEmpty(currParentName)))
	        				{
	        					
	        					IJoint parent = inputSkeleton.JointTable[currParentName];
	        					if (parent!=null)
	        					{
	        						currJoint.Parent = parent;
	        					}
	        				}
	        			
	        			}
	        		}
        		}
        		
        		FSkeletonOutput.MarkPinAsChanged();
        	}
        	
        	FSkeletonOutput.SetInterface(inputSkeleton);
        }
             
        #endregion mainloop  
	}
}
