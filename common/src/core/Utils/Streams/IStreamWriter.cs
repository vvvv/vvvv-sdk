using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Interface which provides the ability to write to an output stream.
    /// </summary>
    [ComVisible(false)]
	public interface IStreamWriter<T> : IStreamer
	{
	    /// <summary>
	    /// Writes an item to the current position in the stream and 
	    /// advances the position within the stream stride many times.
	    /// </summary>
	    /// <param name="value">The item to write to the stream.</param>
	    /// <param name="stride">The stride by which to advance the position.</param>
		void Write(T value, int stride = 1);
		
		/// <summary>
		/// Writes a sequence of items to the current stream and advances 
		/// the current position within this stream by the number of items written.
		/// Use the stride parameter to control advancing of the position after one item has been written.
		/// </summary>
		/// <param name="buffer">The buffer to copy from.</param>
		/// <param name="index">The zero-based offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="length">The number of items to be written to the current stream.</param>
		/// <param name="stride">The stride by which the position is advanced after writing one item.</param>
		/// <returns>
		/// The total number of items written to the stream. 
		/// This can be less than the number of items requested if end of the stream has been reached.
		/// </returns>
		int Write(T[] buffer, int index, int length, int stride = 1);
		
		/// <summary>
		/// Resets the writer by setting its position back to the beginning of the stream.
		/// </summary>
		void Reset();
	}
	
	[ComVisible(false)]
	public static class StreamWriterExtensions
	{
		/// <summary>
		/// Writes a sequence of items to the current stream and advances 
		/// the current position within this stream by the number of items written.
		/// Use the stride parameter to control advancing of the position after one item has been written.
		/// </summary>
	    /// <param name="writer">The writer to use for writing.</param>
	    /// <param name="segment">The array segment to copy from.</param>
	    /// <param name="stride">The stride by which the position is advanced after writing one item.</param>
		/// <returns>
		/// The total number of items written to the stream. 
		/// This can be less than the number of items requested if end of the stream has been reached.
		/// </returns>
		public static int Write<T>(this IStreamWriter<T> writer, ArraySegment<T> segment, int stride = 1)
		{
			return writer.Write(segment.Array, segment.Offset, segment.Count, stride);
		}
	}
}
