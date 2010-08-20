using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Config;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public class ConfigWrapperPin<T> : DiffWrapperPin<T>
	{
		public ConfigWrapperPin(IPluginHost host, ConfigAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				FPin = new DoubleConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(float))
				FPin = new FloatConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(int))
				FPin = new IntConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(bool))
				FPin = new BoolConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(string))
				FPin = new StringConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(RGBAColor))
				FPin = new ColorConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2D))
				FPin = new Vector2DConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3D))
				FPin = new Vector3DConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4D))
				FPin = new Vector4DConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector2))
				FPin = new Vector2ConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector3))
				FPin = new Vector3ConfigPin(host, attribute) as DiffPin<T>;
			else if (type == typeof(Vector4))
				FPin = new Vector4ConfigPin(host, attribute) as DiffPin<T>;
			else if (type.BaseType == typeof(Enum))
				FPin = new EnumConfigPin<T>(host, attribute) as DiffPin<T>;
			else if (type == typeof(EnumEntry))
				FPin = new DynamicEnumConfigPin(host, attribute) as DiffPin<T>;
			else
				throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
		}
	}
}
