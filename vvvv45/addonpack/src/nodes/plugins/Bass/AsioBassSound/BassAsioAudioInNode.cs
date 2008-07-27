using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.BassAsio;
using vvvv.Utils;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace vvvv.Nodes
{


    internal class BassAsioAudioInNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "AudioIn";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "Input node for bass asio";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "Few hard coded vars until I decide how to handle this";

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

        private IStringIn FPinInDevice;
        private IValueIn FPinInChannel;
        private IValueIn FPinChannelCount;

        private IValueOut FPinOutHandle;
        private IStringOut FPinOutStatus;

        private int FDeviceIndex = -1;
        private BassAsioHandler FMyHandler;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //We play this channel trough Asio output, so we choose the device NOSOUND
            Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, null);

            BassAsioUtils.LoadPlugins();

            this.FHost = Host;

            this.FHost.CreateStringInput("Device", TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
            this.FPinInDevice.SetSubType("", false);

            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

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
            if (this.FPinInDevice.PinIsChanged)
            {
                string device;
                this.FPinInDevice.GetString(0, out device);

                this.FDeviceIndex = BassAsioUtils.GetDeviceIndex(device);

                if (this.FDeviceIndex != -1)
                {
                    BassAsio.BASS_ASIO_Init(this.FDeviceIndex);
                    BassAsio.BASS_ASIO_SetDevice(this.FDeviceIndex);

                    this.FMyHandler = new BassAsioHandler(true,this.FDeviceIndex,0,2,BASSASIOFormat.BASS_ASIO_FORMAT_FLOAT,48000);
                    BassAsio.BASS_ASIO_Start(0);

                    BassAsioUtils.InputChannels.Add(this.FMyHandler.InputChannel,this.FMyHandler);
                    
                    this.FPinOutHandle.SetValue(0, this.FMyHandler.InputChannel);
                }
            }
            #endregion
        }
        #endregion

        public void Dispose()
        {
            this.FMyHandler.Dispose();
            BassAsioUtils.InputChannels.Remove(this.FMyHandler.InputChannel);
            BassAsio.BASS_ASIO_ChannelReset(true, 0, BASSASIOReset.BASS_ASIO_RESET_PAUSE | BASSASIOReset.BASS_ASIO_RESET_JOIN);
        }
    }
}
