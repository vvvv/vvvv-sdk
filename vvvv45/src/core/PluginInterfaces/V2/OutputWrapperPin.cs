using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Output;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public class OutputWrapperPin<T> : WrapperPin<T>
	{
		public OutputWrapperPin(IPluginHost host, OutputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating output pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				FPin = new DoubleOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(float))
				FPin = new FloatOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(int))
				FPin = new IntOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(bool))
				FPin = new BoolOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(string))
				FPin = new StringOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(RGBAColor))
				FPin = new ColorOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Matrix4x4))
				FPin = new Matrix4x4OutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Matrix))
				FPin = new SlimDXMatrixOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector2D))
				FPin = new Vector2DOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector3D))
				FPin = new Vector3DOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector4D))
				FPin = new Vector4DOutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector2))
				FPin = new Vector2OutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector3))
				FPin = new Vector3OutputPin(host, attribute) as Pin<T>;
			else if (type == typeof(Vector4))
				FPin = new Vector4OutputPin(host, attribute) as Pin<T>;
			else if (type.BaseType == typeof(Enum))
				FPin = new EnumOutputPin<T>(host, attribute) as Pin<T>;
			else if (type == typeof(EnumEntry))
				FPin = new DynamicEnumOutputPin(host, attribute) as Pin<T>;
			else
				FPin = new GenericOutputPin<T>(host, attribute) as Pin<T>;
		}
	}
}
