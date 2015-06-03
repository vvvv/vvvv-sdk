//Irrklang Plugin http://www.ambiera.com/irrklang/
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
using System.Diagnostics;
using System.Threading;


using IrrKlang;
using IrrVec3D = IrrKlang.Vector3D;
using Vector3D = VVVV.Utils.VMath.Vector3D;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes
{

	#region PluginInfo
	[PluginInfo(Name = "FileStream", Category = "Irrklang", Help = "Irrklang Gamesound Engine", Tags = "Audio, 3D, Sampleplayer, Sound", AutoEvaluate = true, Author = "sanch, phlegma, readme", Bugs = "Does not play samples shorter than 0,17s")]
	#endregion PluginInfo


	public class Irrklang : IPluginEvaluate, IDisposable, ISoundStopEventReceiver
	{

		#region fields & pins

		#region Input Pins

		//File
		[Input("Play", IsBang=true)]
		IDiffSpread<bool> FPlay;
		
		[Input("Loop")]
		IDiffSpread<bool> FLoop;
		
		[Input("Do Seek", IsBang = true)]
		IDiffSpread<bool> FSeek;
		
		[Input("Seek Time", DefaultValue = 0.0, MinValue = 0.0)]
		IDiffSpread<float> FSeekPos;
		
		[Input("Speed", DefaultValue = 1.0)]
		IDiffSpread<float> FPlaybackSpeed;
		
		[Input("Stop", IsBang=true)]
		IDiffSpread<bool> FStop;
		
		[Input("Pause")]
		IDiffSpread<bool> FPause;
		
		// Sound Playback
		string[] FPlayModeArray = { "3D", "2D" };
		[Input("Play Mode", EnumName = "PlayMode")]
		IDiffSpread<EnumEntry> FPlayMode;
		
		[Input("Stream Mode",DefaultEnumEntry = "NoStreaming", Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<StreamMode> FStreamMode;
		
		[Input("Stream Threshold", DefaultValue = 1024, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<int> FStreamThreashold;
		
		//Sound Control
		[Input("Volume", DefaultValue = 1.0, MaxValue = 1.0, MinValue = 0.0)]
		IDiffSpread<float> FVolume;
		
		[Input("Pan", DefaultValue = 0, MaxValue = 1.0, MinValue = -1.0)]
		IDiffSpread<float> FPan;
		
		// 3D Sound Position
        [Input("Position", DefaultValues = new double[] { 0, 0, 0 })]
		IDiffSpread<Vector3D> FSoundPosition;
        [Input("Velocity", DefaultValues = new double[] { 0, 0, 0 })]
		IDiffSpread<Vector3D> FSoundVelocity;
		[Input("Minimal Distance", DefaultValue = 0, MinValue = float.MinValue)]
		IDiffSpread<float> FMinDist;
        [Input("Maximal Distance", DefaultValue = 1000000000, MaxValue = float.MaxValue)]
		IDiffSpread<float> FMaxDist;
		
		//Main Volume
		[Input("Main Volume", DefaultValue = 0.5, IsSingle = true, MinValue = 0.0, MaxValue = 1.0)]
		IDiffSpread<double> FMainVolume;

		[Input("Stop All", IsSingle = true, IsBang = true, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<bool> FStopAll;
		
		[Input("Pause All", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<bool> FPauseAll;

		// 3D Listener Position
        [Input("View Position", DefaultValues = new double[] { 0, 0, 0 })]
		IDiffSpread<Vector3D> FViewPos;
        [Input("View Direction", DefaultValues = new double[] { 0, 0, 1 })]
		IDiffSpread<Vector3D> FViewDir;
		[Input("View Velocity Per Second", DefaultValues = new double[] { 0, 0, 0 }, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<Vector3D> FViewVelocity;
        [Input("View Up Vector", DefaultValues = new double[] { 0, 1, 0 }, Visibility = PinVisibility.OnlyInspector)]
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
		[Input("Disable all Effekts", IsBang = true, DefaultValue = 0.0,  Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<bool> FDisableAllEffekt;

		//Device Selection
		[Input("Device", EnumName = "DeviceName")]
		IDiffSpread<EnumEntry> FDeviceenum;
		[Input("Filename", DefaultString = "", StringType = StringType.Filename)]
		IDiffSpread<string> FFile;


		#endregion Input Pins


		#region Output Pins

		[Output("Duration")]
		ISpread<double> FLength;
		
		[Output("Position")]
		ISpread<ISpread<double>> FCurrentPos;
		
		[Output("StreamDataLength", Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FStreamDataLengthOut;
		
		[Output("Multithreaded")]
		ISpread<bool> FMultiT;
		
		[Output("Driver")]
		ISpread<string> FDriverUse;


		#endregion Output Pins


		//Imports the vvvv Logger
		[Import()]
		ILogger FLogger;
		
		[Import()]
		IPluginHost FHost;

		ISoundEngine FEngine = new ISoundEngine();
		ISoundDeviceList FDevice = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice);

		Dictionary<string, int> FDeviceSelect = new Dictionary<string, int>();
		int FPreviousSpreadMax = -1;
		List<ISoundSource> FSoundsources = new List<ISoundSource>();
		SortedList<int,List<ISound>> FSounds = new SortedList<int,List<ISound>>();
		List<ISound> FFinishedSounds = new List<ISound>();
		
		Object thisLock = new Object();
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
			FCurrentPos.SliceCount = SpreadMax;
			FLength.SliceCount = SpreadMax;
			FStreamDataLengthOut.SliceCount = SpreadMax;
			
			bool ChangedSpreadSize = FPreviousSpreadMax == SpreadMax ? false : true;

			//tells the Irrklang engine which System Audio Output to use
			#region Output devices

			if (FDeviceenum.IsChanged)
			{
				try
				{
					int id = FDeviceSelect[FDeviceenum[0]];
					FEngine.StopAllSounds();
					FEngine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions, FDevice.getDeviceID(id));
					FDriverUse[0] = FEngine.Name;
					FMultiT[0] = FEngine.IsMultiThreaded;
					
					//HACK: Does not init plugins in the construcotr??
					string PluginPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
					FEngine.LoadPlugins(Path.GetDirectoryName(PluginPath));
					
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
			
			if(ChangedSpreadSize || FFile.IsChanged || FDeviceenum.IsChanged || FPlayMode.IsChanged || FStreamMode.IsChanged || FStreamThreashold.IsChanged)
			{
				FEngine.StopAllSounds();
				FEngine.RemoveAllSoundSources();
				FSoundsources.Clear();
				FSounds.Clear();
				
				for(int i = 0; i < SpreadMax; i++)
				{
					ISoundSource Source;
					int Index = FFile.IndexOf(FFile[i]);
					if(Index < i)
					{
						Source  = FEngine.AddSoundSourceAlias(FSoundsources[Index],FFile[i] + i.ToString());
					}
					else
					{
						Source = FEngine.AddSoundSourceFromFile(FFile[i]);
					}
					
					string EnumState = Enum.GetName(typeof(StreamMode), FStreamMode[i]);
					switch (EnumState) {
						case "AutoDetect":
							Source.StreamMode = StreamMode.AutoDetect;
							break;
						case "NoStreaming":
							Source.StreamMode = StreamMode.NoStreaming;
							Source.ForcedStreamingThreshold = 0;
							break;
						case "Streaming":
							Source.StreamMode = StreamMode.Streaming;
							Source.ForcedStreamingThreshold = FStreamThreashold[i];
							break;
						default:
							FLogger.Log(LogType.Message,"No Streammode set");
							break;
					}
					
					FSoundsources.Add(Source);
				}
			}
			
			#endregion File IO
			
			
			#region Finished Sounds
			
			Monitor.Enter(thisLock);
			try
			{
				List<ISound> Temp = new List<ISound>(FFinishedSounds);
				FFinishedSounds.Clear();
				foreach(ISound Sound in Temp)
				{
					foreach(KeyValuePair<int,List<ISound>> Pair in FSounds)
					{
						if(Pair.Value.Contains(Sound))
						{
							Pair.Value.Remove(Sound);
						}
					}
				}
			}
			finally
			{
				Monitor.Exit(thisLock);
			}
			
			
			#endregion Finshed Sounds
			
			
			for(int i = 0; i < SpreadMax; i++)
			{
				#region PlayBack
				
				List<ISound> SoundsPerSlice;
				FSounds.TryGetValue(i,out SoundsPerSlice);
				
				if(SoundsPerSlice == null)
				{
					SoundsPerSlice = new List<ISound>();
					FSounds.Add(i,SoundsPerSlice);
				}

				
				if(FPlay[i] == true)
				{
					try
					{
						FStreamDataLengthOut[i] = FSoundsources[i].SampleData.Length;
					}catch
					{
						FStreamDataLengthOut[i] = -1;
					}
					try
					{
						if(FPlayMode[i] == "2D")
						{
							ISound Sound = FEngine.Play2D(FSoundsources[i],FLoop[i],true,true);
							Sound.Volume = FVolume[i];
							Sound.Pan = FPan[i];
							Sound.PlaybackSpeed = FPlaybackSpeed[i];
							Sound.Paused = FPause[i];
							Sound.setSoundStopEventReceiver(this);
							SoundsPerSlice.Add(Sound);
						}
						else
						{
							ISound Sound = FEngine.Play3D(FSoundsources[i], (float)FSoundPosition[i].x, (float)FSoundPosition[i].y, (float)FSoundPosition[i].z, FLoop[i], true, true);
							Sound.Volume = FVolume[i];
							Sound.PlaybackSpeed = FPlaybackSpeed[i];
							Sound.MaxDistance = FMaxDist[i];
							Sound.MinDistance = FMinDist[i];
							Vector3D Vector = FSoundVelocity[i];
							IrrKlang.Vector3D IrrVector = new IrrKlang.Vector3D((float)Vector.x, (float)Vector.y, (float)Vector.z);
							Sound.Velocity = IrrVector;
							Sound.Paused = FPause[i];
							Sound.setSoundStopEventReceiver(this);
							SoundsPerSlice.Add(Sound);
						}
					}catch(NullReferenceException ex)
					{
						FLogger.Log(LogType.Error,"File not found in Irrklang");
						FLogger.Log(LogType.Error,ex.Message);
						
					}
				}
				
				if (FVolume.IsChanged || FPlay.IsChanged)
				{
					foreach(ISound Sound in SoundsPerSlice)
					{
						Sound.Volume = FVolume[i];
					}
				}
				
				
				if(FStop[i] == true)
				{
					if(SoundsPerSlice.Count > 0)
					{
						SoundsPerSlice[SoundsPerSlice.Count -1].Stop();
						SoundsPerSlice.RemoveAt(SoundsPerSlice.Count - 1);
					}
					
				}
				
				
				if(FLoop.IsChanged)
				{
					if (FLoop[i] == true)
					{
						foreach(ISound Sound in SoundsPerSlice)
							Sound.Looped = true;
					}
					else
					{
						foreach(ISound Sound in SoundsPerSlice)
							Sound.Looped = false;
					}
				}

				
				if (FPause.IsChanged)
				{
					if (FPause[i])
					{
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.Paused = true;
						}
					}
					else
					{
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.Paused = false;
						}
					}
				}
				
				if (FPlaybackSpeed.IsChanged || FPlay.IsChanged)
				{
					foreach(ISound Sound in SoundsPerSlice)
						Sound.PlaybackSpeed = FPlaybackSpeed[i];
				}
				
				
				
				if (FSeek[i] == true)
				{
					foreach(ISound Sound in SoundsPerSlice)
					{
						Sound.PlayPosition = (uint)(((UInt32)FSeekPos[i]));
					}
				}
				





				if (FPan.IsChanged || FPlay.IsChanged)
				{
					if (FPlayMode[i].Name == "2D")
					{
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.Pan = FPan[i];
						}
					}
					
				}
				

				if (FSoundPosition.IsChanged)
				{
					if (FPlayMode[i].Name == "3D")
					{
						Vector3D Vector = FSoundPosition[i];
						IrrKlang.Vector3D IrrVector = new IrrKlang.Vector3D((float)Vector.x, (float)Vector.y, (float)Vector.z);
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.Position = IrrVector;
						}
					}
					
				}
				
				if (FSoundVelocity.IsChanged || FPlay.IsChanged)
				{
					if (FPlayMode[i].Name == "3D")
					{
						Vector3D Vector = FSoundVelocity[i];
						IrrKlang.Vector3D IrrVector = new IrrKlang.Vector3D((float)Vector.x, (float)Vector.y, (float)Vector.z);
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.Velocity = IrrVector;
						}
						
					}
					
				}
				
				
				if (FMinDist.IsChanged || FPlay.IsChanged)
				{
					if (FPlayMode[i].Name == "3D")
					{
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.MinDistance = FMinDist[i];
						}
					}
				}

				if (FMaxDist.IsChanged || FPlay.IsChanged)
				{
					if (FPlayMode[i].Name == "3D")
					{
						foreach(ISound Sound in SoundsPerSlice)
						{
							Sound.MaxDistance = FMaxDist[i];
						}
					}
				}
				
				#endregion Playback
				
				
				#region Node Output

				//Set the OutputPin values
				FCurrentPos[i].SliceCount  = SoundsPerSlice.Count;
				FLength[i] = FSoundsources[i].PlayLength;
				int Counter = 0;
				foreach (ISound Sound in SoundsPerSlice)
				{
					if(!Sound.Finished)
					{
						FCurrentPos[i][Counter] = (double)(Sound.PlayPosition);
					}
					else
					{
						FCurrentPos[i][Counter] = 0;
					}
					Counter++;
				}
				

				#endregion NodeOutput
				
				
				#region Effekts



				//Sets the Chorus Effekt of a ISound Object
				#region Chorus

				if (FPlay.IsChanged || FEnableChorus.IsChanged || FChorusDelay.IsChanged || FChorusFrequency.IsChanged || FChoruspDepth.IsChanged || FChoruspFeedback.IsChanged || FChorusPhase.IsChanged || FChoruspWetDryMix.IsChanged || FChorusSinusWaveForm.IsChanged)
				{
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableChorus[i])
							Fx.EnableChorusSoundEffect(FChoruspWetDryMix[i], FChoruspDepth[i], FChoruspFeedback[i], FChorusFrequency[i], FChorusSinusWaveForm[i], FChorusDelay[i], FChorusPhase[i]);
						else if(Fx != null)
						{
							Fx.DisableChorusSoundEffect();
						}
					}
				}

				#endregion Chorus

				//Sets the Compresser Effekt of a ISound Object
				#region Compressor

				if (FPlay.IsChanged || FEnableComp.IsChanged || FCompAttack.IsChanged || FCompGain.IsChanged || FCompPredelay.IsChanged || FCompRatio.IsChanged || FCompRelease.IsChanged || FCompThreshold.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableComp[i])
							Fx.EnableCompressorSoundEffect(FCompGain[i], FCompAttack[i], FCompRelease[i], FCompThreshold[i], FCompRatio[i], FCompPredelay[i]);
						else if(Fx != null)
							Fx.DisableCompressorSoundEffect();

					}
					
				}

				#endregion Compressor

				//Sets the Distortion Effekt of a ISound Object
				#region Disortion

				if (FPlay.IsChanged || FEnableDistortion.IsChanged || FDistortionGain.IsChanged || FDistortionBandwidth.IsChanged || FDistortionEdge.IsChanged || FDistortionEQCenterFrequenz.IsChanged || FDistortionLowpassCutoff.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableDistortion[i])
							Fx.EnableDistortionSoundEffect(FDistortionGain[i], FDistortionEdge[i], FDistortionEQCenterFrequenz[i], FDistortionBandwidth[i], FDistortionLowpassCutoff[i]);
						else if(Fx != null)
							Fx.DisableDistortionSoundEffect();
					}
					
				}

				#endregion Distortion

				//Sets the Echo Effekt of a ISound Object
				#region Echo

				if (FPlay.IsChanged || FEnableEcho.IsChanged || FEchoFeedback.IsChanged || FEchoLeftDelay.IsChanged || FEchoPanDelay.IsChanged || FEchoRightDelay.IsChanged || FEchoWetDryMix.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableEcho[i])
							Fx.EnableEchoSoundEffect(FEchoWetDryMix[i], FEchoFeedback[i], FEchoLeftDelay[i], FEchoRightDelay[i], FEchoPanDelay[i]);
						else if(Fx != null)
							Fx.DisableEchoSoundEffect();
					}
					
				}

				#endregion Echo

				//Sets the Flanger Effekt of a ISound Object
				#region Flanger

				if (FPlay.IsChanged || FEnableFlanger.IsChanged || FFlangerDelay.IsChanged || FFlangerDepth.IsChanged || FFlangerFeedback.IsChanged || FFlangerFrequency.IsChanged || FFlangerPhase.IsChanged || FFlangerTriangleWaveForm.IsChanged || FFlangerWetDryMix.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableFlanger[i])
							Fx.EnableFlangerSoundEffect(FFlangerWetDryMix[i], FFlangerDepth[i], FFlangerFeedback[i], FFlangerFrequency[i], FFlangerTriangleWaveForm[i], FFlangerDelay[i], FFlangerPhase[i]);
						else if(Fx != null)
							Fx.DisableFlangerSoundEffect();
					}
					
				}

				#endregion Flanger

				//Sets the Gargle Effekt of a ISound Object
				#region Gargle

				if (FPlay.IsChanged || FEnableGargle.IsChanged || FGargleRateHz.IsChanged || FGargleSinusWaveForm.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;
						if (FEnableGargle[i])
							Fx.EnableGargleSoundEffect(FGargleRateHz[i], FGargleSinusWaveForm[i]);
						else if(Fx != null)
							Fx.DisableGargleSoundEffect();
					}
					
				}

				#endregion Gargle

				//Sets the I3Dl2 Reverb Effekt of a ISound Object
				#region I3Dl2 Reverb

				if (FPlay.IsChanged || FEnableI3DL2.IsChanged || FI3DL2DecayHFRatio.IsChanged || FI3DL2DecayTime.IsChanged || FI3DL2Density.IsChanged || FI3DL2Diffusion.IsChanged || FI3DL2HfReference.IsChanged || FI3DL2ReflectionDelay.IsChanged || FI3DL2Reflections.IsChanged || FI3DL2Reverb.IsChanged || FI3DL2ReverbDelay.IsChanged || FI3DL2Room.IsChanged || FI3DL2RoomHF.IsChanged || FI3DL2RoomRollOffFactor.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableI3DL2[i])
							Fx.EnableI3DL2ReverbSoundEffect(FI3DL2Room[i], FI3DL2RoomHF[i], FI3DL2RoomRollOffFactor[i], FI3DL2DecayTime[i], FI3DL2DecayHFRatio[i], FI3DL2Reflections[i], FI3DL2ReflectionDelay[i], FI3DL2Reverb[i], FI3DL2ReverbDelay[i], FI3DL2Diffusion[i], FI3DL2Density[i], FI3DL2HfReference[i]);
						else if(Fx != null)
							Fx.DisableI3DL2ReverbSoundEffect();
					}
					
				}

				#endregion I3Dl2 Reverb

				//Sets the Param EQ Effekt of a ISound Objec
				#region Param EQ

				if (FPlay.IsChanged || FEnableEq.IsChanged || FEqBandwidth.IsChanged || FEqCenter.IsChanged || FEqGain.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;
						if (FEnableEq[i])
							Fx.EnableParamEqSoundEffect(FEqCenter[i], FEqBandwidth[i], FEqGain[i]);
						else if(Fx != null)
							Fx.DisableParamEqSoundEffect();
					}
					
				}

				#endregion param EQ

				//Sets the Wave Effekt of a ISound Object
				#region Wave Reverb

				if (FPlay.IsChanged ||  FEnableWaveReverb.IsChanged || FWaveReverbFreq.IsChanged || FWaveReverbInGain.IsChanged || FWaveReverbMix.IsChanged || FWaveReverbTime.IsChanged)
				{
					
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;

						if (FEnableWaveReverb[i])
						{
							Fx.EnableWavesReverbSoundEffect(FWaveReverbInGain[i], FWaveReverbMix[i], FWaveReverbTime[i], FWaveReverbFreq[i]);
						}
						else if(Fx != null)
							Fx.DisableWavesReverbSoundEffect();
						
					}
					
				}

				#endregion Wave Reverb
				
				//Disables all Effekts
				#region Disable All Effekts

				if (FDisableAllEffekt.IsChanged)
				{
					foreach(ISound Sound in SoundsPerSlice)
					{
						ISoundEffectControl Fx = Sound.SoundEffectControl;
						if (FDisableAllEffekt[i])
							Fx.DisableAllEffects();
					}
				}

				#endregion Disabel All Effects

				#endregion Effekts
				
			}
			
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
			
			//set the Listener Position of the Engine
			#region View Listener

			if (FViewDir.IsChanged || FViewPos.IsChanged || FViewUpVector.IsChanged || FViewVelocity.IsChanged)
			{
				IrrKlang.Vector3D ViewDir = new IrrKlang.Vector3D((float)FViewDir[0].x, (float)FViewDir[0].y, (float)FViewDir[0].z);
				IrrKlang.Vector3D ViewPos = new IrrKlang.Vector3D((float)FViewPos[0].x, (float)FViewPos[0].y, (float)FViewPos[0].z);
				IrrKlang.Vector3D ViewVelocity = new IrrKlang.Vector3D((float)FViewVelocity[0].x, (float)FViewVelocity[0].y, (float)FViewVelocity[0].z);
				IrrKlang.Vector3D ViewUp = new IrrKlang.Vector3D((float)FViewUpVector[0].x, (float)FViewUpVector[0].y, (float)FViewUpVector[0].z);
				FEngine.SetListenerPosition(ViewPos, ViewDir, ViewVelocity, ViewUp);
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
			Monitor.TryEnter(thisLock);
			try
			{
				FFinishedSounds.Add(sound);
			}
			finally
			{
				Monitor.Exit(thisLock);
			}
		}
	}

	#endregion
}