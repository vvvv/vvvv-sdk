using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Generic
{

	public abstract class Unzip<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		protected IIOContainer<IInStream<T>> FInputContainer;
		protected IIOContainer<IInStream<IOutStream<T>>> FOutputContainer;

        [Import]
        IIOFactory FFactory;

        public void OnImportsSatisfied()
        {        
            FInputContainer = FFactory.CreateIOContainer<IInStream<T>>(
               new InputAttribute("Input") { BinSize = -2 });

            FOutputContainer = FFactory.CreateIOContainer<IInStream<IOutStream<T>>>(
                new OutputAttribute("Output") { IsPinGroup = true });
        }

        protected virtual void Prepare() { }

        public void Evaluate(int SpreadMax)
		{
            Prepare();

            var inputStream = FInputContainer.IOObject;
            var outputStreams = FOutputContainer.IOObject;

			outputStreams.SetLengthBy(inputStream);

            if (inputStream.IsChanged || outputStreams.IsChanged)
            {
                var buffer = MemoryPool<T>.GetArray();
                try
                {
                    var outputStreamsLength = outputStreams.Length;

                    using (var reader = inputStream.GetCyclicReader())
                    {
                        int i = 0;
                        foreach (var outputStream in outputStreams)
                        {
                            int numSlicesToWrite = Math.Min(outputStream.Length, buffer.Length);

                            reader.Position = i++;
                            using (var writer = outputStream.GetWriter())
                            {
                                while (!writer.Eos)
                                {
                                    reader.Read(buffer, 0, numSlicesToWrite, outputStreamsLength);
                                    writer.Write(buffer, 0, numSlicesToWrite);
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
}
