using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class EllipsoidSoftShapeDefnition : AbstractSoftShapeDefinition
	{
		private Vector3 center;
		private Vector3 radius;
		private int res;
		private float[] uvs;

		public EllipsoidSoftShapeDefnition(Vector3 center, Vector3 radius, int res)
		{
			this.center = center;
			this.radius = radius;
			this.res = res;
		}
		
		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			si.SparseSdf.Reset();
			SoftBody sb = SoftBodyHelpers.CreateEllipsoid(si, center, radius, res);
			this.uvs = new float[sb.Faces.Count * 6];
			int cnt = 0;
			for (int i = 0; i < sb.Faces.Count; i++)
			{
				 double div = Math.PI;
				 //arccos(x/(r sin(v pi))) ) / (2 pi)
				
				 
				 Vector3 n = sb.Faces[i].N[0].X - center;
				 n.Normalize();
				 this.uvs[cnt + 1] = Convert.ToSingle((Math.Acos(n.Z) / div));
				 this.uvs[cnt] = Convert.ToSingle(Math.Acos(n.X / Math.Sin(this.uvs[cnt + 1] * div)) / (2.0 * div));
				 
				 cnt += 2;
				 n = sb.Faces[i].N[1].X - center;
				 n.Normalize();
				 this.uvs[cnt + 1] = Convert.ToSingle((Math.Acos(n.Z) / div));
				 this.uvs[cnt] = Convert.ToSingle(Math.Acos(n.X / Math.Sin(this.uvs[cnt + 1] * div)) / (2.0 * div));
				 
				 cnt += 2;
				 n = sb.Faces[i].N[2].X - center;
				 n.Normalize();
				 this.uvs[cnt + 1] = Convert.ToSingle((Math.Acos(n.Z) / div));
				 this.uvs[cnt] = Convert.ToSingle(Math.Acos(n.X / Math.Sin(this.uvs[cnt + 1] * div)) / (2.0 * div));
				 
				 cnt += 2;


				/*
				 Vector3 n = sb.Faces[i].N[0].X - center;
				 n.Normalize();
				 this.uvs[cnt] = Convert.ToSingle(n.X / div + 0.5);
				 this.uvs[cnt + 1] = Convert.ToSingle(n.Y / div + 0.5);
				 cnt += 2;
				 n = sb.Faces[i].N[1].X - center;
				 n.Normalize();
				 this.uvs[cnt] = Convert.ToSingle(n.X / div + 0.5);
				 this.uvs[cnt + 1] = Convert.ToSingle(n.Y / div + 0.5);
				 cnt += 2;
				 n = sb.Faces[i].N[2].X - center;
				 n.Normalize();
				 this.uvs[cnt] = Convert.ToSingle(n.X / div + 0.5);
				 this.uvs[cnt + 1] = Convert.ToSingle(n.Y / div + 0.5);
				 cnt += 2;*/
			}

			this.SetConfig(sb);

			return sb;
		}

		public override bool HasUV
		{
			get { return true; }
		}

		public override float[] GetUV(SoftBody sb)
		{
			return this.uvs;
		}
	}
}
