//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.SkeletonInterfaces;
using VVVV.Shared.VSlimDX;
using VVVV.Core.Logging;

using ColladaSlimDX.Utils;
using ColladaSlimDX.ColladaModel;
#endregion usings

namespace VVVV.Nodes
{
    [PluginInfo (Name = "Skeleton",
                 Category = "Skeleton",
                 Version = "Collada",
                 Author = "Elias Holzer",
                 Help = "Loads a skeleton from a COLLADA document.",
                 Tags = "dae")]
	public class ColladaSkeletonNode: IPluginEvaluate
    {		          	
    	#region pins & fields
    	[Input ("COLLADA Model", SliceMode = TSliceMode.Single)]
        IObservableSpread<Model> FColladaModelIn;

        [Input ("Time", SliceMode = TSliceMode.Single)]
        IObservableSpread<float> FTimeInput;
        
        [Input ("Index", SliceMode = TSliceMode.Single)]
        IObservableSpread<int> FIndex;
        
        [Output ("Skeleton", SliceMode = TSliceMode.Single)]
        ISpread<Skeleton> FSkeletonOut;
        
        [Import]
    	private ILogger FLogger;   
    	    	
    	private ITransformOut FInvBindPoseOut;
        private ITransformOut FBindShapeOut;
        
   		private ISkeleton FSkeleton;
   		private Model FColladaModel;
   		private Model.SkinnedInstanceMesh FSelectedMesh;
    	#endregion pins & fields
       
    	#region constructor
    	[ImportingConstructor]
        public ColladaSkeletonNode(IPluginHost host)
        {
			FSkeleton = new Skeleton();
		    
			host.CreateTransformOutput("Bind Shape Matrix", TSliceMode.Single, TPinVisibility.True, out FBindShapeOut);
	    	FBindShapeOut.Order = 1;
	    	host.CreateTransformOutput("Inverse Bind Pose Matrix", TSliceMode.Dynamic, TPinVisibility.True, out FInvBindPoseOut);
	    	FInvBindPoseOut.Order = 2;
		}
        #endregion constructor
        
        #region mainloop
        public void Evaluate(int SpreadMax)
        {     	
        	//if any of the inputs has changed
        	//recompute the outputs
        	FColladaModel = FColladaModelIn[0];
        	if (FColladaModel == null)
    			return;
        	
        	if (FColladaModelIn.IsChanged || FIndex.IsChanged)
        	{
        	    int index = FIndex[0];
        		
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
        	
        	if (FColladaModelIn.IsChanged || FIndex.IsChanged || FTimeInput.IsChanged)
        	{
        	    float time = FTimeInput[0];
        		
        		FSelectedMesh.ApplyAnimations(time);
        		
        		if (FColladaModelIn.IsChanged || FIndex.IsChanged)
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
        		}
        		else
        		{
        			foreach (Model.Bone bone in FSelectedMesh.Bones)
        			{
        				FSkeleton.JointTable[bone.Name].BaseTransform = VSlimDXUtils.SlimDXMatrixToMatrix4x4(bone.TransformMatrix);
        			}
        		}
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
