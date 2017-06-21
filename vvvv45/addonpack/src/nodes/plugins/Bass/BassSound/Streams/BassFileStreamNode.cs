using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using BassSound.Internals;
using System.IO;

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
                Info.Help = "Bass API File Stream Node";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "audio, sound";

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

        private IValueIn FPinCfgIsDecoding;

        private IValueIn FPinInPlay;
        private IStringIn FPinInFilename;
        private IValueIn FPInInLoop;
        private IValueIn FPinInStartTime;
        private IValueIn FPinInEndTime;
        private IValueIn FPinInDoSeek;
        private IValueIn FPinInPosition;
        private IValueIn FPinInMono;
        private IValueIn FPinInPitch;
        private IValueIn FPinInTempo;

        private IValueOut FPinOutHandle;
        private IValueOut FPinOutCurrentPosition;
        private IValueOut FPinOutLength;
        private IValueOut FPinOutEnd;

        private bool FEnd = false;
        private bool FConnected = false;
        private FileChannelInfo FChannelInfo = new FileChannelInfo();

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

            this.FHost.CreateValueInput("Loop", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPInInLoop);
            this.FPInInLoop.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Loop Start Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInStartTime);
            this.FPinInStartTime.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
            
            this.FHost.CreateValueInput("Loop End Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEndTime);
            this.FPinInEndTime.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
            
            this.FHost.CreateValueInput("Do Seek", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoSeek);
            this.FPinInDoSeek.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueInput("Seek Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueInput("Mono", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInMono);
            this.FPinInMono.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Pitch", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPitch);
            this.FPinInPitch.SetSubType(-60, 60, 1, 0, false, false, false);

            this.FHost.CreateValueInput("Tempo", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInTempo);
            this.FPinInTempo.SetSubType(-95, 5000, 1, 0, false, false, false);

            this.FHost.CreateStringInput("File Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            //Output Pins
            this.FHost.CreateValueOutput("Handle Out", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateValueOutput("Current Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutCurrentPosition);
            this.FPinOutCurrentPosition.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueOutput("Length", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutLength);
            this.FPinOutLength.SetSubType(0, double.MaxValue, 0, 0.0, false, false, false);

            this.FHost.CreateValueOutput("File End", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutEnd);
            this.FPinOutEnd.SetSubType(0, 1, 1, 0, true, false, true);
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

            if (this.FConnected != this.FPinOutHandle.IsConnected)
            {
                updateplay = true;
                this.FConnected = this.FPinOutHandle.IsConnected;
            }

            #region Reset pins
            if (this.FPinInFilename.PinIsChanged || this.FPinCfgIsDecoding.PinIsChanged || this.FPinInMono.PinIsChanged)
            {
                string file;
                this.FPinInFilename.GetString(0, out file);

                if (File.Exists(file))
                {
                    //Remove the old one
                    if (this.FChannelInfo.InternalHandle != 0)
                    {
                        if (this.FChannelInfo.BassHandle.HasValue)
                        {
                            Bass.BASS_ChannelStop(this.FChannelInfo.BassHandle.Value);
                            Bass.BASS_StreamFree(this.FChannelInfo.BassHandle.Value);
                        }
                        this.manager.Delete(this.FChannelInfo.InternalHandle);
                    }

                    FileChannelInfo info = new FileChannelInfo();
                    info.FileName = file;
                    info.OnStreamEnd += new EventHandler(info_OnStreamEnd);

                    double isdecoding;
                    this.FPinCfgIsDecoding.GetValue(0, out isdecoding);
                    info.IsDecoding = isdecoding ==1;

                    this.manager.CreateChannel(info);
                    this.FChannelInfo = info;

                    this.FPinOutHandle.SetValue(0, this.FChannelInfo.InternalHandle);

                    updateplay = true;
                    updateloop = true;
                }
            }
            #endregion

            #region Update Play/Pause
            if (updateplay || this.FPinInPlay.PinIsChanged)
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

            #region Update Looping
            if (this.FPinInStartTime.PinIsChanged)
            {
            	double start;
                this.FPinInStartTime.GetValue(0, out start);
                this.FChannelInfo.LoopStart = start;
            }
            
            if (this.FPinInEndTime.PinIsChanged)
            {
            	double end;
                this.FPinInEndTime.GetValue(0, out end);
                this.FChannelInfo.LoopEnd = end;
            }
            
            if (updateloop || this.FPInInLoop.PinIsChanged)
            {
                if (this.FChannelInfo.InternalHandle != 0)
                {
                    double doloop;
                    this.FPInInLoop.GetValue(0, out doloop);
                    if (doloop == 1)
                    {
                        this.manager.GetChannel(this.FChannelInfo.InternalHandle).Loop = true;
                    }
                    else
                    {
                        this.manager.GetChannel(this.FChannelInfo.InternalHandle).Loop = false;
                    }
                }
            }
            #endregion

            #region Update Seek position
            if (this.FPinInDoSeek.PinIsChanged && this.FChannelInfo.InternalHandle != 0)
            {
                double doseek;
                this.FPinInDoSeek.GetValue(0, out doseek);
                if (doseek == 1)
                {
                    ChannelInfo info = this.manager.GetChannel(this.FChannelInfo.InternalHandle);
                    if (info.BassHandle.HasValue)
                    {
                        double position;
                        this.FPinInPosition.GetValue(0, out position);
                        Bass.BASS_ChannelSetPosition(info.BassHandle.Value, (float)position);
                    }
                }
            }
            #endregion

            #region Update Current Position/Length
            if (this.FChannelInfo.InternalHandle != 0)
            {
                ChannelInfo info = this.manager.GetChannel(this.FChannelInfo.InternalHandle);
                if (info.BassHandle.HasValue)
                {
                    long pos = Bass.BASS_ChannelGetPosition(info.BassHandle.Value);
                    double seconds = Bass.BASS_ChannelBytes2Seconds(info.BassHandle.Value, pos);
                    this.FPinOutCurrentPosition.SetValue(0, seconds);
                    this.FPinOutLength.SetValue(0, info.Length);
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

            #region End
            if (this.FEnd)
            {
                this.FPinOutEnd.SetValue(0, 1);
                this.FEnd = false;
            }
            else
            {
                this.FPinOutEnd.SetValue(0, 0);
            }
            #endregion

        }
        #endregion

        #region On stream End
        private void info_OnStreamEnd(object sender, EventArgs e)
        {
            this.FEnd = true;
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
