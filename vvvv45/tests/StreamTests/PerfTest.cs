using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.IO.Streams;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Tests.StreamTests
{
    public unsafe static class PerfTest
    {
        private static bool Validate()
        {
            return true;
        }
        
        public static void Main(string[] args)
        {
            var unmanagedArray = new UnmanagedArray(64);
            
            var stream = new DoubleInStream(unmanagedArray.PLength, unmanagedArray.PPData, Validate);
            
            // JIT
            ReadUnbuffered(stream, 1, 1);
            ReadAsEnumerable(stream, 1);
            ReadBuffered(stream, 1, 1);
            ReadCyclic(stream, 1, 1);
            
            ReadUnbuffered(stream, 1024 * 1024, 1);
            ReadAsEnumerable(stream, 1024 * 1024);
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
        
        private static void ReadAsEnumerable<T>(IInStream<T> stream, int length)
        {
            int slicesRead = 0;
            while (slicesRead < length)
            {
                foreach (var value in stream)
                {
                    slicesRead++;
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
