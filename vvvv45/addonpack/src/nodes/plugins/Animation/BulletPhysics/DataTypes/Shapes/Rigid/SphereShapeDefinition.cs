using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

using SlimDX.Direct3D9;

using VVVV.Internals.Bullet.EX9;

namespace VVVV.DataTypes.Bullet
{
	public class SphereShapeDefinition : AbstractRigidShapeDefinition
	{
		private float radius;
		private int resx = 10;
		private int resy = 10;

		public SphereShapeDefinition(float radius,int resx,int resy)
		{
			this.radius = radius;
			this.resx = resx;
			this.resy = resy;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}

		protected override CollisionShape CreateShape()
		{
			CollisionShape shape = new SphereShape(this.radius);
			//shape.CalculateLocalInertia(this.Mass);
			return shape;
		}

		protected override BulletMesh CreateMesh(Device device)
		{
			return new BulletMesh(Mesh.CreateSphere(device, this.radius, resx, resy));	
		}
	}
}
