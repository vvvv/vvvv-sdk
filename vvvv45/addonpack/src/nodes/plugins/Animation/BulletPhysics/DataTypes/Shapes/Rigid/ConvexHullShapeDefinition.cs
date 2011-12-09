using System;
using System.Collections.Generic;
using System.Text;

using SlimDX.Direct3D9;

using BulletSharp;

using VVVV.Internals.Bullet.EX9;



namespace VVVV.DataTypes.Bullet
{
	public class ConvexHullShapeDefinition : AbstractRigidShapeDefinition
	{
		private Vector3[] vertices;
		private int[] indices;

		public ConvexHullShapeDefinition(Vector3[] vertices, int[] indices)
		{
			this.vertices = vertices;
			this.indices = indices;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}

		protected override CollisionShape CreateShape()
		{
			ConvexHullShape shape = new ConvexHullShape(this.vertices);
            //BvhTriangleMeshShape bvh = new BvhTriangleMeshShape(
            //StridingMeshInterface smi = new StridingMeshInterface();
            //TriangleIndexVertexArray tiv = new TriangleIndexVertexArray(
            //StridingMeshInterface
			return shape;
		}

		protected override BulletMesh CreateMesh(Device device)
		{
			int totalTriangles = this.indices.Length /3;
			int totalVerts = this.vertices.Length;

			Mesh m = new Mesh(device, totalTriangles, totalVerts, MeshFlags.Use32Bit | MeshFlags.SystemMemory, VertexFormat.Position | VertexFormat.Normal);
			SlimDX.DataStream data = m.LockVertexBuffer(LockFlags.None);
			for (int i = 0; i < this.vertices.Length; i++)
			{
					data.Write(this.vertices[i].X);
					data.Write(this.vertices[i].Y);
					data.Write(this.vertices[i].Z);

					data.Write(0.0f);
					data.Write(0.0f);
					data.Write(0.0f);

					//data.Write(this.texcoords[i]);

			}
			m.UnlockVertexBuffer();

			data = m.LockIndexBuffer(LockFlags.None);
			for (int i = 0; i < this.indices.Length; i++)
			{
				data.Write(this.indices[i]);
			}
			m.UnlockIndexBuffer();
			m.ComputeNormals();
			return new BulletMesh(m);
		}
	}
}
