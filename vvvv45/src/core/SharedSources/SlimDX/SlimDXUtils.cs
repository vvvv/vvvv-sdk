using System;
using VVVV.Utils.VMath;
using SlimDX;

/// <summary>
/// VVVV Shared SlimDX Sources
/// </summary>
namespace VVVV.Shared.VSlimDX
{
	public sealed class VSlimDXUtils
	{
		public static SlimDX.Matrix Matrix4x4ToSlimDXMatrix(Matrix4x4 Input)
		{
			SlimDX.Matrix Result = new Matrix();
			
			Result.M11 = (float) Input.m11;
			Result.M12 = (float) Input.m12;
			Result.M13 = (float) Input.m13;
			Result.M14 = (float) Input.m14;
			
			Result.M21 = (float) Input.m21;
			Result.M22 = (float) Input.m22;
			Result.M23 = (float) Input.m23;
			Result.M24 = (float) Input.m24;
			
			Result.M31 = (float) Input.m31;
			Result.M32 = (float) Input.m32;
			Result.M33 = (float) Input.m33;
			Result.M34 = (float) Input.m34;
			
			Result.M41 = (float) Input.m41;
			Result.M42 = (float) Input.m42;
			Result.M43 = (float) Input.m43;
			Result.M44 = (float) Input.m44;
			
			return Result;
		}
		
		public static Matrix4x4 SlimDXMatrixToMatrix4x4(SlimDX.Matrix Input)
		{
			Matrix4x4 Result;
			
			Result.m11 = (double) Input.M11;
			Result.m12 = (double) Input.M12;
			Result.m13 = (double) Input.M13;
			Result.m14 = (double) Input.M14;			
			
			Result.m21 = (double) Input.M21;
			Result.m22 = (double) Input.M22;
			Result.m23 = (double) Input.M23;
			Result.m24 = (double) Input.M24;			
			
			Result.m31 = (double) Input.M31;
			Result.m32 = (double) Input.M32;
			Result.m33 = (double) Input.M33;
			Result.m34 = (double) Input.M34;			
			
			Result.m41 = (double) Input.M41;
			Result.m42 = (double) Input.M42;
			Result.m43 = (double) Input.M43;
			Result.m44 = (double) Input.M44;			
						
			return Result;
		}
		
		public static bool Decompose(Matrix4x4 m, out Vector3D scale, out Vector4D rotationQuaternion, out Vector3D translation)
		{			
			SlimDX.Vector3 s;
			SlimDX.Quaternion r;
			SlimDX.Vector3 t;
			
			if (Matrix4x4ToSlimDXMatrix(m).Decompose(out s, out r, out t))
			{
				scale.x = s.X;
				scale.y = s.Y;
				scale.z = s.Z;
				rotationQuaternion.x = r.X;
				rotationQuaternion.y = r.Y;
				rotationQuaternion.z = r.Z;
				rotationQuaternion.w = r.W;
				translation.x = t.X;
				translation.y = t.Y;
				translation.z = t.Z;
				
				return true;
			}
			else
			{
				scale = new Vector3D(0);
				rotationQuaternion = new Vector4D(0);
				translation = new Vector3D(0);
				return false;
			}
		}
		
		public static bool Decompose(Matrix4x4 m, out Vector3D scale, out Vector3D rotation, out Vector3D translation)
		{
			SlimDX.Vector3 s;
			SlimDX.Quaternion r;
			SlimDX.Vector3 t;
			
			if (Matrix4x4ToSlimDXMatrix(m).Decompose(out s, out r, out t))
			{
				scale.x = s.X;
				scale.y = s.Y;
				scale.z = s.Z;
				
				SlimDX.Vector3 euler = QuaternionToEulerAngleVector3(r);
				rotation.x = euler.X;
				rotation.y = euler.Y;
				rotation.z = euler.Z;
				
				translation.x = t.X;
				translation.y = t.Y;
				translation.z = t.Z;
				
				return true;
			}
			else
			{
				scale = new Vector3D(0);
				rotation = new Vector3D(0);
				translation = new Vector3D(0);
				return false;
			}
		}
		
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
		
		public static bool Blend(Matrix4x4 m1, Matrix4x4 m2, double amount1, double amount2, out Matrix4x4 m)
		{
			SlimDX.Vector3 s, s1, s2;
			SlimDX.Quaternion r, r1, r2;
			SlimDX.Vector3 t, t1, t2;
			bool success = true;
			
			if (!Matrix4x4ToSlimDXMatrix(m1).Decompose(out s1, out r1, out t1))
				success = false;
			if (!Matrix4x4ToSlimDXMatrix(m2).Decompose(out s2, out r2, out t2))
				success = false;
			
			t = t1 * (Single)amount1 + t2 * (Single)amount2;
			s = new Vector3(1,1,1) + (Single)amount1 * (new Vector3(1,1,1) - s1) + (Single)amount2 * (new Vector3(1,1,1) - s2);
			
			r1 = Quaternion.Slerp(Quaternion.Identity, r1, (Single)amount1);
			r2 = Quaternion.Slerp(Quaternion.Identity, r2, (Single)amount2);
			r = r1*r2;
			r.Normalize();

			m = SlimDXMatrixToMatrix4x4(Matrix.Scaling(s) * Matrix.RotationQuaternion(r) * Matrix.Translation(t));
			
			return success;
		}
		
	}
	
}
