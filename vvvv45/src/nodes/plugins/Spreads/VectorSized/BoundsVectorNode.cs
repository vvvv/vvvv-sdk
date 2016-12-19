#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Bounds", Category = "Spectral", Version = "Vector", Help = "Bounds (Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class BoundsVectorNode : IPluginEvaluate
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
		

		[Output("Center")]
		IOutStream<double> FCenter;
		
		[Output("Width")]
		IOutStream<double> FWidth;
		
		[Output("Minimum")]
		IOutStream<double> FMin;
		
		[Output("Maximum")]
		IOutStream<double> FMax;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FVec.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
					spread.Sync(FInput,vecSize,FBin);				
					
					FCenter.Length = FWidth.Length = FMin.Length = FMax.Length = spread.Count*vecSize;
					using (var cWriter = FCenter.GetWriter())
					using (var wWriter = FWidth.GetWriter())
					using (var minWriter = FMin.GetWriter())
					using (var maxWriter = FMax.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							for (int v = 0; v < vecSize; v++)
							{
								if (spread[b].Length>0)
								{
									double min = spread.GetBinColumn(b,v).Min();
									double max = spread.GetBinColumn(b,v).Max();
									double width = max-min;
									cWriter.Write(max - width/2);
									wWriter.Write(width);
									minWriter.Write(min);
									maxWriter.Write(max);
								}
								else
								{
									cWriter.Write(double.NaN);
									wWriter.Write(double.NaN);
									minWriter.Write(double.MinValue);
									maxWriter.Write(double.MaxValue);
								}
							}
						}
					}
				}
				else
					FCenter.Length = FWidth.Length = FMin.Length = FMax.Length = 0;
			}
		}
	}
}
