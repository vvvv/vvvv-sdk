
using System;

namespace VVVV.Utils.Streams
{
	// Base class with various utility functions.
	public static class StreamUtils
	{
		public static int GetNumSlicesToRead<T>(IInStream<T> stream, int index, int length, int stepSize)
		{
			int slicesAhead = stream.Length - stream.ReadPosition;
			
			if (stepSize > 0)
			{
				int r = 0;
				slicesAhead = Math.DivRem(slicesAhead, stepSize, out r);
				if (r > 0)
					slicesAhead++;
			}
			
			return Math.Max(Math.Min(length, slicesAhead), 0);
		}
		
		public static int GetNumSlicesToWrite<T>(IOutStream<T> stream, int index, int length, int stepSize)
		{
			int slicesAhead = stream.Length - stream.WritePosition;
			
			if (stepSize > 0)
			{
				int r = 0;
				slicesAhead = Math.DivRem(stream.Length - stream.WritePosition, stepSize, out r);
				if (r > 0)
					slicesAhead++;
			}
			
			return Math.Max(Math.Min(length, slicesAhead), 0);
		}
		
		const int SMALL_SLICE_COUNT = 16;
		public static void ReadCyclic<T>(IInStream<T> inStream, T[] buffer, int index, int length, int stepSize = 1)
		{
			// Exception handling
			if (inStream.Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
			
			if (length > inStream.Length && inStream.Length < SMALL_SLICE_COUNT)
			{
				switch (inStream.Length)
				{
					case 1:
						// Special treatment for streams of length one:
						// The loop below in the default case would need to do a modulo every step.
						// The running time would be driven by the if and modulo statements instead
						// of the IInStream.Read() method.
						if (inStream.Eof) inStream.Reset();
						
						if (index == 0 && length == buffer.Length)
							buffer.Init(inStream.Read(stepSize)); // Slightly faster
						else
							buffer.Fill(index, length, inStream.Read(stepSize));
						break;
					default:
						// Example:
						//   length == 32, stepSize == 1
						// 0 1 2 3 4 5 6 7 9 10 11
						//           ^
						//           |
						//      ReadPosition
						//
						// 1) Read until EOF
						// 2.1) If numSlicesToRead > streamLength
						//      1) Read whole stream into tmpBuf
						//      2) Fill buffer with tmbBuf (slicesToRead / streamLength) many times
						// 2.2) Else
						// 		1) Read numSlicesToRead
						// 3) Fill buffer with the rest
						
						// Read as much as possible
						int numSlicesRead = inStream.Read(buffer, index, length, stepSize);
						int numSlicesToRead = length - numSlicesRead;
						
						// Reset the stream and increase the index by number of slices read
						inStream.Reset();
						index += numSlicesRead;
						
						// Read the whole stream into a temporary buffer
						int numSlicesToReadInTmp = GetNumSlicesToRead(inStream, 0, SMALL_SLICE_COUNT, stepSize);
						var tmpBuf = new T[numSlicesToReadInTmp];
						int tmpLength = inStream.Read(tmpBuf, 0, tmpBuf.Length, stepSize);
						int remainder = 0;
						int times = Math.DivRem(numSlicesToRead, inStream.Length, out remainder);
						for (int i = 0; i < times; i++)
						{
							for (int j = 0; j < tmpBuf.Length; j++)
							{
								buffer[index++] = tmpBuf[j];
							}
						}
						
						inStream.Reset();
						
						inStream.Read(buffer, index, remainder, stepSize);
						break;
				}
			}
			else
			{
				int numSlicesRead = inStream.Read(buffer, index, length, stepSize);
				
				while (numSlicesRead < length)
				{
					if (inStream.Eof)
					{
						inStream.ReadPosition %= inStream.Length;
					}
					
					numSlicesRead += inStream.Read(buffer, index + numSlicesRead, length - numSlicesRead, stepSize);
				}
			}
		}
	}
}
