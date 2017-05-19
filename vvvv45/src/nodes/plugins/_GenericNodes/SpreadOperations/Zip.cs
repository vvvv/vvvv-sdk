using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using SlimDX;
using VVVV.Hosting.Pins;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    static class ZipInfo 
    { 
        public const string HELP = "Interleaves all Input spreads.";
        public const string TAGS = "interleave, join, generic, spreadop";
    }

    [PluginInfo(Name = "Zip", Category = "Value", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class ValueZipNode : Zip<double>
	{

	}
	
	[PluginInfo(Name = "Zip", Category = "Value", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class ValueBinSizeZipNode : Zip<IInStream<double>>
	{

	}
	
	[PluginInfo(Name = "Zip", Category = "2d", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class Vector2DZipNode : Zip<Vector2D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "2d", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class Vector2DBinSizeZipNode : Zip<IInStream<Vector2D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "3d", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class Vector3DZipNode : Zip<Vector3D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "3d", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class Vector3DBinSizeZipNode : Zip<IInStream<Vector3D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "4d", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class Vector4DZipNode : Zip<Vector4D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "4d", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class Vector4DBinSizeZipNode : Zip<IInStream<Vector4D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Color", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class ColorZipNode : Zip<RGBAColor>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Color", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class ColorBinSizeZipNode : Zip<IInStream<RGBAColor>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "String", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class StringZipNode : Zip<string>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "String", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class StringBinSizeZipNode : Zip<IInStream<string>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Transform", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class TransformZipNode : Zip<Matrix>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Transform", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class TransformBinSizeZipNode : Zip<IInStream<Matrix>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Enumerations", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class EnumZipNode : Zip<EnumEntry>
	{
        string FLastSubType;

        protected override bool Prepare()
        {
            var outputPin = FOutputContainer.GetPluginIO() as IPin;
            var subType = outputPin.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (outputPin as IEnumOut).SetSubType(subType);
                foreach (var inputPin in FInputContainer.GetPluginIOs().OfType<IEnumIn>())
                    inputPin.SetSubType(subType);
                return true;
            }
            return base.Prepare();
        }
    }

    [PluginInfo(Name = "Zip", Category = "Enumerations", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class EnumBinSizeZipNode : Zip<IInStream<EnumEntry>>
    {
        string FLastSubType;

        protected override bool Prepare()
        {
            var outputPin = FOutputContainer.GetPluginIO() as IPin;
            var subType = outputPin.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (outputPin as IEnumOut).SetSubType(subType);
                foreach (var inputPin in FInputContainer.GetPluginIOs().OfType<IEnumIn>())
                    inputPin.SetSubType(subType);
                return true;
            }
            return base.Prepare();
        }
    }
    
    [PluginInfo(Name = "Zip", Category = "Raw", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class RawZipNode : Zip<System.IO.Stream>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Raw", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class RawBinSizeZipNode : Zip<IInStream<System.IO.Stream>>
    {

    }
}
