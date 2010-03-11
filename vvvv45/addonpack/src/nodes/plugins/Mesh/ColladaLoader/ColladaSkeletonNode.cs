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
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.SkeletonInterfaces;
using VVVV.Shared.VSlimDX;

using ColladaSlimDX.Utils;
using ColladaSlimDX.ColladaModel;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class ColladaSkeletonNode: IPlugin, IDisposable, IPluginConnections
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
   		private ISkeleton FSkeleton;
   		private IColladaModelNodeIO FUpstreamInterface;
   		private Model FColladaModel;
   		private Model.SkinnedInstanceMesh FSelectedMesh;

    	//input pin declaration
    	private INodeIn FColladaModelIn;
    	private IValueIn FTimeInput;
    	private IValueIn FIndex;
    	
    	//output pin declaration
    	private INodeOut FSkeletonOut;
    	private ITransformOut FBindShapeOut;
    	private ITransformOut FInvBindPoseOut;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public ColladaSkeletonNode()
        {
			//the nodes constructor
			FSkeleton = new Skeleton();
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
	        	
        		if (FHost != null)
	        		FHost.Log(TLogType.Debug, "ColladaSkeletonNode is being deleted");
        		
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
        ~ColladaSkeletonNode()
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
					FPluginInfo.Name = "Skeleton";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Collada";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Elias Holzer";
					//describe the nodes function
					FPluginInfo.Help = "Loads a skeleton from a COLLADA document.";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
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
	    	FHost.CreateNodeInput("COLLADA Model", TSliceMode.Single, TPinVisibility.True, out FColladaModelIn);
			FColladaModelIn.SetSubType(new Guid[1]{ColladaModelNodeIO.GUID}, ColladaModelNodeIO.FriendlyName);
			
			FHost.CreateValueInput("Time", 1, null, TSliceMode.Single, TPinVisibility.True, out FTimeInput);
			FTimeInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
			
			FHost.CreateValueInput("Index", 1, null, TSliceMode.Single, TPinVisibility.True, out FIndex);
			FIndex.SetSubType(double.MinValue, double.MaxValue, 1.0, 0.0, false, false, true);
	    	
	    	//create outputs	    	
	    	FHost.CreateNodeOutput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonOut);
	    	FSkeletonOut.SetSubType(new Guid[1] {SkeletonNodeIO.GUID}, SkeletonNodeIO.FriendlyName);
	    	FSkeletonOut.SetInterface(FSkeleton);
	    	
	    	FHost.CreateTransformOutput("Bind Shape Matrix", TSliceMode.Single, TPinVisibility.True, out FBindShapeOut);
	    	FBindShapeOut.Order = 1;
	    	FHost.CreateTransformOutput("Inverse Bind Pose Matrix", TSliceMode.Dynamic, TPinVisibility.True, out FInvBindPoseOut);
	    	FInvBindPoseOut.Order = 2;
	    	
	    	COLLADAUtil.Logger = new LoggerWrapper(FHost);
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
        public void ConnectPin(IPluginIO Pin)
        {
        	if (Pin == FColladaModelIn)
        	{
        		INodeIOBase usI;
        		FColladaModelIn.GetUpstreamInterface(out usI);
        		FUpstreamInterface = usI as IColladaModelNodeIO;
        	}
        }
        
        public void DisconnectPin(IPluginIO Pin)
        {
        	if (Pin == FColladaModelIn)
        	{
        		FUpstreamInterface = null;
        	}
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {     	
        	//if any of the inputs has changed
        	//recompute the outputs
        	
        	if (FColladaModelIn.PinIsChanged)
        	{	
        		if (FUpstreamInterface != null)
        			FUpstreamInterface.GetSlice(0, out FColladaModel); 
        		else
        			FColladaModel = null;
        	}
        	
        	if (FColladaModel == null)
    		{
    			return;
    		}
        	
        	if (FColladaModelIn.PinIsChanged || FIndex.PinIsChanged)
        	{
        		double tmp;
        		FIndex.GetValue(0, out tmp);
        		int index = (int) tmp;
        		
        		List<Model.SkinnedInstanceMesh> skinnedMeshes = new List<Model.SkinnedInstanceMesh>();
        		foreach (Model.InstanceMesh mesh in FColladaModel.InstanceMeshes)
        		{
        			if (mesh is Model.SkinnedInstanceMesh)
        			{
        				skinnedMeshes.Add((Model.SkinnedInstanceMesh) mesh);
        			}
        		}
        		
        		if (skinnedMeshes.Count > 0)
        		{
        			if (FSelectedMesh != skinnedMeshes[index % skinnedMeshes.Count])
        			{
        				FSelectedMesh = skinnedMeshes[index % skinnedMeshes.Count];
        			}
        		}
        		else
        		{
        			FSelectedMesh = null;
        		}
        	}
        	
        	if (FSelectedMesh == null)
        	{
    			return;
        	}
        	
        	if (FColladaModelIn.PinIsChanged || FIndex.PinIsChanged || FTimeInput.PinIsChanged)
        	{
        		double time;
        		FTimeInput.GetValue(0, out time);
        		
        		FSelectedMesh.ApplyAnimations((float) time);
        		
        		if (FColladaModelIn.PinIsChanged || FIndex.PinIsChanged)
        		{
        			FSkeleton.ClearAll();
        			CreateSkeleton(ref FSkeleton, FSelectedMesh.SkeletonRootBone);
        			// Set the IDs
        			int id = 0;
        			foreach (Model.Bone bone in FSelectedMesh.Bones)
        			{
        				FSkeleton.JointTable[bone.Name].Id = id;
        				id++;
        			}
        			
        			FBindShapeOut.SetMatrix(0, VSlimDXUtils.SlimDXMatrixToMatrix4x4(FSelectedMesh.BindShapeMatrix));
        			FInvBindPoseOut.SliceCount = FSelectedMesh.InvBindMatrixList.Count;
        			for (int i = 0; i < FSelectedMesh.InvBindMatrixList.Count; i++)
        			{
        				FInvBindPoseOut.SetMatrix(i, VSlimDXUtils.SlimDXMatrixToMatrix4x4(FSelectedMesh.InvBindMatrixList[i]));
        			}
        		}
        		else
        		{
        			foreach (Model.Bone bone in FSelectedMesh.Bones)
        			{
        				FSkeleton.JointTable[bone.Name].BaseTransform = VSlimDXUtils.SlimDXMatrixToMatrix4x4(bone.TransformMatrix);
        			}
        		}
        		
        		FSkeletonOut.MarkPinAsChanged();
        	}
        }
             
        #endregion mainloop  
        
        #region helper
        private void CreateSkeleton(ref ISkeleton skeleton, Model.Bone bone)
        {
        	IJoint joint = new BoneWrapper(bone);
        	joint.Id = -1;
        	if (skeleton.Root == null)
        		skeleton.InsertJoint("", joint);
        	else
        		skeleton.InsertJoint(bone.Parent.Name, joint);
        	
        	foreach (Model.Bone child in bone.Children)
        	{
        		CreateSkeleton(ref skeleton, child);
        	}
        }
        #endregion
	}
}
