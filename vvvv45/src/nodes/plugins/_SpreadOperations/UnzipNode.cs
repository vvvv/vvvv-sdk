using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	public abstract class UnzipNode<T> : IPluginEvaluate
	{
		[Input("Input", BinSize = 1)]
		protected ISpread<T> FInput;

		[Output("Output", IsPinGroup = true)]
		protected ISpread<ISpread<T>> FOutputSpreads;
		
		private readonly T[] FBuffer = new T[128];
		
		public void Evaluate(int SpreadMax)
		{
			FOutputSpreads.SetSliceCountBy(FInput);
			
			var inputStream = FInput.GetStream();
			var outputSpreadCount = FOutputSpreads.SliceCount;
			for (int i = 0; i < outputSpreadCount; i++)
			{
				var outputStream = FOutputSpreads[i].GetStream();
				int numSlicesToWrite = Math.Min(outputStream.Length, FBuffer.Length);
				
				inputStream.ReadPosition = i;
				while (!outputStream.Eof)
				{
					inputStream.ReadCyclic(FBuffer, 0, numSlicesToWrite, outputSpreadCount);
					outputStream.Write(FBuffer, 0, numSlicesToWrite);
				}
			}
		}
	}
	
	[PluginInfo(Name = "Unzip", Category = "Value", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class ValueUnzipNode : UnzipNode<double>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "Spreads", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class Value2dUnzipNode : UnzipNode<ISpread<double>>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "2d", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class Vector2DUnzipNode : UnzipNode<Vector2D>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "3d", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class Vector3DUnzipNode : UnzipNode<Vector3D>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "4d", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class Vector4DUnzipNode : UnzipNode<Vector4D>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "Color", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class ColorUnzipNode : UnzipNode<RGBAColor>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "String", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class StringUnzipNode : UnzipNode<string>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "Transform", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class TransformUnzipNode : UnzipNode<Matrix4x4>
	{
		
	}
	
	[PluginInfo(Name = "Unzip", Category = "Enumerations", Help = "Unzips a spread into multiple spreads", Tags = "")]
	public class EnumUnzipNode : UnzipNode<EnumEntry>
	{
		
	}
}
