using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic
{
	
	public class Cast<T> : IPluginEvaluate
	{
#pragma warning disable 0649
        [Input("Input")]
        ISpread<object> FInput;

        [Output("Output")]
        ISpread<T> FOutput; 
#pragma warning restore
		
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			for (int i = 0; i < SpreadMax; i++) 
			{
				FOutput[i] = (T)FInput[i];
			}
		}
	}
}
