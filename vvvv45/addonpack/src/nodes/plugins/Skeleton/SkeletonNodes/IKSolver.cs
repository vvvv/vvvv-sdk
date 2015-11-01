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
	public class IKSolver: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private INodeIn FPoseInput;
    	private IStringIn FChainStart;
    	private IStringIn FChainEnd;
    	private IValueIn FTargetInput;
    	private IValueIn FEpsilonInput;
    	private IValueIn FVelocityInput;
    	private IValueIn FPoleTargetInput;
    	private IValueIn FEnablePoleTargetInput;
    	
    	private INodeOut FPoseOutput;
    	private IValueOut FDebugOutput;
    	
    	private Skeleton FOutputSkeleton;
    	private Skeleton FWorkingSkeleton;
    	private string chainStart;
    	private string chainEnd;
    	private Vector3D targetPosW;
    	private Vector3D endPosW;
    	private Vector3D poleTargetW;
    	private List<IJoint> jointChain;
    	private Dictionary<String, Vector3D> rotations;
    	private Matrix4x4 chainRotation = VMath.IdentityMatrix;
        private bool enablePoleTarget = false;
    	private int iterationsPerFrame = 10;
    	private double epsilon = 0.001;
    	
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
        ~IKSolver()
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
					FPluginInfo.Name = "IKSolver";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "IKSolver";
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
            FHost.CreateNodeInput("Pose", TSliceMode.Single, TPinVisibility.True, out FPoseInput);
		    FPoseInput.SetSubType(guids, "Skeleton");
		    FHost.CreateStringInput("Start Joint", TSliceMode.Single, TPinVisibility.True, out FChainStart);
	    	FHost.CreateStringInput("End Joint", TSliceMode.Single, TPinVisibility.True, out FChainEnd);
	    	FHost.CreateValueInput("Target", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FTargetInput);
	    	FHost.CreateValueInput("Epsilon", 1, null, TSliceMode.Single, TPinVisibility.True, out FEpsilonInput);
	    	FHost.CreateValueInput("Velocity", 1, null, TSliceMode.Single, TPinVisibility.True, out FVelocityInput);
	    	FVelocityInput.SetSubType(0.0, 10.0, 0.1, 1.0, false, false, false);
	    	FHost.CreateValueInput("Pole Target", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPoleTargetInput);
	    	FHost.CreateValueInput("Enable Pole Target", 1, null, TSliceMode.Single, TPinVisibility.True, out FEnablePoleTargetInput);
	    	FEnablePoleTargetInput.SetSubType(0.0, 1.0, 1.0, 0.0, false, false, true);
	    	
	    	// create outputs
	    	FHost.CreateNodeOutput("Output Pose", TSliceMode.Single, TPinVisibility.True, out FPoseOutput);
	    	FPoseOutput.SetSubType(guids, "Skeleton");
	    	FHost.CreateValueOutput("Debug", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDebugOutput);
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
        	bool chainRangeChanged = false;
        	bool recalculateOrientation = false;
        	
        	if (FChainStart.PinIsChanged)
        	{
        		FChainStart.GetString(0, out chainStart);
        		recalculate = true;
        		chainRangeChanged = true;
        	}
        	
        	if (FChainEnd.PinIsChanged)
        	{
        		FChainEnd.GetString(0, out chainEnd);
        		recalculate = true;
        		chainRangeChanged = true;
        	}

        	object currInterface;
        	if (FPoseInput.PinIsChanged || chainRangeChanged)
        	{
        		if (FPoseInput.IsConnected)
        		{
	        		FPoseInput.GetUpstreamInterface(out currInterface);
	        		Skeleton s = (Skeleton)currInterface;
	        		if (FOutputSkeleton==null || !s.Uid.Equals(FOutputSkeleton.Uid))
	        		{
		        		FOutputSkeleton = (Skeleton)((Skeleton)currInterface).DeepCopy();
		        		FOutputSkeleton.BuildJointTable();
		        		FWorkingSkeleton = (Skeleton)FOutputSkeleton.DeepCopy();
		        		FWorkingSkeleton.BuildJointTable();
		        		chainRangeChanged = true;
	        		}
	        		else
	        		{
	        			foreach (KeyValuePair<string, IJoint> pair in s.JointTable)
	        			{
	        				if (!jointChain.Exists(delegate(IJoint j) {return j.Name==pair.Key;}))
	        				{
		        				FOutputSkeleton.JointTable[pair.Key].BaseTransform = pair.Value.BaseTransform;
		        				FOutputSkeleton.JointTable[pair.Key].AnimationTransform = pair.Value.AnimationTransform;
		        				FWorkingSkeleton.JointTable[pair.Key].BaseTransform = pair.Value.BaseTransform;
		        				FWorkingSkeleton.JointTable[pair.Key].AnimationTransform = pair.Value.AnimationTransform;
	        				}
	        				FOutputSkeleton.JointTable[pair.Key].Constraints = pair.Value.Constraints;
		        			FWorkingSkeleton.JointTable[pair.Key].Constraints = pair.Value.Constraints;
	        			}
	        		}
	        		FWorkingSkeleton.CalculateCombinedTransforms();
	        		recalculate = true;
        		}
        		else
        			FOutputSkeleton = null;
        	}
        	
        	if (FVelocityInput.PinIsChanged)
        	{
        		double x = 1;
                if (FVelocityInput.SliceCount > 0)
                    FVelocityInput.GetValue(0, out x);
        		iterationsPerFrame = (int)(x*10);
        	}
        	
        	if (iterationsPerFrame > 0)
        	{
	        	if (FTargetInput.PinIsChanged)
	        	{
	        		targetPosW = new Vector3D();
                    if (FTargetInput.SliceCount > 0)
	        		    FTargetInput.GetValue3D(0, out targetPosW.x, out targetPosW.y, out targetPosW.z);
	        		recalculate = true;
	        	}
	        	
	        	if (FEpsilonInput.PinIsChanged)
	        	{
                    if (FEpsilonInput.SliceCount > 0)
                        FEpsilonInput.GetValue(0, out epsilon);
	        		recalculate = true;
	        	}
	        	
	        	if (FPoleTargetInput.PinIsChanged || FEnablePoleTargetInput.PinIsChanged)
	        	{
	        		double x = 0;
                    if (FEnablePoleTargetInput.SliceCount > 0)
                        FEnablePoleTargetInput.GetValue(0, out x);
	        		enablePoleTarget = x>0.0;
	        		poleTargetW = new Vector3D();
                    if (FPoleTargetInput.SliceCount > 0)
                        FPoleTargetInput.GetValue3D(0, out poleTargetW.x, out poleTargetW.y, out poleTargetW.z);
	        		recalculateOrientation = true;
	        	}
	        	
	        	if (chainRangeChanged && FOutputSkeleton!=null)
	        	{
	        		initRotations();
	        	}
	        	
	        	double delta = VMath.Dist(endPosW, targetPosW);
	        	if ((delta>epsilon || recalculate) && FOutputSkeleton!=null && !string.IsNullOrEmpty(chainStart) && !string.IsNullOrEmpty(chainEnd))
	        	{
	        		List<Vector2D> constraints = new List<Vector2D>();
	        		for (int i=0; i<iterationsPerFrame; i++)
	        		{
	        			for (int j=0; j<3; j++)
	        			{
			        		IJoint currJoint = FWorkingSkeleton.JointTable[chainEnd];
			        		endPosW = currJoint.CombinedTransform*new Vector3D(0);
			        		while (currJoint.Name!=chainStart)
			        		{
			        			currJoint = currJoint.Parent;
			        			Vector3D rotationAxis = new Vector3D(0,0,0);
			        			rotationAxis[j] = 1;
			        			double torque = calculateTorque(currJoint, rotationAxis);
			        			Vector3D rot = rotations[currJoint.Name];
			        			if ((rot[j]+torque)<currJoint.Constraints[j].x*2*Math.PI || (rot[j]+torque)>currJoint.Constraints[j].y*2*Math.PI)
			        				torque = 0;
			        			Matrix4x4 newTransform = VMath.Rotate(torque*rotationAxis.x, torque*rotationAxis.y, torque*rotationAxis.z)*currJoint.AnimationTransform;
			        			Vector3D testVec = newTransform*new Vector3D(0);
			        			if (!Double.IsInfinity(testVec.x) && !Double.IsNaN(testVec.x)) // an evil bug fix, to avoid n.def. values in animation transform matrix. the actual reason, why this would happen has not been found yet.
			        			{
				        			rot[j] += torque;
				        			rotations[currJoint.Name] = rot;
				        			currJoint.AnimationTransform = newTransform;
				        			FOutputSkeleton.JointTable[currJoint.Name].AnimationTransform = currJoint.AnimationTransform;
			        			}
			        		}
	        			}
	        			try
	        			{
		        			Matrix4x4 pre;
			        		if (FWorkingSkeleton.JointTable[chainStart].Parent!=null)
			        			pre = FWorkingSkeleton.JointTable[chainStart].Parent.CombinedTransform;
			        		else
			        			pre = VMath.IdentityMatrix;
			        		((JointInfo)FWorkingSkeleton.JointTable[chainStart]).CalculateCombinedTransforms(pre);
	        			}
	        			catch (Exception)
	        			{
	        				FWorkingSkeleton.CalculateCombinedTransforms();
	        			}
	        		}
	        		
	        		
	        		FPoseOutput.MarkPinAsChanged();
	        	}
	        	
	        	if ((recalculate || recalculateOrientation) && enablePoleTarget && FOutputSkeleton!=null && !string.IsNullOrEmpty(chainStart) && !string.IsNullOrEmpty(chainEnd))
	        	{
	        		endPosW = FWorkingSkeleton.JointTable[chainEnd].CombinedTransform*new Vector3D(0);
	        		Vector3D poleTargetLocal =  VMath.Inverse(FWorkingSkeleton.JointTable[chainStart].CombinedTransform)*poleTargetW;
	        		Vector3D t = VMath.Inverse(FWorkingSkeleton.JointTable[chainStart].CombinedTransform)*endPosW; // endpoint in local coords
	        		Vector3D a = VMath.Inverse(FWorkingSkeleton.JointTable[chainStart].CombinedTransform)*(FWorkingSkeleton.JointTable[chainStart].Children[0].CombinedTransform*new Vector3D(0)); // next child in local coords
	        		Vector3D x = t*((a.x*t.x+a.y*t.y+a.z*t.z)/Math.Pow(VMath.Dist(new Vector3D(0), t),2));
	        		Vector3D y = t*((poleTargetLocal.x*t.x+poleTargetLocal.y*t.y+poleTargetLocal.z*t.z)/Math.Pow(VMath.Dist(new Vector3D(0), t),2));
	        		
	        		Vector3D c = poleTargetLocal - y;
	        		Vector3D b = a - x;
	        		double angle = vectorAngle(b,c);
	        		Vector3D n = new Vector3D();
	        		n.x = c.y*b.z-c.z*b.y;
		        	n.y = c.z*b.x-c.x*b.z;
		        	n.z = c.x*b.y-c.y*b.x;
		        	n = n / VMath.Dist(new Vector3D(0), n);
	        		FDebugOutput.SetValue(0, angle);
	        		chainRotation = RotateAroundAxis(angle, n);
	        		
	        		FPoseOutput.MarkPinAsChanged();
	        	}
	        	if (!enablePoleTarget)
	        		chainRotation = VMath.IdentityMatrix;
	        	
                if (FOutputSkeleton != null)
	        	    FOutputSkeleton.JointTable[chainStart].AnimationTransform = chainRotation * FOutputSkeleton.JointTable[chainStart].AnimationTransform;
        	}
        	
        	FPoseOutput.SetInterface(FOutputSkeleton);
        }
             
        #endregion mainloop  
        
        #region helper
        
        private double calculateTorque(IJoint joint, Vector3D axis)
        {
        	Vector3D targetPosLocal = VMath.Inverse(joint.CombinedTransform)*targetPosW;
        	Vector3D endPosLocal = VMath.Inverse(joint.CombinedTransform)*endPosW;
        	
        	if (double.IsInfinity(endPosLocal.x) || double.IsNaN(endPosLocal.x))
        		return 0;
        	
        	Vector3D f = targetPosLocal - endPosLocal;
        	Vector3D a = axis;
        	Vector3D b = endPosLocal;
        	Vector3D r = new Vector3D(0);
        	r.x = a.y*b.z-a.z*b.y;
        	r.y = a.z*b.x-a.x*b.z;
        	r.z = a.x*b.y-a.y*b.x;
        	
        	double torque = VMath.Dist(new Vector3D(0), f) * Math.Sin(vectorAngle(a,f)) * Math.Sin(vectorAngle(b,f)) * Math.Sign(Math.Cos(vectorAngle(r,f))) * 0.03;
        	                                                                                                                     
        	return torque;
        }
        
        private double vectorAngle(Vector3D a, Vector3D b)
        {
        	double adotb = a.x*b.x + a.y*b.y + a.z*b.z;
        	double maga = VMath.Dist(new Vector3D(0), a);
        	double magb = VMath.Dist(new Vector3D(0), b);
        	
        	if (maga*magb==0)
        		return 0;
        	return Math.Acos(adotb/(maga*magb));
        }
        
        private void initRotations()
        {
        	rotations = new Dictionary<string, Vector3D>();
        	jointChain = new List<IJoint>();
        	IJoint currJoint = FOutputSkeleton.JointTable[chainEnd];
    		while (currJoint.Name!=chainStart)
    		{
    			currJoint = currJoint.Parent;
    			rotations.Add(currJoint.Name, new Vector3D(0));
    			jointChain.Add(currJoint);
    		}
    		jointChain.Reverse();
        }
        
        private Matrix4x4 RotateAroundAxis(double angle, Vector3D axis)
        {
        	if (VMath.Dist(new Vector3D(0), axis)<=0)
        		return VMath.IdentityMatrix;
        	axis = 1*axis/VMath.Dist(new Vector3D(0), axis);
        	double c = Math.Cos(angle);
        	double s = Math.Sin(angle);
        	
        	Matrix4x4 ret = new Matrix4x4();
        	
        	ret.m11 = axis.x*axis.x*(1-c)+c;
        	ret.m12 = axis.x*axis.y*(1-c)-axis.z*s;
        	ret.m13 = axis.x*axis.z*(1-c)+axis.y*s;
        	ret.m14 = 0;
        	
        	ret.m21 = axis.y*axis.x*(1-c)+axis.z*s;
        	ret.m22 = axis.y*axis.y*(1-c)+c;
        	ret.m23 = axis.y*axis.z*(1-c)-axis.x*s;
        	ret.m24 = 0;
        	
        	ret.m31 = axis.x*axis.z*(1-c)-axis.y*s;
        	ret.m32 = axis.y*axis.z*(1-c)+axis.x*s;
        	ret.m33 = axis.z*axis.z*(1-c)+c;
        	ret.m34 = 0;
        	
        	ret.m41 = 0;
        	ret.m42 = 0;
        	ret.m43 = 0;
        	ret.m44 = 1;
        	
        	return ret;
        }
        
        #endregion helper
	}
}

