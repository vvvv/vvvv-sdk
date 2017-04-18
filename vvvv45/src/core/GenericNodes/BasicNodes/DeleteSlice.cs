#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Linq;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Generic
{

	public class DeleteSlice<T> : IPluginEvaluate
	{
		#region fields & pins	
		#pragma warning disable 649
		[Input("Input", BinSize =  1, BinName = "Bin Size")]
		IDiffSpread<ISpread<T>> FInput;
		
		[Input("Index")]
		IDiffSpread<int> FIndex;

		[Output("Output")]
		ISpread<ISpread<T>> FOutput;
		#pragma warning restore
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsChanged || FIndex.IsChanged)
			{
	  			FOutput.AssignFrom(FInput);
	  			foreach (int i in FIndex.Select(x => x%FInput.SliceCount).Distinct().OrderByDescending(x => x))
	  				FOutput.RemoveAt(i);
			}
		}
	}
	
}
