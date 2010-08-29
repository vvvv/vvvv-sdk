using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Output
{

	public class Vector2OutputPin : ValueOutputPin<Vector2>
	{
		public Vector2OutputPin(IPluginHost host, OutputAttribute attribute)
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
	
	public class Vector3OutputPin : ValueOutputPin<Vector3>
	{
		public Vector3OutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector3 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector3D*)ptr)[index % FSliceCount].ToSlimDXVector();
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
	
	public class Vector4OutputPin : ValueOutputPin<Vector4>
	{
		public Vector4OutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector4 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector4D*)ptr)[index % FSliceCount].ToSlimDXVector();
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
