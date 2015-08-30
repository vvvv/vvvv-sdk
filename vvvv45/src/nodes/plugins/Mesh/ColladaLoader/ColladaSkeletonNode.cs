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
using VVVV.Core.Logging;
using VVVV.Utils.SlimDX;

using ColladaSlimDX.Utils;
using ColladaSlimDX.ColladaModel;
#endregion usings

namespace VVVV.Nodes
{
    [PluginInfo(Name = "Skeleton",
                Category = "Skeleton",
                Version = "Collada",
                Author = "vvvv group",
                Help = "Loads a skeleton from a COLLADA document.",
                Tags = "dae")]
    public class ColladaSkeletonNode: IPluginEvaluate
    {
        #region pins & fields
        [Input("COLLADA Model", IsSingle = true)]
        protected IDiffSpread<Model> FColladaModelIn;

        [Input("Time", IsSingle = true)]
        protected IDiffSpread<float> FTimeInput;
        
        [Input("Index", IsSingle = true)]
        protected IDiffSpread<int> FIndex;
        
        //        [Output("Skeleton", IsSingle = true)]
        //        protected ISpread<Skeleton> FSkeletonOut;
        private INodeOut FSkeletonOutput;
        
        [Import]
        protected ILogger FLogger;
        
        private Skeleton FSkeleton;
        private Model FColladaModel;
        private Model.SkinnedInstanceMesh FSelectedMesh;
        #endregion pins & fields
        
        #region constructor
        [ImportingConstructor]
        public ColladaSkeletonNode(IPluginHost host)
        {
            FSkeleton = new Skeleton();
            
            System.Guid[] guids = new System.Guid[1];
            guids[0] = new Guid("AB312E34-8025-40F2-8241-1958793F3D39");
            
            host.CreateNodeOutput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonOutput);
            FSkeletonOutput.SetSubType2(typeof(ISkeleton), guids, "Skeleton");
        }
        #endregion constructor
        
        #region mainloop
        public void Evaluate(int SpreadMax)
        {
            //if any of the inputs has changed
            //recompute the outputs
            FColladaModel = FColladaModelIn.SliceCount > 0 ? FColladaModelIn[0] : null;
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
                
                if (FColladaModelIn.IsChanged || FIndex.IsChanged)
                {
                    FSkeleton.ClearAll();
                    CreateSkeleton(FSkeleton, FSelectedMesh.RootBone, FSelectedMesh.Bones);
                }
                
                foreach (Model.Bone bone in FSelectedMesh.Bones)
                {
                    FSkeleton.JointTable[bone.Name].BaseTransform = bone.GetTransformMatrix(time).ToMatrix4x4();
                }
                
                FSkeletonOutput.SetInterface(FSkeleton);
                FSkeletonOutput.MarkPinAsChanged();
            }
        }
        
        #endregion mainloop
        
        #region helper
        void CreateSkeleton(Skeleton skeleton, Model.Bone bone, List<Model.Bone> bonesToSkin)
        {
            var joint = new BoneWrapper(bonesToSkin.IndexOf(bone), bone.Name, bone.GetTransformMatrix(0).ToMatrix4x4());
            var parentName = bone.Parent != null ? bone.Parent.Name : string.Empty;
            skeleton.InsertJoint(parentName, joint);
            foreach (var child in bone.Children)
                CreateSkeleton(skeleton, child, bonesToSkin);
        }
        #endregion
    }
}
