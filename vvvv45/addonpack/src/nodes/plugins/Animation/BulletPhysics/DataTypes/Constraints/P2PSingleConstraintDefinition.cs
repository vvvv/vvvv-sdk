using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class P2PSingleConstraintDefinition : AbstractSingleBodyConstraintDef
	{
		public Vector3 Pivot { get; set; }
		public float Damping { get; set; }
		public float Tau { get; set; }
		public float ImpulseClamp { get; set; }

		//public void Test()
		//{
		//	BulletSharp.Point2PointConstraint pt;
		//	/BulletSharp.Generic6DofConstraint sdof = new Generic6DofConstraint(
		//	pt.Setting.
		 //sdof.sett
		//}

		public override TypedConstraint GetConstraint(RigidBody body)
		{
			Point2PointConstraint cst = new Point2PointConstraint(body, this.Pivot);
			cst.Setting.Damping = this.Damping;
			cst.Setting.ImpulseClamp = this.ImpulseClamp;
			cst.Setting.Tau = this.Tau;

			return cst;
		}
	}
}
