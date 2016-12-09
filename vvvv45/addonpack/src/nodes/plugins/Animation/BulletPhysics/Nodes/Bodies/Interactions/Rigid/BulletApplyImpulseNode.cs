using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "ApplyImpulse", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Applies an impulse on a rigid body", AutoEvaluate = true)]
	public class BulletApplyImuplseNode : AbstractBodyForceNode
	{
		[Input("Position",Order=1)]
		ISpread<Vector3D> FPosition;

		[Input("Impulse",Order=2)]
		ISpread<Vector3D> FImpulse;

		protected override void Apply(RigidBody obj, int slice)
		{
			Vector3D pos = this.FPosition[slice];
			Vector3D force = this.FImpulse[slice];

			obj.ApplyImpulse(new Vector3((float)force.x, (float)force.y, (float)force.z),
				new Vector3((float)pos.x, (float)pos.y, (float)pos.z));
		}
	}
}
