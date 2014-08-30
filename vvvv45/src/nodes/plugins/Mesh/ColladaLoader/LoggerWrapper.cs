
using System;
using ColladaSlimDX.Utils;
using VVVV.Core.Logging;

namespace VVVV.Nodes
{
    class LoggerWrapper : ICOLLADALogger
    {
        private readonly ILogger FLogger;
        
        public LoggerWrapper(ILogger logger)
        {
            FLogger = logger;
        }
        
        public void Log(COLLADALogType type, string msg)
        {
            switch (type) 
            {
                case COLLADALogType.Debug:
                    FLogger.Log(LogType.Debug, msg);
                    break;
                case COLLADALogType.Message:
                    FLogger.Log(LogType.Message, msg);
                    break;
                case COLLADALogType.Warning:
                    FLogger.Log(LogType.Warning, msg);
                    break;
                case COLLADALogType.Error:
                    FLogger.Log(LogType.Error, msg);
                    break;
                default:
                    throw new Exception("Invalid value for COLLADALogType");
            }
        }
    }
}
