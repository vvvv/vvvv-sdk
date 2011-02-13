using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class RopeSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private Vector3 from;
		private Vector3 to;
		private int res;
		private int fix;

		public RopeSoftShapeDefinition(Vector3 from, Vector3 to,int res,int fix)
		{
			this.from = from;
			this.to = to;
			this.res = res;
			this.fix = fix;
		}
		
		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			si.SparseSdf.Reset();
			SoftBody sb = SoftBodyHelpers.CreateRope(si, from, to, res,fix);
			this.SetConfig(sb);

			return sb;
		}

	}
}


