using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.BassAsio;
using Un4seen.Bass;

namespace vvvv.Utils
{
    public class BassUtils
    {
        #region Plugins Loaded
        private static bool FPluginsLoaded = false;

        public static void LoadPlugins()
        {
            if (!FPluginsLoaded)
            {
                try
                {
                    Bass.BASS_PluginLoadDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\plugins\\");
                }
                catch
                {
                }
                FPluginsLoaded = true;
            }
        }
        #endregion

        public static Dictionary<int, bool> DecodingChannels = new Dictionary<int, bool>();

        public static bool IsChannelPlay(int handle)
        {
            if (BassUtils.DecodingChannels.ContainsKey(handle))
            {
                return BassUtils.DecodingChannels[handle];
            }
            else
            {
                return false;
            }
        }
    }
}
