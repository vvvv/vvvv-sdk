using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	public abstract class ZipNode<T> : IPluginEvaluate
	{
		[Input("Input 1", BinSize = 1)]
		protected ISpread<ISpread<T>> FInputA;
		
		[Input("Input 2", BinSize = 1)]
		protected ISpread<ISpread<T>> FInputB;
		
		[Output("Output")]
		protected ISpread<ISpread<T>> FOutput;
		
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = FInputA.CombineWith(FInputB) * 2;
			
			for (int i = 0; i < FOutput.SliceCount / 2; i++)
			{
				FOutput[2 * i] = FInputA[i];
				FOutput[2 * i + 1] = FInputB[i];
			}
		}
	}
	
	[PluginInfo(Name = "Zip", Category = "Spreads", Help = "zips two spreads together", Tags = "")]
	public class SpreadsZipNode : ZipNode<double>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "2d", Help = "zips two spreads together", Tags = "")]
	public class Vector2DZipNode : ZipNode<Vector2D>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "3d", Help = "zips two spreads together", Tags = "")]
	public class Vector3DZipNode : ZipNode<Vector3D>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "4d", Help = "zips two spreads together", Tags = "")]
	public class Vector4DZipNode : ZipNode<Vector4D>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "Color", Help = "zips two spreads together", Tags = "")]
	public class ColorZipNode : ZipNode<RGBAColor>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "String", Help = "zips two spreads together", Tags = "")]
	public class StringZipNode : ZipNode<string>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "Transform", Help = "zips two spreads together", Tags = "")]
	public class TransformZipNode : ZipNode<Matrix4x4>
	{
		
	}
	
	[PluginInfo(Name = "Zip", Category = "Enumerations", Help = "zips two spreads together", Tags = "")]
	public class EnumZipNode : ZipNode<EnumEntry>
	{
		
	}
}
