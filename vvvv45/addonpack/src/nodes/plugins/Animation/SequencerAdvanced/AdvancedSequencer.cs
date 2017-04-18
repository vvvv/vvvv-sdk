using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System.Threading;
using System.Windows.Forms;
using VVVV.Lib;
using System.IO;

namespace VVVV.Nodes
{
    
    public class MidiSequencerNode : IPlugin, IDisposable 
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Sequencer";							//use CamelCaps and no spaces
                Info.Category = "Animation";						//try to use an existing one
                Info.Version = "Advanced";						//versions are optional. leave blank if not needed
                Info.Help = "Sequencer with track system, and direct midi clock bindings.";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "midi,record,playback";

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

        private IStringIn FPinInTrackId;
        private IValueIn FPinInInput;
        private IValueIn FPinInBufferLength;
        private IValueIn FPinInInterpolate;
        private IValueIn FPinInInterMode;
        private IValueIn FPinInPlay;
        private IValueIn FPinInRecord;
        private IValueIn FPinInKeepPosition;
        private IValueIn FPinInSeekPos;
        private IValueIn FPinInDoSeek;
        private IValueIn FPinInReset;
        private IStringIn FpinInSavePath;
        private IValueIn FPinInSave;
        private IValueIn FPinInLoad;


        private IValueConfig FPinInManualTiming;
        private IEnumIn FPinInDevice;
        private IValueIn FPinInTime;
        private InputDevice FClock = null;


        private IStringOut FPinOutTrackId;
        private IValueOut FPinOutput;
        private IValueOut FPinOutPosition;
        private IValueOut FPinOutBufferLen;
        private IValueOut FPinOutTicks;
        private IStringOut FPinOutUnusedTracks;

        private List<string> FActiveTracks = new List<string>();
        private TrackDictionnary FTracks = new TrackDictionnary();
        private int FTicks = 0;

        private bool FirstFrame = true;

        private List<double> FPrevious = new List<double>();
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            List<string> devices = new List<string>();
            
            for(int i = 0; i < InputDevice.DeviceCount; i++)
            {
                devices.Add(InputDevice.GetDeviceCapabilities(i).name);
            }

            this.FHost.UpdateEnum("Clock Input Devices", "VVVV Internal Clock", devices.ToArray());
   
            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueInput("Record", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInRecord);
            this.FPinInRecord.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueInput("Keep Position", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInKeepPosition);
            this.FPinInKeepPosition.SetSubType(0, 1, 1, 0, false, true, false);
   
            this.FHost.CreateStringInput("Track Id", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInTrackId);
            this.FPinInTrackId.SetSubType("track", false);  
                    
