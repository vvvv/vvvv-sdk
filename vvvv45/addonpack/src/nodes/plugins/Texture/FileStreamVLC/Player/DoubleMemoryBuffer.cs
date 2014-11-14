/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 29/02/2012
 * Time: 17:47
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
using System.Runtime.InteropServices;


namespace VVVV.Nodes.Vlc.Player
{
	/// <summary>
	/// Description of DoubleMemoryBuffer.
	/// </summary>
	public class DoubleMemoryBuffer : IDisposable
	{
		private IntPtr pixelPlane0;
		private IntPtr pixelPlane1;
		private bool frontBuffer = false;
		//if false => texture0=front, true => texture1=front
		private int width, height, pitch;

		private ReaderWriterLockSlim frontBufferLock;
		private ReaderWriterLockSlim backBufferLock;
		//private Mutex decodeLock; //lock used for decoding => locks the writePixelPlane

		private int tryLockTimeout = 500;
		//millliseconds

		// What the event handler should look like
		public delegate void ToggleHandler();
		//event handler delegate: called when ToggleFrontBack sccessfully called
		// Public event that one can subscribe to
		public event ToggleHandler Toggle;

		public DoubleMemoryBuffer(int w, int h)
		{
			frontBufferLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			backBufferLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			SetNewSize(w, h);
		}
		~DoubleMemoryBuffer()
		{
			Dispose();
		}


		public IntPtr FrontBuffer
		{
			get { return (frontBuffer ? pixelPlane1 : pixelPlane0); }
		}
		public IntPtr BackBuffer
		{
			get { return (frontBuffer ? pixelPlane0 : pixelPlane1); }
		}
		public bool LockFrontBufferForReading(int millisecondsTimeout)
		{
			if (frontBufferLock.IsWriteLockHeld) {
				frontBufferLock.ExitWriteLock();
			}
			return frontBufferLock.TryEnterReadLock(millisecondsTimeout);
		}
		private bool LockFrontBufferForWriting(int millisecondsTimeout)
		{
			return frontBufferLock.TryEnterWriteLock(millisecondsTimeout);
		}
		public void UnlockFrontBuffer()
		{
			if (frontBufferLock.IsWriteLockHeld)
				frontBufferLock.ExitWriteLock();
			if (frontBufferLock.IsReadLockHeld)
				frontBufferLock.ExitReadLock();
		}

		private bool LockBackBufferForReading(int millisecondsTimeout)
		{
			if (backBufferLock.IsWriteLockHeld) {
				backBufferLock.ExitWriteLock();
			}
			return backBufferLock.TryEnterReadLock(millisecondsTimeout);
		}
		public bool LockBackBufferForWriting(int millisecondsTimeout)
		{
			return backBufferLock.TryEnterWriteLock(millisecondsTimeout);
		}
		public void UnlockBackBuffer()
		{
			if (backBufferLock.IsWriteLockHeld)
				backBufferLock.ExitWriteLock();
			if (backBufferLock.IsReadLockHeld)
				backBufferLock.ExitReadLock();
		}

		private bool LockBothBuffers(int millisecondsTimeout)
		{
			if (LockFrontBufferForWriting(tryLockTimeout)) {
				if (LockBackBufferForWriting(tryLockTimeout)) {
					return true;
				}
			}
			//UnlockBothBuffers();
			return false;
		}
		private void UnlockBothBuffers()
		{
			UnlockBackBuffer();
			UnlockFrontBuffer();
		}

		public bool ToggleFrontBack()
		{
			if (LockBothBuffers(tryLockTimeout)) {
				frontBuffer = !frontBuffer;
				UnlockBothBuffers();
				OnToggle();
				return true;
			}

			return false;
		}

		// The method which fires the Event
		protected void OnToggle()
		{
			// Check if there are any Subscribers
			if (Toggle != null) {
				// Call the Event
				Toggle();
			}
		}

		public bool SetNewSize(int w, int h)
		{
			if ( pixelPlane0 != null && pixelPlane1 != null && width == w && height == h ) {
				//do nothing
			} else {
				if ( LockBothBuffers(tryLockTimeout) ) {
					try {
						//only reallocate if a bigger buffer is requested? ( => possibly some wasted memory )
						if ( w * h > width * height ) {
							Dispose();
							pixelPlane0 = Marshal.AllocHGlobal(w * h * 4 + 32);
							pixelPlane1 = Marshal.AllocHGlobal(w * h * 4 + 32);
						}

						width = w;
						height = h;
						pitch = w * 4;
//						} catch (Exception e) {
						
					} finally {
						UnlockBothBuffers();
					}
				} else {
					return false;
				}
			}
			return true;
		}

		public int Width
		{
			get { return width; }
		}
		public int Height
		{
			get { return height; }
		}
		public int Pitch
		{
			get { return pitch; }
		}
		public void Dispose()
		{
			if (pixelPlane0 != null) {
				/*try {*/					Marshal.FreeHGlobal( pixelPlane0 );
				/*} catch {}*/				}				
			if (pixelPlane1 != null) {
				/*try {*/					Marshal.FreeHGlobal( pixelPlane1 );
				/*} catch {}*/				}				

			// Use SupressFinalize in case a subclass of this type implements a finalizer.
			GC.SuppressFinalize( this );

		}
	}
}
