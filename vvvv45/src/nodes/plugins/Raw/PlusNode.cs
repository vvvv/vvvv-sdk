using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.IO;
using VVVV.Utils.Streams;
using VVVV.Utils.Linq;

namespace VVVV.Nodes.Raw
{
    [PluginInfo(Name = "+", Category = "Raw")]
    public class PlusNode : IPluginEvaluate
    {
        [Input("Input", IsPinGroup = true)]
        public IInStream<IInStream<Stream>> FInputGroup;

        [Input("Intersperse Sequence", Order = int.MaxValue)]
        public IInStream<Stream> FIntersperseSequence;

        [Output("Output")]
        public IOutStream<Stream> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.Length = FInputGroup.GetMaxLength();
            var inputStreamReaders = FInputGroup.Select(i => i.GetCyclicReader()).ToArray();
            using (var intersperseReader = FIntersperseSequence.GetCyclicReader())
            using (var outputWriter = FOutput.GetWriter())
            {
                while (!outputWriter.Eos)
                {
                    var intersperseElement = intersperseReader.Read();
                    var streams = inputStreamReaders
                        .Select(r => r.Read());
                    if (intersperseElement.Length > 0)
                        streams = streams.Intersperse(intersperseElement);
                    outputWriter.Write(new AggregatedStream(streams));
                }
            }
        }
    }
}
