using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

using SlimDX.Direct3D9;
using VVVV.Internals.Bullet.EX9;

namespace VVVV.DataTypes.Bullet
{
	public class CylinderShapeDefinition : AbstractRigidShapeDefinition
	{
		private float hw,hh,hd;
		private int resx = 10;
		private int resy = 10;

		public CylinderShapeDefinition(float hwidth, float hheight, float hdepth,int resx,int resy)
		{
			this.hw = hwidth;
			this.hh = hheight;
			this.hd = hdepth;
			this.resx = resx;
			this.resy = resy;
		}

		public override int ShapeCount
		{
			get { return 1; }
		}


		protected override CollisionShape CreateShape()
		{
			//Cylinder are around Z axis in vvvv
			//If we need Y/X axis, we can rotate, so i use Z
			CollisionShape shape = new CylinderShapeZ(this.hw,this.hh,this.hd);
			//BvhTriangleMeshShape bvh ;//= new BvhTriangleMeshShape(new StridingMeshInterface(),
			//bvh.
			return shape;
		}

		protected override BulletMesh CreateMesh(Device device)
		{

			return new BulletMesh(Mesh.CreateCylinder(device, this.hh, this.hw, this.hd * 2.0f, resx, resy));
		}
	}
}
