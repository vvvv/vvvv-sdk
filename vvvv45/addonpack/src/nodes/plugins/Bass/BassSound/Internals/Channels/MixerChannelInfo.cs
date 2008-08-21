using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace BassSound.Internals
{
    public class MixerChannelInfo : ChannelInfo
    {
        private int numchans;
        private List<int> streams = new List<int>();

        public int NumChans
        {
            get { return numchans; }
            set { numchans = value; }
        }

        public List<int> Streams
        {
            get { return streams; }
            set 
            {
                BeforChannelUpdate(value);
                streams = value; 
            }
        }

        #region Initialize
        public override void Initialize(int deviceid)
        {
            Bass.BASS_SetDevice(deviceid);

            BASSFlag flag = BASSFlag.BASS_SAMPLE_FLOAT;

            if (this.IsDecoding)
            {
                flag = flag | BASSFlag.BASS_STREAM_DECODE;
            }

            int handle = BassMix.BASS_Mixer_StreamCreate(44100, this.NumChans, flag);

            this.BassHandle = handle;

            foreach (int channel in this.Streams)
            {
                BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value, channel, BASSFlag.BASS_MIXER_MATRIX);
            }

            Bass.BASS_ChannelPlay(this.BassHandle.Value, false);
        }
        #endregion

        #region Before Channel Update
        public void BeforChannelUpdate(List<int> newchans)
        {
            if (this.BassHandle.HasValue)
            {
                //Remove channels not in the list anymore
                foreach (int oldchan in this.streams)
                {
                    ChannelInfo info = ChannelsManager.GetChannel(oldchan);
                    if (!newchans.Contains(oldchan) && info.BassHandle.HasValue)
                    {
                        BassMix.BASS_Mixer_ChannelRemove(info.BassHandle.Value);
                    }
                }

                //Add new channels
                foreach (int newchan in newchans)
                {
                    ChannelInfo info = ChannelsManager.GetChannel(newchan);
                    //Need to be decoding channel, so can put it in nosound
                    if (!info.BassHandle.HasValue)
                    {
                        info.Initialize(0);
                    }

                    if (!this.streams.Contains(newchan))
                    {
                        BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value,info.BassHandle.Value, BASSFlag.BASS_MIXER_MATRIX);
                    }
                }
            }
        }
        #endregion
    }
}
