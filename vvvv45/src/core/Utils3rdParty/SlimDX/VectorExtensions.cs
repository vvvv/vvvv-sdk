using System;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.Utils.SlimDX
{
	/// <summary>
	/// Extension methods for <see cref="Vector2">Vector2</see>, <see cref="Vector3">Vector3</see>
	/// and <see cref="Vector4">Vector4</see>.
	/// </summary>
	public static class VectorExtensions
	{
		/// <summary>
		/// Converts a <see cref="Vector2">Vector2</see> to a <see cref="Vector2D">Vector2D</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Vector2">Vector2</see> to convert.</param>
		/// <returns>The converted <see cref="Vector2D">Vector2D</see>.</returns>
		public static Vector2D ToVector2D(this Vector2 Input)
		{
			Vector2D Result;
			Result.x = Input.X;
			Result.y = Input.Y;
			return Result;
		}
		
		/// <summary>
		/// Converts a <see cref="Vector3">Vector3</see> to a <see cref="Vector3D">Vector3D</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Vector3">Vector3</see> to convert.</param>
		/// <returns>The converted <see cref="Vector3D">Vector3D</see>.</returns>
		public static Vector3D ToVector3D(this Vector3 Input)
		{
			Vector3D Result;
			Result.x = Input.X;
			Result.y = Input.Y;
			Result.z = Input.Z;
			return Result;
		}
		
		/// <summary>
		/// Converts a <see cref="Vector4">Vector4</see> to a <see cref="Vector4D">Vector4D</see>.
		/// </summary>
		/// <param name="Input">The <see cref="Vector4">Vector4</see> to convert.</param>
		/// <returns>The converted <see cref="Vector4D">Vector4D</see>.</returns>
		public static Vector4D ToVector4D(this Vector4 Input)
		{
			Vector4D Result;
			Result.x = Input.X;
			Result.y = Input.Y;
			Result.z = Input.Z;
			Result.w = Input.W;
			return Result;
		}

        /// <summary>
        /// Converts a <see cref="Quaternion">Quaternion</see> to a <see cref="Vector4D">Vector4D</see>.
        /// </summary>
        /// <param name="Input">The <see cref="Quaternion">Quaternion</see> to convert.</param>
        /// <returns>The converted <see cref="Vector4D">Vector4D</see>.</returns>
        public static Vector4D ToVector4D(this Quaternion Input)
        {
            Vector4D Result;
            Result.x = Input.X;
            Result.y = Input.Y;
            Result.z = Input.Z;
            Result.w = Input.W;
            return Result;
        }
	}
}
