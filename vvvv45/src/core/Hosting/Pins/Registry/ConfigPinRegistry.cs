//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//
//using SlimDX;
//using VVVV.Hosting.Pins.Config;
//using VVVV.Hosting.Streams;
//using VVVV.PluginInterfaces.V1;
//using VVVV.PluginInterfaces.V2;
//using VVVV.Utils.VColor;
//using VVVV.Utils.VMath;
//
//namespace VVVV.Hosting.Pins
//{
//	[ComVisible(false)]
//	public class ConfigPinRegistry : PinTypeRegistry<ConfigAttribute>
//	{
//		public ConfigPinRegistry()
//		{
//			//Register default types
//			this.RegisterType(typeof(double), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new DoubleInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new DoubleOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<double>(inStream, outStream);
//			                  });
//			
//			this.RegisterType(typeof(float), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new FloatInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new FloatOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<float>(inStream, outStream);
//			                  });
//			
//			this.RegisterType(typeof(int), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new IntInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new IntOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<int>(inStream, outStream);
//			                  });
//			
//			this.RegisterType(typeof(bool), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new BoolInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new BoolOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<bool>(inStream, outStream);
//			                  });
//
//			this.RegisterType(typeof(Vector2D), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new Vector2DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new Vector2DOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<Vector2D>(inStream, outStream);
//			                  });
//			this.RegisterType(typeof(Vector3D),(host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new Vector3DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new Vector3DOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<Vector3D>(inStream, outStream);
//			                  });
//			this.RegisterType(typeof(Vector4D),(host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new Vector4DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new Vector4DOutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<Vector4D>(inStream, outStream);
//			                  });
//
//			this.RegisterType(typeof(Vector2), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new Vector2InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new Vector2OutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<Vector2>(inStream, outStream);
//			                  });
//			this.RegisterType(typeof(Vector3), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new Vector3InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new Vector3OutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<Vector3>(inStream, outStream);
//			                  });
//			this.RegisterType(typeof(Vector4), (host, attribute, t) => {
//			                  	var valueConfig = host.CreateValueConfig(attribute, t);
//			                  	var inStream = new Vector4InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
//			                  	var outStream = new Vector4OutStream(ResizeValueArrayFunc(valueConfig));
//			                  	return new ConfigIOStream<Vector4>(inStream, outStream);
//			                  });
//
//			this.RegisterType(typeof(string), (host, attribute, t) => {
//			                  	var stringConfig = host.CreateStringConfig(attribute, t);
//			                  	return new StringConfigStream(stringConfig);
//			                  });
//			
//			this.RegisterType(typeof(RGBAColor), (host, attribute, t) => {
//			                  	var colorConfig = host.CreateColorConfig(attribute, t);
//			                  	var inStream = new ColorInStream(GetColorPointerFunc(colorConfig), GetValidateFunc(colorConfig));
//			                  	var outStream = new ColorOutStream(ResizeColorArrayFunc(colorConfig));
//			                  	return new ConfigIOStream<RGBAColor>(inStream, outStream);
//			                  });
//
//			this.RegisterType(typeof(EnumEntry), (host, attribute, t) => {
//			                  	var enumConfig = host.CreateEnumConfig(attribute, t);
//			                  	return new DynamicEnumConfigStream(enumConfig);
//			                  });
//		}
//		
//		unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueConfig valueConfig)
//		{
//			return () => {
//				int length;
//				double* ptr;
//				valueConfig.GetValuePointer(out length, out ptr);
//				return Tuple.Create(new IntPtr(ptr), length);
//			};
//		}
//		
//		private Func<bool> GetValidateFunc(IValueConfig valueConfig)
//		{
//			// TODO: check this
//			return () => { return valueConfig.PinIsChanged; };
//		}
//		
//		private Func<bool> GetValidateFunc(IColorConfig colorConfig)
//		{
//			// TODO: check this
//			return () => { return colorConfig.PinIsChanged; };
//		}
//		
//		unsafe private Func<int, IntPtr> ResizeValueArrayFunc(IValueConfig valueConfig)
//		{
//			return (int newLength) => {
//				int length;
//				double* ptr;
//				valueConfig.SliceCount = newLength;
//				valueConfig.GetValuePointer(out length, out ptr);
//				return new IntPtr(ptr);
//			};
//		}
//		
//		unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorConfig colorConfig)
//		{
//			return () => {
//				int length;
//				double* ptr;
//				colorConfig.GetColorPointer(out length, out ptr);
//				return Tuple.Create(new IntPtr(ptr), length);
//			};
//		}
//		
//		unsafe private Func<int, IntPtr> ResizeColorArrayFunc(IColorConfig colorConfig)
//		{
//			return (int newLength) => {
//				int length;
//				double* ptr;
//				colorConfig.SliceCount = newLength;
//				colorConfig.GetColorPointer(out length, out ptr);
//				return new IntPtr(ptr);
//			};
//		}
//	}
//}
