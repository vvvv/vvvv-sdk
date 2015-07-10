using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VVVV.Core.Logging
{
    public static class DebugHelpers
    {
        public static void CatchAndLog(Action a, string FailingContext)
        {
            if (Shell.Instance.CommandLineArguments.ThrowExceptions)

                a();
                
            else

                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format("{0} failed with exception: \n{1}", FailingContext, e));
                }
        }

        public static void CatchAndLogNeverStop(Action a, string FailingContext)
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("{0} failed with exception: \n{1}", FailingContext, e));
            }
        }
    }
}
