#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Spreads", Version = "Vector", Help = "CDR (Spreads) with bin and vector size", Author = "woei")]
	#endregion PluginInfo
	public class CDRVectorNode : IPluginEvaluate
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
		
		[Output("Remainder")]
		IOutStream<double> FRemainder;
		
		[Output("Last Slice")]
		IOutStream<double> FLast;
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
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
					spread.Sync(FInput,vecSize,FBin);
					
					FLast.Length = spread.NonEmptyBinCount * vecSize;
					if (FLast.Length == spread.ItemCount || spread.ItemCount == 0)
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
                                if (spread[b].Length > 0)
                                {
                                    int rLength = spread[b].Length - vecSize;
                                    rWriter.Write(spread[b], 0, rLength);

                                    lWriter.Write(spread[b], rLength, vecSize);
                                }
							}
						}
					}
				}
				else
					FRemainder.Length = FLast.Length = 0;
			}
		}
	}

	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "String", Version = "Bin", Help = "CDR (String) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRStringBin : CDRBin<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Color", Version = "Bin", Help = "CDR (Color) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRColorBin : CDRBin<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Transform", Version = "Bin", Help = "Splits the spread into the last element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRTransformBin : CDRBin<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Enumerations", Version = "Bin", Help = "Splits the spread into the last element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDREnumBin : CDRBin<EnumEntry>
    {
        string FLastSubType;

        protected override void Prepare()
        {
            var remainder = FRemainderContainer.GetPluginIO() as IPin;
            var last = FLastContainer.GetPluginIO() as IPin;
            var subType = remainder.GetDownstreamSubType() ?? last.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (last as IEnumOut).SetSubType(subType);
                (remainder as IEnumOut).SetSubType(subType);
                (FInputContainer.GetPluginIO() as IEnumIn).SetSubType(subType);
            }
        }
    }
	
	#region PluginInfo
	[PluginInfo(Name = "CDR", Category = "Raw", Version = "Bin", Help = "Splits the spread into the last element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CDRRawBin : CDRBin<System.IO.Stream> {}
	
}
