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
		
		public void Dispose()
		{
			FHost.DeletePin(this.PluginIO);
		}
	}
}
