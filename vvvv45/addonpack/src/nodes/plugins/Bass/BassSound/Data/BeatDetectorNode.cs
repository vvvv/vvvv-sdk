using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using Un4seen.Bass.AddOn.Fx;

namespace BassSound.Data
{
    public class BeatDetectorNode: IPlugin,IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "BeatDetector";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Outputs a boang on each beat";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound";

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

        protected IPluginHost FHost;
        private ChannelsManager FManager;
        private ChannelInfo FChannel;

        private IValueIn FPinInHandle;

        private IValueIn FPinInRelease;
        private IValueIn FPinInBandWidth;
        private IValueIn FPinInCenter;

        private IValueOut FPinOutOnBeat;
        private bool FOnBeat = false;
        private BPMBEATPROC _beatProc;


        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("BandWidth", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInBandWidth);
            this.FPinInBandWidth.SetSubType(double.MinValue, double.MaxValue, 0.01,10.0, false, false, false);
            this.FHost.CreateValueInput("Center Frequency", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInCenter);
            this.FPinInCenter.SetSubType(double.MinValue, double.MaxValue, 0.01, 90.0, false, false, false);
            this.FHost.CreateValueInput("Release Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRelease);
            this.FPinInRelease.SetSubType(double.MinValue, double.MaxValue, 1,20.0, false, false, true);

            this.FHost.CreateValueOutput("On Beat", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutOnBeat);
            this.FPinOutOnBeat.SetSubType(0, 1, 1, 0, true, false, true);

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
            bool resetprms = false;

            if (this.FPinInHandle.PinIsChanged)
            {
                this.ClearUp();
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                int ihandle = Convert.ToInt32(Math.Round(dhandle));

                if (this.FManager.Exists(ihandle))
                {
                    this.FChannel = this.FManager.GetChannel(ihandle);
                    this.FChannel.OnInit += new EventHandler(FCHannel_OnInit);
                    if (this.FChannel.BassHandle.HasValue)
                    { 
                        this.Setup();
                        resetprms = true;
                    }
                }
            }

            if (this.FPinInCenter.PinIsChanged || this.FPinInRelease.PinIsChanged || this.FPinInBandWidth.PinIsChanged || resetprms)
            {
                if (this.FChannel != null)
                {
                    if (this.FChannel.BassHandle.HasValue)
                    {
                        double band, center, release;

                        this.FPinInBandWidth.GetValue(0, out band);
                        this.FPinInCenter.GetValue(0, out center);
                        this.FPinInRelease.GetValue(0, out release);

                        BassFx.BASS_FX_BPM_BeatSetParameters(this.FChannel.BassHandle.Value, (float)band, (float)center, (float)release);
                    }
                }
            }

            if (this.FOnBeat)
            {
                this.FPinOutOnBeat.SliceCount = 1;
                this.FPinOutOnBeat.SetValue(0, 1);
                this.FOnBeat = false;
            }
            else
            {
                this.FPinOutOnBeat.SliceCount = 1;
                this.FPinOutOnBeat.SetValue(0, 0);
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
            this.ClearUp();
        }
        #endregion

        #region Add the DSP
        private void Setup()
        {
            if (this.FChannel != null)
            {
                if (this.FChannel.BassHandle.HasValue)
                {
                    _beatProc = new BPMBEATPROC(MyBeatProc);
                    BassFx.BASS_FX_BPM_BeatCallbackSet(this.FChannel.BassHandle.Value, _beatProc, IntPtr.Zero);

                    double band, center, release;

                    this.FPinInBandWidth.GetValue(0, out band);
                    this.FPinInCenter.GetValue(0, out center);
                    this.FPinInRelease.GetValue(0, out release);

                    BassFx.BASS_FX_BPM_BeatSetParameters(this.FChannel.BassHandle.Value, (float)band, (float)center, (float)release);
                }
            }
        }
        #endregion

        private void FCHannel_OnInit(object sender, EventArgs e)
        {
            this.Setup();
        }

        private void MyBeatProc(int channel, double beatpos, IntPtr user)
        {
            this.FOnBeat = true;
        }

        private void ClearUp()
        {
            if (this.FChannel != null)
            {
                if (this.FChannel.BassHandle.HasValue)
                {
                    _beatProc = new BPMBEATPROC(MyBeatProc);
                    BassFx.BASS_FX_BPM_BeatFree(this.FChannel.BassHandle.Value);
                }
            }
        }
    }
}
