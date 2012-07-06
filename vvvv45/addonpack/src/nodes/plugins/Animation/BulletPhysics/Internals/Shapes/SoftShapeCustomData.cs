using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DataTypes.Bullet;

namespace VVVV.Internals.Bullet
{
	public class SoftShapeCustomData
	{
		private int id;
		private AbstractSoftShapeDefinition def;

		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		//Original shape definition (To build mesh on request)
		public AbstractSoftShapeDefinition ShapeDef
		{
			get { return this.def; }
			set { this.def = value; }
		}

		public ICloneable CustomObject { get; set; }

	}
}
