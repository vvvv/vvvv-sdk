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
    [PluginInfo(Name = "=", Category = "Raw")]
    public class EqualsNode : IPluginEvaluate
    {
        [Input("Input", IsPinGroup = true)]
        public IInStream<IInStream<Stream>> FInputGroup;

        [Output("Output")]
        public IOutStream<bool> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.Length = FInputGroup.GetMaxLength();
            var inputStreamReaders = FInputGroup.Select(i => i.GetCyclicReader()).ToArray();
            using (var outputWriter = FOutput.GetWriter())
            {
                while (!outputWriter.Eos)
                {
                    var areEqual = inputStreamReaders
                        .Select(r => r.Read())
                        .Pairwise((s1, s2) => s1.StreamEquals(s2))
                        .All(b => b);
                    outputWriter.Write(areEqual);
                }
            }
        }
    }
}
