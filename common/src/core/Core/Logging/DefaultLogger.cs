using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace VVVV.Core.Logging
{
	/// <summary>
	/// The DefaultLogger simply routes log messages to other loggers.
	/// The used logger collection is set up with one DebugLogger.
	/// </summary>
	public class DefaultLogger : ILogger
	{
		protected List<ILogger> FLoggers;
		protected SynchronizationContext FSyncContext;
		protected Thread FThread;
		
		public DefaultLogger()
		{
			FSyncContext = SynchronizationContext.Current;
			if (FSyncContext == null)
			{
			    FSyncContext = new SynchronizationContext();
			}
			Debug.Assert(FSyncContext != null, "Current SynchronizationContext must be set.");
			
			FLoggers = new List<ILogger>();
			FLoggers.Add(new DebugLogger());
			FThread = Thread.CurrentThread;
		}
		
		public void Log(LogType logType, string message)
		{
			if (Thread.CurrentThread != FThread)
			{
				FSyncContext.Post(PostedLog, new object[] { logType, message });
				return;
			}
			
			foreach (var logger in FLoggers)
			{
				logger.Log(logType, message);
			}
		}
		
		private void PostedLog(object state)
		{
			var arguments = state as object[];
			var logType = (LogType) arguments[0];
			var message = (string) arguments[1];
			Log(logType, message);
		}
		
		public void AddLogger(ILogger logger)
		{
			if (logger.GetType() != typeof(DebugLogger))
				FLoggers.Add(logger);
		}
		
		public void RemoveLogger(ILogger logger)
		{
			FLoggers.Remove(logger);
		}
	}
}
