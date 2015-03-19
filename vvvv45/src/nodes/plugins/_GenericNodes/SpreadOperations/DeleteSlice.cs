#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Linq;

using VVVV.Core.Logging;
using VVVV.Nodes.Generic;
#endregion usings

namespace VVVV.Nodes
{
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Spreads",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceSpreads : DeleteSlice<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "String",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceString : DeleteSlice<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Color",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceColor : DeleteSlice<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Transform",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceTransform : DeleteSlice<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Enumerations",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceEnum : DeleteSlice<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Raw",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, filter",
	            Author = "woei")]
	#endregion PluginInfo
	public class DeleteSliceRaw : DeleteSlice<System.IO.Stream> {}
}
