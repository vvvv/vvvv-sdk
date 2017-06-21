using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    /// <summary>
    /// Abstract Type for all DSPs, so we can have a common beahaviour.
    /// Basically DSP Node does nothing by themselves, and return an auto generated ID.
    /// All the effects are applied in the setDSP node, this will just return the right structure.
    /// </summary>
    /// <typeparam name="T">Effect Structure to send to bass.</typeparam>
    public abstract class AbstractDSPNode<T>: IDisposable
    {
        protected IPluginHost FHost;

        private ChannelInfo FChannel;
        private ChannelsManager FManager;

        private IValueIn FPinInHandle;
        private IValueIn FPinInPriority;
        private IValueIn FPinInEnabled;

        private IValueOut FPinOutFxHandle;

        protected T FDsp;
        protected int FDSPHandle = 0;

        protected abstract void OnPluginHostSet();
        protected abstract void OnEvaluate();
        protected abstract BASSFXType EffectType { get; }

        private bool FConnected = false;

        #region SetPluginHost
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Priority", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPriority);
            this.FPinInPriority.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateValueOutput("FX Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutFxHandle);

            this.OnPluginHostSet();

            //We put the enabled node at the end
            this.FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEnabled);
            this.FPinInEnabled.SetSubType(0, 1, 1, 0, false,true, false);
        }
        #endregion

        #region Configurate
        public virtual void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool reset = false;

            #region Reset is handle or fconnected change
            if (FConnected != this.FPinInHandle.IsConnected)
            {
                if (this.FPinInHandle.IsConnected)
                {
                    reset = true;
                }
                else
                {
                    this.ClearUp();
                }
                this.FConnected = this.FPinInHandle.IsConnected;
            }
            #endregion

            #region Reset
            if (this.FPinInHandle.PinIsChanged || reset || this.FPinInEnabled.PinIsChanged)
            {
                this.ClearUp();

                if (this.FPinInHandle.IsConnected)
                {
                    //Just Update the Handle
                    double dhandle;
                    this.FPinInHandle.GetValue(0, out dhandle);
                    int ihandle = Convert.ToInt32(Math.Round(dhandle));

                    if (this.FManager.Exists(ihandle))
                    {
                        this.FChannel = this.FManager.GetChannel(ihandle);
                        this.FChannel.OnInit += new EventHandler(FChannel_OnInit);
                        if (this.FChannel.BassHandle.HasValue)
                        {
                            this.AddDSP();
                            this.UpdateDSP();
                        }
                    }
                    else
                    {
                        this.FChannel = null;
                        this.FDSPHandle = 0;
                    }
                }
            }
            #endregion

            this.OnEvaluate();
        }

        void FChannel_OnInit(object sender, EventArgs e)
        {
            this.ClearUp();
            this.AddDSP();
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Clear Up
        private void ClearUp()
        {
            try
            {
                if (this.FChannel.BassHandle.HasValue && this.FDSPHandle != 0)
                {
                    Bass.BASS_ChannelRemoveDSP(this.FChannel.BassHandle.Value, this.FDSPHandle);
                }
            }
            catch
            {

            }

            this.FDSPHandle = 0;
        }
        #endregion

        #region Add the DSP
        private void AddDSP()
        {
            if (this.FChannel != null)
            {
                double dpriority;
                this.FPinInPriority.GetValue(0, out dpriority);

                double denabled;
                this.FPinInEnabled.GetValue(0, out denabled);

                if (this.FChannel.BassHandle.HasValue && denabled >=0.5)
                {
                    this.FDSPHandle = Bass.BASS_ChannelSetFX(this.FChannel.BassHandle.Value, this.EffectType, Convert.ToInt32(dpriority));
                    this.FPinOutFxHandle.SliceCount = 1;
                    this.FPinOutFxHandle.SetValue(0, this.FDSPHandle);
                }
            }
        }
        #endregion

        #region Update DSP
        protected virtual void UpdateDSP()
        {
            if (this.FChannel != null)
            {
                if (this.FChannel.BassHandle.HasValue && this.FDSPHandle != 0)
                {
                    Bass.BASS_FXSetParameters(this.FDSPHandle, this.FDsp);
                }
            }
        }
        #endregion


        #region IDisposable Members
        public void Dispose()
        {
            this.ClearUp();
        }
        #endregion
    }
}
