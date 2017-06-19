using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace VVVV.Nodes.Generic
{
    public class UnoptimizedSelectForTestsOnly<T> : IPluginEvaluate
    {
        [Input("Input")]
        protected ISpread<T> FInput;
        [Input("Select", CheckIfChanged = true)]
        protected ISpread<int> FSelect;
        [Output("Output")]
        protected ISpread<T> FOutput;
        [Output("Former Slice")]
        protected ISpread<int> FFormerSlice;

        public void Evaluate(int spreadMax)
        {
            int sMax = SpreadUtils.SpreadMax(FInput, FSelect);

            FOutput.SliceCount = 0;
            FFormerSlice.SliceCount = 0;

            for (int i = 0; i < sMax; i++)
            {
                for (int s = 0; s < FSelect[i]; s++)
                {
                    if (s == 0)
                    {
                        FOutput.SliceCount++;
                    }
                    FOutput[FOutput.SliceCount - 1] = FInput[i];
                    FFormerSlice.Add(i);
                }
            }
        }
    }

    public class Select<T> : IPluginEvaluate
    {
        [Input("Input")]
        protected IInStream<T> FDataIn;
        [Input("Select", CheckIfChanged = true)]
        protected IInStream<int> FSelectIn;
        [Output("Output")]
        protected IOutStream<T> FDataOut;
        [Output("Former Slice")]
        protected IOutStream<int> FFormerSliceOut;

        public void Evaluate(int spreadMax)
        {
            // Check if any inputs changed (important for string)
            if (!StreamUtils.AnyChanged(FDataIn, FSelectIn)) return;
            spreadMax = StreamUtils.GetSpreadMax(FDataIn, FSelectIn);

            // Early exit
            if (spreadMax == 0)
            {
                FDataOut.Length = 0;
                FFormerSliceOut.Length = 0;
                return;
            }

            // In case nothing changed also do an early exit - important if T is a string or a reference type
            if (!FDataIn.IsChanged && !FSelectIn.IsChanged)
                return;

            // Fetch readers and writers
            using (var dataReader = FDataIn.GetCyclicReader())
            using (var selectReader = FSelectIn.GetCyclicReader())
            using (var dataWriter = FDataOut.GetDynamicWriter())
            using (var formerSliceWriter = FFormerSliceOut.GetDynamicWriter())
            {
                // Grab buffers from pool
                var dataInBuffer = MemoryPool<T>.GetArray();
                var dataOutBuffer = MemoryPool<T>.GetArray();
                var selectBuffer = MemoryPool<int>.GetArray();
                var sliceBuffer = MemoryPool<int>.GetArray();
                try
                {
                    var numSlicesToRead = spreadMax;
                    var offset = 0;
                    var formerSlice = 0;
                    while (numSlicesToRead > 0)
                    {
                        var blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToRead);
                        dataReader.Read(dataInBuffer, 0, blockSize);
                        selectReader.Read(selectBuffer, 0, blockSize);

                        // This loop iterates through the input data
                        for (int i = 0; i < blockSize; i++)
                        {
                            var data = dataInBuffer[i];
                            var select = selectBuffer[i];

                            // This loop replicates the input data on the output select times
                            for (int j = 0; j < select; j++)
                            {
                                // Buffer result data
                                dataOutBuffer[offset] = data;
                                sliceBuffer[offset] = formerSlice;
                                offset++;

                                // Write data out if buffer is full
                                if (offset == StreamUtils.BUFFER_SIZE)
                                {
                                    dataWriter.Write(dataOutBuffer, 0, StreamUtils.BUFFER_SIZE);
                                    formerSliceWriter.Write(sliceBuffer, 0, StreamUtils.BUFFER_SIZE);
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
                        dataWriter.Write(dataOutBuffer, 0, offset);
                        formerSliceWriter.Write(sliceBuffer, 0, offset);
                    }
                }
                finally
                {
                    MemoryPool<T>.PutArray(dataInBuffer);
                    MemoryPool<T>.PutArray(dataOutBuffer);
                    MemoryPool<int>.PutArray(selectBuffer);
                    MemoryPool<int>.PutArray(sliceBuffer);
                }
            }
        }
    }
}
