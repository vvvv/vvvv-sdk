using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using vvvv.Utils;
using Un4seen.BassAsio;

namespace vvvv.Nodes
{
    public class BassAsioChannelNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "DeviceChannels";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "Lists and initializes all Asio channels for an asio device";
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

        private IStringIn FPinInDevice;

        private IStringOut FPinOutInputChannels;
        private IStringOut FPinOutOutputChannels;


        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input
            this.FHost.CreateStringInput("Device", TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
            this.FPinInDevice.SetSubType("", false);

            //Output
            this.FHost.CreateStringOutput("Input Channels", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutInputChannels);
            this.FPinOutInputChannels.SetSubType("", false);

            this.FHost.CreateStringOutput("Output Channels", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutOutputChannels);
            this.FPinOutOutputChannels.SetSubType("", false);
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
            if (this.FPinInDevice.PinIsChanged)
            {
                string device;
                this.FPinInDevice.GetString(0,out device);

                int deviceindex = BassAsioUtils.GetDeviceIndex(device);
                if (deviceindex != -1)
                {
                    BassAsio.BASS_ASIO_Init(deviceindex);
                    if (BassAsio.BASS_ASIO_GetDevice() == deviceindex || BassAsio.BASS_ASIO_SetDevice(deviceindex))
                    {
                        //Get Device info
                        BASS_ASIO_INFO deviceinfo = BassAsio.BASS_ASIO_GetInfo();

                        //Setup inputs
                        this.FPinOutInputChannels.SliceCount = deviceinfo.inputs;
                        for (int i = 0; i < deviceinfo.inputs; i++) 
                        {
                            BASS_ASIO_CHANNELINFO info = BassAsio.BASS_ASIO_ChannelGetInfo(true, i);
                            this.FPinOutInputChannels.SetString(i, info.name);
                        }

                        //Setup Outputs
                        this.FPinOutOutputChannels.SliceCount = deviceinfo.outputs;
                        for (int i = 0; i < deviceinfo.outputs; i++) 
                        {
                            BASS_ASIO_CHANNELINFO info = BassAsio.BASS_ASIO_ChannelGetInfo(false, i);
                            this.FPinOutOutputChannels.SetString(i, info.name);
                        }
                    }
                } 
                else 
                {
                    this.FPinOutInputChannels.SliceCount = 0;
                    this.FPinOutOutputChannels.SliceCount = 0;
                }
            }
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion
    }
}
