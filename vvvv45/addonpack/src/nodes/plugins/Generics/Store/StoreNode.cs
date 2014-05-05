#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Spreads", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreValue: Store<double> 
	{
		[Input("Increment", IsBang = true, Order=1)]
    	private IDiffSpread<bool> FIncrement;
		
		public override bool PinChanged()
		{
			return base.PinChanged() || FIncrement.Any(x => x == true);
		}
		
		public override void Alter(int i, int incr, int binSize, ref Spread<Spread<double>> buffer)
		{
			if (FIncrement[i] && buffer.SliceCount > 0)
			{
				for (int s=0; s<binSize; s++)
					buffer[FId[i]][s]+=FIn[incr+s];
			}
			base.Alter(i, incr, binSize, ref buffer);
		}
		
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "String", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreString: Store<string> 
	{
		[Input("Increment", IsBang = true, Order=1)]
    	private IDiffSpread<bool> FIncrement;
    	
    	public override bool PinChanged()
		{
			return base.PinChanged() || FIncrement.Any(x => x == true);
		}
    	
    	public override void Alter(int i, int incr, int binSize, ref Spread<Spread<string>> buffer)
		{
			if (FIncrement[i] && buffer.SliceCount > 0)
			{
				for (int s=0; s<binSize; s++)
					buffer[FId[i]][s]+=FIn[incr+s];
			}
			base.Alter(i, incr, binSize, ref buffer);
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Color", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreColor: Store<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Transform", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreTransform: Store<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Enumerations", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreEnum: Store<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Raw", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreRaw: Store<System.IO.Stream> {}
}
