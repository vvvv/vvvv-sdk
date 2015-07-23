using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="SoftWorld", Category="Bullet", Version = "DX9", Author="vux", AutoEvaluate=true)]
	public class BulletSoftWorldNode : IPluginEvaluate
	{
		[Input("Gravity",DefaultValues=new double[] { 0.0,-9.8,0.0 })]
		IDiffSpread<Vector3D> FGravity;

		[Input("Air Density", DefaultValue = 1.2, IsSingle = true)]
		IDiffSpread<float> FAirDensity;

		[Input("TimeStep", DefaultValue = 0.01, IsSingle = true)]
		IDiffSpread<float> FTimeStep;

		[Input("Iterations", DefaultValue = 8, IsSingle = true)]
		IDiffSpread<int> FIterations;

		[Input("Enabled", DefaultValue = 1, IsSingle = true)]
		IDiffSpread<bool> FEnabled;

		[Input("Reset", DefaultValue = 0, IsSingle = true,IsBang=true)]
		ISpread<bool> FReset;

		[Output("World",IsSingle=true)]
		ISpread<BulletRigidSoftWorld> FWorld;

		[Output("Rigid Bodies")]
		ISpread<RigidBody> FRigidBodies;

		[Output("SoftBodies")]
		ISpread<SoftBody> FSoftBodies;

		[Output("Constraints")]
		ISpread<TypedConstraint> FConstraints;

		BulletRigidSoftWorld internalworld = new BulletRigidSoftWorld();

		[Output("Has Reset", DefaultValue = 0, IsSingle = true, IsBang = true)]
		ISpread<bool> FHasReset;

		bool bFirstFrame = true;

		public void Evaluate(int SpreadMax)
		{
			bool hasreset = false;

			if (this.FReset[0] || this.bFirstFrame)
			{
				this.FWorld[0] = this.internalworld;
				this.bFirstFrame = false;
				if (this.internalworld.Created)
				{
					this.internalworld.Destroy();
				}
				this.internalworld.Create();

				hasreset = true;
			}

			if (this.FGravity.IsChanged
				|| this.FAirDensity.IsChanged
				|| hasreset)
			{
				Vector3D g = this.FGravity[0];
				this.internalworld.SetGravity((float)g.x, (float)g.y, (float)g.z);
				this.internalworld.WorldInfo.Gravity = new BulletSharp.Vector3((float)g.x, (float)g.y, (float)g.z);
				this.internalworld.WorldInfo.AirDensity = this.FAirDensity[0];
			}

			if (this.FEnabled.IsChanged || hasreset)
			{
				this.internalworld.Enabled = this.FEnabled[0];
			}

			if (this.FTimeStep.IsChanged || hasreset)
			{
				this.internalworld.TimeStep = this.FTimeStep[0];
			}

			if (this.FIterations.IsChanged || hasreset)
			{
				this.internalworld.Iterations = this.FIterations[0];
			}

			


			if (this.internalworld.Enabled)
			{
				this.internalworld.ProcessDelete();
				this.internalworld.Step();
				//this.internalworld.WorldInfo.SparseSdf.GarbageCollect();
			}

			this.FRigidBodies.SliceCount = this.internalworld.RigidBodies.Count;
			for (int i = 0; i < this.internalworld.RigidBodies.Count; i++)
			{
				this.FRigidBodies[i] = this.internalworld.RigidBodies[i];
			}

			this.FSoftBodies.SliceCount = this.internalworld.SoftBodies.Count;
			for (int i = 0; i < this.internalworld.SoftBodies.Count; i++)
			{
				this.FSoftBodies[i] = this.internalworld.SoftBodies[i];
			}

			this.FConstraints.SliceCount = this.internalworld.Constraints.Count;
			for (int i = 0; i < this.internalworld.Constraints.Count; i++)
			{
				this.FConstraints[i] = this.internalworld.Constraints[i];
			}


			this.FHasReset[0] = hasreset;
		}
	}
}
