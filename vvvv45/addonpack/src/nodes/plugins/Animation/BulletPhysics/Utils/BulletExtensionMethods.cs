using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.VMath;


public static class BulletExtensions
{
	public static Vector3D Abs(this Vector3D vec)
	{
		return new Vector3D(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
	}

	public static BulletSharp.Vector3 ToBulletVector(this Vector3D vec)
	{
		return new BulletSharp.Vector3((float)vec.x, (float)vec.y, (float)vec.z);
	}

	public static BulletSharp.Quaternion ToBulletQuaternion(this Vector4D vec)
	{
		return new BulletSharp.Quaternion((float)vec.x, (float)vec.y, (float)vec.z,(float)vec.w);
	}

	public static Vector3D ToVVVVector(this BulletSharp.Vector3 vec)
	{
		return new Vector3D(vec.X, vec.Y, vec.Z);
	}
}



