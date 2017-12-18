#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes.Generic
{
	public class GetSpreadAdvanced<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
	    #region fields & pins
	    protected IIOContainer<IInStream<IInStream<T>>> FInputContainer;
	    
	    [Input("Offset", Order = 10)]
        protected IInStream<int> FOffset;
	    
	    [Input("Count", DefaultValue = 1, Order = 20)]
        protected IInStream<int> FCount;
	
        protected IIOContainer<IOutStream<IInStream<T>>> FOutputContainer;

        [Import]
        IIOFactory FFactory;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<IInStream<IInStream<T>>>(
                new InputAttribute("Input"));

            FOutputContainer = FFactory.CreateIOContainer<IOutStream<IInStream<T>>>(
                new OutputAttribute("Output"));
        }

        protected virtual void Prepare() { }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
	    {
            Prepare();

            var input = FInputContainer.IOObject;
            var output = FOutputContainer.IOObject; 

	        output.Length = StreamUtils.GetMaxLength(input, FOffset, FCount);
	
	        var inputBuffer = MemoryPool<IInStream<T>>.GetArray();
	        var offsetBuffer = MemoryPool<int>.GetArray();
	        var countBuffer = MemoryPool<int>.GetArray();
	        
	        try
	        {
	            using (var inputReader = input.GetCyclicReader())
	            using (var offsetReader = FOffset.GetCyclicReader())
	            using (var countReader = FCount.GetCyclicReader())
	            using (var outputWriter = output.GetWriter())
	            {
	                var numSlicesToWrite = output.Length;
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
}
