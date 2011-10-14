
using System;

namespace VVVV.Utils.Streams
{
	public interface IStream : ICloneable
	{
		void Reset();
		
		int Length
		{
			get;
		}
		
		bool Eof
		{
			get;
		}
	}
}
