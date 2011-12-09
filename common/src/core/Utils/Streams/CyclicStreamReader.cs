using System;
using System.Diagnostics;

namespace VVVV.Utils.Streams
{
	public class CyclicStreamReader<T> : IStreamReader<T>
	{
		private readonly IStreamReader<T> FReader;
		
		public CyclicStreamReader(IInStream<T> stream)
		{
			FReader = stream.GetReader();
			Eos = stream.Length == 0;
			Length = stream.Length;
		}
		
		public bool Eos 
		{
			get;
			private set;
		}
		
		public int Position 
		{
			get
			{
				return FReader.Position;
			}
			set
			{
				if (value < 0)
				{
					value = VMath.VMath.Zmod(value, FReader.Length);
				}
				
				if (value >= FReader.Length)
				{
					value %= FReader.Length;
				}
				
				FReader.Position = value;
			}
		}
		
		public int Length 
		{
			get;
			private set;
		}
		
		public T Current 
		{
			get 
			{
				return Read(0);
			}
		}
		
		object System.Collections.IEnumerator.Current 
		{
			get 
			{
				return Current;
			}
		}
		
		public T Read(int stride)
		{
			var result = FReader.Read(stride);
			if (FReader.Eos)
			{
				FReader.Position %= FReader.Length;
			}
			return result;
		}
		
		public int Read(T[] buffer, int index, int length, int stride)
		{
			int readerLength = FReader.Length;
			
			// Exception handling
			if (readerLength == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
			
			// Normalize the stride
			stride %= readerLength;
			
			switch (readerLength)
			{
				case 1:
					// Special treatment for streams of length one
					if (FReader.Eos) FReader.Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(FReader.Read(stride)); // Slightly faster
					else
						buffer.Fill(index, length, FReader.Read(stride));
					break;
				default:
					int numSlicesRead = 0;
					
					// Read till end
					while ((numSlicesRead < length) && (FReader.Position %= readerLength) > 0)
					{
						numSlicesRead += FReader.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					// Save start of possible block
					int startIndex = index + numSlicesRead;
					
					// Read one block
					while (numSlicesRead < length)
					{
						numSlicesRead += FReader.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
						// Exit the loop once Position is back at beginning
						if ((FReader.Position %= readerLength) == 0) break;
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
						if (FReader.Eos) FReader.Position %= readerLength;
						numSlicesRead += FReader.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					break;
			}
			
			return length;
		}
		
		public void Dispose()
		{
			FReader.Dispose();
		}
		
		public bool MoveNext()
		{
			Position++;
			return true;
		}
		
		public void Reset()
		{
			Position = 0;
			FReader.Reset();
		}
	}
}
