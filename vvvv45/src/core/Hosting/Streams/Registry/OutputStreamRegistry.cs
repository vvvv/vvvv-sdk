using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams.Registry
{
	public class OutputStreamRegistry : StreamRegistry<OutputAttribute>
	{
		public OutputStreamRegistry()
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
		
		static unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueOut valueFastOut)
		{
			return () => {
				double* ptr;
				valueFastOut.GetValuePointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), valueFastOut.SliceCount);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetMatrixPointerFunc(ITransformOut transformOut)
		{
			return () => {
				float* ptr;
				transformOut.GetMatrixPointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), transformOut.SliceCount);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorOut colorOut)
		{
			return () => {
				double* ptr;
				colorOut.GetColorPointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), colorOut.SliceCount);
			};
		}
		
		static unsafe private Func<int, IntPtr> GetResizeValueArrayFunc(IValueOut valueOut)
		{
			return (newLength) =>
			{
				double* ptr;
				valueOut.SliceCount = newLength;
				valueOut.GetValuePointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		static unsafe private Func<int, IntPtr> GetResizeColorArrayFunc(IColorOut colorOut)
		{
			return (newLength) =>
			{
				double* ptr;
				colorOut.SliceCount = newLength;
				colorOut.GetColorPointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		static unsafe private Func<int, IntPtr> GetResizeMatrixArrayFunc(ITransformOut transformOut)
		{
			return (newLength) =>
			{
				float* ptr;
				transformOut.SliceCount = newLength;
				transformOut.GetMatrixPointer(out ptr);
				return new IntPtr(ptr);
			};
		}
	}
}
