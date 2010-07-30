using System;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public static class ExtensionMethods
	{
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
