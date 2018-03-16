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

            BASSFlag flag = BASSFlag.BASS_DEFAULT | BASSFlag.BASS_SAMPLE_FLOAT;

            if (this.IsDecoding)
            {
                flag = flag | BASSFlag.BASS_STREAM_DECODE;
            }

            int handle = BassMix.BASS_Mixer_StreamCreate(44100, this.NumChans, flag);

            this.BassHandle = handle;

            //Add the channel list in bass now
            foreach (ChannelInfo info in this.Streams)
            {
                if (!info.BassHandle.HasValue)
                {
                    info.Initialize(deviceid);
                }
                else
                {
                    if (info.BassHandle.Value == 0)
                    {
                        info.Initialize(deviceid);
                    }
                }
                BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value, info.BassHandle.Value, BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_MIXER_BUFFER);
                info.Play = info.Play;
            }

            this.Play = this.Play;

            //Bass.BASS_ChannelPlay(this.BassHandle.Value, false);
            this.OnInitialize();
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
            if (info is InputChannelInfo)
            {
                InputChannelInfo iinfo = (InputChannelInfo)info;
                if (this.BassHandle.HasValue)
                {
                    BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value, iinfo.Handler.OutputChannel, BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_MIXER_BUFFER);
                }
            }
            else
            {
                if (!info.BassHandle.HasValue)
                {
                    //Initialize to no sound as it's decoding anyway
                    info.Initialize(0);
                }
                this.streams.Add(info);

                if (this.BassHandle.HasValue)
                {
                    BassMix.BASS_Mixer_StreamAddChannel(this.BassHandle.Value, info.BassHandle.Value, BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_MIXER_BUFFER);
                }
                //Simple trick to refresh play status now it's attached
                
            }
            info.Play = info.Play;
        }
        #endregion

        protected override void OnLoopEndUpdated()
        {
            
        }
    }
}
