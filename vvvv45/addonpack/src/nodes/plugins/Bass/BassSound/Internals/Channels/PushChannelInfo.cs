using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;

namespace BassSound.Internals
{
    public class PushChannelInfo : ChannelInfo
    {
        public override void Initialize(int deviceid)
        {
            Bass.BASS_SetDevice(deviceid);

            BASSFlag flag = BASSFlag.BASS_SAMPLE_FLOAT;
            if (this.IsDecoding)
            {
                flag = flag | BASSFlag.BASS_STREAM_DECODE;
            }

            if (this.Mono)
            {
                flag = flag | BASSFlag.BASS_SAMPLE_MONO;
            }

            int handle = Bass.BASS_StreamCreatePush(44100,1,flag,IntPtr.Zero);

            this.BassHandle = handle;

            this.OnInitialize();
        }
    }
}
