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
	public class InputWrapperPin<T> : WrapperPin<T>
	{
		public InputWrapperPin(IPluginHost host, InputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating input pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				FPin = new DoubleInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(float))
				FPin = new FloatInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(int))
				FPin = new IntInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(bool))
				FPin = new BoolInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(string))
				FPin = new StringInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(RGBAColor))
				FPin = new ColorInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Matrix4x4))
				FPin = new Matrix4x4InputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Matrix))
				FPin = new SlimDXMatrixInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector2D))
				FPin = new Vector2DInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector3D))
				FPin = new Vector3DInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector4D))
				FPin = new Vector4DInputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector2))
				FPin = new Vector2InputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector3))
				FPin = new Vector3InputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector4))
				FPin = new Vector4InputPin(host, attribute) as Pin<T>;
			else if (type.BaseType == typeof(Enum))
				FPin = new EnumInputPin<T>(host, attribute) as Pin<T>;
			else if (type == typeof(EnumEntry))
				FPin = new DynamicEnumInputPin(host, attribute) as Pin<T>;
			else
				FPin = new GenericInputPin<T>(host, attribute) as Pin<T>;
		}
	}
}
