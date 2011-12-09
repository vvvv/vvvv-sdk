using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.IO.Streams;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Tests.StreamTests
{
	public unsafe static class PerfTest
	{
		private static double[] FDoubleArray;
		private static GCHandle FDoubleArrayHandle;
		private static int FDoubleArrayBegin = 4;
		private static int FDoubleArrayEnd = 4;
		private static double** FPPData;
		private static int* FPLength;
		private static int FLength;
		private static GCHandle FPPDataHandle;
		
		private static Tuple<IntPtr, int> GetUnmanagedArray()
		{
			if (FDoubleArrayHandle.IsAllocated)
				FDoubleArrayHandle.Free();

			FDoubleArrayHandle = GCHandle.Alloc(FDoubleArray, GCHandleType.Pinned);
			return Tuple.Create(FDoubleArrayHandle.AddrOfPinnedObject() + sizeof(double) * FDoubleArrayBegin, FDoubleArray.Length - (FDoubleArrayBegin + FDoubleArrayEnd));
		}
		
		private static bool Validate()
		{
			return true;
		}
		
		private static void SetDataLength(int newLength)
		{
			if (FDoubleArrayHandle.IsAllocated)
				FDoubleArrayHandle.Free();
			
			FLength = newLength;
			Array.Resize(ref FDoubleArray, newLength + FDoubleArrayBegin + FDoubleArrayEnd);
			FDoubleArray.Fill(0, FDoubleArrayBegin, double.MinValue);
			FDoubleArray.Fill(FDoubleArray.Length - FDoubleArrayEnd, FDoubleArrayEnd, double.MaxValue);
			FDoubleArrayHandle = GCHandle.Alloc(FDoubleArray, GCHandleType.Pinned);
			IntPtr dataPtr = FDoubleArrayHandle.AddrOfPinnedObject() + sizeof(double) * FDoubleArrayBegin;
			FPPDataHandle = GCHandle.Alloc(dataPtr, GCHandleType.Pinned);
			FPPData = (double**) FPPDataHandle.AddrOfPinnedObject().ToPointer();
		}
		
		public static void Main(string[] args)
		{
		    FLength = 64;
		    var lengthHandle = GCHandle.Alloc(FLength, GCHandleType.Pinned);
		    FPLength = (int*) lengthHandle.AddrOfPinnedObject().ToPointer();
		    
			FDoubleArray = new double[FLength];
			FDoubleArray.Init(1.0);
			FDoubleArray.Fill(0, FDoubleArrayBegin, double.MinValue);
			FDoubleArray.Fill(FDoubleArray.Length - FDoubleArrayEnd, FDoubleArrayEnd, double.MaxValue);
			
			var stream = new DoubleInStream(FPLength, FPPData, Validate);
			stream.Sync();
			
			ReadUnbuffered(stream, 1024 * 1024, 1);
			ReadBuffered(stream, 1024 * 1024, 1);
			ReadCyclic(stream, 1024 * 1024, 1);
		}
		
		private static void ReadUnbuffered<T>(IInStream<T> stream, int length, int stepSize)
		{
			using (var reader = stream.GetReader())
			{
				for (int i = 0; i < length; i++)
				{
					if (reader.Eos) reader.Reset();
					
					reader.Read(stepSize);
				}
			}
		}
		
		private static void ReadBuffered<T>(IInStream<T> stream, int length, int stepSize)
		{
			T[] buffer = new T[Math.Min(1024, length)];
			
			using (var reader = stream.GetReader())
			{
				int readCount = 0;
				while (readCount < length)
				{
					if (reader.Eos) reader.Reset();
					
					readCount += reader.Read(buffer, 0, Math.Min(buffer.Length, length - readCount), stepSize);
				}
			}
		}
		
		private static void ReadCyclic<T>(IInStream<T> stream, int length, int stepSize)
		{
			T[] buffer = new T[length];
			
			using (var reader = stream.GetCyclicReader())
			{
				reader.Read(buffer, 0, buffer.Length, stepSize);
			}
		}
	}
}
