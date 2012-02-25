
using System;
using System.Collections.Generic;

namespace VVVV.Utils.Streams
{
	public interface IInStream : IStream, ISynchronizable
	{
	}
	
	public interface IInStream<T> : IInStream, IEnumerable<T>
	{
		IStreamReader<T> GetReader();
	}
	
	public static class InStreamExtensions
	{
		public static T[] CreateReadBuffer<T>(this IInStream<T> inStream)
		{
			return new T[Math.Max(0, Math.Min(inStream.Length, 512))];
		}
	}
}
