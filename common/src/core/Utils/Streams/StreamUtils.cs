
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
	// Base class with various utility functions.
	public static class StreamUtils
	{
		public const int BUFFER_SIZE = 128;
		public const string STREAM_IN_USE_MSG = "Stream is in use.";
		
		public static IStreamReader<T> GetCyclicReader<T>(this IInStream<T> stream)
		{
			return new CyclicStreamReader<T>(stream);
		}
		
		public static int GetNumSlicesAhead(IStreamer streamer, int index, int length, int stride)
		{
			int slicesAhead = streamer.Length - streamer.Position;
			
			if (stride > 1)
			{
				int r = 0;
				slicesAhead = Math.DivRem(slicesAhead, stride, out r);
				if (r > 0)
					slicesAhead++;
			}
			
			return Math.Max(Math.Min(length, slicesAhead), 0);
		}
		
		public static int Read<T>(IStreamReader<T> reader, T[] buffer, int index, int length, int stride = 1)
		{
			int slicesToRead = GetNumSlicesAhead(reader, index, length, stride);
			
			switch (stride)
			{
				case 0:
					if (index == 0 && slicesToRead == buffer.Length)
						buffer.Init(reader.Read(stride)); // Slightly faster
					else
						buffer.Fill(index, slicesToRead, reader.Read(stride));
					break;
				default:
					for (int i = index; i < index + slicesToRead; i++)
					{
						buffer[i] = reader.Read(stride);
					}
					break;
			}
			
			return slicesToRead;
		}
		
		public static void ReadCyclic<T>(this IStreamReader<T> reader, T[] buffer, int index, int length, int stride = 1)
		{
			// Exception handling
			if (reader.Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
			
			// Normalize the stride
			stride %= reader.Length;
			
			switch (reader.Length)
			{
				case 1:
					// Special treatment for streams of length one
					if (reader.Eos) reader.Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(reader.Read(stride)); // Slightly faster
					else
						buffer.Fill(index, length, reader.Read(stride));
					break;
				default:
					int numSlicesRead = 0;
					
					// Read till end
					while ((numSlicesRead < length) && (reader.Position %= reader.Length) > 0)
					{
						numSlicesRead += reader.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					// Save start of possible block
					int startIndex = index + numSlicesRead;
					
					// Read one block
					while (numSlicesRead < length)
					{
						numSlicesRead += reader.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
						// Exit the loop once ReadPosition is back at beginning
						if ((reader.Position %= reader.Length) == 0) break;
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
						if (reader.Eos) reader.Position %= reader.Length;
						numSlicesRead += reader.Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					break;
			}
		}
		
		public static int CombineStreams(this int c1, int c2)
		{
			if (c1 == 0 || c2 == 0)
				return 0;
			else
				return Math.Max(c1, c2);
		}
		
		public static int CombineWith<U, V>(this IInStream<U> stream1, IInStream<V> stream2)
		{
			return CombineStreams(stream1.Length, stream2.Length);
		}
		
		public static void SetLengthBy<T>(this IOutStream<T> outStream, IInStream<IInStream<T>> inputStreams)
		{
			outStream.Length = inputStreams.GetMaxLength() * inputStreams.Length;
		}
		
		public static void SetLengthBy<T>(this IIOStream<IOutStream<T>> outputStreams, IInStream<T> inputStream)
		{
			int outputLength = outputStreams.Length;
			int remainder = 0;
			int lengthPerStream = Math.DivRem(inputStream.Length, outputLength, out remainder);
			if (remainder > 0) lengthPerStream++;
			
			foreach (var outputStream in outputStreams)
			{
				outputStream.Length = lengthPerStream;
			}
		}
		
		public static int GetMaxLength<T>(this IInStream<IInStream<T>> streams)
		{
			switch (streams.Length)
			{
				case 0:
					return 0;
				default:
					using (var reader = streams.GetReader())
					{
						var stream = reader.Read();
						int result = stream.Length;
						while (!reader.Eos)
						{
							stream = reader.Read();
							result = result.CombineStreams(stream.Length);
						}
						return result;
					}
			}
		}
	}
}
