using System;
using System.Runtime.InteropServices;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.Core.Logging;

namespace VVVV.Hosting
{
	/// <summary>
	/// Logs messages to the IVVVVHost.
	/// </summary>
	[ComVisible(false)]
	class VVVVLogger : ILogger
	{
		private readonly IVVVVHost FHost;
		
		public VVVVLogger(IVVVVHost host)
		{
			FHost = host;
		}
		
		public void Log(LogType logType, string message)
		{
			FHost.Log((TLogType) logType, message);
		}
	}
}
