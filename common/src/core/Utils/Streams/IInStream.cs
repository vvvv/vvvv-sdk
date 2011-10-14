
using System;

namespace VVVV.Utils.Streams
{
	public interface IInStream<T> : IStream
	{
		T Read(int stepSize = 1);
		
		int Read(T[] buffer, int index, int length, int stepSize = 1);
		
		void ReadCyclic(T[] buffer, int index, int length, int stepSize = 1);
		
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
		
		public static void CyclicRead<T>(this IInStream<T> inStream, T[] buffer, int index, int length, int stepSize = 1)
		{
			switch (inStream.Length)
			{
				case 0:
					throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
				case 1:
					// Special treatment for streams of length one:
					// The loop below in the default case would need to do a modulo every step.
					// The running time would be driven by the if and modulo statements instead
					// of the IInStream.Read() method.
					if (inStream.Eof) inStream.Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(inStream.Read(stepSize)); // Slightly faster
					else
						buffer.Fill(index, length, inStream.Read(stepSize));
					break;
				default:
					int numSlicesRead = inStream.Read(buffer, index, length, stepSize);
					
					while (numSlicesRead < length)
					{
						if (inStream.Eof)
						{
							inStream.ReadPosition %= inStream.Length;
						}
						
						numSlicesRead += inStream.Read(buffer, index + numSlicesRead, length - numSlicesRead, stepSize);
					}
					break;
			}
		}
	}
}
