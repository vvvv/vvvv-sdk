using System;
using System.Collections.Generic;
using System.Text;

namespace BassSound.Internals
{
    public class ChannelsManager
    {
        private static Dictionary<int, ChannelInfo> channels = new Dictionary<int, ChannelInfo>();
        private static int ChannelId = -10000000;

        public static ChannelInfo CreateChannel(ChannelInfo info)
        {
            ChannelId++;
            info.InternalHandle = ChannelId;
            channels[info.InternalHandle] = info;
            return info;
        }

        public static bool Exists(int id)
        {
            return channels.ContainsKey(id);
        }

        public static ChannelInfo GetChannel(int id)
        {
            return channels[id];
        }

        public static void Delete(int id)
        {
            channels.Remove(id);
        }
    }
}
