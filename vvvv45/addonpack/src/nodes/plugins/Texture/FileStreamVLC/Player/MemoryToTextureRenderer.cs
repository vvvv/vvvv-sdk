/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 29/02/2012
 * Time: 17:54
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
using System.Collections.Generic;
using System.Threading;
using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.Nodes.Vlc.Utils;

using System.Runtime.InteropServices;


namespace VVVV.Nodes.Vlc.Player {
	/// <summary>
	/// Description of MemoryToTextureRenderer.
	/// </summary>
	public class MemoryToTextureRenderer : IDisposable {
		private char group;

		private FileStreamVlcNode parent;
		private DoubleMemoryBuffer doubleBuffer;
		private Dictionary<Device, DoubleTexture> device2DoubleTexture;
		private int slice;

		//private ReaderWriterLock textureLock = new ReaderWriterLock( ); //lock used for displaying => switching between the 2 buffers
		private bool textureNeedsResizingOnEvaluate;	//if the memory buffer's size changes, we need to update the textures also (in the mainloop)
		private bool deviceDataNeedsUpdatingOnEvaluate;	//if the textures are filled (and frontback toggled), the deviceData needs to be updated (make everything point to the new front textures)
		private Mutex memoryToTextureRendererBusyMutex;	//used for starting and stopping etc. in separate thread
		Thread updateTextureThread;						//will work when signalled by the right EventWaitHandle
		private EventWaitHandle updateTextureEventWaitHandle;
		private EventWaitHandle updateTextureStopThreadWaitHandle;

		private bool initialized = false;

		public MemoryToTextureRenderer( FileStreamVlcNode vvvvNode, int slice, char group, DoubleMemoryBuffer doubleMemoryBuffer ) {
			this.group = group;
			this.slice = slice;
			this.doubleBuffer = doubleMemoryBuffer;
			this.parent = vvvvNode;
			device2DoubleTexture = new Dictionary<Device, DoubleTexture>( );

			//We will subscribe to the event fired by DoubleMemoryBuffer
			doubleBuffer.Toggle += new DoubleMemoryBuffer.ToggleHandler( DoubleBufferChanged );

			textureNeedsResizingOnEvaluate = true;

			memoryToTextureRendererBusyMutex = new Mutex( );

			//CREATE A THREAD THAT WILL TRY TO LOAD NEW FILES ETC. 
			//when signalled by evaluateEventWaitHandle
			updateTextureEventWaitHandle = new EventWaitHandle( false, EventResetMode.AutoReset );
			updateTextureStopThreadWaitHandle = new EventWaitHandle( false, EventResetMode.AutoReset );
			updateTextureThread = new Thread( new ThreadStart( UpdateTexture_ThreadProc ) );


			initialized = true;

			updateTextureThread.Start( );
		}

		~MemoryToTextureRenderer( ) {
			Dispose( );
		}


		/*
		 * Makes sure we stop the media-player completely when it's no longer needed, 
		 * after that the GC can do it's job when it wants to.
		 */
		public void PrepareForDisposal( ) {
			updateTextureStopThreadWaitHandle.Set( );
		}

		public void Dispose( ) {
			updateTextureStopThreadWaitHandle.Set( );
			updateTextureThread.Join( );

			//cleanup all data we created
			try {
				//Log( LogType.Debug, "[CleanupDevice2DoubleTexture] current device " + device.ComPointer.ToInt64() );
				foreach ( Device d in device2DoubleTexture.Keys ) {
					device2DoubleTexture[d].Dispose( );
				}

				memoryToTextureRendererBusyMutex.Dispose( );

				updateTextureEventWaitHandle.Dispose( );
				updateTextureStopThreadWaitHandle.Dispose( );

			}
			catch ( Exception e ) {
				Log( LogType.Error, "[Dispose] " + e.Message );
			}

			Log( LogType.Debug, "[Dispose] done..." );

			// Use SupressFinalize in case a subclass of this type implements a finalizer.
			GC.SuppressFinalize( this );
		}

