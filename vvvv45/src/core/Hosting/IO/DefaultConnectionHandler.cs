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

            var sourceDataType = GetDataType(source.GetType());
            var sinkDataType = GetDataType(sink.GetType());
			
			return sinkDataType.IsAssignableFrom(sourceDataType);
		}
		
		public string GetFriendlyNameForSink(object sink)
		{
            var sinkDataType = GetDataType(sink.GetType());
			return string.Format(" [ Needs: {0} ]", sinkDataType.GetCSharpName());
		}
		
		public string GetFriendlyNameForSource(object source)
		{
            var sourceDataType = GetDataType(source.GetType());
			return string.Format(" [ Supports: {0} ]", sourceDataType.GetCSharpName());
		}

        private static Type GetDataType(Type ioType)
        {
            if (ioType.IsGenericType)
                return ioType.GetGenericArguments()[0];
            return GetDataType(ioType.BaseType);
        }
	}
}
