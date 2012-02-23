using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    class InputBinSpread<T> : BinSpread<T>
    {
        private readonly IInStream<int> FBinSizeStream;
        private readonly IInStream<T> FDataStream;
        private readonly BufferedIOStream<int> FNormBinSizeStream;
        
        public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
            : base(ioFactory, attribute)
        {
            attribute = ManipulateAttribute(attribute);
            
            attribute.AutoValidate = false;
            FDataStream = FIOFactory.CreateIO<IInStream<T>>(attribute, false);
            FBinSizeStream = FIOFactory.CreateIO<IInStream<int>>(attribute.GetBinSizeInputAttribute(), false);
            FNormBinSizeStream = new BufferedIOStream<int>();
        }
        
        protected virtual InputAttribute ManipulateAttribute(InputAttribute attribute)
        {
            // Do nothing by default
            return attribute;
        }
        
        public override bool Sync()
        {
            // Sync source
            var isChanged = base.Sync();
            isChanged |= FBinSizeStream.Sync();
            isChanged |= FDataStream.Sync();
            
            if (isChanged)
            {
                // Normalize bin size and compute sum
                int dataStreamLength = FDataStream.Length;
                int binSizeSum = 0;
                
                var binSizeBuffer = MemoryPool<int>.GetArray();
                var dataBuffer = MemoryPool<T>.GetArray();
                
                try 
                {
                    FNormBinSizeStream.Length = FBinSizeStream.Length;
                    using (var binSizeReader = FBinSizeStream.GetReader())
                    {
                        using (var binSizeWriter = FNormBinSizeStream.GetWriter())
                        {
                            while (!binSizeReader.Eos)
                            {
                                int blockSize = binSizeReader.Read(binSizeBuffer, 0, binSizeBuffer.Length);
                                for (int i = 0; i < blockSize; i++)
                                {
                                    binSizeBuffer[i] = SpreadUtils.NormalizeBinSize(dataStreamLength, binSizeBuffer[i]);
                                    binSizeSum += binSizeBuffer[i];
                                }
                                
                                binSizeWriter.Write(binSizeBuffer, 0, blockSize);
                            }
                        }
                    }
                    
    
                    int binTimes = SpreadUtils.DivByBinSize(dataStreamLength, binSizeSum);
                    binTimes = binTimes > 0 ? binTimes : 1;
                    SliceCount = binTimes * FBinSizeStream.Length;
                    
                    using (var binSizeReader = FNormBinSizeStream.GetCyclicReader())
                    {
                        using (var dataReader = FDataStream.GetCyclicReader())
                        {
                            foreach (var spread in this)
                            {
                                spread.SliceCount = binSizeReader.Read();
                                
                                var stream = spread.Stream;
                                using (var writer = stream.GetWriter())
                                {
                                    while (!writer.Eos)
                                    {
                                        // Since we're using cyclic readers we need to limit the amount
                                        // of data we request.
                                        int numSlicesRead = dataReader.Read(dataBuffer, 0, Math.Min(dataBuffer.Length, writer.Length));
                                        writer.Write(dataBuffer, 0, numSlicesRead);
                                    }
                                }
                            }
                        }
                    }
                } 
                finally
                {
                    MemoryPool<int>.PutArray(binSizeBuffer);
                    MemoryPool<T>.PutArray(dataBuffer);
                }
            }
            
            return isChanged;
        }
    }
}
