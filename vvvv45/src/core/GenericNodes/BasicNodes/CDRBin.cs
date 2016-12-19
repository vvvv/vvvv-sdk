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
	public class CDRBin<T> : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<T> spread = new VecBinSpread<T>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Remainder")]
		IOutStream<T> FRemainder;
		
		[Output("Last Slice")]
		IOutStream<T> FLast;
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
				
				FLast.Length = spread.Count;
				if (FLast.Length == spread.ItemCount || spread.ItemCount==0)
				{
					FRemainder.Length = 0;
					if (spread.ItemCount!=0)
						FLast.AssignFrom(FInput);
					else
						FLast.Length=0;
				}
				else
				{
					FRemainder.Length = spread.ItemCount-FLast.Length;
					using (var rWriter = FRemainder.GetWriter())
					using (var lWriter = FLast.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							int rLength = spread[b].Length-1;
							rWriter.Write(spread[b], 0, rLength);
							
							lWriter.Write(spread[b][rLength]);
						}
					}
				}
			}
		}
	}
}
