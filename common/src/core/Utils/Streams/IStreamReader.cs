using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Interface which provides the ability to read from an input stream.
    /// </summary>
    [ComVisible(false)]
	public interface IStreamReader<T> : IStreamer, IEnumerator<T>
	{
	    /// <summary>
	    /// Reads one item from the stream and advances the position within the
	    /// stream stride many times.
	    /// </summary>
	    /// <param name="stride">The stride by which to advance the position.</param>
	    /// <returns>The item at the current position.</returns>
		T Read(int stride = 1);
		
		/// <summary>
		/// Reads a sequence of items from the current stream and advances the position 
		/// within the stream by the number of items read.
        /// Use the stride parameter to control advancing of the position after one item has been read.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The zero-based offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="length">The maximum number of items to be read from the current stream.</param>
		/// <param name="stride">The stride by which the position is advanced after reading one item.</param>
		/// <returns>
		/// The total number of items read into the buffer. 
		/// This can be less than the number of items requested if that many items are not currently available, 
		/// or zero (0) if the end of the stream has been reached.
		/// </returns>
		int Read(T[] buffer, int offset, int length, int stride = 1);
	}
	
	[ComVisible(false)]
	public static class StreamReaderExtensions
	{
		/// <summary>
		/// Reads a sequence of items from the current stream and advances the position 
		/// within the stream by the number of items read.
        /// Use the stride parameter to control advancing of the position after one item has been read.
		/// </summary>
	    /// <param name="reader">The reader to use from reading.</param>
	    /// <param name="segment">The array segment to read into.</param>
	    /// <param name="stride">The stride by which the position is advanced after reading one item.</param>
		/// <returns>
		/// The total number of items read into the buffer. 
		/// This can be less than the number of items requested if that many items are not currently available, 
		/// or zero (0) if the end of the stream has been reached.
		/// </returns>
		public static int Read<T>(this IStreamReader<T> reader, ArraySegment<T> segment, int stride = 1)
		{
			return reader.Read(segment.Array, segment.Offset, segment.Count, stride);
		}
	}
}
