using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;

using BulletSharp.SoftBody;



namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "CreateSoftBody", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Creates a soft body", AutoEvaluate = true)]
	public class BulletCreateSoftBodyNode : IPluginEvaluate
	{
		[Input("World", IsSingle = true)]
		Pin<BulletRigidSoftWorld> FWorld;

		[Input("Shapes")]
		Pin<AbstractSoftShapeDefinition> FShapes;

		[Input("Position")]
		ISpread<Vector3D> FPosition;

		[Input("Scale", DefaultValues = new double[] { 1, 1, 1 })]
		ISpread<Vector3D> FScale;

		[Input("Rotate", DefaultValues = new double[] { 0,0,0,1 })]
		ISpread<Vector4D> FRotate;


		[Input("Friction")]
		ISpread<float> FFriction;

		[Input("Restitution")]
		ISpread<float> FRestitution;

		[Input("Custom")]
		ISpread<string> FCustom;

		[Input("Custom Object")]
		Pin<ICloneable> FCustomObj;



		[Input("Do Create", IsBang = true)]
		ISpread<bool> FDoCreate;

		[Output("Body")]
		ISpread<SoftBody> FOutBodies;

		public void Evaluate(int SpreadMax)
		{
			if (this.FWorld.PluginIO.IsConnected && this.FShapes.PluginIO.IsConnected)
			{
				List<SoftBody> bodies = new List<SoftBody>();
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FDoCreate[i])
					{
						AbstractSoftShapeDefinition shapedef = this.FShapes[i];

						SoftBody body = shapedef.GetSoftBody(this.FWorld[0].WorldInfo);

						body.Translate(this.FPosition[i].ToBulletVector());
						body.Scale(this.FScale[i].ToBulletVector());
						body.Rotate(this.FRotate[i].ToBulletQuaternion());

						body.Friction = this.FFriction[i];
						body.Restitution = this.FRestitution[i];

						SoftBodyCustomData bd = new SoftBodyCustomData();
						bd.Id = this.FWorld[0].GetNewBodyId();
						bd.Custom = this.FCustom[i];
						bd.HasUV = shapedef.HasUV;
						bd.UV = shapedef.GetUV(body);
						body.UserObject = bd;

						
						if (this.FCustomObj.PluginIO.IsConnected)
						{
							bd.CustomObject = (ICloneable)this.FCustomObj[i].Clone();
						}
						else
						{
							bd.CustomObject = null;
						}



						this.FWorld[0].Register(body);
						bodies.Add(body);
					}
				}

				this.FOutBodies.SliceCount = bodies.Count;
				for (int i = 0; i < bodies.Count; i++)
				{
					this.FOutBodies[i] = bodies[i];
				}
			}
			else
			{
				this.FOutBodies.SliceCount = 0;
			}

		}
	}
}
