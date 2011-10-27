using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Tests.StreamTests
{
	public static class PerfTest
	{
		private static double[] FDoubleArray;
		private static GCHandle FDoubleArrayHandle;
		private static int FDoubleArrayBegin = 4;
		private static int FDoubleArrayEnd = 4;
		
		private static Tuple<IntPtr, int> GetUnmanagedArray()
		{
			if (FDoubleArrayHandle.IsAllocated)
				FDoubleArrayHandle.Free();

			FDoubleArrayHandle = GCHandle.Alloc(FDoubleArray, GCHandleType.Pinned);
			return Tuple.Create(FDoubleArrayHandle.AddrOfPinnedObject() + sizeof(double) * FDoubleArrayBegin, FDoubleArray.Length - (FDoubleArrayBegin + FDoubleArrayEnd));
		}
		
		private static void Validate()
		{
			
		}
		
		private static IntPtr ResizeUnmanagedArray(int newLength)
		{
			if (FDoubleArrayHandle.IsAllocated)
				FDoubleArrayHandle.Free();
			
			Array.Resize(ref FDoubleArray, newLength + FDoubleArrayBegin + FDoubleArrayEnd);
			FDoubleArray.Fill(0, FDoubleArrayBegin, double.MinValue);
			FDoubleArray.Fill(FDoubleArray.Length - FDoubleArrayEnd, FDoubleArrayEnd, double.MaxValue);
			FDoubleArrayHandle = GCHandle.Alloc(FDoubleArray, GCHandleType.Pinned);
			return FDoubleArrayHandle.AddrOfPinnedObject() + sizeof(double) * FDoubleArrayBegin;
		}
		
		public static void Main(string[] args)
		{
			FDoubleArray = new double[64];
			FDoubleArray.Init(1.0);
			FDoubleArray.Fill(0, FDoubleArrayBegin, double.MinValue);
			FDoubleArray.Fill(FDoubleArray.Length - FDoubleArrayEnd, FDoubleArrayEnd, double.MaxValue);
			
			var stream = UnmanagedInStream<double>.Create(GetUnmanagedArray, Validate);
			stream.Sync();
			
			ReadUnbuffered(stream, 1024 * 1024, 1);
			ReadBuffered(stream, 1024 * 1024, 1);
			ReadCyclic(stream, 1024 * 1024, 1);
		}
		
		private static void ReadUnbuffered<T>(IInStream<T> stream, int length, int stepSize)
		{
			for (int i = 0; i < length; i++)
			{
				if (stream.Eof) stream.Reset();
				
				stream.Read(stepSize);
			}
		}
		
		private static void ReadBuffered<T>(IInStream<T> stream, int length, int stepSize)
		{
			T[] buffer = new T[Math.Min(1024, length)];
			
			int readCount = 0;
			while (readCount < length)
			{
				if (stream.Eof) stream.Reset();
				
				readCount += stream.Read(buffer, 0, Math.Min(buffer.Length, length - readCount), stepSize);
			}
		}
		
		private static void ReadCyclic<T>(IInStream<T> stream, int length, int stepSize)
		{
			T[] buffer = new T[length];
			
			stream.ReadCyclic(buffer, 0, buffer.Length, stepSize);
		}
	}
}
