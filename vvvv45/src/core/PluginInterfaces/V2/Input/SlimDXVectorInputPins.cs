using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class Vector2InputPin : ValueInputPin<Vector2>
	{
		public Vector2InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector2 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector2D*)ptr)[index].ToSlimDXVector();
				}
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
	
	public class Vector3InputPin : ValueInputPin<Vector3>
	{
		public Vector3InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector3 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector3D*)ptr)[index].ToSlimDXVector();
				}
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
	
	public class Vector4InputPin : ValueInputPin<Vector4>
	{
		public Vector4InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector4 this[int index] 
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector4D*)ptr)[index].ToSlimDXVector();
				}
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}
