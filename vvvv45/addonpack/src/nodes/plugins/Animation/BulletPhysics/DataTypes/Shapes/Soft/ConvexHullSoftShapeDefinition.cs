using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class ConvexHullSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private Vector3[] vertices;
		private bool randomizeContraints;

		public ConvexHullSoftShapeDefinition(Vector3[] vertices, bool randomizeContraints)
		{
			this.vertices = vertices;
			this.randomizeContraints = randomizeContraints;
		}

		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			si.SparseSdf.Reset();
			SoftBody sb = SoftBodyHelpers.CreateFromConvexHull(si, this.vertices, this.randomizeContraints);
			this.SetConfig(sb);

			return sb;
		}
	}
}
