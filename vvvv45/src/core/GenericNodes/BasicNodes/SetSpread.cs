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

	public class SetSpread<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Spread")]
        protected ISpread<ISpread<T>> FSpread;
		
		[Input("Input", BinSize =  1, BinName = "Count", BinOrder = 1)]
        protected ISpread<ISpread<T>> FInput;
		
		[Input("Offset")]
        protected ISpread<int> FOffset;

		[Output("Output")]
        protected ISpread<ISpread<T>> FOutput;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			FOutput.SliceCount = Math.Max(FSpread.SliceCount,Math.Max(FInput.SliceCount,FOffset.SliceCount));
			for (int i=0; i<FInput.SliceCount; i++)
			{
				FOutput[i].AssignFrom(FSpread[i]);
				for (int s=0; s<FInput[i].SliceCount; s++)
					FOutput[i][s+FOffset[i]]=FInput[i][s];
			}
		}
	}
	
}
