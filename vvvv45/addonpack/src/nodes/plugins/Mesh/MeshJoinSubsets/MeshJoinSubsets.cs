#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Mesh", Category = "EX9.Geometry", Version = "Join Subsets", 
              Help = "Creates a mesh with subsets from Position, Normals, Texturecoordinates and Indices", 
              Tags = "mesh, subset, geometry, spreadable", 
              Author = "woei")]
	#endregion PluginInfo
	public class MeshJoinSubsetsNode : DXMeshOutPluginBase, IPluginEvaluate
	{
		#region fields & pins
		[Input("Position")]
		IDiffSpread<Vector3> FPos;

		[Input("Normal", DefaultValues = new double[]{0.0,0.0,1.0})]
		IDiffSpread<Vector3> FNorm;
		
		[Input("Texture Coordinate")]
		IDiffSpread<Vector2> FTexcd;

		[Input("Indices", StepSize = 1)]
		IDiffSpread<ISpread<Vector3>> FInd;
		
		[Output("Centroid", Order = 1)]
		ISpread<Vector3D> FCen;
		
		[Import()]
		ILogger FLogger;
		
		private bool reinit;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public MeshJoinSubsetsNode(IPluginHost host) : base(host)
		{
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			reinit = false;
			//update mesh
			if (FPos.IsChanged || FNorm.IsChanged || FTexcd.IsChanged) {
				Update();
			}
			
			//recreate mesh
			if (FInd.IsChanged || reinit) {
				Reinitialize();
			}
		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Mesh CreateMesh(Device device)
		{
			var meshPin = FMeshOut;
			meshPin.SliceCount = FInd.SliceCount;
			
			int faceCount = 0;
			foreach (ISpread<Vector3> s in FInd)
				faceCount+=s.SliceCount;
			
			var mesh = new Mesh(device, faceCount , FPos.SliceCount, MeshFlags.Managed, VertexFormat.PositionNormal | VertexFormat.Texture1);
			
			var vertices = mesh.LockVertexBuffer(LockFlags.None);
			for (int v = 0; v < FPos.SliceCount; v++) {
				vertices.Write(FPos[v]);
				vertices.Write(FNorm[v]);
				vertices.Write(FTexcd[v]);
			}
			mesh.UnlockVertexBuffer();
			
			Vector3 min = new Vector3(0), max = new Vector3(0), newmin, newmax;
			FCen.SliceCount=FInd.SliceCount;
			
			var attributes = mesh.LockAttributeBuffer(LockFlags.None);			
			var indices = mesh.LockIndexBuffer(LockFlags.None);
			for (int a = 0; a < FInd.SliceCount; a++)
			{
				for(int i=0; i<FInd[a].SliceCount; i++)
				{
					attributes.Write(a);
					indices.Write((short)(FInd[a][i].X));
					indices.Write((short)(FInd[a][i].Y));
					indices.Write((short)(FInd[a][i].Z));
					
					Vector3 pos = FPos[(int)FInd[a][i].X];
					if (i==0)
					{
						min = pos;
						max = pos;
					}
					else
					{	
						Vector3.Minimize(ref min, ref pos,out newmin);
						Vector3.Maximize(ref max, ref pos,out newmax);
						min = newmin;
						max = newmax;
					}
					pos = FPos[(int)FInd[a][i].Y];
					Vector3.Minimize(ref min, ref pos,out newmin);
					Vector3.Maximize(ref max, ref pos,out newmax);
					min = newmin;
					max = newmax;
					pos = FPos[(int)FInd[a][i].Z];
					Vector3.Minimize(ref min, ref pos,out newmin);
					Vector3.Maximize(ref max, ref pos,out newmax);
					min = newmin;
					max = newmax;
				}
				
				FCen[a]=new Vector3D((min.X+max.X)/2.0,(min.Y+max.Y)/2.0,(min.Z+max.Z)/2.0);
			}
			mesh.UnlockAttributeBuffer();
			mesh.UnlockIndexBuffer();

			return mesh;
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its mesh, here you can alter the data of the mesh
		protected override void UpdateMesh(Mesh mesh)
		{
			//do something with the mesh data
			var vertices = mesh.LockVertexBuffer(LockFlags.None);
			int vCount = Math.Max(FPos.SliceCount, FNorm.SliceCount);
			vCount = Math.Max(vCount, FTexcd.SliceCount);
			
			if (mesh.VertexCount == vCount)
			{
				for (int i = 0; i < mesh.VertexCount; i++) {
					vertices.Write(FPos[i]);
					vertices.Write(FNorm[i]);
					vertices.Write(FTexcd[i]);
				}
			}
			else
			{
				reinit = true;
			}
			mesh.UnlockVertexBuffer();
		}
	}
}
