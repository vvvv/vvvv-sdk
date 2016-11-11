#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
    public enum RoundMode
    {
        Ceiling,
        Floor,
        Truncate,
        ToEven,
        AwayFromZero
    };

    #region PluginInfo
    [PluginInfo(Name = "Round", Category = "Value", Help = "Rounds Values to given digits in various ways.", Tags = "", Author = "sebl", Credits = "bjo:rn")]
    #endregion PluginInfo
    public class ValueRoundNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input", DefaultValue = 1.0)]
        public IDiffSpread<double> FInput;

        [Input("Digits", DefaultValue = 1, MinValue = 0)]
        public IDiffSpread<int> FDigits;

        [Input("Rounding", DefaultEnumEntry="AwayFromZero")]
        public IDiffSpread<RoundMode> FRounding;

        [Output("Output")]
        public ISpread<double> FOutput;
        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged || FDigits.IsChanged || FRounding.IsChanged)
            {
                FOutput.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    double d; // needed to calc digits in Ceil/Floor/Truncate

                    switch (FRounding[i])
                    {
                        case RoundMode.Ceiling:
                            d = Math.Pow(10, FDigits[i]);
                            FOutput[i] = Math.Ceiling(FInput[i] * d) / d;
                            break;

                        case RoundMode.Floor:
                            d = Math.Pow(10, FDigits[i]);
                            FOutput[i] = Math.Floor(FInput[i] * d) / d;
                            break;

                        case RoundMode.Truncate:
                            d = Math.Pow(10, FDigits[i]);
                            FOutput[i] = Math.Truncate(FInput[i] * d) / d;
                            break;

                        case RoundMode.ToEven:
                            FOutput[i] = Math.Round(FInput[i], Math.Max(0, Math.Min(15, FDigits[i])), MidpointRounding.ToEven);
                            break;

                        case RoundMode.AwayFromZero:
                            FOutput[i] = Math.Round(FInput[i], Math.Max(0, Math.Min(15, FDigits[i])), MidpointRounding.AwayFromZero);
                            break;
                    }
                }
            }
        }
    }
}