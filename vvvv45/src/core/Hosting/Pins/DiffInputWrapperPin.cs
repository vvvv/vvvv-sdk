using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using SlimDX;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	public class DiffInputWrapperPin<T> : DiffWrapperPin<T>
	{
		public DiffInputWrapperPin(IPluginHost host, InputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating input pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				DiffPin = new DiffDoubleInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(float))
				DiffPin = new DiffFloatInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(int))
				DiffPin = new DiffIntInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(bool))
				DiffPin = new DiffBoolInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(string))
				DiffPin = new StringInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(RGBAColor))
				DiffPin = new ColorInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Matrix4x4))
				DiffPin = new Matrix4x4InputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Matrix))
				DiffPin = new SlimDXMatrixInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2D))
				DiffPin = new DiffVector2DInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3D))
				DiffPin = new DiffVector3DInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4D))
				DiffPin = new DiffVector4DInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2))
				DiffPin = new DiffVector2InputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3))
				DiffPin = new DiffVector3InputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4))
				DiffPin = new DiffVector4InputPin(host, attribute) as DiffPin<T>;
			else if (type.BaseType == typeof(Enum))
				DiffPin = new EnumInputPin<T>(host, attribute) as DiffPin<T>;
			else if (type == typeof(EnumEntry))
				DiffPin = new DynamicEnumInputPin(host, attribute) as DiffPin<T>;
			else
				DiffPin = new GenericInputPin<T>(host, attribute) as DiffPin<T>;
		}
	}
}
