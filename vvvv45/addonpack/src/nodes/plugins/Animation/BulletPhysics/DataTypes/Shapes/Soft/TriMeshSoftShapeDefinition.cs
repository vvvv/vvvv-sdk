using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class TriMeshSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private Vector3[] vertices;
		private int[] indices;
		private bool randomizeContraints;

		public TriMeshSoftShapeDefinition(Vector3[] vertices,int[] indices, bool randomizeContraints)
		{
			this.vertices = vertices;
			this.indices = indices;
			this.randomizeContraints = randomizeContraints;
		}

		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			SoftBody sb = SoftBodyHelpers.CreateFromTriMesh(si, this.vertices, this.indices,this.randomizeContraints);
			this.SetConfig(sb);

			return sb;
		}

	}
}
