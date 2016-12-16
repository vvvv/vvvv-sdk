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
    [PluginInfo(Name = "+", Category = "Raw", Version = "Spectral")]
    public class PlusSpectralNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<ISpread<Stream>> FInputs;

        [Input("Intersperse Sequence", Order = int.MaxValue, IsSingle = true)]
        public ISpread<Stream> FIntersperseSequence;

        [Output("Output")]
        public ISpread<Stream> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FInputs.SliceCount;
            var intersperseElement = FIntersperseSequence[0];
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                IEnumerable<Stream> streams = FInputs[i];
                if (intersperseElement.Length > 0)
                    streams = streams.Intersperse(intersperseElement);
                FOutput[i] = new AggregatedStream(streams);
            }
        }
    }
}
