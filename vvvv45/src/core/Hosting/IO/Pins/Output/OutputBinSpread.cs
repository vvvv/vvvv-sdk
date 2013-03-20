using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
    public class OutputBinSpread<T> : BinSpread<T>, IDisposable
    {
        public class OutputBinSpreadStream : BinSpreadStream, IDisposable
        {
            private readonly IIOContainer<IOutStream<T>> FDataContainer;
            private readonly IIOContainer<IOutStream<int>> FBinSizeContainer;
            private readonly IOutStream<T> FDataStream;
            private readonly IOutStream<int> FBinSizeStream;
            
            public OutputBinSpreadStream(IIOFactory ioFactory, OutputAttribute attribute)
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
                            var stream = Buffer[i].Stream;
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
                        bool anyChanged = force || IsChanged;
                        for (int i = 0; i < Length; i++)
                        {
                            var spread = Buffer[i];
                            anyChanged |= spread.IsChanged;
                            if (anyChanged)
                            {
                                var stream = spread.Stream;
                                switch (stream.Length)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        dataWriter.Write(stream.Buffer[0]);
                                        break;
                                    default:
                                        dataWriter.Write(stream.Buffer, 0, stream.Length);
                                        break;
                                }
                                // Reset the changed flags
                                stream.Flush(force);
                            }
                            else
                                dataWriter.Position += spread.SliceCount;
                        }
                    }
                }
                finally
                {
                    MemoryPool<int>.PutArray(binSizeBuffer);
                    MemoryPool<T>.PutArray(buffer);
                }
                
                FDataStream.Flush();
                FBinSizeStream.Flush();
                
                base.Flush();
            }
        }
        
        private readonly OutputBinSpreadStream FStream;
        
        public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute)
            : this(ioFactory, attribute, new OutputBinSpreadStream(ioFactory, attribute))
        {
            
        }
        
        public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute, OutputBinSpreadStream stream)
            : base(ioFactory, attribute, stream)
        {
            FStream = stream;
        }
        
        public void Dispose()
        {
            FStream.Dispose();
        }
    }
}
