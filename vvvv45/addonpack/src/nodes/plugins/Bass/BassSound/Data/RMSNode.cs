using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass.Misc;
using BassSound.Internals;

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
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Analysis";

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
        private ChannelInfo FCHannel;
        private ChannelsManager FManager;
        //private int FHandle;

        private IValueIn FPinInHandle;

        private IValueOut FPinOutAverage;
        private IValueOut FPinOutRMS;

        private DSP_PeakLevelMeter FLevelMeter;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
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
                this.ClearUp();
                //Just Update the Handle
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                int ihandle = Convert.ToInt32(Math.Round(dhandle));

                if (this.FManager.Exists(ihandle))
                {
                    this.FCHannel = this.FManager.GetChannel(ihandle);
                    this.FCHannel.OnInit += new EventHandler(FCHannel_OnInit);
                    if (this.FCHannel.BassHandle.HasValue)
                    {
                        this.AddDSP();
                    }
                }
                else
                {
                    this.FCHannel = null;
                }
            }

            if (this.FCHannel != null && this.FPinInHandle.IsConnected)
            {
                if (this.FCHannel.BassHandle.HasValue)
                {
                    double rms, avg;

                    //Convert decibel to normalized.
                    rms = Math.Pow(10.0, (this.FLevelMeter.RMS_dBV / 20.0));
                    avg = Math.Pow(10.0, (this.FLevelMeter.AVG_dBV / 20.0));

                    

                    this.FPinOutRMS.SetValue(0, rms);
                    this.FPinOutAverage.SetValue(0, avg);
                }
            }
        }

        private void FCHannel_OnInit(object sender, EventArgs e)
        {
            this.ClearUp();
            this.AddDSP();
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
            try
            {
                this.FLevelMeter.Stop();
                this.FLevelMeter.Dispose();
            }
            catch
            {

            }
        }

        #endregion

        #region Add the DSP
        private void AddDSP()
        {
            if (this.FCHannel != null)
            {
                // create a buffer of the source stream
                //We can't get it from the main stream otherwise it would interfere with the asio buffering
                this.FLevelMeter = new DSP_PeakLevelMeter(this.FCHannel.BassHandle.Value, 1);
                this.FLevelMeter.UpdateTime = 0.05f;
                this.FLevelMeter.CalcRMS = true;
                this.FLevelMeter.Start();
            }
        }
        #endregion

        #region Clear Up
        private void ClearUp()
        {
            try
            {
                this.FLevelMeter.Stop();
                this.FLevelMeter.Dispose();
            }
            catch
            {

            }
        }
        #endregion
    }
}
