using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using BassSound.Internals;

namespace vvvv.Nodes
{
    /// <summary>
    /// Abstract class for bass data retrieval.
    /// </summary>
    public abstract class AbstractChannelData : IDisposable
    {
        protected IPluginHost FHost;
        private int FInternalHandle = 0;
        protected int FHandle;

        private IValueIn FPinInHandle;

        protected IValueIn FPinInAttribute;

        private IValueOut FPinOutLeft;
        private IValueOut FPinOutRight;
        private IStringOut FPinOutMsg;
        private IValueOut FPinOutSize;

        private DSP_BufferStream bufferStream;

        protected abstract void OnPluginHostSet();
        protected abstract string FAttributeIn { get; }
        protected abstract int DataType { get; }
        protected abstract int DataLength { get; }
        protected abstract string ErrorMsg { get; }
        
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

            IntPtr ptr = IntPtr.Zero;

            //Input Pins
            this.FHost.CreateValueInput("HandleIn", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateValueInput(this.FAttributeIn, 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInAttribute);

            //Output Pins
            this.FHost.CreateValueOutput("Left", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutLeft);

            this.FHost.CreateValueOutput("Right", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutRight);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutMsg);

            this.FHost.CreateValueOutput("Size",1,null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinOutSize);

            this.OnPluginHostSet();
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
            if (this.FPinInHandle.PinIsChanged || this.FHandle == 0)
            {
                //Just Update the Handle
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                this.FInternalHandle = Convert.ToInt32(Math.Round(dhandle));

                if (ChannelsManager.Exists(this.FInternalHandle))
                {
                    ChannelInfo info = ChannelsManager.GetChannel(this.FInternalHandle);
                    if (info.BassHandle.HasValue)
                    {
                        this.FHandle = info.BassHandle.Value;
                        if (info.IsDecoding)
                        {
                            // create a buffer of the source stream
                            //We can't get it from the main stream otherwise it would interfere with the asio buffering
                            bufferStream = new DSP_BufferStream();
                            bufferStream.ChannelHandle = info.BassHandle.Value; // the stream to copy
                            bufferStream.DSPPriority = -4000;
                            bufferStream.Start();
                            this.FHandle = bufferStream.BufferStream;
                        }
                        else
                        {
                            //If it's not decoding, no problem :)
                            this.FHandle = info.BassHandle.Value;
                        }
                    }
                    else
                    {
                        this.FHandle = 0;
                        this.FInternalHandle = 0;
                    }
                }
                else
                {
                    this.FInternalHandle = 0;
                }



            }

            int len = this.DataLength;

            this.FPinOutSize.SetValue(0, len);

            if (len != -1)
            {
                //We get float, so length is divided by 4
                float[] samples = new float[len];
                int val = Bass.BASS_ChannelGetData(this.FHandle, samples,this.DataType);

                this.FPinOutLeft.SliceCount = len / 2;
                this.FPinOutRight.SliceCount = len / 2;


                //Note: Change that to make sure it Goes with any channel soundtrack.
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
            bufferStream.Stop();
            bufferStream.Dispose();
            //Bass.BASS_ChannelStop(this.FHandle);
        }

        #endregion
    }
}
