using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class Vector2ConfigPin : VectorConfigPin<Vector2>
	{
		public Vector2ConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 2, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		public override Vector2 this[int index]
		{
			get
			{
				double value1, value2;
				FValueConfig.GetValue2D(index, out value1, out value2);
				return new Vector2((float) value1, (float) value2);
			}
			set
			{
				FValueConfig.SetValue2D(index, (double) value.X, (double) value.Y);
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector3ConfigPin : VectorConfigPin<Vector3>
	{
		public Vector3ConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 3, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		public override Vector3 this[int index]
		{
			get
			{
				double value1, value2, value3;
				FValueConfig.GetValue3D(index, out value1, out value2, out value3);
				return new Vector3((float) value1, (float) value2, (float) value3);
			}
			set
			{
				FValueConfig.SetValue3D(index, (double) value.X, (double) value.Y, (double) value.Z);
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector4ConfigPin : VectorConfigPin<Vector4>
	{
		public Vector4ConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 4, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		public override Vector4 this[int index]
		{
			get
			{
				double value1, value2, value3, value4;
				FValueConfig.GetValue4D(index, out value1, out value2, out value3, out value4);
				return new Vector4((float) value1, (float) value2, (float) value3, (float) value4);
			}
			set
			{
				FValueConfig.SetValue4D(index, (double) value.X, (double) value.Y, (double) value.Z, (double) value.W);
			}
		}
	}

    [ComVisible(false)]
    public class QuaternionConfigPin : VectorConfigPin<Quaternion>
    {
        public QuaternionConfigPin(IPluginHost host, ConfigAttribute attribute)
            : base(host, attribute, 4, float.MinValue, float.MaxValue, 0.01)
        {
        }

        public override Quaternion this[int index]
        {
            get
            {
                double value1, value2, value3, value4;
                FValueConfig.GetValue4D(index, out value1, out value2, out value3, out value4);
                return new Quaternion((float)value1, (float)value2, (float)value3, (float)value4);
            }
            set
            {
                FValueConfig.SetValue4D(index, (double)value.X, (double)value.Y, (double)value.Z, (double)value.W);
            }
        }
    }
}
