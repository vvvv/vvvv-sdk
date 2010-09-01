using System;
using SlimDX;
using VVVV.Utils.SlimDX;
	
namespace VVVV.Utils.VMath
{
	/// <summary>
	/// VMath Matrix utils.
	/// </summary>
	public static class Matrix4x4Utils
	{
		public static bool Decompose(this Matrix4x4 m, out Vector3D scale, out Vector4D rotationQuaternion, out Vector3D translation)
		{
			Vector3 s;
			Quaternion r;
			Vector3 t;
			
			if (m.ToSlimDXMatrix().Decompose(out s, out r, out t))
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
		
		public static bool Decompose(this Matrix4x4 m, out Vector3D scale, out Vector3D rotation, out Vector3D translation)
		{
			Vector3 s;
			Quaternion r;
			Vector3 t;
			
			if (m.ToSlimDXMatrix().Decompose(out s, out r, out t))
			{
				scale.x = s.X;
				scale.y = s.Y;
				scale.z = s.Z;
				
				Vector3 euler = VectorUtils.QuaternionToEulerAngleVector3(r);
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
		
		public static bool Blend(Matrix4x4 m1, Matrix4x4 m2, double amount1, double amount2, out Matrix4x4 m)
		{
			Vector3 s, s1, s2;
			Quaternion r, r1, r2;
			Vector3 t, t1, t2;
			bool success = true;
			
			if (!m1.ToSlimDXMatrix().Decompose(out s1, out r1, out t1))
				success = false;
			if (!m2.ToSlimDXMatrix().Decompose(out s2, out r2, out t2))
				success = false;
			
			t = t1 * (Single)amount1 + t2 * (Single)amount2;
			s = new Vector3(1,1,1) + (Single)amount1 * (new Vector3(1,1,1) - s1) + (Single)amount2 * (new Vector3(1,1,1) - s2);
			
			r1 = Quaternion.Slerp(Quaternion.Identity, r1, (Single)amount1);
			r2 = Quaternion.Slerp(Quaternion.Identity, r2, (Single)amount2);
			r = r1*r2;
			r.Normalize();

			m = (Matrix.Scaling(s) * Matrix.RotationQuaternion(r) * Matrix.Translation(t)).ToMatrix4x4();
			
			return success;
		}
	}
}
