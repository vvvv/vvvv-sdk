using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Input;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public class DiffInputWrapperPin<T> : DiffWrapperPin<T>
	{
		public DiffInputWrapperPin(IPluginHost host, InputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating input pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				FPin = new DiffDoubleInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(float))
				FPin = new DiffFloatInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(int))
				FPin = new DiffIntInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(bool))
				FPin = new DiffBoolInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(string))
				FPin = new StringInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(RGBAColor))
				FPin = new ColorInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Matrix4x4))
				FPin = new Matrix4x4InputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Matrix))
				FPin = new SlimDXMatrixInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2D))
				FPin = new Vector2DInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3D))
				FPin = new Vector3DInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4D))
				FPin = new Vector4DInputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2))
				FPin = new Vector2InputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3))
				FPin = new Vector3InputPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4))
				FPin = new Vector4InputPin(host, attribute) as DiffPin<T>;
			else if (type.BaseType == typeof(Enum))
				FPin = new EnumInputPin<T>(host, attribute) as DiffPin<T>;
			else if (type == typeof(EnumEntry))
				FPin = new DynamicEnumInputPin(host, attribute) as DiffPin<T>;
			else
				FPin = new GenericInputPin<T>(host, attribute) as DiffPin<T>;
		}
	}
}
