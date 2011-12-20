using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.BassAsio;
using Un4seen.Bass;
using BassSound.Internals;

namespace vvvv.Utils
{
    public class BassAsioUtils
    {

        #region Device Index
        public static int GetDeviceIndex(string name) 
        {
            int index = -1;
            BASS_ASIO_DEVICEINFO[] devices = BassAsio.BASS_ASIO_GetDeviceInfos();
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].name == name)
                {
                    index = i;
                }
            }

            return index;
        }
        #endregion

        #region Free Channels
        public static void FreeChannels(int[] handles)
        {
            foreach (int h in handles)
            {
                Bass.BASS_ChannelStop(h);
                Bass.BASS_StreamFree(h);
            }
        }

        public static void FreeChannel(int handle)
        {
            Bass.BASS_ChannelStop(handle);
            Bass.BASS_StreamFree(handle);
        }
        #endregion

        public static Dictionary<int, InputChannelInfo> InputChannels = new Dictionary<int, InputChannelInfo>();


    }
}
