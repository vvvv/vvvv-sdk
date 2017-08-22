using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using SlimDX.Direct3D9;
using BulletSharp.SoftBody;

namespace VVVV.DataTypes.Bullet
{
	public abstract class AbstractSoftShapeDefinition
	{
		private float mass;
		private float dp;
		private float pr;
		private float df;
		private float lf;
		private AeroModel aeroModel;
		private bool genbend;
		private int benddist;
		private float chr;

		public AeroModel AeroModel
		{
			get { return this.aeroModel; }
			set { this.aeroModel = value; }
		}

		public float Mass
		{
			get { return mass; }
			set { mass = value; }
		}

		public bool IsVolumeMass { get; set; }

		public float PressureCoefficient
		{
			get { return pr; }
			set { pr = value; }
		}

		public float DampingCoefficient
		{
			get { return dp; }
			set { dp = value; }
		}

		public float DragCoefficient { get; set; }
		public float AnchorHardness { get; set; }

		public float LiftCoefficient
		{
			get { return lf; }
			set { lf = value; }
		}

		public float DynamicFrictionCoefficient
		{
			get { return df; }
			set { df = value; }
		}

		public float VolumeConservation { get; set; }

		public bool GenerateBendingConstraints
		{
			get { return this.genbend; }
			set { this.genbend = value; }
		}

		public int BendingDistance
		{
			get { return this.benddist; }
			set { this.benddist = value; }
		}

		public float RigidContactHardness { get; set; }
		public float SoftContactHardness { get; set; }

		public SoftBody GetSoftBody(SoftBodyWorldInfo si)
		{
			SoftBody body = this.CreateSoftBody(si);
			return body;
		}


		protected abstract SoftBody CreateSoftBody(SoftBodyWorldInfo si);
		public virtual bool HasUV { get { return false; } }
		public virtual float[] GetUV(SoftBody sb) { return null; }

		protected void SetConfig(SoftBody sb)
		{
			sb.Cfg.AeroModel = this.AeroModel;
			sb.Cfg.DF = this.DynamicFrictionCoefficient;
			sb.Cfg.DP = this.DampingCoefficient;
			sb.Cfg.PR = this.PressureCoefficient;
			sb.Cfg.LF = this.LiftCoefficient;
			sb.Cfg.VC = this.VolumeConservation;
			sb.Cfg.Collisions |= FCollisions.VFSS;

			sb.Cfg.Chr = this.RigidContactHardness;
			sb.Cfg.Shr = this.SoftContactHardness;


			sb.Cfg.DG = this.DragCoefficient;
			sb.Cfg.Ahr = this.AnchorHardness;
			if (this.IsVolumeMass)
			{
				sb.SetVolumeMass(this.Mass);
			}
			else
			{
				sb.SetTotalMass(this.Mass, false);
			}

			if (this.GenerateBendingConstraints)
			{
				sb.GenerateBendingConstraints(this.BendingDistance);
			}
		}



		
	}
}
