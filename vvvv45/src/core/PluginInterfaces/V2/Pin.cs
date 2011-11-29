using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using SlimDX;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public delegate void PinUpdatedEventHandler(object sender, EventArgs args);
	
	[ComVisible(false)]
	public delegate void PinConnectionEventHandler(object sender, PinConnectionEventArgs args);
	
	[ComVisible(false)]
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
	
	[ComVisible(false)]
	public class Pin<T> : Spread<T>, IDisposable
	{
		private readonly IPluginHost FHost;
		private readonly IPluginIO FPluginIO;
		
		public Pin(IPluginHost host, IPluginIO pluginIO, IIOStream<T> stream)
			: base(stream)
		{
			FHost = host;
			FPluginIO = pluginIO;
			
//			FPluginIO.SetPinUpdater(this);
//			SliceCount = 1;
		}
		
		public override string ToString()
		{
			return base.ToString() + ": " + FPluginIO.Name;
		}
		
		public IPluginIO PluginIO
		{
			get
			{
				return FPluginIO;
			}
		}
		
		public event PinConnectionEventHandler Connected;
		
		public event PinConnectionEventHandler Disconnected;
		
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
//			Updated = null;
			FHost.DeletePin(this.PluginIO);
		}
		
		protected virtual void DisposeUnmanaged()
		{
			
		}
		
		#endregion
	}
}
