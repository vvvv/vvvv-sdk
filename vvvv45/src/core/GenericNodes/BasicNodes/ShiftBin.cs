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
	public class ShiftBin<T> : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<T> spread = new VecBinSpread<T>();

        #pragma warning disable 649
        [Input("Spread", CheckIfChanged = true, AutoValidate = false)]
		IInStream<T> FInput;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Toggle Phase/Slicecount", Visibility = PinVisibility.Hidden)]
		IDiffSpread<bool> FTogPhase;
		
		[Input("Phase")]
		IDiffSpread<double> FPhase;
		
		[Output("Output")]
		IOutStream<T> FOutput;
		
		[Output("Output Bin Size")]
		IOutStream<int> FOutBin;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FBin.Sync();
			
			if (FInput.IsChanged || FBin.IsChanged || FTogPhase.IsChanged || FPhase.IsChanged)
			{
				spread.Sync(FInput,1,FBin,FPhase.CombineWith(FTogPhase));				
				
				FOutBin.Length = spread.Count;
				FOutput.Length = spread.ItemCount;
				using (var binWriter = FOutBin.GetWriter())
				using (var dataWriter = FOutput.GetWriter())
				{
					for (int b = 0; b < spread.Count; b++)
					{
						int phase = 0;
						if (FTogPhase[b])
							phase = (int)FPhase[b];
						else
							phase = (int)Math.Round(FPhase[b]*spread[b].Length);
	
						T[] src = spread[b];
						phase = VMath.Zmod(phase, src.Length);
	
						if (phase !=0)
						{
							T[] dst = new T[src.Length];
							Array.Copy(src, 0, dst, phase, src.Length-phase);
							Array.Copy(src, src.Length-phase, dst, 0, phase);
							dataWriter.Write(dst,0,dst.Length);
						}
						else
							dataWriter.Write(src,0,src.Length);
	
						binWriter.Write(spread[b].Length);
					}
				}
			}
		}
	}
}
