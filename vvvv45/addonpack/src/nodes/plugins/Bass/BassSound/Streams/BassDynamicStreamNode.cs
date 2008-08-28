using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using Un4seen.Bass;

namespace BassSound.Streams
{
    public class BassDynamicStreamNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "DynamicStream";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Dynamic Stream for Bass, set the buffer";
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
        private ChannelsManager manager;
        private DynamicChannelInfo FChannelInfo = new DynamicChannelInfo();
        private Dictionary<int, float> FOriginalIndices = new Dictionary<int, float>();

        private IValueIn FPinCfgIsDecoding;
        private IValueIn FPinInPlay;
        private IValueIn FPinInBuffer;
        private IValueIn FPinInReset;

        private IValueIn FPinInIndices;
        private IValueIn FPinInDoWrite;
        private IValueIn FPinInData;
        private IValueIn FPinInRestore;

        private IValueOut FPinOutHandle;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.manager = ChannelsManager.GetInstance();

            //Config Pins
            this.FHost.CreateValueInput("Is Decoding", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinCfgIsDecoding);
            this.FPinCfgIsDecoding.SetSubType(0, 1, 1, 0, false, true, true);

            //Input Pins
            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Buffer", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBuffer);
            this.FPinInBuffer.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);


            this.FHost.CreateValueInput("Data", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInData);
            this.FPinInData.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInIndices);
            this.FPinInIndices.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Do Write", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoWrite);
            this.FPinInDoWrite.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueInput("Restore", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRestore);
            this.FPinInRestore.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueInput("Reset", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, true);

            //Output
            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);
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
            bool updateplay = false;
            bool reset = false;

            reset = this.FPinCfgIsDecoding.PinIsChanged;
            if (!reset)
            {
                if (this.FPinInReset.PinIsChanged)
                {
                    double dblreset;
                    this.FPinInReset.GetValue(0, out dblreset);
                    reset = dblreset == 1;
                }
            }

            #region Reset the channel
            if (reset)
            {
                DynamicChannelInfo info = new DynamicChannelInfo();

                double dbldec;
                this.FPinCfgIsDecoding.GetValue(0, out dbldec);
                info.IsDecoding = dbldec == 1;
                
                this.manager.CreateChannel(info);
                this.FChannelInfo = info;

                this.FChannelInfo.Buffer = new float[this.FPinInBuffer.SliceCount];
                for (int i = 0; i < this.FPinInBuffer.SliceCount; i++)
                {
                    double d;
                    this.FPinInBuffer.GetValue(i, out d);
                    this.FChannelInfo.Buffer[i] = (float)d;
                }

                this.FOriginalIndices.Clear();
                this.FPinOutHandle.SetValue(0, this.FChannelInfo.InternalHandle);
                updateplay = true;
            }
            #endregion

            #region Write Data
            if (this.FPinInDoWrite.PinIsChanged)
            {
                double dbldowrite;
                this.FPinInDoWrite.GetValue(0, out dbldowrite);

                if (dbldowrite == 1)
                {

                    int len = this.FPinInData.SliceCount;
                    if (this.FPinInIndices.SliceCount < len)
                    {
                        len = this.FPinInIndices.SliceCount;
                    }

                    for (int i = 0; i < len; i++)
                    {
                        double dblindices, dbldata;
                        this.FPinInIndices.GetValue(i, out dblindices);
                        this.FPinInData.GetValue(i, out dbldata);

                        int idx = Convert.ToInt32(dblindices);
                        if (idx < this.FChannelInfo.Buffer.Length)
                        {
                            if (!this.FOriginalIndices.ContainsKey(idx))
                            {
                                this.FOriginalIndices.Add(idx, this.FChannelInfo.Buffer[idx]);
                            }
                            this.FChannelInfo.Buffer[idx] = (float)dbldata;
                        }
                    }
                }
            }
            #endregion

            #region Restore 
            if (this.FPinInRestore.PinIsChanged)
            {
                double dblrestore;
                this.FPinInRestore.GetValue(0, out dblrestore);

                if (dblrestore == 1)
                {
                    //Restore the buffer with orginal values
                    foreach (int i in this.FOriginalIndices.Keys)
                    {
                        this.FChannelInfo.Buffer[i] = this.FOriginalIndices[i];
                    }
                    this.FOriginalIndices.Clear();
                }
            }
            #endregion


            #region Update Play/Pause
            if (this.FPinInPlay.PinIsChanged || updateplay)
            {
                if (this.FChannelInfo.InternalHandle != 0)
                {
                    double doplay;
                    this.FPinInPlay.GetValue(0, out doplay);
                    if (doplay == 1 && this.FPinOutHandle.IsConnected)
                    {
                        this.manager.GetChannel(this.FChannelInfo.InternalHandle).Play = true;
                    }
                    else
                    {
                        this.manager.GetChannel(this.FChannelInfo.InternalHandle).Play = false;
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if (this.FChannelInfo.InternalHandle != 0)
            {
                if (this.FChannelInfo.BassHandle.HasValue)
                {
                    Bass.BASS_ChannelStop(this.FChannelInfo.BassHandle.Value);
                    Bass.BASS_StreamFree(this.FChannelInfo.BassHandle.Value);
                }
                this.manager.Delete(this.FChannelInfo.InternalHandle);
            }
        }

        #endregion
    }
}
