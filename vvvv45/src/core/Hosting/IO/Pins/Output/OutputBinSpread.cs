using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
    class OutputBinSpread<T> : BinSpread<T>, IDisposable
    {
        private readonly IIOContainer<IOutStream<T>> FDataContainer;
        private readonly IIOContainer<IOutStream<int>> FBinSizeContainer;
        private readonly IOutStream<T> FDataStream;
        private readonly IOutStream<int> FBinSizeStream;
        
        public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute)
            : base(ioFactory, attribute)
        {
            FDataContainer = FIOFactory.CreateIOContainer<IOutStream<T>>(attribute, false);
            FBinSizeContainer = FIOFactory.CreateIOContainer<IOutStream<int>>(attribute.GetBinSizeOutputAttribute(), false);
            FDataStream = FDataContainer.IOObject;
            FBinSizeStream = FBinSizeContainer.IOObject;
            
            SliceCount = 1;
        }
        
        public void Dispose()
        {
            FDataContainer.Dispose();
            FBinSizeContainer.Dispose();
        }
        
        public override void Flush()
        {
            if (IsChanged)
            {
                FBinSizeStream.Length = SliceCount;
                
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
                        foreach (var spread in this)
                        {
                            if (spread.IsChanged)
                                dataWriter.Write(spread.Stream, buffer);
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
            }
            
            base.Flush();
        }
    }
}
