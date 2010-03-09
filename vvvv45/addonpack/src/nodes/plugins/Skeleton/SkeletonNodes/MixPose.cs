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
using System.Drawing;
using System.Collections;
using System.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Text.RegularExpressions;
using System.Globalization;
using VVVV.Utils.SharedMemory;
using VVVV.SkeletonInterfaces;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class MixPose: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private INodeIn FPose1Input;
    	private INodeIn FPose2Input;
    	private INodeIn FPose3Input;
    	private INodeIn FPose4Input;
    	private INodeIn FPose5Input;
    	private INodeIn FPose6Input;
    	private IValueIn FAmount1Input;
    	private IValueIn FAmount2Input;
    	private IValueIn FAmount3Input;
    	private IValueIn FAmount4Input;
    	private IValueIn FAmount5Input;
    	private IValueIn FAmount6Input;
    	
    	private INodeOut FPoseOutput;
    	
    	private JointInfo outputJoint;
    	private Skeleton outputSkeleton;
    	
    	private JointInfo pose1, pose2;
    	private double amount1, amount2;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public MixPose()
        {
			//the nodes constructor
			//nothing to declare for this node
			outputJoint = new JointInfo();
			outputSkeleton = new Skeleton(outputJoint);
			
			pose1 = new JointInfo();
			pose2 = new JointInfo();
			amount1 = 1.0;
			amount2 = 0.0;
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
        ~MixPose()
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
					FPluginInfo.Name = "MixPose";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "MixPose";
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
	    	
	    	System.Guid[] guids = new System.Guid[1];
	    	guids[0] = new Guid("AB312E34-8025-40F2-8241-1958793F3D39");

	    	//create inputs
	    	FHost.CreateNodeInput("Pose 1", TSliceMode.Single, TPinVisibility.True, out FPose1Input);
	    	FPose1Input.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateNodeInput("Pose 2", TSliceMode.Single, TPinVisibility.True, out FPose2Input);
	    	FPose2Input.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateNodeInput("Pose 3", TSliceMode.Single, TPinVisibility.OnlyInspector, out FPose3Input);
	    	FPose3Input.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateNodeInput("Pose 4", TSliceMode.Single, TPinVisibility.OnlyInspector, out FPose4Input);
	    	FPose4Input.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateNodeInput("Pose 5", TSliceMode.Single, TPinVisibility.OnlyInspector, out FPose5Input);
	    	FPose5Input.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateNodeInput("Pose 6", TSliceMode.Single, TPinVisibility.OnlyInspector, out FPose6Input);
	    	FPose6Input.SetSubType(guids, "Skeleton");
	    	
	    	FHost.CreateValueInput("Amount 1", 1, null, TSliceMode.Single, TPinVisibility.True, out FAmount1Input);
	    	FAmount1Input.SetSubType(0.0, 1.0, 0.01, 1.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Amount 2", 1, null, TSliceMode.Single, TPinVisibility.True, out FAmount2Input);
	    	FAmount2Input.SetSubType(0.0, 1.0, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Amount 3", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FAmount3Input);
	    	FAmount2Input.SetSubType(0.0, 1.0, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Amount 4", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FAmount4Input);
	    	FAmount2Input.SetSubType(0.0, 1.0, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Amount 5", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FAmount5Input);
	    	FAmount2Input.SetSubType(0.0, 1.0, 0.01, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Amount 6", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FAmount6Input);
	    	FAmount2Input.SetSubType(0.0, 1.0, 0.01, 0.0, false, false, false);
	    	
	    	// create outputs
	    	
	    	FHost.CreateNodeOutput("Mixed Pose", TSliceMode.Single, TPinVisibility.True, out FPoseOutput);
	    	FPoseOutput.SetSubType(guids, "Skeleton");
	    	
	    	
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
            INodeIOBase currInterface;

        	if (FPose1Input.PinIsChanged)
        	{
        		FPose1Input.GetUpstreamInterface(out currInterface);
        		pose1 = (JointInfo)((Skeleton)currInterface).Root;
        		recalculate = true;
        	}
        	
        	if (FPose2Input.PinIsChanged)
        	{
        		FPose2Input.GetUpstreamInterface(out currInterface);
        		pose2 = (JointInfo)((Skeleton)currInterface).Root;
        		recalculate = true;
        	}
        	
        	if (FAmount1Input.PinIsChanged)
        	{
        		FAmount1Input.GetValue(0, out amount1);
        		recalculate = true;
        	}
        	
        	if (FAmount2Input.PinIsChanged)
        	{
        		FAmount2Input.GetValue(0, out amount2);
        		recalculate = true;
        	}
        	
        	
        	if (recalculate)
        	{
        		outputJoint = (JointInfo)pose1.DeepCopy();
        		mixJoints(outputJoint, pose1, pose2);
				outputSkeleton.Root = outputJoint;
				outputSkeleton.BuildJointTable();
        		FPoseOutput.MarkPinAsChanged();
        	}
        

        	FPoseOutput.SetInterface(outputSkeleton);
        	
        }
             
        #endregion mainloop  
        
        #region helper
        
        private void mixJoints(JointInfo result, JointInfo joint1, JointInfo joint2)
        {
        	Vector3D resultRot = new Vector3D(0);
        	resultRot.x = amount1*joint1.Rotation.x + amount2*joint2.Rotation.x;
        	resultRot.y = amount1*joint1.Rotation.y + amount2*joint2.Rotation.y;
        	resultRot.z = amount1*joint1.Rotation.z + amount2*joint2.Rotation.z;
        	
        	result.Rotation = resultRot;
        	
        	for (int i=0; i<result.children.Count; i++)
        	{
        		mixJoints((JointInfo)result.Children[i], (JointInfo)joint1.Children[i], (JointInfo)joint2.Children[i]);
        	}
        }
        
        #endregion helper
        
        
        
	}
}
