using System;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Interface which provides the ability to retrieve the length
    /// of a stream. It acts as the base interface for all stream
    /// implementations.
    /// </summary>
	public interface IStream : ICloneable
	{
	    /// <summary>
	    /// Gets the length of the stream.
	    /// </summary>
		int Length
		{
			get;
		}
	}
}
