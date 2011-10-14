using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using SlimDX;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	[ComVisible(false)]
	public class InputPinRegistry : PinTypeRegistry<InputAttribute>
	{
		public InputPinRegistry()
		{
			//Register default types
			this.RegisterType(typeof(double), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new DoubleInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<double>(host, valueFastIn, stream);
			                  });
			
			this.RegisterType(typeof(float), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new FloatInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<float>(host, valueFastIn, stream);
			                  });
			
			this.RegisterType(typeof(int), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new IntInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<int>(host, valueFastIn, stream);
			                  });
			
			this.RegisterType(typeof(bool), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new BoolInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<bool>(host, valueFastIn, stream);
			                  });

			this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => {
			                  	var transformIn = host.CreateTransformInput(attribute, t);
			                  	var stream = new Matrix4x4InStream(GetMatrixPointerFunc(transformIn), GetValidateAction(transformIn));
			                  	return new InputPin<Matrix4x4>(host, transformIn, stream);
			                  });
			
			this.RegisterType(typeof(Matrix), (host, attribute, t) => {
			                  	var transformIn = host.CreateTransformInput(attribute, t);
			                  	var stream = new MatrixInStream(GetMatrixPointerFunc(transformIn), GetValidateAction(transformIn));
			                  	return new InputPin<Matrix>(host, transformIn, stream);
			                  });

			this.RegisterType(typeof(Vector2D), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new Vector2DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<Vector2D>(host, valueFastIn, stream);
			                  });
			this.RegisterType(typeof(Vector3D),(host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new Vector3DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<Vector3D>(host, valueFastIn, stream);
			                  });
			this.RegisterType(typeof(Vector4D),(host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new Vector4DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<Vector4D>(host, valueFastIn, stream);
			                  });

			this.RegisterType(typeof(Vector2), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new Vector2InStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<Vector2>(host, valueFastIn, stream);
			                  });
			this.RegisterType(typeof(Vector3), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new Vector3InStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<Vector3>(host, valueFastIn, stream);
			                  });
			this.RegisterType(typeof(Vector4), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	var stream = new Vector4InStream(GetFastValuePointerFunc(valueFastIn), GetValidateAction(valueFastIn));
			                  	return new InputPin<Vector4>(host, valueFastIn, stream);
			                  });

			this.RegisterType(typeof(string), (host, attribute, t) => {
			                  	var stringIn = host.CreateStringInput(attribute, t);
			                  	var stream = new StringInStream(stringIn);
			                  	return new InputPin<string>(host, stringIn, stream);
			                  });
			
			this.RegisterType(typeof(RGBAColor), (host, attribute, t) => {
			                  	var colorIn = host.CreateColorInput(attribute, t);
			                  	var stream = new ColorInStream(GetColorPointerFunc(colorIn), GetValidateAction(colorIn));
			                  	return new InputPin<RGBAColor>(host, colorIn, stream);
			                  });

			this.RegisterType(typeof(EnumEntry), (host, attribute, t) => {
			                  	var enumIn = host.CreateEnumInput(attribute, t);
			                  	var stream = new DynamicEnumInStream(enumIn);
			                  	return new InputPin<EnumEntry>(host, enumIn, stream);
			                  });
		}
		
		unsafe private Func<Tuple<IntPtr, int>> GetFastValuePointerFunc(IValueFastIn valueFastIn)
		{
			return () => {
				int length;
				double* ptr;
				valueFastIn.GetValuePointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		unsafe private Func<Tuple<IntPtr, int>> GetMatrixPointerFunc(ITransformIn transformIn)
		{
			return () => {
				int length;
				float* ptr;
				transformIn.GetMatrixPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorIn colorIn)
		{
			return () => {
				int length;
				double* ptr;
				colorIn.GetColorPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		private Action GetValidateAction(IValueFastIn valueFastIn)
		{
			// TODO: check this
			return () => { };
		}
		
		private Action GetValidateAction(IColorIn colorIn)
		{
			// TODO: check this
			return () => { };
		}
		
		private Action GetValidateAction(ITransformIn transformIn)
		{
			// TODO: check this
			return () => { };
		}
	}
}
