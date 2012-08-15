using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    public class InputBinSpread<T> : BinSpread<T>, IDisposable
    {
        public class InputBinSpreadStream : BinSpreadStream, IDisposable
        {
            private readonly IIOContainer<IInStream<T>> FDataContainer;
            private readonly IIOContainer<IInStream<int>> FBinSizeContainer;
            private readonly IInStream<T> FDataStream;
            private readonly IInStream<int> FBinSizeStream;
            private readonly BufferedIOStream<int> FNormBinSizeStream;
            private readonly IPluginIO FDataIO;
            
            public InputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute)
                : this(ioFactory, attribute, false)
            {
            }

            public InputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute, bool checkIfChanged)
                : this(ioFactory, attribute, checkIfChanged, () => ioFactory.CreateIOContainer<IInStream<int>>(attribute.GetBinSizeInputAttribute(), false))
            {
            }

            public InputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute, bool checkIfChanged, Func<IIOContainer<IInStream<int>>> binSizeIOContainerFactory)
            {
                // Don't do this, as spread max won't get computed for this pin
                //                attribute.AutoValidate = false;
                attribute.CheckIfChanged = checkIfChanged;
                FDataContainer = ioFactory.CreateIOContainer<IInStream<T>>(attribute, false);
                FBinSizeContainer = binSizeIOContainerFactory();
                FDataStream = FDataContainer.IOObject;
                FBinSizeStream = FBinSizeContainer.IOObject;
                FNormBinSizeStream = new BufferedIOStream<int>(FBinSizeStream.Length);
                FDataIO = FDataContainer.GetPluginIO();
            }

            public bool IsConnected
            {
                get
                {
                    return this.FDataIO.IsConnected;
                }
            }
            
            public void Dispose()
            {
                FDataContainer.Dispose();
                FBinSizeContainer.Dispose();
            }
            
            public override bool Sync()
            {
                // Sync source
                IsChanged = FBinSizeStream.Sync() | FDataStream.Sync();
                
                if (IsChanged)
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
                        Length = binTimes * FBinSizeStream.Length;
                        
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
                                        while (!writer.Eos && !dataReader.Eos)
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
                
                return base.Sync();
            }
        }
        
        private readonly InputBinSpreadStream FStream;
        
        public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
            : this(ioFactory, attribute, new InputBinSpreadStream(ioFactory, attribute))
        {
            
        }

        public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute, IIOContainer<IInStream<int>> binSizeIOContainer)
            : this(ioFactory, attribute, new InputBinSpreadStream(ioFactory, attribute, false, () => binSizeIOContainer))
        {

        }
        
        public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute, InputBinSpreadStream stream)
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
