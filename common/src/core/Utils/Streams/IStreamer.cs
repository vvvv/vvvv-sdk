using System;

namespace VVVV.Utils.Streams
{
	public interface IStreamer: IDisposable
	{
		bool Eos
		{
			get;
		}
		
		int Position
		{
			get;
			set;
		}
		
		int Length
		{
			get;
		}
	}
}
