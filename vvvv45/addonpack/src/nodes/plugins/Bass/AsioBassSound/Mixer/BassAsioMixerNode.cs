using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using vvvv.Utils;

namespace vvvv.Nodes
{
    public class BassAsioMixerNode : IPlugin,IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Mixer";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "MixerNode for Bass Asio";
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

        private IValueIn FPinInChanCount;
        private IValueIn FPinInChannels;

        private IValueOut FPinOutHandle;

        private int FHandle = -1;
        private List<int> FChannels = new List<int>();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //We play this channel trough Asio output, so we choose the device NOSOUND
            Bass.BASS_Init(0, 44100, 0, IntPtr.Zero, null);

            this.FHost = Host;

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
            #region Reset the mixer
            if (this.FPinInChanCount.PinIsChanged)
            {
                if (this.FHandle != 0)
                {
                    BassAsioUtils.DecodingChannels.Remove(this.FHandle);
                    foreach (int channel in this.FChannels)
                    {
                        BassMix.BASS_Mixer_ChannelRemove(channel);
                    }
                    bool free = Bass.BASS_StreamFree(this.FHandle);
                }

                double numchans;
                this.FPinInChanCount.GetValue(0,out numchans);
                this.FHandle = BassMix.BASS_Mixer_StreamCreate(48000, Convert.ToInt32(numchans), BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);

                if (this.FHandle != 0)
                {
                    foreach (int channel in this.FChannels)
                    {
                        BassMix.BASS_Mixer_StreamAddChannel(this.FHandle, channel, BASSFlag.BASS_MIXER_MATRIX);
                    }
                    BassAsioUtils.DecodingChannels[this.FHandle] = true;
                }

                this.FPinOutHandle.SetValue(0, this.FHandle);
            }
            #endregion

            #region Channel pins changed
            if (this.FPinInChannels.PinIsChanged)
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

                //Remove channels not in the list anymore
                foreach (int oldchan in this.FChannels)
                {
                    if (!newchans.Contains(oldchan))
                    {
                        BassMix.BASS_Mixer_ChannelRemove(oldchan);
                    }
                }

                //Add new channels
                foreach (int newchan in newchans)
                {
                    if (!this.FChannels.Contains(newchan))
                    {
                        BassMix.BASS_Mixer_StreamAddChannel(this.FHandle, newchan, BASSFlag.BASS_MIXER_MATRIX);
                    }
                }

                //Update new channel list
                this.FChannels = newchans;
            }
            #endregion

            //Ned to check the play pause status on the play pin of the decoding channels
            ResetPlay();
        }
        #endregion

        #region Reset Play
        private void ResetPlay()
        {
            foreach (int handle in this.FChannels)
            {
                if (BassAsioUtils.IsChannelPlay(handle))
                {
                    BassMix.BASS_Mixer_ChannelPlay(handle);
                } 
                else 
                {
                    BassMix.BASS_Mixer_ChannelPause(handle);
                }
            }
        }
        #endregion

        public void Dispose()
        {
            BassAsioUtils.DecodingChannels.Remove(this.FHandle);
            Bass.BASS_StreamFree(this.FHandle);
        }
    }
}
