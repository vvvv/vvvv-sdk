using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    static class UnzipInfo
    {
        public const string HELP = "Unzips a spread into multiple spreads";
        public const string TAGS = "spread, split";
    }

	[PluginInfo(Name = "Unzip", Category = "Value", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class ValueUnzipNode : Unzip<double>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "Value", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class ValueBinSizeUnzipNode : Unzip<IInStream<double>>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "2d", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class Vector2DUnzipNode : Unzip<Vector2D>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "2d", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class Vector2DBinSizeUnzipNode : Unzip<IInStream<Vector2D>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "3d", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class Vector3DUnzipNode : Unzip<Vector3D>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "3d", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class Vector3DBinSizeUnzipNode : Unzip<IInStream<Vector3D>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "4d", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class Vector4DUnzipNode : Unzip<Vector4D>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "4d", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class Vector4DBinSizeUnzipNode : Unzip<IInStream<Vector4D>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "Color", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class ColorUnzipNode : Unzip<RGBAColor>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Color", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class ColorBinSizeUnzipNode : Unzip<IInStream<RGBAColor>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "String", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class StringUnzipNode : Unzip<string>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "String", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class StringBinSizeUnzipNode : Unzip<IInStream<string>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "Transform", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class TransformUnzipNode : Unzip<Matrix4x4>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Transform", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class TransformBinSizeUnzipNode : Unzip<IInStream<Matrix4x4>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "Enumerations", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class EnumUnzipNode : Unzip<EnumEntry>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Enumerations", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class EnumBinSizeUnzipNode : Unzip<IInStream<EnumEntry>>
    {

    }
    
    [PluginInfo(Name = "Unzip", Category = "Raw", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class RawUnzipNode : Unzip<System.IO.Stream>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Raw", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class RawBinSizeUnzipNode : Unzip<IInStream<System.IO.Stream>>
    {

    }
}
