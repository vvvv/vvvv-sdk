using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    [ComVisible(false)]
	public interface IIOStream : IInStream, IOutStream
	{
		
	}
	
	[ComVisible(false)]
	public interface IIOStream<T> : IIOStream, IInStream<T>, IOutStream<T>
	{
		
	}
}
