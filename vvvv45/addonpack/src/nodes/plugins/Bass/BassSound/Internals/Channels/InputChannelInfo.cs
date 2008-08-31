using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.BassAsio;
using Un4seen.Bass;

namespace BassSound.Internals
{
    public class InputChannelInfo : ChannelInfo
    {
        private BassAsioHandler FMyHandler;

        public BassAsioHandler Handler
        {
            get { return this.FMyHandler; }
        }

        public override void Initialize(int deviceid)
        {
            //BassAsio.BASS_ASIO_ChannelEnable(true,
            this.FMyHandler = new BassAsioHandler(true, deviceid, 0, 2, BASSASIOFormat.BASS_ASIO_FORMAT_FLOAT, 48000);
            if (!BassAsio.BASS_ASIO_IsStarted()) {
                BassAsio.BASS_ASIO_Start(0);
            }

            this.FMyHandler.SetFullDuplex(0, BASSFlag.BASS_STREAM_DECODE, false);

            this.BassHandle = this.FMyHandler.InputChannel;
            this.IsDecoding = true;
        }

        protected override void OnLoopEndUpdated()
        {
            
        }
    }
}
