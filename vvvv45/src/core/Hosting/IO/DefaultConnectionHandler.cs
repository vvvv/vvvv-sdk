using System;
using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Reflection;

namespace VVVV.Hosting.IO
{
	class DefaultConnectionHandler : IConnectionHandler
	{
        public DefaultConnectionHandler(Type sourceDataType, Type sinkDataType)
        {
            FSourceDataType = sourceDataType;
            FSinkDataType = sinkDataType;
        }

        Type FSourceDataType;
        Type FSinkDataType;

        Type GetSourceDataType(object sourceData) => FSourceDataType ?? GetDataType(GetType(sourceData));
        Type GetSinkDataType(object sinkData) => FSinkDataType ?? GetDataType(GetType(sinkData));

        public bool Accepts(object source, object sink)
		{
            if (source == null) return true;
            if (sink == null) return true;

			return GetSinkDataType(sink).IsAssignableFrom(GetSourceDataType(source));
		}
		
		public string GetFriendlyNameForSink(object sink)
		{
			return string.Format(" [{0}]", GetSinkDataType(sink).GetCSharpName());
		}
		
		public string GetFriendlyNameForSource(object source)
		{
			return string.Format(" [{0}]", GetSourceDataType(source).GetCSharpName());
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
            // this should probably be spelled out specifically.
            // it's about getting rid of ISpread<> and alikes not about any generic type.
            if (ioType.IsGenericType)
                return ioType.GetGenericArguments()[0];
            return GetDataType(ioType.BaseType);
        }
	}
}
