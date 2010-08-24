using System;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using SlimDX;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Description of KnownTypes.
	/// </summary>
	public sealed class KnownTypes
	{
		private static Type[] Types =
		{
			typeof(double),
			typeof(float),
			typeof(int),
			typeof(bool),
			typeof(string),
			typeof(EnumEntry),
			typeof(RGBAColor),
			typeof(Vector2D),
			typeof(Vector3D),
			typeof(Vector4D),
			typeof(Matrix4x4),
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Matrix),
			typeof(Enum),
			typeof(EnumEntry)
		};
		
		public static bool IsKnown(Type type)
		{
			var known = false;
			foreach (var t in Types)
			{
				if(type == t)
				{
					known = true;
					break;
				}
			}
			
			if(!known)
			{
				known = type.BaseType == typeof(Enum);
			}
			
			return known;
		}
	}
}
