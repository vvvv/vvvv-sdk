using System;
using SlimDX;

namespace VVVV.Utils.SlimDX
{
	/// <summary>
	/// SlimDX vector utils.
	/// </summary>
	public static class VectorUtils
	{
		public static Vector3 AngleTo(Vector3 from, Vector3 location)
		{
			Vector3 angle = new Vector3();
			Vector3 v3 = Vector3.Normalize(location - from);
			
			angle.X = (float)Math.Asin(v3.Y);
			angle.Y = (float)Math.Atan2((double)-v3.X, (double)-v3.Z);
			
			return angle;
		}
		
		public static Vector3 QuaternionToEulerAngleVector3(Quaternion rotation)
		{
			Vector3 rotationaxes = new Vector3();
			Vector4 forward4 = Vector3.Transform(new Vector3(0, 0, -1), rotation);
			Vector4 up4 = Vector3.Transform(new Vector3(0, 1, 0), rotation);
			Vector3 forward = new Vector3(forward4.X, forward4.Y, forward4.Z);
			Vector3 up = new Vector3(up4.X, up4.Y, up4.Z);
			
			rotationaxes = AngleTo(new Vector3(), forward);
			
			if (rotationaxes.X == Math.PI/2)
			{
				rotationaxes.Y = (float)Math.Atan2((double)up.X, (double)up.Z);
				rotationaxes.Z = 0;
			}
			else if (rotationaxes.X == -Math.PI/2)
			{
				rotationaxes.Y = (float)Math.Atan2((double)-up.X, (double)-up.Z);
				rotationaxes.Z = 0;
			}
			else
			{
				up4 = Vector3.Transform(up, Matrix.RotationY(-rotationaxes.Y));
				up = new Vector3(up4.X, up4.Y, up4.Z);
				up4 = Vector3.Transform(up, Matrix.RotationX(-rotationaxes.X));
				up = new Vector3(up4.X, up4.Y, up4.Z);
				
				rotationaxes.Z = (float)Math.Atan2((double)-up.X, (double)up.Y);
			}
			
			return rotationaxes;
		}
	}
}
