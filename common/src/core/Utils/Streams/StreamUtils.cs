
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
	// Base class with various utility functions.
	public static class StreamUtils
	{
		public const int BUFFER_SIZE = 1024;
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
		
		public static void SetLengthBy<T>(this IInStream<IOutStream<T>> outputStreams, IInStream<T> inputStream)
		{
			int outputLength = outputStreams.Length;
			int remainder = 0;
			int lengthPerStream = outputLength > 0 ? Math.DivRem(inputStream.Length, outputLength, out remainder) : 0;
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
		
		public static void AssignFrom<T>(this IOutStream<T> outStream, IInStream<T> inStream, T[] buffer)
		{
			outStream.Length = inStream.Length;
			
			using (var reader = inStream.GetReader())
			{
				using (var writer = outStream.GetWriter())
				{
					while (!reader.Eos)
					{
						int numSlicesRead = reader.Read(buffer, 0, buffer.Length);
						writer.Write(buffer, 0, numSlicesRead);
					}
				}
			}
		}
		
		public static void AssignFrom<T>(this IOutStream<T> outStream, IInStream<T> inStream)
		{
			using (var buffer = MemoryPool<T>.GetMemory(StreamUtils.BUFFER_SIZE))
			{
				outStream.AssignFrom(inStream, buffer.Array);
			}
		}
		
		// From: http://en.wikipedia.org/wiki/Power_of_two
		public static bool IsPowerOfTwo(int x)
		{
			return (x & (x - 1)) == 0;
		}
		
		// From: http://en.wikipedia.org/wiki/Power_of_two
		public static int NextHigher(int k) {
			if (k == 0) return 1;
			k--;
			for (int i = 1; i < sizeof(int) * 8; i <<= 1)
				k = k | k >> i;
			return k + 1;
		}
	}
}
