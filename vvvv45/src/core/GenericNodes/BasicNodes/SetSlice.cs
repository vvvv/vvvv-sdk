#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Generic
{

    public class SetSlice<T> : IPluginEvaluate
    {
        #region fields & pins
#pragma warning disable 0649
        [Input("Spread", BinName = "Bin Size", BinSize = 1, BinOrder = 1)]
        ISpread<ISpread<T>> FSpread;

        [Input("Input")]
        ISpread<T> FInput;

        [Input("Index", Order = 2)]
        ISpread<int> FIndex;

        [Output("Output")]
        ISpread<ISpread<T>> FOutput;
#pragma warning restore
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            var count = FOutput.SliceCount = FSpread.SliceCount;

            int incr = 0;
            for (int i = 0; i < count; i++)
            {
                var os = FOutput[i];
                var ind = VMath.Zmod(FIndex[i], count);
                if (i != ind)
                    os.AssignFrom(FSpread[i]);
                else
                {
                    var osCount = os.SliceCount;
                    for (int s = 0; s < osCount; s++)
                        os[s] = FInput[incr + s];
                    incr += osCount;
                }
            }
        }
    }

}
