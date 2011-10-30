using System;
using System.Diagnostics;

namespace VVVV.Core.Logging
{
	/// <summary>
	/// Writes log messages to the System.Diagnostics.Debug class.
	/// </summary>
	public class DebugLogger : ILogger
	{
		public void Log(LogType logType, string message)
		{
			Debug.WriteLine(string.Format("{0}: {1}", logType, message));
		}
	}
}
