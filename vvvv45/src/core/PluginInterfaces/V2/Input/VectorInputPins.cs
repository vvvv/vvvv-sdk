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
					return ((Vector2D*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				if (!FValueFastIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector2D*)ptr)[index % FSliceCount] = value;
					}
				}
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
					return ((Vector3D*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				if (!FValueFastIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector3D*)ptr)[index % FSliceCount] = value;
					}
				}
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
					return ((Vector4D*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				if (!FValueFastIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector4D*)ptr)[index % FSliceCount] = value;
					}
				}
			}
		}
	}
	
	public class DiffVector2DInputPin : DiffValueInputPin<Vector2D>
	{
		public DiffVector2DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector2D this[int index]
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector2D*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				if (!FValueIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector2D*)ptr)[index % FSliceCount] = value;
					}
				}
			}
		}
	}
	
	public class DiffVector3DInputPin : DiffValueInputPin<Vector3D>
	{
		public DiffVector3DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector3D this[int index]
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector3D*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				if (!FValueIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector3D*)ptr)[index % FSliceCount] = value;
					}
				}
			}
		}
	}
	
	public class DiffVector4DInputPin : DiffValueInputPin<Vector4D>
	{
		public DiffVector4DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		unsafe public override Vector4D this[int index]
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((Vector4D*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				if (!FValueIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((Vector4D*)ptr)[index % FSliceCount] = value;
					}
				}
			}
		}
	}
}
