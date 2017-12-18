using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.BassAsio;
using vvvv.Utils;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.ComponentModel;
using BassSound.Internals;

namespace vvvv.Nodes
{
    public class BassAsioAudioInNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "AudioIn";
                Info.Category = "Bass";
                Info.Version = "Asio";
                Info.Help = "Input node for bass asio";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Asio";

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

        private IValueIn FPinInDevice;
        private IValueIn FPinChannelIndex;
        private IValueIn FPinChannelCount;
        private IValueIn FPinInEnabled;

        private IValueOut FPinOutHandle;
        private IStringOut FPinOutStatus;

        private ChannelsManager manager;
        private InputChannelInfo FChannelInfo = new InputChannelInfo();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //We play this channel trough Asio output, so we choose the device NOSOUND
            Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, null);
            this.manager = ChannelsManager.GetInstance();

            BassUtils.LoadPlugins();

            this.FHost = Host;

            this.FHost.CreateValueInput("Device",1,null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
            this.FPinInDevice.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Channel Index", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinChannelIndex);
            this.FPinChannelIndex.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
        
            this.FHost.CreateValueInput("Channels Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinChannelCount);
            this.FPinChannelCount.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEnabled);
            this.FPinInEnabled.SetSubType(0, 1, 1, 0, false, true, true);
        
            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue,0, 0, false, false, true);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            #region Channel and device
            if (this.FPinInDevice.PinIsChanged || 
                this.FPinInEnabled.PinIsChanged || 
                this.FPinChannelCount.PinIsChanged ||
                this.FPinChannelIndex.PinIsChanged)
            {
                double ddevice;
                this.FPinInDevice.GetValue(0, out ddevice);
                int device = Convert.ToInt32(ddevice);

                if (device != -1)
                {
                    if (this.FChannelInfo.BassHandle.HasValue)
                    {
                        Bass.BASS_ChannelStop(this.FChannelInfo.BassHandle.Value);
                        Bass.BASS_StreamFree(this.FChannelInfo.BassHandle.Value);
                    }
                    this.manager.Delete(this.FChannelInfo.InternalHandle);

                    double enabled;
                    this.FPinInEnabled.GetValue(0, out enabled);
                    if (enabled >= 0.5)
                    {
                        double chancount, index;
                        this.FPinChannelCount.GetValue(0, out chancount);
                        this.FPinChannelIndex.GetValue(0, out index);

                        BassAsio.BASS_ASIO_Init(device);
                        BassAsio.BASS_ASIO_SetDevice(device);

                        this.FChannelInfo = new InputChannelInfo();
                        this.FChannelInfo.Count = Convert.ToInt32(chancount);
                        this.FChannelInfo.Index = Convert.ToInt32(index);

                        this.manager.CreateChannel(this.FChannelInfo);
                        this.FChannelInfo.Initialize(device);

                        this.FPinOutHandle.SetValue(0, this.FChannelInfo.InternalHandle);

                        this.FPinOutStatus.SetString(0, "OK");
                    }
                    else
                    {
                        this.FPinOutHandle.SetValue(0, 0);
                        this.FPinOutStatus.SetString(0, "Disabled");
                    }
                }
            }
            #endregion
        }
        #endregion

        public void Dispose()
        {
            BassAsio.BASS_ASIO_ChannelReset(true, 0, BASSASIOReset.BASS_ASIO_RESET_PAUSE | BASSASIOReset.BASS_ASIO_RESET_JOIN);
        }
    }
}
