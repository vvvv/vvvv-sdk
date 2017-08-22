//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.SkeletonInterfaces;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class AutoSkinWeights: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private IValueFastIn FVerticesInput;
    	private IValueFastIn FApplyInput;
    	private INodeIn FSkeletonInput;
    	
    	private IValueOut FSkinWeightsOutput;
    	private IValueOut FBindIndicesOutput;
    	private IValueOut FIndicesOutput;
    	
    	private Skeleton FSkeleton;
    	private List<Vector3D> FVertices = new List<Vector3D>();
    	private Dictionary<int, Dictionary<int, double>> FSkinWeights = new Dictionary<int, Dictionary<int, double>>();
    	
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
        ~AutoSkinWeights()
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
					FPluginInfo.Name = "AutoSkinWeights";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "AutoSkinWeights";
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
	    	String[] dimensions = new String[3];
	    	FHost.CreateValueFastInput("Vertices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVerticesInput);

            var guids = new System.Guid[1];
            guids[0] = SkeletonNodeIO.GUID;
            FHost.CreateNodeInput("Skeleton", TSliceMode.Dynamic, TPinVisibility.True, out FSkeletonInput);
	    	FSkeletonInput.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateValueFastInput("Apply", 1, null, TSliceMode.Single, TPinVisibility.True, out FApplyInput);
	    	FApplyInput.SetSubType(0,1,1,0,true, false, true);
	    	
	    	// create outputs
			FHost.CreateValueOutput("Bind Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBindIndicesOutput);
	    	FHost.CreateValueOutput("Skin Weights", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSkinWeightsOutput);
	    	FHost.CreateValueOutput("Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIndicesOutput);
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
        	
        	if (FVertices.Count != FVerticesInput.SliceCount / 3)
        	{
        		FVertices.Clear();
        		double x, y, z;
	        	for (int i = 0; i < FVerticesInput.SliceCount - 2; i += 3)
	        	{
	        		FVerticesInput.GetValue(i, out x);
	        		FVerticesInput.GetValue(i+1, out y);
	        		FVerticesInput.GetValue(i+2, out z);
	        		FVertices.Add(new Vector3D(x,y,z));
	        	}
        	}
        	
        	if (FSkeletonInput.PinIsChanged)
        	{
        		if (FSkeletonInput.IsConnected)
        		{
	        		object currInterface;
	        		FSkeletonInput.GetUpstreamInterface(out currInterface);
	        		FSkeleton = (Skeleton)currInterface;
        		}
        	}

        	double apply;
        	FApplyInput.GetValue(0, out apply);
        	if (apply == 1 && FSkeleton != null)
        	{
        		FSkeleton.CalculateCombinedTransforms();
                FSkinWeights.Clear();
        		Vector3D origin = new Vector3D(0);
        		double d;
        		for (int i = 0; i < FVertices.Count; i++)
        		{
        			IJoint nearest = getNearestBone(FVertices[i], FSkeleton.Root, out d);
        			Dictionary<int, double> jointWeights = new Dictionary<int, double>();
        			jointWeights.Add(nearest.Id, 1.0);
        			FSkinWeights.Add(i, jointWeights);
        			
        		}
        	
	        	int sliceNum = 0;
				for (int i = 0; i < FVertices.Count; i++)
				{
					if (!FSkinWeights.ContainsKey(i))
						continue;
					IDictionaryEnumerator boneEnum = FSkinWeights[i].GetEnumerator();
					while (boneEnum.MoveNext())
					{
						FIndicesOutput.SliceCount = sliceNum+1;
		    			FBindIndicesOutput.SliceCount = sliceNum+1;
		    			FSkinWeightsOutput.SliceCount = sliceNum+1;
						FIndicesOutput.SetValue(sliceNum, i);
						FBindIndicesOutput.SetValue(sliceNum, (int)boneEnum.Key);
						FSkinWeightsOutput.SetValue(sliceNum, (double)boneEnum.Value);
						sliceNum++;
					}
				}
        	}
        }
             
        #endregion mainloop  
        
        #region helper
        
        private IJoint getNearestBone(Vector3D pos, IJoint currJoint, out double nearestDistance)
        {
        	double distBone = 0.0;
        	Vector3D origin = new Vector3D(0);
        	Matrix4x4 t = currJoint.CombinedTransform;
        	Vector3D p1 = t * origin;
        	
        	distBone = VMath.Dist(pos, p1);
        	
        	for (int i=0; i<currJoint.Children.Count; i++)
        	{
        		bool outside = false;
        		t = currJoint.Children[i].CombinedTransform;
	        	Vector3D p2 = t * origin;
	        	
	        	Vector3D v = p2 - p1;
	        	Vector3D w = pos - p1;
	        	double angle = (v.x*w.x + v.y*w.y + v.z*w.z) / (Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z)*Math.Sqrt(w.x*w.x + w.y*w.y + w.z*w.z));
	        	angle = Math.Acos(angle);
	        	if (Math.Abs(angle)>3.14/2)
	        	{
	        		distBone = VMath.Min(distBone, VMath.Dist(pos, p1));
	        		outside = true;
	        	}
	        	
	        	Vector3D v_ = p1 - p2;
	        	Vector3D w_ = pos - p2;
	        	angle = (v_.x*w_.x + v_.y*w_.y + v_.z*w_.z) / (Math.Sqrt(v_.x*v_.x + v_.y*v_.y + v_.z*v_.z)*Math.Sqrt(w_.x*w_.x + w_.y*w_.y + w_.z*w_.z));
	        	angle = Math.Acos(angle);
	        	if (Math.Abs(angle)>3.14/2)
	        	{
	        		distBone = VMath.Min(distBone, VMath.Dist(pos, p2));
	        		outside = true;
	        	}
	        	
	        	if (!outside)
	        	{
		        	Vector3D vxw = new Vector3D();
		        	vxw.x = v.y*w.z - v.z*w.y;
		        	vxw.y = v.z*w.x - v.x*w.z;
		        	vxw.z = v.x*w.y - v.y*w.x;
		        	distBone = VMath.Min(distBone, System.Math.Sqrt(vxw.x*vxw.x + vxw.y*vxw.y + vxw.z*vxw.z) / System.Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z));
	        	}
        	}

		    nearestDistance = 1000000;
		    IJoint nearestChild = currJoint;
		    for (int i=0; i<currJoint.Children.Count; i++)
		    {
		    	double childDistance;
		    	IJoint candidate = getNearestBone(pos, (IJoint)currJoint.Children[i], out childDistance);
		    	if (childDistance<nearestDistance)
		    	{
		    		nearestChild = candidate;
		    		nearestDistance = childDistance;
		    	}
		    }
		    
		    if (nearestDistance <= distBone)
		    	return nearestChild;
		    else
		    {
		    	nearestDistance = distBone;
        		return currJoint;
		    }
        }
  
        #endregion helper
	}
}
