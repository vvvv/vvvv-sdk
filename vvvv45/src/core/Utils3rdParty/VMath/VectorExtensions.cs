using System;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.Utils.VMath
{
	/// <summary>
	/// Extension methods for <see cref="Vector2D">Vector2D</see>, <see cref="Vector3D">Vector3D</see>
	/// and <see cref="Vector4D">Vector4D</see>.
	/// </summary>
	public static class VectorExtensions
	{
		/// <summary>
		/// Converts a <see cref="Vector2D">Vector2D</see> to a <see cref="Vector2">Vector2</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Vector2D">Vector2D</see> to convert.</param>
		/// <returns>The converted <see cref="Vector2">Vector2</see>.</returns>
		public static Vector2 ToSlimDXVector(this Vector2D Input)
		{
			Vector2 Result;
			Result.X = (float) Input.x;
			Result.Y = (float) Input.y;
			return Result;
		}
		
		/// <summary>
		/// Converts a <see cref="Vector3D">Vector3D</see> to a <see cref="Vector3">Vector3</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Vector3D">Vector3D</see> to convert.</param>
		/// <returns>The converted <see cref="Vector3">Vector3</see>.</returns>
		public static Vector3 ToSlimDXVector(this Vector3D Input)
		{
			Vector3 Result;
			Result.X = (float) Input.x;
			Result.Y = (float) Input.y;
			Result.Z = (float) Input.z;
			return Result;
		}
		
		/// <summary>
		/// Converts a <see cref="Vector4D">Vector4D</see> to a <see cref="Vector4">Vector4</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Vector4D">Vector4D</see> to convert.</param>
		/// <returns>The converted <see cref="Vector4">Vector4</see>.</returns>
		public static Vector4 ToSlimDXVector(this Vector4D Input)
		{
			Vector4 Result;
			Result.X = (float) Input.x;
			Result.Y = (float) Input.y;
			Result.Z = (float) Input.z;
			Result.W = (float) Input.w;
			return Result;
		}
	}
}
