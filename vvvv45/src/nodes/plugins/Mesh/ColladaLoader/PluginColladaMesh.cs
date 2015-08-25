//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.Utils.SlimDX;

using ColladaSlimDX.ColladaModel;
using ColladaSlimDX.ColladaDocument;
using ColladaSlimDX.Utils;

using SlimDX;
using SlimDX.Direct3D9;
#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes
{
    [PluginInfo (Name = "Mesh",
                 Category = "EX9.Geometry",
                 Version = "Collada",
                 Author = "vvvv group, woei",
                 Help = "Returns a D3D9 mesh consisting of all meshes specified by index.",
                 Tags = "dae")]
    public class PluginColladaMesh: IPluginEvaluate, IPluginDXMesh
    {
        #region pins & fields
        [Input ("COLLADA Model")]
        protected IDiffSpread<Model> FColladaModelIn;
        
        [Input ("Time")]
        protected IDiffSpread<float> FTimeInput;
        
        [Input ("Bin Size", IsSingle = true, DefaultValue = -1)]
        protected IDiffSpread<int> FBinSize;
        
        [Input ("Index")]
        protected IDiffSpread<int> FIndex;

        [Output("Bone Names", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<string> FBoneNamesOutput;
        
        [Output ("MeshName")]
        protected ISpread<string> FMeshNameOutput;
                
        [Output ("MeshPath")]
        protected ISpread<string> FMeshPathOutput;
        
        [Output ("MaterialName")]
        protected ISpread<string> FMaterialNameOutput;

        
//        [Output ("EffectID")]
//        protected ISpread<string> FFxIdOutput;
        
        [Output ("TextureFileName")]
        protected ISpread<string> FTextureFileNameOutput;
        
        [Output ("Emissive Color")]
        protected ISpread<RGBAColor> FEmissiveColorOut;
        
        [Output ("Diffuse Color", DefaultValues = new double[4]{1, 1, 1, 1})]
        protected ISpread<RGBAColor> FDiffuseColorOut;
        
        [Output ("Specular Color")]
        protected ISpread<RGBAColor> FSpecularColorOut;
        
        [Output ("Power", MinValue = 0, DefaultValue = 25)]
        protected ISpread<double> FShininessOut;
        
        [Output ("Opaque", DefaultValue = 1)]
        protected ISpread<double> FOpaqueOut;
        
        [Import]
    	protected ILogger FLogger;            

        //pin declaration
        private IDiffSpread<bool> FOpaqueIsOneInput;
        private IDXMeshOut FMyMeshOutput;
        private ITransformOut FTransformOutput;
        private ITransformOut FSkinningTransformOutput;
        private ITransformOut FInvBindPoseTransformOutput;
        private ITransformOut FBindShapeTransformOutput;
		
		private Dictionary<Device, Mesh> FDeviceMeshes;
    	private Model FColladaModel;
    	private List<Model.InstanceMesh> FSelectedInstanceMeshes;
    	private static Model.BasicMaterial FNoMaterial = new Model.BasicMaterial();
		//how transparency tag is treated
		private bool FOpaqueIsOne = true;
    	#endregion pins & fields
       
    	#region constructor
        [ImportingConstructor]
        public PluginColladaMesh(
            IPluginHost host,
            [Config ("Opaque=1?", IsSingle = true, DefaultValue = 1)]
            IDiffSpread<bool> opaqueIsOneInput)
        {
            //the nodes constructor
            FDeviceMeshes = new Dictionary<Device, Mesh>();
            FSelectedInstanceMeshes = new List<Model.InstanceMesh>();
            
            host.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMyMeshOutput);
            FMyMeshOutput.Order = int.MinValue;
            host.CreateTransformOutput("Transforms", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOutput);
            FTransformOutput.Order = int.MinValue + 1;
            host.CreateTransformOutput("Skinning Transforms", TSliceMode.Dynamic, TPinVisibility.True, out FSkinningTransformOutput);
            FSkinningTransformOutput.Order = int.MinValue + 2;
            host.CreateTransformOutput("Inverse Bind Pose Transforms", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FInvBindPoseTransformOutput);
			host.CreateTransformOutput("Bind Shape Transform", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FBindShapeTransformOutput);
            
            FOpaqueIsOneInput = opaqueIsOneInput;
            FOpaqueIsOneInput.Changed += new SpreadChangedEventHander<bool>(FOpaqueIsOneInput_Changed);
        }

        void FOpaqueIsOneInput_Changed(IDiffSpread<bool> spread)
        {
            FOpaqueIsOne = FOpaqueIsOneInput[0];
        }
        #endregion constructor
    	
        
        #region mainloop
        public void Evaluate(int SpreadMax)
        {     	
            COLLADAUtil.Logger = new LoggerWrapper(FLogger);
            
        	try
        	{
	        	//if any of the inputs has changed
	        	//recompute the outputs
	        	if (FColladaModelIn.IsChanged || FIndex.IsChanged || FBinSize.IsChanged)
				{
	        		FSelectedInstanceMeshes.Clear();
	        		FMeshNameOutput.SliceCount = 0;
	        		FMeshPathOutput.SliceCount = 0;
	        		FColladaModel = FColladaModelIn.SliceCount > 0 ? FColladaModelIn[0] : null;
	        		
	        		if (FColladaModel == null)
	        		{
	        			FMyMeshOutput.SliceCount = 0;
	        			FMaterialNameOutput.SliceCount = 0;
//	        			FFxIdOutput.SliceCount = 0;
	        			FTextureFileNameOutput.SliceCount = 0;
						FEmissiveColorOut.SliceCount = 0;
						FDiffuseColorOut.SliceCount = 0;
						FSpecularColorOut.SliceCount = 0;
						FShininessOut.SliceCount = 0;
						FOpaqueOut.SliceCount = 0;
	        		}
	        		else
	        		{
		        		//make negative bin sizes ok
		        		int binSize = FBinSize[0];
		        		if (binSize < 0)
		        			binSize = FColladaModel.InstanceMeshes.Count / Math.Abs(binSize);
		        		
						List<Model.BasicMaterial> materialList = new List<Model.BasicMaterial>();
//						FFxIdOutput.SliceCount = 0;
						for (int i = 0; i < FIndex.SliceCount; i++)
						{
						    int index = FIndex[i] * binSize;
							index = ((index % FColladaModel.InstanceMeshes.Count) + FColladaModel.InstanceMeshes.Count) % FColladaModel.InstanceMeshes.Count;
							for (int j = index; j < index + binSize; j++)
							{
								Model.InstanceMesh instanceMesh = FColladaModel.InstanceMeshes[j % FColladaModel.InstanceMeshes.Count];
								FSelectedInstanceMeshes.Add(instanceMesh);
								FLogger.Log(LogType.Debug, "Instance of mesh '" + instanceMesh + "' loaded.");
								
								string name = instanceMesh.ParentBone.NodeName;
								string path = name;
								var bone = instanceMesh.ParentBone;
								while (bone.Parent.NodeType == "NODE")
								{
									path = bone.Parent.NodeName+"/"+path;
									bone = bone.Parent;
								}
								
								foreach (Document.Primitive primitive in instanceMesh.Mesh.Primitives)
								{
									Model.BasicMaterial material;
									string bindedMaterialId;
									if (!instanceMesh.MaterialBinding.TryGetValue(primitive.material, out bindedMaterialId)) {
										bindedMaterialId = primitive.material;
									}
									
									if (FColladaModel.BasicMaterialsBinding.TryGetValue(bindedMaterialId, out material))
									{
										materialList.Add(material);
//										FFxIdOutput.Add(bindedMaterialId);
									}
									else
									{
										materialList.Add(FNoMaterial);
//										FFxIdOutput.Add(bindedMaterialId);
									}
									
									FMeshNameOutput.Add(name); //add name of the mesh
									FMeshPathOutput.Add(path); //and prepend the group names it's in
								}
							}
						}
						
						FMaterialNameOutput.SliceCount = materialList.Count;
						FTextureFileNameOutput.SliceCount = materialList.Count;
						FEmissiveColorOut.SliceCount = materialList.Count;
						FDiffuseColorOut.SliceCount = materialList.Count;
						FSpecularColorOut.SliceCount = materialList.Count;
						FShininessOut.SliceCount = materialList.Count;
						FOpaqueOut.SliceCount = materialList.Count;
						for (int j = 0; j < materialList.Count; j++)
						{
							Model.BasicMaterial material = materialList[j];
							FMaterialNameOutput[j] = material.Name;
							FTextureFileNameOutput[j] = material.Texture;
							if (material.EmissiveColor.HasValue)
								FEmissiveColorOut[j] = new RGBAColor(material.EmissiveColor.Value.X, material.EmissiveColor.Value.Y, material.EmissiveColor.Value.Z, 1.0);
							else
							    FEmissiveColorOut[j] = VColor.Black;
							if (material.DiffuseColor.HasValue)
								FDiffuseColorOut[j] = new RGBAColor(material.DiffuseColor.Value.X, material.DiffuseColor.Value.Y, material.DiffuseColor.Value.Z, 1.0);
							else
								FDiffuseColorOut[j] = VColor.White;
							if (material.SpecularColor.HasValue)
								FSpecularColorOut[j] = new RGBAColor(material.SpecularColor.Value.X, material.SpecularColor.Value.Y, material.SpecularColor.Value.Z, 1.0);
							else
								FSpecularColorOut[j] = VColor.Black;
							if (material.SpecularPower.HasValue)
								FShininessOut[j] = material.SpecularPower.Value;
							else
								FShininessOut[j] = 25.0;
							// as of FCollada 3.03 opaque = 1.0, before opaque = 0.0
							double alpha = 1.0;
							if (material.Alpha.HasValue)
								alpha = material.Alpha.Value;
							if (!FOpaqueIsOne)
								FOpaqueOut[j] = 1 - alpha;
							else
								FOpaqueOut[j] = alpha;
						}
						
						FMyMeshOutput.SliceCount = materialList.Count;
						FMyMeshOutput.MarkPinAsChanged();
						
						foreach (Mesh m in FDeviceMeshes.Values)
						{
							FLogger.Log(LogType.Debug, "Destroying Resource...");
							m.Dispose();
						}
						FDeviceMeshes.Clear();
	        		}
				}     
	        	
	        	if (FColladaModelIn.IsChanged || FIndex.IsChanged || FBinSize.IsChanged || FTimeInput.IsChanged)
	        	{
	        		int maxCount = Math.Max(FTimeInput.SliceCount, FSelectedInstanceMeshes.Count);
					var transforms = new List<Matrix>();
					var skinningTransforms = new List<Matrix>();
					var bindShapeTransforms = new List<Matrix>();
					var invBindPoseTransforms = new List<Matrix>();
                    var boneNames = new List<string>();
					for (int i = 0; i < maxCount && FSelectedInstanceMeshes.Count > 0; i++)
					{
						int meshIndex = i % FSelectedInstanceMeshes.Count;
						var instanceMesh = FSelectedInstanceMeshes[meshIndex];
						float time = FTimeInput[i];
						var m = instanceMesh.ParentBone.GetAbsoluteTransformMatrix(time) * FColladaModel.ConversionMatrix;
						
						for (int j = 0; j < instanceMesh.Mesh.Primitives.Count; j++)
							transforms.Add(m);
						
						// Skinning
						if (instanceMesh is Model.SkinnedInstanceMesh)
                        {
							var skinnedInstanceMesh = instanceMesh as Model.SkinnedInstanceMesh;
							skinningTransforms.AddRange(skinnedInstanceMesh.GetSkinningMatrices(time));  // am i right, that this whole thing will only work with 1 selected mesh?
							bindShapeTransforms.Add(skinnedInstanceMesh.BindShapeMatrix);
							for (int j = 0; j<skinnedInstanceMesh.InvBindMatrixList.Count; j++)
								invBindPoseTransforms.Add(skinnedInstanceMesh.InvBindMatrixList[j]);
                            boneNames.AddRange(skinnedInstanceMesh.Bones.Select(b => b.Name));
						}
					}
					
					
					FTransformOutput.SliceCount = transforms.Count;
					for (int j = 0; j < transforms.Count; j++)
						FTransformOutput.SetMatrix(j, transforms[j].ToMatrix4x4());
					
					FSkinningTransformOutput.SliceCount = skinningTransforms.Count;
					for (int j = 0; j < skinningTransforms.Count; j++)
						FSkinningTransformOutput.SetMatrix(j, skinningTransforms[j].ToMatrix4x4());
					
					FBindShapeTransformOutput.SliceCount = bindShapeTransforms.Count;
					for (int j = 0; j < bindShapeTransforms.Count; j++)
						FBindShapeTransformOutput.SetMatrix(j, bindShapeTransforms[j].ToMatrix4x4());
					
					FInvBindPoseTransformOutput.SliceCount = invBindPoseTransforms.Count;
					for (int j = 0; j<invBindPoseTransforms.Count; j++)
						FInvBindPoseTransformOutput.SetMatrix(j, invBindPoseTransforms[j].ToMatrix4x4());

                    FBoneNamesOutput.AssignFrom(boneNames);
	        	}
        	}
        	catch (Exception e)
        	{
        		FLogger.Log(e);
        	}
        }
             
        #endregion mainloop  
        
        #region DXMesh
		public void UpdateResource(IPluginOut ForPin, Device OnDevice)
		{
			//Called by the PluginHost every frame for every device. Therefore a plugin should only do 
			//device specific operations here and still keep node specific calculations in the Evaluate call.
			
			if (FColladaModel != null)
			{
				Mesh m;
				if (!FDeviceMeshes.TryGetValue(OnDevice, out m))
				{
					//if resource is not yet created on given Device, create it now
					if (FSelectedInstanceMeshes.Count > 0)
					{
						FLogger.Log(LogType.Debug, "Creating Resource...");
						try
						{
							m = CreateUnion3D9Mesh(OnDevice, FSelectedInstanceMeshes);
							FDeviceMeshes.Add(OnDevice, m);
						}
						catch (Exception e)
						{
							FLogger.Log(LogType.Error, e.Message);
						}
					}
				}
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
		{
			//Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
			//This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()
			Mesh m;
            if (FDeviceMeshes.TryGetValue(OnDevice, out m))
            {
                if (m != null)
                {
                    FLogger.Log(LogType.Debug, "Destroying Resource...");
                    FDeviceMeshes.Remove(OnDevice);
                    //dispose mesh
                    m.Dispose();
                }
            }
		}
		
		public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
		{
		    Mesh mesh = null;
			FDeviceMeshes.TryGetValue(OnDevice, out mesh);
			return mesh;
		}
		#endregion
        
        #region helper functions
        private Mesh CreateUnion3D9Mesh(Device graphicsDevice, List<Model.InstanceMesh> instanceMeshes)
       	{
    		List<Mesh> meshes = new List<Mesh>();
    		
			int attribId = 0;
			foreach (Model.InstanceMesh instanceMesh in instanceMeshes)
    		{
				if (instanceMesh.Mesh.Primitives.Count > 0)
				{
					meshes.Add(instanceMesh.Mesh.Create3D9Mesh(graphicsDevice, ref attribId));
					attribId++;
				}
    		}
        	
			var options = MeshFlags.Use32Bit;
			if (graphicsDevice is DeviceEx)
				options |= MeshFlags.Dynamic;
			else
				options |= MeshFlags.Managed;
			Mesh mesh = Mesh.Concatenate(graphicsDevice, meshes.ToArray(), options);
			
			foreach (Mesh m in meshes)
				m.Dispose();
			
        	mesh.OptimizeInPlace(MeshOptimizeFlags.AttributeSort);

        	return mesh;
        }
		#endregion
	}
}
