/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 29/02/2012
 * Time: 17:35
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
using VVVV.Nodes.Vlc.Utils;
using System.Threading;

namespace VVVV.Nodes.Vlc.Player {
	/// <summary>
	/// Description of DoubleTexture.
	/// </summary>
	public class DoubleTexture : IDisposable {
		private Texture texture0 = null;
		private Texture texture1 = null;
		//handles and parentDoubleTexture only used when creating shared textures
		private DoubleTexture dx9exSharedTexture = null; //texture this texture will be based upon (we will use its shared handke to create this texture, making them the same texture !!!)
		private IntPtr texture0SharedHandle = IntPtr.Zero;
		private IntPtr texture1SharedHandle = IntPtr.Zero;
		//private DoubleTexture parentDoubleTexture = null;

		private bool frontBuffer = false; //if false => texture0=front, true => texture1=front
		private int width, height, pitch;
		private Device device;

		private ReaderWriterLockSlim frontBufferLock;
		private ReaderWriterLockSlim backBufferLock;

		private int tryLockTimeout = 500; //millliseconds
		// What the event handler should look like
		public delegate void ToggleHandler( );
		//event handler delegate: called when ToggleFrontBack sccessfully called
		// Public event that one can subscribe to
		public event ToggleHandler Toggle;

		public DoubleTexture( Device d, int w, int h )
			: this( d, w, h, null ) {
			/*
							device = d;
							IntPtr handle = new IntPtr();

							frontBufferLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
							backBufferLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

							SetNewSize(w, h);
			*/
		}

		/*
		 * This is a constructor that is based on another texture. This one 
		 * should be used if we are using dx9ex shared textures, and if this
		 * texture should basically be the same (directx can share textures between devices now)
		 *  as the other one.
		 */
		public DoubleTexture( Device d, int w, int h, DoubleTexture sharedTexture ) {
			device = d;
			//parentDoubleTexture = sharedTexture;
			dx9exSharedTexture = sharedTexture;

			frontBufferLock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
			backBufferLock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );

			SetNewSize( w, h );
		}

		~DoubleTexture( ) {
			Dispose( );
		}

		public bool Disposed {
			get { return ( texture0 != null && texture0.Disposed ) || ( texture1 != null && texture1.Disposed ); }
		}

		public Texture FrontTexture {
			get { return ( frontBuffer ? texture1 : texture0 ); }
		}
		public Texture BackTexture {
			get { return ( frontBuffer ? texture0 : texture1 ); }
		}

		/// <summary>
		/// If dx9ex, the texture this texture is based upon !!!
		/// </summary>
		public DoubleTexture BaseDoubleTexture {
			get { return dx9exSharedTexture; }
		}


		public IntPtr FrontTextureHandle {
			get { return ( frontBuffer ? texture1SharedHandle : texture0SharedHandle ); }
			//set { seconds = value * 3600; }
		}

		public IntPtr BackTextureHandle {
			get { return ( frontBuffer ? texture0SharedHandle : texture1SharedHandle ); }
			//set { seconds = value * 3600; }
		}


		public bool LockFrontTextureForReading( int millisecondsTimeout ) {
			if ( frontBufferLock.IsWriteLockHeld ) {
				frontBufferLock.ExitWriteLock( );
			}
			return frontBufferLock.TryEnterReadLock( millisecondsTimeout );
		}
		private bool LockFrontTextureForWriting( int millisecondsTimeout ) {
			return frontBufferLock.TryEnterWriteLock( millisecondsTimeout );
		}

		public void UnlockFrontTexture( ) {
			if ( frontBufferLock.IsWriteLockHeld )
				frontBufferLock.ExitWriteLock( );
			if ( frontBufferLock.IsReadLockHeld )
				frontBufferLock.ExitReadLock( );
		}
		private bool LockBackTextureForReading( int millisecondsTimeout ) {
			if ( backBufferLock.IsWriteLockHeld ) {
				backBufferLock.ExitWriteLock( );
			}
			return backBufferLock.TryEnterReadLock( millisecondsTimeout );
		}

		public bool LockBackTextureForWriting( int millisecondsTimeout ) {
			return backBufferLock.TryEnterWriteLock( millisecondsTimeout );
		}
		public void UnlockBackTexture( ) {
			if ( backBufferLock.IsWriteLockHeld )
				backBufferLock.ExitWriteLock( );
			if ( backBufferLock.IsReadLockHeld )
				backBufferLock.ExitReadLock( );
		}

