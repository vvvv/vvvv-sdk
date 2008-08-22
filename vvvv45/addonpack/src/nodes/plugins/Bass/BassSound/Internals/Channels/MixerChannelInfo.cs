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
        private ChannelList streams = new ChannelList();

        public int NumChans
        {
            get { return numchans; }
            set { numchans = value; }
        }

        public ChannelList Streams
        {
            get { return streams; }
            set 
            {
                //BeforeChannelUpdate(value);
                streams = value; 
            }
        }

        #region Initialize
        public override void Initialize(int deviceid)
        {
            Bass.BASS_SetDevice(deviceid);

            BASSFlag flag = BASSFlag.BASS_DEFAULT;

            if (this.IsDecoding)
            {
                flag = flag | BASSFlag.BASS_STREAM_DECODE;
            }

            int handle = BassMix.BASS_Mixer_StreamCreate(44100, this.NumChans, flag);

            this.BassHandle = handle;

            //Add the channel list in bass now
            foreach (ChannelInfo info in this.Streams)
            {
                BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value, info.BassHandle.Value, BASSFlag.BASS_MIXER_MATRIX);
            }

            Bass.BASS_ChannelPlay(this.BassHandle.Value, false);
        }
        #endregion

        #region Detach Channel
        public void DetachChannel(ChannelInfo info)
        {
            if (info.BassHandle.HasValue)
            {
                Bass.BASS_ChannelPause(info.BassHandle.Value);
                BassMix.BASS_Mixer_ChannelRemove(info.BassHandle.Value);
            }
            this.Streams.Remove(info);
        }
        #endregion

        #region Attach Channel
        public void AttachChannel(ChannelInfo info)
        {
            if (!info.BassHandle.HasValue)
            {
                info.Initialize(0);
            }

            this.streams.Add(info);

            if (this.BassHandle.HasValue)
            {
                BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value, info.BassHandle.Value, BASSFlag.BASS_MIXER_MATRIX);
            }
        }
        #endregion
    }
}
