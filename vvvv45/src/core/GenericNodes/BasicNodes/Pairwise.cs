#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.FSharp.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes.Generic
{
    public class Pairwise<T> : IPluginEvaluate
    {
        [Input("Input")]
        protected ISpread<ISpread<T>> Input;

        [Output("Output 1")]
        protected ISpread<ISpread<T>> Output1;

		[Output("Output 2")]
        protected ISpread<ISpread<T>> Output2;
        
        public void Evaluate(int SpreadMax)
        {
            Output1.SliceCount = Input.SliceCount;
            Output2.SliceCount = Input.SliceCount;

            for (var spread = 0; spread<Input.SliceCount; spread++)
        	{
        		var pairs = SeqModule.Pairwise(Input[spread]).ToArray();
	
        		Output1[spread].SliceCount = pairs.Length;
	            Output2[spread].SliceCount = pairs.Length;
	
	            for (var i = 0; i < pairs.Length; i++)
	            {
	                var pair = pairs[i];
	                Output1[spread][i] = pair.Item1;
	                Output2[spread][i] = pair.Item2;
	            }
        	}
        }
    } 
}