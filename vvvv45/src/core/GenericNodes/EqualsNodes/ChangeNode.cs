using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Nodes.Generic
{
	public class ChangeNode<T> : IPluginEvaluate
	{
        [Input("Input")]
        public IInStream<T> Input;

        [Input("Bang On Create", Visibility = PinVisibility.Hidden, IsSingle = true)]
        public ISpread<bool> BangOnCreateIn;

        [Output("OnChange")]
        public IOutStream<bool> Output;

        private readonly EqualityComparer<T> FComparer;
        private readonly MemoryIOStream<T> FLastInput = new MemoryIOStream<T>();

        public ChangeNode(EqualityComparer<T> comparer = null)
        {
            FComparer = comparer ?? EqualityComparer<T>.Default;
        }
		
		public void Evaluate(int spreadMax)
		{
            // The final output length is known in advance
            Output.Length = spreadMax;

            // Whether or not new slices should be initialized with true
            var bangOnCreate = BangOnCreateIn.SliceCount > 0 ? BangOnCreateIn[0] : false;

            // Fetch readers and writers
            var inputReader = Input.GetReader();
            var lastInputReader = FLastInput.GetReader();
            var lastInputWriter = FLastInput.GetDynamicWriter();
            var outputWriter = Output.GetWriter();

            // In case of very low spread counts this saves a few ticks
            if (spreadMax < 16)
            {
                var slicesToWrite = spreadMax;
                var inputLength = inputReader.Length;
                var lastInputLength = lastInputReader.Length;
                var minLength = Math.Min(inputLength, lastInputLength);
                for (int i = 0; i < minLength; i++)
                {
                    var input = inputReader.Read();
                    var lastInput = lastInputReader.Read();
                    var changed = !FComparer.Equals(input, lastInput);
                    outputWriter.Write(changed);
                    lastInputWriter.Write(CopySlice(input));
                }
                for (int i = lastInputLength; i < inputLength; i++)
                {
                    var changed = bangOnCreate;
                    outputWriter.Write(changed);
                    var input = inputReader.Read();
                    lastInputWriter.Write(CopySlice(input));
                }
            }
            else
            {
                // Fetch the buffers to work with from the pool
                var inputBuffer = MemoryPool<T>.GetArray();
                var lastInputBuffer = MemoryPool<T>.GetArray();
                var outputBuffer = MemoryPool<bool>.GetArray();
                try
                {
                    var slicesToWrite = spreadMax;
                    while (slicesToWrite > 0)
                    {
                        // Read the input
                        var inputReadCount = inputReader.Read(inputBuffer, 0, inputBuffer.Length);
                        // Read the input from the previous frame
                        var lastInputReadCount = lastInputReader.Read(lastInputBuffer, 0, lastInputBuffer.Length);
                        // Calculate min and max read counts
                        var minCount = Math.Min(inputReadCount, lastInputReadCount);
                        var maxCount = Math.Max(inputReadCount, lastInputReadCount);
                        // Do the equality check for all the slices where values from
                        // the previous frame are available
                        for (int i = 0; i < minCount; i++)
                            outputBuffer[i] = !FComparer.Equals(inputBuffer[i], lastInputBuffer[i]);
                        // Set the output for new slices to the value of bang on create
                        for (int i = minCount; i < maxCount; i++)
                            outputBuffer[i] = bangOnCreate;
                        // Write the result
                        outputWriter.Write(outputBuffer, 0, maxCount);
                        // Store the input values for the next frame
                        CopySlices(inputBuffer, inputReadCount);
                        lastInputWriter.Write(inputBuffer, 0, inputReadCount);
                        // Decrease the number of slices we still need to look at
                        slicesToWrite -= maxCount;
                    }
                }
                finally
                {
                    // Put the buffers back in the pool
                    MemoryPool<T>.PutArray(inputBuffer);
                    MemoryPool<T>.PutArray(lastInputBuffer);
                    MemoryPool<bool>.PutArray(outputBuffer);
                }
            }

            // Dispose the readers and writers
            inputReader.Dispose();
            lastInputReader.Dispose();
            lastInputWriter.Dispose();
            outputWriter.Dispose();
		}

        // Overwrite this method in case T is not a value type
        protected virtual T CopySlice(T slice)
        {
            return slice;
        }

        // Overwrite this method in case T is not a value type
        protected virtual void CopySlices(T[] slices, int count)
        {
        }
	}
}
