using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.DataTypes.Bullet;

using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	//[PluginInfo(Name = "CollisionCheck", Category = "Bullet", Version = "DX9", Author = "vux")]
	public class BulletCollisionCheckNode : IPluginEvaluate
	{
		[Input("World",IsSingle=true)]
		Pin<BulletRigidSoftWorld> FWorld;

		[Input("Body 1")]
		Pin<RigidBody> FBody1;

		[Input("Body 2")]
		Pin<CollisionObject> FBody2;

		[Output("Collision")]
		ISpread<bool> FCollision;

		public void Evaluate(int SpreadMax)
		{
			if (FBody1.PluginIO.IsConnected &&
				FBody2.PluginIO.IsConnected &&
				FWorld.PluginIO.IsConnected)
			{
				this.FCollision.SliceCount = SpreadMax;
				for (int i = 0; i < SpreadMax; i++)
				{
					//CollisionWorld.ContactResultCallback cb;// = new CollisionWorld.ContactResultCallback();
					//this.FWorld[0].World.Broadphase.
					//this.FWorld[0].World.ContactPairTest(
					//this.FCollision[i] = FBody1[i].CheckCollideWithOverride(FBody2[i]);
					//this.FWorld[0].World.Broadphase.OverlappingPairCache.OverlappingPairArray[0].Proxy0.
				}
			}
			else
			{
				FCollision.SliceCount = 0;
			}
		}
	}
}
