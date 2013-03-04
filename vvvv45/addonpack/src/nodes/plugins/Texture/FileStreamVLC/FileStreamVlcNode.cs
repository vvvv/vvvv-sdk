//    vvvv Vlc plugin
//    
//    Author:  Frederik Tilkin
//
//    vvvv Vlc plugin is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published by
//    the Free Software Foundation, either version 2.1+ of the License, or
//    (at your option) any later version.
//
//    vvvv Vlc plugin is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Lesser General Public License for more details.
//     
// ========================================================================


#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;


using SlimDX;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Core.Logging;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

using System.IO;
using System.Collections.Generic;

using VVVV.Nodes.Vlc.Player;
using VVVV.Nodes.Vlc.Utils;


//FOR MEDIARENDERER
using LibVlcWrapper;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
//using Un4seen.Bass;
//using BassSound.Internals;
//needed for loading the file with extra search paths
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.IO;



#endregion usings

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.TexturedVertex;

namespace VVVV.Nodes.Vlc
{
	#region PluginInfo
    [PluginInfo(Name = "FileStream", Category = "EX9.Texture", Version = "VLC", Author = "ft", Help = "Fully spreadeble video/image to texture player based on LibVlc", Tags = "video, audio, image, texture", Credits = "Frederik Tilkin, the authors of Vlc player, Roman Ginzburg", Bugs = "see http://trac.videolan.org/vlc/ticket/3152 | Don't trust the position and frame pins.", Warnings = "")]
	#endregion PluginInfo
	public class FileStreamVlcNode : DXTextureOutPluginBase, IPluginEvaluate, IDisposable
	{
		
		#region pins

		[Input("Play", DefaultValue = 1)]
		IDiffSpread<bool> FPlayIn;

		[Input("Loop", DefaultValue = 0)]
		IDiffSpread<bool> FLoopIn;

        [Input("Loop Start Time", DefaultValue = 0)]
        IDiffSpread<float> FLoopStartIn;

        [Input("Loop End Time", DefaultValue = 999999)]
        IDiffSpread<float> FLoopEndIn;

		[Input("Do Seek", DefaultValue = 0, IsBang = true)]
		IDiffSpread<bool> FDoSeekIn;

		[Input("Seek Time", DefaultValue = 0)]
		IDiffSpread<float> FSeekTimeIn;

		[Input("Speed", DefaultValue = 1)]
		IDiffSpread<float> FSpeedIn;        

		[Input("Volume", DefaultValue = 1)]
		IDiffSpread<float> FVolumeIn;

