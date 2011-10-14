using System;

namespace VVVV.Core.Logging
{
	/// <summary>
	/// Used in the <see cref="VVVV.Core.Logging.ILogger.Log()">ILogger.Log</see> 
	/// function to specify the type of the log message.
	/// </summary>
	public enum LogType 
	{
		/// <summary>
		/// Specifies a debug message.
		/// </summary>
		Debug,
		/// <summary>
		/// Specifies an ordinary message.
		/// </summary>
		Message,
		/// <summary>
		/// Specifies a warning message.
		/// </summary>
		Warning,
		/// <summary>
		/// Specifies an errormessage.
		/// </summary>
		/// 
		Error
	};
}
