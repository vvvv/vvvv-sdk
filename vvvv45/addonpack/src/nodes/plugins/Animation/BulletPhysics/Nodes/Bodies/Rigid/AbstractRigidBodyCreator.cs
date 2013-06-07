using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.Utils;
using VVVV.Internals.Bullet;


using BulletSharp;



namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractRigidBodyCreator : IPluginEvaluate, IPluginConnections
	{
		[Input("World", IsSingle = true)]
		protected Pin<BulletRigidSoftWorld> FWorld;

		[Input("Shapes")]
        protected Pin<AbstractRigidShapeDefinition> FShapes;

		[Input("Position")]
        protected ISpread<Vector3D> FPosition;

		[Input("Rotation")]
        protected ISpread<Vector4D> FRotation;

		[Input("Linear Velocity")]
        protected ISpread<Vector3D> FLinearVelocity;

		[Input("Angular Velocity")]
        protected ISpread<Vector3D> FAngularVelocity;

		[Input("Friction")]
        protected ISpread<float> FFriction;

		[Input("Restitution")]
        protected ISpread<float> FRestitution;

		[Input("Is Active",DefaultValue=1)]
        protected ISpread<bool> FActive;

		[Input("Allow Sleep",DefaultValue=1)]
        protected ISpread<bool> FAllowSleep;

		[Input("Has Contact Response", DefaultValue = 1)]
        protected ISpread<bool> FContactResponse;

		[Input("Is Static", DefaultValue = 0)]
        protected ISpread<bool> FStatic;

		[Input("Is Kinematic", DefaultValue = 0)]
        protected ISpread<bool> FKinematic;

		[Input("Custom")]
        protected ISpread<string> FCustom;

		[Input("Custom Object")]
        protected Pin<ICloneable> FCustomObj;

		[Input("Do Create", IsBang = true)]
        protected ISpread<bool> FDoCreate;

		protected virtual void OnWorldConnected() { }
		protected virtual void OnWorldDiconnected() { }

		protected bool CanCreate(int slice)
		{
			return this.FWorld.PluginIO.IsConnected
				&& this.FShapes.PluginIO.IsConnected
				&& this.FDoCreate[slice];
		}

		protected RigidBody CreateBody(int i,out int id)
		{
			if (this.CanCreate(i))
			{
				AbstractRigidShapeDefinition shapedef = this.FShapes[i];

				ShapeCustomData sc = new ShapeCustomData();
				sc.ShapeDef = shapedef;

				Vector3D pos = this.FPosition[i];
				Vector4D rot = this.FRotation[i];

				DefaultMotionState ms = BulletUtils.CreateMotionState(pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);

				CollisionShape shape = shapedef.GetShape(sc);


					Vector3 localinertia = Vector3.Zero;
					if (!this.FKinematic[i] && !this.FStatic[i])
					{
						if (shapedef.Mass > 0.0f)
						{
							shape.CalculateLocalInertia(shapedef.Mass, out localinertia);
						}
					}
				
				RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(shapedef.Mass,
					ms, shape, localinertia);
				rbInfo.Friction = this.FFriction[i];
				rbInfo.Restitution = this.FRestitution[i];

				RigidBody body = new RigidBody(rbInfo);

				if (!this.FActive[i]) { body.ActivationState |= ActivationState.DisableSimulation; }
				if (!this.FAllowSleep[i]) { body.ActivationState = ActivationState.DisableDeactivation; }

				
				body.LinearVelocity = this.FLinearVelocity[i].ToBulletVector();
				body.AngularVelocity = this.FAngularVelocity[i].ToBulletVector();
				body.CollisionFlags = CollisionFlags.None;

				if (!this.FContactResponse[i]) { body.CollisionFlags |= CollisionFlags.NoContactResponse; }
				if (this.FKinematic[i]) { body.CollisionFlags |= CollisionFlags.KinematicObject; }
				if (this.FStatic[i]) { body.CollisionFlags |= CollisionFlags.StaticObject; }

				BodyCustomData bd = new BodyCustomData();

				body.UserObject = bd;
				bd.Id = this.FWorld[0].GetNewBodyId();
				bd.Custom = this.FCustom[i];
				
				if (this.FCustomObj.PluginIO.IsConnected)
				{
					bd.CustomObject = (ICloneable)this.FCustomObj[i].Clone();
				}
				else
				{
					bd.CustomObject = null;
				}

				this.FWorld[0].Register(body);
				id = bd.Id;
				return body;
			}
			else
			{
				id = -1;
				return null;
				
			}
		}

		public abstract void Evaluate(int SpreadMax);

		public void ConnectPin(IPluginIO pin)
		{
			if (pin == this.FWorld.PluginIO)
			{
				this.OnWorldConnected();
			}
		}

		public void DisconnectPin(IPluginIO pin)
		{
			if (pin == this.FWorld.PluginIO)
			{
				this.OnWorldDiconnected();
			}		
		}
	}
}
