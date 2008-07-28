using System;
using System.Collections.Generic;
using System.Text;

namespace vvvv.Utils
{
    public class BassChannelsStatus
    {
        public static Dictionary<int, bool> ChannelStatus = new Dictionary<int, bool>();

        public static void PutStatus(int channel, bool status)
        {
            BassChannelsStatus.ChannelStatus[channel] = status;
        }
    }
}
