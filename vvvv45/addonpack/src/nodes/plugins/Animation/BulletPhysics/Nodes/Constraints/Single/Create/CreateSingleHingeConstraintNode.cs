using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Hinge", Author="vux", Category="Bullet", Version="Constraint.Single DX9",AutoEvaluate=true)]
	public class CreateSingleHingeConstraintNode : AbstractSingleConstraintNode<HingeConstraint>
	{
		[Input("Pivot", Order=10)]
		ISpread<Vector3D> FPivot;

		[Input("Axis", Order = 11)]
		ISpread<Vector3D> FAxis;

		[Input("Use Reference Frame", Order = 12)]
		ISpread<bool> FUseRef;

		protected override HingeConstraint CreateConstraint(RigidBody body, int slice)
		{
			HingeConstraint cst = new HingeConstraint(body, this.FPivot[slice].ToBulletVector(),FAxis[slice].ToBulletVector(),FUseRef[slice]);
			//cst.SetLimit(
			return cst;	
		}
	}
}
