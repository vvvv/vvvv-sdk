using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Config
{
	public class Vector2DConfigPin : ValueConfigPin<Vector2D>
	{
		public Vector2DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override Vector2D this[int index] 
		{
			get
			{
				double value1, value2;
				FValueConfig.GetValue2D(index, out value1, out value2);
				return new Vector2D(value1, value2);
			}
			set
			{
				FValueConfig.SetValue2D(index, value.x, value.y);
			}
		}
	}
	
	public class Vector3DConfigPin : ValueConfigPin<Vector3D>
	{
		public Vector3DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override Vector3D this[int index] 
		{
			get
			{
				double value1, value2, value3;
				FValueConfig.GetValue3D(index, out value1, out value2, out value3);
				return new Vector3D(value1, value2, value3);
			}
			set
			{
				FValueConfig.SetValue3D(index, value.x, value.y, value.z);
			}
		}
	}
		
	public class Vector4DConfigPin : ValueConfigPin<Vector4D>
	{
		public Vector4DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override Vector4D this[int index] 
		{
			get
			{
				double value1, value2, value3, value4;
				FValueConfig.GetValue4D(index, out value1, out value2, out value3, out value4);
				return new Vector4D(value1, value2, value3, value4);
			}
			set
			{
				FValueConfig.SetValue4D(index, value.x, value.y, value.z, value.w);
			}
		}
	}
}
