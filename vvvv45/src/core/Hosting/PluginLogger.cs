
using System;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting
{
	class PluginLogger : ILogger
	{
		private readonly IPluginHost FPluginHost;
		
		public PluginLogger(IPluginHost host)
		{
			FPluginHost = host;
		}
		
		public void Log(LogType logType, string message)
		{
			FPluginHost.Log((TLogType) logType, message);
		}
	}
}
