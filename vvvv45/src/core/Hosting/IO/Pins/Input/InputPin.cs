using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
	[ComVisible(false)]
	class InputPin<T> : Pin<T>
	{
		public InputPin(IPluginHost host, IPluginIO pluginIO, IInStream<T> inStream)
			: base(host, pluginIO, new InputIOStream<T>(inStream))
		{
		}
	}
	
	[ComVisible(false)]
	class InputIOStream<T> : IIOStream<T>
	{
		private readonly IInStream<T> FInStream;
		private readonly IIOStream<T> FIOStream;
		private readonly T[] FBuffer = new T[StreamUtils.BUFFER_SIZE];
		private IInStream<T> FCurrentInStream;
		
		public InputIOStream(IInStream<T> inStream)
		{
			FInStream = inStream;
			FIOStream = new ManagedIOStream<T>();
			FCurrentInStream = FInStream;
		}
		
		public int Length
		{
			get
			{
				return FCurrentInStream.Length;
			}
			set
			{
				if (Length != value)
				{
					CopyOnWrite();
					FIOStream.Length = value;
				}
			}
		}
		
		public bool Sync()
		{
			var changed = FInStream.Sync();
			FCurrentInStream = FInStream;
			return changed;
		}
		
		public object Clone()
		{
			return new InputIOStream<T>(FInStream.Clone() as IInStream<T>);
		}
		
		private void CopyOnWrite()
		{
			if (FCurrentInStream == FInStream)
			{
				// Copy data
				FIOStream.Length = FInStream.Length;
				using (var reader = FInStream.GetReader())
				{
					using (var writer = FIOStream.GetWriter())
					{
						while (!reader.Eos)
						{
							int n = reader.Read(FBuffer, 0, FBuffer.Length);
							writer.Write(FBuffer, 0, n);
						}
					}
				}
				
				// Set current inStream to ioStream
				FCurrentInStream = FIOStream;
			}
		}
		
		public void Flush()
		{
			FIOStream.Flush();
		}
		
		public IStreamReader<T> GetReader()
		{
			return FCurrentInStream.GetReader();
		}
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return FCurrentInStream.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public IStreamWriter<T> GetWriter()
		{
			CopyOnWrite();
			return FIOStream.GetWriter();
		}
	}
}
