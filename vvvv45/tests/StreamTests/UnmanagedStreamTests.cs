
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace StreamTests
{
	[TestFixture]
	public class UnmanagedStreamTests
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
		
		private static IntPtr ResizeUnmanagedArray(int newLength)
		{
			if (FDoubleArrayHandle.IsAllocated)
				FDoubleArrayHandle.Free();
			
			Array.Resize(ref FDoubleArray, newLength + FDoubleArrayBegin + FDoubleArrayEnd);
			FDoubleArray.Fill(0, FDoubleArrayBegin, double.MinValue);
			FDoubleArray.Fill(FDoubleArray.Length - FDoubleArrayEnd, FDoubleArrayEnd, double.MinValue);
			FDoubleArrayHandle = GCHandle.Alloc(FDoubleArray, GCHandleType.Pinned);
			return FDoubleArrayHandle.AddrOfPinnedObject() + sizeof(double) * FDoubleArrayBegin;
		}
		
		private static void Validate()
		{
			
		}
		
		private void CheckArrayBoundaries()
		{
			for (int i = 0; i < FDoubleArrayBegin; i++)
				Assert.AreEqual(double.MinValue, FDoubleArray[i]);
			for (int i = FDoubleArray.Length - FDoubleArrayEnd; i < FDoubleArray.Length; i++)
				Assert.AreEqual(double.MinValue, FDoubleArray[i]);
		}
		
		[SetUp]
		public void SetUp()
		{
			FDoubleArray = new double[16];
			FDoubleArray.Init(1.0);
			FDoubleArray.Fill(0, FDoubleArrayBegin, double.MinValue);
			FDoubleArray.Fill(FDoubleArray.Length - FDoubleArrayEnd, FDoubleArrayEnd, double.MinValue);
		}
		
		[Test]
		public void TestRead()
		{
			var stream = new DoubleInStream(GetUnmanagedArray, Validate);
			
			for (int stepSize = 1; stepSize < FDoubleArray.Length; stepSize++)
			{
				while (!stream.Eof)
				{
					double v = stream.Read(stepSize);
					Assert.AreEqual(1.0, v);
				}
				CheckArrayBoundaries();
				stream.Reset();
			}
		}
		
		[Test]
		public void TestBufferedRead()
		{
			var stream = new DoubleInStream(GetUnmanagedArray, Validate);
			
			int startIndex = 2;
			int length = 2;
			
			for (int stepSize = 1; stepSize < FDoubleArray.Length; stepSize++)
			{
				while (!stream.Eof)
				{
					var buffer = stream.CreateReadBuffer();
					int n = stream.Read(buffer, startIndex, length, stepSize);
					for (int i = 0; i < startIndex; i++)
						Assert.AreEqual(0.0, buffer[i], "Step size was: {0}", stepSize);
					for (int i = startIndex; i < startIndex + n; i++)
						Assert.AreEqual(1.0, buffer[i], "Step size was: {0}", stepSize);
					for (int i = startIndex + n; i < buffer.Length; i++)
						Assert.AreEqual(0.0, buffer[i], "Step size was: {0}", stepSize);
				}
				CheckArrayBoundaries();
				stream.Reset();
			}
		}
		
		[Test]
		public void TestWrite()
		{
			var stream = new DoubleOutStream(ResizeUnmanagedArray);
			
			var buffer = stream.CreateWriteBuffer();
			for (int stepSize = 1; stepSize < FDoubleArray.Length; stepSize++)
			{
				while (!stream.Eof)
				{
					stream.Write((double) stepSize, stepSize);
				}
				CheckArrayBoundaries();
				stream.Reset();
			}
		}
		
		[Test]
		public void TestBufferedWrite()
		{
			var stream = new DoubleOutStream(ResizeUnmanagedArray);
			
			var buffer = stream.CreateWriteBuffer();
			buffer[1] = 3.0;
			buffer[2] = 4.0;
			
			for (int stepSize = 1; stepSize < FDoubleArray.Length; stepSize++)
			{
				stream.Reset();
				
				int n = stream.Write(buffer, 1, 2, stepSize);
				
				stream.Reset();
				for (int i = 0; i < n; i++)
				{
					Assert.AreEqual(buffer[i + 1], FDoubleArray[FDoubleArrayBegin + i], "Step size was: {0}", stepSize);
				}
			}
		}
	}
}
