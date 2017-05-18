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
	
	#region String Node
	[PluginInfo(Name = "Select", 
				Category = "String",
				Version = "Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei")]
	public class SelectString : SelectBin<string>
	{	
	}
	#endregion String Node
	
	#region Color Node
	[PluginInfo(Name = "Select", 
				Category = "Color",
				Version = "Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei")]
	public class SelectColor : SelectBin<RGBAColor>
	{	
	}
	#endregion Color Node
	
	#region Transform Node
	[PluginInfo(Name = "Select", 
				Category = "Transform",
				Version = "Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei")]
	public class SelectTransform : SelectBin<Matrix4x4>
	{	
	}
	#endregion Transform Node
	
	#region Enumerations Node
	[PluginInfo(Name = "Select", 
				Category = "Enumerations",
				Version = "Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei")]
	
	public class SelectEnum : SelectBin<EnumEntry>
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
	#endregion Enumerations Node
	
	#region Raw Node
	[PluginInfo(Name = "Select", 
				Category = "Raw",
				Version = "Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei")]
	
	public class SelectRaw : SelectBin<System.IO.Stream>
	{	
	}
	#endregion
}
