using System;
using System.Collections;
using System.Collections.Generic;

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
			var type = typeof(T);
			
			if (type == typeof(double))
				FSpread = new DoubleOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(float))
				FSpread = new FloatOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(int))
				FSpread = new IntOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(bool))
				FSpread = new BoolOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(string))
				FSpread = new StringOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(RGBAColor))
				FSpread = new ColorOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Matrix4x4))
				FSpread = new Matrix4x4OutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Matrix))
				FSpread = new SlimDXMatrixOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector2D))
				FSpread = new Vector2DOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector3D))
				FSpread = new Vector3DOutputPin(host, attribute) as ISpread<T>;
			else if (type == typeof(Vector4D))
				FSpread = new Vector4DOutputPin(host, attribute) as ISpread<T>;
			else
				throw new NotImplementedException(string.Format("OutputPin of type '{0}' not supported.", type));
		}
	}
}
