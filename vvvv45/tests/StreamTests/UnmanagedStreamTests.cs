
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using VVVV.Hosting.IO.Streams;
using VVVV.Tests.StreamTests;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace StreamTests
{
    [TestFixture]
    public unsafe class UnmanagedStreamTests
    {
        private static bool Validate()
        {
            return true;
        }
        
        [Test]
        public void TestEos()
        {
            using (var unmanagedArray = new UnmanagedArray(16))
            {
                var stream = new DoubleInStream(unmanagedArray.PLength, unmanagedArray.PPData, Validate);
                using (var reader = stream.GetReader())
                {
                    Assert.IsFalse(reader.Eos);
                }
            }
        }
        
        [Test]
        public void TestRead()
        {
            using (var unmanagedArray = new UnmanagedArray(16))
            {
                var stream = new DoubleInStream(unmanagedArray.PLength, unmanagedArray.PPData, Validate);
                
                using (var reader = stream.GetReader())
                {
                    for (int stepSize = 1; stepSize < unmanagedArray.Length; stepSize++)
                    {
                        double s = 0.0;
                        while (!reader.Eos)
                        {
                            double v = reader.Read(stepSize);
                            Assert.AreEqual(s, v);
                            s += stepSize;
                        }
                        unmanagedArray.CheckBoundaries();
                        reader.Reset();
                    }
                }
            }
        }
        
        [Test]
        public void TestBufferedRead()
        {
            using (var unmanagedArray = new UnmanagedArray(16))
            {
                var stream = new DoubleInStream(unmanagedArray.PLength, unmanagedArray.PPData, Validate);
                
                int startIndex = 2;
                int length = 2;
                
                using (var reader = stream.GetReader())
                {
                    for (int stepSize = 1; stepSize < unmanagedArray.Length; stepSize++)
                    {
                        double s = 0.0;
                        
                        while (!reader.Eos)
                        {
                            var buffer = new double[16];
                            int n = reader.Read(buffer, startIndex, length, stepSize);
                            for (int i = 0; i < startIndex; i++)
                                Assert.AreEqual(0.0, buffer[i], "Step size was: {0}", stepSize);
                            for (int i = startIndex; i < startIndex + n; i++)
                            {
                                Assert.AreEqual(s, buffer[i], "Step size was: {0}", stepSize);
                                s += stepSize;
                            }
                            for (int i = startIndex + n; i < buffer.Length; i++)
                                Assert.AreEqual(0.0, buffer[i], "Step size was: {0}", stepSize);
                        }
                        unmanagedArray.CheckBoundaries();
                        reader.Reset();
                    }
                }
            }
        }
        
        [Test]
        public void TestCyclicRead()
        {
            using (var unmanagedArray = new UnmanagedArray(16))
            {
                var stream = new DoubleInStream(unmanagedArray.PLength, unmanagedArray.PPData, Validate);
                
                using (var reader = stream.GetCyclicReader())
                {
                    for (int bufferSize = 2; bufferSize < stream.Length * 5; bufferSize++)
                    {
                        var buffer = new double[bufferSize];
                        
                        int startIndex = 0;
                        int length = buffer.Length;
                        
                        if (bufferSize > 5)
                        {
                            startIndex = 2;
                            length = bufferSize - 2;
                        }
                        
                        for (int stepSize = 8; stepSize < unmanagedArray.Length; stepSize++)
                        {
                            reader.Read(buffer, startIndex, length, stepSize);
                            for (int i = 0; i < startIndex; i++)
                                Assert.AreEqual(0.0, buffer[i], "Step size was: {0}, Buffer size was: {1}", stepSize, bufferSize);
                            
                            var doubleArray = unmanagedArray.ToArray();
                            for (int i = startIndex; i < startIndex + length; i++)
                            {
                                Assert.AreEqual(doubleArray[(stepSize * (i - startIndex)) % stream.Length], buffer[i], "Step size was: {0}, Buffer size was: {1}", stepSize, bufferSize);
                            }
                            
                            for (int i = startIndex + length; i < buffer.Length; i++)
                                Assert.AreEqual(0.0, buffer[i], "Step size was: {0}, Buffer size was: {1}", stepSize, bufferSize);
                            
                            unmanagedArray.CheckBoundaries();
                            reader.Reset();
                        }
                    }
                }
            }
        }
        
        [Test]
        public void TestWrite()
        {
            using (var unmanagedArray = new UnmanagedArray(16))
            {
                var stream = new DoubleOutStream(unmanagedArray.PPData, unmanagedArray.Resize);
                
                var buffer = new double[16];
                using (var writer = stream.GetWriter())
                {
                    for (int stepSize = 1; stepSize < unmanagedArray.Length; stepSize++)
                    {
                        while (!writer.Eos)
                        {
                            writer.Write((double) stepSize, stepSize);
                        }
                        unmanagedArray.CheckBoundaries();
                        writer.Reset();
                    }
                }
            }
        }
        
        [Test]
        public void TestBufferedWrite()
        {
            using (var unmanagedArray = new UnmanagedArray(16))
            {
                var stream = new DoubleOutStream(unmanagedArray.PPData, unmanagedArray.Resize);
                
                var buffer = new double[16];
                buffer[1] = 3.0;
                buffer[2] = 4.0;
                
                using (var writer = stream.GetWriter())
                {
                    for (int stepSize = 1; stepSize < unmanagedArray.Length; stepSize++)
                    {
                        writer.Reset();
                        
                        int n = writer.Write(buffer, 1, 2, stepSize);
                        unmanagedArray.CheckBoundaries();
                        
                        writer.Reset();
                        
                        var doubleArray = unmanagedArray.ToArray();
                        for (int i = 0; i < n; i++)
                        {
                            Assert.AreEqual(buffer[i + 1], doubleArray[i], "Step size was: {0}", stepSize);
                        }
                    }
                }
            }
        }
    }
}
