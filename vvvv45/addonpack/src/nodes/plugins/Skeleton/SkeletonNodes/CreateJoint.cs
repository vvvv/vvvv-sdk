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
	public class CreateJoint: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private IStringIn FJointNameInput;
    	private IStringIn FParentNameInput;
    	private IValueIn FConstraintsInput;
    	private ITransformIn FBaseTransformInput;
    	private IEnumIn FOffsetModeInput;
    	
    	private INodeOut FSkeletonOutput;

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
        ~CreateJoint()
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
					FPluginInfo.Name = "CreateJoint";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "CreateJoint";
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
	    	FHost.CreateStringInput("Joint Name", TSliceMode.Dynamic, TPinVisibility.True, out FJointNameInput);
	    	FHost.CreateStringInput("Parent Name", TSliceMode.Dynamic, TPinVisibility.True, out FParentNameInput);
	    	FHost.CreateTransformInput("Base Transformation", TSliceMode.Dynamic, TPinVisibility.True, out FBaseTransformInput);
	    	
	    	String[] dimensions = new String[2];
	    	dimensions[0] = "Min";
	    	dimensions[1] = "Max";
            
    		FHost.CreateValueInput("Constraints", 2, dimensions, TSliceMode.Dynamic, TPinVisibility.True,  out FConstraintsInput);
	    	FConstraintsInput.SetSubType2D(-1.0, 1.0, 0.1, -1.0, 1.0, false, false, false);
	    	
	    	String[] offsetModes = new String[2];
	    	offsetModes[0] = "parent";
	    	offsetModes[1] = "world";
	    	FHost.UpdateEnum("OffsetModes", "parent", offsetModes);
	    	FHost.CreateEnumInput("Position relative to", TSliceMode.Single, TPinVisibility.True, out FOffsetModeInput);
	    	FOffsetModeInput.SetSubType("OffsetModes");

            //create outputs
            var guids = new System.Guid[1];
            guids[0] = SkeletonNodeIO.GUID;

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
        	
        	if (FJointNameInput.PinIsChanged || FBaseTransformInput.PinIsChanged || FOffsetModeInput.PinIsChanged || FParentNameInput.PinIsChanged || FConstraintsInput.PinIsChanged || recalculate)
        	{
        		FSkeleton = new Skeleton();
        		
        		int currId = 0;
        		for (int i=0; i<FJointNameInput.SliceCount; i++)
        		{
        			string jointName;
        			FJointNameInput.GetString(i%FJointNameInput.SliceCount, out jointName);
                    if (!string.IsNullOrEmpty(jointName))
                    {
                        string parentName;
                        FParentNameInput.GetString(i % FParentNameInput.SliceCount, out parentName);
                        IJoint currJoint = new JointInfo(jointName);
                        Matrix4x4 baseTransform;
                        FBaseTransformInput.GetMatrix(i % FBaseTransformInput.SliceCount, out baseTransform);
                        currJoint.BaseTransform = baseTransform; //VMath.Translate(basePositionX, basePositionY, basePositionZ);

                        currJoint.Constraints.Clear();
                        for (int j = i * 3; j < i * 3 + 3; j++)
                        {
                            double constraintMin, constraintMax;
                            FConstraintsInput.GetValue2D(j % FConstraintsInput.SliceCount, out constraintMin, out constraintMax);
                            currJoint.Constraints.Add(new Vector2D(constraintMin, constraintMax));
                        }

                        if (string.IsNullOrEmpty(parentName))
                        {
                            if (FSkeleton.Root == null)
                            {
                                FSkeleton.Root = currJoint;
                                currJoint.Id = currId;
                                currId++;
                                FSkeleton.BuildJointTable();
                            }
                        }
                        else
                        {
                            if (FSkeleton.JointTable.ContainsKey(parentName))
                            {
                                currJoint.Parent = FSkeleton.JointTable[parentName];
                                currJoint.Id = currId;
                                currId++;
                            }
                            FSkeleton.BuildJointTable();
                        }
                    }
        		}
        		
        		int positionInWorldSpace = 0;
        		FOffsetModeInput.GetOrd(0, out positionInWorldSpace);
        		if (positionInWorldSpace>0)
    			{
        			List<Vector3D> offsetList = new List<Vector3D>();
    				foreach (KeyValuePair<string, IJoint> pair in FSkeleton.JointTable)
    				{
    					Vector3D worldPos = pair.Value.BaseTransform * (new Vector3D(0));
    					Vector3D parentWorldPos;
    					if (pair.Value.Parent!=null)
    						parentWorldPos = pair.Value.Parent.BaseTransform * (new Vector3D(0));
    					else
    						parentWorldPos = new Vector3D(0);
    					Vector3D offset = worldPos - parentWorldPos;
    					offsetList.Add(offset);
    				}
    				int i=0;
    				foreach (KeyValuePair<string, IJoint> pair in FSkeleton.JointTable)
    				{
    					pair.Value.BaseTransform = VMath.Translate(offsetList[i]);
    					i++;
    				}
    			}

        		FSkeletonOutput.MarkPinAsChanged();
        	}
        	
        	FSkeletonOutput.SetInterface(FSkeleton);
        }

        #endregion mainloop
    }
}
