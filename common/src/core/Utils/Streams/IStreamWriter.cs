using System;

namespace VVVV.Utils.Streams
{
	public interface IStreamWriter<T> : IStreamer
	{
		void Reset();
		
		void Write(T value, int stride = 1);
		
		int Write(T[] buffer, int index, int length, int stride = 1);
	}
	
	public static class StreamWriterExtensions
	{
		public static int Write<T>(this IStreamWriter<T> writer, ArraySegment<T> segment, int stride = 1)
		{
			return writer.Write(segment.Array, segment.Offset, segment.Count, stride);
		}
	}
}