		//Called every vvvv frame, to do stuff that's only safe to do in the mainloop
		public void Evaluate( ) {
			//debug
			//return;

			prevTime = DateTime.Now.Ticks;
			//for ReportElapsedTime
			if ( memoryToTextureRendererBusyMutex.WaitOne( 200 ) ) {
				try {
					ReportElapsedTime( "Entering MUTEX", 15.7 );
					//prevTime = DateTime.Now.Ticks; //for ReportElapsedTime
					if ( textureNeedsResizingOnEvaluate ) {
						Log( LogType.Debug, "Texture needs to be resized to " + doubleBuffer.Width + "x" + doubleBuffer.Height );
						if ( ResizeTextureIfNecessary( ) ) {
							//Log( LogType.Debug, "resizing successful..." );
							textureNeedsResizingOnEvaluate = false;
							UpdateTexture_Threaded( );
						}
						ReportElapsedTime( "Resizing textures", 15.7 );
					}

					//only if 'front' renderer
					if ( this == parent.GetMemoryToTextureRendererCurrent( slice ) ) {
						if ( deviceDataNeedsUpdatingOnEvaluate ) {
							UpdateDeviceData( );

							deviceDataNeedsUpdatingOnEvaluate = false;
						}
					}

					ReportElapsedTime( "Updating device data", 15.7 );
				}
				catch ( Exception e ) {
					Log( LogType.Debug, "[Evaluate] Exception" + e.Message + "\n" + e.StackTrace );
				}
				finally {
					memoryToTextureRendererBusyMutex.ReleaseMutex( );
				}
			}
			else {
				ReportElapsedTime( "[Evaluate] Entering MUTEX FAILED and it ", 0 );
			}
		}

		private void DoubleBufferChanged( ) {
			//Log( LogType.Debug, "DoubleBufferChanged" );
			UpdateTexture_Threaded( );
		}

		public void UpdateTexture_ThreadProc( ) {
			while ( initialized ) {
				int waitHandleIndex = WaitHandle.WaitAny( new EventWaitHandle[2] {
					updateTextureEventWaitHandle,
					updateTextureStopThreadWaitHandle
				} );

				if ( waitHandleIndex == 0 ) {
					try {
						if ( ! UpdateTexture( null ) ) {
							//TRY AGAIN ?
							Log( LogType.Error, "[UpdateTexture_ThreadProc] UpdateTexture FAILED, should we try again ?" );
							//UpdateTexture_Threaded( );
						}
					}
					catch ( Exception e ) {
						Log( LogType.Error, "[UpdateTexture] Something went terribly wrong: " + e.Message + "\n" + e.StackTrace );
					}
					//Thread.Sleep(2);
				}
				else if ( waitHandleIndex == 1 ) {
					if ( memoryToTextureRendererBusyMutex.WaitOne( 10000 ) ) {
						memoryToTextureRendererBusyMutex.ReleaseMutex( );
					}
					else {

					}

					initialized = false;
					//break;
				}
			}
			Log( LogType.Debug, "... exiting updateTexture thread for memoryToTextureRenderer " + slice + " ... " );
		}

		public void UpdateTexture_Threaded( ) {
			//ThreadPool.QueueUserWorkItem( UpdateTexture, null );
			updateTextureEventWaitHandle.Set( );
		}

		//public void UpdateTexture( ) {
		//	UpdateTexture( null );
		//}

		private bool UpdateTexture( object unused ) {
			bool success = false;
			prevTime = DateTime.Now.Ticks; //for ReportElapsedTime
			if ( memoryToTextureRendererBusyMutex.WaitOne( 50 ) ) {
				//we will copy this information to the texture, but ONLY if the sizes match!
				//resizing and updating device data, will be done in the main thread (in Evaluate(), 
				//to make sure we don't get funky results)
				success = CopyMemoryToTexture( );
				memoryToTextureRendererBusyMutex.ReleaseMutex( );
			}
			else {
				ReportElapsedTime( "Entering MUTEX FAILED and it ", 0 );
			}
			return success;
		}

