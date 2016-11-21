using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    class MultiDimOutStream<T> : MemoryIOStream<IInStream<T>>, IIOMultiPin, IDisposable
    {
        private readonly IIOContainer<IOutStream<T>> FDataContainer;
        private readonly IIOContainer<IOutStream<int>> FBinSizeContainer;
        private readonly IOutStream<T> FDataStream;
        private readonly IOutStream<int> FBinSizeStream;

        public IIOContainer BaseContainer => FDataContainer;
        public IIOContainer[] AssociatedContainers => new IIOContainer[]{ FBinSizeContainer };

        public MultiDimOutStream(IIOFactory ioFactory, OutputAttribute attribute)
        {
            FDataContainer = ioFactory.CreateIOContainer<IOutStream<T>>(attribute.DecreaseBinSizeWrapCount(), false);
            FBinSizeContainer = ioFactory.CreateIOContainer<IOutStream<int>>(attribute.GetBinSizeOutputAttribute(FDataContainer), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = FBinSizeContainer.IOObject;
            Length = 1;
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }

        public override void Flush(bool force = false)
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
                        var length = Buffer[i]?.Length ?? 0;
                        binSizeBuffer[numSlicesBuffered++] = length;
                        dataStreamLength += length;
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
                    bool anyChanged = force || IsChanged;
                    var numSlicesBuffered = 0;
                    for (int i = 0; i < Length; i++)
                    {
                        var stream = Buffer[i];
                        if (stream != null)
                        {
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
                                // Reset the changed flags
                                var flushable = stream as IFlushable;
                                if (flushable != null)
                                    flushable.Flush(force);
                            }
                            else
                                dataWriter.Position += stream.Length;
                        }
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

            FBinSizeStream.Flush(force);
            FDataStream.Flush(force);

            base.Flush(force);
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
