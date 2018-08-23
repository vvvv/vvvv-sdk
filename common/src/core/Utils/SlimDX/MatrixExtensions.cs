using System;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.Utils.SlimDX
{
	/// <summary>
	/// Extension methods for type <see cref="Matrix">Matrix</see>.
	/// </summary>
	public static class MatrixExtensions
	{
		/// <summary>
		/// Converts a <see cref="Matrix">Matrix</see> to a <see cref="Matrix4x4">Matrix4x4</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Matrix">Matrix</see> to convert.</param>
		/// <returns>The converted <see cref="Matrix4x4">Matrix4x4</see>.</returns>
		public static Matrix4x4 ToMatrix4x4(this Matrix Input)
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
	}
}
