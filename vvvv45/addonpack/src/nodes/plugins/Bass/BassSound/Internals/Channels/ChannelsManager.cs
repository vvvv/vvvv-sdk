using System;
using System.Collections.Generic;
using System.Text;

namespace BassSound.Internals
{
    public delegate void GenericEventHandler<T>(T args);

    public class ChannelsManager
    {
        private static ChannelsManager instance;

        private Dictionary<int, ChannelInfo> channels = new Dictionary<int, ChannelInfo>();
        private int ChannelId = -10000000;

        public event GenericEventHandler<int> OnChannelDeleted;

        #region Get Instance
        public static ChannelsManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ChannelsManager();
            }
            return instance;
        }
        #endregion

        #region Create Channel
        public ChannelInfo CreateChannel(ChannelInfo info)
        {
            ChannelId++;
            info.InternalHandle = ChannelId;
            channels[info.InternalHandle] = info;
            return info;
        }
        #endregion

        #region Exists
        public bool Exists(int id)
        {
            return channels.ContainsKey(id);
        }
        #endregion

        #region Get Channel
        public ChannelInfo GetChannel(int id)
        {
            if (channels.ContainsKey(id))
            {
                return channels[id];
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Delete
        public void Delete(int id)
        {

            if (OnChannelDeleted != null)
            {
                OnChannelDeleted(id);
            }
            channels.Remove(id);
        }
        #endregion
    }
}