            this.FHost.CreateValueInput("Input", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInInput);
            this.FPinInInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);           
            
            this.FHost.CreateValueInput("Buffer Length", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBufferLength);
            this.FPinInBufferLength.SetSubType(0, double.MaxValue, 0.01, 4.0, false, false, false);

            this.FHost.CreateValueInput("Interpolate", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInInterpolate);
            this.FPinInInterpolate.SetSubType(0, 1, 1, 0, false, true, false);

         
            this.FHost.CreateValueInput("Loop Interpolation", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out this.FPinInInterMode);
            this.FPinInInterMode.SetSubType(0, 1, 1, 0, false, true, false);
               
            this.FHost.CreateValueInput("Seek Position", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInSeekPos);
            this.FPinInSeekPos.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Do Seek", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInDoSeek);
            this.FPinInDoSeek.SetSubType(0, 1, 1, 0, true, false, false);
                 
            this.FHost.CreateValueInput("Clear", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, false);

            //string nodepath;
            //this.FHost.GetHostPath(out nodepath);
            //this.FHost.get
            
            this.FHost.CreateStringInput("Path", TSliceMode.Single, TPinVisibility.True, out this.FpinInSavePath);
            this.FpinInSavePath.SetSubType("", false);
   
            this.FHost.CreateValueInput("Load", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInLoad);
            this.FPinInLoad.SetSubType(0, 1, 1, 0, true, false, false);
    
            this.FHost.CreateValueInput("Save", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSave);
            this.FPinInSave.SetSubType(0, 1, 1, 0, true, false, false);
           
            this.FHost.CreateValueConfig("Manual Clock", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInManualTiming);
            this.FPinInManualTiming.SetSubType(0, 1, 1,1, false, true, false);


            
            this.FHost.CreateStringOutput("Track Id", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutTrackId);
            this.FPinOutTrackId.SetSubType("track", false);

            this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
 
            this.FHost.CreateValueOutput("Position", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPosition);
            this.FPinOutPosition.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
        
            this.FHost.CreateValueOutput("Buffer Length", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBufferLen);
            this.FPinOutBufferLen.SetSubType(0, double.MaxValue, 0.01, 0.0, false, false, false);

            this.FHost.CreateStringOutput("Unused Tracks", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutUnusedTracks);
            this.FPinOutUnusedTracks.SetSubType("track", false);
        
            this.FHost.CreateValueOutput("Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutTicks);
            this.FPinOutTicks.SetSubType(0, double.MaxValue,0.01, 0, false, false, false);

            Configurate(this.FPinInManualTiming);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            if (Input == this.FPinInManualTiming)
            {
                try
                {
                    double dblmanual;
                    this.FPinInManualTiming.GetValue(0, out dblmanual);

                    if (dblmanual < 0.5)
                    {
                        if (this.FPinInTime != null)
                        {
                            this.FHost.DeletePin(this.FPinInTime);
                            this.FPinInTime = null;
                        }

                        this.FHost.CreateEnumInput("Clock", TSliceMode.Single, TPinVisibility.True, out this.FPinInDevice);
                        this.FPinInDevice.SetSubType("Clock Input Devices");
                    }
                    else
                    {
                        if (this.FPinInDevice != null)
                        {
                            this.FHost.DeletePin(this.FPinInDevice);
                            this.FPinInDevice = null;
                        }

                        this.FHost.CreateValueInput("Time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInTime);
                        this.FPinInTime.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
                    }
                }
                catch (Exception ex)
                {
                    this.FHost.Log(TLogType.Error, ex.Message);
                }
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            double currentposition = this.GetCurrentPosition();

            bool clear = false;
            for (int i = 0; i < this.FPinInReset.SliceCount; i++)
            {
                double dblreset;//,dblseek;
                this.FPinInReset.GetValue(i, out dblreset);
                //this.FPinInDoSeek.GetValue(i, out dblseek);
                if (dblreset >= 0.5)// || dblseek >= 0.5)
                {
                    clear = true;
                }
            }

            double dblload;
            this.FPinInLoad.GetValue(0, out dblload);

            #region Track Data
            //First we update our tracks data
            if (this.FPinInTrackId.PinIsChanged
                || this.FPinInPlay.PinIsChanged
                || this.FPinInRecord.PinIsChanged
                || this.FPinInBufferLength.PinIsChanged
                || this.FPinInReset.PinIsChanged
                || clear
                || dblload >= 0.5)
            {
                if (dblload >= 0.5)
                {
                    this.FActiveTracks.Clear();
                    this.FTracks.Clear();

                    string path;
                    this.FpinInSavePath.GetString(0, out path);
                    path = path == null ? String.Empty : path;

                    if (Directory.Exists(path))
                    {
                        foreach (string f in Directory.GetFiles(path))
                        {
                            SeqTrack s = TrackSerializer.LoadTrack(f, 1.0);
                            if (s != null)
                            {
                                this.FTracks.Add(s.Id, s);
                            }
                        }
                    }
                }


                this.FActiveTracks.Clear();
                
                for (int i = 0; i < this.FPinInTrackId.SliceCount; i++)
                {
                    string id;
                    this.FPinInTrackId.GetString(i, out id);


                    //Keep a record of active tracks
                    this.FActiveTracks.Add(id);

                    SeqTrack track;
                    if (!this.FTracks.ContainsKey(id))
                    {
                        //Create new track
                        track = new SeqTrack();
                        track.Id = id;
                        this.FTracks.Add(id, track);
                    }
                    else
                    {
                        track = this.FTracks[id];
                    }
                    //Update slice index if it has changed;
                    track.TrackIndex = i;
                    

                    //Update Play/Record/Buffer
                    double dblplay, dblrecord, dblbuffer,dblclear,dblkeep;

                    this.FPinInPlay.GetValue(i, out dblplay);
                    this.FPinInRecord.GetValue(i, out dblrecord);
                    this.FPinInBufferLength.GetValue(i, out dblbuffer);
                    this.FPinInReset.GetValue(i, out dblclear);
                    this.FPinInKeepPosition.GetValue(i, out dblkeep);

                    dblbuffer = Math.Max(dblbuffer, 0.1);

                    if (dblclear >= 0.5)
                    {
                        track.Clear();
                    }


                    bool resettime = false;
                    if (dblrecord >= 0.5 && track.Play)
                    {
                        track.StartRecording(currentposition);
                    }
                    else
                    {
                        resettime = track.StopRecording(currentposition, dblkeep > 0.5);
                    }

                    if (dblplay >= 0.5)
                    {
                        
                        track.StartPlay(currentposition, resettime);
                    }



                    track.Play = dblplay >= 0.5;
                    track.BufferLength = dblbuffer;
                }
            }
            #endregion

            #region Save
            double dblsave;
            this.FPinInSave.GetValue(0, out dblsave);
            if (dblsave >= 0.5)
            {
                string path;
                this.FpinInSavePath.GetString(0, out path);
                path = path == null ? String.Empty : path;

                if (Directory.Exists(path))
                {
                    foreach (SeqTrack track in this.FTracks.Values)
                    {
                        try
                        {
                            TrackSerializer.SaveTrack(path, track);
                        }
                        catch (Exception ex)
                        {
                            this.FHost.Log(TLogType.Error, ex.Message);
                        }
                    }
                }
            }
            #endregion

            #region Output Results
            this.FPinOutput.SliceCount = this.FActiveTracks.Count;
            this.FPinOutTrackId.SliceCount = this.FActiveTracks.Count;
            this.FPinOutBufferLen.SliceCount = this.FActiveTracks.Count;
            this.FPinOutPosition.SliceCount = this.FActiveTracks.Count;

            for (int i = 0; i < this.FActiveTracks.Count; i++)
            {
                SeqTrack track = this.FTracks[this.FActiveTracks[i]];
                this.FPinOutTrackId.SetString(i, this.FActiveTracks[i]);

                double dblinput;
                this.FPinInInput.GetValue(track.TrackIndex, out dblinput);

                double dblinter;
                this.FPinInInterpolate.GetValue(track.TrackIndex, out dblinter);

                double dblloop;
                this.FPinInInterMode.GetValue(track.TrackIndex, out dblloop);

                if (track.Record)
                {
                    double pos = track.RecordValue(currentposition, dblinput);
                    this.FPinOutput.SetValue(i, dblinput);
                    this.FPinOutPosition.SetValue(i, pos);
                }
                else
                {
                    double seekpos, dbldoseek;

                    this.FPinInDoSeek.GetValue(track.TrackIndex, out dbldoseek);
                    this.FPinInSeekPos.GetValue(track.TrackIndex, out seekpos);

                    if (dbldoseek >= 0.5)
                    {
                        track.DoSeek(currentposition - seekpos);
                    }

                    double pos = track.BufferPosition ;
                    if (track.Play)
                    {
                        if (!track.IsEmpty)
                        {
                            double val = track.Getvalue(currentposition,dblinter >= 0.5,dblloop >= 0.5, out pos);
                            this.FPinOutput.SetValue(i, val);
                        }
                        else
                        {
                            this.FPinOutput.SetValue(i, dblinput);
                        }
                    }
                    else
                    {
                        //this.FPinOutput.SetValue(i, dblinput);
                        
                        /*
                        if (track.TrackIndex < this.FPrevious.Count)
                        {
                            if (this.FPrevious[track.TrackIndex] != dblinput)
                            {
                                this.FPinOutput.SetValue(i, dblinput);
                            }
                        }*/
                        
                        if (FirstFrame)
                        {
                            this.FPinOutput.SetValue(i, dblinput);
                        }
                    }

                    if (track.TrackIndex < this.FPrevious.Count)
                    {
                        if (this.FPrevious[track.TrackIndex] != dblinput)
                        {
                            this.FPinOutput.SetValue(i, dblinput);
                        }
                    }

                    this.FPinOutPosition.SetValue(i, pos);
                }
                this.FPinOutBufferLen.SetValue(i, track.RealBufferLength);
            }

            List<string> unused = new List<string>();
            foreach (string id in this.FTracks.Keys)
            {
                if (!this.FActiveTracks.Contains(id))
                {
                    unused.Add(id);
                }
            }

            this.FPinOutUnusedTracks.SliceCount = unused.Count;
            for (int i = 0; i < unused.Count; i++) { this.FPinOutUnusedTracks.SetString(i, unused[i]); }

            this.FPinOutTicks.SetValue(0, currentposition);
            #endregion

            this.SavePreviousData();

            this.FirstFrame = false;

        }
        #endregion

        #region Save Previous
        private void SavePreviousData()
        {
            this.FPrevious.Clear();
            for (int i = 0; i < this.FPinInInput.SliceCount; i++)
            {
                double dbl;
                this.FPinInInput.GetValue(i, out dbl);
                this.FPrevious.Add(dbl);
            }
        }
        #endregion

        #region GetCurrentPosition
        private double GetCurrentPosition()
        {
            double currentposition;
            if (this.FPinInDevice != null)
            {
                currentposition = Convert.ToDouble(this.FTicks) / 24.0;
                if (this.FPinInDevice.PinIsChanged)
                {
                    int idx;
                    this.FPinInDevice.GetOrd(0, out idx);

                    if (this.FClock != null)
                    {
                        this.FClock.SysRealtimeMessageReceived -= SysRealtimeMessageReceived;
                        this.FClock.StopRecording();
                    }

                    this.FTicks = 0;

                    this.FClock = new InputDevice(idx);
                    this.FClock.SysRealtimeMessageReceived += SysRealtimeMessageReceived;
                    this.FClock.StartRecording();
                }
            }
            else
            {
                this.FPinInTime.GetValue(0, out currentposition);
            }

            return currentposition;
        }
        #endregion

        #region Respond to midi events
        private void SysRealtimeMessageReceived(object sender, SysRealtimeMessageEventArgs e)
        {
            if (e.Message.SysRealtimeType == SysRealtimeType.Start)
            {
                this.FTicks = 0;
            }
            if (e.Message.SysRealtimeType == SysRealtimeType.Reset)
            {
                this.FTicks = 0;
            }
            if (e.Message.SysRealtimeType == SysRealtimeType.Clock)
            {
                this.FTicks++;
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            //Stop listening to midi
            if (this.FClock != null)
            {
                this.FClock.SysRealtimeMessageReceived -= SysRealtimeMessageReceived;
                this.FClock.StopRecording();
            }
        }
        #endregion
    }
        
        
}
