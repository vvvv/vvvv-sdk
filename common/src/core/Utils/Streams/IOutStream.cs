
using System;

namespace VVVV.Utils.Streams
{
	public interface IOutStream : IStream
	{
		new int Length
		{
			get;
			set;
		}
		
		void Flush();
	}
	
	public interface IOutStream<T> : IOutStream
	{
		IStreamWriter<T> GetWriter();
	}
	
	public static class OutStreamExtensions
	{
		public static T[] CreateWriteBuffer<T>(this IOutStream<T> inStream)
		{
			return new T[Math.Max(8, Math.Min(inStream.Length, 512))];
		}
		
		internal static void CyclicWrite<T>(this IStreamWriter<T> writer, T[] buffer, int index, int length, int stride = 1)
		{
			int numSlicesWritten = writer.Write(buffer, index, length, stride);
			
			for (int i = numSlicesWritten; i < length; i++)
			{
				writer.Position %= writer.Length;
				numSlicesWritten += writer.Write(buffer, index + i, length - numSlicesWritten, stride);
			}
		}
	}
}
