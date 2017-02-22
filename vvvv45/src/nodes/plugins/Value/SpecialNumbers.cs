using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Value
{
    [PluginInfo(Name = "NaN", Category = "Value",
                Help = "Outputs NaN = not a number. See also Infinity and AvoidSpecialNumbers.")]
    public class NANNode : IPluginEvaluate
    {
        [Output("Output")]
        protected ISpread<double> FOutput;

        public void Evaluate(int SpreadMax)
        {
            FOutput[0] = double.NaN;
        }
    }

    [PluginInfo(Name = "Infinity", Category = "Value",
                Help = "Outputs Infinity. See also NaN and AvoidSpecialNumbers.")]
    public class MaxFloatNode : IPluginEvaluate
    {
        [Output("Output")]
        protected ISpread<double> FOutput;

        public void Evaluate(int SpreadMax)
        {
            FOutput[0] = double.PositiveInfinity;
        }
    }

    [PluginInfo(Name = "AvoidSpecialNumbers", Category = "Value", Tags = "NaN, Infinity",
                Help = "Outputs the last 'valid' input. Success if current input is no special number.")]
    public class AvoidInfAndNaNNode : IPluginEvaluate
    {
        [Input("Input")]
        protected ISpread<double> FInput;

        [Output("Output")]
        protected ISpread<double> FOutput;

        [Output("Success")]
        protected ISpread<double> FSuccess;

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;
            FSuccess.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var x = FInput[i];
                var success = !double.IsInfinity(x) && !double.IsNaN(x);
                if (success)
                    FOutput[i] = FInput[i];
                FSuccess[i] = success ? 1d : 0d;
            }
        }
    }
}
