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
	public class InputWrapperPin<T> : WrapperPin<T>
	{
		public InputWrapperPin(IPluginHost host, InputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating input pin '{0}'.", attribute.Name));
			
			var type = typeof(T);
			
			if (type == typeof(double))
				FSpread = new DoubleInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(float))
				FSpread = new FloatInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(int))
				FSpread = new IntInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(bool))
				FSpread = new BoolInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(string))
				FSpread = new StringInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(RGBAColor))
				FSpread = new ColorInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Matrix4x4))
				FSpread = new Matrix4x4InputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Matrix))
				FSpread = new SlimDXMatrixInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector2D))
				FSpread = new Vector2DInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector3D))
				FSpread = new Vector3DInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector4D))
				FSpread = new Vector4DInputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector2))
				FSpread = new Vector2InputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector3))
				FSpread = new Vector3InputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector4))
				FSpread = new Vector4InputPin(host, attribute) as ISpread<T>;
			else if (type.BaseType == typeof(Enum))
				FSpread = new EnumInputPin<T>(host, attribute) as ISpread<T>;
			else
				throw new NotImplementedException(string.Format("InputPin of type '{0}' not supported.", type));
		}
	}
}
