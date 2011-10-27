
using System;

namespace VVVV.Utils.Streams
{
	public interface IInStream<T> : IStream
	{
		T Read(int stride = 1);
		
		int Read(T[] buffer, int index, int length, int stride = 1);
		
		void ReadCyclic(T[] buffer, int index, int length, int stride = 1);
		
		void Sync();
		
		int ReadPosition
		{
			get;
			set;
		}
	}
	
	public static class InStreamExtensions
	{
		public static T[] CreateReadBuffer<T>(this IInStream<T> inStream)
		{
			return new T[Math.Max(0, Math.Min(inStream.Length, 512))];
		}
	}
}
