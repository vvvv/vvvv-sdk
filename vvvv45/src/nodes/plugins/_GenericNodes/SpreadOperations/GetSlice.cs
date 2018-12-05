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
	[PluginInfo(Name = "GetSlice",
	            Category = "Transform",
	            Help = "Returns slices of the Input spread that are addressed by the Index pin.",
	            Tags = "")]
	#endregion PluginInfo
	public class GetSliceTransform : GetSlice<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSlice",
	            Category = "Enumerations",
	            Help = "Returns slices of the Input spread that are addressed by the Index pin.",
	            Tags = "")]
	#endregion PluginInfo
	public class GetSliceEnum : GetSlice<EnumEntry>
    {
        string FLastSubType;

        protected override void Prepare()
        {
            var outputPin = FOutputContainer.GetPluginIO() as IPin;
            var subType = outputPin.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (outputPin as IEnumOut).SetSubType(subType);
                (FInputContainer.GetPluginIO() as IEnumIn).SetSubType(subType);
            }
        }
    }
	
	#region PluginInfo
	[PluginInfo(Name = "GetSlice",
	            Category = "Raw",
	            Help = "Returns slices of the Input spread that are addressed by the Index pin.",
	            Tags = "")]
	#endregion PluginInfo
	public class GetSliceRaw : GetSlice<System.IO.Stream> {}
}
