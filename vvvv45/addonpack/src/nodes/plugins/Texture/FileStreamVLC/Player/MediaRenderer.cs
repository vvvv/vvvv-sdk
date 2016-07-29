/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 29/02/2012
 * Time: 18:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

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


using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

using LibVlcWrapper;


namespace VVVV.Nodes.Vlc.Player
{
	/// <summary>
	/// Description of MediaRenderer.
	/// </summary>
/*
	public class MediaRenderer : IDisposable
	{
		#region MediaRenderer fields
		//needed to access pins (at the right slice)
		private FileStreamVlcNode parent;
		private int mediaRendererIndex = 0;
		//slice index

		private IntPtr libVLC = IntPtr.Zero;

		string currFileNameIn;
		string newFileNameIn = "";
		//COPY OF CURRFILENAMEIN FOR USING IN THE (THREADED) UpdateMediaPlayerStatus
		string prevFileNameIn;
		bool currPlayIn;
		bool currLoopIn;
		float currSpeedIn;
		float currSeekTimeIn;
		bool currDoSeekIn;
		int currRotateIn;
		int currWidthIn;
		int currHeightIn;
		float currVolumeIn;

		Thread evaluateThread;
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
		private const int STATUS_WATING = -5;
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
		
		#endregion MediaRenderer fields

		#region MediaRenderer constructor/destructor
		public MediaRenderer(FileStreamVlcNode parentObject, int index)
		{
			parent = parentObject;
			mediaRendererIndex = index;

			libVLC = parent.libVLC;
			//LibVlcMethods.libvlc_new(parent.argv.GetLength(0), parent.argv);	//argc, argv
			PrepareMediaPlayer();
		}

		~MediaRenderer()
		{
			Dispose();
		}

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
			evaluateThread.Start();
		}

		public void Dispose()
		{
			//parent.FLogger.Log( LogType.Debug, "[Dispose] Disposing media renderer " + mediaRendererIndex);
			//evaluateThread.Abort();
			evaluateStopThreadWaitHandle.Set();
			evaluateThread.Join();
			//preloadingStatus = STATUS_INACTIVE;

			if ( mediaPlayer != IntPtr.Zero ) {
				try {
					LibVlcMethods.libvlc_media_player_stop( mediaPlayer );
				} catch {
				}
				try {
					LibVlcMethods.libvlc_media_player_release( mediaPlayer );
				} catch {
				}
			}
			mediaPlayer = IntPtr.Zero;


			//deallocate video memory
			try {
				pixelPlanes.Dispose();
				Marshal.FreeHGlobal(opaqueForCallbacks);
			} catch {
			}

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
					currFileNameIn = GetFileNameIn(active)[mediaRendererIndex];

					if (currFileNameIn == null) {
						Log( LogType.Debug, (active ? "FileNameIn" : "NextFileNameIn") + "[" + mediaRendererIndex + "] IS NULL!" );
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
				currLoopIn = parent.FLoopIn[mediaRendererIndex];
				currSpeedIn = parent.FSpeedIn[mediaRendererIndex];
				if (parent.FDoSeekIn[mediaRendererIndex]) {
					currSeekTimeIn = parent.FSeekTimeIn[mediaRendererIndex];
					currDoSeekIn = true;
				}
				currRotateIn = parent.FRotateIn[mediaRendererIndex];
				currWidthIn = parent.FWidthIn[mediaRendererIndex];
				currHeightIn = parent.FHeightIn[mediaRendererIndex];
				currVolumeIn = parent.FVolumeIn[mediaRendererIndex];

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

		#region MediaRenderer Vlc Callback functions

		//////////////////////////////////////////////////
		// Next 3 functions are used for PLAYING the video
		//////////////////////////////////////////////////
		public IntPtr VlcVideoLockCallBack(ref IntPtr data, ref IntPtr pixelPlane)
		{
			if (disposing) {
				Log( LogType.Error, ("VlcLockCallback(" + data.ToInt32() + ") : PLAYER HAS BEEN DISPOSED AND STILL PLAYING ???") );
				return IntPtr.Zero;
			}
			else {
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
			//if (lockCalled != unlockCalled) Log( LogType.Error, (parent.IsFrontMediaRenderer(this) ? "FRONT " : "BACK ") + "(lock/unlock=" + lockCalled  + "/" + unlockCalled + ")" );

			try {
				currentFrame++;
				
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
				if (disposing) {
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
				Log( LogType.Error, ("[VlcDisplayCallback(" + data.ToInt32() + ") Exception] " + e.Message));
			}
		}

		private void AllowDisplay(IntPtr data)
		{
			//if ( pixelPlanes.LockBackBufferForWriting(0) ) {
			//	pixelPlanes.UnlockBackBuffer();
			pixelPlanes.ToggleFrontBack();
			//}
		}

		public void VlcAudioPlayCallBack(ref IntPtr data, IntPtr samples, UInt32 count, Int64 pts) {
//				Bass.BASS_SampleSetData(bassStreamHandle, samples);
		}
		
		#endregion MediaRenderer Vlc Callback functions

		private void EvaluateThreadProc()
		{
			while (true) {
				int waitHandleIndex = WaitHandle.WaitAny(new EventWaitHandle[2] {
					evaluateEventWaitHandle,
					evaluateStopThreadWaitHandle
				});

				if (waitHandleIndex == 0) {
					try {
						//Log( (evaluateCurrentActiveParameter ? "[signalled FRONT player] " : "[signalled BACK player] ") );
						UpdateMediaPlayerStatus_Threaded(null);
					} catch (Exception e) {
						Log( LogType.Error, "[EvaluateThreadProc] Something went terribly wrong: " + e.Message + "\n" + e.StackTrace);
					}
					//Thread.Sleep(2);
				} else if (waitHandleIndex == 1) {
					if (mediaPlayerBusyMutex.WaitOne(10000)) {
						try {
							LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
						} catch {
						}
						mediaPlayerBusyMutex.ReleaseMutex();
					} else {

					}
					disposing = true;
					break;
				}
			}
			Log( LogType.Debug, "... exiting evaluate thread for renderer " + mediaRendererIndex + " ... " );				
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

//				if ( currFileNameIn.Length == 0 ) {
//					readyForPlaying = false;
//					if (	LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Playing 
//						 || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Paused 
//						 || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Ended 
//						 || LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Error) {
//						
//						Log( LogType.Debug, "Filename empty, STOP mediaPlayer" + (this == parent.mediaRendererA ? "A " : "B ") + (this == parent.mediaRendererCurrent[mediaRendererIndex] ? "(FRONT) " : "(BACK) " ) + currFileNameIn );
//						LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
//						Log( LogType.Debug, ( LibVlcMethods.libvlc_media_player_get_state(mediaPlayer) == libvlc_state_t.libvlc_Stopped ? "STOPPED!!!" : "" ) );
//					}
//				}
//				else 

				if ((currFileNameIn != null && currFileNameIn != prevFileNameIn) && (prevFileNameIn == null || prevFileNameIn.CompareTo(currFileNameIn) != 0)) {
					newFileNameIn = string.Copy(currFileNameIn);
					if ( parent.IsFrontMediaRenderer(this) ) {
						preloadingStatus = STATUS_NEWFILE;
					}
					else if ( preloadingStatus != STATUS_WATING ) {
						//If not front player, wait a bit to give others the time to load!
						statusWaitingUntilTicks = DateTime.Now.AddTicks( MillisecondsToTicks( 200 + (50 * mediaRendererIndex) ) ).Ticks;
						
						preloadingStatus = STATUS_WATING;
					}
				} else if (currFileNameIn == null) {
					Log( LogType.Error, "[UpdateMediaPlayerStatus Exception] currFileNameIn == null" );
				}

				mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);

				if (preloadingStatus == STATUS_OPENINGFILE && newFileNameIn.Length > 0) {
					Log( LogType.Debug, "		(preloadingStatus == STATUS_OPENINGFILE && newFileNameIn.Length > 0)" );
				}
				if ( preloadingStatus == STATUS_WATING ) {
					
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
						if ( //mpState != libvlc_state_t.libvlc_NothingSpecial &&
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
						if ( //mpState == libvlc_state_t.libvlc_NothingSpecial ||
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
									if ( isStream ) {
										LibVlcMethods.libvlc_media_add_option( preloadMedia, Encoding.UTF8.GetBytes("sout=#dummy") );

										LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, preloadMedia);
										LibVlcMethods.libvlc_media_player_play(mediaPlayer);
									}
									else {
										LibVlcMethods.libvlc_media_parse( preloadMedia );
									}
									
									//Log( LogType.Debug, "SETTING STATUS_GETPROPERTIES" );
									preloadingStatus = STATUS_GETPROPERTIES;
								}
								//else {
								//	Log( LogType.Debug, "Error opening file: " + newFileNameIn );
								//}
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
								Log( LogType.Debug, "Stream detected: wait some time to see if there are more streams..." );
								Thread.Sleep(3000);
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
										} else {
											LibVlcMethods.libvlc_media_add_option(preloadMedia, Encoding.UTF8.GetBytes("no-audio") );
											//dshow-adev=none
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
//				// DEBUG
//				else if ( preloadingStatus == STATUS_GETPROPERTIES ) {
//					libvlc_state_t state = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);
//					string stateDescription = "unknown";
//					switch (state) {
//						case libvlc_state_t.libvlc_Buffering: stateDescription = "buffering"; break;
//						case libvlc_state_t.libvlc_Ended: stateDescription = "ended"; break;
//						case libvlc_state_t.libvlc_Error: stateDescription = "error"; break;
//						case libvlc_state_t.libvlc_NothingSpecial: stateDescription = "nothing special"; break;
//						case libvlc_state_t.libvlc_Opening: stateDescription = "opening"; break;
//						case libvlc_state_t.libvlc_Paused: stateDescription = "paused"; break;
//						case libvlc_state_t.libvlc_Playing: stateDescription = "playing"; break;
//						case libvlc_state_t.libvlc_Stopped: stateDescription = "stopped"; break;
//					}
//					Log( LogType.Debug, "STATUS_GETPROPERTIES but libvlc_media_player_get_state != ended or playing. It's " + stateDescription);
//				}
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


				if (parent.IsFrontMediaRenderer(this) && preloadingStatus == STATUS_PLAYING) {

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
						if ((mediaPlayerState == libvlc_state_t.libvlc_Ended) && currLoopIn) {
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
				if (active && preloadingStatus == STATUS_PLAYING) {
					if (mediaPlayerBusyMutex.WaitOne(0)) {
						try {
							libvlc_state_t mediaPlayerState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);

							if (mediaPlayerState == libvlc_state_t.libvlc_Playing || mediaPlayerState == libvlc_state_t.libvlc_Paused || mediaPlayerState == libvlc_state_t.libvlc_Ended) {

								try {
									if ( videoLength == 0 ) {
										videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_player_get_length(mediaPlayer) ) / 1000.0f;
										//videoLength = Convert.ToSingle( LibVlcMethods.libvlc_media_get_duration( preloadMedia ) ) / 1000.0f;
									}	
									videoFps = LibVlcMethods.libvlc_media_player_get_fps(mediaPlayer);
									//float relativePosition = currentFrame / videoFps / ( (float)LibVlcMethods.libvlc_media_player_get_time(mediaPlayer) / 1000 ); //LibVlcMethods.libvlc_media_player_get_position(mediaPlayer);
									float absolutePosition = currentFrame / videoFps;
									//(float)LibVlcMethods.libvlc_media_player_get_time(mediaPlayer) / 1000;
									parent.FPositionOut[mediaRendererIndex] = absolutePosition;
									//Log( LogType.Debug, "setting FPositionOut " + videoLength + " * " + LibVlcMethods.libvlc_media_player_get_position( mediaPlayer ) + " @" + videoFps + "fps => position = " + absolutePosition);
									parent.FDurationOut[mediaRendererIndex] = videoLength;
									parent.FFrameOut[mediaRendererIndex] = currentFrame;
									//Convert.ToInt32(absolutePosition * videoFps);
									parent.FFrameCountOut[mediaRendererIndex] = Convert.ToInt32(videoLength * videoFps);

//										parent.FBassHandleOut[mediaRendererIndex] = bassStreamHandle;

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
							parent.FPositionOut[mediaRendererIndex] = 0;
							parent.FDurationOut[mediaRendererIndex] = 0;
							parent.FFrameOut[mediaRendererIndex] = 1;
							parent.FFrameCountOut[mediaRendererIndex] = 1;

							UpdateOutput_TextureInfo();
						} catch (Exception e) {
							Log( LogType.Error, "[UpdateParent Exception] " + e.Message);
						}
						mediaPlayerBusyMutex.ReleaseMutex();
					} else {
						//Log( LogType.Warning, "[UpdateParent] Media Player Busy" );
					}
				} else if (!active) {
					parent.FNextReadyOut[mediaRendererIndex] = readyForPlaying;
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
			parent.Log( LogType, "[MediaRenderer " + //(this == parent.mediaRendererA[mediaRendererIndex] ? "A" : "B") +
						 mediaRendererIndex + (parent.IsFrontMediaRenderer(this) ? "+" : "-") + "] " + message);
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
			//Log( LogType.Debug, "Setting Volume to " + Convert.ToInt32( Math.Pow ( Math.Max ( Math.Min( FVolumeIn[mediaRendererIndex], 1), 0 ), Math.E ) * 100 ) );
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
//				try {
//				LibVlcMethods.libvlc_audio_set_format( mediaPlayer, Encoding.UTF8.GetBytes("WAV"), newSampleRate, newNrOfChannels );
//			
//				if ( bassStreamHandle != 0) {
//					Bass.BASS_StreamFree( bassStreamHandle );
//					bassStreamHandle = 0;
//				}
//				
//				bassStreamHandle = Bass.BASS_StreamCreatePush( newSampleRate, newNrOfChannels, BASSFlag.BASS_DEFAULT, new IntPtr() );
//				
//				Log( LogType.Debug, "[UpdateAudioFormat] bass handle = " + bassStreamHandle );
//			} catch (Exception e) {
//				Log( LogType.Error, "[UpdateAudioFormat Exception] " + e.Message);
//			}
//
//			//Log( LogType.Debug, "[UpdateAudioFormat] " + newSampleRate + "x" +  newNrOfChannels + " done!" );

		}

		unsafe private void UpdateOutput_TextureInfo()
		{
//				if ( parent.currentFillTextureFunction == parent.FillTexure || parent.currentFillTextureFunction == parent.Rotate180FillTexure ) {
			parent.FWidthOut[GetMediaRendererIndex()] = VideoWidth;
			parent.FHeightOut[GetMediaRendererIndex()] = VideoHeight;
			parent.FTextureAspectRatioOut[GetMediaRendererIndex()] = (float)VideoWidth / (float)VideoHeight;
			//TODO
			parent.FPixelAspectRatioOut[GetMediaRendererIndex()] = 1f;
//				}
//				else if ( parent.currentFillTextureFunction == parent.RotateLeftFillTexure || parent.currentFillTextureFunction == parent.RotateRightFillTexure ) {
//					parent.FWidthOut[GetMediaRendererIndex()] = VideoHeight;
//					parent.FHeightOut[GetMediaRendererIndex()] = VideoWidth;
//					parent.FTextureAspectRatioOut[GetMediaRendererIndex()] = (float)VideoHeight / (float)VideoWidth;
//					//TODO
//					parent.FPixelAspectRatioOut[GetMediaRendererIndex()] = 1.0F;
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
				
//				string[] mediaOptionParts = mediaOptions[moIndex].Trim().Split("{".ToCharArray());
//				LibVlcMethods.libvlc_media_add_option( media, mediaOptionParts.Trim() );
//
//				if ( mediaOptionParts.Length == 2) {
//					string[] flags = mediaOptionParts[1].Replace("}", "").Split(",".ToCharArray());
//					for (int flagIndex = 1; flagIndex < mediaOptionParts.Length; flagIndex++ ) {
//						string[] flagParts = flags[flagIndex].Split("=".ToCharArray());
//						if ( flagParts.Length == 2) {
//							Log( LogType.Debug, "adding option " + flagParts + " = " + flagParts[1] );
//							LibVlcMethods.libvlc_media_add_option_flag( media, flagParts, LibVlcWrapper.libvlc_video_adjust_option_t .libvlc_media_option_trusted ); //Convert.ToInt32(flagParts[1])
//						}
//						else {
//							Log( LogType.Debug, "Something strange when parsing filename options..." );
//						}
//					}
//				}
				
			}	
			catch {
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

				if (pixelPlanes.LockBackBufferForWriting(3000)) {
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
			return active ? parent.FPlayIn[mediaRendererIndex] : false;
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

		public int GetMediaRendererIndex() {
			get { return mediaRendererIndex; }
		}

		public DoubleMemoryBuffer DoubleMemoryBuffer {
			get { return pixelPlanes; }
		}

		private long prevTime = DateTime.Now.Ticks;
		private long currTime = DateTime.Now.Ticks;
		private double ReportElapsedTime(string description, double reportOnlyIfMoreThanOrEqualToMillis) {
			currTime = DateTime.Now.Ticks;

			double ms = (double)(currTime - prevTime) / 10000;
			if (ms >= reportOnlyIfMoreThanOrEqualToMillis) {
				Log( LogType.Debug, description + " took " + ms + " milliseconds." );
			}
			prevTime = currTime;

			return ms;
		}

	}
	*/
}
