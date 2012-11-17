using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using BassSound.Internals;
using Un4seen.Bass.AddOn.Mix;

namespace vvvv.Nodes
{
    /// <summary>
    /// Abstract class for bass data retrieval.
    /// </summary>
    public abstract class AbstractChannelData : IDisposable
    {
        protected IPluginHost FHost;

        private ChannelsManager FManager;
        private bool FMixer = false;
        protected ChannelInfo FChannel;
        protected int FMyBassHandle;

        private IValueIn FPinInHandle;

        protected IValueIn FPinInAttribute;

        protected IValueOut FPinOutLeft;
        protected IValueOut FPinOutRight;
        protected IStringOut FPinOutMsg;
        protected IValueOut FPinOutSize;

        private DSP_BufferStream bufferStream;

        protected abstract void OnPluginHostSet();
        protected abstract string FAttributeIn { get; }
        protected abstract int DataType { get; }
        protected abstract int DataLength { get; }
        protected abstract string ErrorMsg { get; }
        protected abstract void SetData(float[] samples);
        
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "BaseDataDoNotUse";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Abstract class, will crash if you instanciate";
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

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            IntPtr ptr = IntPtr.Zero;

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateValueInput(this.FAttributeIn, 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInAttribute);
            this.OnPluginHostSet();

            //Output Pins
            this.FHost.CreateValueOutput("Left", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutLeft);

            this.FHost.CreateValueOutput("Right", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutRight);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutMsg);

            this.FHost.CreateValueOutput("Size",1,null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinOutSize);

            
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
            if (this.FPinInHandle.PinIsChanged || this.FChannel == null)
            {
                //Just Update the Handle
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                int ihandle = Convert.ToInt32(Math.Round(dhandle));

                if (this.FManager.Exists(ihandle))
                {
                    this.FChannel = this.FManager.GetChannel(ihandle);
                    if (this.FChannel.BassHandle.HasValue)
                    {
                        if (this.FChannel.IsDecoding)
                        {
                            int mixhandle = BassMix.BASS_Mixer_ChannelGetMixer(this.FChannel.BassHandle.Value);

                            if (mixhandle == 0)
                            {
                                // create a buffer of the source stream
                                //We can't get it from the main stream otherwise it would interfere with the asio buffering
                                bufferStream = new DSP_BufferStream();
                                bufferStream.ChannelHandle = this.FChannel.BassHandle.Value; // the stream to copy
                                bufferStream.DSPPriority = -4000;
                                bufferStream.Start();
                                this.FMyBassHandle = bufferStream.BufferStream;
                                this.FMixer = false;
                            }
                            else
                            {
                                //We have a mixer, much better :)
                                this.FMyBassHandle = this.FChannel.BassHandle.Value;
                                this.FMixer = true;
                            }
                        }
                        else
                        {
                            //If it's not decoding, no problem :)
                            this.FMyBassHandle = this.FChannel.BassHandle.Value;
                            this.FMixer = false;
                        }
                    }
                    else
                    {
                        this.FMyBassHandle = 0;
                        this.FChannel = null;
                    }
                }
                else
                {
                    this.FMyBassHandle = 0;
                    this.FChannel = null;
                }



            }

            int len = this.DataLength;

            this.FPinOutSize.SetValue(0, len);

            if (len != -1)
            {
                BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(this.FChannel.BassHandle.Value);

                //We get float, so length is divided by 4
                float[] samples = new float[len];
                int val;

                if (this.FMixer)
                {
                    val = BassMix.BASS_Mixer_ChannelGetData(this.FMyBassHandle, samples, this.DataType);
                }
                else
                {
                    val = Bass.BASS_ChannelGetData(this.FMyBassHandle, samples, this.DataType);
                }
                this.SetData(samples);
                this.FPinOutMsg.SetString(0,"OK");
            }
            else
            {
                this.FPinOutMsg.SetString(0,this.ErrorMsg);
            }
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion


        #region IDisposable Members

        public virtual void Dispose()
        {

            if (bufferStream != null)
            {
                try
                {
                    bufferStream.Stop();
                    bufferStream.Dispose();
                }
                catch
                {

                }
            }
            //Bass.BASS_ChannelStop(this.FHandle);
        }

        #endregion
    }
}
