﻿using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    class MultiDimOutStream<T> : BufferedIOStream<IInStream<T>>, IDisposable
    {
        private readonly IIOContainer<IOutStream<T>> FDataContainer;
        private readonly IIOContainer<IOutStream<int>> FBinSizeContainer;
        private readonly IOutStream<T> FDataStream;
        private readonly IOutStream<int> FBinSizeStream;
        
        public MultiDimOutStream(IIOFactory ioFactory, OutputAttribute attribute)
        {
            FDataContainer = ioFactory.CreateIOContainer<IOutStream<T>>(attribute, false);
            FBinSizeContainer = ioFactory.CreateIOContainer<IOutStream<int>>(attribute.GetBinSizeOutputAttribute(), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = FBinSizeContainer.IOObject;
            Length = 1;
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }
        
        public override void Flush()
        {
            var buffer = MemoryPool<T>.GetArray();
            var binSizeBuffer = MemoryPool<int>.GetArray();
            try
            {
                FBinSizeStream.Length = Length;

                int dataStreamLength = 0;
                using (var binSizeWriter = FBinSizeStream.GetWriter())
                {
                    var numSlicesBuffered = 0;
                    for (int i = 0; i < Length; i++)
                    {
                        var stream = Buffer[i];
                        binSizeBuffer[numSlicesBuffered++] = stream.Length;
                        dataStreamLength += stream.Length;
                        if (numSlicesBuffered == binSizeBuffer.Length)
                        {
                            binSizeWriter.Write(binSizeBuffer, 0, numSlicesBuffered);
                            numSlicesBuffered = 0;
                        }
                    }
                    if (numSlicesBuffered > 0)
                    {
                        binSizeWriter.Write(binSizeBuffer, 0, numSlicesBuffered);
                    }
                }

                FDataStream.Length = dataStreamLength;
                using (var dataWriter = FDataStream.GetWriter())
                {
                    bool anyChanged = false;
                    var numSlicesBuffered = 0;
                    for (int i = 0; i < Length; i++)
                    {
                        var stream = Buffer[i];
                        anyChanged |= stream.IsChanged;
                        if (anyChanged)
                        {
                            using (var reader = stream.GetReader())
                            {
                                switch (reader.Length)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        buffer[numSlicesBuffered++] = reader.Read();
                                        WriteIfBufferIsFull(dataWriter, buffer, ref numSlicesBuffered);
                                        break;
                                    default:
                                        while (!reader.Eos)
                                        {
                                            numSlicesBuffered += reader.Read(buffer, numSlicesBuffered, buffer.Length - numSlicesBuffered);
                                            WriteIfBufferIsFull(dataWriter, buffer, ref numSlicesBuffered);
                                        }
                                        break;
                                }
                            }
                        }
                        else
                            dataWriter.Position += stream.Length;
                    }
                    if (numSlicesBuffered > 0)
                    {
                        dataWriter.Write(buffer, 0, numSlicesBuffered);
                    }
                }
            }
            finally
            {
                MemoryPool<int>.PutArray(binSizeBuffer);
                MemoryPool<T>.PutArray(buffer);
            }

            FBinSizeStream.Flush();
            FDataStream.Flush();

            base.Flush();
        }

        private static void WriteIfBufferIsFull(IStreamWriter<T> dataWriter, T[] buffer, ref int numSlicesBuffered)
        {
            if (numSlicesBuffered == buffer.Length)
            {
                dataWriter.Write(buffer, 0, numSlicesBuffered);
                numSlicesBuffered = 0;
            }
        }
    }
}
