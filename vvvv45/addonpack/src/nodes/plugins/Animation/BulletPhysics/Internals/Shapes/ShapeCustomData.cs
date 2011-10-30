using System;
using System.Collections.Generic;
using System.Text;
using VVVV.DataTypes.Bullet;

namespace VVVV.Internals.Bullet
{
	public class ShapeCustomData
	{
		private int id;
		private AbstractRigidShapeDefinition def;

		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		public string CustomString { get; set; }

		//Original shape definition (To build mesh on request)
		public AbstractRigidShapeDefinition ShapeDef
		{
			get { return this.def; }
			set { this.def = value; }
		}

		public ICloneable CustomObject { get; set; }
	}
}
