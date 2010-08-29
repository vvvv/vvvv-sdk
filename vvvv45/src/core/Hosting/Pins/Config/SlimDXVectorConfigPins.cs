using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Config
{
	public class Vector2ConfigPin : ValueConfigPin<Vector2>
	{
		public Vector2ConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector2 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector2D*)ptr)[index % FSliceCount].ToSlimDXVector();
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector2D*)ptr)[index % FSliceCount] = value.ToVector2D();
				}
			}
		}
	}
	
		public class Vector3ConfigPin : ValueConfigPin<Vector3>
	{
		public Vector3ConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector3 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector3D*)ptr)[index % FSliceCount].ToSlimDXVector();;
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector3D*)ptr)[index % FSliceCount] = value.ToVector3D();
				}
			}
		}
	}
		
	public class Vector4ConfigPin : ValueConfigPin<Vector4>
	{
		public Vector4ConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector4 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector4D*)ptr)[index % FSliceCount].ToSlimDXVector();;
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector4D*)ptr)[index % FSliceCount] = value.ToVector4D();
				}
			}
		}
	}
	
	
	
}
