
using System;

namespace VVVV.Utils.Streams
{
	public interface IOutStream<T> : IStream
	{
		void Write(T value, int stride = 1);
		
		int Write(T[] buffer, int index, int length, int stride = 1);
		
		void Flush();
		
		int WritePosition
		{
			get;
			set;
		}
		
		new int Length
		{
			get;
			set;
		}
	}
	
	public static class OutStreamExtensions
	{
		public static T[] CreateWriteBuffer<T>(this IOutStream<T> inStream)
		{
			return new T[Math.Max(8, Math.Min(inStream.Length, 512))];
		}
		
		internal static void CyclicWrite<T>(this IOutStream<T> outStream, T[] buffer, int index, int length, int stepSize = 1)
		{
			int numSlicesWritten = outStream.Write(buffer, index, length, stepSize);
			
			for (int i = numSlicesWritten; i < length; i++)
			{
				outStream.WritePosition %= outStream.Length;
				numSlicesWritten += outStream.Write(buffer, index + i, length - numSlicesWritten, stepSize);
			}
		}
	}
}
