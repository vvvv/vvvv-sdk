#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;
using System.Collections.Generic;
#endregion usings

namespace VVVV.Nodes.Generic
{
    public class Cons<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
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
        protected IIOContainer<IInStream<IInStream<T>>> FInputContainer;
        protected IIOContainer<IOutStream<T>> FOutputContainer;

        [Output("Output Bin Size", Order = 100)]
        protected IOutStream<int> FOutputBinSizeStream;

        [Import]
        protected IIOFactory FFactory;

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<IInStream<IInStream<T>>>(
                new InputAttribute("Input") { IsPinGroup = true });

            FOutputContainer = FFactory.CreateIOContainer<IOutStream<T>>(
                new OutputAttribute("Output"));
        }

        /// <summary>
        /// Called before evaluation of the node starts. Return false in case nothing has to be done.
        /// </summary>
        protected virtual bool Prepare() => FInputContainer.IOObject.IsChanged;

        public void Evaluate(int SpreadMax)
        {
            // Early exit - important for expensive types like strings and streams.
            if (!Prepare())
                return;

            var inputStreams = FInputContainer.IOObject;
            var outputStream = FOutputContainer.IOObject;

            var outputLength = inputStreams.GetLengthSum();
            outputStream.Length = outputLength;
            FOutputBinSizeStream.Length = inputStreams.Length;

            var buffer = MemoryPool<T>.GetArray();
            try
            {
                using (var writer = outputStream.GetWriter())
                using (var binSizeWriter = FOutputBinSizeStream.GetWriter())
                {
                    foreach (var inputStream in inputStreams)
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