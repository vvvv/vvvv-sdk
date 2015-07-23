using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "ApplyForce", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Applies a force on a rigid body", AutoEvaluate = true)]
	public class BulletApplyForceNode : AbstractBodyForceNode
	{
		[Input("Position", Order = 1)]
		ISpread<Vector3D> FPosition;

		[Input("Force", Order = 2)]
		ISpread<Vector3D> FForce;

		protected override void Apply(RigidBody obj, int slice)
		{
			Vector3D pos = this.FPosition[slice];
			Vector3D force = this.FForce[slice];

			obj.ApplyForce(new Vector3((float)force.x, (float)force.y, (float)force.z),
				new Vector3((float)pos.x, (float)pos.y, (float)pos.z));
		}
	}
}
