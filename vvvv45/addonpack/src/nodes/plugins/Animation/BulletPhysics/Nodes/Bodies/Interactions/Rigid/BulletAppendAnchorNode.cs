using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using BulletSharp.SoftBody;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "AppendAnchor", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Pins a soft body node to a rigid body", AutoEvaluate = true)]
	public class BulletAppendAnchorNode : AbstractBodyInteractionNode<RigidBody>
	{
		[Input("Soft Body")]
		ISpread<SoftBody> FSoft;

		[Input("Node Index")]
		ISpread<int> FNodeIndex;

		[Input("Collide Connected")]
		ISpread<bool> FCollide;


		protected override void ProcessObject(RigidBody obj, int slice)
		{
			SoftBody sb = this.FSoft[slice];
			this.FSoft[slice].AppendAnchor(this.FNodeIndex[slice] % sb.Nodes.Count, obj, FCollide[slice]);
		}
	}
}
