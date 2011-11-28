
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
	interface IOutputPin
	{
		void Flush();
	}
	
	class OutputPin<T> : Pin<T>, IOutputPin
	{
		public OutputPin(IPluginHost host, IPluginOut pluginOut, IIOStream<T> stream)
			: base(host, pluginOut, stream)
		{
		}
		
		public OutputPin(IPluginHost host, IPluginOut pluginOut, IOutStream<T> outStream)
			: this(host, pluginOut, new OutputIOStream<T>(outStream))
		{
		}
	}
	
	class OutputIOStream<T> : IIOStream<T>
	{
		private readonly IIOStream<T> FIOStream;
		private readonly IOutStream<T> FOutStream;
		private readonly T[] FBuffer = new T[StreamUtils.BUFFER_SIZE];
		private bool FNeedsFlush;
		
		public OutputIOStream(IOutStream<T> outStream)
		{
			FIOStream = new ManagedIOStream<T>();
			FOutStream = outStream;
		}
		
		public int Length
		{
			get
			{
				return FIOStream.Length;
			}
			set
			{
				if (FIOStream.Length != value)
				{
					FNeedsFlush = true;
					FIOStream.Length = value;
				}
			}
		}
		
		public bool Sync()
		{
			// Nothing to do here.
			return true;
		}
		
		public object Clone()
		{
			return new OutputIOStream<T>(FOutStream.Clone() as IOutStream<T>);
		}
		
		public void Flush()
		{
			if (FNeedsFlush)
			{
				FNeedsFlush = false;
				
				// Write the buffered data to the out stream.
				FOutStream.Length = FIOStream.Length;
				
				using (var reader = FIOStream.GetReader())
				{
					using (var writer = FOutStream.GetWriter())
					{
						while (!reader.Eos)
						{
							int n = reader.Read(FBuffer, 0, FBuffer.Length);
							writer.Write(FBuffer, 0, n);
						}
					}
				}
			}
		}
		
		public IStreamReader<T> GetReader()
		{
			return FIOStream.GetReader();
		}
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return FIOStream.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public IStreamWriter<T> GetWriter()
		{
			FNeedsFlush = true;
			return FIOStream.GetWriter();
		}
	}
}
