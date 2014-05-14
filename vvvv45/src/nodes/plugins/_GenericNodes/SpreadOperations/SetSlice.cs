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
	[PluginInfo(Name = "SetSlice",
	            Category = "Transform",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSliceTransform : SetSlice<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSlice",
	            Category = "Enumerations",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSliceEnum : SetSlice<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSlice",
	            Category = "Raw",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSliceRaw : SetSlice<System.IO.Stream> {}
}