		private bool ResizeTextureIfNecessary( ) {
			bool success = true;
			//handle resize
			if ( doubleBuffer.LockFrontBufferForReading( 500 ) ) {
				//Device parentDeviceForDx9ExShared = null;
				foreach ( Device device in new List<Device>( device2DoubleTexture.Keys ) ) {

					DoubleTexture t2;
					if ( device2DoubleTexture.TryGetValue( device, out t2 )
							&& ( t2.Width != doubleBuffer.Width || t2.Height != doubleBuffer.Height )
						) {


						//Log( LogType.Debug, "[ResizeTextureIfNecessary] Recreating the texture because doubleTextSize != doubleBuffer size or " + t2.Width + "x" + t2.Height + " != " + doubleBuffer.Width + "x" + doubleBuffer.Height + " " );

						//remove old texture (will be done when a new one is created)
						//device2DoubleTexture[device].Dispose();
						//create a new one
						if ( CreateTexture( device ) == null ) {
							success = false;
						}
						else {
							//Log( LogType.Debug, "[ResizeTextureIfNecessary] Setting deviceDataNeedsUpdatingOnEvaluate from " + deviceDataNeedsUpdatingOnEvaluate + " to true!" );
							// Will be done IN CreateTexture: deviceDataNeedsUpdatingOnEvaluate = true;
						}
					}
				}
				doubleBuffer.UnlockFrontBuffer( );
			}
			return success;
		}

		private bool CopyMemoryToTexture( ) {
			return FillTextureUsingLockRectangle( );
		}


		/*
		 * This function should decide whether vvvv has been started with the dx9ex option, 
		 * and if so only create one (double)texture, with a sharedhandle 
		 * (instead of a different texture for each device)
		 * 
		 */
		//public Texture CreateOrReturnSharedTexture( Device device /*, Device parentDeviceForDx9ExShared*/ ) {
		//	Texture retVal = null;

		//	retVal = CreateTexture( device );

		//	return retVal;
		//}

		public Texture CreateTexture( Device device /*, Device parentDeviceForDx9ExShared */) {
			Texture retVal = null;
			DoubleTexture t2 = null;

			//return CreateManagedTexture(device, Math.Max( this.doubleBuffer.GetWidth(), 1 ), Math.Max( this.doubleBuffer.GetHeight(), 1 ) );
			if ( memoryToTextureRendererBusyMutex.WaitOne( ) ) {
				//Log( LogType.Debug, "CreateTexture on device " + device.ComPointer.ToInt64() + " ---------------------------------------------------------------- " );	
				t2 = CreateDoubleTexture( device );

				memoryToTextureRendererBusyMutex.ReleaseMutex( );

				retVal = t2 != null ? t2.FrontTexture : null;
			}
			else {
				Log( LogType.Error, "[CreateTexture ERROR] CreateTexture(device " + device.ComPointer.ToInt64( ) + ") FAILED because mutex not available !!!" );
				//This shouldn't happen
				int w = Math.Max( this.doubleBuffer.Width, 2 );
				int h = Math.Max( this.doubleBuffer.Height, 2 );
				return VlcUtils.CreateManagedTexture( device, w, h );
			}

			if ( retVal != null ) {
				//Log( LogType.Debug, "[CreateTexture] done, trying to update." );
				//if ( ( ! ( device is DeviceEx ) || ( device == t2.Device /* device == parentDeviceForDx9ExShared */ ) ) ) {
				UpdateTexture_Threaded( );

				deviceDataNeedsUpdatingOnEvaluate = true;
				//}
				//Log( LogType.Debug, "[CreateTexture] update should be ok. " );
			}
			else {
				Log( LogType.Error, "CreateTexture(" + device.ComPointer.ToInt64( ) + " FAILED" );
			}

			return retVal;
			//				return CreateManagedTexture(device, Math.Max(this.doubleBuffer.GetWidth(), 1), Math.Max(this.doubleBuffer.GetHeight(), 1));
		}

		///// <summary>
		///// Disposes double textures and clears all info about the texture from device2DoubleTexture etc.
		///// </summary>
		///// <param name="device"></param>
		///// <returns></returns>
		//public void DisposeTexture( Device device ) {

		//	if ( memoryToTextureRendererBusyMutex.WaitOne( ) ) {
		//		//Log( LogType.Debug, "CreateTexture on device " + device.ComPointer.ToInt64() + " ---------------------------------------------------------------- " );	

		//		DoubleTexture t2;
		//		try {
		//			if ( device2DoubleTexture.TryGetValue(device, out t2) ) {
		//				t2.Dispose( );
		//				device2DoubleTexture.Remove( device );
		//			}
		//		} 
		//		catch (Exception e) {
		//			Log( LogType.Error, "[DisposeTexture ERROR] Disposing of texture on device failed..." );
		//		}

		//		memoryToTextureRendererBusyMutex.ReleaseMutex( );
		//	}
		//	else {
		//		Log( LogType.Error, "[DisposeTexture ERROR] DisposeTexture(device " + device.ComPointer.ToInt64( ) + ") FAILED because mutex not available !!!" );
		//		//This shouldn't happen
		//	}

