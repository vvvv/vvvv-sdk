using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Nodes._SpreadOperations
{
    public class SelectNode<T> : IPluginEvaluate
    {
        [Input("Input")]
        protected IInStream<T> FDataIn;
        [Input("Select")]
        protected IInStream<int> FSelectIn;
        [Output("Output")]
        protected IOutStream<T> FDataOut;
        [Output("Former Slice")]
        protected IOutStream<int> FFormerSliceOut;

        public void Evaluate(int spreadMax)
        {
            // Check if any inputs changed (important for string)
            if (!StreamUtils.AnyChanged(FDataIn, FSelectIn)) return;

            // Compute the output length
            var outputLength = 0;
            var selectLength = FSelectIn.Length;
            if (selectLength > 0)
            {
                var selectSum = FSelectIn.Sum();
                var remainder = 0;
                var numSlicesPerSelect = Math.DivRem(spreadMax, selectLength, out remainder);
                outputLength = selectSum * numSlicesPerSelect;
                if (remainder > 0)
                    outputLength += FSelectIn.Take(remainder).Sum();
            }

            // Set the length of the outputs
            FDataOut.Length = outputLength;
            FFormerSliceOut.Length = outputLength;

            // Early exit
            if (outputLength == 0) return;

            // Fetch readers and writers
            using (var dataReader = FDataIn.GetCyclicReader())
            using (var selectReader = FSelectIn.GetCyclicReader())
            using (var dataWriter = FDataOut.GetWriter())
            using (var formerSliceWriter = FFormerSliceOut.GetWriter())
            // Grab buffers from pool
            using (var dataInBuffer = MemoryPool<T>.GetBuffer())
            using (var dataOutBuffer = MemoryPool<T>.GetBuffer())
            using (var selectBuffer = MemoryPool<int>.GetBuffer())
            using (var sliceBuffer = MemoryPool<int>.GetBuffer())
            {
                var numSlicesToRead = spreadMax;
                var offset = 0;
                var formerSlice = 0;
                while (numSlicesToRead > 0)
                {
                    var blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToRead);
                    dataReader.Read(dataInBuffer.Array, 0, blockSize);
                    selectReader.Read(selectBuffer.Array, 0, blockSize);

                    // This loop iterates through the input data
                    for (int i = 0; i < blockSize; i++)
                    {
                        var data = dataInBuffer.Array[i];
                        var select = selectBuffer.Array[i];

                        // This loop replicates the input data on the output select times
                        for (int j = 0; j < select; j++)
                        {
                            // Buffer result data
                            dataOutBuffer.Array[offset] = data;
                            sliceBuffer.Array[offset] = formerSlice;
                            offset++;

                            // Write data out if buffer is full
                            if (offset == StreamUtils.BUFFER_SIZE)
                            {
                                dataWriter.Write(dataOutBuffer.Array, 0, StreamUtils.BUFFER_SIZE);
                                formerSliceWriter.Write(sliceBuffer.Array, 0, StreamUtils.BUFFER_SIZE);
                                offset = 0;
                            }
                        }

                        formerSlice++;
                    }

                    numSlicesToRead -= blockSize;
                }
                // Write any buffered output data left
                if (offset > 0)
                {
                    dataWriter.Write(dataOutBuffer.Array, 0, offset);
                    formerSliceWriter.Write(sliceBuffer.Array, 0, offset);
                }
            }
        }
    }

    [PluginInfo(Name = "Select", Category = "Value")]
    public class ValueSelectNode : SelectNode<double> { }
}
