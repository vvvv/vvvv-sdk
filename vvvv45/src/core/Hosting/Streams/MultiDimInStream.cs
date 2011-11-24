using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	public class MultiDimInStream<T> : IInStream<IInStream<T>>
	{
		class InnerStream : IInStream<T>
		{
			private readonly IInStream<T> FDataStream;
			private readonly int FOffset;
			
			public InnerStream(IInStream<T> dataStream, int offset, int length)
			{
				FDataStream = dataStream;
				FOffset = offset;
				Length = length;
			}
			
			public int ReadPosition
			{
				get;
				set;
			}
			
			public int Length
			{
				get;
				private set;
			}
			
			public bool Eof
			{
				get
				{
					return Length >= ReadPosition;
				}
			}
			
			public T Read(int stride)
			{
				int originalReadPosition = FDataStream.ReadPosition;
				FDataStream.ReadPosition = FOffset + ReadPosition;
				var value = FDataStream.Read(stride);
				ReadPosition += stride;
				FDataStream.ReadPosition = originalReadPosition;
				return value;
			}
			
			public int Read(T[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stride);
				
				int originalReadPosition = FDataStream.ReadPosition;
				FDataStream.ReadPosition = FOffset + ReadPosition;
				
				int numSlicesRead = FDataStream.Read(buffer, index, length, stride);
				
				if (numSlicesRead < numSlicesToRead)
				{
					FDataStream.ReadCyclic(buffer, index + numSlicesRead, numSlicesToRead - numSlicesRead, stride);
				}
				
				ReadPosition += numSlicesRead * stride;
				FDataStream.ReadPosition = originalReadPosition;
				return numSlicesToRead;
			}
			
			public void ReadCyclic(T[] buffer, int index, int length, int stride)
			{
				StreamUtils.ReadCyclic(this, buffer, index, length, stride);
			}
			
			public bool Sync()
			{
				// We're just a wrapper.
				return true;
			}
			
			public void Reset()
			{
				ReadPosition = 0;
			}
			
			public object Clone()
			{
				throw new NotImplementedException();
			}
		}
		
		private readonly IInStream<T> FDataStream;
		private readonly IInStream<int> FBinSizeStream;
		private readonly IIOStream<int> FNormBinSizeStream;
		private readonly int[] FBinSizeBuffer;
		
		public MultiDimInStream(IOFactory factory, InputAttribute attribute)
		{
			FDataStream = factory.CreateIO<IInStream<T>>(attribute);
			FBinSizeStream = factory.CreateIO<IInStream<int>>(
				new InputAttribute(string.Format("{0} Bin Size", attribute.Name))
			);
			FNormBinSizeStream = new ManagedIOStream<int>();
			FBinSizeBuffer = new int[16]; // 64 byte
		}
		
		public int ReadPosition
		{
			get;
			set;
		}
		
		public int Length
		{
			get;
			private set;
		}
		
		public bool Eof
		{
			get
			{
				return Length >= ReadPosition;
			}
		}
		
		public IInStream<T> Read(int stride)
		{
			if (Eof) throw new InvalidOperationException("Stream is EOF.");

			if (FNormBinSizeStream.Eof) 
			{
				// We need a modulo here :(
				FNormBinSizeStream.ReadPosition %= FNormBinSizeStream.Length;
			}
			if (FDataStream.Eof) 
			{
				// And another one :((
				FDataStream.ReadPosition %= FDataStream.Length;
			}
			
			int offset = FDataStream.ReadPosition;
			int length = FBinSizeStream.Read(1);
			
			FDataStream.ReadPosition += length;
			for (int i = 1; i < stride; i++)
			{
				FDataStream.ReadPosition += FBinSizeStream.Read(1);
			}
			
			ReadPosition += stride;
			
			return new InnerStream(FDataStream, offset, length);
		}
		
		public int Read(IInStream<T>[] buffer, int index, int length, int stride)
		{
			// TODO: Is there a faster solution?
			return StreamUtils.Read(this, buffer, index, length, stride);
		}
		
		public void ReadCyclic(IInStream<T>[] buffer, int index, int length, int stride)
		{
			StreamUtils.ReadCyclic(this, buffer, index, length, stride);
		}
		
		public bool Sync()
		{
			// Sync source
			bool dataChanged = FDataStream.Sync();
			bool binSizeChanged = FBinSizeStream.Sync();
			
			int dataLength = FDataStream.Length;
			int binSizeLength = FBinSizeStream.Length;
			int binSizeSum = 0;
			
			// Normalize bin size
			FNormBinSizeStream.Length = binSizeLength;
			while (!FBinSizeStream.Eof)
			{
				int numSlicesRead = FBinSizeStream.Read(FBinSizeBuffer, 0, binSizeLength);
				for (int i = 0; i < numSlicesRead; i++)
				{
					FBinSizeBuffer[i] = NormalizeBinSize(dataLength, FBinSizeBuffer[i]);
					binSizeSum += FBinSizeBuffer[i];
				}
				FNormBinSizeStream.Write(FBinSizeBuffer, 0, numSlicesRead);
			}
			
			int binTimes = DivByBinSize(dataLength, binSizeSum);
			binTimes = binTimes > 0 ? binTimes : 1;
			Length = binTimes * binSizeLength;
			
			return dataChanged || binSizeChanged;
		}
		
		public void Reset()
		{
			ReadPosition = 0;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		private static int NormalizeBinSize(int sliceCount, int binSize)
		{
			if (binSize < 0)
			{
				return DivByBinSize(sliceCount, Math.Abs(binSize));
			}
			
			return binSize;
		}
		
		private static int DivByBinSize(int sliceCount, int binSize)
		{
			Debug.Assert(binSize >= 0);
			
			if (binSize > 0)
			{
				int remainder = 0;
				int result = Math.DivRem(sliceCount, binSize, out remainder);
				if (remainder > 0)
					result++;
				return result;
			}
			return binSize;
		}
	}
}
