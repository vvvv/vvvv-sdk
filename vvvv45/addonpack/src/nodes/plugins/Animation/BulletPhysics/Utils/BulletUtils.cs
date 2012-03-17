using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace VVVV.Bullet.Utils
{
	public class BulletUtils
	{
		public static DefaultMotionState CreateMotionState(double px, double py, double pz, double rx, double ry, double rz, double rw)
		{
			//KinematicCharacterController c;// = new KinematicCharacterController(
			//c.
			Matrix tr = Matrix.Translation((float)px, (float)py, (float)pz);
			Matrix rot = Matrix.RotationQuaternion(new Quaternion((float)rx, (float)ry, (float)rz, (float)rw));
			return new DefaultMotionState(Matrix.Multiply(rot, tr));
		}
	}
}
