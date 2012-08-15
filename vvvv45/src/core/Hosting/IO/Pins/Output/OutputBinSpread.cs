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
            
            public override void Flush()
            {
                FBinSizeStream.Length = Length;
                
                int dataStreamLength = 0;
                using (var binSizeWriter = FBinSizeStream.GetWriter())
                {
                    foreach (var spread in this)
                    {
                        dataStreamLength += spread.SliceCount;
                        binSizeWriter.Write(spread.SliceCount);
                    }
                }
                
                FDataStream.Length = dataStreamLength;
                
                var buffer = MemoryPool<T>.GetArray();
                try
                {
                    using (var dataWriter = FDataStream.GetWriter())
                    {
                        bool anyChanged = false;
                        foreach (var spread in this)
                        {
                            anyChanged |= spread.IsChanged;
                            if (anyChanged)
                            {
                                dataWriter.Write(spread.Stream, buffer);
                                spread.Flush();
                            }
                            else
                                dataWriter.Position += spread.SliceCount;
                        }
                    }
                }
                finally
                {
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
