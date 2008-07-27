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


        protected override void OnPluginHostSet()
        {
            
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
                    double fft;
                    this.FPinInAttribute.GetValue(0, out fft);
                    int ifft = Convert.ToInt32(Math.Round(fft));
                    return ifft * 2;
                }
                else
                {
                    return -1;
                }
            }
        }

        #region Get Data Type
        protected override int DataType
        {
            get 
            {
                double fft;
                this.FPinInAttribute.GetValue(0, out fft);

                int ifft = Convert.ToInt32(Math.Round(fft));

                switch (ifft)
                {
                    case 256:
                        return (int)BASSData.BASS_DATA_FFT256 | (int)BASSData.BASS_DATA_FFT_INDIVIDUAL;
                    case 512:
                        return (int)BASSData.BASS_DATA_FFT512 | (int)BASSData.BASS_DATA_FFT_INDIVIDUAL;
                    case 1024:
                        return (int)BASSData.BASS_DATA_FFT1024 | (int)BASSData.BASS_DATA_FFT_INDIVIDUAL;
                    case 2048:
                        return (int)BASSData.BASS_DATA_FFT2048 | (int)BASSData.BASS_DATA_FFT_INDIVIDUAL;
                    case 4096:
                        return (int)BASSData.BASS_DATA_FFT4096 | (int)BASSData.BASS_DATA_FFT_INDIVIDUAL;
                    case 8192:
                        return (int)BASSData.BASS_DATA_FFT8192 | (int)BASSData.BASS_DATA_FFT_INDIVIDUAL;
                    default:
                        //Only allowed values for fft
                        return -1;
                }
            }
        }
        #endregion

        #region Error Message
        protected override string ErrorMsg
        {
            get { return "Length must be: 256,512,1024,2048,4096,8192"; }
        }
        #endregion
    }
}
