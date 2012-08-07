using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using vvvv.Utils;
using System.IO;

namespace BassSound.BroadCast
{
    public abstract class AbstractBroadCastNode<T> :IDisposable where T : StreamingServer
    {
        protected IPluginHost FHost;
        protected ChannelsManager FManager;

        private IValueIn FPinInHandle;
        protected IStringIn FPinInServer;
        protected IValueFastIn FPinInPort;
        protected IStringIn FPinInPwd;
        protected IValueFastIn FPinInBitrate;
        private IValueIn FPinInIsActive;

        private IStringOut FPinOutStatus;

        protected T FBroadCast = null;

        protected ChannelInfo FChannelInfo = null;

        protected abstract T GetStreamClass(int handle, out string msg);
        protected abstract void OnPluginHostSet();
        protected abstract void BeginEvaluate();
        protected abstract void EndEvaluate();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            BassUtils.LoadPlugins();

            this.FHost.CreateValueInput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateStringInput("Host", TSliceMode.Single, TPinVisibility.True, out this.FPinInServer);
            this.FPinInServer.SetSubType("localhost", false);

            this.FHost.CreateValueFastInput("Port", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPort);
            this.FPinInPort.SetSubType(double.MinValue, 65000, 8000, 0, false, false, true);

            this.FHost.CreateStringInput("Password", TSliceMode.Single, TPinVisibility.True, out this.FPinInPwd);
            this.FPinInPwd.SetSubType("", false);

            this.FHost.CreateValueFastInput("Bit Rate", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInBitrate);
            this.FPinInBitrate.SetSubType(32, 320000, 1, 64, false, false, true);


            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);

            this.OnPluginHostSet();

            this.FHost.CreateValueInput("Is Active", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInIsActive);
            this.FPinInIsActive.SetSubType(0, 1, 1, 0, false, true, true);
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
            this.BeginEvaluate();

            if (this.FPinInIsActive.PinIsChanged)
            {
                this.StopBroadCast();
                double active;
                this.FPinInIsActive.GetValue(0, out active);

                if (active >= 0.5)
                {

                    double dhandle;
                    this.FPinInHandle.GetValue(0, out dhandle);

                    if (this.FManager.Exists(Convert.ToInt32(dhandle)))
                    {
                        this.FChannelInfo = this.FManager.GetChannel(Convert.ToInt32(dhandle));
                        if (this.FChannelInfo.BassHandle.HasValue)
                        {
                            this.StartBroadCast();
                        }
                        else
                        {
                            this.FChannelInfo.OnInit += new EventHandler(FChannelInfo_OnInit);
                        }
                    }
                }
            }

            if (this.FBroadCast != null)
            {
                if (!this.FBroadCast.IsConnected)
                {
                    this.FPinOutStatus.SetString(0, this.FBroadCast.LastErrorMessage);
                }
                else
                {
                    this.FPinOutStatus.SetString(0, "OK");
                }
            }

            this.EndEvaluate();
        }
        #endregion

        #region Start Broadcast
        private void StartBroadCast()
        {
            string msg;
            this.FBroadCast = this.GetStreamClass(this.FChannelInfo.BassHandle.Value, out msg);
            if (this.FBroadCast.Connect())
            {
                this.FPinOutStatus.SetString(0, "Connected");
            }
            else
            {
                this.FPinOutStatus.SetString(0, this.FBroadCast.LastErrorMessage);
            }
        }
        #endregion

        #region On Channel Init
        void FChannelInfo_OnInit(object sender, EventArgs e)
        {
            this.StartBroadCast();
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Stop Broadcast
        private void StopBroadCast()
        {
            if (this.FBroadCast != null)
            {
                try
                {
                    this.FBroadCast.Disconnect();
                    this.FBroadCast.Dispose();
                    this.FBroadCast = null;
                }
                catch
                {

                }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.StopBroadCast();
        }
        #endregion

        #region Is Connected
        protected bool IsConnected
        {
            get
            {
                if (this.FBroadCast != null)
                {
                    return this.FBroadCast.IsConnected;
                }
                return false;
            }
        }
        #endregion
    }
}
