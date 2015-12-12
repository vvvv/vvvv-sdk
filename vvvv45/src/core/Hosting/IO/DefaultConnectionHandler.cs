using System;
using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Reflection;

namespace VVVV.Hosting.IO
{
	class DefaultConnectionHandler : IConnectionHandler
	{
		public bool Accepts(object source, object sink)
		{
            if (source == null) return true;
            if (sink == null) return true;

            var sourceDataType = GetDataType(GetType(source));
            var sinkDataType = GetDataType(GetType(source));
			
			return sinkDataType.IsAssignableFrom(sourceDataType);
		}
		
		public string GetFriendlyNameForSink(object sink)
		{
            var sinkDataType = GetDataType(GetType(sink));
			return string.Format(" [{0}]", sinkDataType.GetCSharpName());
		}
		
		public string GetFriendlyNameForSource(object source)
		{
            var sourceDataType = GetDataType(GetType(source));
			return string.Format(" [{0}]", sourceDataType.GetCSharpName());
		}

        private static Type GetType(object value)
        {
            var wrapper = value as DynamicTypeWrapper;
            if (wrapper != null)
                return wrapper.Value.GetType();
            return value.GetType();
        }

        private static Type GetDataType(Type ioType)
        {
            if (ioType.IsGenericType)
                return ioType.GetGenericArguments()[0];
            return GetDataType(ioType.BaseType);
        }
	}
}
