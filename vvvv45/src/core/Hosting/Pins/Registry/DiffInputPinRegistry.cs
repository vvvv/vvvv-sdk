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
			                  	var stream = new DoubleInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<double>(host, valueIn, stream);
			                  });
			
			this.RegisterType(typeof(float), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new FloatInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<float>(host, valueIn, stream);
			                  });
			
			this.RegisterType(typeof(int), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new IntInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<int>(host, valueIn, stream);
			                  });
			
			this.RegisterType(typeof(bool), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new BoolInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<bool>(host, valueIn, stream);
			                  });

			this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => {
			                  	var transformIn = host.CreateTransformInput(attribute, t);
			                  	var stream = new Matrix4x4InStream(GetMatrixPointerFunc(transformIn), GetValidateAction(transformIn));
			                  	return new DiffInputPin<Matrix4x4>(host, transformIn, stream);
			                  });
			
			this.RegisterType(typeof(Matrix), (host, attribute, t) => {
			                  	var transformIn = host.CreateTransformInput(attribute, t);
			                  	var stream = new MatrixInStream(GetMatrixPointerFunc(transformIn), GetValidateAction(transformIn));
			                  	return new DiffInputPin<Matrix>(host, transformIn, stream);
			                  });

			this.RegisterType(typeof(Vector2D), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new Vector2DInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<Vector2D>(host, valueIn, stream);
			                  });
			this.RegisterType(typeof(Vector3D),(host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new Vector3DInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<Vector3D>(host, valueIn, stream);
			                  });
			this.RegisterType(typeof(Vector4D),(host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new Vector4DInStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<Vector4D>(host, valueIn, stream);
			                  });

			this.RegisterType(typeof(Vector2), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new Vector2InStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<Vector2>(host, valueIn, stream);
			                  });
			this.RegisterType(typeof(Vector3), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new Vector3InStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<Vector3>(host, valueIn, stream);
			                  });
			this.RegisterType(typeof(Vector4), (host, attribute, t) => {
			                  	var valueIn = host.CreateValueInput(attribute, t);
			                  	var stream = new Vector4InStream(GetValuePointerFunc(valueIn), GetValidateAction(valueIn));
			                  	return new DiffInputPin<Vector4>(host, valueIn, stream);
			                  });

			this.RegisterType(typeof(string), (host, attribute, t) => {
			                  	var stringIn = host.CreateStringInput(attribute, t);
			                  	var stream = new StringInStream(stringIn);
			                  	return new DiffInputPin<string>(host, stringIn, stream);
			                  });
			
			this.RegisterType(typeof(RGBAColor), (host, attribute, t) => {
			                  	var colorIn = host.CreateColorInput(attribute, t);
			                  	var stream = new ColorInStream(GetColorPointerFunc(colorIn), GetValidateAction(colorIn));
			                  	return new DiffInputPin<RGBAColor>(host, colorIn, stream);
			                  });

			this.RegisterType(typeof(EnumEntry), (host, attribute, t) => {
			                  	var enumIn = host.CreateEnumInput(attribute, t);
			                  	var stream = new DynamicEnumInStream(enumIn);
			                  	return new DiffInputPin<EnumEntry>(host, enumIn, stream);
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
		
		private Action GetValidateAction(IValueIn valueIn)
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