		private bool LockBothTextures( int millisecondsTimeout ) {
			if ( LockFrontTextureForWriting( tryLockTimeout ) ) {
				if ( LockBackTextureForWriting( tryLockTimeout ) ) {
					return true;
				}
			}
			//UnlockBothTextures();
			return false;
		}
		private void UnlockBothTextures( ) {
			UnlockBackTexture( );
			UnlockFrontTexture( );
		}

		public bool ToggleFrontBack( ) {
			if ( LockBothTextures( tryLockTimeout ) ) {
				frontBuffer = !frontBuffer;
				UnlockBothTextures( );
				OnToggle( );
				return true;
			}
			return false;
		}

		// The method which fires the Event
		protected void OnToggle( ) {
			// Check if there are any Subscribers
			if ( Toggle != null ) {
				// Call the Event
				Toggle( );
			}
		}

		/*
		 * Simply creating a new DoubleTexture is probably a lot safer, 
		 * so maybe this function shouldn't be public...
		 * (like if the caller still uses references to the old back and front textures)
		 */
		public int SetNewSize( int w, int h ) {
			//if ( texture0 != null && texture1 != null && !texture0.Disposed && !texture1.Disposed && width == w && height == h ) {
			//    //do nothing
			//    return 0;
			//}
			//else 
			if ( device is DeviceEx ) {
				return CreateTexturesIfDeviceSharingOn( w, h );
			}
			else {
				return CreateTexturesIfDeviceSharingOff( w, h );
			}


		}


		private int CreateTexturesIfDeviceSharingOff( int w, int h ) {
			if ( LockBothTextures( tryLockTimeout ) ) {
				try {
					//if ( texture0 != null ) { device = texture0.Device; }

					Texture newTexture0 = VlcUtils.CreateManagedTexture( device, w, h );
					Texture newTexture1 = VlcUtils.CreateManagedTexture( device, w, h );

					Dispose( );

					texture0 = newTexture0;
					texture1 = newTexture1;

					width = w;
					height = h;
					try {
						DataRectangle r = texture0.LockRectangle( 0, LockFlags.Discard );
						if ( r != null )
							pitch = r.Pitch;
						texture0.UnlockRectangle( 0 );
					}
					catch {
						pitch = w;
					}

					if ( device == null ) {
						//Device seems to become null (only when calling SetNewSize only instead of from constructor?), 
						//don't know why. Maybe because we disposed the old textures...
						device = texture0.Device;
					}
				}
				finally {
					UnlockBothTextures( );
				}
			}
			else {
				return -3;
			}
			return 0;
		}

		private int CreateTexturesIfDeviceSharingOn( int w, int h ) {
			if ( LockBothTextures( tryLockTimeout ) ) {
				try {
					//Texture newTexture0 = TextureUtils.CreateTexture(device, w, h);
					//Texture newTexture1 = TextureUtils.CreateTexture(device, w, h);						

					IntPtr newHandle0 = dx9exSharedTexture != null ? dx9exSharedTexture.texture0SharedHandle : IntPtr.Zero;
					Texture newTexture0 = new Texture( device, w, h, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, ref newHandle0 );

					IntPtr newHandle1 = dx9exSharedTexture != null ? dx9exSharedTexture.texture1SharedHandle : IntPtr.Zero;
					Texture newTexture1 = new Texture( device, w, h, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, ref newHandle1 );

					Dispose( );

					texture0 = newTexture0;
					texture1 = newTexture1;

					texture0SharedHandle = newHandle0;
					texture1SharedHandle = newHandle1;

					width = w;
					height = h;
					try {
						DataRectangle r = texture0.LockRectangle( 0, LockFlags.Discard );
						if ( r != null )
							pitch = r.Pitch;
						texture0.UnlockRectangle( 0 );
					}
					catch {
						pitch = w;
					}

					if ( device == null ) {
						//Device seems to become null (only when calling SetNewSize only instead of from constructor?), 
						//don't know why. Maybe because we disposed the old textures...
						device = texture0.Device;
					}
				}
				finally {
					UnlockBothTextures( );
				}
			}
			else {
				return -3;
			}
			return 0;
		}

		public int Width {
			get { return width; }
		}
		public int Height {
			get { return height; }
		}
		public int Pitch {
			get { return pitch; }
		}

		public Device Device {
			get { return texture0 != null ? texture0.Device : null; }
		}

		public void Dispose( ) {
			if ( texture0 != null && !texture0.Disposed ) {
				try {
					texture0.Dispose( );
				}
				catch { }
			}
			if ( texture1 != null && !texture1.Disposed ) {
				try {
					texture1.Dispose( );
				}
				catch { }
			}

			// Use SupressFinalize in case a subclass of this type implements a finalizer.
			GC.SuppressFinalize( this );
		}

	}
}
