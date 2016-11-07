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
		/// <summary>
		/// Decomposes the <see cref="Matrix4x4">matrix</see> into its scalar, rotational, and translational elements. 
		/// </summary>
		/// <param name="m">The matrix to decompose.</param>
		/// <param name="scale">The scalar element.</param>
		/// <param name="rotationQuaternion">The rotational element.</param>
		/// <param name="translation">The translational element.</param>
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
		
		/// <summary>
		/// Decomposes the <see cref="Matrix4x4">matrix</see> into its scalar, rotational, and translational elements. 
		/// </summary>
		/// <param name="m">The matrix to decompose.</param>
		/// <param name="scale">The scalar element.</param>
		/// <param name="rotation">The rotational element.</param>
		/// <param name="translation">The translational element.</param>
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
				
				Vector3 euler = QuaternionToEulerAngleVector3(r);
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
		
		/// <summary>
		/// Blends a matrix <see cref="Matrix4x4">m1</see> by amount1 and a matrix <see cref="Matrix4x4">m2</see> by
		/// amount2 into a new matrix <see cref="Matrix4x4">m</see>.
		/// </summary>
		/// <param name="m1">Matrix 1 to blend.</param>
		/// <param name="m2">Matrix 2 to blend.</param>
		/// <param name="amount1">Amount of matrix 1 to be used in new blended matrix.</param>
		/// <param name="amount2">Amount of matrix 2 to be used in new blended matrix.</param>
		/// <param name="m">The new blenden matrix.</param>
		/// <returns>True if matrix 1 and matrix 2 could be decomposed, otherwise false.</returns>
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
		
		private static Vector3 AngleTo(Vector3 from, Vector3 location)
		{
			Vector3 angle = new Vector3();
			Vector3 v3 = Vector3.Normalize(location - from);
			
			angle.X = (float)Math.Asin(v3.Y);
			angle.Y = (float)Math.Atan2((double)-v3.X, (double)-v3.Z);
			
			return angle;
		}
		
		private static Vector3 QuaternionToEulerAngleVector3(Quaternion rotation)
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
