using System;

namespace VVVV.Utils.Streams
{
	// TODO: This should not be public
	public class ManagedIOStream<T> : IIOStream<T>
	{
		class StreamReader : IStreamReader<T>
		{
			private readonly ManagedIOStream<T> FStream;
			private readonly T[] FBuffer;
			private readonly int FLength;
			
			public StreamReader(ManagedIOStream<T> stream)
			{
				FStream = stream;
				FBuffer = stream.FBuffer;
				FLength = stream.FLength;
			}
			
			public void Dispose()
			{
				FStream.FRefCount--;
			}
			
			public bool Eos
			{
				get
				{
					return Position >= FLength;
				}
			}
			
			public int Position
			{
				get;
				set;
			}
			
			public int Length
			{
				get
				{
					return FLength;
				}
			}
			
			public void Reset()
			{
				Position = 0;
			}
			
			public T Read(int stride = 1)
			{
				var result = FBuffer[Position];
				Position += stride;
				return result;
			}
			
			public int Read(T[] buffer, int index, int length, int stride = 1)
			{
				int slicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				switch (stride)
				{
					case 0:
						if (index == 0 && slicesToRead == buffer.Length)
							buffer.Init(FBuffer[Position]); // Slightly faster
						else
							buffer.Fill(index, slicesToRead, FBuffer[Position]);
						break;
					case 1:
						Array.Copy(FBuffer, Position, buffer, index, slicesToRead);
						Position += slicesToRead * stride;
						break;
					default:
						for (int i = index; i < index + slicesToRead; i++)
						{
							buffer[i] = Read(stride);
						}
						break;
				}
				
				return slicesToRead;
			}
			
//			public void ReadCyclic(T[] buffer, int index, int length, int stride)
//			{
//				// Exception handling
//				if (Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
//				
//				// Normalize the stride
//				stride %= Length;
//				
//				switch (Length)
//				{
//					case 1:
//						// Special treatment for streams of length one
//						if (Eos) Reset();
//						
//						if (index == 0 && length == buffer.Length)
//							buffer.Init(Read(stride)); // Slightly faster
//						else
//							buffer.Fill(index, length, Read(stride));
//						break;
//					default:
//						int numSlicesRead = 0;
//						
//						// Read till end
//						while ((numSlicesRead < length) && (Position %= Length) > 0)
//						{
//							numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
//						}
//						
//						// Save start of possible block
//						int startIndex = index + numSlicesRead;
//						
//						// Read one block
//						while (numSlicesRead < length)
//						{
//							numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
//							// Exit the loop once ReadPosition is back at beginning
//							if ((Position %= Length) == 0) break;
//						}
//						
//						// Save end of possible block
//						int endIndex = index + numSlicesRead;
//						
//						// Calculate block size
//						int blockSize = endIndex - startIndex;
//						
//						// Now see if the block can be replicated to fill up the buffer
//						if (blockSize > 0)
//						{
//							int times = (length - numSlicesRead) / blockSize;
//							buffer.Replicate(startIndex, endIndex, times);
//							numSlicesRead += blockSize * times;
//						}
//						
//						// Read the rest
//						while (numSlicesRead < length)
//						{
//							if (Eos) Position %= Length;
//							numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
//						}
//						
//						break;
//				}
//			}
			
			public T Current
			{
				get;
				private set;
			}
			
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}
			
			public bool MoveNext()
			{
				var result = !Eos;
				if (result)
				{
					Current = Read();
				}
				return result;
			}
		}
		
		class StreamWriter : IStreamWriter<T>
		{
			private readonly ManagedIOStream<T> FStream;
			private readonly T[] FBuffer;
			private readonly int FLength;
			
			public StreamWriter(ManagedIOStream<T> stream)
			{
				FStream = stream;
				FBuffer = stream.FBuffer;
				FLength = stream.FLength;
			}
			
			public void Dispose()
			{
				FStream.FRefCount--;
			}
			
			public bool Eos
			{
				get
				{
					return Position >= FLength;
				}
			}
			
			public int Position
			{
				get;
				set;
			}
			
			public int Length
			{
				get
				{
					return FLength;
				}
			}
			
			public void Reset()
			{
				Position = 0;
			}
			
			public void Write(T value, int stride = 1)
			{
				FBuffer[Position] = value;
				Position += stride;
			}
			
			public int Write(T[] buffer, int index, int length, int stride = 1)
			{
				int slicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				switch (stride)
				{
					case 0:
						FBuffer[Position] = buffer[index + slicesToWrite - 1];
						break;
					case 1:
						Array.Copy(buffer, index, FBuffer, Position, slicesToWrite);
						Position += slicesToWrite;
						break;
					default:
						for (int i = index; i < index + slicesToWrite; i++)
						{
							FBuffer[Position] = buffer[i];
							Position += stride;
						}
						break;
				}
				
				return slicesToWrite;
			}
		}
		
		private T[] FBuffer;
		private int FLength;
		private int FLowerThreshold;
		private int FUpperThreshold;
		private int FDecreaseRequests;
		private int FRefCount;
		protected bool FChanged;
		
		public ManagedIOStream()
		{
			FBuffer = new T[0];
			FChanged = true;
		}
		
		public virtual bool Sync()
		{
			// Nothing to do
			return true;
		}
		
		public virtual void Flush()
		{
			FChanged = false;
		}
		
		public int Length
		{
			get
			{
				return FLength;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				
				if (value != FLength)
				{
					FLength = value;
					ResizeInternalBuffer();
					FChanged = true;
				}
			}
		}
		
		public IStreamReader<T> GetReader()
		{
			FRefCount++;
			return new StreamReader(this);
		}
		
		public IStreamWriter<T> GetWriter()
		{
			FRefCount++;
			FChanged = true;
			return new StreamWriter(this);
		}
		
		public object Clone()
		{
			var stream = new ManagedIOStream<T>();
			stream.Length = Length;
			Array.Copy(FBuffer, stream.FBuffer, FBuffer.Length);
			return stream;
		}
		
		private void ResizeInternalBuffer()
		{
			if (FLength > FUpperThreshold)
			{
				FUpperThreshold = NextHigher(FLength);
				FLowerThreshold = FUpperThreshold / 2;
				FDecreaseRequests = FUpperThreshold;

				var oldBuffer = FBuffer;
				FBuffer = new T[FUpperThreshold];

				BufferIncreased(oldBuffer, FBuffer);
			}
			else if (FLength < FLowerThreshold)
			{
				FDecreaseRequests--;

				if (FDecreaseRequests == 0)
				{
					FUpperThreshold = FUpperThreshold / 2;
					FLowerThreshold = FUpperThreshold / 2;

					var oldBuffer = FBuffer;
					FBuffer = new T[FUpperThreshold];

					BufferDecreased(oldBuffer, FBuffer);
				}
			}
			else
				FDecreaseRequests = FUpperThreshold;
		}
		
		protected virtual void BufferIncreased(T[] oldBuffer, T[] newBuffer)
		{
			Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
			if (oldBuffer.Length > 0)
			{
				for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
					newBuffer[i] = oldBuffer[i % oldBuffer.Length];
			}
		}
		
		protected virtual void BufferDecreased(T[] oldBuffer, T[] newBuffer)
		{
			Array.Copy(oldBuffer, newBuffer, newBuffer.Length);
		}
		
		private static int NextHigher(int k) {
			if (k == 0) return 1;
			k--;
			for (int i = 1; i < sizeof(int)*8; i <<= 1)
				k = k | k >> i;
			return k + 1;
		}
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return GetReader();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
