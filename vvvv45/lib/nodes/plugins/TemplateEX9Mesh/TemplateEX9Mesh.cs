#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template",
	            Category = "EX9.Geometry",
	            Version = "",
	            Help = "Basic template which creates a mesh",
	            Tags = "c#")]
	#endregion PluginInfo
	public class Template : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		public class MeshData
		{
			public Vector3 Randomness;
			public int Resolution;
		}
		
		#region fields & pins
		
        [Input("Randomize")]
        public ISpread<Vector3> FRandomizeIn;
		
		[Input("Reset", IsBang = true)]
        public ISpread<bool> FResetIn;

		[Input("Resolution", DefaultValue = 20, IsSingle = true)]
        public ISpread<int> FResoIn;

        [Output("Mesh")]
        public ISpread<MeshResource<MeshData>> FMeshOut;

		[Import()]
        public ILogger FLogger;

		#endregion fields & pins
		
		public void OnImportsSatisfied()
		{
			FMeshOut.SliceCount = 0;
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			FMeshOut.ResizeAndDispose(spreadMax, CreateMeshResource);
			for (int i = 0; i < spreadMax; i++)
			{
				var meshResource = FMeshOut[i];
				var meshData = meshResource.Metadata;
				
				//recreate mesh
				if (meshData.Resolution != FResoIn[i] || FResetIn[i])
				{
					meshResource.Dispose();
					meshResource = CreateMeshResource(i);
					meshData = meshResource.Metadata;
				}
			
				//update mesh
				if (meshData.Randomness != FRandomizeIn[i])
				{
					meshData.Randomness = FRandomizeIn[i];
					meshResource.NeedsUpdate = true;
				}
				else
				{
					meshResource.NeedsUpdate = false;
				}
				FMeshOut[i] = meshResource;
			}
		}
		
		MeshResource<MeshData> CreateMeshResource(int slice)
		{
			var meshData = new MeshData() { Randomness = FRandomizeIn[slice], Resolution = FResoIn[slice] };
			return MeshResource.Create(meshData, CreateMesh, UpdateMesh);
		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		Mesh CreateMesh(MeshData meshData, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating Mesh...");
			var resolution = Math.Max(meshData.Resolution, 2);
			return Mesh.CreateSphere(device, 1, resolution, resolution);
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its mesh, here you can alter the data of the mesh
		void UpdateMesh(MeshData meshData, Mesh mesh)
		{
			FLogger.Log(LogType.Debug, "Updating Mesh...");
			//do something with the mesh data
			var vertices = mesh.LockVertexBuffer(LockFlags.None);

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                //get the vertex content
			    var pos = vertices.Read<Vector3>();
			    var norm = vertices.Read<Vector3>();

			    pos.X = (pos.X + meshData.Randomness.X) % 2 - 0;
			    pos.Y = (pos.Y + meshData.Randomness.Y) % 2 - 1;
			    pos.Z = (pos.Z + meshData.Randomness.Z) % 2 - 0;
			    
			    //to write the data move the stream position back!
			    vertices.Position -= mesh.BytesPerVertex;
			    vertices.Write(pos);
			    vertices.Write(norm);
			}
			
			mesh.UnlockVertexBuffer();
		}
	}
}
