using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VVVV.Core.Logging
{
    public static class DebugHelpers
    {
        static void CatchAndLog(Action a, bool throwAnyway, string failingContext, Action<Exception> theCatch = null)
        {
            if (throwAnyway)

                a();

            else

                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Shell.Instance.Logger.Log(e, string.Format("{0} failed", failingContext));
                    if (theCatch != null)
                        theCatch(e);
                }
        }

        public static void CatchAndLog(Action a, string failingContext, Action<Exception> theCatch = null)
        {
            CatchAndLog(a, Shell.CommandLineArguments.ThrowExceptions, failingContext, theCatch);
        }

        public static void CatchAndLogFrontEnd(Action a, string failingContext, Action<Exception> theCatch = null)
        {
            var args = Shell.CommandLineArguments;
            CatchAndLog(a, args.ThrowExceptions || args.ThrowFrontEndExceptions, failingContext, theCatch);
        }

        public static void CatchAndLogBackEnd(Action a, string failingContext, Action<Exception> theCatch = null)
        {
            var args = Shell.CommandLineArguments;
            CatchAndLog(a, args.ThrowExceptions || args.ThrowBackEndExceptions, failingContext, theCatch);
        }

        public static void CatchAndLogNeverStop(Action a, string failingContext)
        {
            CatchAndLog(a, false, failingContext, (e) => { } );
        }
    }
}
