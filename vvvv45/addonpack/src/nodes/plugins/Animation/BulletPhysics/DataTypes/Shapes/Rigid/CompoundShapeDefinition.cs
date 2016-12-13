using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;

using SlimDX.Direct3D9;

using VVVV.Internals.Bullet;
using VVVV.Internals.Bullet.EX9;


namespace VVVV.DataTypes.Bullet
{
	public class CompoundShapeDefinition : AbstractRigidShapeDefinition
	{
		private List<AbstractRigidShapeDefinition> children;
		//private float mass;

		public CompoundShapeDefinition(List<AbstractRigidShapeDefinition> children)
		{
			this.children = children;
			//this.mass = mass;
		}

		public List<AbstractRigidShapeDefinition> Children
		{
			get { return children; }
		}

		
		public override float Mass
		{
			get
			{
				float mass = 0;
				foreach (AbstractRigidShapeDefinition def in this.children)
				{
					mass += def.Mass;
				}
				return mass;
			}
			set
			{
				base.Mass = value;
			}
		}

		public override int ShapeCount
		{
			get 
			{
				int cnt = 0;
				foreach (AbstractRigidShapeDefinition def in this.children)
				{
					cnt += def.ShapeCount;
				}
				return cnt;
			}
		}


		protected override CollisionShape CreateShape()
		{
			CompoundShape shape = new CompoundShape();
			foreach (AbstractRigidShapeDefinition shapedef in this.children)
			{
				ShapeCustomData sc = new ShapeCustomData();
				sc.Id = 0;
				sc.ShapeDef = shapedef;

				Matrix tr = Matrix.Translation(shapedef.Translation);
				Matrix rot = Matrix.RotationQuaternion(shapedef.Rotation);

				shape.AddChildShape(Matrix.Multiply(rot, tr), shapedef.GetShape(sc));
			}
			return shape;
		}

		protected override BulletMesh CreateMesh(Device device)
		{
			List<Mesh> meshes = new List<Mesh>();
			foreach (AbstractRigidShapeDefinition def in this.children)
			{
				meshes.AddRange(def.GetMesh(device).Meshes);
			}

			return new BulletMesh(meshes);
		
			//return meshes;
			//Build the box mesh
			//return Mesh.CreateBox(device, this.w * 2.0f, this.h * 2.0f, this.d * 2.0f);
			//return null;
		}
	}
}

