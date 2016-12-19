#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Connect", Category = "Spreads", Help = "Connect n-dimensional with bin and vector siz", Author = "woei")]
	#endregion PluginInfo
	public class ConnectNode : IPluginEvaluate
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
		
		[Input("Close")]
		IDiffSpread<bool> FClose;
		
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
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged || FClose.IsChanged)
			{
				if (FVec.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin,FClose.SliceCount);

                    int outCount = 0;
                    for (int o = 0; o < spread.Count; o++)
                    {
                        int curCount = spread[o].Length * 2;
                        if (!FClose[o])
                            curCount -= 2 * spread.VectorSize;
                        outCount += Math.Max(curCount,0);
                    }


                    FOutBin.Length = spread.Count;
					FOutput.Length = outCount;
                    using (var binWriter = FOutBin.GetWriter())
					using (var dataWriter = FOutput.GetWriter())
					{
						int incr = 0;
						for (int b = 0; b < spread.Count; b++)
						{
							int c = FClose[b] ? 0:1;
							int binSize = Math.Max((spread[b].Length/vecSize-c)*2,0);
							if (spread[b].Length>0)
							{
								for (int v = 0; v < vecSize; v++)
								{
                                    dataWriter.Position = incr + v;
									double[] src = spread.GetBinColumn(b,v).ToArray();

                                    int s = 0;
                                    while (s<src.Length-1)
                                    {
                                        dataWriter.Write(src[s], vecSize);
                                        s++;
                                        dataWriter.Write(src[s], vecSize);
                                    }
                                    if (FClose[b])
                                    {
                                        dataWriter.Write(src[s], vecSize);
                                        dataWriter.Write(src[0], vecSize);
                                    }
								}
								incr+=binSize*vecSize;
							}
							binWriter.Write(binSize,1);
						}
						FOutput.Length = incr;
					}
				}
				else
					FOutBin.Length = FOutput.Length = 0;
			}
		}
	}
}
