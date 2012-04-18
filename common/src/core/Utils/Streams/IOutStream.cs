using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Base interface for all output streams. It provides the ability
    /// to set the length of an output stream.
    /// </summary>
    [ComVisible(false)]
	public interface IOutStream : IStream, IFlushable
	{
	    /// <summary>
	    /// Gets or sets the length of the output stream.
	    /// </summary>
		new int Length
		{
			get;
			set;
		}
	}
	
	/// <summary>
	/// Defines an output stream. Output streams can be written to by
	/// by retrieving a <see cref="IStreamWriter{T}"/>.
	/// </summary>
	[ComVisible(false)]
	public interface IOutStream<T> : IOutStream
	{
	    /// <summary>
	    /// Gets a <see cref="IStreamWriter{T}"/> which can be used to write
	    /// to this output stream.
	    /// </summary>
	    /// <returns>A <see cref="IStreamWriter{T}"/> to write to this stream.</returns>
		IStreamWriter<T> GetWriter();
	}
}
