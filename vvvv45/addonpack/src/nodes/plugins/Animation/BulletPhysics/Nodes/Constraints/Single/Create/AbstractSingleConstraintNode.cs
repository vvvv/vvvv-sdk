using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;

namespace VVVV.Nodes.Bullet
{
	
	public abstract class AbstractSingleConstraintNode<T> : IPluginEvaluate where T: TypedConstraint
	{
		[Input("World", IsSingle = true,Order=1)]
		protected Pin<BulletRigidSoftWorld> FWorld;

		[Input("Bodies",Order=2)]
		Pin<RigidBody> FBodies;

		[Input("Custom", Order = 100)]
		ISpread<string> FCustom;

		[Input("Custom Object", Order = 101)]
		Pin<ICloneable> FCustomObject;

		[Input("Do Create",IsBang=true,Order=1000)]
		ISpread<bool> FDoCreate;

		protected abstract T CreateConstraint(RigidBody body, int slice);

		public void Evaluate(int SpreadMax)
		{
			if (FBodies.PluginIO.IsConnected && this.FWorld.PluginIO.IsConnected)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FDoCreate[i])
					{
						T cst = this.CreateConstraint(this.FBodies[i], i);

						ConstraintCustomData cust = new ConstraintCustomData();
						cust.Id = this.FWorld[0].GetNewConstraintId();
						cust.Custom = this.FCustom[i];
						cust.IsSingle = true;

						if (FCustomObject.PluginIO.IsConnected)
						{
							cust.CustomObject = FCustomObject[i];
						}

						cst.UserObject = cust;
						this.FWorld[0].Register(cst);

					}
				}
			}
		}
	}
}
