using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Point2Point", Author="vux", Category="Bullet", Version="Constraint.Single DX9", AutoEvaluate=true)]
	public class CreateSingleP2PConstraintNode : AbstractSingleConstraintNode<Point2PointConstraint>
	{
		[Input("Pivot", Order=10)]
		ISpread<Vector3D> FPivot;

		[Input("Damping",Order=11)]
		ISpread<float> FDamping;

		[Input("Impulse Clamp",Order=12)]
		ISpread<float> FImpulseClamp;

		[Input("Tau",Order=13)]
		ISpread<float> FTau;

		protected override Point2PointConstraint CreateConstraint(RigidBody body, int slice)
		{
			Point2PointConstraint cst = new Point2PointConstraint(body, this.FPivot[slice].ToBulletVector());
			cst.Setting.Damping = this.FDamping[slice];
			cst.Setting.ImpulseClamp = this.FImpulseClamp[slice];
			cst.Setting.Tau = this.FTau[slice];
			return cst;	
		}
	}
}
