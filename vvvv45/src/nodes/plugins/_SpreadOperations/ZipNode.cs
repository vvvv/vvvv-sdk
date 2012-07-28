using System;
using System.ComponentModel.Composition;
using SlimDX;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	public abstract class ZipNode<T> : IPluginEvaluate
	{
		[Input("Input", IsPinGroup = true)]
		protected IInStream<IInStream<T>> FInputStreams;
		
		[Output("Output")]
		protected IOutStream<T> FOutputStream;
		
		public void Evaluate(int SpreadMax)
		{
			int inputStreamsLength = FInputStreams.Length;
			int maxInputStreamLength = FInputStreams.GetMaxLength();
			FOutputStream.Length = maxInputStreamLength * inputStreamsLength;
			
			var buffer = MemoryPool<T>.GetArray();
			try
			{
				using (var writer = FOutputStream.GetWriter())
				{
					int numSlicesToRead = Math.Min(maxInputStreamLength, buffer.Length);
					int i = 0;
					foreach (var inputStream in FInputStreams)
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
	
	[PluginInfo(Name = "Zip", Category = "Value", Help = "Zips spreads together", Tags = "")]
	public class ValueZipNode : ZipNode<double>
	{

	}
	
	[PluginInfo(Name = "Zip", Category = "Value", Version = "Bin", Help = "Zips spreads together", Tags = "")]
	public class ValueBinSizeZipNode : ZipNode<IInStream<double>>
	{

	}
	
	[PluginInfo(Name = "Zip", Category = "2d", Help = "Zips spreads together", Tags = "")]
	public class Vector2DZipNode : ZipNode<Vector2D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "2d", Version = "Bin", Help = "Zips spreads together", Tags = "")]
    public class Vector2DBinSizeZipNode : ZipNode<IInStream<Vector2D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "3d", Help = "Zips spreads together", Tags = "")]
	public class Vector3DZipNode : ZipNode<Vector3D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "3d", Version = "Bin", Help = "Zips spreads together", Tags = "")]
    public class Vector3DBinSizeZipNode : ZipNode<IInStream<Vector3D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "4d", Help = "Zips spreads together", Tags = "")]
	public class Vector4DZipNode : ZipNode<Vector4D>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "4d", Version = "Bin", Help = "Zips spreads together", Tags = "")]
    public class Vector4DBinSizeZipNode : ZipNode<IInStream<Vector4D>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Color", Help = "Zips spreads together", Tags = "")]
	public class ColorZipNode : ZipNode<RGBAColor>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Color", Version = "Bin", Help = "Zips spreads together", Tags = "")]
    public class ColorBinSizeZipNode : ZipNode<IInStream<RGBAColor>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "String", Help = "Zips spreads together", Tags = "")]
	public class StringZipNode : ZipNode<string>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "String", Version = "Bin", Help = "Zips spreads together", Tags = "")]
    public class StringBinSizeZipNode : ZipNode<IInStream<string>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Transform", Help = "Zips spreads together", Tags = "")]
	public class TransformZipNode : ZipNode<Matrix>
	{
		
	}

    [PluginInfo(Name = "Zip", Category = "Transform", Version = "Bin", Help = "Zips spreads together", Tags = "")]
    public class TransformBinSizeZipNode : ZipNode<IInStream<Matrix>>
    {

    }
	
	[PluginInfo(Name = "Zip", Category = "Enumerations", Help = "Zips spreads together", Tags = "")]
	public class EnumZipNode : ZipNode<EnumEntry>
	{
		
	}
}
