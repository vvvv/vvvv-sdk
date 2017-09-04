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
	[PluginInfo(Name = "CAR", Category = "Spreads", Version = "Vector", Help = "CAR (Spreads) with bin and vector size", Author = "woei")]
	#endregion PluginInfo
	public class CARVectorNode : IPluginEvaluate
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
		
		[Output("First Slice")]
		IOutStream<double> FFirst;
		
		[Output("Remainder")]
		IOutStream<double> FRemainder;
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
				
					FFirst.Length = spread.NonEmptyBinCount * vecSize;
					if (FFirst.Length == spread.ItemCount || spread.ItemCount==0)
					{
						FRemainder.Length=0;
						if (spread.ItemCount!=0)
							FFirst.AssignFrom(FInput);
						else
							FFirst.Length = 0;
					}
					else
					{
						FRemainder.Length = spread.ItemCount-FFirst.Length;
						using (var fWriter = FFirst.GetWriter())
				        using (var rWriter = FRemainder.GetWriter())
						{
							for (int b = 0; b < spread.Count; b++)
							{
                                if (spread[b].Length > 0)
                                {
                                    fWriter.Write(spread[b], 0, vecSize);
                                    rWriter.Write(spread[b], vecSize, spread[b].Length - vecSize);
                                }
							}
						}
					}
				}
				else
					FFirst.Length = FRemainder.Length = 0;
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "String", Version = "Bin", Help = "CAR (String) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARStringBin : CARBin<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Color", Version = "Bin", Help = "CAR (Color) with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARColorBin : CARBin<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Transform", Version = "Bin", Help = "Splits the spread into the first element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARTransformBin : CARBin<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Enumerations", Version = "Bin", Help = "Splits the spread into the first element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CAREnumBin : CARBin<EnumEntry>
    {
        string FLastSubType;

        protected override void Prepare()
        {
            var first = FFirstContainer.GetPluginIO() as IPin;
            var remainder = FRemainderContainer.GetPluginIO() as IPin;
            var subType = first.GetDownstreamSubType() ?? remainder.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (first as IEnumOut).SetSubType(subType);
                (remainder as IEnumOut).SetSubType(subType);
                (FInputContainer.GetPluginIO() as IEnumIn).SetSubType(subType);
            }
        }
    }
	
	#region PluginInfo
	[PluginInfo(Name = "CAR", Category = "Raw", Version = "Bin", Help = "Splits the spread into the first element and the rest, with bin size", Author = "woei")]
	#endregion PluginInfo
	public class CARRawBin : CARBin<System.IO.Stream> {}
}
