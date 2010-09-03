using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{

	public class Vector2DOutputPin : ValueOutputPin<Vector2D>
	{
		public Vector2DOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector2D this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector2D*)ptr)[VMath.Zmod(index, FSliceCount)];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector2D*)ptr)[VMath.Zmod(index, FSliceCount)] = value;
				}
			}
		}
	}
	
	public class Vector3DOutputPin : ValueOutputPin<Vector3D>
	{
		public Vector3DOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector3D this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector3D*)ptr)[VMath.Zmod(index, FSliceCount)];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector3D*)ptr)[VMath.Zmod(index, FSliceCount)] = value;
				}
			}
		}
	}
	
	public class Vector4DOutputPin : ValueOutputPin<Vector4D>
	{
		public Vector4DOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector4D this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector4D*)ptr)[VMath.Zmod(index, FSliceCount)];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector4D*)ptr)[VMath.Zmod(index, FSliceCount)] = value;
				}
			}
		}
	}

}
