using System;

namespace VVVV.Utils.Streams
{
	public interface IStreamWriter<T> : IStreamer
	{
		void Reset();
		
		void Write(T value, int stride = 1);
		
		int Write(T[] buffer, int index, int length, int stride = 1);
	}
}
