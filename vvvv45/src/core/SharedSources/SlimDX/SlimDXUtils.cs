using System;
using VVVV.Utils.VMath;
using SlimDX;

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
				rotation.x = r.X;
				rotation.y = r.Y;
				rotation.z = r.Z;
				rotation.w = r.W;
				translation.x = t.X;
				translation.y = t.Y;
				translation.z = t.Z;
				
				return true;
			}
			else
			{
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
				rotation.x = Math.Atan2(2*(r.X * r.Y + r.Z * r.W), 1 - 2 * (r.Y * r.Y + r.Z * r.Z))
				rotation.y = Math.Asin(2 * (r.X * r.Z - r.W * r.Y));
				rotation.z = Math.Atan2(2 * (r.X * r.W + r.Y * r.Z), 1 - 2 * (r.Z * r.Z + r.W * r.W));
				translation.x = t.X;
				translation.y = t.Y;
				translation.z = t.Z;
				
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
