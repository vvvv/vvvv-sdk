using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
    class OutputBinSpread<T> : BinSpread<T>
    {
        private readonly IOutStream<T> FDataStream;
        private readonly IOutStream<int> FBinSizeStream;
        
        public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute)
            : base(ioFactory, attribute)
        {
            FDataStream = FIOFactory.CreateIO<IOutStream<T>>(attribute, false);
            FBinSizeStream = FIOFactory.CreateIO<IOutStream<int>>(attribute.GetBinSizeOutputAttribute(), false);
            
            SliceCount = 1;
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
