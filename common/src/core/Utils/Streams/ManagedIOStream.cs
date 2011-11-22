using System;

namespace VVVV.Utils.Streams
{
	// TODO: This should not be public
	public class ManagedIOStream<T> : IIOStream<T>
	{
		private T[] FBuffer;
		private int FReadPosition;
		private int FWritePosition;
		private int FLength;
		private int FLowerThreshold;
		private int FUpperThreshold;
		private int FDecreaseRequests;
		
		public ManagedIOStream()
		{
			FBuffer = new T[0];
		}
		
		public object Clone()
		{
			var clonedStream = new ManagedIOStream<T>();
			clonedStream.FBuffer = FBuffer;
			clonedStream.FLength = FLength;
			clonedStream.FLowerThreshold = FLowerThreshold;
			clonedStream.FUpperThreshold = FUpperThreshold;
			clonedStream.FDecreaseRequests = FDecreaseRequests;
			clonedStream.ReadPosition = ReadPosition;
			clonedStream.WritePosition = WritePosition;
			return clonedStream;
		}
		
		public bool Sync()
		{
			// Nothing to do
			return true;
		}
		
		public void Flush()
		{
			// Nothing to do
		}
		
		public void Reset()
		{
			FReadPosition = 0;
			FWritePosition = 0;
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
					throw new ArgumentOutOfRangeException();
				
				if (value != FLength)
				{
					FLength = value;
					ResizeInternalBuffer();
				}
			}
		}
		
		public int ReadPosition
		{
			get
			{
				return FReadPosition;
			}
			set
			{
				FReadPosition = value;
			}
		}
		
		public int WritePosition
		{
			get
			{
				return FWritePosition;
			}
			set
			{
				FWritePosition = value;
			}
		}
		
		public bool Eof
		{
			get
			{
				return FReadPosition >= FLength || FWritePosition >= FLength;
			}
		}
		
		public T Read(int stride)
		{
			var result = FBuffer[FReadPosition];
			FReadPosition += stride;
			return result;
		}
		
		public int Read(T[] buffer, int index, int length, int stride = 1)
		{
			int slicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stride);
			
			switch (stride)
			{
				case 0:
					if (index == 0 && slicesToRead == buffer.Length)
						buffer.Init(FBuffer[FReadPosition]); // Slightly faster
					else
						buffer.Fill(index, slicesToRead, FBuffer[FReadPosition]);
					break;
				case 1:
					Array.Copy(FBuffer, FReadPosition, buffer, index, slicesToRead);
					FReadPosition += slicesToRead;
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
		
		public void ReadCyclic(T[] buffer, int index, int length, int stride)
		{
			// Exception handling
			if (Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
			
			// Normalize the stride
			stride %= Length;
			
			switch (Length)
			{
				case 1:
					// Special treatment for streams of length one
					if (Eof) Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(Read(stride)); // Slightly faster
					else
						buffer.Fill(index, length, Read(stride));
					break;
				default:
					int numSlicesRead = 0;
					
					// Read till end
					while ((numSlicesRead < length) && (ReadPosition %= Length) > 0)
					{
						numSlicesRead += Read(buffer, index, length, stride);
					}
					
					// Save start of possible block
					int startIndex = index + numSlicesRead;
					
					// Read one block
					while (numSlicesRead < length)
					{
						numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
						// Exit the loop once ReadPosition is back at beginning
						if ((ReadPosition %= Length) == 0) break;
					}
					
					// Save end of possible block
					int endIndex = index + numSlicesRead;
					
					// Calculate block size
					int blockSize = endIndex - startIndex;
					
					// Now see if the block can be replicated to fill up the buffer
					if (blockSize > 0)
					{
						int times = (length - numSlicesRead) / blockSize;
						buffer.Replicate(startIndex, endIndex, times);
						numSlicesRead += blockSize * times;
					}
					
					// Read the rest
					while (numSlicesRead < length)
					{
						if (Eof) ReadPosition %= Length;
						numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					break;
			}
		}
		
		public void Write(T value, int stride = 1)
		{
			FBuffer[FWritePosition] = value;
			FWritePosition += stride;
		}
		
		public int Write(T[] buffer, int index, int length, int stride = 1)
		{
			int slicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stride);
			
			switch (stride)
			{
				case 0:
					FBuffer[FWritePosition] = buffer[index + slicesToWrite - 1];
					break;
				case 1:
					Array.Copy(buffer, index, FBuffer, FWritePosition, slicesToWrite);
					FWritePosition += slicesToWrite;
					break;
				default:
					for (int i = index; i < index + slicesToWrite; i++)
					{
						FBuffer[FWritePosition] = buffer[i];
						FWritePosition += stride;
					}
					break;
			}
			
			return slicesToWrite;
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
	}
}
