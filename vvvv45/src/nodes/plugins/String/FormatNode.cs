using System;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.String
{
    [PluginInfo(
        Name = "Format",
        Category = "String",
        Help = "Replaces the placeholder items in the input string")]
    public class FormatNode : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<string> FInput;

        [Input("Argument", IsPinGroup = true)]
        protected IDiffSpread<ISpread<string>> FArgumentsIn;

        [Output("Output")]
        protected ISpread<string> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (!SpreadUtils.AnyChanged(FInput, FArgumentsIn)) return;

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                var arguments = FArgumentsIn.Select(a => a[i]);
                FOutput[i] = string.Format(FInput[i], arguments.ToArray());
            }
        }
    }
}