		[Input("Forced Width", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<int> FWidthIn;

		[Input("Forced Height", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
		IDiffSpread<int> FHeightIn;

		[Input("Rotate", DefaultValue = 0, Visibility = PinVisibility.False)]
		IDiffSpread<int> FRotateIn;

		[Input("NextFilename", StringType = StringType.Filename, DefaultString = "", Visibility = PinVisibility.Hidden)]
		IDiffSpread<string> FNextFileNameIn;

		[Input("Filename", StringType = StringType.Filename, DefaultString = "C:\\video.avi | deinterlace=1 | video-filter=gradient{type=1}")]
		IDiffSpread<string> FFileNameIn;


		[Output("Duration")]
		ISpread<float> FDurationOut;
		
		[Output("Position")]
		ISpread<float> FPositionOut;

		[Output("Frame")]
		ISpread<int> FFrameOut;

		[Output("FrameCount")]
		ISpread<int> FFrameCountOut;

		[Output("Width", Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FWidthOut;

		[Output("Height", Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FHeightOut;

		[Output("Texture Aspect Ratio", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
		ISpread<float> FTextureAspectRatioOut;

		[Output("Pixel Aspect Ratio", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
		ISpread<float> FPixelAspectRatioOut;

		[Output("Next Ready", Visibility = PinVisibility.Hidden)]
		ISpread<bool> FNextReadyOut;

//		[Output("Bass Handle")]
//		ISpread<int> FBassHandleOut;

		#endregion pins

		#region private classes
		public class MediaRenderer : IDisposable
		{
			#region MediaRenderer fields
			
			private static IntPtr libVLC = IntPtr.Zero;
			
			//needed to access pins (at the right slice)
			private FileStreamVlcNode parent;
			private int slice = 0; //slice index
	
			private string currFileNameIn;
			private string newFileNameIn = "";
			//COPY OF CURRFILENAMEIN FOR USING IN THE (THREADED) UpdateMediaPlayerStatus
			private string prevFileNameIn;
			private bool currPlayIn;
            private float currLoopStartIn;
            private float currLoopEndIn;
            private int currLoopLengthIn; //milliseconds
			private bool currLoopIn;
			private float currSpeedIn;
			private float currSeekTimeIn;
			private bool currDoSeekIn;
			private int currRotateIn;
			private int currWidthIn;
			private int currHeightIn;
			private float currVolumeIn;
	
			//private System.Threading.Timer loopTimer;
			private System.Windows.Forms.Timer loopTimer;
			//private System.Timers.Timer loopTimer;
	
			private Thread evaluateThread;
			//will work when signalled by evaluateEventWaitHandle
			private EventWaitHandle evaluateEventWaitHandle;
			private EventWaitHandle evaluateStopThreadWaitHandle;
			private Mutex mediaPlayerBusyMutex;
			//used for starting and stopping etc. in separate thread
			private IntPtr media = IntPtr.Zero;
			private IntPtr preloadMedia = IntPtr.Zero;
			private IntPtr mediaPlayer = IntPtr.Zero;
	
			private IntPtr opaqueForCallbacks = IntPtr.Zero;
			private DoubleMemoryBuffer pixelPlanes;
	
			private int preloadingStatus;
			private const int STATUS_INACTIVE = -11;
			private const int STATUS_NEWFILE = -10;
			private const int STATUS_OPENINGFILE = -9;
			private const int STATUS_GETPROPERTIES = -8;
			private const int STATUS_GETPROPERTIESOK = -7;
			private const int STATUS_GETFIRSTFRAME = -6;
			private const int STATUS_WAITING = -5;
			private const int STATUS_IMAGE = -1;
			private const int STATUS_READY = 0;
			private const int STATUS_PLAYING = 1;
			
			private long statusWaitingUntilTicks = DateTime.Now.Ticks;
			
			private int videoWidth;
			private int videoHeight;
			private float videoLength;
			private float videoFps;
			
			private const int lockMaxTimeout = -1;
			//ms (-1 = forever)
			private int readPlaneNotDrawn = 0;
			//how many times display has been called, without the readplane being copied to the display buffer
			private int displayCalled = 0;
			//how many times display has been called
			private int prevDisplayCalled = 0;
			//last value of display when rendering
			private int lockCalled = 0;
			//how many times LOCK has been called
			private int unlockCalled = 0;
			//how many times UNLOCK has been called
			private int preloadDisplayCalled = 0;
			private int currentFrame = 0;
			//current video frame that has been decoded
			private bool readyForPlaying = false;
	
			//VLC options
			//make sure garbage collector doesn't remove this
			private VlcVideoLockHandlerDelegate vlcVideoLockHandlerDelegate;
			private VlcVideoUnlockHandlerDelegate vlcVideoUnlockHandlerDelegate;
			private VlcVideoDisplayHandlerDelegate vlcVideoDisplayHandlerDelegate;
	
			private VlcAudioPlayDelegate vlcAudioPlayDelegate;
	
			private int bassStreamHandle; //the handle of the BASS stream we will push the audio to c
	
			private bool disposing = false;
			private bool initialized = false;
			
			#endregion MediaRenderer fields
	
			#region MediaRenderer static helper functions
			/// <summary>
			/// The function returns the file path of the assembly (the dll file) this class resides in. 
			/// </summary>
			static public string AssemblyDirectory
			{
			    get
			    {
			        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			        UriBuilder uri = new UriBuilder(codeBase);
			        string path = Uri.UnescapeDataString(uri.Path);
			        return Path.GetDirectoryName(path);
			    }
			}
	
			/// <summary>
			/// This function will look for the file in all of the folders defined in the searchPathFile, 
			/// and returns the FIRST path where the given fileName is found. It will also look in the 
			/// same directory as this dll itself.
			/// </summary>
			/// <param name="fileName"></param>
			/// <param name="searchPathFileName"></param>
			/// <returns></returns>
			private static string FindFilePath(string fileName, string searchPathFileName) {
				//if the libvlc dll is not found in any folder of the PATH environment variable,
				//search also in directories specified in the text-file
	
				string sameDirAsCallingCode = AssemblyDirectory + "\\";
				if ( File.Exists( sameDirAsCallingCode + fileName) ) {
					return sameDirAsCallingCode;
				}
				
				//const string searchPathFileName = "libvlc_searchpath.txt";
				string searchPathFilePath = sameDirAsCallingCode + searchPathFileName;
				
				//string searchpath = searchPathFilePath + "\n";
				try {
					foreach ( string row in File.ReadAllLines( searchPathFilePath ) ) {
						//ignore lines starting with # and ignore empty lines
						if ( ! ( row.Length == 0 || row.StartsWith("#") ) ) {
							string currentPath = row + ( row.EndsWith( "\\" ) ? "" : "\\" );
							
							currentPath = Environment.ExpandEnvironmentVariables( currentPath );
							
							if ( row.StartsWith(".") ) {
								//relative path
								currentPath = AssemblyDirectory + "\\" + currentPath;
							}
							else {
								//absolute path								
							}
	
							if ( File.Exists( currentPath + fileName) ) {
								//ideally check if the version is ok to use
								return currentPath;
							}
						}
					}
				}
				catch (IOException) {
					throw new Exception( "A file named " + searchPathFilePath + " should exist (in the same folder as the Vlc node's dll). This file, which contains paths where the plugin should look for the libvlc.dll (and others) could not be opened, so probably, loading the Vlc plugin will fail." );
					//MessageBox.Show( "A file named " + searchPathFilePath + " should exist (in the same folder as the Vlc node's dll). This file, which contains paths where the plugin should look for the libvlc.dll (and others) could not be opened, so probably, loading the Vlc plugin will fail.", "Vlc plugin error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return null;
			}

			static void TryToFindLibVLC()
			{
				string libvlcdllPath = FindFilePath( "libvlc.dll", "libvlc_searchpath.txt" );
				if ( libvlcdllPath != null ) {
					string pathEnvVar = Environment.GetEnvironmentVariable( "PATH" );
					Environment.SetEnvironmentVariable( "PATH", pathEnvVar + ";" + libvlcdllPath );
				}
				else {
					throw new Exception( "The libvlc.dll file could not be found in any of the paths specified in libvlc_searchpath.txt, so probably, loading the Vlc plugin will fail." );
					//MessageBox.Show( "The libvlc.dll file could not be found in any of the paths specified in libvlc_searchpath.txt, so probably, loading the Vlc plugin will fail.", "Vlc plugin error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

			}
	
	        #endregion MediaRenderer static helper functions

			#region MediaRenderer constructor/destructor			

			static MediaRenderer() {
				
				TryToFindLibVLC();
				
				string[] argv = {
					"--no-video-title",
					"--no-one-instance",
					"--directx-audio-speaker=5.1"
				};

				libVLC = LibVlcMethods.libvlc_new(argv.GetLength(0), argv);				
			}
			
			
			public MediaRenderer(FileStreamVlcNode parentObject, int index)
			{
				//MessageBox.Show("This is a test.", "MessageBox TEST", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				
				//if not initalized yet
/*				if ( libVLC == IntPtr.Zero ) {
					string[] argv = {
						"--no-video-title",
						"--no-one-instance",
						"--directx-audio-speaker=5.1"
					};

					libVLC = LibVlcMethods.libvlc_new(argv.GetLength(0), argv);
				}
*/
				parent = parentObject;
				slice = index;
					
				PrepareMediaPlayer();
			}

/* this way Dispose might get called twice
			~MediaRenderer()
			{
				Dispose();
			}
*/

			private void PrepareMediaPlayer()
			{
	
				vlcVideoLockHandlerDelegate = VlcVideoLockCallBack;
				vlcVideoUnlockHandlerDelegate = VlcVideoUnlockCallBack;
				vlcVideoDisplayHandlerDelegate = VlcVideoDisplayCallBack;
				
				vlcAudioPlayDelegate = VlcAudioPlayCallBack;
				
				opaqueForCallbacks = Marshal.AllocHGlobal(4);
	//				unsafe {
	//					opaqueForCallbacks = Marshal.AllocHGlobal(4);
	//					Int32* p = ((Int32*)opaqueForCallbacks.ToPointer());
	//					*p = i;
	//				}
	
				media = new IntPtr();
				preloadMedia = new IntPtr();
				mediaPlayer = LibVlcMethods.libvlc_media_player_new(libVLC);
				LibVlcMethods.libvlc_media_player_retain(mediaPlayer);
	
	
				//this mutex will protect the mediaPlayer when accessed by different threads
				mediaPlayerBusyMutex = new Mutex();
	
				//Handle some VLC events!
				VlcEventHandlerDelegate h = VlcEventHandler;
				IntPtr ptr = Marshal.GetFunctionPointerForDelegate(h);
				IntPtr eventManager = LibVlcMethods.libvlc_media_player_event_manager(mediaPlayer);
				//LibVlcMethods.libvlc_event_attach( eventManager, libvlc_event_e.libvlc_MediaPlayerEncounteredError, ptr, IntPtr.Zero);
				//LibVlcMethods.libvlc_event_attach( eventManager, libvlc_event_e.libvlc_MediaPlayerEndReached, ptr, IntPtr.Zero);
				//LibVlcMethods.libvlc_event_attach( eventManager, libvlc_event_e.libvlc_MediaPlayerMediaChanged, ptr, IntPtr.Zero);
				//LibVlcMethods.libvlc_event_attach( eventManager, libvlc_event_e.libvlc_MediaStateChanged, ptr, IntPtr.Zero);
	
	
				preloadingStatus = STATUS_INACTIVE;
				videoWidth = 2;
				videoHeight = 2;
	
				videoLength = 0;
				videoFps = 1;
	
				pixelPlanes = new DoubleMemoryBuffer(videoWidth, videoHeight);
	
				preloadingStatus = STATUS_NEWFILE;
	
				currFileNameIn = null;
				prevFileNameIn = null;
	
				//CREATE A THREAD THAT WILL TRY TO LOAD NEW FILES ETC. 
				//when signalled by evaluateEventWaitHandle
				evaluateEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
				evaluateStopThreadWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
				evaluateThread = new Thread(new ThreadStart(EvaluateThreadProc));
				
				initialized = true;
				
				evaluateThread.Start();

				
/*				TIMERS SUCK, THEY ARE ALWAYS TOO LATE !!!

				loopTimer = new System.Timers.Timer( 1000 );		
				loopTimer.Start();
				loopTimer.Elapsed += LoopTimerEventHandler3;


				loopTimer = new System.Threading.Timer( LoopTimerEventHandler2, null, 1000, 1000 ); //
*/

				loopTimer = new System.Windows.Forms.Timer();
				loopTimer.Interval = 1000; //1 second
				loopTimer.Enabled = false;
				loopTimer.Tick += new EventHandler( LoopTimerEventHandler );
			}
	
			/*
			 * Makes sure we stop the media-player completely when it's no longer needed, 
			 * after that the GC can do it's job when it wants to.
			 */
			public void PrepareForDisposal() {
				loopTimer.Stop();
				evaluateStopThreadWaitHandle.Set();				
			}
			
			/*
			 * Should be callable multiple times without causing exceptions !!!
			 */  
			public void Dispose()
			{
				disposing = true;
				
				//parent.FLogger.Log( LogType.Debug, "[Dispose] Disposing media renderer " + mediaRendererIndex);
				//evaluateThread.Abort();
				evaluateStopThreadWaitHandle.Set();
				evaluateThread.Join();
				//preloadingStatus = STATUS_INACTIVE;
				
				//deallocate video memory etc.
				try {
					loopTimer.Stop();
					loopTimer.Dispose();

					
					//DONE IN evaluateThread!!! LibVlcMethods.libvlc_media_player_release( mediaPlayer );

					pixelPlanes.Dispose();
					
					Marshal.FreeHGlobal( opaqueForCallbacks );

					mediaPlayerBusyMutex.Dispose();		
		
					evaluateEventWaitHandle.Dispose();
					evaluateStopThreadWaitHandle.Dispose();
				
				} catch {
				}


				Log( LogType.Debug, "[Dispose] done...");

				// Use SupressFinalize in case a subclass of this type implements a finalizer.
				GC.SuppressFinalize( this );
			}
			#endregion MediaRenderer constructor/destructor
	

			public void Evaluate(bool active)
			{
				//Log( LogType.Debug, "[Evaluate Called] for " + (active ? "FRONT " : "BACK ") + "renderer " + mediaRendererIndex);
	
	//				if (evaluateCalled < 10) {
	//					evaluateCalled++;
	//					return;
	//				}
	
				try {
	
					prevTime = DateTime.Now.Ticks;
					//for ReportElapsedTime
	
					if (GetFileNameIn(true).IsChanged || GetFileNameIn(false).IsChanged || currFileNameIn == null) {
						//prevFileNameIn = currFileNameIn;
						currFileNameIn = GetFileNameIn(active)[slice];
	
						if (currFileNameIn == null) {
							Log( LogType.Debug, (active ? "FileNameIn" : "NextFileNameIn") + "[" + slice + "] IS NULL!" );
							currFileNameIn = "";
						}
	
						if (currFileNameIn.Length > 0) {
							string[] splitFileName = currFileNameIn.Split("|".ToCharArray());
							//Log( LogType.Debug, "Path = " + currFileNameIn );
	
							currFileNameIn = GetFullPath(splitFileName[0]);
							for (int i = 1; i < splitFileName.GetLength(0); i++) {
								currFileNameIn += "|" + splitFileName[i];
							}
							//Log( LogType.Debug, "FULL Path = " + currFileNameIn );
						}
					}
	
					currPlayIn = IsPlaying(active);
					currLoopStartIn = Math.Max( 0, parent.FLoopStartIn[slice] );
					currLoopEndIn = videoLength > 0 
									? Math.Min( parent.FLoopEndIn[slice], videoLength) 
									: parent.FLoopEndIn[slice];
					currLoopIn = parent.FLoopIn[slice];
                    if ( currLoopStartIn >= currLoopEndIn ) {
						currLoopIn = false; //disable looping
                    	currLoopStartIn = 0;
                    	currLoopEndIn = videoLength;
                    }
					
					currLoopLengthIn = Math.Max( (int)( ( currLoopEndIn - currLoopStartIn ) * 1000 ), 1);
					loopTimer.Enabled = currLoopIn;

					
					currSpeedIn = parent.FSpeedIn[slice];
					if (parent.FDoSeekIn[slice]) {
						currSeekTimeIn = parent.FSeekTimeIn[slice];
						currDoSeekIn = true;
					}
					currRotateIn = parent.FRotateIn[slice];
					currWidthIn = parent.FWidthIn[slice];
					currHeightIn = parent.FHeightIn[slice];
					currVolumeIn = parent.FVolumeIn[slice];
	
					ReportElapsedTime("Setting current values", 15.7);
	
					//Log( LogType.Debug, "Evaluate_Threaded( " + active + " )" );
					Evaluate_Threaded(active);
	
					ReportElapsedTime("Evaluate_Threaded", 15.7);
	
					//Log( LogType.Debug, "UpdateParent( " + active + " )" );
					UpdateParent(active);
	
					ReportElapsedTime("UpdateParent", 15.7);
	
				} catch (Exception e) {
					Log( LogType.Error, "[MediaRenderer Evaluate Exception] " + e.Message + "\n\n" + e.StackTrace);
				}
	
			}
	
			#region MediaRenderer Vlc Video Callback functions
	
			//////////////////////////////////////////////////
			// Next 3 functions are used for PLAYING the video
			//////////////////////////////////////////////////
			public IntPtr VlcVideoLockCallBack(ref IntPtr data, ref IntPtr pixelPlane)
			{
				if ( initialized ) {
					switch ( preloadingStatus ) {
						case STATUS_GETFIRSTFRAME:
						case STATUS_PLAYING:
							break;
						case STATUS_INACTIVE:
						case STATUS_OPENINGFILE:
						case STATUS_GETPROPERTIES:
						case STATUS_GETPROPERTIESOK:
						case STATUS_IMAGE:
							Log( LogType.Debug, "Hmm, status is " + StatusToString(preloadingStatus) + " so we shouldn't be arriving here..." );
							//throw new Exception("Hey hey" );
							break;
						case STATUS_NEWFILE:
							Log( LogType.Debug, "Still playing but waiting for a new file... " + StatusToString(preloadingStatus) + "" );
							break;
						case STATUS_READY:
							//Log( LogType.Debug, "Hmm" );
							break;
					}
				}
				else {
					Log( LogType.Error, ("VlcLockCallback(" + data.ToInt32() + ") : PLAYER HAS BEEN DISPOSED AND STILL PLAYING ???") );
					return IntPtr.Zero;
				}
				//if (lockCalled != unlockCalled) Log( LogType.Error, (parent.IsFrontMediaRenderer(this) ? "FRONT " : "BACK ") + "(lock/unlock=" + lockCalled  + "/" + unlockCalled + ")" );
	
				try {
                    if ( currPlayIn && (preloadingStatus == STATUS_PLAYING || preloadingStatus == STATUS_NEWFILE) ) {
                        currentFrame++;
                    }

					lockCalled++;
					pixelPlane = pixelPlanes.BackBuffer;
					//writePixelPlane;
					//pixelPlane = writeMemoryTexture.GetSurfaceLevel(0).LockRectangle(0, LockFlags.None).Data.DataPointer;
					//pixelPlane = writeMemoryTexture.LockRectangle(0, LockFlags.None).Data.DataPointer;
					//pixelPlane = writeMemoryTexture.LockRectangle(0, LockFlags.None).Data.DataPointer;
	
					//if (data.ToInt32() < 0) {
					//	Log( LogType.Error, ("VlcLockCallback(" + data.ToInt32() + ") : Hoe kan data nu < 0 zijn allee? Heeft er iemand in zitten schrijven?") );
					//}
	
				} catch (Exception e) {
					Log( LogType.Error, "[VlcLockCallback(" + data.ToInt32() + ") Exception] " + e.Message);
				}
	
				if ( ! pixelPlanes.LockBackBufferForWriting(500) ) {
					Log( LogType.Error, "[VlcLockCallback(" + data.ToInt32() + ") Problem] locking backbuffer failed..." );
				}
				//decodeLock.WaitOne();
				return pixelPlane;
			}
	
			public void VlcVideoUnlockCallBack(ref IntPtr data, ref IntPtr id, ref IntPtr pixelPlane)
			{
				try {
					if ( ! initialized ) {
						Log( LogType.Error, ("VlcUnlockCallback(" + data.ToInt32() + ") : PLAYER HAS BEEN DISPOSED AND STILL PLAYING ???") );
						return;
					}
	
					// VLC just rendered the video (RGBA), but we can also render stuff
					///////////////////////////////////////////////////////////////////
					unlockCalled++;
	
					//if (data.ToInt32() < 0) {
					//	Log( LogType.Error, ("VlcUnlockCallback(" + data.ToInt32() + ") : Hoe kan data nu < 0 zijn allee? Heeft er iemand in zitten schrijven?") );
					//}
				} catch (Exception e) {
					Log( LogType.Error, ("[VlcUnlockCallback(" + data.ToInt32() + ") Exception] " + e.Message));
				}
	
				pixelPlanes.UnlockBackBuffer();
				//decodeLock.ReleaseMutex();
			}
	
			public void VlcVideoDisplayCallBack(ref IntPtr data, ref IntPtr id)
			{
				try {
					if (disposing) {
						Log( LogType.Error, ("VlcDisplayCallback(" + data.ToInt32() + ") : PLAYER HAS BEEN DISPOSED AND STILL PLAYING ???") );
						return;
					}
	
					if (preloadingStatus == STATUS_GETFIRSTFRAME) {
						videoLength = 0;
						currentFrame = 0;
						
						preloadDisplayCalled++;
						AllowDisplay(data);
	
						//Log( LogType.Debug, (parent.IsFrontMediaRenderer(this) ? "FRONT " : "BACK ") + "[VlcDisplayCallBack] Setting STATUS_READY (from VlcDisplayCallback)" );
						//if ( mediaPlayerBusyMutex.WaitOne() ) {
						preloadingStatus = STATUS_READY;
						//Log( LogType.Debug, (parent.IsFrontMediaRenderer(this) ? "FRONT " : "BACK ") + "[VlcDisplayCallBack] Setting STATUS_READY (from VlcDisplayCallback) DONE !!!" );
						//	mediaPlayerBusyMutex.ReleaseMutex();
						//}
					} else if (preloadingStatus == STATUS_PLAYING) {
						// VLC wants to display the video
						displayCalled++;
	
						AllowDisplay(data);
					}
				} catch (Exception e) {
					Log( LogType.Error, "[VlcDisplayCallback(" + data.ToInt32() + ") Exception] " + e.Message );
				}
			}
	
			private void AllowDisplay(IntPtr data)
			{
				//if ( pixelPlanes.LockBackBufferForWriting(0) ) {
				//	pixelPlanes.UnlockBackBuffer();
				pixelPlanes.ToggleFrontBack();
				//}
			}

			#endregion MediaRenderer Vlc Video Callback functions
			
			#region MediaRenderer Vlc Audio Callback functions

			public void VlcAudioPlayCallBack(ref IntPtr data, IntPtr samples, UInt32 count, Int64 pts) {
	//				Bass.BASS_SampleSetData(bassStreamHandle, samples);
			}
			
			
			#endregion MediaRenderer Vlc Audio Callback functions
	
			private void EvaluateThreadProc()
			{
				while ( initialized ) {
					int waitHandleIndex = WaitHandle.WaitAny( new EventWaitHandle[2] {
						evaluateEventWaitHandle,
						evaluateStopThreadWaitHandle
					} );
	
					if (waitHandleIndex == 0) {
						try {
							//Log( (evaluateCurrentActiveParameter ? "[signalled FRONT player] " : "[signalled BACK player] ") );
							UpdateMediaPlayerStatus_Threaded(null);
						} catch (Exception e) {
							Log( LogType.Error, "[EvaluateThreadProc] Something went terribly wrong: " + e.Message + "\n" + e.StackTrace);
						}
						//Thread.Sleep(2);
					} else if (waitHandleIndex == 1) {
						if ( mediaPlayerBusyMutex.WaitOne(10000) ) {
							if ( mediaPlayer != IntPtr.Zero ) {
								try {
									LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
								}
								catch { }
								try {
									LibVlcMethods.libvlc_media_player_release(mediaPlayer);
								}
								catch { }
							}
							mediaPlayer = IntPtr.Zero;
							
							mediaPlayerBusyMutex.ReleaseMutex();
						} else {
	
						}

						disposing = true;
						initialized = false;
						break;
					}
				}
				Log( LogType.Debug, "... exiting evaluate thread for renderer " + slice + " ... " );				
			}
	
			
			private DateTime prevNow = DateTime.Now;
			private DateTime theoreticalNow = DateTime.Now;
			private void SeekToLoopStart() {
				if ( parent.IsFrontMediaRenderer(this) ) {
					DateTime now = DateTime.Now;
					theoreticalNow = theoreticalNow.AddMilliseconds( currLoopLengthIn );

					if ( now.Subtract( theoreticalNow ).TotalMilliseconds < 0 ) {
						Log( LogType.Debug, "theoreticalNow is too far from Now, so set equal... " + now.Subtract( theoreticalNow ).TotalMilliseconds );
						theoreticalNow = now;
					}
					
					//Log( LogType.Debug, "i WILL seek " + now.Second + "." + now.Millisecond + "(diff=" + ( now.Subtract( prevNow ).TotalMilliseconds ) + " & " + ( now.Subtract( theoreticalNow ).TotalMilliseconds ) + ") in order to implement a decent loop ..." );
					
					mediaPlayerBusyMutex.WaitOne();
					LibVlcMethods.libvlc_media_player_set_time(mediaPlayer, (long)(currLoopStartIn * 1000));					
					mediaPlayerBusyMutex.ReleaseMutex();
					
					prevNow = now;
				}
			}
			
			private void LoopTimerEventHandler( Object myObject, EventArgs myEventArgs ) {
				if ( parent.IsFrontMediaRenderer(this) ) {
					SeekToLoopStart();

					int interval = (int)( currLoopLengthIn + theoreticalNow.Subtract( DateTime.Now ).TotalMilliseconds );
					
					//if ( currLoopIn ) {
						loopTimer.Interval = interval > 0 ? interval : (int)currLoopLengthIn;
					//}
					//loopTimer.Enabled = currLoopIn;
					
					Log( LogType.Debug, "currLoopLengthIn = " + currLoopLengthIn + " and new loopTimer interval = " + 
						//interval 
						loopTimer.Interval
					  );
				}
			}

			private void LoopTimerEventHandler2( Object state ) {
				SeekToLoopStart();
			}

			private void LoopTimerEventHandler3( Object sender, System.Timers.ElapsedEventArgs e ) {
				SeekToLoopStart();
			}
			
			private void VlcEventHandler(ref libvlc_event_t libvlc_event, IntPtr userData)
			{
				Log( LogType.Debug, "======== VLC SENT A " + libvlc_event.ToString() + " SIGNAL ======" );
				evaluateEventWaitHandle.Set();
			}
	
			private void Evaluate_Threaded(Boolean active)
			{
				//ONE METHOD WOULD BE TO DO SOME STUFF USING THE THREADPOOL
				//ThreadPool.QueueUserWorkItem( UpdateMediaPlayerStatus_Threaded, active );
	
				//BUT THE BETTER CHOICE I THINK IS TO SIGNAL A RUNNING THREAD
				// (because we can signal it at any time we need)
				evaluateEventWaitHandle.Set();
	
				//FOR TESTING -> NO THREADS
				//UpdateMediaPlayerStatus( );
	
			}
	
			private void UpdateMediaPlayerStatus_Threaded(object active)
			{
				if (mediaPlayerBusyMutex.WaitOne(5000)) {
					//ReportElapsedTime("locking mediaPlayerBusyMutex" );
	
					UpdateMediaPlayerStatus();
	
					mediaPlayerBusyMutex.ReleaseMutex();
					//ReportElapsedTime("releasing mediaPlayerBusyMutex" );
				} else {
					Log( LogType.Debug, "locking MediaPlayerBusyMutex FAILED!!!" );
				}
			}
	
			//private int test = 0;
			private bool isStream;
			private void UpdateMediaPlayerStatus()
			{
				//Log( LogType.Debug, (parent.IsFrontMediaRenderer(this) ? "FRONT " : "BACK ") + "[UpdateMediaPlayerStatus BEGIN] "  + StatusToString(preloadingStatus) + " " + currFileNameIn);
	
				libvlc_state_t mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
	
				int w = 2; //video width
				int h = 2; //video height
				int br = 48000; //audio bitrate
				int ch = 2; //audio nr of channels
	
				try {
					//stop player if in error
					if ( mpState == LibVlcWrapper.libvlc_state_t.libvlc_Error ) {
						Log( LogType.Debug, "LibVlc STATUS = " + LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) + " Trying to stop mediaPlayer... " + LibVlcMethods.libvlc_errmsg() );
						
						LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
	
						try {
							//Logging functions are obsolete... how to get more info on	the error?
							string logStr = "";
							int logCounter = 0;
							IntPtr libVlcLog = LibVlcMethods.libvlc_log_open( libVLC );
							IntPtr libVlcLogIterator = LibVlcMethods.libvlc_log_get_iterator( libVlcLog );
							while ( LibVlcMethods.libvlc_log_iterator_has_next( libVlcLogIterator ) == 0 ) {
								LibVlcWrapper.libvlc_log_message_t logMsg = new LibVlcWrapper.libvlc_log_message_t();
								LibVlcMethods.libvlc_log_iterator_next( libVlcLogIterator, ref logMsg );
																
								if ( logCounter == 0 && logMsg.psz_message != null ) {
									Int32 msgSize = (Int32)logMsg.sizeof_msg;
									Log( LogType.Debug, "Message has size " + logMsg.sizeof_msg + " severity " + logMsg.i_severity);							
									break; //logStr += Marshal.PtrToStringAuto( logMsg.psz_message, msgSize );
								}
								logCounter++;
							}
							LibVlcMethods.libvlc_log_iterator_free( libVlcLogIterator );
							LibVlcMethods.libvlc_log_close( libVlcLog );
	
							Log( LogType.Debug, logStr + "\nVlc Log contained " + logCounter + " messages." );
						}
						catch (Exception e) {
							Log( LogType.Debug, e.Message + "\n" + e.StackTrace );
						}
	
					}
					//then set everything right
	
//					if ( currFileNameIn.Length == 0 ) {
//						readyForPlaying = false;
//						if (	LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Playing 
//							 || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Paused 
//							 || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Ended 
//							 || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Error) {
//							
//							Log( LogType.Debug, "Filename empty, STOP mediaPlayer" + (this == parent.mediaRendererA ? "A " : "B ") + (this == parent.mediaRendererCurrent[slice] ? "(FRONT) " : "(BACK) " ) + currFileNameIn );
//							LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
//							Log( LogType.Debug, ( LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Stopped ? "STOPPED!!!" : "" ) );
//						}
//					}
//					else

					if ((currFileNameIn != null && currFileNameIn != prevFileNameIn) && (prevFileNameIn == null || prevFileNameIn.CompareTo(currFileNameIn) != 0)) {
						newFileNameIn = string.Copy(currFileNameIn);
						if ( parent.IsFrontMediaRenderer(this) ) {
							preloadingStatus = STATUS_NEWFILE;
						}
						else if ( preloadingStatus != STATUS_WAITING ) {
							//If not front player, wait a bit to give others the time to load!
							statusWaitingUntilTicks = DateTime.Now.AddTicks( MillisecondsToTicks( 200 + (50 * slice) ) ).Ticks;
							
							preloadingStatus = STATUS_WAITING;
						}
					} else if (currFileNameIn == null) {
						Log( LogType.Error, "[UpdateMediaPlayerStatus Exception] currFileNameIn == null" );
					}
	
					mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
	
					if (preloadingStatus == STATUS_OPENINGFILE && newFileNameIn.Length > 0) {
						Log( LogType.Debug, "		(preloadingStatus == STATUS_OPENINGFILE && newFileNameIn.Length > 0)" );
					}
					if ( preloadingStatus == STATUS_WAITING ) {
						
						currentFrame = 0;
						videoLength = 0;
						readyForPlaying = false;
						prevFileNameIn = newFileNameIn;
	
						if ( DateTime.Now.Ticks >= statusWaitingUntilTicks ) {
							preloadingStatus = STATUS_NEWFILE;
						}
						//else {
						//	Log( LogType.Debug, "Still waiting..." + DateTime.Now.Ticks + " < " + statusWaitingUntilTicks );
						//}
					}
					if ( preloadingStatus == STATUS_NEWFILE ) {
						//Log( LogType.Debug, "Trying to load " + newFileNameIn + "..." );
						try {
	
							currentFrame = 0;
							videoLength = 0;
							readyForPlaying = false;
							prevFileNameIn = newFileNameIn;
	
							if ( mpState == libvlc_state_t.libvlc_Opening || mpState == libvlc_state_t.libvlc_Playing || mpState == libvlc_state_t.libvlc_Paused || mpState == libvlc_state_t.libvlc_Ended || mpState == libvlc_state_t.libvlc_Error) {
								//Log( LogType.Debug, "Calling STOP first" );
								LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
								//Log( LogType.Debug, "STOPPED..." );
							}
							if ( mpState == libvlc_state_t.libvlc_NothingSpecial ) {
								Log( LogType.Debug, "STOP Mediaplayer (Nothing Special)" );
								LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
							}
							if ( // mpState != libvlc_state_t.libvlc_NothingSpecial && 
							     mpState != libvlc_state_t.libvlc_Stopped ) {
								Log( LogType.Debug, "Mediaplayer not stopped yet (" + LibVlcPlayerStatusToString(mpState) + "), so we will wait a little bit before we try to load a new file" );
								//Log( LogType.Debug, "Not calling STOP first (new filename) because state = " + LibVlcPlayerStatusToString(mpState) );
								return;
							}
	
						} catch (Exception e) {
							Log( LogType.Error, "[UpdateMediaPlayerStatus PRELOAD Exception 1] " + e.Message);
						}
						try {
							mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
							//Log( LogType.Debug, "state = " + mpState );
							if ( // mpState == libvlc_state_t.libvlc_NothingSpecial || 
							     mpState == libvlc_state_t.libvlc_Stopped ) {
	
								if ( currFileNameIn.Length == 0 ) {
									Log( LogType.Debug, "FileName is empty, DEACTIVATING media player." );
									preloadingStatus = STATUS_INACTIVE;
									readyForPlaying = true;
								}
								else if ( IsImageFileName(newFileNameIn) ) {
									Log( LogType.Debug, "Trying to load image '" + newFileNameIn + "'" );
									LoadImage(newFileNameIn);
									preloadingStatus = STATUS_IMAGE;
								} 
								else {
									Log( LogType.Debug, "Trying to load VIDEO '" + newFileNameIn + "'" );
	
									//example: c:\video.avi | video-filter=adjust {           hue=120 ,          gamma=2.} | video-filter=gradient{type=1}
									//         filename     | option              {optionflagname=optionflagvalue, ...   }
									preloadMedia = ParseFilename(newFileNameIn);
	
									string[] tmp = newFileNameIn.Split("|".ToCharArray());
									isStream = tmp.Length > 0 && tmp[0].Length > 0 && tmp[0].Contains("://" );
									//isStream = false;
	
									if ( preloadMedia != IntPtr.Zero ) {
										//only get the file's description without actually playing it
										if ( ! isStream ) {
											LibVlcMethods.libvlc_media_parse( preloadMedia );
											//file parsed, check if streams found
											IntPtr trackInfoArray;
											if ( 0 == LibVlcMethods.libvlc_media_get_tracks_info(preloadMedia, out trackInfoArray) ) {
												//no streams found, fallback to OLD way of finding trackinfo
												isStream = true;
												Log( LogType.Debug, "Detecting tracks (by parsing the file) for file " + newFileNameIn + " failed. Fallback to old detection mechanism." );
											}
											else {
												Marshal.DestroyStructure(trackInfoArray, typeof(LibVlcWrapper.libvlc_media_track_info_t*));
											}

										}
															
										if ( isStream ) {
											LibVlcMethods.libvlc_media_add_option( preloadMedia, Encoding.UTF8.GetBytes("sout=#description:dummy") );
	
											LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, preloadMedia);
											LibVlcMethods.libvlc_media_player_play(mediaPlayer);
										}
										
										//Log( LogType.Debug, "SETTING STATUS_GETPROPERTIES" );
										preloadingStatus = STATUS_GETPROPERTIES;
									}
									else {
										Log( LogType.Debug, "Error opening file: " + newFileNameIn );
										preloadingStatus = STATUS_INACTIVE;
									}
								}
							} 
						} catch (Exception e) {
							Log( LogType.Error, "[UpdateMediaPlayerStatus PRELOAD Exception 2] " + e.Message);
						}
					} 
					
					if ( (preloadingStatus == STATUS_GETPROPERTIES)
	//							&& ( 
	//									(mpState == libvlc_state_t.libvlc_Ended) || (mpState == libvlc_state_t.libvlc_Stopped)
	//									|| (isStream && (mpState == libvlc_state_t.libvlc_Playing) ) 
	//								)
							) {
						//Log( LogType.Debug, "STATUS_GETPROPERTIES" );
						try {
							IntPtr trackInfoArray;
							int nrOfStreams;
							unsafe {
								nrOfStreams = LibVlcMethods.libvlc_media_get_tracks_info(preloadMedia, out trackInfoArray);
								if ( nrOfStreams == 0 ) {
									//preloadingStatus = STATUS_OPENINGFILE;
									if (LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Buffering) {
										Log( LogType.Debug, "=== BUFFERING" );
									} else if (LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Opening) {
										Log( LogType.Debug, "=== OPENING" );
									}
								}
								if ( nrOfStreams > 0 && isStream ) { //&& newFileNameIn.StartsWith("dvb-")
									//not all streams end up in the structure at the same time
									if ( nrOfStreams == 1 ) {
										Log( LogType.Debug, "Stream detected: wait some time to see if there are more streams..." );
										Thread.Sleep(3000);
									}
									//and check again
									nrOfStreams = LibVlcMethods.libvlc_media_get_tracks_info(preloadMedia, out trackInfoArray);
								}
	
								bool hasAudio = false;
								bool hasVideo = false;
								if ( LibVlcMethods.libvlc_media_is_parsed( preloadMedia ) )
									Log( LogType.Debug, "streams " + nrOfStreams + " trackInfo size = " + sizeof(LibVlcWrapper.libvlc_media_track_info_t) );
								
								string logString = "";
	
								for (int i = 0; i < nrOfStreams; i++) {
									LibVlcWrapper.libvlc_media_track_info_t trackInfo = ((LibVlcWrapper.libvlc_media_track_info_t*)trackInfoArray)[i];
	
	
									if (trackInfo.i_type == LibVlcWrapper.libvlc_track_type_t.libvlc_track_audio) {
										br = trackInfo.audio.i_rate;
										ch = trackInfo.audio.i_channels;
										hasAudio = true;
										logString += "AUDIO(" + trackInfo.audio.i_rate + "x" + trackInfo.audio.i_channels + ") ";
										//Log( LogType.Debug, "Detected AUDIO track with samplerate " + trackInfo.audio.i_rate + " and " + trackInfo.audio.i_channels + " channels" );
									} else if (!hasVideo && trackInfo.i_type == LibVlcWrapper.libvlc_track_type_t.libvlc_track_video) {
										//setting w+h is important !!!
										w = trackInfo.video.i_width;
										h = trackInfo.video.i_height;
										hasVideo = true;
	
										logString += "VIDEO(" + trackInfo.video.i_width + "x" + trackInfo.video.i_height + ") ";
	
										//Log( LogType.Debug, "Detected VIDEO track with size " + w + "x" + h);
									} else if (trackInfo.i_type == LibVlcWrapper.libvlc_track_type_t.libvlc_track_text) {
										logString += "TEXT(" + trackInfo.video.i_width + "x" + trackInfo.video.i_height + ") ";
										//Log( LogType.Debug, "Detected TEXT track with size " + trackInfo.video.i_width + "x" + trackInfo.video.i_height);
									} else if (trackInfo.i_type == LibVlcWrapper.libvlc_track_type_t.libvlc_track_unknown) {
										logString += "UNKNOWN(" + trackInfo.video.i_width + "x" + trackInfo.video.i_height + ", " + ") ";
										//Log( LogType.Debug, "Detected UNKNOWN track with size " + trackInfo.video.i_width + "x" + trackInfo.video.i_height);
									} else {
										Log( LogType.Debug, "Detected UNSUPPORTED track with size " + trackInfo.video.i_width + "x" + trackInfo.video.i_height);
									}
								}
	
								if (nrOfStreams > 0) {
									Marshal.DestroyStructure(trackInfoArray, typeof(LibVlcWrapper.libvlc_media_track_info_t*));
	
									if (logString.Length > 0) {
										Log( LogType.Debug, "Detected tracks: " + logString + " for file " + newFileNameIn);
									}
								}
	
								if (hasAudio || hasVideo) {
									try { LibVlcMethods.libvlc_media_release(media); } catch {}
									try { LibVlcMethods.libvlc_media_release(preloadMedia); } catch {}
									
									preloadMedia = ParseFilename(newFileNameIn);
									media = ParseFilename(newFileNameIn);
									if (media != IntPtr.Zero) {
										if (hasAudio && !hasVideo) {
											//Log( LogType.Debug, "AUDIO only -> start playing" );
	
											currentFrame = 0;
											videoLength = 0;
											//videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_player_get_length(mediaPlayer) ) / 1000;
											//videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_get_duration( preloadMedia ) ) / 1000;
											videoFps = -1;
	
											LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
	
											try {
												UpdateAudioFormat( br, ch );
											} catch (Exception e) {
												Log( LogType.Error, "[UpdateMediaPlayerStatus UpdateAudioFormat (audio only) Exception] " + e.Message);
											}
											try {
												UpdateVideoFormat(2, 2);
											} catch (Exception e) {
												Log( LogType.Error, "[UpdateMediaPlayerStatus UpdateVideoFormat (audio only) Exception] " + e.Message);
											}
	
											//parent.currentFillTextureFunction = TransparentFillTexure;
											lockCalled = 0;
											unlockCalled = 0;
											displayCalled = 0;
											//reset "frames drawn"
											//Log( LogType.Debug, "Calling PLAY (after getting properties the right way)" );
											UpdateVolume();
	
											if (currPlayIn) {
												try {
													LibVlcMethods.libvlc_media_release(preloadMedia);
												} catch {
												}
												// ! clean up
												LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, media);
												LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer, 0);
												LibVlcMethods.libvlc_media_player_play(mediaPlayer);
												//Log( LogType.Debug, "SETTING STATUS_PLAYING" );
												preloadingStatus = STATUS_PLAYING;
											} else {
												LibVlcMethods.libvlc_media_add_option(preloadMedia, Encoding.UTF8.GetBytes("no-audio") );
												//dshow-adev=none
												LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, preloadMedia);
	
												LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer, 1);
												LibVlcMethods.libvlc_media_player_play(mediaPlayer);
												//Log( LogType.Debug, "SETTING STATUS_READY" );
												preloadingStatus = STATUS_READY;
											}
										} else if (hasVideo) {
											//Log( LogType.Debug, "VIDEO " + (hasAudio ? "(+ AUDIO)" : "") + "-> start playing! " + LibVlcMethods.libvlc_video_get_aspect_ratio(mediaPlayer));
											//string ar = LibVlcMethods.libvlc_video_get_aspect_ratio( mediaPlayer );
											//Log( LogType.Debug, "video.width = " + videoWidth + " height = " + videoHeight  + " ar = " + ar + " scale = " + LibVlcMethods.libvlc_video_get_scale( mediaPlayer ) );
	
											currentFrame = 0;
											videoLength = 0;
											//videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_player_get_length(mediaPlayer) ) / 1000;
											//videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_get_duration( preloadMedia ) ) / 1000;
											videoFps = LibVlcMethods.libvlc_media_player_get_fps(mediaPlayer);
											//Log( LogType.Debug, "video length = " + videoLength + " fps=" + videoFps);
	
											LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
	
											LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, media);
	
											LibVlcMethods.libvlc_video_set_callbacks(mediaPlayer, Marshal.GetFunctionPointerForDelegate(vlcVideoLockHandlerDelegate), Marshal.GetFunctionPointerForDelegate(vlcVideoUnlockHandlerDelegate), Marshal.GetFunctionPointerForDelegate(vlcVideoDisplayHandlerDelegate), opaqueForCallbacks);
											
											//LibVlcMethods.libvlc_audio_set_format_callbacks
											//LibVlcMethods.libvlc_audio_set_callbacks(mediaPlayer, Marshal.GetFunctionPointerForDelegate(vlcAudioPlayDelegate), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, opaqueForCallbacks);
	
											
											try {
												if ( hasAudio ) {
													UpdateAudioFormat( br, ch );
												}
											} catch (Exception e) {
												Log( LogType.Error, "[UpdateMediaPlayerStatus UpdateAudioFormat Exception] " + e.Message);
											}
	
											
											try {
												//Log( LogType.Debug, "try to update video size..." );
	
												Size newVideoSize = GetWantedSize(w, h);
												//Log( LogType.Debug, "try to update video size to " + newVideoSize.Width + "x" + newVideoSize.Height);
												UpdateVideoFormat(newVideoSize.Width, newVideoSize.Height);
	
												//Log( LogType.Debug, "finished update video size..." );
											} catch (Exception e) {
												Log( LogType.Error, "[UpdateMediaPlayerStatus UpdateVideoFormat Exception] " + e.Message);
											}
	
											//UpdateRotation();
											lockCalled = 0;
											unlockCalled = 0;
											displayCalled = 0;
											//reset "frames drawn"
											//Log( LogType.Debug, "Calling PLAY -> getfirstframe (after getting properties the right way)" );
											if (currPlayIn) {
												try {
													LibVlcMethods.libvlc_media_release(preloadMedia);
												} catch {
												}
												// ! clean up
												LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, media);
												LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer, 0);
												LibVlcMethods.libvlc_media_player_play(mediaPlayer);
												//Log( LogType.Debug, "SETTING STATUS_PLAYING" );
												preloadingStatus = STATUS_PLAYING;
											}
											else {
												LibVlcMethods.libvlc_media_add_option(preloadMedia, Encoding.UTF8.GetBytes("no-audio") ); //dshow-adev=none
												LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, preloadMedia);
	
												LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer, 1);
												LibVlcMethods.libvlc_media_player_play(mediaPlayer);
	
												//Log( LogType.Debug, "SETTING STATUS_GETFIRSTFRAME" );
												preloadingStatus = STATUS_GETFIRSTFRAME;
											}
											//LibVlcMethods.libvlc_media_player_next_frame(mediaPlayer);
										}
									}
								}
							}
						} catch (Exception e) {
							Log( LogType.Error, "[UpdateMediaPlayerStatus GetProperties Exception] " + e.Message);
						}
					} 
					// DEBUG
//					else if ( preloadingStatus == STATUS_GETPROPERTIES ) {
//						libvlc_state_t state = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
//						string stateDescription = "unknown";
//						switch (state) {
//							case libvlc_state_t.libvlc_Buffering: stateDescription = "buffering"; break;
//							case libvlc_state_t.libvlc_Ended: stateDescription = "ended"; break;
//							case libvlc_state_t.libvlc_Error: stateDescription = "error"; break;
//							case libvlc_state_t.libvlc_NothingSpecial: stateDescription = "nothing special"; break;
//							case libvlc_state_t.libvlc_Opening: stateDescription = "opening"; break;
//							case libvlc_state_t.libvlc_Paused: stateDescription = "paused"; break;
//							case libvlc_state_t.libvlc_Playing: stateDescription = "playing"; break;
//							case libvlc_state_t.libvlc_Stopped: stateDescription = "stopped"; break;
//						}
//						Log( LogType.Debug, "STATUS_GETPROPERTIES but libvlc_media_player_get_state != ended or playing. It's " + stateDescription);
//					}
					
					else if (preloadingStatus == STATUS_GETFIRSTFRAME) {
						//Log( LogType.Debug, "STATUS_GETFIRSTFRAME: set to ready in VlcCallback functions!!!" );
					} 
					else if (preloadingStatus == STATUS_READY) {
						//Log( LogType.Debug, "STATUS_READY" );
						//at this stage we have been playing preloadMedia with the "noaudio" option, so set the media to media here!!!
						try {
							readyForPlaying = true;
	
							if (LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Playing || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Paused) {
	
								LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer, 1);
	
								if (prevDisplayCalled != displayCalled) {
									prevDisplayCalled = displayCalled;
								}
	
								if (currPlayIn) {
									//Log( LogType.Debug, "Still on pause after getting first frame, but now we want to start playing !!!" );
									try { LibVlcMethods.libvlc_media_release(preloadMedia); } catch {}
									// ! clean up
									LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, media);
	
									currentFrame = 0;
									lockCalled = 0;
									unlockCalled = 0;
									displayCalled = 0;
									//reset "frames drawn"
									UpdateVolume();
									UpdateSpeed();
	
									LibVlcMethods.libvlc_media_player_play(mediaPlayer);
									preloadingStatus = STATUS_PLAYING;
	
									LibVlcMethods.libvlc_audio_set_mute(mediaPlayer, false);
									UpdateVolume();
									UpdateSpeed();
								}
							}
						} catch (Exception e) {
							Log( LogType.Error, "[UpdateMediaPlayerStatus READY FOR PLAYING Exception] " + e.Message);
						}
					}
	
	
					if (parent.IsFrontMediaRenderer(this) && ( preloadingStatus == STATUS_PLAYING ) ) {
	
						//Log( LogType.Debug, "STATUS_PLAYING" );
						try {
							libvlc_state_t mediaPlayerState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
							if (mediaPlayerState == libvlc_state_t.libvlc_Playing) {
								if (prevDisplayCalled != displayCalled) {
									prevDisplayCalled = displayCalled;
								}
							} else if (mediaPlayerState == libvlc_state_t.libvlc_Paused) {
								displayCalled = prevDisplayCalled;
							}
	
							if ((currPlayIn && mediaPlayerState == libvlc_state_t.libvlc_Paused) || (!currPlayIn && mediaPlayerState == libvlc_state_t.libvlc_Playing)) {
	
								LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer, currPlayIn ? 0 : 1);
							}
	
							if (mediaPlayerState == libvlc_state_t.libvlc_Paused || mediaPlayerState == libvlc_state_t.libvlc_Playing || mediaPlayerState == libvlc_state_t.libvlc_Ended) {
	
								if (currDoSeekIn) {
									//float relativePosition = currSeekTimeIn * 1000 / LibVlcMethods.libvlc_media_get_duration(media);
	
									currentFrame = (int)(currSeekTimeIn * videoFps);
									lockCalled = currentFrame;
									unlockCalled = currentFrame;
									displayCalled = currentFrame;
	
									//Log( LogType.Debug, "Seeking to position RELATIVE = " + (currSeekTimeIn*1000) + "/" + LibVlcMethods.libvlc_media_get_duration(media) + "=" + relativePosition + " ABSOLUTE = " + currSeekTimeIn + (0 == LibVlcMethods.libvlc_media_player_is_seekable(mediaPlayer) ? " NOT seekable" : "") );
									if ( LibVlcMethods.libvlc_media_player_is_seekable(mediaPlayer) ) {
										if (mediaPlayerState == libvlc_state_t.libvlc_Ended) {
											LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
											LibVlcMethods.libvlc_media_player_play(mediaPlayer);
											UpdateVolume();
											UpdateSpeed();
										}
										//LibVlcMethods.libvlc_media_player_set_position(mediaPlayer, relativePosition );
										LibVlcMethods.libvlc_media_player_set_time(mediaPlayer, (long)(currSeekTimeIn * 1000));
									}
									currDoSeekIn = false;
								}
	
								UpdateVolume();
								UpdateSpeed();
							}
	
	
							// || mediaPlayerState == libvlc_state_t.libvlc_Playing
							if ( ( mediaPlayerState == libvlc_state_t.libvlc_Ended ) && currLoopIn ) {
								LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
								currentFrame = 0;
								lockCalled = 0;
								unlockCalled = 0;
								displayCalled = 0;
								LibVlcMethods.libvlc_media_player_play(mediaPlayer);
								UpdateVolume();
								UpdateSpeed();
								//LibVlcMethods.libvlc_media_player_set_position( mediaPlayer, 0 );
							}
	
							//LibVlcMethods.libvlc_video_set_int(mediaPlayer, "adjust", null, 1);
							//if ( test++ > 100) { test = 0; }
							//LibVlcMethods.libvlc_video_set_int(mediaPlayer, "adjust", "hue", test);
						} catch (Exception e) {
							Log( LogType.Error, "[UpdateMediaPlayerStatus PLAYING Exception] " + e.Message);
						}
					}
	
				} catch (Exception e) {
					Log( LogType.Error, "[UpdateMediaPlayerStatus Exception] " + e.Message);
				}
	
				//Log( LogType.Debug, (parent.IsFrontMediaRenderer(this) ? "FRONT " : "BACK ") + "[UpdateMediaPlayerStatus END] "  + StatusToString(preloadingStatus) + " " + newFileNameIn);
	
			}
	
			private void UpdateParent(bool active)
			{
				try {
					//if (active) Log( LogType.Debug, "current status = " + StatusToString(preloadingStatus) );
					
					if (active && ( preloadingStatus == STATUS_PLAYING || preloadingStatus == STATUS_READY) ) {
						if (mediaPlayerBusyMutex.WaitOne(0)) {
							try {
								libvlc_state_t mediaPlayerState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
	
								if ( mediaPlayerState == libvlc_state_t.libvlc_Playing || mediaPlayerState == libvlc_state_t.libvlc_Paused || mediaPlayerState == libvlc_state_t.libvlc_Ended ) {
	
									try {
										if ( videoLength <= 0 ) {
                                            long duration = LibVlcMethods.libvlc_media_get_duration(media);
                                            if (duration >= 0) {
                                                //videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_player_get_length(mediaPlayer) ) / 1000.0f;
                                                videoLength = Convert.ToSingle(duration) / 1000.0f;
                                            }
										}
										videoFps = LibVlcMethods.libvlc_media_player_get_fps( mediaPlayer );
										float absolutePosition;
										if ( videoFps == 0 ) {
											videoFps = -1;
											//float relativePosition = currentFrame / videoFps / ( (float)LibVlcMethods.libvlc_media_player_get_time(mediaPlayer) / 1000 ); //LibVlcMethods.libvlc_media_player_get_position(mediaPlayer);
											absolutePosition = Convert.ToSingle( LibVlcMethods.libvlc_media_player_get_time(mediaPlayer) ) / 1000;
										}
										else {
											absolutePosition = currentFrame / videoFps;
										}
										parent.FPositionOut[slice] = absolutePosition;
										//Log( LogType.Debug, "setting FPositionOut " + videoLength + " * " + LibVlcMethods.libvlc_media_player_get_position( mediaPlayer ) + " @" + videoFps + "fps => position = " + absolutePosition);
										parent.FDurationOut[slice] = videoLength;
										parent.FFrameOut[slice] = currentFrame; //Convert.ToInt32(absolutePosition * videoFps);
										parent.FFrameCountOut[slice] = Convert.ToInt32(videoLength * videoFps);
									} catch (Exception e) {
										Log( LogType.Error, "[UpdateParent (position) Exception] " + e.Message);
									}
								}
								UpdateOutput_TextureInfo();
	
							} catch (Exception e) {
								Log( LogType.Error, "[UpdateParent Exception] " + e.Message);
							}
							mediaPlayerBusyMutex.ReleaseMutex();
						} else {
							//Log( LogType.Warning, "[UpdateParent] Media Player Busy" );
						}
					} else if (active && preloadingStatus == STATUS_IMAGE) {
						if (mediaPlayerBusyMutex.WaitOne(0)) {
							try {
								parent.FPositionOut[slice] = 0;
								parent.FDurationOut[slice] = 0;
								parent.FFrameOut[slice] = 1;
								parent.FFrameCountOut[slice] = 1;
	
								UpdateOutput_TextureInfo();
							} catch (Exception e) {
								Log( LogType.Error, "[UpdateParent Exception] " + e.Message);
							}
							mediaPlayerBusyMutex.ReleaseMutex();
						} else {
							//Log( LogType.Warning, "[UpdateParent] Media Player Busy" );
						}
					} else if (!active) {
						parent.FNextReadyOut[slice] = readyForPlaying;
					}
	
	//						if ( currRotateIn ) {
	//							if (videoWidth > 0 && videoHeight > 0)
	//								UpdateRotation();
	//						}
				} catch (Exception e) {
					Log( LogType.Error, "[UpdateParent Exception] " + e.Message);
				}
	
			}
	
			private void Log( LogType logType, string message)
			{
				parent.Log( logType, "[MediaRenderer " +  ( initialized ? (this == parent.mediaRendererA[slice] ? "A" : "B")  : "D" ) + slice + ( initialized ? ( parent.IsFrontMediaRenderer(this) ? "+" : "-" ) : "*" ) + "] " + message);
			}
	
			private long MillisecondsToTicks(int millis) {
				//1 tick = 100 nanoseconds = 100 * 10^-9 seconds = 10^-7 seconds 
				//=> 1 millisecond = 10^-3 seconds = 10^4 * 10^-7 seconds = 10000 ticks
				return 10000 * millis;
			}
			
			private string StatusToString(int status)
			{
				if (status == STATUS_INACTIVE) {
					return "INACTIVE";
				}
				if (status == STATUS_NEWFILE) {
					return "NEWFILE";
				}
				if (status == STATUS_OPENINGFILE) {
					return "OPENINGFILE";
				}
				if (status == STATUS_GETPROPERTIES) {
					return "GETPROPERTIES";
				}
				if (status == STATUS_GETPROPERTIESOK) {
					return "STATUS_GETPROPERTIESOK";
				}
				if (status == STATUS_GETFIRSTFRAME) {
					return "GETFIRSTFRAME";
				}
				if (status == STATUS_READY) {
					return "READY";
				}
				if (status == STATUS_PLAYING) {
					return "PLAYING";
				}
				return "UNKNOWN";
			}
	
			private string LibVlcPlayerStatusToString(libvlc_state_t status)
			{
				if (status == libvlc_state_t.libvlc_Buffering) {
					return "BUFFEREING";
				}
				if (status == libvlc_state_t.libvlc_Ended) {
					return "ENDED";
				}
				if (status == libvlc_state_t.libvlc_Error) {
					return "ERROR";
				}
				if (status == libvlc_state_t.libvlc_NothingSpecial) {
					return "NOTHING SPECIAL";
				}
				if (status == libvlc_state_t.libvlc_Opening) {
					return "OPENING";
				}
				if (status == libvlc_state_t.libvlc_Paused) {
					return "PAUSED";
				}
				if (status == libvlc_state_t.libvlc_Playing) {
					return "PLAYING";
				}
				if (status == libvlc_state_t.libvlc_Stopped) {
					return "STOPPED";
				}
				return "UNKNOWN";
			}
	
			//TODO
			private void UpdateColorSettings()
			{
				//brightness, contrast, hue, saturation, ...
				//LibVlcMethods.libvlc_video_set_adjust_float(mediaPlayer, libvlc_video_adjust_option_t.libvlc_adjust_Brightness, currBrightnessIn);
			}
	
			private void UpdateSpeed()
			{
				//Log( LogType.Debug, "Setting SPEED to " + parent.FSpeedIn[mediaPlayerIndex] );
				LibVlcMethods.libvlc_media_player_set_rate(mediaPlayer, currSpeedIn);
			}
	
			private void UpdateVolume()
			{
				//Log( LogType.Debug, "Setting Volume to " + Convert.ToInt32( Math.Pow ( Math.Max ( Math.Min( FVolumeIn[slice], 1), 0 ), Math.E ) * 100 ) );
				LibVlcMethods.libvlc_audio_set_volume(mediaPlayer, Convert.ToInt32(currVolumeIn * 100) ); // Convert.ToInt32(Math.Pow(Math.Max(Math.Min(currVolumeIn, 2), 0), Math.E) * 100)
			}
	
			//also updates OUTPUT aspect ratio's and texture width and height
			private void UpdateRotation(bool active)
			{
			}
	
			private void UpdateVideoFormat(int newWidth, int newHeight)
			{
				try {
					if ( ! pixelPlanes.SetNewSize(newWidth, newHeight) ) {
						throw new Exception("pixelPlanes.SetNewSize(" + newWidth + "," + newHeight + ") FAILED !" );
					}
	
					videoWidth = newWidth;
					videoHeight = newHeight;
	
					//UpdateOutput_TextureInfo();
	
					//"RV32" = RGBA I think, "RV24"=RGB
					int pitch = videoWidth * 4;
					//depends on pixelformat ( = width * nrOfBytesPerPixel) !!!
					LibVlcMethods.libvlc_video_set_format(mediaPlayer, Encoding.UTF8.GetBytes("RV32"), videoWidth, videoHeight, pitch);
				} catch (Exception e) {
					Log( LogType.Error, "[UpdateVideoFormat Exception] " + e.Message);
				}
	
				//Log( LogType.Debug, "[Update Video Size] " + newWidth + "x" +  newHeight + " done!" );				
			}
	
			private void UpdateAudioFormat(int newSampleRate, int newNrOfChannels)
			{
//					try {
//					LibVlcMethods.libvlc_audio_set_format( mediaPlayer, Encoding.UTF8.GetBytes("WAV"), newSampleRate, newNrOfChannels );
//					
//					if ( bassStreamHandle != 0) {
//						Bass.BASS_StreamFree( bassStreamHandle );
//						bassStreamHandle = 0;
//					}
//					
//					bassStreamHandle = Bass.BASS_StreamCreatePush( newSampleRate, newNrOfChannels, BASSFlag.BASS_DEFAULT, new IntPtr() );
//					
//					Log( LogType.Debug, "[UpdateAudioFormat] bass handle = " + bassStreamHandle );
//				} catch (Exception e) {
//					Log( LogType.Error, "[UpdateAudioFormat Exception] " + e.Message);
//				}
//	
//				//Log( LogType.Debug, "[UpdateAudioFormat] " + newSampleRate + "x" +  newNrOfChannels + " done!" );

			}
	
			unsafe private void UpdateOutput_TextureInfo()
			{
	//				if ( parent.currentFillTextureFunction == parent.FillTexure || parent.currentFillTextureFunction == parent.Rotate180FillTexure ) {
				parent.FWidthOut[Slice] = VideoWidth;
				parent.FHeightOut[Slice] = VideoHeight;
				parent.FTextureAspectRatioOut[Slice] = (float)VideoWidth / (float)VideoHeight;
				//TODO
				parent.FPixelAspectRatioOut[Slice] = 1f;
	//				}
	//				else if ( parent.currentFillTextureFunction == parent.RotateLeftFillTexure || parent.currentFillTextureFunction == parent.RotateRightFillTexure ) {
	//					parent.FWidthOut[Slice] = VideoHeight;
	//					parent.FHeightOut[Slice] = VideoWidth;
	//					parent.FTextureAspectRatioOut[Slice] = (float)VideoHeight / (float)VideoWidth;
	//					//TODO
	//					parent.FPixelAspectRatioOut[Slice] = 1.0F;
	//				}
			}
	
			protected string GetFullPath(string path)
			{
				return parent.GetFullPath(path);
			}
	
			private IntPtr ParseFilename(string fileName)
			{
				//Log( LogType.Debug, "ParseFilename( " + fileName + " )" );
				if (fileName.Length == 0) {
					return IntPtr.Zero;
				}
	
				string[] mediaOptions = fileName.Split("|".ToCharArray());
				if ( mediaOptions[0].TrimEnd().Length == 0 || ( ! mediaOptions[0].Contains("://") && ! File.Exists( mediaOptions[0].TrimEnd() ) ) ) {
					return IntPtr.Zero;
				}
	
				IntPtr retVal = new IntPtr();
				try {
					if ( ! mediaOptions[0].Contains("://") ) {
						retVal = LibVlcMethods.libvlc_media_new_path( libVLC, Encoding.UTF8.GetBytes(mediaOptions[0].TrimEnd()) );						
					}
					else {
						retVal = LibVlcMethods.libvlc_media_new_location( libVLC, Encoding.UTF8.GetBytes(mediaOptions[0].TrimEnd()) );						
					}
					for (int moIndex = 1; moIndex < mediaOptions.Length; moIndex++) {
						LibVlcMethods.libvlc_media_add_option( retVal, Encoding.UTF8.GetBytes(mediaOptions[moIndex].Trim()) );
					}
					
//					string[] mediaOptionParts = mediaOptions[moIndex].Trim().Split("{".ToCharArray());
//					LibVlcMethods.libvlc_media_add_option( media, mediaOptionParts.Trim() );
//	
//					if ( mediaOptionParts.Length == 2) {
//						string[] flags = mediaOptionParts[1].Replace("}", "").Split(",".ToCharArray());
//						for (int flagIndex = 1; flagIndex < mediaOptionParts.Length; flagIndex++ ) {
//							string[] flagParts = flags[flagIndex].Split("=".ToCharArray());
//							if ( flagParts.Length == 2) {
//								Log( LogType.Debug, "adding option " + flagParts + " = " + flagParts[1] );
//								LibVlcMethods.libvlc_media_add_option_flag( media, flagParts, LibVlcWrapper.libvlc_video_adjust_option_t .libvlc_media_option_trusted ); //Convert.ToInt32(flagParts[1])
//							}
//							else {
//								Log( LogType.Debug, "Something strange when parsing filename options..." );
//							}
//						}
//					}

				}				 catch {
					retVal = IntPtr.Zero;
				}
	
				return retVal;
			}
	
			private bool IsImageFileName(string fileName)
			{
				try {
					if ( fileName.Contains("|") ) {
						return false;
					}
	
					string ext = Path.GetExtension(fileName).ToLower();
					bool retVal = (!fileName.Contains("|")) && (ext.CompareTo(".png") == 0 || ext.CompareTo(".gif") == 0 || ext.CompareTo(".bmp") == 0 || ext.CompareTo(".tif") == 0 || ext.CompareTo(".tiff") == 0 || ext.CompareTo(".jpg") == 0 || ext.CompareTo(".jpeg") == 0);
	
					//Log( LogType.Debug, "[IsImagefileName] Checking if '" + fileName + "' with extension '" + ext + "' is an image... " + (retVal ? "YES" : "NO"));
	
					return retVal;
				} catch (Exception e) {
					Log( LogType.Error, "[IsImageFileName] exception (for " + fileName + "): " + e.Message);
				}
				return false;
			}
	
			public void LoadImage(string path)
			{
				try {
					Image image = Image.FromFile(path);
					Size newSize = GetWantedSize(image.Width, image.Height);
	
					//lock as short as possible!
					//Log( LogType.Debug, "[LoadImage] LOCKING before UpdateVideoFormat" );
					try {
						//Log( LogType.Debug, "[LoadImage] start UpdateVideoFormat" );
						UpdateVideoFormat(newSize.Width, newSize.Height);
						//Log( LogType.Debug, "[LoadImage] stop UpdateVideoFormat" );
					} catch (Exception e) {
						Log( LogType.Error, "[LoadImage Exception] " + " UpdateVideoFormat " + e.Message);
					}
	
					//Graphics objects can not be created from bitmaps with an Indexed Pixel Format, use RGB instead.
					Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppArgb);
					Graphics canvas = Graphics.FromImage(newImage);
					canvas.SmoothingMode = SmoothingMode.AntiAlias;
					canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
					canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
					canvas.DrawImage(image, new Rectangle(new Point(0, 0), newSize));
	
					//readPixelPlane = newImage.GetHbitmap();
	//					unsafe {
	//						Marshal.Copy( (int[])(newImage.GetHbitmap()), 0, readPixelPlane, newSize.Width * newSize.Height);
					//Marshal.CreateWrapperOfType(readPixelPlane, typeof(int[]));
	//					}
	
					bool locked;
					locked = pixelPlanes.LockBackBufferForWriting( 3000 );
					
					if ( locked ) {
						IntPtr pixelPlane = pixelPlanes.BackBuffer;
						
						//readPixelPlane
						//copy to memory buffer (slow)
						for (int x = 0; x < newSize.Width; x++) {
							for (int y = 0; y < newSize.Height; y++) {
								Marshal.WriteInt32(pixelPlane, (y * newSize.Width + x) * 4, newImage.GetPixel(x, y).ToArgb());
							}
						}
						
						pixelPlanes.UnlockBackBuffer();
						pixelPlanes.ToggleFrontBack();
					}
					
					readyForPlaying = true;
					
					canvas.Dispose();
					newImage.Dispose();
					image.Dispose();
	
				} catch (Exception e) {
					Log( LogType.Error, "[LoadImage Exception] " + e.Message);
				}
	
			}
	
	
			public Size GetWantedSize(int sourceWidth, int sourceHeight)
			{
				Size wantedSize = new Size(sourceWidth, sourceHeight);
				double sar = 1;
				if (sourceWidth == 0 && sourceHeight == 0 && currWidthIn == 0 && currHeightIn == 0) {
					Log( LogType.Debug, "STRANGE wxh = 0x0" );
					wantedSize.Width = 320;
					wantedSize.Height = 240;
				} else {
					sar = (double)sourceWidth / sourceHeight;
					//Log( LogType.Debug, "sar = (double)" + sourceWidth + " / " + sourceHeight + " = " + sar );
				}
				//if width or height forced, calculate the other one autoamticlly (keep aspect ratio)
				if (currWidthIn > 0 && currHeightIn == 0) {
					wantedSize.Width = currWidthIn;
					wantedSize.Height = Math.Max(1, (int)((double)currWidthIn / sar));
				} else if (currWidthIn == 0 && currHeightIn > 0) {
					wantedSize.Width = Math.Max(1, (int)((double)currHeightIn * sar));
					wantedSize.Height = currHeightIn;
				} else if (currWidthIn > 0 && currHeightIn > 0) {
					wantedSize.Width = currWidthIn;
					wantedSize.Height = currHeightIn;
				}
	
				//sometimes LibVLC returns a negative value? LibVLC BUG, or is this 'normal' behaviour?
				if ( wantedSize.Width < 0 ) wantedSize.Width = - wantedSize.Width;
				if ( wantedSize.Height < 0 ) wantedSize.Height = - wantedSize.Height;
				
				return wantedSize;
			}
			
