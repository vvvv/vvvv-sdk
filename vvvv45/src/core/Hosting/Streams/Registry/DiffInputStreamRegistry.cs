using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Streams.Registry
{
	public class DiffInputStreamRegistry : StreamRegistry<InputAttribute>
	{
		public DiffInputStreamRegistry()
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
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueIn valueIn)
		{
			return () => {
				int length;
				double* ptr;
				valueIn.GetValuePointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static private Func<bool> GetValidateFunc(IValueIn valueIn)
		{
			// TODO: check this
			return () => { return valueIn.PinIsChanged; };
		}
	}
}
