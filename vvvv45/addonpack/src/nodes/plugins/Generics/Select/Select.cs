#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	public class Select<T>: IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", BinSize = 1)]
		protected ISpread<ISpread<T>> FInput;

		[Input("Select", DefaultValue = 1, MinValue = 0)]
		protected ISpread<int> FSelect;
		
		[Output("Output")]
		protected ISpread<ISpread<T>> FOutput;
		
		[Output("Former Slice")]
		protected ISpread<int> FFormerSlice;
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public virtual void Evaluate(int SpreadMax)
		{
			int sMax = Math.Max(FInput.SliceCount,FSelect.SliceCount);
			
			FOutput.SliceCount = 0;
			FFormerSlice.SliceCount=0;
			
			for (int i = 0; i < sMax; i++) 
			{		
				for (int s=0; s<FSelect[i]; s++)
				{
					if (s==0)
					{
						FOutput.SliceCount++;
						FOutput[FOutput.SliceCount-1].SliceCount=0;
					}
					FOutput[FOutput.SliceCount-1].AddRange(FInput[i]);
					
					FFormerSlice.Add(i);
				}
			}
		}
		
	}
	#region String Node
	[PluginInfo(Name = "Select", 
				Category = "String",
				Version = "Bin",				
				Help = "select the slices which form the new spread", 
				Tags = "select, repeat, binsize",
				Author = "woei")]
	public class SelectString : Select<string>
	{	
	}
	#endregion String Node
	
	#region Color Node
	[PluginInfo(Name = "Select", 
				Category = "Color",
				Version = "Bin",				
				Help = "select the slices which form the new spread", 
				Tags = "select, repeat, binsize",
				Author = "woei")]
	public class SelectColor : Select<RGBAColor>
	{	
	}
	#endregion Color Node
	
	#region Transform Node
	[PluginInfo(Name = "Select", 
				Category = "Transform",
				Version = "Bin",				
				Help = "select the slices which form the new spread", 
				Tags = "select, repeat",
				Author = "woei")]
	public class SelectTransform : Select<Matrix4x4>
	{	
	}
	#endregion Transform Node
	
	#region Enumerations Node
	[PluginInfo(Name = "Select", 
				Category = "Enumerations",
				Version = "Bin",				
				Help = "select the slices which form the new spread", 
				Tags = "select, repeat",
				Author = "woei")]
	
	public class SelectEnum : Select<EnumEntry>
	{	
	}
	#endregion Enumerations Node
	
	#region Raw Node
	[PluginInfo(Name = "Select", 
				Category = "Raw",
				Version = "Bin",				
				Help = "select the slices which form the new spread", 
				Tags = "select, repeat",
				Author = "woei")]
	
	public class SelectRaw : Select<System.IO.Stream>
	{	
	}
	#endregion
}
