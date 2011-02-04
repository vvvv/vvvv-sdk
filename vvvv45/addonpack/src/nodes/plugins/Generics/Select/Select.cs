#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Collections.Generic;

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
			List<int> formerSlice = new List<int>();
			
			FOutput.SliceCount = sMax;			
			for (int i = 0; i < sMax; i++) 
			{
				List<T> outList = new List<T>();
				for (int s=0; s<FSelect[i]; s++)
				{
					outList.AddRange(FInput[i]);
					formerSlice.Add(i);
				}
				FOutput[i].AssignFrom(outList);
			}
			FFormerSlice.SliceCount=formerSlice.Count;
			FFormerSlice.AssignFrom(formerSlice);
		}
		
	}
	#region String Node
	[PluginInfo(Name = "Select", 
				Category = "String",
				Version = "Advanced",				
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
				Version = "Advanced",				
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
				Help = "select the slices which form the new spread", 
				Tags = "select, repeat",
				Author = "woei")]
	
	public class SelectEnum : Select<EnumEntry>
	{	
	}
	#endregion Enumerations Node
}
