#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Shift", Category = "Spreads", Version = "Vector", Help = "Shift (Spreads) with Bin and Vector Size.", Author = "woei")]
	#endregion PluginInfo
	public class ShiftVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();
        
        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Toggle Phase/Slicecount", Visibility = PinVisibility.Hidden)]
		IDiffSpread<bool> FTogPhase;
		
		[Input("Phase")]
		IDiffSpread<double> FPhase;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		
		[Output("Output Bin Size")]
		IOutStream<int> FOutBin;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged || FTogPhase.IsChanged || FPhase.IsChanged)
			{
				if (FInput.Length>0 &&FVec.Length>0 && FBin.Length>0 && FTogPhase.SliceCount>0 && FPhase.SliceCount>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
					spread.Sync(FInput,vecSize,FBin,FPhase.CombineWith(FTogPhase));				
					
					FOutBin.Length = spread.Count;
					FOutput.Length = spread.ItemCount;
					using (var binWriter = FOutBin.GetWriter())
					using (var dataWriter = FOutput.GetWriter())
					{
						int incr = 0;
						for (int b = 0; b < spread.Count; b++)
						{
							int phase = 0;
							if (FTogPhase[b])
								phase = (int)FPhase[b];
							else
								phase = (int)Math.Round(FPhase[b]*(spread[b].Length/vecSize));
							
							if (spread[b].Length>0)
							{
								for (int v = 0; v < vecSize; v++)
								{
									dataWriter.Position = incr+v;
									double[] src = spread.GetBinColumn(b,v).ToArray();
									phase = VMath.Zmod(phase, src.Length);
			
									if (phase !=0)
									{
										double[] dst = new double[src.Length];
										Array.Copy(src, 0, dst, phase, src.Length-phase);
										Array.Copy(src, src.Length-phase, dst, 0, phase);
										for (int s=0; s<dst.Length;s++)
											dataWriter.Write(dst[s],vecSize);
									}
									else
									{
										for (int s=0; s<src.Length;s++)
											dataWriter.Write(src[s],vecSize);
									}
									
								}
								incr+=spread[b].Length;
							}
							binWriter.Write((spread[b].Length/vecSize),1);
						}
					}
				}
				else
					FOutput.Length = FOutBin.Length = 0;
			}
		}
	}

	
	#region PluginInfo
	[PluginInfo(Name = "Shift", Category = "String", Version = "Bin", Help = "Shift (String) with Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ShiftStringBin : ShiftBin<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Shift", Category = "Color", Version = "Bin", Help = "Shift (Color) with Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ShiftColorBin : ShiftBin<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Shift", Category = "Transform", Version = "Bin", Help = "Shifts the slices in the Spread by the given Phase. With Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ShiftTransformBin : ShiftBin<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Shift", Category = "Enumerations", Version = "Bin", Help = "Shifts the slices in the Spread by the given Phase. With Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ShiftEnumBin : ShiftBin<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Shift", Category = "Raw", Version = "Bin", Help = "Shifts the slices in the Spread by the given Phase. With Bin Size.", Author = "woei")]
	#endregion PluginInfo
	public class ShiftRawBin : ShiftBin<System.IO.Stream> {}
}
