using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Hosting.Pins.Output;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
    [ComVisible(false)]
    public class OutputPinRegistry : PinTypeRegistry<OutputAttribute>
    {
    	public OutputPinRegistry()
    	{
			//Register default types
			this.RegisterType(typeof(double), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new DoubleOutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			
			this.RegisterType(typeof(float), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new FloatOutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			
			this.RegisterType(typeof(int), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new IntOutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			
			this.RegisterType(typeof(bool), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new BoolOutStream(GetResizeValueArrayFunc(valueOut));
			                  });

			this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => {
			                  	var transformOut = host.CreateTransformOutput(attribute, t);
			                  	return new Matrix4x4OutStream(GetResizeMatrixArrayFunc(transformOut));
			                  });
			
			this.RegisterType(typeof(Matrix), (host, attribute, t) => {
			                  	var transformOut = host.CreateTransformOutput(attribute, t);
			                  	return new MatrixOutStream(GetResizeMatrixArrayFunc(transformOut));
			                  });

			this.RegisterType(typeof(Vector2D), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new Vector2DOutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			this.RegisterType(typeof(Vector3D),(host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new Vector3DOutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			this.RegisterType(typeof(Vector4D),(host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new Vector4DOutStream(GetResizeValueArrayFunc(valueOut));
			                  });

			this.RegisterType(typeof(Vector2), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new Vector2OutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			this.RegisterType(typeof(Vector3), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new Vector3OutStream(GetResizeValueArrayFunc(valueOut));
			                  });
			this.RegisterType(typeof(Vector4), (host, attribute, t) => {
			                  	var valueOut = host.CreateValueOutput(attribute, t);
			                  	return new Vector4OutStream(GetResizeValueArrayFunc(valueOut));
			                  });

			this.RegisterType(typeof(string), (host, attribute, t) => {
			                  	var stringOut = host.CreateStringOutput(attribute, t);
			                  	return new StringOutStream(stringOut);
			                  });
			
			this.RegisterType(typeof(RGBAColor), (host, attribute, t) => {
			                  	var colorOut = host.CreateColorOutput(attribute, t);
			                  	return new ColorOutStream(GetResizeColorArrayFunc(colorOut));
			                  });

			this.RegisterType(typeof(EnumEntry), (host, attribute, t) => {
			                  	var enumOut = host.CreateEnumOutput(attribute, t);
			                  	return new DynamicEnumOutStream(enumOut);
			                  });
		}
		
		unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueOut valueFastOut)
		{
			return () => {
				double* ptr;
				valueFastOut.GetValuePointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), valueFastOut.SliceCount);
			};
		}
		
		unsafe private Func<Tuple<IntPtr, int>> GetMatrixPointerFunc(ITransformOut transformOut)
		{
			return () => {
				float* ptr;
				transformOut.GetMatrixPointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), transformOut.SliceCount);
			};
		}
		
		unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorOut colorOut)
		{
			return () => {
				double* ptr;
				colorOut.GetColorPointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), colorOut.SliceCount);
			};
		}
		
		unsafe private Func<int, IntPtr> GetResizeValueArrayFunc(IValueOut valueOut)
		{
			return (newLength) =>
			{
				double* ptr;
				valueOut.SliceCount = newLength;
				valueOut.GetValuePointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		unsafe private Func<int, IntPtr> GetResizeColorArrayFunc(IColorOut colorOut)
		{
			return (newLength) =>
			{
				double* ptr;
				colorOut.SliceCount = newLength;
				colorOut.GetColorPointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		unsafe private Func<int, IntPtr> GetResizeMatrixArrayFunc(ITransformOut transformOut)
		{
			return (newLength) =>
			{
				float* ptr;
				transformOut.SliceCount = newLength;
				transformOut.GetMatrixPointer(out ptr);
				return new IntPtr(ptr);
			};
		}
    	
//        public OutputPinRegistry()
//        {
//            //Register default types
//            this.RegisterType(typeof(double), (host, attribute, t) => new DoubleOutputPin(host, attribute));
//            this.RegisterType(typeof(float), (host, attribute, t) => new FloatOutputPin(host, attribute));
//            this.RegisterType(typeof(int), (host, attribute, t) => new IntOutputPin(host, attribute));
//            this.RegisterType(typeof(bool), (host, attribute, t) => new BoolOutputPin(host, attribute));
//
//            this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => new Matrix4x4OutputPin(host, attribute));
//            this.RegisterType(typeof(Matrix), (host, attribute, t) => new SlimDXMatrixOutputPin(host, attribute));
//
//            this.RegisterType(typeof(Vector2D), (host, attribute, t) => new Vector2DOutputPin(host, attribute));
//            this.RegisterType(typeof(Vector3D), (host, attribute, t) => new Vector3DOutputPin(host, attribute));
//            this.RegisterType(typeof(Vector4D), (host, attribute, t) => new Vector4DOutputPin(host, attribute));
//
//            this.RegisterType(typeof(Vector2), (host, attribute, t) => new Vector2OutputPin(host, attribute));
//            this.RegisterType(typeof(Vector3), (host, attribute, t) => new Vector3OutputPin(host, attribute));
//            this.RegisterType(typeof(Vector4), (host, attribute, t) => new Vector4OutputPin(host, attribute));
//
//            this.RegisterType(typeof(string), (host, attribute, t) => new StringOutputPin(host, attribute));
//            this.RegisterType(typeof(RGBAColor), (host, attribute, t) => new ColorOutputPin(host, attribute));
//
//            this.RegisterType(typeof(EnumEntry), (host, attribute, t) => new DynamicEnumOutputPin(host, attribute));
//            
//            this.RegisterType(typeof(DXResource<,>), 
//                              (host, attribute, t) =>
//                              {
//                                  var genericArguments = t.GetGenericArguments();
//                                  var resourceType = genericArguments[0];
//                                  var metadataType = genericArguments[1];
//                                  
//                                  if (resourceType == typeof(Texture))
//                                  {
//                                    var textureOutPinType = typeof(TextureOutputPin<,>);
//                                    textureOutPinType = textureOutPinType.MakeGenericType(t, metadataType);
//                                    return Activator.CreateInstance(textureOutPinType, host, attribute);
//                                  }
//                                  else
//                                  {
//                                      throw new NotImplementedException();
//                                  }
//                              });
//        }
    }
}
