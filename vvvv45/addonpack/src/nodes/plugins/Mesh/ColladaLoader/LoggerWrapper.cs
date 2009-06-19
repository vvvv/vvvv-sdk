using System;
using VVVV.PluginInterfaces.V1;
using ColladaSlimDX.Utils;

namespace VVVV.Nodes
{
	/// <summary>
	/// Delegates logging of Collada to VVVV
	/// </summary>
	public class LoggerWrapper : ICOLLADALogger
	{
		IPluginHost FHost;
		public LoggerWrapper(IPluginHost Host)
		{
			FHost = Host;
		}
		
		public void Log(COLLADALogType LogType, string Message)
		{
			switch (LogType) 
			{
				case COLLADALogType.Debug: FHost.Log(TLogType.Debug, Message); break;
				case COLLADALogType.Message: FHost.Log(TLogType.Message, Message); break;
				case COLLADALogType.Warning: FHost.Log(TLogType.Warning, Message); break;
				case COLLADALogType.Error: FHost.Log(TLogType.Error, Message); break;
			}
		}
	}
}
