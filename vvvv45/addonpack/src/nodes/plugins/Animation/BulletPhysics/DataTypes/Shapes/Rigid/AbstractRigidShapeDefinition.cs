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
        protected Dictionary<int, BulletMesh> mesh = new Dictionary<int, BulletMesh>();
		
	

		protected Quaternion localRotation = Quaternion.Identity;
		protected Vector3 localTranslation = Vector3.Zero;
		
		public abstract int ShapeCount { get; }

		public BulletMesh GetMesh(Device device)
		{
			//Will handle multi screen later
			if (!this.mesh.ContainsKey(device.ComPointer.ToInt32()))
			{
                this.mesh.Add(device.ComPointer.ToInt32(),this.CreateMesh(device));
			}
            return this.mesh[device.ComPointer.ToInt32()];
		}

        public void DestroyMesh(Device device)
        {
            if (!this.mesh.ContainsKey(device.ComPointer.ToInt32()))
            {
                this.mesh[device.ComPointer.ToInt32()].Dispose();
            }
            this.mesh.Remove(device.ComPointer.ToInt32());
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
            foreach (BulletMesh m in this.mesh.Values)
			{
				m.Dispose();
			}
		}

		
	}
}
