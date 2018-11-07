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
	//[PluginInfo(Name = "SplitBody", Category = "Bullet", Version = "DX9", Author = "vux", AutoEvaluate = true,Ignore=true)]
	public class BulletSplitRigidBodyNode : AbstractBodyInteractionNode<RigidBody>
	{
		[Input("World")]
		Pin<BulletRigidSoftWorld> FWorld;

		protected override void ProcessObject(RigidBody obj, int slice)
		{
			if (obj.CollisionShape.IsCompound && FWorld.PluginIO.IsConnected)
			{
				CompoundShape comp = (CompoundShape)obj.CollisionShape;
				BodyCustomData bc = (BodyCustomData)obj.UserObject;
				bc.MarkedForDeletion = true;


				for (int i = 0; i < comp.ChildList.Count; i++)
				{
					CollisionShape shape = comp.GetChildShape(i);
					
					float mass = 1.0f / obj.InvMass;
					float massshape = mass / (float)comp.ChildList.Count;

					Vector3 inert;
					shape.CalculateLocalInertia(massshape,out inert);

					Matrix m = obj.MotionState.WorldTransform;
					MotionState ms = new DefaultMotionState(m);
					//List<RigidBody> bodies = new List<RigidBody>();

					RigidBody rb = new RigidBody(new RigidBodyConstructionInfo(mass, ms, shape, inert));
					rb.LinearVelocity = obj.LinearVelocity;
					rb.AngularVelocity = obj.AngularVelocity;
					rb.Restitution = obj.Restitution;
					rb.Friction = obj.Friction;
					//rb.CollisionShape = shape;

					BodyCustomData copy = new BodyCustomData();
					copy.Id = this.FWorld[0].GetNewBodyId();
					copy.Custom = bc.Custom;

					rb.UserObject = copy;

					this.FWorld[0].Register(rb);

					//bodies.Add(rb);
				}



				
			}
		}
	}
}