			private bool IsPlaying(bool active)
			{
				return active ? parent.FPlayIn[slice] : false;
			}
			private IDiffSpread<String> GetFileNameIn(bool active)
			{
				return active ? parent.FFileNameIn : parent.FNextFileNameIn;
			}
	
			public int VideoWidth {
				get { return videoWidth; }
			}
			public int VideoHeight {
				get { return videoHeight; }
			}
	
			public int Slice {
				get { return slice; }
			}
	
			public DoubleMemoryBuffer DoubleMemoryBuffer {
				get { return pixelPlanes; }
			}
	
			private long prevTime = DateTime.Now.Ticks;
			private long currTime = DateTime.Now.Ticks;
			private double ReportElapsedTime(string description, double reportOnlyIfMoreThanOrEqualToMillis)
			{
				currTime = DateTime.Now.Ticks;
	
				double ms = (double)(currTime - prevTime) / 10000;
				if (ms >= reportOnlyIfMoreThanOrEqualToMillis) {
					Log( LogType.Debug, description + " took " + ms + " milliseconds." );
				}
				prevTime = currTime;
	
				return ms;
			}
	
		}
		#endregion private classes
		
		#region fields
		ISpread<string> prevFileNameIn;
		ISpread<string> prevNextFileNameIn;

		private ISpread<MediaRenderer> mediaRendererA;
		private ISpread<MediaRenderer> mediaRendererB;
		private ISpread<MediaRenderer> mediaRendererCurrent;
		//points to A or B, depending on which one is the 'visible' mediaRenderer
		private ISpread<MediaRenderer> mediaRendererNext;
		//points to B or A, depending on which one is the 'invisible' mediaRenderer
		private ISpread<MemoryToTextureRenderer> memoryToTextureRendererA;
		private ISpread<MemoryToTextureRenderer> memoryToTextureRendererB;


