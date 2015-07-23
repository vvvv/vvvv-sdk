using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Bullet.Utils;

using BulletSharp;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "CreateRigidBody", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Creates a rigid body", AutoEvaluate = true)]
	public class BulletCreateRigidBodyNode : AbstractRigidBodyCreator
	{
		[Output("Bodies")]
		ISpread<RigidBody> FOutBodies;

		[Output("Id")]
		ISpread<int> FOutIds;

		public override void Evaluate(int SpreadMax)
		{
			List<RigidBody> output = new List<RigidBody>();
			List<int> outid = new List<int>();

			for (int i = 0; i < SpreadMax; i++)
			{
				if (this.CanCreate(i))
				{
					int id;
					RigidBody rb = this.CreateBody(i,out id);
					output.Add(rb);
					outid.Add(id);
				}
			}

			this.FOutBodies.SliceCount = output.Count;
			this.FOutIds.SliceCount = outid.Count;

			for (int i = 0; i < output.Count; i++)
			{
				this.FOutBodies[i] = output[i];
				this.FOutIds[i] = outid[i];
			}
		}
	}
}
