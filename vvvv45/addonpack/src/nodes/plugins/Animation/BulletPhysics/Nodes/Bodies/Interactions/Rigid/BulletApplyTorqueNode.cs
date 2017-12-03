using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "ApplyTorque", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Applies a torque on a rigid body", AutoEvaluate = true)]
	public class BulletApplyTorqueNode : AbstractBodyForceNode
	{
		[Input("Torque",Order=1)]
		ISpread<Vector3D> FTorque;

		protected override void Apply(RigidBody obj, int slice)
		{
			Vector3D torque = this.FTorque[slice];

			obj.ApplyTorque(new Vector3((float)torque.x, (float)torque.y, (float)torque.z));
		}
	}
}
