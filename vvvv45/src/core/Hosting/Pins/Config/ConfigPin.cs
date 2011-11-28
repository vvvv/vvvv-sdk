using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Config
{
	[ComVisible(false)]
	public class ConfigPin<T> : Pin<T>, IDiffSpread<T>, IInputPin
	{
		private readonly IPluginConfig FPluginConfig;
		
		public ConfigPin(IPluginHost host, IPluginConfig pluginConfig, IIOStream<T> stream)
			: base(host, pluginConfig, stream)
		{
			FPluginConfig = pluginConfig;
		}
		
		public event SpreadChangedEventHander<T> Changed;
		
		protected SpreadChangedEventHander FChanged;
		event SpreadChangedEventHander IDiffSpread.Changed
		{
			add
			{
				FChanged += value;
			}
			remove
			{
				FChanged -= value;
			}
		}
		
		protected virtual void OnChanged()
		{
			if (Changed != null)
				Changed(this);
			if (FChanged != null)
				FChanged(this);
		}
		
		public bool IsChanged
		{
			get
			{
				return FPluginConfig.PinIsChanged;
			}
		}
		
		public override bool Sync()
		{
			if (base.Sync())
			{
				OnChanged();
				return true;
			}
			
			return false;
		}
	}
	
	// In and out streams use the same data store. Simply delegate.
	class ConfigIOStream<T> : IIOStream<T>
	{
		private readonly IInStream<T> FInStream;
		private readonly IOutStream<T> FOutStream;
		
		public ConfigIOStream(IInStream<T> inStream, IOutStream<T> outStream)
		{
			FInStream = inStream;
			FOutStream = outStream;
		}
		
		public int Length
		{
			get
			{
				return FInStream.Length;
			}
			set
			{
				FOutStream.Length = value;
			}
		}
		
		public bool Sync()
		{
			return FInStream.Sync();
		}
		
		public object Clone()
		{
			return new ConfigIOStream<T>(
				FInStream.Clone() as IInStream<T>, 
				FOutStream.Clone() as IOutStream<T>
			);
		}
		
		public void Flush()
		{
			FOutStream.Flush();
		}
		
		public IStreamReader<T> GetReader()
		{
			return FInStream.GetReader();
		}
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return FInStream.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public IStreamWriter<T> GetWriter()
		{
			return FOutStream.GetWriter();
		}
	}
}
