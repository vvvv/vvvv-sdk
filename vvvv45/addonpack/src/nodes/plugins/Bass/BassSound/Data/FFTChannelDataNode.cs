using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using VVVV.PluginInterfaces.V1;

namespace vvvv.Nodes
{
    /// <summary>
    /// Retrieves FFT data from a channel
    /// </summary>
    public class FFTChannelDataNode : AbstractChannelData, IPlugin
    {
        #region Plugin Information
        public static new IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "FFT";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Get FFT data from a channel";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,Analysis,Spectrum";

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

        private IValueIn FPinInInvidivual;
        private IValueIn FPinInHanning;

        protected override void OnPluginHostSet()
        {     
            this.FHost.CreateValueInput("Individual", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInInvidivual);
            this.FPinInInvidivual.SetSubType(0, 1, 1, 1, false, true, true);
         
            this.FHost.CreateValueInput("Windowed", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHanning);
            this.FPinInHanning.SetSubType(0, 1, 1, 1, false, true, true);
        
        }

        protected override string FAttributeIn
        {
            get { return "FFTSize"; }
        }

        protected override int DataLength
        {
            get 
            {
                if (this.DataType != -1)
                {
                    BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(this.FChannel.BassHandle.Value);
                    double fft;
                    this.FPinInAttribute.GetValue(0, out fft);
                    int ifft = Convert.ToInt32(Math.Round(fft));

                    if (!Individual || info.chans == 1)
                    {
                        ifft = ifft / 2;
                    }
                    return ifft;
                }
                else
                {
                    return -1;
                }
            }
        }

        private bool Individual
        {
            get
            {
                double individual;
                this.FPinInInvidivual.GetValue(0, out individual);
                return individual >= 0.5;
            }
        }

        #region Get Data Type
        protected override int DataType
        {
            get 
            {
                double fft, hanning;
                this.FPinInAttribute.GetValue(0, out fft);

                int ifft = Convert.ToInt32(Math.Round(fft));

                BASSData flag;

                switch (ifft)
                {
                    case 256:
                        flag = BASSData.BASS_DATA_FFT256;
                        break;
                    case 512:
                        flag = BASSData.BASS_DATA_FFT512;
                        break;
                    case 1024:
                        flag = BASSData.BASS_DATA_FFT1024;
                        break;
                    case 2048:
                        flag = BASSData.BASS_DATA_FFT2048;
                        break;
                    case 4096:
                        flag = BASSData.BASS_DATA_FFT4096;
                        break;
                    case 8192:
                        flag = BASSData.BASS_DATA_FFT8192;
                        break;
                    default:
                        //Only allowed values for fft
                        return -1;
                }

                if (this.Individual)
                {
                    flag = flag | BASSData.BASS_DATA_FFT_INDIVIDUAL;
                }

                this.FPinInHanning.GetValue(0, out hanning);

                if (hanning < 0.5)
                {
                    flag = flag | BASSData.BASS_DATA_FFT_NOWINDOW;
                }

                return (int)flag;
            }
        }
        #endregion

        #region Error Message
        protected override string ErrorMsg
        {
            get { return "Length must be: 256,512,1024,2048,4096,8192"; }
        }
        #endregion


        protected override void SetData(float[] samples)
        {
            int len = this.DataLength;

            BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(this.FChannel.BassHandle.Value);
            if (info.chans == 1 || !this.Individual)
            {
                this.FPinOutLeft.SliceCount = len;
                this.FPinOutRight.SliceCount = len;
                for (int i = 0; i < len; i++)
                {
                    this.FPinOutLeft.SetValue(i, (double)samples[i]);
                    //this.FPinOutLeft.SetValue(i, (double)samples[i]);
                    this.FPinOutRight.SetValue(i, (double)samples[i]);
                }
            }
            else
            {
                this.FPinOutLeft.SliceCount = len / 2;
                this.FPinOutRight.SliceCount = len / 2;
                for (int i = 0; i < len; i++)
                {
                    if (i % 2 == 0)
                    {
                        this.FPinOutLeft.SetValue(i / 2, (double)samples[i]);
                    }
                    else
                    {
                        this.FPinOutRight.SetValue(i / 2, (double)samples[i]);
                    }
                }
            }




        }
    }
}
