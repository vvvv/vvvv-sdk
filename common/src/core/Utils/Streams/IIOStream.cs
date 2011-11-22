using System;

namespace VVVV.Utils.Streams
{
	public interface IIOStream : IInStream, IOutStream
	{
		
	}
	
	public interface IIOStream<T> : IIOStream, IInStream<T>, IOutStream<T>
	{
		
	}
}
