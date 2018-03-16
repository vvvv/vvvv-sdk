using System;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Collections.Generic;

namespace VVVV.Nodes.Generic
{
	public abstract class Zip<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		protected IIOContainer<IInStream<IInStream<T>>> FInputContainer;
        protected IDiffSpread<bool> FAllowEmptySpreadsConfig;
		protected IIOContainer<IOutStream<T>> FOutputContainer;

        [Import]
        protected IIOFactory FFactory;

        private bool FAllowEmptySpreads;

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<IInStream<IInStream<T>>>(
                new InputAttribute("Input") { IsPinGroup = true });

            FOutputContainer = FFactory.CreateIOContainer<IOutStream<T>>(
                new OutputAttribute("Output"));

            FAllowEmptySpreadsConfig = FFactory.CreateDiffSpread<bool>(
                new InputAttribute("Allow Empty Spreads") { IsSingle = true, Visibility = PinVisibility.OnlyInspector });

            FAllowEmptySpreadsConfig.Changed += (s) => FAllowEmptySpreads = s[0];
        }

        /// <summary>
        /// Called before evaluation of the node starts. Return false in case nothing has to be done.
        /// </summary>
        protected virtual bool Prepare() => FInputContainer.IOObject.IsChanged || FAllowEmptySpreadsConfig.IsChanged;
		
		public void Evaluate(int SpreadMax)
		{
            if (!Prepare()) return;

            IEnumerable<IInStream<T>> inputStreams;
            int inputStreamsLength;
            var inputStreams_ = FInputContainer.IOObject;
            if (FAllowEmptySpreads)
            {
                inputStreams = inputStreams_.Where(s => s.Length > 0);
                inputStreamsLength = inputStreams.Count();
            }
            else
            {
                inputStreams = inputStreams_;
                inputStreamsLength = inputStreams_.Length;
            }
            int maxInputStreamLength = inputStreams.GetMaxLength();

            var outputStream = FOutputContainer.IOObject;
            outputStream.Length = maxInputStreamLength * inputStreamsLength;

            if (outputStream.Length > 0)
            {
                var buffer = MemoryPool<T>.GetArray();
                try
                {
                    using (var writer = outputStream.GetWriter())
                    {
                        int numSlicesToRead = Math.Min(maxInputStreamLength, buffer.Length);
                        int i = 0;
                        foreach (var inputStream in inputStreams)
                        {
                            writer.Position = i++;
                            using (var reader = inputStream.GetCyclicReader())
                            {
                                while (!writer.Eos)
                                {
                                    reader.Read(buffer, 0, numSlicesToRead);
                                    writer.Write(buffer, 0, numSlicesToRead, inputStreamsLength);
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
