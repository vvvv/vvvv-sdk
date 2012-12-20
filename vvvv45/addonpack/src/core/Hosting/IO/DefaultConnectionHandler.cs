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
			var sourceDataType = source.GetType().GetGenericArguments().First();
			var sinkDataType = sink.GetType().GetGenericArguments().First();
			
			return sinkDataType.IsAssignableFrom(sourceDataType);
		}
		
		public string GetFriendlyNameForSink(object sink)
		{
			var sinkDataType = sink.GetType().GetGenericArguments().First();
			return string.Format(" [ Needs: {0} ]", sinkDataType.GetCSharpName());
		}
		
		public string GetFriendlyNameForSource(object source)
		{
			var sourceDataType = source.GetType().GetGenericArguments().First();
			return string.Format(" [ Supports: {0} ]", sourceDataType.GetCSharpName());
		}
	}
}
