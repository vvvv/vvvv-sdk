using System;
using System.Collections.Generic;

namespace VVVV.Utils.Streams
{
	public interface IStreamReader<T> : IStreamer, IEnumerator<T>
	{
		T Read(int stride = 1);
		
		int Read(T[] buffer, int index, int length, int stride = 1);
		
//		void ReadCyclic(T[] buffer, int index, int length, int stride = 1);
	}
}
