using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	//[PluginInfo(Name="Point2Point", Author="vux", Category="Bullet", Version="Constraint.Dual DX9", AutoEvaluate=true)]
	public class CreateDualP2PConstraintNode : AbstractDualConstraintNode<Point2PointConstraint>
	{
		[Input("Pivot 1", Order = 10)]
		ISpread<Vector3D> FPivot1;

		[Input("Pivot 2", Order = 11)]
		ISpread<Vector3D> FPivot2;

		[Input("Damping", Order = 12)]
		ISpread<float> FDamping;

		[Input("Impulse Clamp", Order = 13)]
		ISpread<float> FImpulseClamp;

		[Input("Tau", Order = 14)]
		ISpread<float> FTau;

		protected override Point2PointConstraint CreateConstraint(RigidBody body1,RigidBody body2, int slice)
		{
			Point2PointConstraint cst = new Point2PointConstraint(body1, body2,
				this.FPivot1[slice].ToBulletVector(), this.FPivot2[slice].ToBulletVector());
			cst.Setting.Damping = this.FDamping[slice];
			cst.Setting.ImpulseClamp = this.FImpulseClamp[slice];
			cst.Setting.Tau = this.FTau[slice];
			return cst;	
		}
	}
}
