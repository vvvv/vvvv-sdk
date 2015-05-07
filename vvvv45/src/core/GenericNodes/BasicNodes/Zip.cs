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
		[Input("Input", IsPinGroup = true)]
		protected IInStream<IInStream<T>> FInputStreams;

        [Input("Allow Empty Spreads", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FAllowEmptySpreadsConfig;
		
		[Output("Output")]
		protected IOutStream<T> FOutputStream;

        private bool FAllowEmptySpreads;

        public void OnImportsSatisfied()
        {
            FAllowEmptySpreadsConfig.Changed += (s) => FAllowEmptySpreads = s[0];
        }
		
		public void Evaluate(int SpreadMax)
		{
            if (!FInputStreams.IsChanged && !FAllowEmptySpreadsConfig.IsChanged) return;

            IEnumerable<IInStream<T>> inputStreams;
            int inputStreamsLength;
            if (FAllowEmptySpreads)
            {
                inputStreams = FInputStreams.Where(s => s.Length > 0);
                inputStreamsLength = inputStreams.Count();
            }
            else
            {
                inputStreams = FInputStreams;
                inputStreamsLength = FInputStreams.Length;
            }
            int maxInputStreamLength = inputStreams.GetMaxLength();
            FOutputStream.Length = maxInputStreamLength * inputStreamsLength;

            if (FOutputStream.Length > 0)
            {
                var buffer = MemoryPool<T>.GetArray();
                try
                {
                    using (var writer = FOutputStream.GetWriter())
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
