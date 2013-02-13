﻿//Irrklang Plugin http://www.ambiera.com/irrklang/
//irrKlang is free for non-commercial use.
//The commercial version is named 'irrKlang pro' and has pricing schemes ideal for independent developers.
//irrKlang is a high level 2D and 3D cross platform (Windows, Mac OS X, Linux) sound engine and audio library
// which plays WAV, MP3, OGG, FLAC, MOD, XM, IT, S3M and more file formats, and is usable in C++ and
// all .NET languages (C#, VisualBasic.NET, etc). It has all the features known from low level audio libraries 
//as well as lots of useful features like a sophisticated streaming engine, extendable audio reading, 
//single and multithreading modes, 3d audio emulation for low end hardware, a plugin system, 
//multiple rolloff models and more. All this can be accessed via an extremely simple API. 



//Coded by sanch(http://sanchtv.com) with the help of tonfilm , sebastian gregor , mr vux and joreg


#region usings

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Reflection;
using System.IO;


using IrrKlang;
using IrrVec3D = IrrKlang.Vector3D;
using Vector3D = VVVV.Utils.VMath.Vector3D;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Irrklang", Category = "Sound", Version = "Legacy", Help = "Irrklang Gamesound Engine", Tags = "Irrklang, Sound, 3D", AutoEvaluate = true, Author = "sanch,phlegma")]
    #endregion PluginInfo


    public class Irrklang : IPluginEvaluate, IDisposable, ISoundStopEventReceiver
    {

        #region fields & pins

        #region Input Pins

        //File
        [Input("File", DefaultString = "", StringType = StringType.Filename)]
        IDiffSpread<string> FFile;

        //Main Volume
        [Input("MainVolume", DefaultValue = 0.5, IsSingle = true, MinValue = 0.0, MaxValue = 1.0)]
        IDiffSpread<double> FMainVolume;

        // Sound Playback
        string[] FPlayModeArray = { "3D", "2D" };
        [Input("PlayMode", EnumName = "PlayMode")]
        IDiffSpread<EnumEntry> FPlayMode;
        [Input("Play")]
        IDiffSpread<bool> FPlay;
        [Input("Pause")]
        IDiffSpread<bool> FPause;
        [Input("Loop")]
        IDiffSpread<bool> FLoop;
        [Input("Seek Position", DefaultValue = 0.0, MinValue = 0.0)]
        IDiffSpread<float> FSeekPos;
        [Input("Seek", IsBang = true)]
        IDiffSpread<bool> FSeek;
        [Input("Stop All", IsSingle = true, IsBang = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FStopAll;
        [Input("Pause All", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FPauseAll;

        //Sound Control
        [Input("Playback Speed", DefaultValue = 1.0)]
        IDiffSpread<float> FPlaybackSpeed;
        [Input("Volume", DefaultValue = 1.0, MaxValue = 1.0, MinValue = 0.0)]
        IDiffSpread<float> FVolume;
        [Input("Pan", DefaultValue = 0, MaxValue = 1.0, MinValue = -1.0)]
        IDiffSpread<float> FPan;


        // 3D Sound Position
        [Input("Pos", DefaultValue = 0)]
        IDiffSpread<Vector3D> FSoundPosition;
        [Input("Velocity", DefaultValue = 0)]
        IDiffSpread<Vector3D> FSoundVelocity;
        [Input("Minimal Distance", DefaultValue = 0, MinValue = float.MinValue)]
        IDiffSpread<float> FMinDist;
        [Input("Maximal Distance", DefaultValue = 100, MaxValue = float.MaxValue)]
        IDiffSpread<float> FMaxDist;

        // 3D Listener Position
        [Input("View Position", DefaultValue = 0, IsSingle = true)]
        IDiffSpread<Vector3D> FViewPos;
        [Input("View Diriection", DefaultValue = 0, IsSingle = true)]
        IDiffSpread<Vector3D> FViewDir;
        [Input("View Velocity Per Second", DefaultValue = 0, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<Vector3D> FViewVelocity;
        [Input("View Up Vector", DefaultValue = 0, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<Vector3D> FViewUpVector;

        //RollOff
        [Input("Rolloff Factor", DefaultValue = 1.0, MinValue = 0.0, MaxValue = 1.0, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FRollOff;

        //Doppler
        [Input("Doppler Factor", DefaultValue = 2.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDoplerFactor;
        [Input("Doppler Distance Factor", DefaultValue = 4.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDoplerDistanceFactor;

        //Effects
        [Input("Enabel Effects")]
        ISpread<bool> FEnableEffekts;

        //Chorus
        [Input("Enable Chorus", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableChorus;
        [Input("Chorus DryWetMix", DefaultValue = 50.0, MaxValue=100.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FChoruspWetDryMix;
        [Input("Chorus Depth", DefaultValue = 100, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FChoruspDepth;
        [Input("Chorus Feedback", DefaultValue = 10, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FChoruspFeedback;
        [Input("Chorus Frequency", DefaultValue = 10, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FChorusFrequency;
        [Input("Chorus SinusWaveForm", DefaultValue = 0,AsInt=true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FChorusSinusWaveForm;
        [Input("Chorus Delay", DefaultValue = 100, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FChorusDelay;
        [Input("Chorus Phase", DefaultValue = 0, AsInt = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FChorusPhase;

        //Compressor
        [Input("Enable Compressor", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableComp;
        [Input("Comp Gain", DefaultValue = 6.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FCompGain;
        [Input("Comp Attack", DefaultValue = 10.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FCompAttack;
        [Input("Comp Release", DefaultValue = 50.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FCompRelease;
        [Input("Comp Threshold", DefaultValue = -10.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FCompThreshold;
        [Input("Comp Ratio", DefaultValue = 2, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FCompRatio;
        [Input("Comp Predelay", DefaultValue = 2, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FCompPredelay;

        //Distort
        [Input("Enable Distortion", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableDistortion;
        [Input("Dist Gain", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDistortionGain;
        [Input("Dist Edge", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDistortionEdge;
        [Input("Dist Post EQCenterFrequenz", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDistortionEQCenterFrequenz;
        [Input("Dist Post Bandwidth", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDistortionBandwidth;
        [Input("Dist Pre Lowpass Cutoff", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FDistortionLowpassCutoff;


        //Echo
        [Input("Enable Echo", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableEcho;
        [Input("Echo DryWetMix", DefaultValue = 50, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEchoWetDryMix;
        [Input("Echo Feedback", DefaultValue = 10, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEchoFeedback;
        [Input("Echo LeftDelay", DefaultValue = -100, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEchoLeftDelay;
        [Input("Echo RightDelay", DefaultValue = 50, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEchoRightDelay;
        [Input("Echo PanDelay", DefaultValue = 0, AsInt = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FEchoPanDelay;


        //Flanger
        [Input("Enable Flanger", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableFlanger;
        [Input("Flanger WetDryMix", DefaultValue = 0.5, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FFlangerWetDryMix;
        [Input("Flanger Depth", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FFlangerDepth;
        [Input("Flanger Feedback", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FFlangerFeedback;
        [Input("Flanger Frequency", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FFlangerFrequency;
        [Input("Flanger Triangle Waveform", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FFlangerTriangleWaveForm;
        [Input("Flanger Delay", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FFlangerDelay;
        [Input("Flanger Phase", DefaultValue = 0.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FFlangerPhase;

        //Gargle
        [Input("Enable Gargle", DefaultValue = 0,  Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableGargle;
        [Input("Gargle Rate Hz", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FGargleRateHz;
        [Input("Gargle SinusWaveForm", DefaultValue = 0.0,  Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FGargleSinusWaveForm;


        //I3DL2 Reverb
        [Input("Enable I3DL2 Reverb", DefaultValue = 0,  Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableI3DL2;
        [Input("I3DL2 Room", DefaultValue = 0, AsInt = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FI3DL2Room;
        [Input("I3DL2 RoomHF", DefaultValue = 0, AsInt = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FI3DL2RoomHF;
        [Input("I3DL2 RoomRollOffFactor", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2RoomRollOffFactor;
        [Input("I3DL2 DecayTime", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2DecayTime;
        [Input("I3DL2 DecayHFRatio", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2DecayHFRatio;
        [Input("I3DL2 Reflections", DefaultValue = 0, AsInt = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FI3DL2Reflections;
        [Input("I3DL2 ReflectionsDelay", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2ReflectionDelay;
        [Input("I3DL2 Reverb", DefaultValue = 0, AsInt = true, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<int> FI3DL2Reverb;
        [Input("I3DL2 ReverbDelay", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2ReverbDelay;
        [Input("I3DL2 Diffusion", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2Diffusion;
        [Input("I3DL2 Density", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2Density;
        [Input("I3DL2 HfReference", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FI3DL2HfReference;


        //Param EQ
        [Input("Enable EQ", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableEq;
        [Input("EQ Center", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEqCenter;
        [Input("EQ Bandwidth", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEqBandwidth;
        [Input("EQ Gain", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FEqGain;


        //wave Reverb
        [Input("Enable Wave Reverb", DefaultValue = 0,  Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FEnableWaveReverb;
        [Input("Reverb InGain", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FWaveReverbInGain;
        [Input("Reverb Mix", DefaultValue = 0.5, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FWaveReverbMix;
        [Input("Reverb Time", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FWaveReverbTime;
        [Input("Reverb HighFreqRTRatio", DefaultValue = 1.0, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<float> FWaveReverbFreq;

        [Input("Disable All Effekts", IsBang = true, DefaultValue = 0.0,  Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FDisableAllEffekt;


        //Device Selection
        [Input("Device", EnumName = "DeviceName")]
        IDiffSpread<EnumEntry> FDeviceenum;



        #endregion Input Pins


        #region Output Pins

        [Output("Length")]
        ISpread<double> FLength;
        [Output("Finished")]
        ISpread<bool> FFinish;
        [Output("Play Position")]
        ISpread<double> FCurrentPos;
        [Output("Multithreaded")]
        ISpread<bool> FMultiT;
        [Output("Driver")]
        ISpread<string> FDriverUse;
        [Output("Message")]
        ISpread<string> FMessage;

        #endregion Output Pins


        //Imports the vvvv Logger
        [Import()]
        ILogger FLogger;

        ISoundEngine FEngine = new ISoundEngine();
        ISoundDeviceList FDevice = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice);

        Dictionary<string, int> FDeviceSelect = new Dictionary<string, int>();
        Dictionary<int, ISoundSource> FLoadedSourceFiles = new Dictionary<int, ISoundSource>();
        List<string> FLoadedFiles = new List<string>();
        List<ISound> FPlayedSounds = new List<ISound>();
        List<int> FFinishedSounds = new List<int>();
        List<string> FilePath = new List<string>();
        Dictionary<int, ISoundSource> SoundSources = new Dictionary<int, ISoundSource>();
        List<string> DeleteList = new List<string>();

        int FPreviousSpreadMax = -1;

        #endregion fields & pins


        public Irrklang()
        {
            //gets all SoundDevice and write it to the Enum Pin
            var s = new string[FDevice.DeviceCount];
            for (int a = 0; a < FDevice.DeviceCount; ++a)
            {
                try
                {
                    FDeviceSelect.Add(FDevice.getDeviceDescription(a), a);
                    s[a] = FDevice.getDeviceDescription(a);
                }
                catch (Exception ex)
                {
                    FLogger.Log(ex);
                }
            }
            EnumManager.UpdateEnum("DeviceName", s[0], s);
            EnumManager.UpdateEnum("PlayMode", "3D", FPlayModeArray);
        }


        public void Dispose()
        {
            if (FEngine != null)
            {
                FEngine.StopAllSounds();
                FEngine.RemoveAllSoundSources();
                FEngine = null;
            }
        }


        //called each frame by vvvv
        public void Evaluate(int SpreadMax)
        {
            bool ChangedSpreadSize = FPreviousSpreadMax == SpreadMax ? false : true;
            bool Reload = false;
            if (FPlay.IsChanged || ChangedSpreadSize || FFile.IsChanged)
                Reload = true;

            FLength.SliceCount = SpreadMax;

            //tells the Irrklang engine which System Audio Output to use
            #region output devices

            if (FDeviceenum.IsChanged)
            {
                try
                {
                    int id = FDeviceSelect[FDeviceenum[0]];
                    FEngine.StopAllSounds();
                    FEngine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions, FDevice.getDeviceID(id));
                    FEngine.LoadPlugins(Path.Combine(Environment.CurrentDirectory, "plugins"));
                    FDriverUse[0] = FEngine.Name;
                    FMultiT[0] = FEngine.IsMultiThreaded;
                }
                catch (Exception ex)
                {
                    FLogger.Log(LogType.Error, ex.Message);
                }

            }

            #endregion output devices

            //Sets the MainVoume of the Engine
            #region MainVolume

            if (FMainVolume.IsChanged)
            {
                FEngine.SoundVolume = (float)FMainVolume[0];
            }

            #endregion MainVolume

            //Handles the Reading and Deleting of the Files
            //and Creating the ISoundSource Onject
            #region File IO
            if (FFile.IsChanged || ChangedSpreadSize)
            {
                FilePath.Clear();
                SoundSources.Clear();

                if (FLoadedFiles.Count < SpreadMax)
                {
                    int Diff = SpreadMax - FLoadedFiles.Count;
                    for (int i = FLoadedFiles.Count; i < SpreadMax; i++)
                    {
                        FLoadedFiles.Add("");
                    }
                }


                for (int i = 0; i < SpreadMax; i++)
                {
                    if (!String.IsNullOrEmpty(FFile[i]))
                    {
                        if (FLoadedFiles[i] != FFile[i])
                        {
                            if (FLoadedFiles.Contains(FFile[i]))
                            {
                                int SourceIndex = FLoadedFiles.IndexOf(FFile[i]);
                                ISoundSource SoundSource;
                                FLoadedSourceFiles.TryGetValue(SourceIndex, out SoundSource);

                                string FileName = Path.Combine(Path.GetDirectoryName(FFile[i]), Path.GetFileNameWithoutExtension(FFile[i]) + "D" + i.ToString() + Path.GetExtension(FFile[i]));
                                ISoundSource NewSoundSource = FEngine.AddSoundSourceAlias(SoundSource, FileName);
                                SoundSources.Add(i, NewSoundSource);
                                FilePath.Add(FFile[i]);
                            }
                            else
                            {
                                if (FilePath.Contains(FFile[i]))
                                {
                                    int SourceIndex = FilePath.IndexOf(FFile[i]);
                                    ISoundSource SoundSource;
                                    SoundSources.TryGetValue(SourceIndex, out SoundSource);

                                    string FileName = Path.Combine(Path.GetDirectoryName(FFile[i]), Path.GetFileNameWithoutExtension(FFile[i]) + "D" + i.ToString() + Path.GetExtension(FFile[i]));
                                    ISoundSource NewSoundSource = FEngine.AddSoundSourceAlias(SoundSource, FileName);
                                    SoundSources.Add(i, NewSoundSource);
                                    FilePath.Add(FFile[i]);
                                }
                                else
                                {
                                    ISoundSource SoundSource = FEngine.AddSoundSourceFromFile(FFile[i]);
                                    SoundSources.Add(i, SoundSource);
                                    FilePath.Add(FFile[i]);
                                }
                            }
                        }
                        else
                        {
                            ISoundSource SoundSource;
                            FLoadedSourceFiles.TryGetValue(i, out SoundSource);
                            SoundSources.Add(i, SoundSource);
                            FilePath.Add(FFile[i]);
                        }
                    }
                    else
                    {
                        SoundSources.Add(i, null);
                        FilePath.Add(FFile[i]);
                    }
                }

                DeleteList.Clear();

                if (FLoadedFiles.Count > SpreadMax)
                {
                    for (int i = FLoadedFiles.Count; i > SpreadMax; i--)
                    {
                        int Index = i - 1;
                        ISoundSource SoundSource;
                        FLoadedSourceFiles.TryGetValue(Index, out SoundSource);
                        if (SoundSource != null)
                        {
                            DeleteList.Add(SoundSource.Name);
                        }
                    }
                }

                int LastFoundIndex = -1;
                foreach (string tFile in FLoadedFiles)
                {
                    if (FilePath.Contains(tFile) == false)
                    {
                        int Index = FLoadedFiles.IndexOf(tFile, LastFoundIndex + 1);
                        if (Index != -1)
                        {
                            ISoundSource SoundSource;
                            FLoadedSourceFiles.TryGetValue(Index, out SoundSource);
                            LastFoundIndex = Index;
                            if (SoundSource != null)
                            {
                                DeleteList.Add(SoundSource.Name);
                            }
                        }
                    }
                }


                foreach (string File in DeleteList)
                {
                    FEngine.RemoveSoundSource(File);
                }


                FLoadedSourceFiles = new Dictionary<int, ISoundSource>(SoundSources);
                FLoadedFiles = new List<string>(FilePath);
            }



            #endregion FileIO

            //Start and stops the laoded files
            //if a file is started the ISound Object is created
            #region Start / Stop Sounds
            if (FPlay.IsChanged || Reload)
            {
                List<ISound> NewSounds = new List<ISound>();

                for (int i = 0; i < SpreadMax; i++)
                {
                    ISoundSource SoundSource;
                    FLoadedSourceFiles.TryGetValue(i, out SoundSource);
                    FLength[i] = (double)SoundSource.PlayLength / 1000;

                    //checks the Play pin
                    if (FPlay[i])
                    {
                        //Creates the sound onject every frame and adds it to the FSound List
                        //can not just played the sound at the given position or replayed 
                        //after Position changed??


                        bool SoundActive = false;

                        try
                        {
                            if (FPlayedSounds[i] != null)
                            {
                                SoundActive = true;
                            }

                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            SoundActive = false;
                        }

                        if (SoundSource != null)
                        {
                            if (SoundActive == false)
                            {
                                if (FPlayMode[0].Name == "3D")
                                {
                                    ISound Sound = FEngine.Play3D(SoundSource, (float)FSoundPosition[i].x, (float)FSoundPosition[i].y, (float)FSoundPosition[i].z, FLoop[i], false, true);
                                    Sound.setSoundStopEventReceiver(this);
                                    NewSounds.Add(Sound);

                                }
                                else
                                {
                                    ISound Sound = FEngine.Play2D(SoundSource, FLoop[i], false, true);
                                    Sound.setSoundStopEventReceiver(this);
                                    NewSounds.Add(Sound);
                                }
                            }
                            else
                            {
                                NewSounds.Add(FPlayedSounds[i]);
                            }
                        }
                        else
                        {
                            FLogger.Log(LogType.Error, "No SoundSource found");
                            NewSounds.Add(null);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (FPlayedSounds[i] != null)
                            {
                                FPlayedSounds[i].Stop();
                                NewSounds.Add(null);
                            }
                            else
                            {
                                NewSounds.Add(null);
                            }

                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            NewSounds.Add(null);
                        }
                        catch (NullReferenceException)
                        {
                            NewSounds.Add(null);
                        }
                    }
                }
                FPlayedSounds = new List<ISound>(NewSounds);
            }

            #endregion Start / Stop Sounds

            //set the Loop Propertie of a ISound Object
            #region Loop

            if (FLoop.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        if (FLoop[i] == true)
                        {
                            FPlayedSounds[i].Looped = true;
                        }
                        else
                        {
                            FPlayedSounds[i].Looped = false;
                        }
                    }
                }
            }

            #endregion Loop

            //handles the seeking operation
            #region Seek

            if (FSeek.IsChanged)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        if (FSeek[i] == true)
                        {
                            FPlayedSounds[i].PlayPosition = (uint)(((UInt32)FSeekPos[i]) * 1000.0);
                        }
                    }
                }
            }

            #endregion Seek

            ////set the Pause Propertie of a ISound Object
            #region Pause

            if (FPause.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        if (FPause[i])
                            FPlayedSounds[i].Paused = true;
                        else
                            FPlayedSounds[i].Paused = false;
                    }
                }
            }

            #endregion Pause

            //set the PlaybackSpeed Propertie of a ISound Object
            #region Speed

            if (FPlaybackSpeed.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                        FPlayedSounds[i].PlaybackSpeed = FPlaybackSpeed[i];
                }
            }


            #endregion Speed

            //stops or paused all SoundSource which are playedback with the Irrklangengine
            #region Stop / Pause All

            if (FStopAll.IsChanged)
            {
                if (FStopAll[0])
                {
                    FEngine.StopAllSounds();
                }
            }

            if (FPauseAll.IsChanged)
            {
                if (FPause[0])
                {
                    FEngine.SetAllSoundsPaused(true);
                }
                else
                {
                    FEngine.SetAllSoundsPaused(false);
                }
            }

            #endregion Stop / Pause All

            //sets the Volume Property of a ISound Object
            #region Volume

            if (FVolume.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                        FPlayedSounds[i].Volume = FVolume[i];
                }
            }

            #endregion Volume

            //sets the Pan Property of a ISound Object, only works if the sound is plyed pack in 2D Mode
            #region Pan

            if (FPan.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null && FPlayMode[i].Name == "2D")
                    {
                        FPlayedSounds[i].Pan = FPan[i];
                    }
                }
            }

            #endregion Pan

            //Sets the Postion Property of a ISound Object, only works in the 3D Playback mode
            #region Position

            if (FSoundPosition.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        Vector3D Vector = FSoundPosition[i];
                        IrrKlang.Vector3D IrrVector = new IrrKlang.Vector3D((float)Vector.x, (float)Vector.y, (float)Vector.z);
                        FPlayedSounds[i].Position = IrrVector;
                    }
                }
            }

            #endregion Position

            //Sets the Velocity Property of a ISound Object, only works in the 3D Playback mode
            #region Sound Velocity

            if (FSoundVelocity.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        Vector3D Vector = FSoundVelocity[i];
                        IrrKlang.Vector3D IrrVector = new IrrKlang.Vector3D((float)Vector.x, (float)Vector.y, (float)Vector.z);
                        FPlayedSounds[i].Velocity = IrrVector;
                    }
                }
            }



            #endregion Sound Velocity

            //sets the MinDistance Propertie of a ISound Object
            #region MinDistance

            if (FMinDist.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        FPlayedSounds[i].MinDistance = FMinDist[i];
                    }
                }
            }

            #endregion MinDistance

            //sets the MaxDistance Propertie of a ISound Object
            #region MaxDistance

            if (FMaxDist.IsChanged || Reload)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FPlayedSounds[i] != null)
                    {
                        FPlayedSounds[i].MaxDistance = FMinDist[i];
                    }
                }
            }

            #endregion MaxDistance

            //set the Listener Position of the Engine
            #region View Listener

            if (FViewDir.IsChanged || FViewPos.IsChanged || FViewUpVector.IsChanged || FViewVelocity.IsChanged)
            {
                IrrKlang.Vector3D ViewDir = new IrrKlang.Vector3D((float)FViewDir[0].x, (float)FViewDir[0].y, (float)FViewDir[0].z);
                IrrKlang.Vector3D ViewPos = new IrrKlang.Vector3D((float)FViewPos[0].x, (float)FViewPos[0].y, (float)FViewPos[0].z);
                IrrKlang.Vector3D ViewVelocity = new IrrKlang.Vector3D((float)FViewVelocity[0].x, (float)FViewVelocity[0].y, (float)FViewVelocity[0].z);
                IrrKlang.Vector3D ViewUp = new IrrKlang.Vector3D((float)FViewUpVector[0].x, (float)FViewUpVector[0].y, (float)FViewUpVector[0].z);

                FEngine.SetListenerPosition(ViewDir, ViewPos, ViewVelocity, ViewUp);
            }

            #endregion View Listener

            //sets the RollOff effekt of the Engine
            #region RollOff

            if (FRollOff.IsChanged)
                FEngine.SetRolloffFactor(FRollOff[0]);

            #endregion RollOFF

            //sets the DopllerEffekt of the Engine
            #region DopplerEffekt

            if (FDoplerFactor.IsChanged || FDoplerDistanceFactor.IsChanged)
            {
                FEngine.SetDopplerEffectParameters(FDoplerFactor[0], FDoplerDistanceFactor[0]);
            }

            #endregion DopplerEffekt

            #region Effekts 

            if (FEnableEffekts[0] == true)
            {

                //Sets the Chorus Effekt of a ISound Object
                #region Chorus

                if (Reload || FEnableChorus.IsChanged || FChorusDelay.IsChanged || FChorusFrequency.IsChanged || FChoruspDepth.IsChanged || FChoruspFeedback.IsChanged || FChorusPhase.IsChanged || FChoruspWetDryMix.IsChanged || FChorusSinusWaveForm.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableChorus[i])
                                Fx.EnableChorusSoundEffect(FChoruspWetDryMix[i], FChoruspDepth[i], FChoruspFeedback[i], FChorusFrequency[i], FChorusSinusWaveForm[i], FChorusDelay[i], FChorusPhase[i]);
                            else
                                Fx.DisableChorusSoundEffect();
                        }
                    }
                }

                #endregion Chorus

                //Sets the Compresser Effekt of a ISound Object
                #region Compressor

                if (Reload || FEnableComp.IsChanged || FCompAttack.IsChanged || FCompGain.IsChanged || FCompPredelay.IsChanged || FCompRatio.IsChanged || FCompRelease.IsChanged || FCompThreshold.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableComp[i])
                                Fx.EnableCompressorSoundEffect(FCompGain[i], FCompAttack[i], FCompRelease[i], FCompThreshold[i], FCompRatio[i], FCompPredelay[i]);
                            else
                                Fx.DisableCompressorSoundEffect();
                        }
                    }
                }

                #endregion Compressor

                //Sets the Distortion Effekt of a ISound Object
                #region Disortion

                if (Reload || FEnableDistortion.IsChanged || FDistortionGain.IsChanged || FDistortionBandwidth.IsChanged || FDistortionEdge.IsChanged || FDistortionEQCenterFrequenz.IsChanged || FDistortionLowpassCutoff.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableDistortion[i])
                                Fx.EnableDistortionSoundEffect(FDistortionGain[i], FDistortionEdge[i], FDistortionEQCenterFrequenz[i], FDistortionBandwidth[i], FDistortionLowpassCutoff[i]);
                            else
                                Fx.DisableDistortionSoundEffect();
                        }
                    }
                }

                #endregion Distortion

                //Sets the Echo Effekt of a ISound Object
                #region Echo

                if (Reload || FEnableEcho.IsChanged || FEchoFeedback.IsChanged || FEchoLeftDelay.IsChanged || FEchoPanDelay.IsChanged || FEchoRightDelay.IsChanged || FEchoWetDryMix.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableEcho[i])
                                Fx.EnableEchoSoundEffect(FEchoWetDryMix[i], FEchoFeedback[i], FEchoLeftDelay[i], FEchoRightDelay[i], FEchoPanDelay[i]);
                            else
                                Fx.DisableEchoSoundEffect();
                        }
                    }
                }

                #endregion Echo

                //Sets the Flanger Effekt of a ISound Object
                #region Flanger

                if (Reload || FEnableFlanger.IsChanged || FFlangerDelay.IsChanged || FFlangerDepth.IsChanged || FFlangerFeedback.IsChanged || FFlangerFrequency.IsChanged || FFlangerPhase.IsChanged || FFlangerTriangleWaveForm.IsChanged || FFlangerWetDryMix.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableFlanger[i])
                                Fx.EnableFlangerSoundEffect(FFlangerWetDryMix[i], FFlangerDepth[i], FFlangerFeedback[i], FFlangerFrequency[i], FFlangerTriangleWaveForm[i], FFlangerDelay[i], FFlangerPhase[i]);
                            else
                                Fx.DisableFlangerSoundEffect();
                        }
                    }
                }

                #endregion Flanger

                //Sets the Gargle Effekt of a ISound Object
                #region Gargle

                if (Reload || FEnableGargle.IsChanged || FGargleRateHz.IsChanged || FGargleSinusWaveForm.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;
                            if (FEnableGargle[i])
                                Fx.EnableGargleSoundEffect(FGargleRateHz[i], FGargleSinusWaveForm[i]);
                            else
                                Fx.DisableGargleSoundEffect();
                        }
                    }
                }

                #endregion Gargle

                //Sets the I3Dl2 Reverb Effekt of a ISound Object
                #region I3Dl2 Reverb

                if (Reload || FEnableI3DL2.IsChanged || FI3DL2DecayHFRatio.IsChanged || FI3DL2DecayTime.IsChanged || FI3DL2Density.IsChanged || FI3DL2Diffusion.IsChanged || FI3DL2HfReference.IsChanged || FI3DL2ReflectionDelay.IsChanged || FI3DL2Reflections.IsChanged || FI3DL2Reverb.IsChanged || FI3DL2ReverbDelay.IsChanged || FI3DL2Room.IsChanged || FI3DL2RoomHF.IsChanged || FI3DL2RoomRollOffFactor.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableI3DL2[i])
                                Fx.EnableI3DL2ReverbSoundEffect(FI3DL2Room[i], FI3DL2RoomHF[i], FI3DL2RoomRollOffFactor[i], FI3DL2DecayTime[i], FI3DL2DecayHFRatio[i], FI3DL2Reflections[i], FI3DL2ReflectionDelay[i], FI3DL2Reverb[i], FI3DL2ReverbDelay[i], FI3DL2Diffusion[i], FI3DL2Density[i], FI3DL2HfReference[i]);
                            else
                                Fx.DisableI3DL2ReverbSoundEffect();
                        }
                    }
                }

                #endregion I3Dl2 Reverb

                //Sets the Param EQ Effekt of a ISound Objec
                #region Param EQ

                if (Reload || FEnableEq.IsChanged || FEqBandwidth.IsChanged || FEqCenter.IsChanged || FEqGain.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableEq[i])
                                Fx.EnableParamEqSoundEffect(FEqCenter[i], FEqBandwidth[i], FEqGain[i]);
                            else
                                Fx.DisableParamEqSoundEffect();
                        }
                    }
                }

                #endregion param EQ

                //Sets the Wave Effekt of a ISound Object
                #region Wave Reverb

                if (Reload || FEnableWaveReverb.IsChanged || FWaveReverbFreq.IsChanged || FWaveReverbInGain.IsChanged || FWaveReverbMix.IsChanged || FWaveReverbTime.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;

                            if (FEnableWaveReverb[i])
                            {
                                Fx.EnableWavesReverbSoundEffect(FWaveReverbInGain[i], FWaveReverbMix[i], FWaveReverbTime[i], FWaveReverbFreq[i]);
                            }
                            else
                            {
                                Fx.DisableWavesReverbSoundEffect();
                            }
                        }
                    }
                }

                #endregion Wave Reverb

                //Disables all Effekts
                #region Disable All Effekts

                if (FDisableAllEffekt.IsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (FPlayedSounds[i] != null)
                        {
                            ISoundEffectControl Fx = FPlayedSounds[i].SoundEffectControl;
                            if (FDisableAllEffekt[i])
                                Fx.DisableAllEffects();

                        }
                    }
                }

                #endregion Disabel All Effects

            }

            #endregion Effekts


            //Reads the Output values form the ISound Object
            #region Node Output

            FCurrentPos.SliceCount = SpreadMax;
            FFinish.SliceCount = SpreadMax;
            FMessage.SliceCount = SpreadMax;

            //Set the OutputPin values
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FPlayedSounds[i] != null)
                {
                    if (FPlayedSounds[i].Finished)
                        FCurrentPos[i] = 0.0;
                    else
                        FCurrentPos[i] = (double)(FPlayedSounds[i].PlayPosition) / 1000.0;
                }
                else
                {
                    FCurrentPos[i] = 0;
                }
                FFinish[i] = false;
            }

            foreach (int Index in FFinishedSounds)
            {
                FFinish[Index] = true;
            }

            FFinishedSounds.Clear();

            #endregion NodeOutput

            FPreviousSpreadMax = SpreadMax;
        }

        #region ISoundStopEventReceiver Members

        /// <summary>
        /// This function is called from the Irrklang Engine if the Playback of an Sound is finished or the SoundSource is deleted from the Engine
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="reason"></param>
        /// <param name="userData"></param>
        public void OnSoundStopped(ISound sound, StopEventCause reason, object userData)
        {
            int Index = FPlayedSounds.IndexOf(sound);
            FCurrentPos[Index] = (double)0.0;
            FFinishedSounds.Add(Index);
        }

        #endregion
    }


}



