		//private ISpread<Mutex> mediaRendererBackFrontMutex; //used to make sure which one is the back and which one the front renderer status is always correct


		private const int lockMaxTimeout = -1; //ms (-1 = forever)

		//unsafe private delegate void FillTexureFunctionInPlace(uint* data, int row, int col, int width, int height);
		private VVVV.Utils.SlimDX.TextureFillFunctionInPlace currentFillTextureFunction;

		private String logMe = "";
		//used in Callback functions, because logging from there crashes

		private TexturedVertex[] myQuad;
		private int myQuadSize;
		private Dictionary<Int64, VertexBuffer> device2QuadVertexBuffer;


		[Import()]
		IPluginHost FHost;
		//IPluginHost host;


		[Import()]
		VVVV.Core.Logging.ILogger FLogger;


		#endregion fields


		// import host and hand it to base constructor
		[ImportingConstructor()]
		public FileStreamVlcNode(IPluginHost host) : base(host)
		{
			//this.host = host;

			unsafe {
				currentFillTextureFunction = FillTexure;
			}

			//fill the quad
			///////////////
			float xy = 1f;
			float z = 0;
			float u = 0;
			float v = 0;

			myQuad = new TexturedVertex[4];
			myQuad[0].Position.X = -xy;
			myQuad[0].Position.Y = xy;
			myQuad[0].Position.Z = z;
			myQuad[0].TextureCoordinate.X = u;
			myQuad[0].TextureCoordinate.Y = v;
			myQuad[1].Position.X = -xy;
			myQuad[1].Position.Y = -xy;
			myQuad[1].Position.Z = z;
			myQuad[1].TextureCoordinate.X = u;
			myQuad[1].TextureCoordinate.Y = 1 + v;
			myQuad[2].Position.X = xy;
			myQuad[2].Position.Y = xy;
			myQuad[2].Position.Z = z;
			myQuad[2].TextureCoordinate.X = 1 + u;
			myQuad[2].TextureCoordinate.Y = v; //- (float)(FBlendIn[0] / 2)
			myQuad[3].Position.X = xy; //+ (float)(FBlendIn[0] / 2)
			myQuad[3].Position.Y = -xy;
			myQuad[3].Position.Z = z;
			myQuad[3].TextureCoordinate.X = 1 + u;
			myQuad[3].TextureCoordinate.Y = 1 + v;

			myQuadSize = myQuad.GetLength(0) * Marshal.SizeOf(typeof(TexturedVertex));

			device2QuadVertexBuffer = new Dictionary<Int64, VertexBuffer>();


			//argc, argv
			int initialSpreadCount = 1; //1 because we can only initialize on first Evaluate()
			mediaRendererA = new Spread<MediaRenderer>(initialSpreadCount);
			mediaRendererB = new Spread<MediaRenderer>(initialSpreadCount);
			mediaRendererCurrent = new Spread<MediaRenderer>(initialSpreadCount);
			mediaRendererNext = new Spread<MediaRenderer>(initialSpreadCount);

			//mediaRendererBackFrontMutex = new Spread<Mutex>( initialSpreadCount );

			memoryToTextureRendererA = new Spread<MemoryToTextureRenderer>(initialSpreadCount);
			memoryToTextureRendererB = new Spread<MemoryToTextureRenderer>(initialSpreadCount);

			CreateMediaRenderer(0);

		}

