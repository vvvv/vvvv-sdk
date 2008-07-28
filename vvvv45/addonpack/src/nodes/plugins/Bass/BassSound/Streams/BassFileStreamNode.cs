using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace BassSound.Streams
{
    public class BassFileStreamNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "FileStream";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Bass API WDM File Stream Node";
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

        private IValueIn FPinInPlay;
        private IStringIn FPinInFilename;
        private IValueIn FPInInLoop;
        private IValueIn FPinInDoSeek;
        private IValueIn FPinInPosition;
        private IValueIn FPinInPitch;
        private IValueIn FPinInTempo;

        private IValueOut FPinOutHandle;
        private IValueOut FPinOutCurrentPosition;
        private IValueOut FPinOutLength;

        private int FHandle;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            IntPtr ptr = IntPtr.Zero;
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_SPEAKERS, ptr, null);

            //Input Pins
            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Loop", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPInInLoop);
            this.FPInInLoop.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Do Seek", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoSeek);
            this.FPinInDoSeek.SetSubType(0, 1, 0, 0, true, false, true);

            this.FHost.CreateValueInput("Seek Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueInput("Pitch", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPitch);
            this.FPinInPitch.SetSubType(-60, 60, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Tempo", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInTempo);
            this.FPinInTempo.SetSubType(-95, 5000, 0, 0, false, false, false);

            this.FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            //Output Pins
            this.FHost.CreateValueOutput("Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateValueOutput("CurrentPosition", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutCurrentPosition);
            this.FPinOutCurrentPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueOutput("Length", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutLength);
            this.FPinOutLength.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);
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
            bool updateloop = false;

            #region File Change
            if (this.FPinInFilename.PinIsChanged)
            {
                string file;
                this.FPinInFilename.GetString(0, out file);

                Bass.BASS_StreamFree(this.FHandle);
                this.FHandle = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);

                //Creates a tempo channel
                this.FHandle = BassFx.BASS_FX_TempoCreate(this.FHandle, BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_SAMPLE_FLOAT);
             
                this.FPinOutHandle.SetValue(0, this.FHandle);
                long len = Bass.BASS_ChannelGetLength(this.FHandle);
                this.FPinOutLength.SetValue(0, Bass.BASS_ChannelBytes2Seconds(this.FHandle, len));

                updateplay = true;
                updateloop = true;
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
                    Bass.BASS_ChannelSetPosition(this.FHandle, (float)position);
                }
            }

            if (Bass.BASS_ChannelIsActive(this.FHandle) > 0)
            {
                long pos = Bass.BASS_ChannelGetPosition(this.FHandle);
                double seconds = Bass.BASS_ChannelBytes2Seconds(this.FHandle, pos);
                this.FPinOutCurrentPosition.SetValue(0, seconds);
            }
            #endregion

            #region Tempo and Pitch
            if (this.FPinInPitch.PinIsChanged || this.FPinInTempo.PinIsChanged)
            {
                if (this.FHandle != 1)
                {
                    double pitch, tempo;
                    this.FPinInPitch.GetValue(0, out pitch);
                    this.FPinInTempo.GetValue(0, out tempo);

                    Bass.BASS_ChannelSetAttribute(this.FHandle, BASSAttribute.BASS_ATTRIB_TEMPO, (float)tempo);
                    Bass.BASS_ChannelSetAttribute(this.FHandle, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)pitch);
                }
            }
            #endregion

            #region Update Play/Pause
            if (updateplay || this.FPinInPlay.PinIsChanged)
            {
                if (this.FHandle != 0)
                {
                    double doplay;
                    this.FPinInPlay.GetValue(0, out doplay);
                    if (doplay == 1)
                    {
                        Bass.BASS_ChannelPlay(this.FHandle, false);
                    }
                    else
                    {
                        Bass.BASS_ChannelPause(this.FHandle);
                    }
                }
            }
            #endregion

            #region Update Looping
            if (updateloop || this.FPInInLoop.PinIsChanged)
            {
                if (this.FHandle != 0)
                {
                    double doloop;
                    this.FPInInLoop.GetValue(0, out doloop);
                    if (doloop == 1)
                    {
                        Bass.BASS_ChannelFlags(this.FHandle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                    }
                    else
                    {
                        Bass.BASS_ChannelFlags(this.FHandle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);              
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
            Bass.BASS_ChannelStop(this.FHandle);
            Bass.BASS_StreamFree(this.FHandle);
        }
        #endregion
    }
}
