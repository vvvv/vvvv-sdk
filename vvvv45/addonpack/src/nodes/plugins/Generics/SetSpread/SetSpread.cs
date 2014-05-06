#region usings
using System;
using System.ComponentModel.Composition;
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
	[PluginInfo(Name = "SetSpread",
	            Category = "Spreads",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadSpreads : SetSpread<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "String",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadString : SetSpread<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Color",
	            Help = "SetSpread with Bin Size",
	            Tags = "split",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadColor : SetSpread<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Transform",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadTransform : SetSpread<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Enumerations",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadEnum : SetSpread<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Raw",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadRaw : SetSpread<System.IO.Stream> {}
}
