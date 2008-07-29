using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using System.IO;
using vvvv.Utils;
using System.Runtime.InteropServices;
using Un4seen.BassAsio;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;

namespace vvvv.Nodes
{
    public class BassAsioFileStreamNode : IPlugin ,IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "FileStream";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "Creates a stream for a file (one handle per channel)";
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

        private IStringIn FPinInFilename;
        private IValueIn FPInInLoop;
        private IValueIn FPinInPlay;
        private IValueIn FPinInDoSeek;
        private IValueIn FPinInPosition;
        private IValueIn FPinInMono;
        private IValueIn FPinInPitch;
        private IValueIn FPinInTempo;

        private IValueOut FPinOutHandle;
        private IValueOut FPinOutChanCount;
        private IValueOut FPinOutCurrentPosition;
        private IValueOut FPinOutLength;

        private int FMainHandle = -1;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //We play this channel trough Asio output, so we choose the device NOSOUND
            Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, null);

            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Loop", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPInInLoop);
            this.FPInInLoop.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Do Seek", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoSeek);
            this.FPinInDoSeek.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueInput("Seek Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType(0, double.MaxValue, 0.01, 0.0, false, false, false);

            this.FHost.CreateValueInput("Mono", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMono);
            this.FPinInMono.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Pitch", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPitch);
            this.FPinInPitch.SetSubType(-60, 60, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Tempo", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInTempo);
            this.FPinInTempo.SetSubType(-95, 5000, 0.01, 0, false, false, false);

            this.FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueOutput("Channel Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutChanCount);
            this.FPinOutChanCount.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueOutput("CurrentPosition", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutCurrentPosition);
            this.FPinOutCurrentPosition.SetSubType(0, double.MaxValue, 0.01, 0.0, false, false, false);

            this.FHost.CreateValueOutput("Length", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutLength);
            this.FPinOutLength.SetSubType(0, double.MaxValue, 0.01, 0.0, false, false, false);
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

        #region IDisposable Members
        public void Dispose()
        {
            BassAsioUtils.DecodingChannels.Remove(this.FMainHandle);
            BassAsioUtils.FreeChannel(this.FMainHandle);
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool updateloop = false;
            bool updateplay = false;

            #region File Change
            if (this.FPinInFilename.PinIsChanged || this.FPinInMono.PinIsChanged)
            {
                BassAsioUtils.FreeChannel(this.FMainHandle);

                string file;
                this.FPinInFilename.GetString(0, out file);

                if (File.Exists(file))
                {
                    double mono;
                    this.FPinInMono.GetValue(0, out mono);

                    if (mono != 1)
                    {
                        this.FMainHandle = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
                        //Create tempo channel
                        this.FMainHandle = BassFx.BASS_FX_TempoCreate(this.FMainHandle, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_SAMPLE_FLOAT);

                    }
                    else
                    {
                        this.FMainHandle = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_SAMPLE_MONO);
                        //Create tempo channel
                        this.FMainHandle = BassFx.BASS_FX_TempoCreate(this.FMainHandle, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_SAMPLE_MONO);
                    }

                    BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(this.FMainHandle);

                    this.FPinOutLength.SliceCount = 1;
                    long total = Bass.BASS_ChannelGetLength(this.FMainHandle);
                    this.FPinOutLength.SetValue(0, Bass.BASS_ChannelBytes2Seconds(this.FMainHandle, total));

                    this.FPinOutChanCount.SliceCount = 1;
                    this.FPinOutChanCount.SetValue(0, info.chans);

                    this.FPinOutHandle.SliceCount = 1;
                    this.FPinOutHandle.SetValue(0, this.FMainHandle);

                    updateloop = true;
                }
                else
                {
                    this.FPinOutHandle.SliceCount = 0;
                    this.FPinOutLength.SetValue(0, 0);
                    this.FPinOutChanCount.SliceCount = 0;
                    this.FMainHandle = -1;
                }

            }
            #endregion

            #region Position
            if (this.FPinInDoSeek.PinIsChanged)
            {
                double doseek;
                this.FPinInDoSeek.GetValue(0, out doseek);

                if (doseek == 1)
                {
                    double position;
                    this.FPinInPosition.GetValue(0, out position);
                    Bass.BASS_ChannelSetPosition(this.FMainHandle, (float)position);
                }
            }

            if (Bass.BASS_ChannelIsActive(this.FMainHandle) > 0)
            {
                long pos = Bass.BASS_ChannelGetPosition(this.FMainHandle);
                double seconds = Bass.BASS_ChannelBytes2Seconds(this.FMainHandle, pos);
                this.FPinOutCurrentPosition.SetValue(0, seconds);
            }
            #endregion

            #region Pitch Tempo
            if (this.FPinInPitch.PinIsChanged || this.FPinInTempo.PinIsChanged)
            {
                if (this.FMainHandle != -1)
                {
                    double pitch, tempo;
                    this.FPinInPitch.GetValue(0, out pitch);
                    this.FPinInTempo.GetValue(0, out tempo);

                    Bass.BASS_ChannelSetAttribute(this.FMainHandle, BASSAttribute.BASS_ATTRIB_TEMPO, (float)tempo);
                    Bass.BASS_ChannelSetAttribute(this.FMainHandle, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)pitch);
                }
            }
            #endregion

            #region Update Looping
            if (updateloop || this.FPInInLoop.PinIsChanged)
            {
                if (this.FMainHandle != 0)
                {
                    double doloop;
                    this.FPInInLoop.GetValue(0, out doloop);
                    if (doloop == 1)
                    {
                        Bass.BASS_ChannelFlags(this.FMainHandle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                    }
                    else
                    {
                        Bass.BASS_ChannelFlags(this.FMainHandle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                    }
                }
            }
            #endregion

            #region Update Play/Pause
            if (updateplay || this.FPinInPlay.PinIsChanged)
            {
                if (this.FMainHandle != 0)
                {
                    double doplay;
                    this.FPinInPlay.GetValue(0, out doplay);
                    //Check if decoding channel attached to a mixer
                    BassAsioUtils.DecodingChannels[this.FMainHandle] = (doplay == 1);
                }
            }
            #endregion

        }
        #endregion

    }
}
