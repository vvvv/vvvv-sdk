using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.Bullet.Internals.Shapes.Soft
{
	public class GenericSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private Vector3[] nodes;
		private int[] indices;
		private bool randomizeContraints;

		public GenericSoftShapeDefinition(Vector3[] nodes, int[] indices, bool randomizeContraints)
		{
			this.vertices = vertices;
			this.indices = indices;
			this.randomizeContraints = randomizeContraints;
		}

		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			SoftBody sb = new SoftBody(si,
			sb.Nodes.
				sb.Cfg.AeroModel = this.AeroModel;
			sb.Cfg.DF = this.DynamicFrictionCoefficient;
			sb.Cfg.DP = this.DampingCoefficient;
			sb.Cfg.PR = this.PressureCoefficient;
			sb.Cfg.LF = this.LiftCoefficient;
			sb.SetTotalMass(this.Mass, true);
			sb.Cfg.Collisions |= FCollisions.VFSS;// | FCollisions.CLRS | FCollisions.
			sb.Cfg.Chr = this.RigidContactHardness;
			sb.Cfg.DG = this.DragCoefficient;
			sb.Cfg.Ahr = this.AnchorHardness;
			if (this.GenerateBendingConstraints)
			{
				sb.GenerateBendingConstraints(this.BendingDistance);
			}

			return sb;
		}

	}
}