		~FileStreamVlcNode()
		{
			Dispose();
		}

		public void Dispose()
		{
			for (int index = 0; index < mediaRendererA.SliceCount; index++) {
				try {
					DisposeMediaRenderer(index);
				} catch (Exception e) {
				}
			}

			//base.Dispose();

			// Use SupressFinalize in case a subclass of this type implements a finalizer.
			GC.SuppressFinalize( this );
		}


		
		public MemoryToTextureRenderer GetMemoryToTextureRendererCurrent(int slice)
		{
			if ( memoryToTextureRendererA[slice] == null || memoryToTextureRendererB[slice] == null ) {
				Log( LogType.Error, "[GetMemoryToTextureRendererCurrent] memoryToTextureRendererA[" + slice + "] IS NULL" );
			}
			return (mediaRendererCurrent[slice] == mediaRendererA[slice] ? memoryToTextureRendererA[slice] : memoryToTextureRendererB[slice]);
		}

		public MemoryToTextureRenderer GetMemoryToTextureRendererNext(int slice)
		{
			if ( memoryToTextureRendererA[slice] == null || memoryToTextureRendererB[slice] == null ) {
				Log( LogType.Error, "[GetMemoryToTextureRendererNext] memoryToTextureRendererA[" + slice + "] IS NULL" );
			}
			return (mediaRendererNext[slice] == mediaRendererA[slice] ? memoryToTextureRendererA[slice] : memoryToTextureRendererB[slice]);
		}


