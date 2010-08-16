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
					return ((Vector2D*)ptr)[index % FSliceCount].ToSlimDXVector();
				}
			}
			set
			{
				if (!FValueFastIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector2D*)ptr)[index % FSliceCount] = value.ToVector2D();
					}
				}
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
					return ((Vector3D*)ptr)[index % FSliceCount].ToSlimDXVector();
				}
			}
			set
			{
				if (!FValueFastIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector3D*)ptr)[index % FSliceCount] = value.ToVector3D();
					}
				}
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
					return ((Vector4D*)ptr)[index % FSliceCount].ToSlimDXVector();
				}
			}
			set
			{
				if (!FValueFastIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector4D*)ptr)[index % FSliceCount] = value.ToVector4D();
					}
				}
			}
		}
	}
	
	public class ObservableVector2InputPin : ObservableValueInputPin<Vector2>
	{
		public ObservableVector2InputPin(IPluginHost host, InputAttribute attribute)
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
				if (!FValueIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector2D*)ptr)[index % FSliceCount] = value.ToVector2D();
					}
				}
			}
		}
	}
	
	public class ObservableVector3InputPin : ObservableValueInputPin<Vector3>
	{
		public ObservableVector3InputPin(IPluginHost host, InputAttribute attribute)
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
				if (!FValueIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector3D*)ptr)[index % FSliceCount] = value.ToVector3D();
					}
				}
			}
		}
	}
	
	public class ObservableVector4InputPin : ObservableValueInputPin<Vector4>
	{
		public ObservableVector4InputPin(IPluginHost host, InputAttribute attribute)
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
				if (!FValueIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector4D*)ptr)[index % FSliceCount] = value.ToVector4D();
					}
				}
			}
		}
	}
}
