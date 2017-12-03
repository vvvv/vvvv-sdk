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
	            Help = "Replaces a sub-spread in the Spread that is addressed via Offset and Count, with the given Input.",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadSpreads : SetSpread<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "String",
	            Help = "Replaces a sub-spread in the Spread that is addressed via Offset and Count, with the given Input.",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadString : SetSpread<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Color",
	            Help = "Replaces a sub-spread in the Spread that is addressed via Offset and Count, with the given Input.",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadColor : SetSpread<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Transform",
	            Help = "Replaces a sub-spread in the Spread that is addressed via Offset and Count, with the given Input.",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadTransform : SetSpread<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Enumerations",
	            Help = "Replaces a sub-spread in the Spread that is addressed via Offset and Count, with the given Input.",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadEnum : SetSpread<EnumEntry>
    {
        string FLastSubType;

        protected override void Prepare()
        {
            var output = FOutputContainer.GetPluginIO() as IPin;
            var subType = output.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (output as IEnumOut).SetSubType(subType);
                (FSpreadContainer.GetPluginIO() as IEnumIn).SetSubType(subType);
                (FInputContainer.GetPluginIO() as IEnumIn).SetSubType(subType);
            }
        }
}
	
	#region PluginInfo
	[PluginInfo(Name = "SetSpread",
	            Category = "Raw",
	            Help = "Replaces a sub-spread in the Spread that is addressed via Offset and Count, with the given Input.",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class SetSpreadRaw : SetSpread<System.IO.Stream> {}
}
