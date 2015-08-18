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
    public abstract class LengthSpectral<T> : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<ISpread<T>> FInput;

        [Input("Closed", DefaultValue = 0.0)]
        public ISpread<bool> FClosed;

        [Output("Output")]
        public ISpread<double> FOutput;

        //[Import()]
        //public ILogger FLogger;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.SliceCount < 1)
            {
                FOutput.SliceCount = 0;
                return;
            }

            FOutput.SliceCount = FInput.SliceCount;

            for (int s = 0; s < FInput.SliceCount; s++)
            {
                FOutput[s] = 0.0;

                if (FInput[s].SliceCount >= 0 && FInput[s].SliceCount < 2)
                    continue;

                else
                {
                    for (int i = 1; i < FInput[s].SliceCount; i++)
                    {
                        FOutput[s] += Distance(FInput[s][i], FInput[s][i - 1]);
                    }
                    if (FClosed[s] == true)
                        FOutput[s] += Distance(FInput[s][0], FInput[s][-1]);
                }
            }
        }

        protected abstract double Distance(T t1, T t2);
    }
}