		//	deviceDataNeedsUpdatingOnEvaluate = true;
		//}





		private void doSharedTextureTests( Device device ) {
			if ( device is DeviceEx ) {
				byte[] raw = new byte[8];
				for ( int t = 0; t < 8; t++ ) { raw[t] = 0; }

				GCHandle h = GCHandle.Alloc( raw, GCHandleType.Pinned );

				IntPtr newHandle = IntPtr.Zero; //h.AddrOfPinnedObject();

				try {
					Texture newTexture0 = new Texture( device, 16, 16, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default, ref newHandle );
					Log( LogType.Debug, "usage.rendertarget pool.default CREATED" );
				}
				catch ( Exception e ) { Log( LogType.Debug, e.Source + "\n" + e.Message + "\n" + e.GetBaseException( ).Message + "\n" + e.StackTrace ); }
				try {
					Texture newTexture0 = new Texture( device, 16, 16, 1, Usage.None, Format.A8R8G8B8, Pool.Default, ref newHandle );
					Log( LogType.Debug, "usage.none pool.default CREATED" );
				}
				catch ( Exception e ) { Log( LogType.Debug, e.Source + "\n" + e.Message + "\n" + e.GetBaseException( ).Message + "\n" + e.StackTrace ); }
				try {
					Texture newTexture0 = new Texture( device, 16, 16, 1, Usage.None, Format.A8R8G8B8, Pool.Default, ref newHandle );
					Log( LogType.Debug, "usage.rendertarget pool.systemmem CREATED" );
				}
				catch ( Exception e ) { Log( LogType.Debug, e.Source + "\n" + e.Message + "\n" + e.GetBaseException( ).Message + "\n" + e.StackTrace ); }


				//free pinned memory
				try { h.Free( ); }
				catch ( Exception ) { }

				//							IntPtr newHandle0 = h0.AddrOfPinnedObject();
				//							IntPtr newHandle1 = h1.AddrOfPinnedObject();
				unsafe {
					//newHandle0 = Marshal.AllocHGlobal( 8 );
					//newHandle1 = Marshal.AllocHGlobal( 8 );
				}
				//Texture newTexture0 = VlcUtils.CreateSharedTexture(device, w, h, ref newHandle0);
				//Texture newTexture1 = VlcUtils.CreateSharedTexture(device, w, h, ref newHandle1);
				//							Texture newTexture0 = new Texture( device, width, height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, ref newHandle0 );
				//							Texture newTexture1 = new Texture( device, width, height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, ref newHandle1 );
			}
		}





