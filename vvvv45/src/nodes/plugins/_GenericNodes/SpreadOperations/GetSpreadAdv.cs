#region usings
using System;
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
    [PluginInfo(Name = "GetSpread",
                Category = "Spreads",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadSpreads : GetSpreadAdvanced<double> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "String",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadString : GetSpreadAdvanced<string> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Color",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadColor : GetSpreadAdvanced<RGBAColor> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Transform",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadTransform : GetSpreadAdvanced<Matrix4x4> {}
    
    #region PluginInfo
    [PluginInfo(Name = "GetSpread",
                Category = "Enumerations",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadEnum : GetSpreadAdvanced<EnumEntry>
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
    [PluginInfo(Name = "GetSpread",
                Category = "Raw",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class GetSpreadRaw : GetSpreadAdvanced<System.IO.Stream> {}
}
