using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D9;
using BulletSharp;
using System.ComponentModel.Composition;
using BulletSharp.SoftBody;
using VVVV.Internals.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "SoftBody", Category = "Bullet", Version="EX9.Geometry DX9",
		Help = "Gets a soft body data as mesh", Author = "vux")]
	public class BulletGetSoftBodyMesh : IPluginEvaluate,IPluginDXMesh
	{
		[Input("Bodies")]
		ISpread<SoftBody> FBodies;

		[Output("Is Valid")]
		ISpread<bool> FValid;

		IPluginHost FHost;

		private Mesh FMesh;
		private Dictionary<int, Mesh> FMeshes = new Dictionary<int, Mesh>();

	
		protected IDXMeshOut FMeshOut;

		[ImportingConstructor()]
		public BulletGetSoftBodyMesh(IPluginHost host)
		{
			this.FHost = host;
			this.FHost.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out this.FMeshOut);
		}

		public void Evaluate(int SpreadMax)
		{
			
			this.FValid.SliceCount = SpreadMax;

			int validcnt = 0;
			for (int i = 0; i < SpreadMax; i++)
			{
				FValid[i] = this.FBodies[i].Faces.Count > 0 || this.FBodies[i].Tetras.Count > 0;
				if (FValid[i]) { validcnt++; }
			}

			this.FMeshOut.SliceCount = validcnt;
		}

		public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
		{
			if (this.FMesh != null)
			{
				return this.FMesh;
			}
			else
			{
				return null;
			}		
		}

		public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
		{
			if (this.FMesh != null)
			{
				this.FMesh.Dispose();
				this.FMesh = null;
			}
		}

		public void UpdateResource(IPluginOut ForPin, Device OnDevice)
		{
			List<Mesh> soft = new List<Mesh>();

			if (this.FMesh != null)
			{
				this.FMesh.Dispose();
				this.FMesh = null;
			}
			if (this.FBodies.SliceCount > 0)
			{
				int cnt = this.FBodies.SliceCount;

				for (int i = 0; i < cnt; i++)
				{
					SoftBody body = this.FBodies[i];

					SoftBodyCustomData sc = (SoftBodyCustomData)body.UserObject;

					AlignedFaceArray faces = body.Faces;

					if (FValid[i])
					{
						if (body.Faces.Count > 0)
						{
							#region Build from Faces
							VertexFormat decl;
							if (sc.HasUV)
							{
								decl = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;
							}
							else
							{
								decl = VertexFormat.Position | VertexFormat.Normal;
							}

							Mesh mesh = new Mesh(OnDevice, faces.Count, faces.Count * 3, MeshFlags.SystemMemory | MeshFlags.Use32Bit, decl);

							SlimDX.DataStream verts = mesh.LockVertexBuffer(LockFlags.None);
							SlimDX.DataStream indices = mesh.LockIndexBuffer(LockFlags.None);

							int j;
							int uvcnt = 0;
							for (j = 0; j < faces.Count; j++)
							{
								NodePtrArray nodes = faces[j].N;
								verts.Write(nodes[0].X);
                                verts.Write(nodes[0].Normal);
								//verts.Position += 12;

								if (sc.HasUV)
								{
									verts.Write(sc.UV[uvcnt]);
									uvcnt++;
									verts.Write(sc.UV[uvcnt]);
									uvcnt++;
								}

								verts.Write(nodes[1].X);
                                verts.Write(nodes[1].Normal);

								//verts.Position += 12;

								if (sc.HasUV)
								{
									verts.Write(sc.UV[uvcnt]);
									uvcnt++;
									verts.Write(sc.UV[uvcnt]);
									uvcnt++;
								}

								verts.Write(nodes[2].X);
                                verts.Write(nodes[2].Normal);
								//verts.Position += 12;

								if (sc.HasUV)
								{
									verts.Write(sc.UV[uvcnt]);
									uvcnt++;
									verts.Write(sc.UV[uvcnt]);
									uvcnt++;
								}

								indices.Write(j * 3);
								indices.Write(j * 3 + 1);
								indices.Write(j * 3 + 2);

							}

							mesh.UnlockVertexBuffer();
							mesh.UnlockIndexBuffer();
							//mesh.ComputeNormals();

							soft.Add(mesh);
							#endregion
						}
						else
						{
							#region Build from tetras
							int tetraCount = body.Tetras.Count;
							int vertexCount = tetraCount * 12;
							
							VertexFormat decl;
							if (sc.HasUV)
							{
								decl = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;
							}
							else
							{
								decl = VertexFormat.Position | VertexFormat.Normal;
							}

							Mesh mesh = new Mesh(OnDevice, tetraCount * 4, vertexCount, MeshFlags.SystemMemory | MeshFlags.Use32Bit, decl);

							
							SlimDX.DataStream indices = mesh.LockIndexBuffer(LockFlags.Discard);

							for (int idx = 0; idx < vertexCount; idx++) { indices.Write(idx); }
							mesh.UnlockIndexBuffer();

							SlimDX.DataStream verts = mesh.LockVertexBuffer(LockFlags.None);
							foreach (Tetra t in body.Tetras)
							{
								NodePtrArray nodes = t.Nodes;

								verts.Write(nodes[2].X);
                                verts.Write(nodes[2].Normal);
								//verts.Position += 12;
								verts.Write(nodes[1].X);
                                verts.Write(nodes[1].Normal);
								//verts.Position += 12;
								verts.Write(nodes[0].X);
                                verts.Write(nodes[0].Normal);
								//verts.Position += 12;

								verts.Write(nodes[0].X);
                                verts.Write(nodes[0].Normal);
								//verts.Position += 12;
								verts.Write(nodes[1].X);
                                verts.Write(nodes[1].Normal);
								//verts.Position += 12;
								verts.Write(nodes[3].X);
                                verts.Write(nodes[3].Normal);
								//verts.Position += 12;

								verts.Write(nodes[2].X);
                                verts.Write(nodes[2].Normal);
								//verts.Position += 12;
								verts.Write(nodes[3].X);
                                verts.Write(nodes[3].Normal);
								//verts.Position += 12;
								verts.Write(nodes[1].X);
                                verts.Write(nodes[1].Normal);
								//verts.Position += 12;

								verts.Write(nodes[2].X);
                                verts.Write(nodes[2].Normal);
								//verts.Position += 12;
								verts.Write(nodes[0].X);
                                verts.Write(nodes[0].Normal);
								//verts.Position += 12;
								verts.Write(nodes[3].X);
                                verts.Write(nodes[3].Normal);
								//verts.Position += 12;
							}

							mesh.UnlockVertexBuffer();
							mesh.UnlockIndexBuffer();
							//mesh.ComputeNormals();

							soft.Add(mesh);
							#endregion
						}


					}
				}

                Mesh merge = null;
                if (OnDevice is DeviceEx)
                {
                    merge = Mesh.Concatenate(OnDevice, soft.ToArray(), MeshFlags.Use32Bit);
                }
                else
                {
                    merge = Mesh.Concatenate(OnDevice, soft.ToArray(), MeshFlags.Use32Bit | MeshFlags.Managed);
                }


                this.FMesh = merge;

				foreach (Mesh m in soft)
				{
					m.Dispose();
				}
			}

		}
	}
}
