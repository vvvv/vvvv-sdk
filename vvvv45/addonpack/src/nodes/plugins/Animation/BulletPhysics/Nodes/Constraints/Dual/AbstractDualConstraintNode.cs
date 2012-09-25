using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.DataTypes.Bullet;

using BulletSharp;


namespace VVVV.Nodes.Bullet
{
	
	public abstract class AbstractDualConstraintNode<T> : IPluginEvaluate where T :TypedConstraint
	{
		[Input("World", IsSingle = true,Order=0)]
		protected Pin<BulletRigidSoftWorld> FWorld;

		[Input("Body 1",Order=1)]
		Pin<RigidBody> FBody1;

		[Input("Body 2", Order = 2)]
		Pin<RigidBody> FBody2;

		[Input("Collide Connected")]
		ISpread<bool> FCollideConnected;

		[Input("Do Create",IsBang=true,Order=500)]
		ISpread<bool> FDoCreate;

		protected abstract T CreateConstraint(RigidBody body1,RigidBody body2, int slice);

		public void Evaluate(int SpreadMax)
		{
			if (this.FBody1.PluginIO.IsConnected 
				&& this.FWorld.PluginIO.IsConnected
				&& this.FBody2.PluginIO.IsConnected)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FDoCreate[i])
					{
						T cst = this.CreateConstraint(this.FBody1[i],this.FBody2[i], i);
						this.FWorld[0].World.AddConstraint(cst, !this.FCollideConnected[i]);
					}
				}
			}
		}
	}
}
