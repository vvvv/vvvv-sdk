using System;
using System.ComponentModel.Composition;
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

	public abstract class UnzipNode<T> : IPluginEvaluate
	{
		[Input("Input", BinSize = -2)]
		protected IInStream<T> FInputStream;

		[Output("Output", IsPinGroup = true)]
		protected IInStream<IOutStream<T>> FOutputStreams;
		
		public void Evaluate(int SpreadMax)
		{
			FOutputStreams.SetLengthBy(FInputStream);
	
			var buffer = MemoryPool<T>.GetArray();			
			try
			{
				var outputStreamsLength = FOutputStreams.Length;
				
				using (var reader = FInputStream.GetCyclicReader())
				{
					int i = 0;
					foreach (var outputStream in FOutputStreams)
					{
						int numSlicesToWrite = Math.Min(outputStream.Length, buffer.Length);
						
						reader.Position = i++;
						using (var writer = outputStream.GetWriter())
						{
							while (!writer.Eos)
							{
								reader.Read(buffer, 0, numSlicesToWrite, outputStreamsLength);
								writer.Write(buffer, 0, numSlicesToWrite);
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
	
	[PluginInfo(Name = "Unzip", Category = "Value", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class ValueUnzipNode : UnzipNode<double>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "Value", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class ValueBinSizeUnzipNode : UnzipNode<IInStream<double>>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "2d", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class Vector2DUnzipNode : UnzipNode<Vector2D>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "2d", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class Vector2DBinSizeUnzipNode : UnzipNode<IInStream<Vector2D>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "3d", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class Vector3DUnzipNode : UnzipNode<Vector3D>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "3d", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class Vector3DBinSizeUnzipNode : UnzipNode<IInStream<Vector3D>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "4d", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class Vector4DUnzipNode : UnzipNode<Vector4D>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "4d", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class Vector4DBinSizeUnzipNode : UnzipNode<IInStream<Vector4D>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "Color", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class ColorUnzipNode : UnzipNode<RGBAColor>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Color", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class ColorBinSizeUnzipNode : UnzipNode<IInStream<RGBAColor>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "String", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class StringUnzipNode : UnzipNode<string>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "String", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class StringBinSizeUnzipNode : UnzipNode<IInStream<string>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "Transform", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class TransformUnzipNode : UnzipNode<Matrix4x4>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Transform", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class TransformBinSizeUnzipNode : UnzipNode<IInStream<Matrix4x4>>
    {

    }
	
	[PluginInfo(Name = "Unzip", Category = "Enumerations", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
	public class EnumUnzipNode : UnzipNode<EnumEntry>
	{
		
	}

    [PluginInfo(Name = "Unzip", Category = "Enumerations", Version = "Bin", Help = UnzipInfo.HELP, Tags = UnzipInfo.TAGS)]
    public class EnumBinSizeUnzipNode : UnzipNode<IInStream<EnumEntry>>
    {

    }
}
