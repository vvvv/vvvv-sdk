
using System;

namespace VVVV.Utils.Streams
{
	public interface IStream : ICloneable
	{
		int Length
		{
			get;
		}
	}
}
