using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.DataTypes.Bullet;

using BulletSharp;
using System.ComponentModel.Composition;

using VVVV.Core.Logging;
using VVVV.Internals.Bullet;
using VVVV.Utils.VMath;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "GetContactDetails", Category = "Bullet", Version = "DX9", Author = "vux")]
	public class BulletGetContactDetailsNode : IPluginEvaluate
	{
		[Input("World")]
		Pin<BulletRigidSoftWorld> FWorld;

		[Output("Body 1")]
		ISpread<RigidBody> FBody1;

		[Output("Body 2")]
		ISpread<RigidBody> FBody2;

		[Output("Contact Points")]
		ISpread<ISpread<ManifoldPoint>> FContactPoints;

		[Import()]
		ILogger FLogger;

		public void Evaluate(int SpreadMax)
		{

			if (this.FWorld.PluginIO.IsConnected)
			{
				int contcnt = this.FWorld[0].World.Dispatcher.NumManifolds;
				this.FBody1.SliceCount = contcnt;
				this.FBody2.SliceCount = contcnt;
				this.FContactPoints.SliceCount = contcnt;

				for (int i = 0; i < contcnt; i++)
				{
					PersistentManifold pm = this.FWorld[0].World.Dispatcher.GetManifoldByIndexInternal(i);
					RigidBody b1 = RigidBody.Upcast(pm.Body1());
					RigidBody b2 = RigidBody.Upcast(pm.Body2());

					this.FBody1[i] = b1;
					this.FBody2[i] = b2;

					this.FContactPoints[i].SliceCount = pm.GetNumContacts();
					for (int j = 0; j < pm.GetNumContacts(); j++)
					{
						this.FContactPoints[i][j] = pm.GetContactPoint(j);
					}

				}
			}
			else
			{
				FBody1.SliceCount = 0;
				FBody2.SliceCount = 0;
			}
		}


	}
}
