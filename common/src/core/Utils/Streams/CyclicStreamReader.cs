using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// A stream reader which reads a stream in a cyclic fashion.
    /// It will therefor never go into an end of stream state.
    /// Exceptions to this rule is if the stream is empty.
    /// </summary>
    [ComVisible(false)]
	public class CyclicStreamReader<T> : IStreamReader<T>
	{
		private IStreamReader<T> FReader;
		
		internal CyclicStreamReader(IInStream<T> stream)
		{
		    if (stream.Length == 0)
		    {
		        FReader = StreamUtils.GetEmptyStream<T>().GetReader();
		    }
		    else
		    {
		        FReader = stream.GetReader();
		    }
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
			    if (Length > 0) FReader.Position = VMath.VMath.Zmod(value, Length);
			}
		}
		
		public int Length
		{
			get;
			private set;
		}
		
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
		
		public T Read(int stride = 1)
		{
			var result = FReader.Read(stride);
			if (FReader.Eos)
			{
				FReader.Position %= FReader.Length;
			}
			return result;
		}
		
		public int Read(T[] buffer, int index, int length, int stride = 1)
		{
			int readerLength = Length;
			
			switch (readerLength)
			{
				case 1:
					// Special treatment for streams of length one
					if (FReader.Eos) FReader.Reset();

                    var value = Read(stride);
					if (index == 0 && length == buffer.Length)
						buffer.Init(value); // Slightly faster
					else
						buffer.Fill(index, length, value);
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
					
                    // Early exit
					if (numSlicesRead == length) break;
					
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
            FReader = null;
		}
		
		public bool MoveNext()
		{
            if (!Eos)
            {
                Current = Read();
                return true;
            }
            return false;
		}
		
		public void Reset()
		{
			FReader.Reset();
		}
	}
}
