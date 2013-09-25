#region usings
using System;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes
{
    public class GetSpread<T> : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        IInStream<IInStream<T>> FInput;
        
        [Input("Offset")]
        IInStream<int> FOffset;
        
        [Input("Count", DefaultValue = 1)]
        IInStream<int> FCount;

        [Output("Output")]
        IOutStream<IInStream<T>> FOutput;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            FOutput.Length = StreamUtils.GetMaxLength(FInput, FOffset, FCount);

            var inputBuffer = MemoryPool<IInStream<T>>.GetArray();
            var offsetBuffer = MemoryPool<int>.GetArray();
            var countBuffer = MemoryPool<int>.GetArray();
            
            try
            {
                using (var inputReader = FInput.GetCyclicReader())
                using (var offsetReader = FOffset.GetCyclicReader())
                using (var countReader = FCount.GetCyclicReader())
                using (var outputWriter = FOutput.GetWriter())
                {
                    var numSlicesToWrite = FOutput.Length;
                    while (numSlicesToWrite > 0)
                    {
                        var blockSize = Math.Min(numSlicesToWrite, inputBuffer.Length);
                        inputReader.Read(inputBuffer, 0, blockSize);
                        offsetReader.Read(offsetBuffer, 0, blockSize);
                        countReader.Read(countBuffer, 0, blockSize);

                        for (int i = 0; i < blockSize; i++)
                        {
                            var source = inputBuffer[i];
                            var sourceLength = source.Length;
                            if (sourceLength > 0)
                            {
                                var offset = offsetBuffer[i];
                                var count = countBuffer[i];

                                if (offset < 0 || offset >= sourceLength)
                                {
                                    offset = VMath.Zmod(offset, sourceLength);
                                }
                                if (count < 0)
                                {
                                    source = source.Reverse();
                                    count = -count;
                                    offset = sourceLength - offset;
                                }
                                // offset and count are positive now
                                if (offset + count > sourceLength)
                                {
                                    source = source.Cyclic();
                                }

                                inputBuffer[i] = source.GetRange(offset, count);
                            }
                        }

                        numSlicesToWrite -= outputWriter.Write(inputBuffer, 0, blockSize);
                    }
                }
            }
            finally
            {
                MemoryPool<IInStream<T>>.PutArray(inputBuffer);
                MemoryPool<int>.PutArray(offsetBuffer);
                MemoryPool<int>.PutArray(countBuffer);
            }
        }
    }
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Spreads",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadSpreads : GetSpread<double> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "String",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadString : GetSpread<string> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Color",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadColor : GetSpread<RGBAColor> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Transform",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadTransform : GetSpread<Matrix4x4> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Enumerations",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadEnum : GetSpread<EnumEntry> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Raw",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadRaw : GetSpread<System.IO.Stream> {}
}
