using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace BassSound.Streams
{
    [Obsolete("Will undergo change on standard File streams")]
    internal class BassFileStreamNode : IDisposable
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
                Info.Help = "Bass API File Stream Node";
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
        private IValueIn FPinInPaused;
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
            this.FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            this.FHost.CreateValueInput("Pause", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPaused);
            this.FPinInPaused.SetSubType(0.0, 1.0, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Do Seek", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoSeek);
            this.FPinInDoSeek.SetSubType(0, 1, 0, 0, true, false, true);

            this.FHost.CreateValueInput("Seek Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueInput("Pitch", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPitch);
            this.FPinInPitch.SetSubType(-60, 60, 0, 0, false, false, false);

            this.FHost.CreateValueInput("Tempo", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInTempo);
            this.FPinInTempo.SetSubType(-95, 5000, 0, 0, false, false, false);

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

                Bass.BASS_ChannelPlay(this.FHandle, true);
            }
            #endregion

            #region Pause
            if (this.FPinInPaused.PinIsChanged)
            {
                double dpause;
                this.FPinInPaused.GetValue(0, out dpause);
                if (dpause == 1)
                {
                    Bass.BASS_ChannelPause(this.FHandle);
                }
                else
                {
                    Bass.BASS_ChannelPlay(this.FHandle, false);
                }
            }
            #endregion

            #region Position
            if (this.FPinInPosition.PinIsChanged)
            {
                double position;
                this.FPinInPosition.GetValue(0, out position);
                Bass.BASS_ChannelSetPosition(this.FHandle, (float)position);
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

        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
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
