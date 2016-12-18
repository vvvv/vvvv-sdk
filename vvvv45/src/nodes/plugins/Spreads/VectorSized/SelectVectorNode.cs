#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Select", Category = "Value", Version = "Vector", Tags = "repeat, resample, duplicate, spreadop", Help = "Select (Value) with Bin and Vector Size.", Author = "woei")]
	#endregion PluginInfo
	public class SelectVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = 1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Input("Select", DefaultValue = 1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FSelect;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		
		[Output("Former Slice")]
		IOutStream<int> FFormer;
        #pragma warning restore

		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			FSelect.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged || FSelect.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0 && FSelect.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());	
					spread.Sync(FInput,vecSize,FBin,FSelect.Length);

                    using (var selReader = FSelect.GetCyclicReader())
                    using (var outputWriter = FOutput.GetDynamicWriter())
                    using (var formerIdWriter = FFormer.GetDynamicWriter())
                    {
                        int offset = 0;
                        for (int b = 0; b < spread.Count; b++)
                        {
                            var data = spread[b];
                            int binSize = data.Length / vecSize;
                            int sel = selReader.Read();

                            if (sel > 0 && binSize > 0)
                            {
                                int[] ids = new int[binSize];
                                for (int i = 0; i < binSize; i++)
                                    ids[i] = offset + i;
                                for (int s = 0; s < sel; s++)
                                {
                                    outputWriter.Write(data, 0, data.Length);
                                    formerIdWriter.Write(ids, 0, ids.Length);
                                }
                            }
                            offset += binSize;
                        }
                    }
				}
				else
					FOutput.Length = FFormer.Length = 0;
			}
		}
	}
}
