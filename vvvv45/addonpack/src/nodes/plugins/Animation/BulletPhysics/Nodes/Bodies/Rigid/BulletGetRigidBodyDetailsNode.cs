using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="GetRigidBodyDetails", Category="Bullet", Version = "DX9", 
		Help = "Retrieves details for a rigid body", Author = "vux")]
	public class BulletGetRigidBodyDetailsNode : IPluginEvaluate
	{
		[Input("Bodies")]
		Pin<RigidBody> FBodies;

		[Output("Position")]
		ISpread<Vector3D> FPosition;

		[Output("Rotation")]
		ISpread<Vector4D> FRotation;

		[Output("Linear Velocity")]
		ISpread<Vector3D> FLinVel;

		[Output("Angular Velocity")]
		ISpread<Vector3D> FAngVel;

		[Output("Shapes")]
		ISpread<ISpread<CollisionShape>> FShapes;

		[Output("Shapes Transform")]
		ISpread<ISpread<Matrix4x4>> FShapeTransform;

		[Output("Is Active")]
		ISpread<bool> FActive;

		[Output("Has Contact Response")]
		ISpread<bool> FContactResponse;

		[Output("Is Static")]
		ISpread<bool> FStatic;

		[Output("Is Kinematic")]
		ISpread<bool> FKinematic;


		[Output("Custom")]
		ISpread<string> FCustom;

		[Output("Has Custom Object")]
		ISpread<bool> FHasCustomObj;

		[Output("Custom Object")]
		ISpread<ICloneable> FCustomObj;

		[Output("Id")]
		ISpread<int> FId;

		[Output("Is New")]
		ISpread<bool> FIsNew;

		[Import()]
		ILogger FLogger;


		//[Import()]
		//VVVV. FLog;


		public void Evaluate(int SpreadMax)
		{


			if (this.FBodies.PluginIO.IsConnected)
			{
				this.FId.SliceCount = this.FBodies.SliceCount;
				this.FPosition.SliceCount = this.FBodies.SliceCount;
				this.FRotation.SliceCount = this.FBodies.SliceCount;
				this.FShapes.SliceCount = this.FBodies.SliceCount;
				this.FCustom.SliceCount = this.FBodies.SliceCount;
				this.FIsNew.SliceCount = this.FBodies.SliceCount;
				this.FLinVel.SliceCount = this.FBodies.SliceCount;
				this.FAngVel.SliceCount = this.FBodies.SliceCount;
				this.FActive.SliceCount = this.FBodies.SliceCount;
				this.FContactResponse.SliceCount = this.FBodies.SliceCount;
				this.FStatic.SliceCount = this.FBodies.SliceCount;
				this.FKinematic.SliceCount = this.FBodies.SliceCount;
                this.FShapeTransform.SliceCount = this.FBodies.SliceCount;
                this.FHasCustomObj.SliceCount = this.FBodies.SliceCount;
                this.FCustomObj.SliceCount = this.FBodies.SliceCount;



				List<Matrix4x4> transforms = new List<Matrix4x4>();

				for (int i = 0; i < SpreadMax; i++)
				{
					RigidBody body = this.FBodies[i];


					this.FPosition[i] = new Vector3D(body.MotionState.WorldTransform.M41,
						body.MotionState.WorldTransform.M42, body.MotionState.WorldTransform.M43);

					Quaternion rot = body.Orientation;
					this.FRotation[i] = new Vector4D(rot.X, rot.Y, rot.Z, rot.W);

					this.FLinVel[i] = body.LinearVelocity.ToVVVVector();
					this.FAngVel[i] = body.AngularVelocity.ToVVVVector();

					CollisionShape shape = body.CollisionShape;

					if (shape.IsCompound)
					{
						//CompoundShape sp = new CompoundShape(
						CompoundShape comp = (CompoundShape)shape;
						this.FShapes[i].SliceCount = comp.NumChildShapes;
                        this.FShapeTransform[i].SliceCount = comp.NumChildShapes;


						for (int j = 0; j < comp.NumChildShapes; j++)
						{
							CollisionShape child = comp.GetChildShape(j);

                            this.FShapes[i][j] = child;

							Matrix m = comp.GetChildTransform(j);

							Matrix4x4 mn = new Matrix4x4(m.M11, m.M12, m.M13, m.M14,
								m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34,
								m.M41, m.M42, m.M43, m.M44);

                            mn *= VMath.Scale(child.LocalScaling.ToVVVVector());
                            this.FShapeTransform[i][j] = mn;
							//comp.
							//comp.GetChildTransform(
							//this.FShapes[i][j].
						}
					}
					else
					{
						this.FShapes[i].SliceCount = 1;
						this.FShapes[i][0] = shape;
						this.FShapeTransform[i].SliceCount = 1;



                        this.FShapeTransform[i][0] = VMath.Scale(shape.LocalScaling.ToVVVVector());
						//transforms.Add(VMath.IdentityMatrix);
					}


					BodyCustomData bd = (BodyCustomData)body.UserObject;

					ShapeCustomData sc = (ShapeCustomData)shape.UserObject;

					this.FActive[i] = body.IsActive;
					this.FContactResponse[i] = body.HasContactResponse;
					this.FStatic[i] = body.IsStaticObject;
					this.FKinematic[i] = body.IsKinematicObject;


					//this.FShapeCount[i] = sc.ShapeDef.ShapeCount;
					this.FId[i] = bd.Id;
					this.FIsNew[i] = bd.Created;
					this.FCustom[i] = bd.Custom;
					
					if (bd.CustomObject != null)
					{
						this.FHasCustomObj[i] = true;
						this.FCustomObj[i] = bd.CustomObject;
					}
					else
					{
						this.FHasCustomObj[i] = false;
						this.FCustomObj[i] = null;
					}


				}

				//this.FShapeTransform.SliceCount = transforms.Count;
				//this.FShapeTransform.AssignFrom(transforms);
			}
			else
			{
				this.FId.SliceCount = 0;
				this.FPosition.SliceCount = 0;
				this.FRotation.SliceCount = 0;
				this.FShapes.SliceCount = 0;
				this.FCustom.SliceCount = 0;
				this.FIsNew.SliceCount = 0;
				this.FLinVel.SliceCount = 0;
				this.FAngVel.SliceCount = 0;
                this.FShapeTransform.SliceCount = 0;
			}

		}
	}
}