		private DoubleTexture CreateDoubleTexture( Device device ) {
			int w = Math.Max( this.doubleBuffer.Width, 2 );
			int h = Math.Max( this.doubleBuffer.Height, 2 );
			//Log( LogType.Debug, "CreateDoubleTexture(" + " device " + device.ComPointer.ToInt64( ) + ", " + w + "x" + h + " ) called " );



			//doSharedTextureTests( device );




			DoubleTexture t2 = null;

			try {

				//if not null we found an existing texture created on another device
				DoubleTexture t2Base = null;
				if ( device is DeviceEx ) {
					//try to find an already existing shared texture with the right dimensions
					foreach ( DoubleTexture t2Temp in device2DoubleTexture.Values ) {
						if ( t2Temp.Width == w && t2Temp.Height == h && !t2Temp.Disposed && t2Temp.BaseDoubleTexture == null ) {
							//Log( LogType.Debug, "[CreateDoubleTexture] Found an existing shared DoubleTexture created on another device " + t2Temp.Device.ComPointer.ToInt64() );
							t2Base = t2Temp;
							break;
						}
					}
					if ( t2Base == null ) {
						//Log( LogType.Debug, "[CreateDoubleTexture] Need to create new DoubleTexture ..." + ( device is DeviceEx ? " (dx9ex shared is ON)" : " (dx9ex shared is OFF)" ) );
					}
				}

				//only create a new one if necessary
				t2 = t2Base != null && t2Base.Device == device ? t2Base : new DoubleTexture( device, w, h, t2Base );
				Log( LogType.Debug, "[CreateDoubleTexture] Created new DoubleTexture (" + t2.Width + "x" + t2.Height + " pitch/4=" + ( t2.Pitch / 4 ) + ") for device " + device.ComPointer + " created" + ( t2Base != null ? " based on sharedTexture on device " + t2Base.Device.ComPointer : "" ) + ( device is DeviceEx ? " (dx9ex shared is ON)" : " (dx9ex shared is OFF)" ) );

				if ( t2 != null && t2 != t2Base ) {
					DoubleTexture t2Temp;
					if ( device2DoubleTexture.TryGetValue( device, out t2Temp ) ) {
						try {
								//Log( LogType.Debug, "\tTrying to dispose old DoubleTexture (device  " + device.ComPointer.ToInt64( ) + ", " + t2Temp.Width + "x" + t2Temp.Height + ")" );
								//if ( t2.BaseDoubleTexture != t2Temp ) { 
								t2Temp.Dispose( );
								//}
						}
						catch {
							Log( LogType.Debug, "Disposing old DoubleTexture (device  " + device.ComPointer.ToInt64( ) + ", " + t2Temp.Width + "x" + t2Temp.Height + ") failed" );
						}
					}

					device2DoubleTexture[device] = t2;
					//Log( LogType.Debug, "Added new DoubleTexture to device2DoubleTexture..." );
				}

				//Log( LogType.Debug , (this == parent.mediaRendererA ? "A " : "B ") + (active ? "(FRONT) " + parent.FFileNameIn[GetMediaRendererIndex()] : "(BACK) " + parent.FNextFileNameIn[GetMediaRendererIndex()] ) + " Created new texture (" + w + "x" + h + ") for device " + device.ComPointer.ToInt64() + ". " );
			}
			catch ( Exception e ) {
				Log( LogType.Error, "[CreateDoubleTexture Exception] " + e.Message );
				//t2 = new DoubleTexture(device, 2, 2);
			}

			return t2;
		}

		/**
		 * Used to show the correct textures
		 */
		private void UpdateDeviceData( ) {
			try {
				CleanupDevice2DoubleTexture( );

				foreach ( Device d in parent.DeviceData.Keys ) {
					//Log( LogType.Debug, "k = " + k );
					DoubleTexture t2;
					//Device device = parent.FDeviceData[k].Data[slice].Device;
					if ( device2DoubleTexture.TryGetValue( d, out t2 ) ) {
						parent.DeviceData[d].Data[slice] = t2.FrontTexture;
					}
				}
			}
			catch ( Exception e ) {
				Log( LogType.Error, "[UpdateDeviceData Exception] " + e.Message );
			}
		}


		/** parameter = current device (this device should NOT be removed)
		 */
		private void CleanupDevice2DoubleTexture( ) {
			try {
				//Log( LogType.Debug, "[CleanupDevice2DoubleTexture] current device " + device.ComPointer.ToInt64() );

				List<Device> devicesToDelete = new List<Device>( );
				foreach ( Device d in device2DoubleTexture.Keys ) {
					//never remove current device
					//if (currentDevice.ComPointer.ToInt64().CompareTo(currentDevice) != 0) {
					//Log( LogType.Debug, "remove device " + d + " ?" );
					bool found = false;

					foreach ( Device k in parent.DeviceData.Keys ) {
						//Log( LogType.Debug, "  " + k + " " + ( FDeviceData[k].Data[mediaRendererIndex].Disposed ? "DISPOSED!!!" : "REMOVE DEVICE" ) );
						if ( k == d ) {
							found = true;
							break;
						}
					}
					if ( !found ) {
						//Log( LogType.Debug, "remove device " + d + " !!!!!!!!!!!!!" );
						devicesToDelete.Add( d );
					}
					//}
				}

				int x = 1;
				foreach ( Device d in devicesToDelete ) {
					//Log( LogType.Debug, "[CleanupDevice2DoubleTexture] trying to remove device " + d.ComPointer.ToInt64() + " being device " + ( x++ ) + " of " + devicesToDelete.Count + " devices to delete..." );
					device2DoubleTexture[d].Dispose( );
					device2DoubleTexture.Remove( d );
					//Log( LogType.Debug, "[CleanupDevice2DoubleTexture] removed device " + d.ComPointer.ToInt64( ) );
				}

				if ( devicesToDelete.Count > 0 ) {
					//Log( LogType.Debug, "[CleanupDevice2DoubleTexture] after cleaning up " + devicesToDelete.Count + " devices, there's only " + device2DoubleTexture.Keys.Count + " devices left !" );
				}

				//for dx9ex !!!
				if ( devicesToDelete.Count > 0 ) {
					List<Device> devicesToKeep = new List<Device>( );
					devicesToKeep.AddRange( device2DoubleTexture.Keys );

					foreach ( Device d in devicesToKeep ) {
						if ( d is DeviceEx ) {
							//Log( LogType.Debug, "[CleanupDevice2DoubleTexture] (DX9EX is ON) so recreating the texture on device " + d.ComPointer.ToInt64() );
							device2DoubleTexture[d].Dispose( );
							device2DoubleTexture.Remove( d );

							CreateTexture( d );

							//In this same thread, make sure the texture is filled with the right picture again
							CopyMemoryToTexture( );
						}
					}
				}
			}
			catch ( Exception e ) {
				Log( LogType.Error, "[CleanupDevice2DoubleTexture Exception (FDeviceData cleanup)] " + e.Message );
			}
		}


