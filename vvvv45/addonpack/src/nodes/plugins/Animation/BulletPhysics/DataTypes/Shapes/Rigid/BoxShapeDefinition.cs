using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

using SlimDX.Direct3D9;

using VVVV.Internals.Bullet.EX9;

namespace VVVV.DataTypes.Bullet
{
	public class BoxShapeDefinition : AbstractRigidShapeDefinition
	{
		private float w,h,d;

		public override int ShapeCount
		{
			get { return 1; }
		}

		public BoxShapeDefinition(float width, float height, float depth)
		{
			this.w = width / 2.0f;
			this.h = height / 2.0f;
			this.d = depth / 2.0f;
		}


		protected override CollisionShape CreateShape()
		{
			CollisionShape shape = new BoxShape(this.w,this.h,this.d);
			return shape;
		}

		protected override BulletMesh CreateMesh(Device device)
		{
			//Build the box mesh
			return new BulletMesh(Mesh.CreateBox(device, this.w * 2.0f, this.h * 2.0f, this.d * 2.0f));
		}


	}
}
