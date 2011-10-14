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
		
		public void Sync()
		{
			// Nothing to do
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
		
		public T Read(int stepSize)
		{
			var result = FBuffer[FReadPosition];
			FReadPosition += stepSize;
			return result;
		}
		
		public int Read(T[] buffer, int index, int length, int stepSize = 1)
		{
			int slicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			switch (stepSize)
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
						buffer[i] = Read(stepSize);
					}
					break;
			}
			
			return slicesToRead;
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stepSize)
		{
			StreamUtils.ReadCyclic(this, buffer, index, length, stepSize);
		}
		
		public void Write(T value, int stepSize = 1)
		{
			FBuffer[FWritePosition] = value;
			FWritePosition += stepSize;
		}
		
		public int Write(T[] buffer, int index, int length, int stepSize = 1)
		{
			int slicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			switch (stepSize)
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
						FWritePosition += stepSize;
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
