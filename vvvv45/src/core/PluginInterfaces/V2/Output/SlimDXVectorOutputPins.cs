using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Output
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
				throw new NotSupportedException();
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector2D*)ptr)[index] = value.ToVector2D();
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
				throw new NotSupportedException();
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector3D*)ptr)[index] = value.ToVector3D();
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
				throw new NotSupportedException();
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((Vector4D*)ptr)[index] = value.ToVector4D();
				}
			}
		}
	}

}
