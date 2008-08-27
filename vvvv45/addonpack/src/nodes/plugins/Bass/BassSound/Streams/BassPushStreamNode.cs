using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using Un4seen.Bass;

namespace BassSound.Streams
{
    public class BassPushStreamNode: IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "PushStream";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Push Stream for Bass, feed the buffer";
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
        private ChannelsManager manager;
        private PushChannelInfo FChannelInfo = new PushChannelInfo();
        private float[] FBuffer;

        private IValueIn FPinCfgIsDecoding;
        private IValueIn FPinInPlay;
        private IValueIn FPinInData;

        private IValueOut FPinOutHandle;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.manager = ChannelsManager.GetInstance();

            //Config Pins
            this.FHost.CreateValueInput("Is Decoding", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinCfgIsDecoding);
            this.FPinCfgIsDecoding.SetSubType(0, 1, 1, 0, false, true, true);

            //Input Pins
            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Data", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInData);
            this.FPinInPlay.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            //Output
            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);
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
            if (this.FPinInData.PinIsChanged)
            {
                this.FBuffer = new float[this.FPinInData.SliceCount];
                for (int i = 0; i < this.FPinInData.SliceCount; i++)
                {
                    double d;
                    this.FPinInData.GetValue(i, out d);
                    this.FBuffer[i] = (float)d;
                }
            }

            if (this.FChannelInfo.BassHandle != null)
            {
                if (this.FBuffer != null)
                {
                    Bass.BASS_StreamPutData(this.FChannelInfo.BassHandle.Value, this.FBuffer, this.FBuffer.Length);
                }
            }

            #region Update Play/Pause
            if (this.FPinInPlay.PinIsChanged)
            {
                if (this.FChannelInfo.InternalHandle != 0)
                {
                    double doplay;
                    this.FPinInPlay.GetValue(0, out doplay);
                    if (doplay == 1 && this.FPinOutHandle.IsConnected)
                    {
                        this.manager.GetChannel(this.FChannelInfo.InternalHandle).Play = true;
                    }
                    else
                    {
                        this.manager.GetChannel(this.FChannelInfo.InternalHandle).Play = false;
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
        }
        #endregion
    }
}
