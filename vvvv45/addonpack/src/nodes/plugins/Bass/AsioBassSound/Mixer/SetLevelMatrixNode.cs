using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass;

namespace AsioBassSound.Mixer
{
    public class SetLevelMatrixNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo 
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SetMixerLevel";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "Set Mixer levelfor Bass Asio";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        private IPluginHost FHost;

        private IValueIn FPinInHandle;
        private IValueIn FPinInLevel;

        private IValueOut FPinOutHandle;

        private int FHandle = 0;
        private int FMixerHandle = 0;
        private BASS_CHANNELINFO FMixerInfo;
        private BASS_CHANNELINFO FChannelInfo;
        private List<int> FChannels = new List<int>();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Levels", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInLevel);
            this.FPinInLevel.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            int oldmixhandle = this.FMixerHandle;
            if (this.FPinInHandle.PinIsChanged || this.FMixerHandle == 0)
            {
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                this.FHandle = Convert.ToInt32(dhandle);

                if (this.FHandle == 0)
                {
                    this.FMixerHandle = 0;
                }
                else
                {
                    this.FMixerHandle = BassMix.BASS_Mixer_ChannelGetMixer(this.FHandle);
                    this.FMixerInfo = Bass.BASS_ChannelGetInfo(FMixerHandle);
                    this.FChannelInfo = Bass.BASS_ChannelGetInfo(this.FHandle);
                    
                }
                this.FPinOutHandle.SetValue(0, dhandle);
            }

            if ((oldmixhandle != this.FMixerHandle && this.FMixerHandle != 0) || (this.FPinInLevel.PinIsChanged && this.FMixerHandle != 0))
            {
                UpdateMatrix();
            }
        }
        #endregion

        private void UpdateMatrix()
        {
            
            float[,] matrix = new float[this.FMixerInfo.chans, this.FChannelInfo.chans];
            //BassMix.BASS_Mixer_ChannelGetMatrix(this.FHandle, matrix);
            int idx = 0;

            
            for (int i = 0; i < this.FMixerInfo.chans; i++)
            {
                for (int j = 0; j < this.FChannelInfo.chans; j++)
                {
                    double level;
                    this.FPinInLevel.GetValue(idx, out level);
                    matrix[i, j] = (float)level;

                    idx++;
                    if (idx == this.FPinInLevel.SliceCount)
                    {
                        idx = 0;
                    }
                }
            }

            


            BassMix.BASS_Mixer_ChannelSetMatrix(this.FHandle, matrix);
        }
    }
}
