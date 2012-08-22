using System;
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
            FBinSizeStream.Length = Length;
            
            int binSizeSum = 0;
            using (var binSizeWriter = FBinSizeStream.GetWriter())
            {
                foreach (var outputStream in this)
                {
                    binSizeWriter.Write(outputStream.Length);
                    binSizeSum += outputStream.Length;
                }
            }
            
            var buffer = MemoryPool<T>.GetArray();
            try
            {
                FDataStream.Length = binSizeSum;
                using (var dataWriter = FDataStream.GetWriter())
                {
                    bool anyChanged = false;
                    var numSlicesBuffered = 0;
                    foreach (var outputStream in this)
                    {
                        anyChanged |= outputStream.IsChanged;
                        if (anyChanged)
                        {
                            using (var reader = outputStream.GetReader())
                            {
                                while (!reader.Eos)
                                {
                                    numSlicesBuffered += reader.Read(buffer, numSlicesBuffered, buffer.Length - numSlicesBuffered);
                                    if (numSlicesBuffered == buffer.Length)
                                    {
                                        dataWriter.Write(buffer, 0, numSlicesBuffered);
                                        numSlicesBuffered = 0;
                                    }
                                }
                            }
                        }
                        else
                            dataWriter.Position += outputStream.Length;
                    }
                    if (numSlicesBuffered > 0)
                    {
                        dataWriter.Write(buffer, 0, numSlicesBuffered);
                    }
                }
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }

            FBinSizeStream.Flush();
            FDataStream.Flush();

            base.Flush();
        }
    }
}
