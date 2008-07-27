using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace BassSound
{
    [Obsolete("In progress")]
    internal class BassAudioOutNode
    {
        protected IPluginHost FHost;
        private int FHandle;

        private IValueIn FPinInHandle;

        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("HandleIn", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);



        }

        public void Configurate(IPluginConfig Input)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInHandle.IsConnected)
            {

            }
            else
            {
                if (this.FHandle != -1)
                {
                    StopChannel();
                }
            }
        }

        private void StopChannel()
        {
            Bass.BASS_ChannelStop(this.FHandle);
            this.FHandle = -1;
        }

        public bool AutoEvaluate
        {
            get { return true; }
        }

        public void Dispose()
        {

        }
    }
}
