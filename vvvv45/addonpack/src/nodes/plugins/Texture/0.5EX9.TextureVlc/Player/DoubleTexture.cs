/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 29/02/2012
 * Time: 17:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
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

namespace VVVV.Nodes.Vlc.Player
{
	/// <summary>
	/// Description of DoubleTexture.
	/// </summary>
		public class DoubleTexture : IDisposable
		{
			private Texture texture0;
			private Texture texture1;
			private bool frontBuffer = false;
			//if false => texture0=front, true => texture1=front
			private int width, height, pitch;
			private Device device;

			private ReaderWriterLockSlim frontBufferLock;
			private ReaderWriterLockSlim backBufferLock;

			private int tryLockTimeout = 500;
			//millliseconds
			// What the event handler should look like
			public delegate void ToggleHandler();
			//event handler delegate: called when ToggleFrontBack sccessfully called
			// Public event that one can subscribe to
			public event ToggleHandler Toggle;

			public DoubleTexture(Device d, int w, int h)
			{
				device = d;
				IntPtr handle = new IntPtr();

				frontBufferLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
				backBufferLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

					/*, ref handle*/				SetNewSize(w, h)				;
			}
			/*			public DoubleTexture(Device d, int w, int h , ref IntPtr handleForSharedTexture ) {
				device = d;

				frontBufferLock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
				backBufferLock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );

				SetNewSize(w, h); //, ref handleForSharedTexture
			}
*/
			~DoubleTexture()
			{
				Dispose();
			}
			public Texture GetFrontTexture()
			{
				return (frontBuffer ? texture1 : texture0);
			}
			public Texture GetBackTexture()
			{
				return (frontBuffer ? texture0 : texture1);
			}

			public bool LockFrontTextureForReading(int millisecondsTimeout)
			{
				if (frontBufferLock.IsWriteLockHeld) {
					frontBufferLock.ExitWriteLock();
				}
				return frontBufferLock.TryEnterReadLock(millisecondsTimeout);
			}
			private bool LockFrontTextureForWriting(int millisecondsTimeout)
			{
				return frontBufferLock.TryEnterWriteLock(millisecondsTimeout);
			}

			public void UnlockFrontTexture()
			{
				if (frontBufferLock.IsWriteLockHeld)
					frontBufferLock.ExitWriteLock();
				if (frontBufferLock.IsReadLockHeld)
					frontBufferLock.ExitReadLock();
			}
			private bool LockBackTextureForReading(int millisecondsTimeout)
			{
				if (backBufferLock.IsWriteLockHeld) {
					backBufferLock.ExitWriteLock();
				}
				return backBufferLock.TryEnterReadLock(millisecondsTimeout);
			}

			public bool LockBackTextureForWriting(int millisecondsTimeout)
			{
				return backBufferLock.TryEnterWriteLock(millisecondsTimeout);
			}
			public void UnlockBackTexture()
			{
				if (backBufferLock.IsWriteLockHeld)
					backBufferLock.ExitWriteLock();
				if (backBufferLock.IsReadLockHeld)
					backBufferLock.ExitReadLock();
			}

			private bool LockBothTextures(int millisecondsTimeout)
			{
				if (LockFrontTextureForWriting(tryLockTimeout)) {
					if (LockBackTextureForWriting(tryLockTimeout)) {
						return true;
					}
				}
				//UnlockBothTextures();
				return false;
			}
			private void UnlockBothTextures()
			{
				UnlockBackTexture();
				UnlockFrontTexture();
			}

			public bool ToggleFrontBack()
			{
				if (LockBothTextures(tryLockTimeout)) {
					frontBuffer = !frontBuffer;
					UnlockBothTextures();
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
			public int SetNewSize(int w, int h)			/*, ref IntPtr handleForSharedTexture*/
			{
				if (texture0 != null && texture1 != null && !texture0.Disposed && !texture1.Disposed && width == w && height == h) {
					//do nothing
					return 0;
				} else {
					if (texture0 != null)
						device = texture0.Device;

					if (LockBothTextures(tryLockTimeout)) {
						try {
							//new Texture(device, w, h, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
							Texture newTexture0 = VlcUtils.CreateManagedTexture(device, w, h);
							Texture newTexture1 = VlcUtils.CreateManagedTexture(device, w, h);

							Dispose();

							texture0 = newTexture0;
							texture1 = newTexture1;

							//texture0 = CreateDefaultTexture(device, w, h);
							//texture1 = CreateDefaultTexture(device, w, h);

							width = w;
							height = h;
							try {
								DataRectangle r = texture0.LockRectangle(0, LockFlags.Discard);
								if (r != null)
									pitch = r.Pitch;
								texture0.UnlockRectangle(0);
							} catch {
								pitch = w;
							}

							//Device seems to become null, don't know why
							device = texture0.Device;
						} finally {
							UnlockBothTextures();
						}
					} else {
						return -3;
					}
					return 0;
				}

			}
			public int GetWidth()
			{
				return width;
			}
			public int GetHeight()
			{
				return height;
			}
			public int GetPitch()
			{
				return pitch;
			}
			public Device GetDevice()
			{
				return texture0 != null ? texture0.Device : null;
			}
			public void Dispose()
			{
				if (texture0 != null && !texture0.Disposed) {
					/*try {*/					texture0.Dispose();
					/*} catch {}*/				}				
				if (texture1 != null && !texture1.Disposed) {
					/*try {*/					texture1.Dispose();
					/*} catch {}*/				}				
			}
		}
}
