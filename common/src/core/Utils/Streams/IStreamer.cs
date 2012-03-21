using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Base interface for all stream readers and writers.
    /// It provides the ability to test whether or not the end
    /// of the stream is reached or to seek in the stream.
    /// </summary>
    [ComVisible(false)]
	public interface IStreamer: IDisposable
	{
	    /// <summary>
	    /// Gets whether or not the end of the stream is reached.
	    /// </summary>
		bool Eos
		{
			get;
		}
		
		/// <summary>
		/// Gets or sets the position of the reader or writer.
		/// </summary>
		int Position
		{
			get;
			set;
		}
		
		/// <summary>
		/// Gets the length of the stream this streamer reads from or writes to.
		/// </summary>
		int Length
		{
			get;
		}
	}
}
