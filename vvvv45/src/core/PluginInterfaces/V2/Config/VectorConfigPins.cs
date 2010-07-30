using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class Vector2DConfigPin : ValueConfigPin<Vector2D>
	{
		public Vector2DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector2D this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector2D*)ptr)[index];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector2D*)ptr)[index] = value;
				}
			}
		}
	}
	
		public class Vector3DConfigPin : ValueConfigPin<Vector3D>
	{
		public Vector3DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector3D this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector3D*)ptr)[index];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector3D*)ptr)[index] = value;
				}
			}
		}
	}
		
	public class Vector4DConfigPin : ValueConfigPin<Vector4D>
	{
		public Vector4DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector4D this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector4D*)ptr)[index];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector4D*)ptr)[index] = value;
				}
			}
		}
	}
	
	
	
}
