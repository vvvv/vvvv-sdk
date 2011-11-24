
using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
	// Base class with various utility functions.
	public static class StreamUtils
	{
		public static int GetNumSlicesToRead<T>(IInStream<T> stream, int index, int length, int stride)
		{
			int slicesAhead = stream.Length - stream.ReadPosition;
			
			if (stride > 0)
			{
				int r = 0;
				slicesAhead = Math.DivRem(slicesAhead, stride, out r);
				if (r > 0)
					slicesAhead++;
			}
			
			return Math.Max(Math.Min(length, slicesAhead), 0);
		}
		
		public static int GetNumSlicesToWrite<T>(IOutStream<T> stream, int index, int length, int stride)
		{
			int slicesAhead = stream.Length - stream.WritePosition;
			
			if (stride > 0)
			{
				int r = 0;
				slicesAhead = Math.DivRem(stream.Length - stream.WritePosition, stride, out r);
				if (r > 0)
					slicesAhead++;
			}
			
			return Math.Max(Math.Min(length, slicesAhead), 0);
		}
		
		public static int Read<T>(IInStream<T> inStream, T[] buffer, int index, int length, int stride = 1)
		{
			int slicesToRead = GetNumSlicesToRead(inStream, index, length, stride);
			
			switch (stride)
			{
				case 0:
					if (index == 0 && slicesToRead == buffer.Length)
						buffer.Init(inStream.Read(stride)); // Slightly faster
					else
						buffer.Fill(index, slicesToRead, inStream.Read(stride));
					break;
				default:
					for (int i = index; i < index + slicesToRead; i++)
					{
						buffer[i] = inStream.Read(stride);
					}
					break;
			}
			
			return slicesToRead;
		}
		
		public static void ReadCyclic<T>(IInStream<T> inStream, T[] buffer, int index, int length, int stride = 1)
		{
			// Exception handling
			if (inStream.Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
			
			// Normalize the stride
			stride %= inStream.Length;
			
			switch (inStream.Length)
			{
				case 1:
					// Special treatment for streams of length one
					if (inStream.Eof) inStream.Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(inStream.Read(stride)); // Slightly faster
					else
						buffer.Fill(index, length, inStream.Read(stride));
					break;
				default:
					int numSlicesRead = 0;
					
					// Read till end
					while ((numSlicesRead < length) && (inStream.ReadPosition %= inStream.Length) > 0)
					{
						numSlicesRead += inStream.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					// Save start of possible block
					int startIndex = index + numSlicesRead;
					
					// Read one block
					while (numSlicesRead < length)
					{
						numSlicesRead += inStream.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
						// Exit the loop once ReadPosition is back at beginning
						if ((inStream.ReadPosition %= inStream.Length) == 0) break;
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
						if (inStream.Eof) inStream.ReadPosition %= inStream.Length;
						numSlicesRead += inStream.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					break;
			}
		}
		
		
	}
}
