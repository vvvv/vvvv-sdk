using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using vvvv.Utils;
using BassSound.Internals;

namespace vvvv.Nodes
{
    public class BassMixerNode : IPlugin,IDisposable
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
                Info.Help = "MixerNode for Bass";
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

        private IValueIn FPinInIsDecoding;
        private IValueIn FPinInChanCount;
        private IValueIn FPinInChannels;

        private IValueOut FPinOutHandle;

        private int FHandle = 0;
        private List<int> FChannels = new List<int>();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("Is Decoding", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinInIsDecoding);
            this.FPinInIsDecoding.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Channel Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInChanCount);
            this.FPinInChanCount.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Handles",1,null,TSliceMode.Dynamic,TPinVisibility.True, out this.FPinInChannels);
            this.FPinInChannels.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
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
            get { return false; }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool updatechans = false;

            #region Reset the mixer
            if (this.FPinInChanCount.PinIsChanged)
            {
                if (this.FHandle != 0)
                {
                    ChannelInfo channelinfo = ChannelsManager.GetChannel(this.FHandle);

                    if (channelinfo.BassHandle != null)
                    {
                        foreach (int channel in this.FChannels)
                        {
                            BassMix.BASS_Mixer_ChannelRemove(channelinfo.BassHandle.Value);
                        }
                        bool free = Bass.BASS_StreamFree(channelinfo.BassHandle.Value);
                    }
                }

                double numchans;
                this.FPinInChanCount.GetValue(0,out numchans);

                double decoding;
                this.FPinInIsDecoding.GetValue(0,out decoding);

                MixerChannelInfo mixerinfo = new MixerChannelInfo();
                mixerinfo.IsDecoding = decoding == 1;
                mixerinfo.NumChans = Convert.ToInt32(numchans);
                mixerinfo.Play = true;
                ChannelsManager.CreateChannel(mixerinfo);


                this.FHandle = mixerinfo.InternalHandle;
                updatechans = true;

                this.FPinOutHandle.SetValue(0, this.FHandle);
            }
            #endregion

            #region Channel pins changed
            if (this.FPinInChannels.PinIsChanged)
            {
                if (this.FHandle != 0)
                {
                    List<int> newchans = new List<int>();
                    for (int i = 0; i < this.FPinInChannels.SliceCount; i++)
                    {
                        //Get list of new channels
                        double ch;
                        this.FPinInChannels.GetValue(i, out ch);
                        if (ch != 0 && ch != -1)
                        {
                            newchans.Add(Convert.ToInt32(ch));
                        }
                    }

                    //Update new channel list
                    this.FChannels = newchans;
                }
                else
                {
                    this.FChannels.Clear();
                }

                updatechans = true; 
            }
            #endregion

            if (updatechans)
            {
                MixerChannelInfo mixerinfo = (MixerChannelInfo)ChannelsManager.GetChannel(this.FHandle);
                mixerinfo.Streams = this.FChannels;
            }

            //Ned to check the play pause status on the play pin of the decoding channels
            ResetPlay();
        }
        #endregion

        #region Reset Play
        private void ResetPlay()
        {
            foreach (int handle in this.FChannels)
            {
                ChannelInfo info = ChannelsManager.GetChannel(handle);
                if (info.BassHandle != null)
                {
                    if (info.Play)
                    {
                        BassMix.BASS_Mixer_ChannelPlay(handle);
                    }
                    else
                    {
                        BassMix.BASS_Mixer_ChannelPause(handle);
                    }
                }
            }
        }
        #endregion

        public void Dispose()
        {

        }
    }
}
