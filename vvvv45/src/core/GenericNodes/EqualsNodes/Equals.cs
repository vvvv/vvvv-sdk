using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic
{
	
	public class Equals<T> : IPluginEvaluate
	{
#pragma warning disable 0649
        [Input("Input", IsPinGroup = true)]
        ISpread<ISpread<T>> FInput;

        [Output("Output")]
        ISpread<bool> FOutput;
#pragma warning restore
		
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			var pinCount = FInput.SliceCount;
			for (int i = 0; i < SpreadMax; i++) 
			{
				FOutput[i] = EqualsSlice(i, pinCount);
			}
		}
		
		bool EqualsSlice(int slice, int pinCount)
		{
			for (int j = 1; j < pinCount; j++) 
			{
				var a = FInput[j-1][slice];
				var b = FInput[j][slice];
				
				if(a == null)
				{
					if(b != null)
						return false;
				}
				else if(b == null)
				{
					if(a != null)
						return false;
				}
				else if(!a.Equals(b)) 
					return false;
			}
			return true;
		}
	}
}
