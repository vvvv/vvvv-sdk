using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;
using VVVV.Internals.Bullet;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "RayCast", Category = "Bullet", Version = "DX9", Author = "vux")]
	public class BulletRayCastNode : IPluginEvaluate
	{
		[Input("World", IsSingle = true)]
		Pin<BulletRigidSoftWorld> FWorld;

		[Input("From")]
		ISpread<Vector3D> FFrom;

		[Input("To")]
		ISpread<Vector3D>FTo;

		[Output("Hit")]
		ISpread<bool> FHit;

		[Output("Hit Fraction")]
		ISpread<double> FHitFraction;

		[Output("Hit Position")]
		ISpread<Vector3D> FHitPosition;

		[Output("Hit Normal")]
		ISpread<Vector3D> FHitNormal;

		[Output("Query Index")]
		ISpread<int> FQueryIndex;

		[Output("Body")]
		ISpread<RigidBody> FBody;

		[Output("Body Id")]
		ISpread<int> FId;

		public void Evaluate(int SpreadMax)
		{


			if (this.FWorld.PluginIO.IsConnected)
			{
				this.FHit.SliceCount = SpreadMax;

				List<double> fraction = new List<double>();
				List<Vector3D> position = new List<Vector3D>();
				List<Vector3D> normal = new List<Vector3D>();
				List<RigidBody> body = new List<RigidBody>();
				List<int> bodyid = new List<int>();
				List<int> qidx = new List<int>();

				for (int i = 0; i < SpreadMax; i++)
				{
					Vector3 from = this.FFrom[i].ToBulletVector();
					Vector3 to = this.FTo[i].ToBulletVector();
					CollisionWorld.ClosestRayResultCallback cb =
						new CollisionWorld.ClosestRayResultCallback(from,to );

					this.FWorld[0].World.RayTest(from, to, cb);

					if (cb.HasHit)
					{
						this.FHit[i] = true;
						BodyCustomData bd = (BodyCustomData)cb.CollisionObject.UserObject;
						fraction.Add(cb.ClosestHitFraction);
						position.Add(cb.HitPointWorld.ToVVVVector());
						normal.Add(cb.HitNormalWorld.ToVVVVector());
						body.Add((RigidBody)cb.CollisionObject);
						bodyid.Add(bd.Id);
						qidx.Add(i);
					}
					else
					{
						this.FHit[i] = false;
					}
				}

				this.FId.AssignFrom(bodyid);
				this.FHitFraction.AssignFrom(fraction);
				this.FHitNormal.AssignFrom(normal);
				this.FHitPosition.AssignFrom(position);
				this.FQueryIndex.AssignFrom(qidx);
				this.FBody.AssignFrom(body);
			}
			else
			{
				this.FHit.SliceCount = 0;
				this.FId.SliceCount = 0;
				this.FHitFraction.SliceCount = 0;
				this.FHitPosition.SliceCount = 0;
			}

		}
	}
}
