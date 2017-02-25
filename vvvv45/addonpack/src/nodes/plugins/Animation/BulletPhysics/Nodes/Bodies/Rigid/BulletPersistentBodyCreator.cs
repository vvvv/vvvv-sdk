using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "CreateRigidBody", Category = "Bullet", Version="Persist DX9", Author = "vux",
		Help = "Creates a rigid body, and keeps reference as output",AutoEvaluate = true)]
	public class BulletPersistentBodyCreator : AbstractRigidBodyCreator
	{
		private SortedDictionary<int, RigidBody> buffer = new SortedDictionary<int, RigidBody>();
		private BulletRigidSoftWorld m_world; //Cached world;
		private bool connectevent = false;

		[Output("Bodies")]
		ISpread<RigidBody> FOutBodies;

		[Output("Id")]
		ISpread<int> FOutIds;

		public override void Evaluate(int SpreadMax)
		{
			if (this.connectevent)
			{
				this.m_world = this.FWorld[0];
				this.m_world.RigidBodyDeleted += RigidBodyDeleted;
				this.m_world.WorldHasReset += WorldHasReset;
				this.connectevent = false;
			}

			if (this.FWorld.PluginIO.IsConnected)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (this.CanCreate(i))
					{
						int id = 0;
						RigidBody rb = this.CreateBody(i, out id);
						this.buffer.Add(id, rb);
					}
				}

				this.FOutBodies.SliceCount = buffer.Count;
				this.FOutIds.SliceCount = buffer.Count;

				int cnt =0;
				foreach (int id in this.buffer.Keys)
				{
					this.FOutIds[cnt] = id;
					this.FOutBodies[cnt] = this.buffer[id];
					cnt++;
				}
			}
			else
			{
				this.FOutBodies.SliceCount = 0;
				this.FOutIds.SliceCount = 0;
			}
		}

		protected void WorldHasReset()
		{
			this.buffer.Clear();
		}

		protected override void OnWorldConnected()
		{
			this.connectevent = true;
		}

		protected override void OnWorldDiconnected()
		{
			this.m_world.RigidBodyDeleted -= RigidBodyDeleted;
			this.buffer.Clear();
		}

		private void RigidBodyDeleted(RigidBody rb, int id)
		{
			if (this.buffer.ContainsKey(id))
			{
				this.buffer.Remove(id);
			}
		}
	}
}