		#region helper functions
		private void UpdateSliceCount(int spreadMax)
		{
			//Log( LogType.Debug, "UPDATING SliceCount from " + mediaRendererA.SliceCount + " to " + spreadMax);

			//change everything that has an influence if the spreadMax value changes, like the nr of mediaplayers
			Log( LogType.Debug, "EXISTING MEDIA RENDERERS: --------------------------------" );
			for (int i = 0; i < mediaRendererA.SliceCount; i++) {
				Log( LogType.Debug, "    " + "A" + mediaRendererA[i].Slice + " B" + mediaRendererB[i].Slice + " C" + mediaRendererCurrent[i].Slice + " N" + mediaRendererNext[i].Slice);
			}
			int c = spreadMax;
			int prevc = Math.Max(0, mediaRendererA.SliceCount);

			//if shrinking -> dispose first before resizing spreads
			if (c < prevc) {
				for (int j = prevc - 1; j >= c; j--) {
					DisposeMediaRenderer(j);
				}
				SetSliceCount(spreadMax);
			}

			mediaRendererA.SliceCount = c;
			mediaRendererB.SliceCount = c;
			mediaRendererCurrent.SliceCount = c;
			mediaRendererNext.SliceCount = c;
			memoryToTextureRendererA.SliceCount = c;
			memoryToTextureRendererB.SliceCount = c;
			//mediaRendererBackFrontMutex.SliceCount = c;
			try {
				FDurationOut.SliceCount = c;
				FFrameCountOut.SliceCount = c;
				FFrameOut.SliceCount = c;
				FNextReadyOut.SliceCount = c;
				FPixelAspectRatioOut.SliceCount = c;
				FTextureAspectRatioOut.SliceCount = c;
				FPositionOut.SliceCount = c;
				FTextureOut.SliceCount = c;
				FWidthOut.SliceCount = c;
				FHeightOut.SliceCount = c;
				
//				FBassHandleOut.SliceCount = c;
			} catch {
			}

			//if growing -> resize spreads first before creating new mediaRenderers
			if (c >= prevc) {
				SetSliceCount(spreadMax);
				for (int i = prevc; i < c; i++) {
					CreateMediaRenderer(i);
				}
			}

			Log( LogType.Debug, "NEW MEDIA RENDERERS: --------------------------------" );
			for (int i = 0; i < mediaRendererA.SliceCount; i++) {
				Log( LogType.Debug, "    " + "A" + mediaRendererA[i].Slice + " B" + mediaRendererB[i].Slice + " C" + mediaRendererCurrent[i].Slice + " N" + mediaRendererNext[i].Slice);
			}

		}

