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

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Shared.VSlimDX;
using VVVV.Core.Logging;

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
                 Author = "vvvv group",
                 Help = "Returns a D3D9 mesh consisting of all meshes specified by index.",
                 Tags = "dae")]
    public class PluginColladaMesh: IPluginEvaluate, IPluginDXMesh
    {
        #region pins & fields
        [Input ("COLLADA Model")]
        IObservableSpread<Model> FColladaModelIn;
        
        [Input ("Time")]
        IObservableSpread<float> FTimeInput;
        
        [Input ("Bin Size", SliceMode = TSliceMode.Single, DefaultValue = -1)]
        IObservableSpread<int> FBinSize;
        
        [Input ("Index")]
        IObservableSpread<int> FIndex;
        
        [Output ("TextureFileName")]
        ISpread<string> FTextureFileNameOutput;
        
        [Output ("Emissive Color")]
        ISpread<RGBAColor> FEmissiveColorOut;
        
        [Output ("Diffuse Color", DefaultValues = new double[4]{1, 1, 1, 1})]
        ISpread<RGBAColor> FDiffuseColorOut;
        
        [Output ("Specular Color")]
        ISpread<RGBAColor> FSpecularColorOut;
        
        [Output ("Power", MinValue = 0, DefaultValue = 25)]
        ISpread<double> FShininessOut;
        
        [Output ("Opaque", DefaultValue = 1)]
        ISpread<double> FOpaqueOut;
        
        [Import]
    	private ILogger FLogger;            

        //pin declaration
        private IObservableSpread<bool> FOpaqueIsOneInput;
        private IDXMeshOut FMyMeshOutput;
        private ITransformOut FTransformOutput;
        private ITransformOut FSkinningTransformOutput;
        private ITransformOut FInvBindPoseTransformOutput;
        private ITransformOut FBindShapeTransformOutput;
		
		private Dictionary<int, Mesh> FDeviceMeshes;
    	private Model FColladaModel;
    	private List<Model.InstanceMesh> selectedInstanceMeshes;
    	private static Model.BasicMaterial FNoMaterial = new Model.BasicMaterial();
		//how transparency tag is treated
		private bool FOpaqueIsOne = true;
    	#endregion pins & fields
       
    	#region constructor
        [ImportingConstructor]
        public PluginColladaMesh(
            IPluginHost host,
            [Config ("Opaque=1?", SliceMode = TSliceMode.Single, DefaultValue = 1)]
            IObservableSpread<bool> OpaqueIsOneInput)
        {
            //the nodes constructor
            FDeviceMeshes = new Dictionary<int, Mesh>();
            selectedInstanceMeshes = new List<Model.InstanceMesh>();
            
            host.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMyMeshOutput);
            FMyMeshOutput.Order = int.MinValue;
            host.CreateTransformOutput("Transforms", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOutput);
            FTransformOutput.Order = int.MinValue + 1;
            host.CreateTransformOutput("Skinning Transforms", TSliceMode.Dynamic, TPinVisibility.True, out FSkinningTransformOutput);
            FSkinningTransformOutput.Order = int.MinValue + 2;
            host.CreateTransformOutput("Inverse Bind Pose Transforms", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FInvBindPoseTransformOutput);
			host.CreateTransformOutput("Bind Shape Transform", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FBindShapeTransformOutput);
            
            FOpaqueIsOneInput = OpaqueIsOneInput;
            FOpaqueIsOneInput.Changed += new SpreadChangedEventHander<bool>(FOpaqueIsOneInput_Changed);
        }

        void FOpaqueIsOneInput_Changed(IObservableSpread<bool> spread)
        {
            FOpaqueIsOne = FOpaqueIsOneInput[0];
        }
        #endregion constructor
    	
        
        #region mainloop
        public void Evaluate(int SpreadMax)
        {     	
        	try
        	{
	        	double tmp;
	        	//if any of the inputs has changed
	        	//recompute the outputs
	        	if (FColladaModelIn.IsChanged || FIndex.IsChanged || FBinSize.IsChanged)
				{
	        		selectedInstanceMeshes.Clear();
	        		
	        		FColladaModel = FColladaModelIn[0];
	        		
	        		if (FColladaModel == null)
	        		{
	        			FMyMeshOutput.SliceCount = 0;
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
						for (int i = 0; i < FIndex.SliceCount; i++)
						{
						    int index = FIndex[i] * binSize;
							index = ((index % FColladaModel.InstanceMeshes.Count) + FColladaModel.InstanceMeshes.Count) % FColladaModel.InstanceMeshes.Count;
							
							for (int j = index; j < index + binSize; j++)
							{
								Model.InstanceMesh instanceMesh = FColladaModel.InstanceMeshes[j % FColladaModel.InstanceMeshes.Count];
								selectedInstanceMeshes.Add(instanceMesh);
								
								FLogger.Log(LogType.Debug, "Instance of mesh '" + instanceMesh + "' loaded.");
								
								foreach (Document.Primitive primitive in instanceMesh.Mesh.Primitives)
								{
									Model.BasicMaterial material;
									string bindedMaterialId;
									if (!instanceMesh.MaterialBinding.TryGetValue(primitive.material, out bindedMaterialId)) {
										bindedMaterialId = primitive.material;
									}
									
									if (FColladaModel.BasicMaterialsBinding.TryGetValue(bindedMaterialId, out material))
										materialList.Add(material);
									else
									{
										materialList.Add(FNoMaterial);
									}
								}
							}
						}
						
						FTextureFileNameOutput.SliceCount = materialList.Count;
						FEmissiveColorOut.SliceCount = materialList.Count;
						FDiffuseColorOut.SliceCount = materialList.Count;
						FSpecularColorOut.SliceCount = materialList.Count;
						FShininessOut.SliceCount = materialList.Count;
						FOpaqueOut.SliceCount = materialList.Count;
						for (int j = 0; j < materialList.Count; j++)
						{
							Model.BasicMaterial material = materialList[j];
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
	        		int maxCount = Math.Max(FTimeInput.SliceCount, selectedInstanceMeshes.Count);
					List<Matrix> transforms = new List<Matrix>();
					List<Matrix> skinningTransforms = new List<Matrix>();
					List<Matrix> bindShapeTransforms = new List<Matrix>();
					for (int i = 0; i < maxCount && selectedInstanceMeshes.Count > 0; i++)
					{
						int meshIndex = i % selectedInstanceMeshes.Count;
						Model.InstanceMesh instanceMesh = selectedInstanceMeshes[meshIndex];
						
						float time = FTimeInput[i];
						Matrix m = FColladaModel.GetAbsoluteTransformMatrix(instanceMesh, time);
						
						for (int j = 0; j < instanceMesh.Mesh.Primitives.Count; j++) {
							transforms.Add(m);
						}
						
						// Skinning
						if (instanceMesh is Model.SkinnedInstanceMesh) {
							Model.SkinnedInstanceMesh skinnedInstanceMesh = (Model.SkinnedInstanceMesh) instanceMesh;							
							skinnedInstanceMesh.ApplyAnimations(time);
							skinningTransforms = skinnedInstanceMesh.GetSkinningMatrices();  // am i right, that this whole thing will only work with 1 selected mesh?
							bindShapeTransforms.Add(skinnedInstanceMesh.BindShapeMatrix);
							FInvBindPoseTransformOutput.SliceCount = skinnedInstanceMesh.InvBindMatrixList.Count;
							for (int j=0; j<skinnedInstanceMesh.InvBindMatrixList.Count; j++)
							{
								FInvBindPoseTransformOutput.SetMatrix(j, VSlimDXUtils.SlimDXMatrixToMatrix4x4(skinnedInstanceMesh.InvBindMatrixList[j]));
							}
							
						}
					}
					
					FTransformOutput.SliceCount = transforms.Count;
					for (int j = 0; j < transforms.Count; j++)
						FTransformOutput.SetMatrix(j, VSlimDXUtils.SlimDXMatrixToMatrix4x4(transforms[j]));
					
					FSkinningTransformOutput.SliceCount = skinningTransforms.Count;
					for (int j = 0; j < skinningTransforms.Count; j++)
						FSkinningTransformOutput.SetMatrix(j, VSlimDXUtils.SlimDXMatrixToMatrix4x4(skinningTransforms[j]));
					
					FBindShapeTransformOutput.SliceCount = bindShapeTransforms.Count;
					for (int j = 0; j < bindShapeTransforms.Count; j++)
						FBindShapeTransformOutput.SetMatrix(j, VSlimDXUtils.SlimDXMatrixToMatrix4x4(bindShapeTransforms[j]));
	        	}
        	}
        	catch (Exception e)
        	{
        		FLogger.Log(LogType.Error, e.Message);
        		FLogger.Log(LogType.Error, e.StackTrace);
        	}
        }
             
        #endregion mainloop  
        
        #region DXMesh
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			//Called by the PluginHost every frame for every device. Therefore a plugin should only do 
			//device specific operations here and still keep node specific calculations in the Evaluate call.
			
			if (FColladaModel != null)
			{
				Mesh m;
				if (!FDeviceMeshes.TryGetValue(OnDevice, out m))
				{
					//if resource is not yet created on given Device, create it now
					if (selectedInstanceMeshes.Count > 0)
					{
						FLogger.Log(LogType.Debug, "Creating Resource...");
						Device dev = Device.FromPointer(new IntPtr(OnDevice));
						try
						{
							m = CreateUnion3D9Mesh(dev, selectedInstanceMeshes);
							FDeviceMeshes.Add(OnDevice, m);
						}
						catch (Exception e)
						{
							FLogger.Log(LogType.Error, e.Message);
						}
						finally
						{
							//dispose device
							dev.Dispose();
						}
					}
				}
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			//Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
			//This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()
			
			Mesh m = FDeviceMeshes[OnDevice];
			if (m != null)
			{
				FLogger.Log(LogType.Debug, "Destroying Resource...");
				FDeviceMeshes.Remove(OnDevice);
				//dispose mesh
				m.Dispose();
			}
		}
		
		public void GetMesh(IDXMeshOut ForPin, int OnDevice, out int Mesh)
		{
			Mesh m = FDeviceMeshes[OnDevice];
			if (m != null)
				Mesh = m.ComPointer.ToInt32();
			else
				Mesh = 0;
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
        	
			Mesh mesh = Mesh.Concatenate(graphicsDevice, meshes.ToArray(), MeshFlags.Use32Bit | MeshFlags.Managed);
			
			foreach (Mesh m in meshes)
				m.Dispose();
			
        	mesh.OptimizeInPlace(MeshOptimizeFlags.AttributeSort);

        	return mesh;
        }
		#endregion
	}
}
