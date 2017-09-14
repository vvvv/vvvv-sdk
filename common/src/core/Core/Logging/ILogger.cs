using System;

namespace VVVV.Core.Logging
{
	/// <summary>
	/// Provides a simple logging mechanism.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Logs a message of specified type.
		/// </summary>
		void Log(LogType logType, string message);
	}
	
	public static class LoggerExtensions
	{
		/// <summary>
		/// Overload function to easier log exceptions.
		/// </summary>
		public static void Log(this ILogger logger, Exception e, LogType severity)
		{
			if (!string.IsNullOrEmpty(e.StackTrace) && e.InnerException != null)
			    logger.Log(severity, string.Format("{0} in {1}: {2}\n\nStacktrace:\n{3}\n\nInnerException:\n{4}\n", e.GetType(), e.Source, e.Message, e.StackTrace, e.InnerException));
			else if (!string.IsNullOrEmpty(e.StackTrace))
				logger.Log(severity, string.Format("{0} in {1}: {2}\n\nStacktrace:\n{3}\n", e.GetType(), e.Source, e.Message, e.StackTrace));
			else if (e.InnerException != null)
				logger.Log(severity, string.Format("{0} in {1}: {2}\n\nInnerException:\n{3}\n", e.GetType(), e.Source, e.Message, e.InnerException));
			else
				logger.Log(severity, string.Format("{0} in {1}: {2}\n", e.GetType(), e.Source, e.Message));
		}

        /// <summary>
        /// Overload function to easier log exceptions.
        /// </summary>
        public static void Log(this ILogger logger, Exception e)
        {
            Log(logger, e, LogType.Error);
        }

        public static void Log(this ILogger logger, LogType logType, string msg, object arg0)
		{
			logger.Log(logType, string.Format(msg, arg0));
		}
		
		public static void Log(this ILogger logger, LogType logType, string msg, object arg0, object arg1)
		{
			logger.Log(logType, string.Format(msg, arg0, arg1));
		}
		
		public static void Log(this ILogger logger, LogType logType, string msg, object arg0, object arg1, object arg2)
		{
			logger.Log(logType, string.Format(msg, arg0, arg1, arg2));
		}
		
		public static void Log(this ILogger logger, LogType logType, string msg, params object[] args)
		{
			logger.Log(logType, string.Format(msg, args));
		}
	}
}
