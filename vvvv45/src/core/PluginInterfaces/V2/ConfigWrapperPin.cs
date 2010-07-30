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
	public class ConfigWrapperPin<T> : WrapperPin<T>
	{
		public ConfigWrapperPin(IPluginHost host, ConfigAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				FSpread = new DoubleConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(float))
				FSpread = new FloatConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(int))
				FSpread = new IntConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(bool))
				FSpread = new BoolConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(string))
				FSpread = new StringConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(RGBAColor))
				FSpread = new ColorConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector2D))
				FSpread = new Vector2DConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector3D))
				FSpread = new Vector3DConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector4D))
				FSpread = new Vector4DConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector2))
				FSpread = new Vector2ConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector3))
				FSpread = new Vector3ConfigPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector4))
				FSpread = new Vector4ConfigPin(host, attribute) as ISpread<T>;
			else
				throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
		}
	}
}
