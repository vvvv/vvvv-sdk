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
	public class ConfigWrapperPin<T> : ObservableWrapperPin<T>
	{
		public ConfigWrapperPin(IPluginHost host, ConfigAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				ObservablePin = new DoubleConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(float))
				ObservablePin = new FloatConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(int))
				ObservablePin = new IntConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(bool))
				ObservablePin = new BoolConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(string))
				ObservablePin = new StringConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(RGBAColor))
				ObservablePin = new ColorConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector2D))
				ObservablePin = new Vector2DConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector3D))
				ObservablePin = new Vector3DConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector4D))
				ObservablePin = new Vector4DConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector2))
				ObservablePin = new Vector2ConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector3))
				ObservablePin = new Vector3ConfigPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector4))
				ObservablePin = new Vector4ConfigPin(host, attribute) as ObservablePin<T>;
			else if (type.BaseType == typeof(Enum))
				ObservablePin = new EnumConfigPin<T>(host, attribute) as ObservablePin<T>;
			else if (type == typeof(EnumEntry))
				ObservablePin = new DynamicEnumConfigPin(host, attribute) as ObservablePin<T>;
			else
				throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
		}
	}
}
