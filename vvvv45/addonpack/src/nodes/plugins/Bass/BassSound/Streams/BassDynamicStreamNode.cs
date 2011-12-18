using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

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
                Info.Author = "vux";
                Info.Tags = "Audio,Sound";

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
        private IValueIn FPinInLoopStartPos;
        private IValueIn FPinInLoopEndPos;
        private IValueFastIn FPinInBuffer;
        private IValueIn FPinInReset;

        private IValueFastIn FPinInIndices;
        private IValueIn FPinInDoWrite;
        private IValueFastIn FPinInData;
        private IValueIn FPinInRestore;
        private IValueIn FPinInPitch;
        private IValueIn FPinInTempo;

        private IValueOut FPinOutHandle;
        private IValueOut FPinOutCurrentPosition;
        private IValueOut FPinOutBufferPosition;

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

            this.FHost.CreateValueInput("Buffer Start Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInLoopStartPos);
            this.FPinInLoopStartPos.SetSubType(0, double.MaxValue, 1, 0, false, false,true);

            this.FHost.CreateValueInput("Buffer End Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInLoopEndPos);
            this.FPinInLoopEndPos.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Pitch", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPitch);
            this.FPinInPitch.SetSubType(-60, 60, 1, 0, false, false, false);

            this.FHost.CreateValueInput("Tempo", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInTempo);
            this.FPinInTempo.SetSubType(-95, 5000, 1, 0, false, false, false);
            
            this.FHost.CreateValueFastInput("Buffer", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBuffer);
            this.FPinInBuffer.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Write Buffer", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueFastInput("Data", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInData);
            this.FPinInData.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueFastInput("Index", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInIndices);
            this.FPinInIndices.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Write Data", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoWrite);
            this.FPinInDoWrite.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueInput("Restore", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRestore);
            this.FPinInRestore.SetSubType(0, 1, 1, 0, true, false, true);



            //Output
            this.FHost.CreateValueOutput("Handle Out", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);
            
            this.FHost.CreateValueOutput("Current Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutCurrentPosition);
            this.FPinOutCurrentPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueOutput("Buffer Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutBufferPosition);
            this.FPinOutBufferPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, true);
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
                double dblreset;
                this.FPinInReset.GetValue(0, out dblreset);
                reset = dblreset >= 0.5;
            }

            if (this.FPinInLoopStartPos.PinIsChanged || this.FPinInLoopEndPos.PinIsChanged)
            {
                this.ProcessStartEnd();
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
                this.ProcessStartEnd();

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
            double doWrite;
            this.FPinInDoWrite.GetValue(0, out doWrite);
            if (doWrite >= 0.5)
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
            #endregion

            #region Restore
            double dblrestore;
            this.FPinInRestore.GetValue(0, out dblrestore);

            if (dblrestore >= 0.5)
            {
                //Restore the buffer with orginal values
                foreach (int i in this.FOriginalIndices.Keys)
                {
                    this.FChannelInfo.Buffer[i] = this.FOriginalIndices[i];
                }
                this.FOriginalIndices.Clear();
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

            #region Update Current Position/Length
            if (this.FChannelInfo.InternalHandle != 0)
            {
                if (this.FChannelInfo.BassHandle.HasValue)
                {
                    int mixerhandle = BassMix.BASS_Mixer_ChannelGetMixer(this.FChannelInfo.BassHandle.Value);
                    long pos;
                    if (mixerhandle != 0)
                    {
                        pos = BassMix.BASS_Mixer_ChannelGetPosition(this.FChannelInfo.BassHandle.Value);
                    }
                    else
                    {
                        pos = Bass.BASS_ChannelGetPosition(this.FChannelInfo.BassHandle.Value);
                    }
                    double seconds = Bass.BASS_ChannelBytes2Seconds(this.FChannelInfo.BassHandle.Value, pos);
                    this.FPinOutCurrentPosition.SetValue(0, seconds);

                    this.FPinOutBufferPosition.SetValue(0, this.FChannelInfo.BufferPosition);
                }
            }
            #endregion

            #region Tempo and Pitch
            if (this.FPinInPitch.PinIsChanged || this.FPinInTempo.PinIsChanged)
            {
                if (this.FChannelInfo.InternalHandle != 0)
                {
                    double pitch, tempo;
                    this.FPinInPitch.GetValue(0, out pitch);
                    this.FPinInTempo.GetValue(0, out tempo);

                    this.FChannelInfo.Pitch = pitch;
                    this.FChannelInfo.Tempo = tempo;
                }
            }
            #endregion
        }
        #endregion

        private void ProcessStartEnd()
        {
            double start,end;
            this.FPinInLoopStartPos.GetValue(0, out start);
            this.FChannelInfo.BufferStart = Convert.ToInt32(start);

            this.FPinInLoopEndPos.GetValue(0, out end);
            this.FChannelInfo.BufferEnd = Convert.ToInt32(end);

        }

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
