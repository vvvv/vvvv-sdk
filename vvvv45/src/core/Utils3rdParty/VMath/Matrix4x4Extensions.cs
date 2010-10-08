using System;
using SlimDX;

namespace VVVV.Utils.VMath
{
	/// <summary>
	/// Extension methods for type <see cref="Matrix4x4">Matrix4x4</see>.
	/// </summary>
	public static class Matrix4x4Extensions
	{
		/// <summary>
		/// Converts a <see cref="Matrix4x4">Matrix4x4</see> to a <see cref="Matrix">Matrix</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Matrix4x4">Matrix4x4</see> to convert.</param>
		/// <returns>The converted <see cref="Matrix">Matrix</see>.</returns>
		public static Matrix ToSlimDXMatrix(this Matrix4x4 Input)
		{
			Matrix Result;
			
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
	}
}
