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
	public class ObservableInputWrapperPin<T> : ObservableWrapperPin<T>
	{
		public ObservableInputWrapperPin(IPluginHost host, InputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating input pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				ObservablePin = new ObservableDoubleInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(float))
				ObservablePin = new ObservableFloatInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(int))
				ObservablePin = new ObservableIntInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(bool))
				ObservablePin = new ObservableBoolInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(string))
				ObservablePin = new StringInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(RGBAColor))
				ObservablePin = new ColorInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Matrix4x4))
				ObservablePin = new Matrix4x4InputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Matrix))
				ObservablePin = new SlimDXMatrixInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector2D))
				ObservablePin = new Vector2DInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector3D))
				ObservablePin = new Vector3DInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector4D))
				ObservablePin = new Vector4DInputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector2))
				ObservablePin = new Vector2InputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector3))
				ObservablePin = new Vector3InputPin(host, attribute) as ObservablePin<T>;
			else if (type == typeof(Vector4))
				ObservablePin = new Vector4InputPin(host, attribute) as ObservablePin<T>;
			else if (type.BaseType == typeof(Enum))
				ObservablePin = new EnumInputPin<T>(host, attribute) as ObservablePin<T>;
			else if (type == typeof(EnumEntry))
				ObservablePin = new DynamicEnumInputPin(host, attribute) as ObservablePin<T>;
			else
				ObservablePin = new GenericInputPin<T>(host, attribute) as ObservablePin<T>;
		}
	}
}
