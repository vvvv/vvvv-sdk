using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "UpdateBody", Category = "Bullet", Version="Rigid DX9", Author = "vux",
		Help = "Updates rigid body properties", AutoEvaluate = true)]
	public class BulletUpdateRigidBodyNode : IPluginEvaluate
	{
		[Input("Bodies", Order = 0)]
		ISpread<RigidBody> FInput;

		[Input("Position")]
		ISpread<Vector3D> FPosition;

		[Input("Rotation")]
		ISpread<Vector4D> FRotation;

		[Input("Set Position Rotation", IsBang = true)]
		IDiffSpread<bool> FSetPosition;

		[Input("Linear Velocity")]
		ISpread<Vector3D> FLinVel;

		[Input("Set Linear Velocity", IsBang = true)]
		IDiffSpread<bool> FSetLinVel;

		[Input("Angular Velocity")]
		ISpread<Vector3D> FAngVel;

		[Input("Set Angular Velocity", IsBang = true)]
		IDiffSpread<bool> FSetAngVel;

		[Input("Activate", IsBang = true)]
		IDiffSpread<bool> FSetActive;

        [Input("Disable", IsBang = true)]
        IDiffSpread<bool> FSetDisabled;

		[Input("Mass")]
		ISpread<float> FMass;

		[Input("Set Mass", IsBang = true)]
		IDiffSpread<bool> FSetMass;

		public void Evaluate(int SpreadMax)
		{
			for (int i = 0; i < SpreadMax; i++)
			{
				RigidBody rb = this.FInput[i];
				
				if (this.FSetPosition[i]) 
				{ 
					Vector3 v = this.FPosition[i].ToBulletVector();
					//rb.MotionState.WorldTransform.M41 = 2.0f;
					//rb.MotionState.WorldTransform.M41 = v.X;

					Matrix m = rb.MotionState.WorldTransform;
					//m.M41 = v.X;
					//m.M42 = v.Y;
					//m.M43 = v.Z;
					//rb.MotionState.WorldTransform = m;
					Matrix tr = Matrix.Translation(v);
					Matrix rot = Matrix.RotationQuaternion(this.FRotation[i].ToBulletQuaternion());
					rb.WorldTransform  = Matrix.Multiply(rot, tr);
                    rb.MotionState.WorldTransform = Matrix.Multiply(rot, tr); ;
					}
				if (this.FSetLinVel[i]) { rb.LinearVelocity = this.FLinVel[i].ToBulletVector(); }
				if (this.FSetAngVel[i]) { rb.AngularVelocity = this.FAngVel[i].ToBulletVector(); }
				if (this.FSetMass[i]) { rb.SetMassProps(FMass[i], Vector3.Zero); }
                if (this.FSetActive[i]) { rb.ForceActivationState(ActivationState.ActiveTag); }
                if (this.FSetDisabled[i]) { rb.ForceActivationState(ActivationState.DisableSimulation); }
                //rb.
				//rb.SetMassProps(
				
			}
		}
	}
}
