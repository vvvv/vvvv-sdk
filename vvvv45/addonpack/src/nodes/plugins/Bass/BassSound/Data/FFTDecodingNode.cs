using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using System.IO;
using vvvv.Utils;
using System.Runtime.InteropServices;

namespace VVVV.Nodes
{
    
    public class FFTDecodingNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "FFT";							//use CamelCaps and no spaces
                Info.Category = "Bass";						//try to use an existing one
                Info.Version = "NRT";						//versions are optional. leave blank if not needed
                Info.Help = "Non real time FFT Data";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;

        private IStringIn FPinInPath;
        private IValueIn FPinInSize;
        private IValueIn FPinInWindowed;
        private IValueIn FPinInPosition;
        private IValueIn FPinInInvidivual;

        private IValueOut FPinOutLeft;
        private IValueOut FPinOutRight;
        private IStringOut FPinOutStatus;
        private IValueOut FPinOutPosition;

        private int FFlag;
        private int FHandle;

        private bool FFileValid;
        private bool FFlagValid;

        private bool FIndividual;
        private int FSize;
        private int FNumChans;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            Bass.BASS_Init(-1, 44100, 0, IntPtr.Zero, null);
            BassUtils.LoadPlugins();
            
            //assign host
            this.FHost = Host;

            this.FHost.CreateStringInput("Path", TSliceMode.Single, TPinVisibility.True, out this.FPinInPath);
            this.FPinInPath.SetSubType("", true);
  
            this.FHost.CreateValueInput("FFT Size", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSize);
            this.FPinInSize.SetSubType(0, double.MaxValue, 1, 256, false, false, true);
            
            this.FHost.CreateValueInput("Windowed", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInWindowed);
            this.FPinInWindowed.SetSubType(0, 1, 1, 1, false, true, false);

            this.FHost.CreateValueInput("Individual", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInInvidivual);
            this.FPinInInvidivual.SetSubType(0, 1, 1, 1, false, true, false);

            this.FHost.CreateValueInput("Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            
            this.FHost.CreateValueOutput("FFT Left", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutLeft);
            this.FPinOutLeft.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("FFT Right", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutRight);
            this.FPinOutRight.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutPosition);
            this.FPinOutPosition.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
              
            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);
            this.FPinOutStatus.SetSubType("",false);
        
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool reset = false;

            if (this.FPinInPath.PinIsChanged)
            {
                //Bass.BASS_StreamFree(this.FHandle);

                string path;
                this.FPinInPath.GetString(0, out path);

                if (File.Exists(path))
                {
                    int handle = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
                    if (handle < -1)
                    {
                        this.FHandle = handle;
                        BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(this.FHandle);
                        this.FNumChans = info.chans;
                        this.FFileValid = true;

                    }
                    else
                    {
                        this.FFileValid = false;
                        this.FHandle = -1;
                        this.FPinOutLeft.SliceCount = 0;
                        this.FPinOutRight.SliceCount = 0;
                        this.FPinOutStatus.SetString(0, "Inavlid file format");
                    }
                }
                else
                {
                    this.FFileValid = false;
                    this.FHandle = -1;
                    this.FPinOutLeft.SliceCount = 0;
                    this.FPinOutRight.SliceCount = 0;
                    this.FPinOutStatus.SetString(0, "File does not exist");
                }

                reset = true;
            }

            if (this.FPinInInvidivual.PinIsChanged ||
                this.FPinInSize.PinIsChanged || reset)
            {
                this.FFlag = this.GetDataType() ;
                if (this.FFlag != -1)
                {
                    this.FFlagValid = true;
                }
                else
                {
                    this.FFlagValid = false;
                    this.FPinOutLeft.SliceCount = 0;
                    this.FPinOutRight.SliceCount = 0;
                    this.FPinOutStatus.SetString(0, "FFT must be 256,512,1024,2048,4096 or 8192");
                }
                reset = true;
            }

            if (this.FPinInPosition.PinIsChanged || reset)
            {
                if (this.FFlagValid && this.FFileValid)
                {
                    double position;
                    this.FPinInPosition.GetValue(0, out position);

                    int len = this.FSize;
                    if (!this.FIndividual || this.FNumChans == 1)
                    {
                        len = len / 2;
                    }

                    //byte[] buffer = new byte[len * 4];
                    float[] samples = new float[len];

                    Bass.BASS_ChannelSetPosition(this.FHandle, position);
                    int cnt = Bass.BASS_ChannelGetData(this.FHandle, samples, this.GetDataType());

                    long pos = Bass.BASS_ChannelGetPosition(this.FHandle);
                    double dpos = Bass.BASS_ChannelBytes2Seconds(this.FHandle, pos);

                    //float[] samples = new float[len];

                    //GCHandle handle;
                    //handle = GCHandle.Alloc(samples, GCHandleType.Pinned);
                    //Marshal.Copy(samples, 0, handle.AddrOfPinnedObject(), buffer.Length);
                    //handle.Free();

                    this.FPinOutPosition.SetValue(0, dpos);
                    this.FPinOutStatus.SetString(0, "OK");

                    if (this.FNumChans == 1 || !this.FIndividual)
                    {
                        this.FPinOutLeft.SliceCount = len;
                        this.FPinOutRight.SliceCount = len;
                        for (int i = 0; i < len; i++)
                        {
                            this.FPinOutLeft.SetValue(i, (double)samples[i]);
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
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (this.FHandle < -1)
            {
                Bass.BASS_StreamFree(this.FHandle);
            }
        }
        #endregion

        #region Get Data Type
        protected int GetDataType()
        {

            double fft, hanning, individual;
            this.FPinInSize.GetValue(0, out fft);
            this.FPinInWindowed.GetValue(0, out hanning);
            this.FPinInInvidivual.GetValue(0, out individual);

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

            this.FSize = ifft;

            this.FIndividual = individual >= 0.5;
            if (this.FIndividual)
            {
                flag = flag | BASSData.BASS_DATA_FFT_INDIVIDUAL;
            }

            if (hanning < 0.5)
            {
                flag = flag | BASSData.BASS_DATA_FFT_NOWINDOW;
            }


            return (int)flag;
        }
        #endregion
    }
        
        
}