		private void CreateMediaRenderer(int index)
		{
			Log( LogType.Debug, "++++++++ creating renderer pair " + index + " ++++++++" );
			//mediaRendererBackFrontMutex[index] = new Mutex();
			//mediaRendererBackFrontMutex[index].WaitOne();

			mediaRendererA[index] = new MediaRenderer(this, index);
			mediaRendererB[index] = new MediaRenderer(this, index);
			mediaRendererCurrent[index] = mediaRendererA[index];
			mediaRendererNext[index] = mediaRendererB[index];

			memoryToTextureRendererA[index] = new MemoryToTextureRenderer(this, index, 'A', mediaRendererA[index].DoubleMemoryBuffer);
			memoryToTextureRendererB[index] = new MemoryToTextureRenderer(this, index, 'B', mediaRendererB[index].DoubleMemoryBuffer);

			//mediaRendererBackFrontMutex[index].ReleaseMutex();
		}

		private void DisposeMediaRenderer(int index)
		{
			Log( LogType.Debug, "++++++++ disposing of renderer pair " + index + " ++++++++" );
			//mediaRendererBackFrontMutex[index].WaitOne();
			
			memoryToTextureRendererA[index].PrepareForDisposal();
			memoryToTextureRendererB[index].PrepareForDisposal();
			
			mediaRendererCurrent[index].PrepareForDisposal();
			mediaRendererNext[index].PrepareForDisposal();

			//Let TH GC do its job
			memoryToTextureRendererA[index].Dispose();
			memoryToTextureRendererB[index].Dispose();

			mediaRendererCurrent[index].Dispose();
			mediaRendererNext[index].Dispose();
			
			//mediaRendererBackFrontMutex[index].ReleaseMutex();
		}

		private void CloneSpread(ISpread<string> src, ref ISpread<string> dst)
		{
			dst = new Spread<string>(src.SliceCount);
			dst.SliceCount = src.SliceCount;
			for (int i = 0; i < src.SliceCount; i++) {
				dst[i] = (string)src[i].Clone();
			}
		}
		private void CloneSpread(ISpread<int> src, ref ISpread<int> dst)
		{
			dst = new Spread<int>(src.SliceCount);
			dst.SliceCount = src.SliceCount;
			for (int i = 0; i < src.SliceCount; i++) {
				dst[i] = src[i];
			}
		}

//		private bool LockBackFrontMediaRenderer(int index, int timeout) {
//			return mediaRendererBackFrontMutex[index].WaitOne(timeout);
//		}
//
//		private void UnlockBackFrontMediaRenderer(int index) {
//			mediaRendererBackFrontMutex[index].ReleaseMutex();
//		}


		private void FlipMediaRenderers(int index)
		{
			//LogNow( LogType.Debug, "[FlipMediaRenderers] LockBackFrontMediaRenderer " + index);
//			if ( LockBackFrontMediaRenderer(index, 5000) ) {

			//Log( LogType.Debug, "Flipping mediaRenderers" );
			if (mediaRendererCurrent[index] == mediaRendererA[index]) {
				mediaRendererCurrent[index] = mediaRendererB[index];
				mediaRendererNext[index] = mediaRendererA[index];
			} else {
				mediaRendererCurrent[index] = mediaRendererA[index];
				mediaRendererNext[index] = mediaRendererB[index];
			}

//				UnlockBackFrontMediaRenderer(index);
			//LogNow( LogType.Debug, "[FlipMediaRenderers] UNLockBackFrontMediaRenderer " + index);
//			}
//			else {
//				Log( LogType.Error, "[FlipMediaRenderers Warning] seems like BackFrontMediaRenderer was blocking, mediarenderers not flipped !!!" );
//			}
		}

//		private void SetFrontTexture(int deviceDataKey, Texture t, MediaRenderer r) {
//			//at this time it only flips front and backbuffers, but in future we could blend multiple textures together here, and update the real output texture
//			if ( IsFrontMediaRenderer(r) ) {
//				FDeviceData[deviceDataKey].Data[r.Slice] = t;
//			}
//		}


		//REMEMBER TO LOCK / UNLOCK FrontBackMediaRenderer
		public bool IsFrontMediaRenderer(MediaRenderer r)
		{
			try {
				return r == mediaRendererCurrent[r.Slice];
			} catch (Exception e) {
				throw new Exception("[IsFrontMediaRenderer Exception] " + e.Message, e);
			}
		}

		public bool IsFrontMemoryToTextureRenderer(MemoryToTextureRenderer r)
		{
			try {
				return r == GetMemoryToTextureRendererCurrent( r.Slice );
			} catch (Exception e) {
				throw new Exception("[IsFrontMemoryToTextureRenderer Exception] " + e.Message, e);
			}
		}


		public void Log( LogType logType, string message)
		{
			logMe += "\n" + (logType == LogType.Error ? "ERR " : (logType == LogType.Warning ? "WARN " : "")) + message;
		}

		public void LogNow(LogType logType, string message)
		{
			FLogger.Log( logType, message);
		}
		
		public Dictionary<Device, TextureDeviceData> DeviceData {
			get { return FDeviceData; }
		}

		public string GetFullPath(string path)
		{
			if (Path.IsPathRooted(path) || path.Contains("://"))
				return path;

			string fullPath = path;
			try {
				string patchPath;
				FHost.GetHostPath(out patchPath);
				patchPath = Path.GetDirectoryName(patchPath);
				fullPath = Path.GetFullPath(Path.Combine(patchPath, path));
			} catch (Exception e) {
				Log( LogType.Error, e.Message);
				return path;
			}

			return fullPath;
		}
		#endregion helper functions

		///////////////////////////
		//called each frame by vvvv
		///////////////////////////
		private int evaluateCalled = 0;
		public void Evaluate(int spreadMax)
		{
			try {
				if (logMe.Length > 0) {
					FLogger.Log( LogType.Debug, logMe);
					logMe = "";
				}

				//			if (evaluateCalled < 300) {
				//				evaluateCalled++;
				//				return;
				//			}

				if (spreadMax != mediaRendererCurrent.SliceCount) {
					Log( LogType.Debug, "new spreadMax = " + spreadMax);
					try {
						UpdateSliceCount(spreadMax);
					} catch (Exception e) {
						Log( LogType.Error, "[Evaluate Exception] (UpdateSliceCount) " + e.Message + "\n\n" + e.StackTrace);
						throw e;
					}
				}

				for (int index = 0; index < mediaRendererA.SliceCount; index++) {

					try {
						
//						if ( FRotateIn.IsChanged ) {
//							Log( LogType.Debug, "FRotateIn.IsChanged" );
//							if ( prevRotateIn == null ) { CloneSpread( FRotateIn, ref prevRotateIn ); }
//							for ( int i = 0; i < FFileNameIn.SliceCount; i++ ) {
//								Log( LogType.Debug, "FRotateIn.IsChanged " + i);
//								if ( FRotateIn[i].CompareTo(prevRotateIn[i]) != 0 ) {
//									Log( LogType.Debug, "CALLING UpdateRotation" );
//									mediaRendererCurrent[index].UpdateRotation( );
//									mediaRendererNext[index].UpdateRotation( );
//								}
//							}
//							CloneSpread( FRotateIn, ref prevRotateIn );
//						}

						if (prevFileNameIn == null) {
							//Log( LogType.Debug, "Trying to clone FFileNameIn spread to prevFileNameIn spread" );
							CloneSpread(FFileNameIn, ref prevFileNameIn);
						}
						if (prevNextFileNameIn == null) {
							//Log( LogType.Debug, "Trying to clone FNextFileNameIn spread to prevNextFileNameIn spread" );
							CloneSpread(FNextFileNameIn, ref prevNextFileNameIn);
						}
					} catch (Exception e) {
						Log( LogType.Error, "[Evaluate Exception] (FileName) " + e.Message);
					}

					try {
						if (FFileNameIn.IsChanged) {
							//Log( LogType.Debug, "FileNameIn changed " );
							if ((prevFileNameIn[index] == null || FFileNameIn[index] == null) || prevFileNameIn[index].CompareTo(FFileNameIn[index]) != 0) {
								//Log( LogType.Debug, "and '" + FFileNameIn[index] + "' (new[" + index + "]) != '" + prevFileNameIn[index] + "' (old) " );
								if (prevNextFileNameIn[index] != null && FFileNameIn[index] != null && prevNextFileNameIn[index].CompareTo(FFileNameIn[index]) == 0) {
									FlipMediaRenderers(index);
								} else {
									//Log( LogType.Debug, "BUT ALSO different from previous NEXT[" + index + "] " + prevNextFileNameIn[index] + " " );
								}
							}
						}
					} catch (Exception e) {
						Log( LogType.Error, "[Evaluate Exception] (FileName & FlipMediaRenderers) " + e.Message);
					}

					try {
						//Log( LogType.Debug, "                                <<<< Evaluate CURRENT " + index);
						mediaRendererCurrent[index].Evaluate(true);
						//Log( LogType.Debug, "                                >>>> Evaluate CURRENT " + index);
						//Log( LogType.Debug, "                                <<<< Evaluate NEXT " + index);
						mediaRendererNext[index].Evaluate(false);
						//Log( LogType.Debug, "                                >>>> Evaluate NEXT " + index);

					} catch (Exception e) {
						Log( LogType.Error, "[Evaluate Exception] (MediaRenderer.Evaluate) " + e.Message);
					}

					try {
						//Log( LogType.Debug, "                                <<<< Evaluate CURRENT " + index);
						GetMemoryToTextureRendererCurrent(index).Evaluate();
						//Log( LogType.Debug, "                                >>>> Evaluate CURRENT " + index);
						//Log( LogType.Debug, "                                <<<< Evaluate NEXT " + index);
						GetMemoryToTextureRendererNext(index).Evaluate();
						//Log( LogType.Debug, "                                >>>> Evaluate NEXT " + index);

					} catch (Exception e) {
						Log( LogType.Error, "[Evaluate Exception] (MemoryToTextureRenderer.Evaluate) " + e.Message);
					}

				}


				if (FFileNameIn.IsChanged) {
					CloneSpread(FFileNameIn, ref prevFileNameIn);
				}
				if (FNextFileNameIn.IsChanged) {
					CloneSpread(FNextFileNameIn, ref prevNextFileNameIn);
				}
			} catch (Exception e) {
				FLogger.Log( LogType.Error, "[MAINLOOP ERROR] Something went terribly wrong: " + e.Message + "\n" + e.StackTrace);
			}
		}

