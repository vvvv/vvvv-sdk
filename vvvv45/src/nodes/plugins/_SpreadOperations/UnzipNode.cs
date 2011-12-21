//using System;
//using System.ComponentModel.Composition;
//
//using VVVV.PluginInterfaces.V1;
//using VVVV.PluginInterfaces.V2;
//using VVVV.Utils.VColor;
//using VVVV.Utils.VMath;
//
//namespace VVVV.Nodes
//{
//	public abstract class UnzipNode<T> : IPluginEvaluate
//	{
//		[Input("Input", BinSize = 1)]
//		protected ISpread<ISpread<T>> FInput;
//
//		[Output("Output 1")]
//		protected ISpread<ISpread<T>> FOutputA;
//		
//		[Output("Output 2")]
//		protected ISpread<ISpread<T>> FOutputB;
//		
//		public void Evaluate(int SpreadMax)
//		{
//			FOutputA.SliceCount = FInput.SliceCount - FInput.SliceCount / 2;
//			FOutputB.SliceCount = FInput.SliceCount / 2;
//			
//			for (int i = 0; i < FOutputA.SliceCount; i++)
//			{
//				FOutputA[i] = FInput[2 * i];
//			}
//			
//			for (int i = 0; i < FOutputB.SliceCount; i++)
//			{
//			    FOutputB[i] = FInput[2 * i + 1];
//			}
//		}
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "Spreads", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class SpreadsUnzipNode : UnzipNode<double>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "2d", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class Vector2DUnzipNode : UnzipNode<Vector2D>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "3d", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class Vector3DUnzipNode : UnzipNode<Vector3D>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "4d", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class Vector4DUnzipNode : UnzipNode<Vector4D>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "Color", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class ColorUnzipNode : UnzipNode<RGBAColor>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "String", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class StringUnzipNode : UnzipNode<string>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "Transform", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class TransformUnzipNode : UnzipNode<Matrix4x4>
//	{
//		
//	}
//	
//	[PluginInfo(Name = "Unzip", Category = "Enumerations", Help = "Unzips a spread into two spreads", Tags = "")]
//	public class EnumUnzipNode : UnzipNode<EnumEntry>
//	{
//		
//	}
//}
