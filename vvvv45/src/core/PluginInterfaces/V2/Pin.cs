using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using SlimDX;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void PinUpdatedEventHandler(object sender, EventArgs args);
	public delegate void PinConnectionEventHandler(object sender, PinConnectionEventArgs args);
	
	public class PinConnectionEventArgs : EventArgs
	{
		public IPin OtherPin
		{
			get;
			private set;
		}
		
		public PinConnectionEventArgs(IPin otherPin)
		{
			OtherPin = otherPin;
		}
	}
	
	public abstract class Pin<T> : ISpread<T>, IDisposable, IPinUpdater
	{
		[Import]
		protected ILogger FLogger;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		protected T[] FData;
		protected int FSliceCount;
		
		public Pin(IPluginHost host, PinAttribute attribute)
		{
			FHost = host;
			FAttribute = attribute;
		}
		
		/// <summary>
		/// Must be called by subclass at end of constructor.
		/// </summary>
		protected void Initialize(IPluginIO pluginIO)
		{
			PluginIO = pluginIO;
			PluginIO.SetPinUpdater(this);
			PluginIO.Order = FAttribute.Order;
			SliceCount = 1;
		}
		
		public IPluginIO PluginIO
		{
			get;
			private set;
		}
		
		public event PinUpdatedEventHandler Updated;
		
		protected virtual void OnUpdated()
		{
			if (Updated != null)
			{
				Updated(this, EventArgs.Empty);
			}
		}
		
		public event PinConnectionEventHandler Connected;
		
		protected virtual void OnConnected(PinConnectionEventArgs args)
		{
			if (Connected != null) {
				Connected(this, args);
			}
		}
		
		public event PinConnectionEventHandler Disconnected;
		
		protected virtual void OnDisconnected(PinConnectionEventArgs args)
		{
			if (Disconnected != null) {
				Disconnected(this, args);
			}
		}
		
		public virtual T this[int index]
		{
			get
			{
				return FData[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
		
		public virtual int SliceCount
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
				{
					var old = FData;
					FData = new T[value];
					
					if (old != null && old.Length > 0)
					{
						for (int i = 0; i < FData.Length; i++)
						{
							FData[i] = old[i % old.Length];
						}
					}
					
					FSliceCount = value;
				}
			}
		}
		
		//prepare for IPinUpdater
		public virtual void Update()
		{
			OnUpdated();
		}
		
		public virtual void Connect(IPin otherPin)
		{
			OnConnected(new PinConnectionEventArgs(otherPin));
		}
		
		public virtual void Disconnect(IPin otherPin)
		{
			OnDisconnected(new PinConnectionEventArgs(otherPin));
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < SliceCount; i++)
				yield return this[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		#region IDisposable
		
		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		private bool FDisposed;
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				// If disposing equals true, dispose all managed
				// and unmanaged resources.
				if(disposing)
				{
					// Dispose managed resources.
					DisposeManaged();
				}

				// Call the appropriate methods to clean up
				// unmanaged resources here.
				// If disposing is false,
				// only the following code is executed.
				DisposeUnmanaged();

				// Note disposing has been done.
				FDisposed = true;
			}
		}
		
		protected virtual void DisposeManaged()
		{
			Updated = null;
		}
		
		protected virtual void DisposeUnmanaged()
		{
			
		}
		
		#endregion
	}
}
