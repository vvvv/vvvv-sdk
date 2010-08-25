using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using SlimDX;
using VVVV.Hosting.Pins.Config;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	public class ConfigWrapperPin<T> : DiffWrapperPin<T>
	{
		public ConfigWrapperPin(IPluginHost host, ConfigAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				DiffPin = new DoubleConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(float))
				DiffPin = new FloatConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(int))
				DiffPin = new IntConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(bool))
				DiffPin = new BoolConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(string))
				DiffPin = new StringConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(RGBAColor))
				DiffPin = new ColorConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2D))
				DiffPin = new Vector2DConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3D))
				DiffPin = new Vector3DConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4D))
				DiffPin = new Vector4DConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2))
				DiffPin = new Vector2ConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3))
				DiffPin = new Vector3ConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4))
				DiffPin = new Vector4ConfigPin(host, attribute) as DiffPin<T>;
			else if (type.BaseType == typeof(Enum))
				DiffPin = new EnumConfigPin<T>(host, attribute) as DiffPin<T>;
			else if (type == typeof(EnumEntry))
				DiffPin = new DynamicEnumConfigPin(host, attribute) as DiffPin<T>;
			else
				throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
		}
	}
}
