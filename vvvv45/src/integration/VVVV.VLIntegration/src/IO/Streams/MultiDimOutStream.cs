using System;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.VL.Hosting.IO.Streams
{
    class MultiDimOutStream<T> : MemoryIOStream<global::VL.Lib.Collections.Spread<T>>, IDisposable, IIOMultiPin
    {
        private readonly IIOContainer<IOutStream<T>> FDataContainer;
        private readonly IIOContainer<IOutStream<int>> FBinSizeContainer;
        private readonly IOutStream<T> FDataStream;
        private readonly IOutStream<int> FBinSizeStream;

        public MultiDimOutStream(IIOFactory ioFactory, OutputAttribute attribute)
        {
            FDataContainer = ioFactory.CreateIOContainer<IOutStream<T>>(attribute.DecreaseBinSizeWrapCount(), false);
            FBinSizeContainer = ioFactory.CreateIOContainer<IOutStream<int>>(attribute.GetBinSizeOutputAttribute(FDataContainer), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = FBinSizeContainer.IOObject;
            //Length = 1;
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }

        public IIOContainer BaseContainer => FDataContainer;
        public IIOContainer[] AssociatedContainers => new IIOContainer[] { FBinSizeContainer };

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
                        var array = Buffer[i];
                        if (array != null)
                        {
                            binSizeBuffer[numSlicesBuffered++] = array.Count;
                            dataStreamLength += array.Count;
                        }
                        else
                            binSizeBuffer[numSlicesBuffered++] = 0;

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
                        var array = Buffer[i];
                        if (array != null)
                        {
                            switch (array.Count)
                            {
                                case 0:
                                    break;
                                case 1:
                                    buffer[numSlicesBuffered++] = array[0];
                                    WriteIfBufferIsFull(dataWriter, buffer, ref numSlicesBuffered);
                                    break;
                                default:
                                    var sourceIndex = 0;
                                    while (sourceIndex < array.Count)
                                    {
                                        var chunkLength = Math.Min(array.Count - sourceIndex, buffer.Length - numSlicesBuffered);
                                        array.CopyTo(sourceIndex, buffer, numSlicesBuffered, chunkLength);
                                        sourceIndex += chunkLength;
                                        numSlicesBuffered += chunkLength;
                                        WriteIfBufferIsFull(dataWriter, buffer, ref numSlicesBuffered);
                                    }
                                    break;
                            }
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
