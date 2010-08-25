using System;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.Core.Logging;

namespace VVVV.Hosting
{
	/// <summary>
	/// Logs messages to the IVVVVHost.
	/// </summary>
	public class VVVVLogger : ILogger
	{
		protected IVVVVHost FHost;
		
		public VVVVLogger(IVVVVHost host)
		{
			FHost = host;
		}
		
		public void Log(LogType logType, string message)
		{
			TLogType vvvvLogType;
			switch (logType)
			{
				case LogType.Debug:
					vvvvLogType = TLogType.Debug;
					break;
				case LogType.Warning:
					vvvvLogType = TLogType.Warning;
					break;
				case LogType.Error:
					vvvvLogType = TLogType.Error;
					break;
				default:
					vvvvLogType = TLogType.Message;
					break;
			}
			
			FHost.Log(vvvvLogType, message);
		}
	}
}