		public Texture GetFrontTexture( Device d ) {
			return device2DoubleTexture[d].FrontTexture;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>true if completely successful</returns>
		private bool FillTextureUsingLockRectangle( ) {
			bool success = true;
			if ( doubleBuffer.LockFrontBufferForReading( 500 ) ) {
				try {
					//if device already updated, don't do it again, this should only happen in case we're using dx9ex shared textures
					List<Device> devicesDone = new List<Device>( );

					//foreach ( Device d in device2DoubleTexture.Keys ) {
					//	Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "TEXTURE on device " + d.ComPointer.ToInt64() + " SHOULD BE updated..." );
					//}

					bool firstTextureFilled = false;
					foreach ( Device d in device2DoubleTexture.Keys ) {
						//Log( LogType.Debug, " fill doubletexture " + t2.BackTexture.ComPointer.ToInt64() + "\n" );
						try {
							DoubleTexture t2 = device2DoubleTexture[d];

							PluginInterfaces.V2.EX9.TextureDeviceData textureDeviceData;

							if ( t2 == null || t2.Disposed ) {
								success = false;
								Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] t2 == null or disposed on device " + d.ComPointer + " Does this mean we need to create a texture ourselves for device " + d.ComPointer + " ?" );
							}
							else if ( parent.DeviceData == null ) {
								success = false;
								Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] parent.DeviceData == null" );
							}
							else if ( t2.Device == null ) {
								success = false;
								Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] t2.Device == null" );
							}
							//THIS DOESN'T SEEM TO BE A PROBLEM !!!
							else if ( ! parent.DeviceData.TryGetValue(d, out textureDeviceData) ) {
								//success = false;
								//Log( LogType.Warning, "[FillTextureUsingLockRectangle WARNING] parent.DeviceData.TryGetValue(" + d.ComPointer + ", out textureDeviceData) = FALSE" );
							}
							
							if ( success ) {

								//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "TEXTURE on device " + d.ComPointer + " SHOULD BE updated..." );

								//string errDescription = "";


								// if dx9ex, only fill the first texture as the others are the same texture 
								// we COULD d this by checking if baseDoubleTexture is null?
								// Don't care about that, just fill the first texture you get, and don't do the other ones
								if ( firstTextureFilled && t2.Device is DeviceEx ) { //t2.BaseDoubleTexture != null ) { //
									//Log( LogType.Debug, "[FillTextureUsingLockRectangle DEBUG] We will not fill texture on device " + d.ComPointer + " because we already filled another texture with the same shared handle..." );
								}
								else if ( ! t2.LockBackTextureForWriting( 100 ) ) {
									success = false;
									Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] " + "t2.LockBackTextureForWriting( 100 ) FAILED (timeout?) !!!" );
								}
								else {
									//TEXTURE SUCCESSFULLY LOCKED for writing

									if ( t2.Width != doubleBuffer.Width || t2.Height != doubleBuffer.Height ) {
										success = false;
										textureNeedsResizingOnEvaluate = true;
										Log( LogType.Debug, "[FillTextureUsingLockRectangle WARNING] " + "TEXTURE size wrong !" );
									}
									else if ( t2.BackTexture == null ) {
										success = false;
										Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] " + "TEXTURE == null !!!" );
									}
									else if ( t2.BackTexture != null && t2.BackTexture.Disposed ) {
										success = false; 
										Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] " + "TEXTURE disposed !!!" );
									}
									else if ( devicesDone.Contains( t2.Device ) ) {
										//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "TEXTURE on device " + d.ComPointer + " already updated, don't do it again..." );
									}
									else if ( t2.BackTexture.Device.ComPointer.ToInt64() != d.ComPointer.ToInt64() ) {
										//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "t2.BackTexture.Device=" + t2.BackTexture.Device.ComPointer.ToInt64( ) + " t2.Device=" + d.ComPointer + " don't match. So we will not try to update this texture, but the one where the devices do match." );
									}
									else {
										//DataRectangle rDst = t2.BackTexture.LockRectangle(0, LockFlags.Discard);
										//rDst.Data.WriteRange( GetReadPixelPlane(), t2.GetPitch() * t2.GetHeight() );
										//t2.BackTexture.UnlockRectangle(0);

										DataRectangle rDst = null;
										try {
											//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "Trying to t2.BackTexture.LockRectangle(...) TEXTURE on device " + d.ComPointer + "" );

											rDst = t2.BackTexture.LockRectangle( 0, LockFlags.Discard );

											//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "Done t2.BackTexture.LockRectangle(...) TEXTURE on device " + d.ComPointer + "" );

											try {
												if ( t2.Pitch == t2.Width * 4 ) {
													//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "Trying to WriteRange TEXTURE on device " + d.ComPointer + "" );
													rDst.Data.WriteRange( doubleBuffer.FrontBuffer, t2.Pitch * t2.Height );
												}
												else {
													//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "Trying to Write LINE PER LINE TEXTURE on device " + d.ComPointer + "" );
													//writing line per line
													IntPtr fb = doubleBuffer.FrontBuffer;
													int width = doubleBuffer.Width * 4;
													int remainder = t2.Pitch - width;
													for ( int line = 0; line < t2.Height; line++ ) {
														rDst.Data.WriteRange( IntPtr.Add( fb, line * width ), width );
														rDst.Data.Position += remainder;
													}
												}
												//Log( LogType.Debug, "[FillTextureUsingLockRectangle] " + "Done writing TEXTURE on device " + d.ComPointer + "" );
											}
											catch {
												success = false;
												Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] writerange failed" );
											}

											try {
												t2.BackTexture.UnlockRectangle( 0 );

												if ( ! t2.ToggleFrontBack( ) ) {
													success = false;
													Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] t2.ToggleFrontBack( ) failed" );
												}

											}
											catch {
												success = false;
												Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] UNlockrectangle failed" );
											}
			
										}
										catch {
											success = false;
											Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] lockrectangle failed " + t2.BackTexture.ComPointer.ToInt64( ) + " ?= " + d.ComPointer );
										}
									}

									t2.UnlockBackTexture( );


									if ( success ) {
										deviceDataNeedsUpdatingOnEvaluate = true;
										devicesDone.Add( t2.Device );
										firstTextureFilled = true;

										//Log( LogType.Debug, "[FillTextureUsingLockRectangle] Updated texture on device " + d.ComPointer + "\n" );
									}
									else {
										deviceDataNeedsUpdatingOnEvaluate = true;
									}

								}
							}
						}
						catch ( Exception e ) {
							success = false;
							Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] " + e.Message + "\n" + e.StackTrace);
						}
					}
				}
				catch ( Exception e ) {
					success = false;
					Log( LogType.Error, "[FillTextureUsingLockRectangle ERROR] (source) " + e.Message );
				}

				doubleBuffer.UnlockFrontBuffer( );
			}
			else {
				success = false;
			}

			return success;
		}

		public int Slice {
			get { return slice; }
		}


		private void Log( LogType logType, string message ) {
			parent.Log( logType, "[MemoryToTextureRenderer " + group + slice + ( initialized ? ( parent.IsFrontMemoryToTextureRenderer( this ) ? "+" : "-" ) : "*" ) + "] " + message );
		}

		private long prevTime = DateTime.Now.Ticks;
		private long currTime = DateTime.Now.Ticks;
		private double ReportElapsedTime( string description, double reportOnlyIfMoreThanOrEqualToMillis ) {
			currTime = DateTime.Now.Ticks;

			double ms = (double)( currTime - prevTime ) / 10000;
			if ( ms >= reportOnlyIfMoreThanOrEqualToMillis ) {
				Log( LogType.Debug, description + " took " + ms + " milliseconds." );
			}
			prevTime = currTime;

			return ms;
		}

	}

}
