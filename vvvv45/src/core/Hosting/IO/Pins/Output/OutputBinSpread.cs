using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
    public class OutputBinSpread<T> : BinSpread<T>, IDisposable
    {
        public class OutputBinSpreadStream : BinSpreadStream, IDisposable
        {
            internal readonly IIOContainer FDataContainer;
            internal readonly IIOContainer<IOutStream<int>> FBinSizeContainer;
            private readonly IOutStream<T> FDataStream;
            private readonly IOutStream<int> FBinSizeStream;
            private bool FOwnsBinSizeContainer;

            public OutputBinSpreadStream(IIOFactory ioFactory, OutputAttribute attribute)
                : this(ioFactory, attribute, c => ioFactory.CreateIOContainer<IOutStream<int>>(attribute.GetBinSizeOutputAttribute(c), false))
            {
                FOwnsBinSizeContainer = true;
            }

            public OutputBinSpreadStream(IIOFactory ioFactory, OutputAttribute attribute, Func<IIOContainer, IIOContainer<IOutStream<int>>> binSizeIOContainerFactory)
            {
                if (attribute.IsBinSizeEnabled)
                {
                    var container = ioFactory.CreateIOContainer<ISpread<T>>(attribute.DecreaseBinSizeWrapCount(), false); // Ask for a spread, otherwise we lose track of bin size wrapping
                    FDataContainer = container;
                    FDataStream = container.IOObject.Stream;
                }
                else
                {
                    var container = ioFactory.CreateIOContainer<IOutStream<T>>(attribute, false); // No need for another indirection, access the node output directly
                    FDataContainer = container;
                    FDataStream = container.IOObject;
                }
                FBinSizeContainer = binSizeIOContainerFactory(FDataContainer);
                FBinSizeStream = FBinSizeContainer.IOObject;
                Length = 1;
            }
            
            public void Dispose()
            {
                FDataContainer.Dispose();
                if (FOwnsBinSizeContainer)
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
                            var length = Buffer[i]?.Stream.Length ?? 0;
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
                        for (int i = 0; i < Length; i++)
                        {
                            var spread = Buffer[i];
                            if (spread != null)
                            {
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

        public override IIOContainer BaseContainer
        {
            get
            {
                return FStream.FDataContainer;
            }
        }

        public override IIOContainer[] AssociatedContainers
        {
            get
            {
                return new IIOContainer[] { FStream.FBinSizeContainer };
            }
        }

        public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute)
            : this(ioFactory, attribute, new OutputBinSpreadStream(ioFactory, attribute))
        {
            
        }

        public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute, IIOContainer<IOutStream<int>> binSizeIOContainer)
            : this(ioFactory, attribute, new OutputBinSpreadStream(ioFactory, attribute, _ => binSizeIOContainer))
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
