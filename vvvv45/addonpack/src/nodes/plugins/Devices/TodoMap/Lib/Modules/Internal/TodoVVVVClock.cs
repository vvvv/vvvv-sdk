using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.TodoMap.Lib.Modules.Internal
{
    public class TodoVVVVClock : ITodoClock
    {
        private IPluginHost host;
        private IValueFastIn clockpin;

        public TodoVVVVClock(IPluginHost host)
        {
            this.host = host;
            this.host.CreateValueFastInput("Clock In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.clockpin);
            this.clockpin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false,false, false);        
        }

        public double Time
        {
            get 
            {
                double dbltime;
                this.clockpin.GetValue(0, out dbltime);
                return Time;
            }
        }

        #region ITodoClock Members
        public void Start()
        {

        }

        public void Stop()
        {

        }
        #endregion
    
        public void  Dispose()
        {
            this.host.DeletePin(this.clockpin);
        }

    }
}
