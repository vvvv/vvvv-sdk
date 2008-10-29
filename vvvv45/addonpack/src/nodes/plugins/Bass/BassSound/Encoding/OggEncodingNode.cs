using System;
using System.Collections.Generic;
using System.Text;
using BassSound.Encoding.Internals;
using Un4seen.Bass.Misc;
using VVVV.PluginInterfaces.V1;

namespace BassSound.Encoding
{
    public class OggEncodingNode : AbstractEncoderNode<EncoderOGG>, IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "OGGEncoder";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Ogg encoder";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Encoding";

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

        private IValueIn FPinInBitRate;
        private IValueIn FPinInMinBitRate;
        private IValueIn FPinInMaxBitRate;
        private IValueIn FPinInSampleRate;
        private IValueIn FPinInQualityMode;

        private EncoderOGG FOGG;


        #region Get Encoder
        protected override EncoderOGG GetEncoder(int handle, out string msg)
        {
            double minrate, maxrate, bitrate, samplerate, quality;
            
            this.FPinInBitRate.GetValue(0, out bitrate);
            this.FPinInMaxBitRate.GetValue(0, out maxrate);
            this.FPinInMaxBitRate.GetValue(0, out minrate);
            this.FPinInQualityMode.GetValue(0, out quality);
            this.FPinInSampleRate.GetValue(0, out samplerate);

            EncoderOGG ogg = new EncoderOGG(handle);
            ogg.InputFile = null;
            ogg.OutputFile = this.FFilename;
            ogg.EncoderDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\plugins";
            ogg.OGG_Bitrate = Convert.ToInt32(bitrate);
            ogg.OGG_MinBitrate = Convert.ToInt32(minrate);
            ogg.OGG_MaxBitrate = Convert.ToInt32(maxrate);
            ogg.OGG_TargetSampleRate = Convert.ToInt32(samplerate);
            ogg.OGG_UseQualityMode = samplerate >= 0.5;

            if (ogg.EncoderExists)
            {
                this.FOGG = ogg;
                msg = "OK";
                return ogg;
            }
            else
            {
                this.FOGG = null;
                msg = "oggenc.exe not found in " + ogg.EncoderDirectory;
                return null;
            }
        }
        #endregion

        #region On Plugin Host Set
        protected override void OnPluginHostSet()
        {
            //Add aditional pins
            this.FHost.CreateValueInput("BitRate", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInBitRate);
            this.FPinInBitRate.SetSubType(0, 320, 1, 128, false, false, true);

            this.FHost.CreateValueInput("Min BitRate", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMinBitRate);
            this.FPinInMinBitRate.SetSubType(0, 320, 1, 128, false, false, true);

            this.FHost.CreateValueInput("Max BitRate", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMaxBitRate);
            this.FPinInMaxBitRate.SetSubType(0, 320, 1, 128, false, false, true);

            this.FHost.CreateValueInput("Sample Rate", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSampleRate);
            this.FPinInSampleRate.SetSubType(0, 192000, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Quality Mode", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInQualityMode);
            this.FPinInQualityMode.SetSubType(0, 1, 1, 0, false, true, true);
        }
        #endregion

    }
}
