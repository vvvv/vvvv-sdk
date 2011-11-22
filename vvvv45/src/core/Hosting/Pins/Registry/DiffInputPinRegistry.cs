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
    public class DiffInputPinRegistry : PinTypeRegistry<InputAttribute>
    {
        public DiffInputPinRegistry()
        {
        	this.RegisterType(typeof(double), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new DoubleInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			
			this.RegisterType(typeof(float), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new FloatInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			
			this.RegisterType(typeof(int), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new IntInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			
			this.RegisterType(typeof(bool), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new BoolInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
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
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new Vector2DInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			this.RegisterType(typeof(Vector3D),(host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new Vector3DInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			this.RegisterType(typeof(Vector4D),(host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new Vector4DInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });

			this.RegisterType(typeof(Vector2), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new Vector2InStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			this.RegisterType(typeof(Vector3), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new Vector3InStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			                  });
			this.RegisterType(typeof(Vector4), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	return new Vector4InStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
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
        	
        	
            //Register default types
//            this.RegisterType(typeof(double), (host, attribute, t) => new DiffDoubleInputPin(host, attribute));
//            this.RegisterType(typeof(float), (host, attribute, t) => new DiffFloatInputPin(host, attribute));
//            this.RegisterType(typeof(int), (host, attribute, t) => new DiffIntInputPin(host, attribute));
//            this.RegisterType(typeof(bool), (host, attribute, t) => new DiffBoolInputPin(host, attribute));
//
//            this.RegisterType(typeof(Vector2D), (host, attribute, t) => new DiffVector2DInputPin(host, attribute));
//            this.RegisterType(typeof(Vector3D), (host, attribute, t) => new DiffVector3DInputPin(host, attribute));
//            this.RegisterType(typeof(Vector4D), (host, attribute, t) => new DiffVector4DInputPin(host, attribute));
//
//            this.RegisterType(typeof(Vector2), (host, attribute, t) => new DiffVector2InputPin(host, attribute));
//            this.RegisterType(typeof(Vector3), (host, attribute, t) => new DiffVector3InputPin(host, attribute));
//            this.RegisterType(typeof(Vector4), (host, attribute, t) => new DiffVector4InputPin(host, attribute));

        }
        
        unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueIn valueIn)
		{
			return () => {
				int length;
				double* ptr;
				valueIn.GetValuePointer(out length, out ptr);
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
		
		private Func<bool> GetValidateFunc(IValueIn valueIn)
		{
			// TODO: check this
			return () => { return valueIn.PinIsChanged; };
		}
		
		private Func<bool> GetValidateFunc(IColorIn colorIn)
		{
			// TODO: check this
			return () => { return colorIn.PinIsChanged; };
		}
		
		private Func<bool> GetValidateFunc(ITransformIn transformIn)
		{
			// TODO: check this
			return () => { return transformIn.PinIsChanged; };
		}
    }
}
