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
	public class CARBin<T> : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<T> spread = new VecBinSpread<T>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("First Slice")]
		IOutStream<T> FFirst;
		
		[Output("Remainder")]
		IOutStream<T> FRemainder;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FBin.Sync();
			
			if (FInput.IsChanged || FBin.IsChanged)
			{
				spread.Sync(FInput,1,FBin);				
				
				FFirst.Length = spread.Count;
				if (FFirst.Length == spread.ItemCount || spread.ItemCount == 0)
				{
					FRemainder.Length=0;
					if (spread.ItemCount!=0)
							FFirst.AssignFrom(FInput);
					else
						FFirst.Length = 0;;
				}
				else
				{
					FRemainder.Length = spread.ItemCount-FFirst.Length;
					using (var fWriter = FFirst.GetWriter())
			        using (var rWriter = FRemainder.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							fWriter.Write(spread[b][0]);
							rWriter.Write(spread[b], 1, spread[b].Length-1);
						}
					}
				}
			}
		}
	}
}
