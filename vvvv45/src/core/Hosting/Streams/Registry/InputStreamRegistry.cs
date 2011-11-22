using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams.Registry
{
	public class InputStreamRegistry : StreamRegistry<InputAttribute>
	{
		public InputStreamRegistry()
		{
			//Register default types
			this.RegisterType(typeof(double), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new DoubleInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			
			this.RegisterType(typeof(float), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new FloatInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			
			this.RegisterType(typeof(int), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new IntInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			
			this.RegisterType(typeof(bool), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new BoolInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });

			this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => {
			                  	var transformIn = host.CreateTransformInput(attribute, t);
			                  	return new Matrix4x4InStream(GetMatrixPointerFunc(transformIn), GetValidateFunc(transformIn));
			                  });
			
			this.RegisterType(typeof(Matrix), (host, attribute, t) => {
			                  	var transformIn = host.CreateTransformInput(attribute, t);
			                  	return new MatrixInStream(GetMatrixPointerFunc(transformIn), GetValidateFunc(transformIn));
			                  });

			this.RegisterType(typeof(Vector2D), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new Vector2DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			this.RegisterType(typeof(Vector3D),(host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new Vector3DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			this.RegisterType(typeof(Vector4D),(host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new Vector4DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });

			this.RegisterType(typeof(Vector2), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new Vector2InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			this.RegisterType(typeof(Vector3), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new Vector3InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });
			this.RegisterType(typeof(Vector4), (host, attribute, t) => {
			                  	var valueFastIn = host.CreateValueFastInput(attribute, t);
			                  	return new Vector4InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			                  });

			this.RegisterType(typeof(string), (host, attribute, t) => {
			                  	var stringIn = host.CreateStringInput(attribute, t);
			                  	return new StringInStream(stringIn);
			                  });
			
			this.RegisterType(typeof(RGBAColor), (host, attribute, t) => {
			                  	var colorIn = host.CreateColorInput(attribute, t);
			                  	return new ColorInStream(GetColorPointerFunc(colorIn), GetValidateFunc(colorIn));
			                  });

			this.RegisterType(typeof(EnumEntry), (host, attribute, t) => {
			                  	var enumIn = host.CreateEnumInput(attribute, t);
			                  	return new DynamicEnumInStream(enumIn);
			                  });
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetFastValuePointerFunc(IValueFastIn valueFastIn)
		{
			return () => {
				int length;
				double* ptr;
				valueFastIn.GetValuePointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetMatrixPointerFunc(ITransformIn transformIn)
		{
			return () => {
				int length;
				float* ptr;
				transformIn.GetMatrixPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorIn colorIn)
		{
			return () => {
				int length;
				double* ptr;
				colorIn.GetColorPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static private Func<bool> GetValidateFunc(IValueFastIn valueFastIn)
		{
			// TODO: check this
			return () => { return true; };
		}
		
		static private Func<bool> GetValidateFunc(IColorIn colorIn)
		{
			// TODO: check this
			return () => { return colorIn.PinIsChanged; };
		}
		
		static private Func<bool> GetValidateFunc(ITransformIn transformIn)
		{
			// TODO: check this
			return () => { return transformIn.PinIsChanged; };
		}
	}
}
