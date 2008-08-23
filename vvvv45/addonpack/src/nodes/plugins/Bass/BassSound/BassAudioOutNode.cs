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
        private List<int> FChannels = new List<int>();

        private IValueIn FPinInDevice;
        private IValueIn FPinInHandle;

        private int FDevice = -1000;
        private List<int> FHandles = new List<int>();

        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.manager = ChannelsManager.GetInstance();

            BassUtils.LoadPlugins();

            this.FHost.CreateValueInput("Device", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
            this.FPinInDevice.SetSubType(-1, double.MaxValue, 1, -1, false, false, true);

            this.FHost.CreateValueInput("HandleIn", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue,1, 0, false, false, true);
        }

        public void Configurate(IPluginConfig Input)
        {

        }

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
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
            }

            #region Handle Pin Changed
            if (this.FPinInHandle.PinIsChanged)
            {
                //Make sure we use the proper device
                Bass.BASS_SetDevice(this.FDevice);

                this.FHandles.Clear();

                for (int i = 0; i < this.FPinInHandle.SliceCount; i++)
                {
                    double dblhandle;
                    this.FPinInHandle.GetValue(0, out dblhandle);
                    int hid = Convert.ToInt32(dblhandle);

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

                    this.FHandles.Add(channel.InternalHandle);
                }
            }
            #endregion

            //Do nothing for the moment, can still use as a feed for file stream
            /*
            if (this.FPinInHandle.IsConnected && this.FPinInHandle.PinIsChanged)
            {
                StopChannels();
                this.FChannels.Clear();
                //Browse channel list
                for (int i = 0; i < this.FPinInHandle.SliceCount; i++)
                {
                    double dhandle;
                    this.FPinInHandle.GetValue(0,out dhandle);
                    int handle = Convert.ToInt32(dhandle);
                    this.FChannels.Add(handle);
                    Bass.BASS_ChannelPlay(handle, false);
                }
            }
            else
            {
                StopChannels();
            }*/
        }
        #endregion

        public bool AutoEvaluate
        {
            get { return true; }
        }

        #region Dispose
        public void Dispose()
        {
            foreach (int handle in this.FHandles)
            {
                Bass.BASS_ChannelPause(this.manager.GetChannel(handle).BassHandle.Value);
            }
        }
        #endregion
    }
}
