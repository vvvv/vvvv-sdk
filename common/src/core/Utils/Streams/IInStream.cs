using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Base interface for all input streams.
    /// </summary>
    [ComVisible(false)]
	public interface IInStream : IStream, ISynchronizable
	{
	}
	
	/// <summary>
	/// Defines an input stream. Input streams can be read from by
	/// either retrieving a <see cref="IStreamReader{T}"/> or
	/// or by iterating it with <see cref="IEnumerable{T}"/>.
	/// </summary>
	[ComVisible(false)]
	public interface IInStream<T> : IInStream, IEnumerable<T>
	{
	    /// <summary>
	    /// Gets a <see cref="IStreamReader{T}"/> used to read from this input stream.
	    /// </summary>
	    /// <returns>A <see cref="IStreamReader{T}"/> to read from this stream.</returns>
		IStreamReader<T> GetReader();
	}
}
