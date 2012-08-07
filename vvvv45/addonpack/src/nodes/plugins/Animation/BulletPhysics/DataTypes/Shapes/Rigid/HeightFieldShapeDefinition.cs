using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using BulletSharp;
using System.IO;
using System.Runtime.InteropServices;
using VVVV.Internals.Bullet.EX9;

namespace VVVV.DataTypes.Bullet
{
	public class HeightFieldShapeDefinition : AbstractRigidShapeDefinition
	{
		private int w, l;
		private float[] h;
		private float minh, maxh;
		private MemoryStream ms;
		

		public HeightFieldShapeDefinition(int w, int l, float[] h, float minh, float maxh)
		{
			this.w = w;
			this.l = l;
			this.h = h;
			this.minh = minh;
			this.maxh = maxh;
			this.ms = null;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}


		protected override CollisionShape CreateShape()
		{
			if (this.ms == null)
			{
				byte[] terr = new byte[this.w * this.l * 4];
				ms = new MemoryStream(terr);
				BinaryWriter writer = new BinaryWriter(ms);
				for (int i = 0; i < this.w * this.l; i++)
				{
					writer.Write(this.h[i]);
				}
				writer.Flush();
			}
			ms.Position = 0;
			HeightfieldTerrainShape hs = new HeightfieldTerrainShape(w, l, ms, 0.0f, minh, maxh, 1, PhyScalarType.PhyFloat, false);
			hs.SetUseDiamondSubdivision(true);
			//hs.LocalScaling = new Vector3(this.sx, 1.0f, this.sz);
			return hs;
			
		}

		protected override BulletMesh CreateMesh(Device device)
		{
			if (this.ms == null)
			{
				byte[] terr = new byte[this.w * this.l * 4];
				ms = new MemoryStream(terr);
				BinaryWriter writer = new BinaryWriter(ms);
				for (int i = 0; i < this.w * this.l; i++)
				{
					writer.Write(this.h[i]);
				}
				writer.Flush();
			}
			ms.Position = 0;
			BinaryReader reader = new BinaryReader(ms);

			int totalTriangles = (this.w - 1) * (this.l - 1) * 2;
			int totalVerts = this.w * this.l;

			Mesh m = new Mesh(device, totalTriangles, totalVerts, MeshFlags.Use32Bit | MeshFlags.SystemMemory, VertexFormat.Position | VertexFormat.Normal);
			SlimDX.DataStream data = m.LockVertexBuffer(LockFlags.None);

			float center = (maxh + minh) / 2.0f;
			center = -center;
			for (int i = 0; i < this.w; i++)
			{
				for (int j = 0; j < this.l; j++)
				{
					float height= reader.ReadSingle();

					data.Write((j - (this.l-1) * 0.5f));
					data.Write(height + center);
					data.Write((i - (this.w-1) * 0.5f));

					data.Write(0.0f);
					data.Write(0.0f);
					data.Write(0.0f);

				}
			}
			m.UnlockVertexBuffer();

			data = m.LockIndexBuffer(LockFlags.None);
			for (int i = 0; i < this.w - 1; i++)
			{
				for (int j = 0; j < this.l - 1; j++)
				{
					data.Write(j * this.w + i);
					data.Write(j * this.w + i + 1);
					data.Write((j + 1) * this.w + i + 1);

					data.Write(j * this.w + i);
					data.Write((j + 1) * this.w + i + 1);
					data.Write((j + 1) * this.w + i);
				}
			}
			m.UnlockIndexBuffer();


			m.ComputeNormals();


			return new BulletMesh(m);

		}
	}
}
