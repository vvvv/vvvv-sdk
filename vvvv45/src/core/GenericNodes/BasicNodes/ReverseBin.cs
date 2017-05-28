#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
#endregion usings

namespace VVVV.Nodes.Generic
{
	public class ReverseBin<T> : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<T> spread = new VecBinSpread<T>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Reverse", DefaultBoolean = true)]
		IDiffSpread<bool> FReverse;
		
		[Output("Output")]
		IOutStream<T> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FBin.Sync();
			
			if (FInput.IsChanged || FBin.IsChanged || FReverse.IsChanged)
			{
				spread.Sync(FInput,1,FBin, FReverse.SliceCount);			
	    
				FOutput.Length = spread.ItemCount;
				using (var dataWriter = FOutput.GetWriter())
				{
					for (int b = 0; b < spread.Count; b++)
					{
						T[] bin = spread[b];
						if (FReverse[b])
							Array.Reverse(bin);
						dataWriter.Write(bin, 0, bin.Length);
					}
				}
			}
		}
	}
}