		#region texture functions


		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		// WATCH OUT EXISTING TEXTURES ARE DESTROYED when Reinitialize() is called
		protected override Texture CreateTexture(int slice, Device device)
		{
			//return TextureUtils.CreateTexture(device, 20, 20);

			int index = slice;

			Log( LogType.Debug, "CreateTexture(Slice " + index + (index != mediaRendererCurrent[index].Slice ? " (INDEX PROBLEM: " + mediaRendererCurrent[index].Slice + ") " : "") + ", Device " + device.ComPointer.ToInt64() + ")" );
			//Log( LogType.Debug, "----------------------------------------" );

			//Log( LogType.Debug, "--> CreateTexture(...) CURRENT" );
			//refill the new texture (if display change)
			Texture t = GetMemoryToTextureRendererCurrent(index).CreateTexture(device);

			//Log( LogType.Debug, "--> CreateTexture(...) NEXT" );
			//refill the new texture (if display change)
			GetMemoryToTextureRendererNext(index).CreateTexture(device);

//			if ( ! device2QuadVertexBuffer.ContainsKey(device.ComPointer.ToInt64()) ) {
//				device2QuadVertexBuffer[device.ComPointer.ToInt64()] = CreateQuadTexturedVertexBuffer(device);
//			}

			return t;

		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe protected override void UpdateTexture(int slice, Texture texture)
		{
			//TextureUtils.Fill32BitTexInPlace(texture, FillTexure);
		}

		private void RenderToTexture(int slice, Texture texture)
		{
			try {
				bool blending = true;
				//=> we need 2 textures
				float blend = 0.5f;

				Device d = texture.Device;
				Texture tA = GetMemoryToTextureRendererCurrent(slice).GetFrontTexture(d);
				//= TextureUtils.CreateTexture(texture.Device, Math.Max(FWidthIn[Slice], 1), Math.Max(FHeightIn[Slice], 1));
				//TextureUtils.Fill32BitTexInPlace(tA, FillTexure1);
				Texture tB = GetMemoryToTextureRendererNext(slice).GetFrontTexture(d);
				//= TextureUtils.CreateTexture(texture.Device, Math.Max(FWidthIn[Slice], 1), Math.Max(FHeightIn[Slice], 1));
				//TextureUtils.Fill32BitTexInPlace(tB, FillTexure2);
				d.BeginScene();
				d.SetRenderTarget(0, texture.GetSurfaceLevel(0));

				//this way we see where the texture is
				//d.ColorFill(texture.GetSurfaceLevel(0), new Color4( 1.0f, 0.1f, 0.1f, 0.1f ) );

				d.SetTransform(TransformState.View, SlimDX.Matrix.LookAtLH(new Vector3(0, 0, -2), new Vector3(0, 0, 0), new Vector3(0, 1, 0)));
				d.SetTransform(TransformState.Projection, SlimDX.Matrix.OrthoLH(2, 2, 1, 1000));
				d.SetTransform(TransformState.World, SlimDX.Matrix.Identity);
				//d.Clear(ClearFlags.All, new Color4(), 10.0f, 0);
				d.SetTransform(TransformState.Texture0, SlimDX.Matrix.Identity);

				d.SetRenderState(RenderState.Lighting, false);
				d.SetRenderState(RenderState.CullMode, Cull.None);
				d.SetRenderState(RenderState.ZEnable, false);

				d.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
				d.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
				d.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);

				d.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);
				d.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Count3);

				//d.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Disable);
				d.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
				d.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.SelectArg1);
				d.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
				d.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.SelectArg1);

				d.VertexFormat = TexturedVertex.Format;
				VertexBuffer vb = device2QuadVertexBuffer[d.ComPointer.ToInt64()];
				d.SetStreamSource(0, vb, 0, Marshal.SizeOf(typeof(TexturedVertex)));

				d.SetTexture(0, tA);


				if (blending) {

					d.SetTransform(TransformState.Texture1, SlimDX.Matrix.Identity);

					int blend255 = (int)(Math.Max(0f, Math.Min(blend, 1f)) * 255);
					int blendRGBA = blend255 * 0x1000000 + blend255 * 0x10000 + blend255 * 0x100 + blend255;
					d.SetRenderState(RenderState.TextureFactor, blendRGBA);
					//d.SetRenderState(RenderState.BlendFactor, blendRGBA );
					//d.SetRenderState(RenderState.BlendOperationAlpha, BlendOperation.Minimum);
					//d.SetRenderState(RenderState.AlphaBlendEnable, true);
					//d.SetRenderState(RenderState.Wrap0, TextureWrapping.None);
					//d.SetRenderState(RenderState.Wrap1, TextureWrapping.None);

					d.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Disable);
					d.SetSamplerState(1, SamplerState.AddressU, TextureAddress.Wrap);
					d.SetSamplerState(1, SamplerState.AddressV, TextureAddress.Wrap);
					d.SetSamplerState(1, SamplerState.AddressW, TextureAddress.Wrap);

					d.SetTextureStageState(1, TextureStage.TexCoordIndex, 0);

					d.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Texture);
					//arg 1 is texture
					d.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Current);
					//arg 2 is last stage
					d.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.BlendFactorAlpha);
					//TextureOperation.BlendFactorAlpha);
					d.SetTextureStageState(1, TextureStage.AlphaArg1, TextureArgument.Texture);
					//arg 1 is texture
					d.SetTextureStageState(1, TextureStage.AlphaArg2, TextureArgument.Current);
					//arg 1 is texture
					d.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.BlendFactorAlpha);

					d.SetStreamSource(1, vb, 0, Marshal.SizeOf(typeof(TexturedVertex)));
					//or copy vertexbuffer from 1st stage
					//int vbOffset;
					//int vbStride;
					//d.GetStreamSource(0, out vb, out vbOffset, out vbStride);				
					//d.SetStreamSource(1, vb, vbOffset, vbStride );

					d.SetTexture(1, tB);

				}

				d.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

								/*
				DataStream ds = vb.Lock(0, 0, LockFlags.ReadOnly);
				string s = "datastream: ";
				try {
					while (ds.Position < ds.Length) {
							s += " | " + ds.Read<float>();
					}
				} catch {}
				FLogger.Log( LogType.Debug, s);
				vb.Unlock();
				*/
				if (d.Capabilities.MaxSimultaneousTextures <= 1) {
					FLogger.Log( LogType.Debug, "DRAWING will fail" );
				} else {
						//+ "\nAddresMode: U" + d.GetSamplerState(0, SamplerState.AddressU) + " V" + d.GetSamplerState(0, SamplerState.AddressV) + " W" + d.GetSamplerState(0, SamplerState.AddressW)
					FLogger.Log( LogType.Debug, "DRAWING (up to " + d.Capabilities.MaxSimultaneousTextures + " simultaneous textures) " + "4 x vertex size = " + Marshal.SizeOf(typeof(TexturedVertex)) + " format:" + vb.Description.Format.ToString() + " fvf:" + vb.Description.FVF.ToString() + " pool:" + vb.Description.Pool.ToString() + " size:" + vb.Description.SizeInBytes.ToString() + " usage:" + vb.Description.Usage.ToString());
				}


				d.EndScene();

				if (blending) {
					d.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
				}

			} catch (Exception e) {
				FLogger.Log( LogType.Error, "TEST PROBLEM: " + e.Message);
			}

		}

		private VertexBuffer CreateQuadTexturedVertexBuffer(Device device)
		{
/*
			float[,] aQuad = { 	{-1.0f,1.0f,10.0f,0.0f,0.0f,0.0f,0.0f},
								{-1.0f,-1.0f,10.0f,0.0f,1.0f,0.0f,1.0f},
								{1.0f,1.0f,10.0f,1.0f,0.0f,1.0f,0.0f},
								{1.0f,-1.0f,10.0f,1.0f,1.0f,1.0f,1.0f} };
*/


			FLogger.Log( LogType.Debug, "quadsize = " + myQuadSize);
			//VertexBuffer vb = new VertexBuffer( typeof(TexturedVertex), 4, d, Usage.WriteOnly | Usage.Dynamic, TexturedVertex.Format, Pool.Default );
			//public VertexBuffer(Type typeVertexType, int numVerts, Device device, Usage usage, VertexFormats vertexFormat, Pool pool);
			VertexBuffer vb = new VertexBuffer(device, myQuadSize, Usage.WriteOnly | Usage.Dynamic, TexturedVertex.Format, Pool.Default);

			FillVertexBuffer(vb, myQuad, myQuadSize);

			//Mesh m = new Mesh(d, 2, 4, MeshFlags.DoNotClip, TexturedVertex.Format);
			//vb = m.VertexBuffer;

			return vb;
		}

		private void FillVertexBuffer<T>(VertexBuffer vertexBuffer, T[] vertexArray, int vertexarraySizeInBytes)
		{
			DataStream vbds = vertexBuffer.Lock(0, vertexarraySizeInBytes, LockFlags.None);
			IntPtr tvp = Marshal.UnsafeAddrOfPinnedArrayElement(vertexArray, 0);
			//Marshal.Copy( tvp, 0, vbds.DataPointer, tvsize );
			for (int i = 0; i < vertexarraySizeInBytes; i++) {
				try {
					vbds.WriteByte(Marshal.ReadByte(tvp, i));
				} catch {
				}
			}
			//Unlock the vb before you can use it elsewhere
			vertexBuffer.Unlock();
		}

		//this is a pixelshader like method, which we pass to the fill function
		unsafe private void FillTexure(uint* data, int row, int col, int width, int height)
		{
			try {
				//a pixel is just a 32-bit unsigned int value
				UInt32 pixel = 0x226688c;
				//(((UInt32*)mediaRendererCurrent[0].GetReadPixelPlane().ToPointer())[row * width + col]);
/*				UInt32 pixel = 0xffffffc;
				UInt32* pixelPointer = &( ( (UInt32*) myReadPixelPlane[0].ToPointer() )[row*width + col]);
				pixel = pixel.setARGB( ((Byte*)pixelPointer)[3], ((Byte*)pixelPointer)[0], ((Byte*)pixelPointer)[1], ((Byte*)pixelPointer)[2] );
				// old way (more copying of bytes)
				//byte a, r, g, b;
				//( ( (UInt32*) myReadPixelPlane[0].ToPointer() )[row*width + col] ).getARGB(out a, out b, out g, out r);
				//pixel = pixel.setARGB(a, r, g, b);
*/

				//copy pixel into texture
				TextureUtils.SetPtrVal2D(data, pixel, 				/*pixel.setARGB( ((byte*)&pixel)[3], ((byte*)&pixel)[0], ((byte*)&pixel)[1], ((byte*)&pixel)[2] )*/row, col, width);
			} catch (Exception e) {
				Log( LogType.Error, "[FillTexture Exception] " + e.Message);
			}
		}
		#endregion texture functions
	}
}
