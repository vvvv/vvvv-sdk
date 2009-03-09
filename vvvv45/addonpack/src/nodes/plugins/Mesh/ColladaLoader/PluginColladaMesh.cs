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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Collada;
using VVVV.Collada.ColladaModel;
using VVVV.Collada.ColladaDocument;

using SlimDX;
using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class PluginColladaMesh: IPlugin, IPluginConnections, IDisposable, IPluginDXMesh
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private INodeIn FColladaModelIn;
    	private IValueIn FTimeInput;
    	private IValueIn FBinSize;
    	private IValueIn FIndex;
		private IValueConfig FOpaqueIsOneInput;
		
		//output pin declaration
		private IDXMeshIO FMyMeshOutput;
		private IStringOut FTextureFileNameOutput;
		private ITransformOut FTransformOutput;
		private ITransformOut FSkinningTransformOutput;
		private IColorOut FEmissiveColorOut;
		private IColorOut FDiffuseColorOut;
		private IColorOut FSpecularColorOut;
		private IValueOut FShininessOut;
		private IValueOut FOpaqueOut;
		private IValueOut FSkeletonOut;
		private IValueOut FVertexOut;
		private IValueOut FVertexIndexOut;
		private IValueOut FVCountOut;
		private IValueOut FBlendWeightIndexOut;
		private IValueOut FBlendWeightOut;
		
		private IColladaModelNodeIO FUpstreamInterface;
		
		private Dictionary<int, Mesh> FDeviceMeshes;
    	private Model FColladaModel;
    	private List<Model.InstanceMesh> selectedInstanceMeshes;
    	private static Model.BasicMaterial FNoMaterial = new Model.BasicMaterial();
		//how transparency tag is treated
		private bool FOpaqueIsOne = true;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginColladaMesh()
        {
			//the nodes constructor
			FDeviceMeshes = new Dictionary<int, Mesh>();
			selectedInstanceMeshes = new List<Model.InstanceMesh>();
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

        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "PluginColladaMesh is being deleted");
        		
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
        ~PluginColladaMesh()
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
					FPluginInfo.Name = "Mesh";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9.Geometry";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Collada";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Returns a D3D9 mesh consisting of all meshes specified by index.";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "Collada,Mesh";
					
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
			FHost.CreateNodeInput("COLLADA Model", TSliceMode.Dynamic, TPinVisibility.True, out FColladaModelIn);
			FColladaModelIn.SetSubType(new Guid[1]{ColladaModelNodeIO.GUID}, ColladaModelNodeIO.FriendlyName);
			
			FHost.CreateValueInput("Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTimeInput);
			FTimeInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.0, false, false, false);
			
			FHost.CreateValueInput("Bin Size", 1, null, TSliceMode.Single, TPinVisibility.True, out FBinSize);
			FBinSize.SetSubType(double.MinValue, double.MaxValue, 1.0, -1.0, false, false, true);
			FHost.CreateValueInput("Index", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIndex);
			FIndex.SetSubType(double.MinValue, double.MaxValue, 1.0, 0.0, false, false, true);
			
			//create configuration inputs
			FHost.CreateValueConfig("Opaque=1?", 1, null, TSliceMode.Single, TPinVisibility.True, out FOpaqueIsOneInput);
			FOpaqueIsOneInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);
			
			//create outputs
			FHost.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMyMeshOutput);
			FMyMeshOutput.Order = int.MinValue;
			FHost.CreateTransformOutput("Transforms", TSliceMode.Dynamic, TPinVisibility.True, out FTransformOutput);
			FHost.CreateTransformOutput("Skinning Transforms", TSliceMode.Dynamic, TPinVisibility.True, out FSkinningTransformOutput);
			FHost.CreateStringOutput("TextureFileName", TSliceMode.Dynamic, TPinVisibility.True, out FTextureFileNameOutput);
			FTextureFileNameOutput.SetSubType("", true);
			FHost.CreateColorOutput("Emissive Color", TSliceMode.Dynamic, TPinVisibility.True, out FEmissiveColorOut);
			FEmissiveColorOut.SetSubType(VColor.Black, false);
			FHost.CreateColorOutput("Diffuse Color", TSliceMode.Dynamic, TPinVisibility.True, out FDiffuseColorOut);
			FDiffuseColorOut.SetSubType(VColor.White, false);
			FHost.CreateColorOutput("Specular Color", TSliceMode.Dynamic, TPinVisibility.True, out FSpecularColorOut);
			FSpecularColorOut.SetSubType(VColor.Black, false);
			FHost.CreateValueOutput("Power", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FShininessOut);
			FShininessOut.SetSubType(0.0, float.MaxValue, 0.1, 25.0, false, false, false);
			FHost.CreateValueOutput("Opaque", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOpaqueOut);
			FOpaqueOut.SetSubType(0.0, 1.0, 0.01, 1.0, false, false, false);
			FHost.CreateValueOutput("Skeleton", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSkeletonOut);
			FSkeletonOut.SetSubType(0.0, float.MaxValue, 0.1, 25.0, false, false, false);
			
			// Debug shit
			/*
			FHost.CreateValueOutput("Vertex Index", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVertexIndexOut);
			FVertexIndexOut.SetSubType(0.0, float.MaxValue, 0.1, 0.0, false, false, false);
			FHost.CreateValueOutput("Vertex", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVertexOut);
			FVertexOut.SetSubType(0.0, float.MaxValue, 0.1, 0.0, false, false, false);
			FHost.CreateValueOutput("VCount", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVCountOut);
			FVCountOut.SetSubType(0.0, float.MaxValue, 0.1, 0.0, false, false, false);
			FHost.CreateValueOutput("Blend Weight Index", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBlendWeightIndexOut);
			FBlendWeightIndexOut.SetSubType(0.0, float.MaxValue, 0.1, 0.0, false, false, false);
			FHost.CreateValueOutput("Blend Weight", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBlendWeightOut);
			FBlendWeightOut.SetSubType(0.0, float.MaxValue, 0.1, 0.0, false, false, false);
			*/
			
			COLLADAUtil.Logger = new LoggerWrapper(FHost);
        } 

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	double value;
			FOpaqueIsOneInput.GetValue(0, out value);
			if (value == 0 && FOpaqueIsOne) 
			{
				FOpaqueIsOne = false;
			}
			else if (value == 1 && !FOpaqueIsOne)
			{
				FOpaqueIsOne = true;
			}
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
        	double tmp;
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FColladaModelIn.PinIsChanged || FIndex.PinIsChanged || FBinSize.PinIsChanged)
			{
        		Log(TLogType.Debug, "Evaluating...");
        		selectedInstanceMeshes.Clear();
        		
        		FUpstreamInterface.GetSlice(0, out FColladaModel);
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
	        		int binSize;
	        		FBinSize.GetValue(0, out tmp);
	        		binSize = (int) tmp;
	        		if (binSize < 0)
	        			binSize = FColladaModel.InstanceMeshes.Count / Math.Abs(binSize);
	        		
					List<Model.BasicMaterial> materialList = new List<Model.BasicMaterial>();
					for (int i = 0; i < FIndex.SliceCount; i++)
					{
						int index;
						FIndex.GetValue(i, out tmp);
						index = ((int) tmp) * binSize;
						index = ((index % FColladaModel.InstanceMeshes.Count) + FColladaModel.InstanceMeshes.Count) % FColladaModel.InstanceMeshes.Count;
						
						for (int j = index; j < index + binSize; j++)
						{
							Model.InstanceMesh instanceMesh = FColladaModel.InstanceMeshes[j % FColladaModel.InstanceMeshes.Count];
							selectedInstanceMeshes.Add(instanceMesh);
							
							Log(TLogType.Debug, "Instance of mesh '" + instanceMesh + "' loaded.");
							
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
							
							// Debug shit
							/*
							List<float> vertexList = new List<float>();
							List<int> vertexIndexList = new List<int>();
							List<int> vCountList = new List<int>();
							List<int> blendWeightIndexList = new List<int>();
							List<float> blendWeightList = new List<float>();
							if (instanceMesh is Model.SkinnedInstanceMesh)
							{
								Model.SkinnedInstanceMesh skinnedInstanceMesh = (Model.SkinnedInstanceMesh) instanceMesh;
								Model.CSkinnedMesh skinnedMesh = (Model.CSkinnedMesh)skinnedInstanceMesh.Mesh;
								Document.Skin skin = skinnedMesh.Skin;
								
								foreach (Document.Primitive primitive in skinnedMesh.Geo.mesh.primitives)
								{
									// foreach indexed vertex
									for (int k = 0; k < primitive.p.Length / primitive.stride; k+=primitive.stride)
									{
										vertexIndexList.Add(primitive.p[k]);
									}
								}
								
								Document.Input positionInput = COLLADAUtil.GetPositionInput(skinnedMesh.Geo.mesh);
								Document.Source positionSource = (Document.Source) positionInput.source;
								Document.Array<float> positions = (Document.Array<float>) positionSource.array;
								for (int k = 0; k < positions.Count; k++)
									vertexList.Add(positions[k]);
								
								for (int k = 0; k < skin.vertexWeights.vcount.Length; k++)
									vCountList.Add((int)skin.vertexWeights.vcount[k]);
								
								for (int k = 1; k < skin.vertexWeights.v.Length; k += 2)
									blendWeightIndexList.Add(skin.vertexWeights.v[k]);
								
								foreach (Document.Input input in skin.vertexWeights.inputs)
								{
									if (input.semantic == "WEIGHT")
									{
										Document.Source weightSource = (Document.Source) input.source;
										Document.Array<float> weights = (Document.Array<float>) weightSource.array;
										for (int k = 0; k < weights.Count; k++)
											blendWeightList.Add(weights[k]);
									}
								}
							}
							
							FVertexIndexOut.SliceCount = vertexIndexList.Count;
							for (int k = 0; k < vertexIndexList.Count; k++)
								FVertexIndexOut.SetValue(k, vertexIndexList[k]);
							FVertexOut.SliceCount = vertexList.Count;
							for (int k = 0; k < vertexList.Count; k++)
								FVertexOut.SetValue(k, vertexList[k]);
							FVCountOut.SliceCount = vCountList.Count;
							for (int k = 0; k < vCountList.Count; k++)
								FVCountOut.SetValue(k, vCountList[k]);
							FBlendWeightIndexOut.SliceCount = blendWeightIndexList.Count;
							for (int k = 0; k < blendWeightIndexList.Count; k++)
								FBlendWeightIndexOut.SetValue(k, blendWeightIndexList[k]);
							FBlendWeightOut.SliceCount = blendWeightList.Count;
							for (int k = 0; k < blendWeightList.Count; k++)
								FBlendWeightOut.SetValue(k, blendWeightList[k]);
								*/
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
						FTextureFileNameOutput.SetString(j, material.Texture);
						if (material.EmissiveColor.HasValue)
							FEmissiveColorOut.SetColor(j, new RGBAColor(material.EmissiveColor.Value.X, material.EmissiveColor.Value.Y, material.EmissiveColor.Value.Z, 1.0));
						else
							FEmissiveColorOut.SetColor(j, VColor.Black);
						if (material.DiffuseColor.HasValue)
							FDiffuseColorOut.SetColor(j, new RGBAColor(material.DiffuseColor.Value.X, material.DiffuseColor.Value.Y, material.DiffuseColor.Value.Z, 1.0));
						else
							FDiffuseColorOut.SetColor(j, VColor.White);
						if (material.SpecularColor.HasValue)
							FSpecularColorOut.SetColor(j, new RGBAColor(material.SpecularColor.Value.X, material.SpecularColor.Value.Y, material.SpecularColor.Value.Z, 1.0));
						else
							FSpecularColorOut.SetColor(j, VColor.Black);
						if (material.SpecularPower.HasValue)
							FShininessOut.SetValue(j, material.SpecularPower.Value);
						else
							FShininessOut.SetValue(j, 25.0);
						// as of FCollada 3.03 opaque = 1.0, before opaque = 0.0
						double alpha = 1.0;
						if (material.Alpha.HasValue)
							alpha = material.Alpha.Value;
						if (!FOpaqueIsOne)
							FOpaqueOut.SetValue(j, 1 - alpha);
						else
							FOpaqueOut.SetValue(j, alpha);
					}
					
					FMyMeshOutput.SliceCount = materialList.Count;
					
					foreach (Mesh m in FDeviceMeshes.Values)
					{
						Log(TLogType.Debug, "Destroying Resource...");
						m.Dispose();
					}
					FDeviceMeshes.Clear();
        		}
			}     
        	
        	if (FColladaModelIn.PinIsChanged || FIndex.PinIsChanged || FBinSize.PinIsChanged || FTimeInput.PinIsChanged)
        	{
        		int maxCount = Math.Max(FTimeInput.SliceCount, selectedInstanceMeshes.Count);
				List<Matrix> transforms = new List<Matrix>();
				for (int i = 0; i < maxCount && selectedInstanceMeshes.Count > 0; i++)
				{
					FTimeInput.GetValue(i, out tmp);
					float time = (float) tmp;
					int meshIndex = i % selectedInstanceMeshes.Count;
					FColladaModel.applyAnimations(time);
					
					//transforms.AddRange(FColladaModel.getTransformsOfUnionMesh(selectedInstanceMeshes));
					Matrix m = FColladaModel.GetAbsoluteTransformMatrix(selectedInstanceMeshes[meshIndex], time);
					for (int j = 0; j < selectedInstanceMeshes[meshIndex].Mesh.Primitives.Count; j++)
						transforms.Add(m);
				}
				
				FTransformOutput.SliceCount = transforms.Count;
				for (int j = 0; j < transforms.Count; j++)
					FTransformOutput.SetMatrix(j, SlimDXToVVVVMatrix(transforms[j]));
				
				// Skinning
				List<Matrix> skinningTransforms = new List<Matrix>();
				List<Vector3> skeletonVertices = new List<Vector3>();
				
				foreach (Model.InstanceMesh instanceMesh in selectedInstanceMeshes)
				{
					if (instanceMesh is Model.SkinnedInstanceMesh) {
						Model.SkinnedInstanceMesh skinnedInstanceMesh = (Model.SkinnedInstanceMesh) instanceMesh;
						try
						{
							skinningTransforms.AddRange(skinnedInstanceMesh.GetSkinningMatrices());
							skeletonVertices.AddRange(skinnedInstanceMesh.GetSkeletonVertices());
						}
						catch (Exception e)
						{
							COLLADAUtil.Log(e);
						}
					}
				}
				
				FSkinningTransformOutput.SliceCount = skinningTransforms.Count;
				for (int j = 0; j < skinningTransforms.Count; j++)
					FSkinningTransformOutput.SetMatrix(j, SlimDXToVVVVMatrix(skinningTransforms[j]));
				
				FSkeletonOut.SliceCount = skeletonVertices.Count * 3;
				for (int j = 0; j < skeletonVertices.Count; j++)
				{
					FSkeletonOut.SetValue3D(j, skeletonVertices[j].X, skeletonVertices[j].Y, skeletonVertices[j].Z);
				}
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
						Log(TLogType.Debug, "Creating Resource...");
						Device dev = Device.FromPointer(new IntPtr(OnDevice));
						try
						{
							m = FColladaModel.createUnion3D9Mesh(dev, selectedInstanceMeshes, false);
							FDeviceMeshes.Add(OnDevice, m);
						}
						catch (Exception e)
						{
							Log(TLogType.Error, e.Message);
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
				Log(TLogType.Debug, "Destroying Resource...");
				FDeviceMeshes.Remove(OnDevice);
				//dispose mesh
				m.Dispose();
			}
		}
		
		public void GetMesh(IDXMeshIO ForPin, int OnDevice, out int Mesh)
		{
			Mesh m = FDeviceMeshes[OnDevice];
			if (m != null)
				Mesh = m.ComPointer.ToInt32();
			else
				Mesh = 0;
		}
		#endregion
        
        #region helper functions
		private void Log(TLogType logType, string message)
		{
			FHost.Log(logType, "ColladaMesh: " + message);
		}
		
		private void Log(string message)
		{
			FHost.Log(TLogType.Debug, message);
		}
		
		private Matrix4x4 SlimDXToVVVVMatrix(Matrix sourceSlimDXMatrix)
		{
			return new Matrix4x4(
				sourceSlimDXMatrix.M11,
				sourceSlimDXMatrix.M12,
				sourceSlimDXMatrix.M13,
				sourceSlimDXMatrix.M14,
				sourceSlimDXMatrix.M21,
				sourceSlimDXMatrix.M22,
				sourceSlimDXMatrix.M23,
				sourceSlimDXMatrix.M24,
				sourceSlimDXMatrix.M31,
				sourceSlimDXMatrix.M32,
				sourceSlimDXMatrix.M33,
				sourceSlimDXMatrix.M34,
				sourceSlimDXMatrix.M41,
				sourceSlimDXMatrix.M42,
				sourceSlimDXMatrix.M43,
				sourceSlimDXMatrix.M44);
		}
		#endregion
	}
}
