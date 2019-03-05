using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using BassSound.Internals;
using vvvv.Utils;

namespace BassSound
{
    public class BassAudioOutNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "AudioOut";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Audio out only for WDM handles";
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
        private ChannelsManager manager;

        private IValueIn FPinInDevice;
        private IValueIn FPinInBuffer;
        private IValueIn FPInInLoop;
        private IValueIn FPinInHandle;

        private int FDevice = -1000;
        private ChannelList FChannels = new ChannelList();

        private bool FHandleConnected = false;

        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.manager = ChannelsManager.GetInstance();

            BassUtils.LoadPlugins();

            this.FHost.CreateValueInput("Device", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
            this.FPinInDevice.SetSubType(-1, double.MaxValue, 1, -1, false, false, true);

            this.FHost.CreateValueInput("Handles In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue,1, 0, false, false, true);

            this.FHost.CreateValueInput("Update Period", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPInInLoop);
            this.FPInInLoop.SetSubType(double.MinValue, double.MaxValue, 1, 15, false, false, true);

            this.FHost.CreateValueInput("Buffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBuffer);
            this.FPinInBuffer.SetSubType(double.MinValue, double.MaxValue, 1, 128, false, false, true);
        }

        public void Configurate(IPluginConfig Input)
        {

        }

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {

            #region Pin Device Changed
            if (this.FPinInDevice.PinIsChanged)
            {
                double dbldevice;
                this.FPinInDevice.GetValue(0, out dbldevice);
                int devid = Convert.ToInt32(dbldevice);

                //0 is for no sound device, so redirect to default
                if (devid <= 0)
                {
                    devid = -1;
                }

                IntPtr ptr = IntPtr.Zero;
                Bass.BASS_Init(devid, 44100, BASSInit.BASS_DEVICE_DEFAULT, ptr, null);
                this.FDevice = devid;

                foreach (ChannelInfo channel in this.FChannels)
                {
                    if (channel.BassHandle.HasValue)
                    {
                        //Update all channels in the device
                        int chandevid = Bass.BASS_ChannelGetDevice(channel.BassHandle.Value);
                        if (this.FDevice != chandevid)
                        {
                            Bass.BASS_ChannelSetDevice(channel.BassHandle.Value, chandevid);
                        }
                    }
                }
            }
            #endregion

            if  (this.FPinInBuffer.PinIsChanged)
            {
                double db;
                this.FPinInBuffer.GetValue(0, out db);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, Convert.ToInt32(db));
            }
            if (this.FPInInLoop.PinIsChanged)
            {
                double db;
                this.FPInInLoop.GetValue(0, out db);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, Convert.ToInt32(db));
            }

            #region Handle Pin Changed
            if (this.FPinInHandle.PinIsChanged)
            {
                //Make sure we use the proper device
                Bass.BASS_SetDevice(this.FDevice);

                ChannelList oldchannels = new ChannelList();
                oldchannels.AddRange(this.FChannels);

                this.FChannels.Clear();

                for (int i = 0; i < this.FPinInHandle.SliceCount; i++)
                {
                    double dblhandle;
                    this.FPinInHandle.GetValue(0, out dblhandle);
                    int hid = Convert.ToInt32(dblhandle);

                    if (this.manager.Exists(hid))
                    {
                        //Get the channel in the list
                        ChannelInfo channel = this.manager.GetChannel(hid);

                        if (channel.BassHandle == null)
                        {
                            //Initialize channel
                            channel.Initialize(this.FDevice);
                        }
                        else
                        {
                            //Check if wrong device, if yes, update it
                            int chandevid = Bass.BASS_ChannelGetDevice(channel.BassHandle.Value);
                            if (this.FDevice != chandevid)
                            {
                                Bass.BASS_ChannelSetDevice(channel.BassHandle.Value, chandevid);
                            }
                        }
                        this.FChannels.Add(channel);
                        //Little trick to refresh
                        channel.Play = channel.Play;
                    }
                }


                //Pause the old channels not in it anymore
                foreach (ChannelInfo info in oldchannels)
                {
                    if (this.FChannels.GetByID(info.InternalHandle) == null)
                    {
                        if (info.BassHandle.HasValue)
                        {
                            Bass.BASS_ChannelPause(info.BassHandle.Value);
                        }
                    }
                }
            }
            #endregion

            #region Updated if pin connected
            if (this.FHandleConnected != this.FPinInHandle.IsConnected)
            {
                if (this.FPinInHandle.IsConnected)
                {
                    this.FChannels.RefreshPlay();
                }
                else
                {
                    this.FChannels.PauseAll();
                }
                this.FHandleConnected = this.FPinInHandle.IsConnected;
            }
            #endregion

        }
        #endregion

        public bool AutoEvaluate
        {
            get { return true; }
        }

        #region Dispose
        public void Dispose()
        {
            this.FChannels.PauseAll();
        }
        #endregion
    }
}
