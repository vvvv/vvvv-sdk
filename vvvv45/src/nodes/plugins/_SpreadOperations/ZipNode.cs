using System;
using System.ComponentModel.Composition;
using System.Linq;
using SlimDX;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Collections.Generic;

namespace VVVV.Nodes
{
    static class ZipInfo 
    { 
        public const string HELP = "Zips spreads together";
        public const string TAGS = "spread";
    }

	public abstract class ZipNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		[Input("Input", IsPinGroup = true)]
		protected IInStream<IInStream<T>> FInputStreams;

        [Input("Allow Empty Spreads", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FAllowEmptySpreadsConfig;
		
		[Output("Output")]
		protected IOutStream<T> FOutputStream;

        private bool FAllowEmptySpreads;

        public void OnImportsSatisfied()
        {
            FAllowEmptySpreadsConfig.Changed += (s) => FAllowEmptySpreads = s[0];
        }
		
		public void Evaluate(int SpreadMax)
		{
            IEnumerable<IInStream<T>> inputStreams;
            int inputStreamsLength;
            if (FAllowEmptySpreads)
            {
                inputStreams = FInputStreams.Where(s => s.Length > 0);
                inputStreamsLength = inputStreams.Count();
            }
            else
            {
                inputStreams = FInputStreams;
                inputStreamsLength = FInputStreams.Length;
            }
            int maxInputStreamLength = inputStreams.GetMaxLength();
            FOutputStream.Length = maxInputStreamLength * inputStreamsLength;

            if (FOutputStream.Length > 0)
            {
                var buffer = MemoryPool<T>.GetArray();
                try
                {
                    using (var writer = FOutputStream.GetWriter())
                    {
                        int numSlicesToRead = Math.Min(maxInputStreamLength, buffer.Length);
                        int i = 0;
                        foreach (var inputStream in inputStreams)
                        {
                            writer.Position = i++;
                            using (var reader = inputStream.GetCyclicReader())
                            {
                                while (!writer.Eos)
                                {
                                    reader.Read(buffer, 0, numSlicesToRead);
                                    writer.Write(buffer, 0, numSlicesToRead, inputStreamsLength);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    MemoryPool<T>.PutArray(buffer);
                }
            }
		}
	}

    [PluginInfo(Name = "Zip", Category = "Value", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class ValueZipNode : ZipNode<double>
	{

	}
	
	[PluginInfo(Name = "Zip", Category = "Value", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class ValueBinSizeZipNode : ZipNode<IInStream<double>>
	{

	}
	
	[PluginInfo(Name = "Zip", Category = "2d", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class Vector2DZipNode : ZipNode<Vector2D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "2d", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class Vector2DBinSizeZipNode : ZipNode<IInStream<Vector2D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "3d", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class Vector3DZipNode : ZipNode<Vector3D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "3d", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class Vector3DBinSizeZipNode : ZipNode<IInStream<Vector3D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "4d", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class Vector4DZipNode : ZipNode<Vector4D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "4d", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class Vector4DBinSizeZipNode : ZipNode<IInStream<Vector4D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Color", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class ColorZipNode : ZipNode<RGBAColor>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Color", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class ColorBinSizeZipNode : ZipNode<IInStream<RGBAColor>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "String", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class StringZipNode : ZipNode<string>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "String", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class StringBinSizeZipNode : ZipNode<IInStream<string>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Transform", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class TransformZipNode : ZipNode<Matrix>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Transform", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class TransformBinSizeZipNode : ZipNode<IInStream<Matrix>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Enumerations", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
	public class EnumZipNode : ZipNode<EnumEntry>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Enumerations", Version = "Bin", Help = ZipInfo.HELP, Tags = ZipInfo.TAGS)]
    public class EnumBinSizeZipNode : ZipNode<IInStream<EnumEntry>>
    {

    }
}
