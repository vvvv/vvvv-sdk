using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class PatchSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private Vector3 corner00;
		private Vector3 corner01;
		private Vector3 corner10;
		private Vector3 corner11;
		private int resx,resy;
		private bool diagonals;
		private int fix;
		private float[] uvs;

		public PatchSoftShapeDefinition(Vector3 corner00, Vector3 corner10, Vector3 corner01, Vector3 corner11, int resx, int resy, bool gendiags,int fix)
		{
			this.corner00 = corner00;
			this.corner01 = corner01;
			this.corner10 = corner10;
			this.corner11 = corner11;
			this.resy = resy;
			this.resx = resx;
			this.diagonals = gendiags;
			this.fix = fix;
		}
		
		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			si.SparseSdf.Reset();
			this.uvs = new float[(this.resx - 1) * (this.resy - 1) * 12];
			SoftBody sb = SoftBodyHelpers.CreatePatchUV(si, corner00, corner10, corner01, corner11, resx, resy, fix, diagonals,this.uvs);
			this.SetConfig(sb);

			return sb;
		}

		public override bool HasUV
		{
			get { return true; }
		}

		public override float[]  GetUV(SoftBody sb)
		{
			return this.uvs;
		}

	}
}

