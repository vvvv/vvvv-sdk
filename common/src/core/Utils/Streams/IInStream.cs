
using System;

namespace VVVV.Utils.Streams
{
	public interface IInStream : IStream
	{
		/// <summary>
		/// Synchronize the input buffer with the source.
		/// </summary>
		/// <returns>
		/// Returns true if the stream contains new data.
		/// This will return true in most cases, as a computation whether or not
		/// the data changed might be too expensive.
		/// </returns>
		bool Sync();
		
		int ReadPosition
		{
			get;
			set;
		}
	}
	
	public interface IInStream<T> : IInStream
	{
		T Read(int stride = 1);
		
		int Read(T[] buffer, int index, int length, int stride = 1);
		
		void ReadCyclic(T[] buffer, int index, int length, int stride = 1);
	}
	
	public static class InStreamExtensions
	{
		public static T[] CreateReadBuffer<T>(this IInStream<T> inStream)
		{
			return new T[Math.Max(0, Math.Min(inStream.Length, 512))];
		}
	}
}
