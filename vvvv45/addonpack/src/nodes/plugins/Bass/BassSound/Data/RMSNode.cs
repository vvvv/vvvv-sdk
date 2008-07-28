using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass.Misc;

namespace vvvv.Nodes
{
    public class RMSNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "RMS";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Retrieves the RMS from a channel";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        private IPluginHost FHost;
        private int FHandle;

        private IValueIn FPinInHandle;

        private IValueOut FPinOutAverage;
        private IValueOut FPinOutRMS;

        private DSP_PeakLevelMeter FLevelMeter;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input Pins
            this.FHost.CreateValueInput("HandleIn", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueOutput("RMS", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutRMS);
            this.FHost.CreateValueOutput("Average", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutAverage);

        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInHandle.PinIsChanged)
            {
                //Just Update the Handle
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                this.FHandle = Convert.ToInt32(Math.Round(dhandle));

                // create a buffer of the source stream
                //We can't get it from the main stream otherwise it would interfere with the asio buffering
                this.FLevelMeter = new DSP_PeakLevelMeter(this.FHandle, 1);
                this.FLevelMeter.UpdateTime = 0.05f;
                this.FLevelMeter.CalcRMS = true;
                this.FLevelMeter.Start();

            }

            if (this.FHandle != -1 && this.FPinInHandle.IsConnected)
            {
                this.FPinOutRMS.SetValue(0, this.FLevelMeter.RMS_dBV);
                this.FPinOutAverage.SetValue(0, this.FLevelMeter.AVG_dBV);
            }
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion


        #region IDisposable Members

        public virtual void Dispose()
        {
            this.FLevelMeter.Stop();
            this.FLevelMeter.Dispose();
        }

        #endregion
    }
}
