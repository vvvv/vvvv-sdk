using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using SlimDX.Direct3D9;
using BulletSharp;
using System.ComponentModel.Composition;
using VVVV.Internals.Bullet;
using VVVV.Internals.Bullet.EX9;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "RigidBody", Category = "Bullet",Version="EX9.Geometry",
		Help = "Retrieves mesh data for a rigid body", Author = "vux")]
	public class BulletGetRigidBodyMesh : IPluginEvaluate,IPluginDXMesh
	{
		[Input("Bodies")]
		ISpread<RigidBody> FBodies;

		IPluginHost FHost;

		private Dictionary<Device, Mesh> FMeshes = new Dictionary<Device, Mesh>();

	
		protected IDXMeshOut FMeshOut;

		[ImportingConstructor()]
		public BulletGetRigidBodyMesh(IPluginHost host)
		{
			this.FHost = host;
			this.FHost.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out this.FMeshOut);
		}

		public void Evaluate(int SpreadMax)
		{
			int cnt = 0;
            for (int i = 0; i < SpreadMax; i++)
			{
				RigidBody body = this.FBodies[i];
				CollisionShape shape = body.CollisionShape;
				ShapeCustomData sd = (ShapeCustomData)shape.UserObject;

				cnt += sd.ShapeDef.ShapeCount;
			}
			this.FMeshOut.SliceCount = cnt;
		}

		public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
		{
            
            if (this.FMeshes.ContainsKey(OnDevice))
            {
               return this.FMeshes[OnDevice];
            }
            else
            	return null;
		}

		public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
		{
            if (this.FMeshes.ContainsKey(OnDevice))
            {
                this.FMeshes[OnDevice].Dispose();
                this.FMeshes.Remove(OnDevice);// = null;
            }
		}

		public void UpdateResource(IPluginOut ForPin, Device OnDevice)
		{
            if (this.FMeshes.ContainsKey(OnDevice))
            {
				this.FMeshes[OnDevice].Dispose();
				this.FMeshes.Remove(OnDevice);// = null;
			}
            

			List<Mesh> meshes = new List<Mesh>();

			if (this.FBodies.SliceCount > 0)
			{
				int cnt = this.FBodies.SliceCount;

				for (int i = 0; i < cnt; i++)
				{
					
					RigidBody body = this.FBodies[i];
					CollisionShape shape = body.CollisionShape;
					ShapeCustomData sd = (ShapeCustomData)shape.UserObject;
					BulletMesh m = sd.ShapeDef.GetMesh(OnDevice);
					meshes.AddRange(m.Meshes);
				}

                Mesh merge = null;
                if (OnDevice is DeviceEx)
                {
                    merge = Mesh.Concatenate(OnDevice, meshes.ToArray(), MeshFlags.Use32Bit);
                }
                else
                {
                    merge = Mesh.Concatenate(OnDevice, meshes.ToArray(), MeshFlags.Use32Bit | MeshFlags.Managed);
                }

				this.FMeshes.Add(OnDevice,merge);
			}

		}
	}
}
