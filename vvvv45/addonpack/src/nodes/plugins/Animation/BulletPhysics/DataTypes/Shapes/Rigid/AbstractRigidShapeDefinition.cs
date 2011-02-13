using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

using SlimDX.Direct3D9;

using VVVV.Internals.Bullet.EX9;
using VVVV.Internals.Bullet;

namespace VVVV.DataTypes.Bullet
{
	public abstract class AbstractRigidShapeDefinition
	{
		private float mass;
		protected BulletMesh mesh;
		
	

		protected Quaternion localRotation = Quaternion.Identity;
		protected Vector3 localTranslation = Vector3.Zero;
		
		public abstract int ShapeCount { get; }

		public BulletMesh GetMesh(Device device)
		{
			//Will handle multi screen later
			if (this.mesh == null)
			{
				this.mesh = this.CreateMesh(device);
			}
			return this.mesh;

		}

		public virtual float Mass
		{
			get { return mass; }
			set { mass = value; }
		}

		public Vector3 Scaling { get; set; }

		public Quaternion Rotation
		{
			get { return this.localRotation; }
			set { this.localRotation = value; }
		}

		public Vector3 Translation
		{
			get { return this.localTranslation; }
			set { this.localTranslation = value; }
		}

		public string CustomString { get; set; }
		public ICloneable CustomObject { get; set; }

		public CollisionShape GetShape(ShapeCustomData sc)
		{
			CollisionShape shape = this.CreateShape();
			shape.LocalScaling = this.Scaling;
			sc.CustomString = this.CustomString;
			if (sc.CustomObject != null) { sc.CustomObject = (ICloneable)this.CustomObject.Clone(); }
			shape.UserObject = sc;
			shape.CalculateLocalInertia(this.Mass);

			return shape;
		}

		/*Override that to manage shape
		 * Probably will split the mesh to the shape def
		 * but does the job so far
		 */
		protected abstract CollisionShape CreateShape();

		protected abstract BulletMesh CreateMesh(Device device);

		public void Dispose()
		{
			if (this.mesh != null)
			{
				this.mesh.Dispose();
				this.mesh = null;
			}
		}

		
	}
}
