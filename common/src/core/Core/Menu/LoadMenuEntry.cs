using System;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Model;

namespace VVVV.Core.Menu
{
    public class LoadMenuEntry : MenuEntry
    {
        //protected IPersistent FPersistent;
        protected ILogger FLogger;
        
        public LoadMenuEntry(ICommandHistory history, /*IPersistent persistent, */ILogger logger)
            : base(history, "Open")
        {
            //FPersistent = persistent;
            FLogger = logger;
        }
        
        public override void Click()
        {
            throw new NotImplementedException();
            //FPersistent.Load();
        }
    }
}
