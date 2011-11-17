using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractBodyForceNode : AbstractBodyInteractionNode<RigidBody>
	{
		[Input("Auto Activate", DefaultValue=1, Order = 1000)]
		ISpread<bool> FAutoActivate;

		protected abstract void Apply(RigidBody obj, int slice);

		protected override void ProcessObject(RigidBody obj, int slice)
		{
			if (!obj.IsActive && FAutoActivate[slice])
			{
				obj.Activate(true);
			}
			this.Apply(obj, slice);
		}
	}
}
