
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
	class OutputPin<T> : Pin<T>
	{
		public OutputPin(IPluginHost host, IPluginOut pluginOut, IIOStream<T> stream)
			: base(host, pluginOut, stream)
		{
		}
		
		public OutputPin(IPluginHost host, IPluginOut pluginOut, IOutStream<T> outStream)
			: this(host, pluginOut, new OutputIOStream<T>(outStream))
		{
		}
		
		public override void Update()
		{
			FStream.Flush();
			base.Update();
		}
	}
	
	class OutputIOStream<T> : IIOStream<T>
	{
		private readonly IIOStream<T> FIOStream;
		private readonly IOutStream<T> FOutStream;
		private readonly T[] FBuffer = new T[512];
		private bool FNeedsFlush;
		
		public OutputIOStream(IOutStream<T> outStream)
		{
			FIOStream = new ManagedIOStream<T>();
			FOutStream = outStream;
		}
		
		public int ReadPosition
		{
			get
			{
				return FIOStream.ReadPosition;
			}
			set
			{
				FIOStream.ReadPosition = value;
			}
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
		
		public bool Eof
		{
			get
			{
				return FIOStream.Eof;
			}
		}
		
		public int WritePosition
		{
			get
			{
				return FIOStream.WritePosition;
			}
			set
			{
				FIOStream.WritePosition = value;
			}
		}
		
		public T Read(int stride)
		{
			return FIOStream.Read(stride);
		}
		
		public int Read(T[] buffer, int index, int length, int stride)
		{
			return FIOStream.Read(buffer, index, length, stride);
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stride)
		{
			FIOStream.ReadCyclic(buffer, index, length, stride);
		}
		
		public void Sync()
		{
			// Nothing to do here.
		}
		
		public void Reset()
		{
			FIOStream.Reset();
			FOutStream.Reset();
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public void Write(T value, int stride)
		{
			FNeedsFlush = true;
			FIOStream.Write(value, stride);
		}
		
		public int Write(T[] buffer, int index, int length, int stride)
		{
			FNeedsFlush = true;
			return FIOStream.Write(buffer, index, length, stride);
		}
		
		public void Flush()
		{
			if (FNeedsFlush)
			{
				FNeedsFlush = false;
				
				// Write the buffered data to the out stream.
				FOutStream.Length = FIOStream.Length;
				
				FIOStream.Reset();
				while (!FIOStream.Eof)
				{
					int n = FIOStream.Read(FBuffer, 0, FBuffer.Length);
					FOutStream.Write(FBuffer, 0, n);
				}
			}
		}
	}
}
