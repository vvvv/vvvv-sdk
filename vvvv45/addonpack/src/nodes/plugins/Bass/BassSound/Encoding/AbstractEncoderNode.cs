using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using vvvv.Utils;
using System.IO;

namespace BassSound.Encoding.Internals
{
    public abstract class AbstractEncoderNode<T> :IDisposable where T : IBaseEncoder
    {
        protected IPluginHost FHost;
        protected ChannelsManager FManager;

        private IValueIn FPinInHandle;
        private IValueIn FPinInRecord;
        private IStringIn FPinInFilename;
        private IStringOut FPinOutStatus;

        protected IBaseEncoder FEncoder = null;
        protected ChannelInfo FChannelInfo = null;
        protected string FFilename = "";

        protected abstract T GetEncoder(int handle, out string msg);
        protected abstract void OnPluginHostSet();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            BassUtils.LoadPlugins();

            this.FHost.CreateValueInput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateStringInput("File Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            this.FHost.CreateValueInput("Record", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRecord);
            this.FPinInRecord.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);

            this.OnPluginHostSet();
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
            bool resetencoder = false;

            if (this.FPinInFilename.PinIsChanged || this.FPinInHandle.PinIsChanged)
            {
                this.StopEncoder();
                double dhandle;
                this.FPinInFilename.GetString(0, out this.FFilename);

                this.FPinInHandle.GetValue(0, out dhandle);

                int ihandle = Convert.ToInt32(dhandle);
                if (this.FManager.Exists(ihandle))
                {
                    this.FChannelInfo = this.FManager.GetChannel(ihandle);
                    if (this.FChannelInfo.BassHandle.HasValue)
                    {
                        string msg;
                        this.FEncoder = this.GetEncoder(this.FChannelInfo.BassHandle.Value, out msg);

                        if (this.FEncoder != null)
                        {
                            this.FEncoder.Start(null, IntPtr.Zero, true);
                            resetencoder = true;
                            this.FPinOutStatus.SetString(0, msg);
                        }    
                        
                    }
                    else
                    {
                        this.FChannelInfo.OnInit += new EventHandler(FChannelInfo_OnInit);
                        this.FPinOutStatus.SetString(0,"Channel not initialized");
                    }
                }
                else
                {
                    this.FChannelInfo = null;
                    this.FEncoder = null;
                }
            }

            if ((this.FEncoder != null && this.FPinInRecord.PinIsChanged) || (resetencoder))
            {
                double dorecord;
                this.FPinInRecord.GetValue(0, out dorecord);
                if (dorecord >= 0.5)
                {
                    this.FEncoder.Pause(false);
                }
                else
                {
                    this.FEncoder.Pause(true);
                }
            }
        }
        #endregion

        #region On Channel Init
        void FChannelInfo_OnInit(object sender, EventArgs e)
        {
            string msg;
            this.FEncoder = this.GetEncoder(this.FChannelInfo.BassHandle.Value,out msg);
            
            if (this.FEncoder != null)
            {
                this.FEncoder.Start(null, IntPtr.Zero, true);

            }

            double dorecord;
            this.FPinInRecord.GetValue(0, out dorecord);
            if (dorecord >= 0.5)
            {
                this.FEncoder.Pause(false);
            }
            else
            {
                this.FEncoder.Pause(true);
            }

            this.FPinOutStatus.SetString(0, msg);
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Stop Encoder
        private void StopEncoder()
        {
            if (this.FEncoder != null)
            {
                this.FEncoder.Stop();
            }
        }
        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            this.StopEncoder();
        }

        #endregion
    }
}
