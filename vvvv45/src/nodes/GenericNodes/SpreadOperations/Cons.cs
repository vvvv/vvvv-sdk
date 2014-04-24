#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes.Generic
{
    public class Cons<T> : IPluginEvaluate
    {
        //// Much simpler and cleaner
        //[Input("Input", IsPinGroup = true)]
        //protected ISpread<ISpread<T>> Input;

        //[Output("Output")]
        //protected ISpread<ISpread<T>> Output;

        //public void Evaluate(int SpreadMax)
        //{
        //    Output.SliceCount = Input.SliceCount;
        //    for (var i = 0; i < Input.SliceCount; i++)
        //        Output[i] = Input[i];
        //}

        // But this is a few ticks faster ...
        [Input("Input", IsPinGroup = true)]
        protected IInStream<IInStream<T>> FInputStreams;

        [Output("Output")]
        protected IOutStream<T> FOutputStream;

        [Output("Output Bin Size")]
        protected IOutStream<int> FOutputBinSizeStream;

        public void Evaluate(int SpreadMax)
        {
            var outputLength = FInputStreams.GetLengthSum();
            FOutputStream.Length = outputLength;
            FOutputBinSizeStream.Length = FInputStreams.Length;

            var buffer = MemoryPool<T>.GetArray();
            try
            {
                using (var writer = FOutputStream.GetWriter())
                using (var binSizeWriter = FOutputBinSizeStream.GetWriter())
                {
                    foreach (var inputStream in FInputStreams)
                    {
                        using (var reader = inputStream.GetReader())
                        {
                            var numSlicesToRead = reader.Length;
                            binSizeWriter.Write(numSlicesToRead);
                            if (numSlicesToRead == 1)
                            {
                                writer.Write(reader.Read());
                            }
                            else
                            {
                                while (numSlicesToRead > 0)
                                {
                                    var numSlicesRead = reader.Read(buffer, 0, buffer.Length);
                                    writer.Write(buffer, 0, numSlicesRead);
                                    numSlicesToRead -= numSlicesRead;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }
        }
    }
}