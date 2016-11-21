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
            internal readonly IIOContainer FDataContainer;
            internal readonly IIOContainer<IInStream<int>> FBinSizeContainer;
            private readonly IInStream<T> FDataStream;
            private readonly IInStream<int> FBinSizeStream;
            private readonly IPluginIO FDataIO;
            private bool FOwnsBinSizeContainer;

            public InputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute)
                : this(ioFactory, attribute, false)
            {
            }

            public InputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute, bool checkIfChanged)
                : this(ioFactory, attribute, checkIfChanged, (c) => ioFactory.CreateIOContainer<IInStream<int>>(attribute.GetBinSizeInputAttribute(c), false))
            {
                FOwnsBinSizeContainer = true;
            }

            public InputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute, bool checkIfChanged, Func<IIOContainer, IIOContainer<IInStream<int>>> binSizeIOContainerFactory)
            {
                // Don't do this, as spread max won't get computed for this pin
                // attribute.AutoValidate = false;
                attribute.CheckIfChanged = checkIfChanged;

                if (attribute.IsBinSizeEnabled)
                {
                    var container = ioFactory.CreateIOContainer<ISpread<T>>(attribute.DecreaseBinSizeWrapCount(), false); // Ask for a spread, otherwise we lose track of bin size wrapping
                    FDataContainer = container;
                    FDataStream = container.IOObject.Stream;
                }
                else
                {
                    var container = ioFactory.CreateIOContainer<IInStream<T>>(attribute, false); // No need for another indirection, access the node input directly
                    FDataContainer = container;
                    FDataStream = container.IOObject;
                }

                FBinSizeContainer = binSizeIOContainerFactory(FDataContainer);
                FBinSizeStream = FBinSizeContainer.IOObject;
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
                if (FOwnsBinSizeContainer)
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
                    
                    foreach (var binSize in FBinSizeStream)
                    {
                        binSizeSum += SpreadUtils.NormalizeBinSize(dataStreamLength, binSize);
                    }
                        
                    int binTimes = SpreadUtils.DivByBinSize(dataStreamLength, binSizeSum);
                    binTimes = binTimes > 0 ? binTimes : 1;
                    Length = binTimes * FBinSizeStream.Length;

                    using (var binSizeReader = FBinSizeStream.GetCyclicReader())
                    using (var dataReader = FDataStream.GetCyclicReader())
                    {
                        for (int i = 0; i < Length; i++)
                        {
                            var spread = Buffer[i];
                            var stream = spread.Stream;
                            var binSize = SpreadUtils.NormalizeBinSize(dataStreamLength, binSizeReader.Read());
                            spread.SliceCount = binSize;
                            switch (binSize)
                            {
                                case 0:
                                    break;
                                case 1:
                                    stream.Buffer[0] = dataReader.Read();
                                    break;
                                default:
                                    dataReader.Read(stream.Buffer, 0, binSize);
                                    break;
                            }
                            // Mark the stream as changed
                            stream.IsChanged = true;
                        }
                    }
                }
                
                return base.Sync();
            }
        }
        
        private readonly InputBinSpreadStream FStream;

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
                return new IIOContainer[]{ FStream.FBinSizeContainer };
            }
        }

        public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
            : this(ioFactory, attribute, new InputBinSpreadStream(ioFactory, attribute))
        {
            
        }

        public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute, IIOContainer<IInStream<int>> binSizeIOContainer)
            : this(ioFactory, attribute, new InputBinSpreadStream(ioFactory, attribute, false, _ => binSizeIOContainer))
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
