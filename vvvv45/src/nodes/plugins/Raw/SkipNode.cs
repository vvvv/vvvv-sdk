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
    [PluginInfo(
        Name = "Skip", 
        Category = "Raw",
        Help = "Bypasses n bytes in each byte sequence and then returns the remaining bytes",
        Author = "vvvv group")]
    public class SkipNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<Stream> FInput;

        [Input("Count")]
        public ISpread<int> FCountIn;

        [Output("Output")]
        public ISpread<Stream> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            {
                var stream = FInput[i];
                var count = FCountIn[i];
                if (count < 0)
                    FOutput[i] = new TakeStream(stream, Math.Max(stream.Length + count, 0));
                else
                    FOutput[i] = new SkipStream(stream, count);
            }
        }
    }
}
