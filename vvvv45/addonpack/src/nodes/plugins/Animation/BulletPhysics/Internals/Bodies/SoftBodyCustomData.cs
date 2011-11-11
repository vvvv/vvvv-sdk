using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Internals.Bullet
{
	public class SoftBodyCustomData : BodyCustomData
	{
		public bool HasUV { get; set; }
		public float[] UV { get; set; }
	}
}
