using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class MixerNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Mixer";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Mixer for Bass";
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

        private IPluginHost FHost;
        private ChannelsManager manager;

        private int FInputCount = 0;
        private MixerChannelInfo FMixerInfo = new MixerChannelInfo();

        private IValueIn FPinInPlay;

        private IValueIn FPinInIsDecoding;
        private IValueIn FPinInChanCount;
        private IValueConfig FPinInNumInputs;

        private IValueOut FPinOutHandle;

        private List<IValueIn> FPinHandles = new List<IValueIn>();
        private List<IValueIn> FPinLevels = new List<IValueIn>();


        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.manager = ChannelsManager.GetInstance();
            this.manager.OnChannelDeleted += new GenericEventHandler<int>(manager_OnChannelDeleted);

            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Is Decoding", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinInIsDecoding);
            this.FPinInIsDecoding.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Channel Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInChanCount);
            this.FPinInChanCount.SetSubType(0, double.MaxValue, 1, 2, false, false, true);

            this.FHost.CreateValueConfig("Inputs Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInNumInputs);
            this.FPinInNumInputs.SetSubType(0, double.MaxValue, 1,0, false, false, true);

            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

        }

        public void manager_OnChannelDeleted(int args)
        {
            ChannelInfo info = this.FMixerInfo.Streams.GetByID(args);
            if (info != null)
            {
                this.FMixerInfo.DetachChannel(info);
            }
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            double inputcnt;
            this.FPinInNumInputs.GetValue(0, out inputcnt);

            int prevcnt = this.FInputCount;
            this.FInputCount = Convert.ToInt32(inputcnt);

            if (this.FInputCount > prevcnt)
            {
                //Add new pins, as value is bigger
                while (FPinHandles.Count < this.FInputCount)
                {
                    IValueIn pin;

                    int index = FPinHandles.Count + 1;

                    //Add new Handle pin
                    this.FHost.CreateValueInput("Handle In " + index, 1, null, TSliceMode.Single, TPinVisibility.True, out pin);
                    pin.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
                    this.FPinHandles.Add(pin);

                    //Add associated level pin
                    this.FHost.CreateValueInput("Volume " + index, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pin);
                    pin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
                    this.FPinLevels.Add(pin);       
                }
            }
            else
            {
                //Remove pins, as value is lower
                while (FPinHandles.Count > this.FInputCount)
                {
                    this.FHost.DeletePin(this.FPinHandles[this.FPinHandles.Count - 1]);
                    this.FPinHandles.RemoveAt(this.FPinHandles.Count - 1);

                    this.FHost.DeletePin(this.FPinLevels[this.FPinLevels.Count - 1]);
                    this.FPinLevels.RemoveAt(this.FPinLevels.Count - 1);
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

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            #region Reset the mixer
            if (this.FPinInChanCount.PinIsChanged || this.FPinInIsDecoding.PinIsChanged)
            {
                while (this.FMixerInfo.Streams.Count >0) 
                {
                    this.FMixerInfo.DetachChannel(this.FMixerInfo.Streams[0]);
                }

                if (this.FMixerInfo.InternalHandle != 0)
                {
                    if (this.FMixerInfo.BassHandle != null)
                    {
                        bool free = Bass.BASS_StreamFree(this.FMixerInfo.BassHandle.Value);
                    }
                    this.manager.Delete(this.FMixerInfo.InternalHandle);
                }

                ChannelList channels = this.ListChannels();

                double numchans;
                this.FPinInChanCount.GetValue(0, out numchans);

                double decoding;
                this.FPinInIsDecoding.GetValue(0, out decoding);

                double isplay;
                this.FPinInPlay.GetValue(0, out isplay);

                MixerChannelInfo newmixerinfo = new MixerChannelInfo();
                newmixerinfo.IsDecoding = decoding == 1;
                newmixerinfo.NumChans = Convert.ToInt32(numchans);
                newmixerinfo.Play = isplay == 1;
                this.manager.CreateChannel(newmixerinfo);

                this.FMixerInfo = newmixerinfo;

                foreach (ChannelInfo info in this.ListChannels())
                {
                    this.FMixerInfo.AttachChannel(info);
                }
              
                this.FPinOutHandle.SetValue(0, this.FMixerInfo.InternalHandle);

            }
            #endregion

            #region Update Mixer pins
            bool update = false;
            foreach (IValueIn pin in this.FPinHandles)
            {
                if (pin.PinIsChanged)
                {
                    update = true;
                }
            }

            #region Update Channels
            if (update)
            {
                ChannelList channels = this.ListChannels();

                ChannelList todetach = new ChannelList();

                foreach (ChannelInfo info in this.FMixerInfo.Streams)
                {
                    if (!channels.Contains(info))
                    {
                        todetach.Add(info);
                    }
                }

                foreach (ChannelInfo info in todetach)
                {
                    this.FMixerInfo.DetachChannel(info);
                }

                foreach (ChannelInfo info in channels)
                {
                    if (!this.FMixerInfo.Streams.Contains(info))
                    {
                        this.FMixerInfo.AttachChannel(info);
                        UpdateMatrix(this.FMixerInfo.Streams.IndexOf(info));
                    }
                }
            }
            #endregion


            for (int i=0; i < this.FPinLevels.Count;i++)
            {
                if (this.FPinLevels[i].PinIsChanged) 
                {
                    UpdateMatrix(i);
                }
            }
            #endregion

            if (this.FPinInPlay.PinIsChanged)
            {
                double isplay;
                this.FPinInPlay.GetValue(0, out isplay);
                this.FMixerInfo.Play = isplay == 1;
            }
        }
        #endregion

        #region List Channels
        private ChannelList ListChannels()
        {
            ChannelList result = new ChannelList();

            foreach (IValueIn pin in this.FPinHandles)
            {
                double dhandle;
                pin.GetValue(0, out dhandle);
                int handle = Convert.ToInt32(dhandle);

                if (this.manager.Exists(handle))
                {
                    ChannelInfo info = this.manager.GetChannel(handle);
                    result.Add(info);
                }
            }
            return result;
        }
        #endregion

        #region Update Matrix
        private void UpdateMatrix(int index)
        {
            double dhandle;
            this.FPinHandles[index].GetValue(0, out dhandle);

            ChannelInfo info = this.manager.GetChannel(Convert.ToInt32(dhandle));

            if (info != null)
            {
                int mixerhandle = 0;

                if (info.BassHandle.HasValue)
                {
                    mixerhandle = BassMix.BASS_Mixer_ChannelGetMixer(info.BassHandle.Value);
                }

                if (mixerhandle != 0)
                {
                    BASS_CHANNELINFO MIXER = Bass.BASS_ChannelGetInfo(mixerhandle);
                    BASS_CHANNELINFO CHANNEL = Bass.BASS_ChannelGetInfo(info.BassHandle.Value);
                    float[,] matrix = new float[MIXER.chans, CHANNEL.chans];
                    BassMix.BASS_Mixer_ChannelGetMatrix(info.BassHandle.Value, matrix);
                    int idx = 0;



                    for (int i = 0; i < MIXER.chans; i++)
                    {
                        for (int j = 0; j < CHANNEL.chans; j++)
                        {
                            double level;
                            this.FPinLevels[index].GetValue(idx, out level);
                            matrix[i, j] = (float)level;

                            idx++;
                            if (idx == this.FPinLevels[index].SliceCount)
                            {
                                idx = 0;
                            }
                        }
                    }

                    BassMix.BASS_Mixer_ChannelSetMatrix(info.BassHandle.Value, matrix);
                }
            }
        }
        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (this.FMixerInfo.InternalHandle != 0)
            {
                while (this.FMixerInfo.Streams.Count > 0)
                {
                    this.FMixerInfo.DetachChannel(this.FMixerInfo.Streams[0]);
                }

                if (this.FMixerInfo.BassHandle.HasValue)
                {

                    Bass.BASS_ChannelStop(this.FMixerInfo.BassHandle.Value);
                    Bass.BASS_StreamFree(this.FMixerInfo.BassHandle.Value);
                }
                this.manager.Delete(this.FMixerInfo.InternalHandle);
            }
        }

        #endregion
    }
}
