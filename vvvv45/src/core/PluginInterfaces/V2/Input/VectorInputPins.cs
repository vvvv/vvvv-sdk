using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class Vector2DInputPin : ValueInputPin<Vector2D>
	{
		public Vector2DInputPin(IPluginHost host, InputAttribute attribute)
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
				throw new NotImplementedException();
			}
		}
	}
	
	public class Vector3DInputPin : ValueInputPin<Vector3D>
	{
		public Vector3DInputPin(IPluginHost host, InputAttribute attribute)
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
				throw new NotImplementedException();
			}
		}
	}
	
	public class Vector4DInputPin : ValueInputPin<Vector4D>
	{
		public Vector4DInputPin(IPluginHost host, InputAttribute attribute)
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
				throw new NotImplementedException();
			}
		}
	}
}
