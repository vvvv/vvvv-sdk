using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
	public interface IInputPin
	{
		void Sync();
	}
	
	[ComVisible(false)]
	public class InputPin<T> : Pin<T>, IInputPin
	{
		public InputPin(IPluginHost host, IPluginIO pluginIO, IInStream<T> inStream)
			: base(host, pluginIO, new InputIOStream<T>(inStream))
		{
		}
		
		public void Sync()
		{
			// TODO: Implement changed stuff etc.
			FStream.Sync();
		}
		
		public override void Update()
		{
			FStream.Sync();
			base.Update();
		}
	}
	
	[ComVisible(false)]
	class InputIOStream<T> : IIOStream<T>
	{
		private readonly IInStream<T> FInStream;
		private readonly IIOStream<T> FIOStream;
		private IInStream<T> FCurrentInStream;
		
		public InputIOStream(IInStream<T> inStream)
		{
			FInStream = inStream;
			FIOStream = new ManagedIOStream<T>();
			FCurrentInStream = FInStream;
		}
		
		public int ReadPosition
		{
			get
			{
				return FCurrentInStream.ReadPosition;
			}
			set
			{
				FCurrentInStream.ReadPosition = value;
			}
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
		
		public bool Eof
		{
			get
			{
				return FCurrentInStream.Eof;
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
			return FCurrentInStream.Read(stride);
		}
		
		public int Read(T[] buffer, int index, int length, int stride)
		{
			return FCurrentInStream.Read(buffer, index, length, stride);
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stride)
		{
			FCurrentInStream.ReadCyclic(buffer, index, length, stride);
		}
		
		public bool Sync()
		{
			var changed = FInStream.Sync();
			FCurrentInStream = FInStream;
			return changed;
		}
		
		public void Reset()
		{
			ReadPosition = 0;
			WritePosition = 0;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public void Write(T value, int stride)
		{
			CopyOnWrite();
			FIOStream.Write(value, stride);
		}
		
		public int Write(T[] buffer, int index, int length, int stride)
		{
			CopyOnWrite();
			return FIOStream.Write(buffer, index, length, stride);
		}
		
		private void CopyOnWrite()
		{
			if (FCurrentInStream == FInStream)
			{
				// Save positions
				int readPosition = ReadPosition;
				int writePosition = WritePosition;
				
				// Reset the streams
				FInStream.Reset();
				FIOStream.Reset();
				
				// Copy data
				var buffer = FInStream.CreateReadBuffer();
				FIOStream.Length = FInStream.Length;
				while (!FInStream.Eof)
				{
					int n = FInStream.Read(buffer, 0, buffer.Length);
					FIOStream.Write(buffer, 0, n);
				}
				
				// Set current inStream to ioStream
				FCurrentInStream = FIOStream;
				
				// Restore positions
				ReadPosition = readPosition;
				WritePosition = writePosition;
			}
		}
		
		public void Flush()
		{
			FIOStream.Flush();
		}
	}
}
