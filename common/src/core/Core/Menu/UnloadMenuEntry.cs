using System;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Model;

namespace VVVV.Core.Menu
{
    public class UnloadMenuEntry : MenuEntry
    {
        //protected IPersistent FPersistent;
        protected ILogger FLogger;
        
        public UnloadMenuEntry(ICommandHistory history, /*IPersistent persistent,*/ ILogger logger)
            : base(history, "Close")
        {
            //FPersistent = persistent;
            FLogger = logger;
        }
        
        public override void Click()
        {
            try
            {
                throw new NotImplementedException();
                //FPersistent.Unload();
            }
            catch (Exception e)
            {
                FLogger.Log(e);
            }
        }
    }
}
