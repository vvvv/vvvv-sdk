using System;
using System.Collections.Generic;

namespace VVVV.Utils.Streams
{
	public interface IStreamReader<T> : IStreamer, IEnumerator<T>
	{
		T Read(int stride = 1);
		
		int Read(T[] buffer, int index, int length, int stride = 1);
	}
	
	public static class StreamReaderExtensions
	{
		public static int Read<T>(this IStreamReader<T> reader, ArraySegment<T> segment, int stride = 1)
		{
			return reader.Read(segment.Array, segment.Offset, segment.Count, stride);
		}
	}
}
