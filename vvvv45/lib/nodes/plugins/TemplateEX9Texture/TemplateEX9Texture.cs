#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

#endregion usings

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.TexturedVertex;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template", 
	            Category = "EX9.Texture", 
	            Version = "",
	            Help = "Basic template which creates a texture",
	            Tags = "")]
	#endregion PluginInfo
	public class Template : DXTextureOutPluginBase, IPluginEvaluate
	{
		#region fields & pins

		[Input("Wave Count", DefaultValue = 1)]
		IDiffSpread<double> FWaveCountIn;
		
		[Input("Width", DefaultValue = 64)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 64)]
		IDiffSpread<int> FHeightIn;

		[Import]
		ILogger FLogger;
		
		//track the current texture slice
		int FCurrentSlice;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public Template(IPluginHost host) 
		    : base(host)
		{
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSliceCount(SpreadMax);
			
			//recreate texture if resolution was changed
			if (FWidthIn.IsChanged || FHeightIn.IsChanged) 
			{
				//set new texture size
				Reinitialize();
			}

			//update texture
			if (FWaveCountIn.IsChanged) 
			{
				Update();
			}
			
		}
		
		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Texture CreateTexture(int Slice, Device device)
		{
			FLogger.Log(LogType.Debug, "Creating new texture at slice: " + Slice);
			return TextureUtils.CreateTexture(device, Math.Max(FWidthIn[Slice], 1), Math.Max(FHeightIn[Slice], 1));
		}
		
		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		protected unsafe override void UpdateTexture(int Slice, Texture texture)
		{
			FCurrentSlice = Slice;
			TextureUtils.Fill32BitTexInPlace(texture, FillTexure);
		}
		
		//this is a pixelshader like method, which we pass to the fill function
		private unsafe void FillTexure(uint* data, int row, int col, int width, int height)
		{
			//crate position in texture in the range [0..1]
			var x = (double)row / height;
			var y = (double)col / width;
			
			//make some waves in the range [0..255]
			var wave = (byte)(255*((Math.Sin(x * y * FWaveCountIn[FCurrentSlice] * 10) + 1) / 2.0));
			
			//a pixel is just a 32-bit unsigned int value
			var pixel = UInt32Utils.fromARGB(255, wave, wave, wave);
			
			//copy pixel into texture
			TextureUtils.SetPtrVal2D(data, pixel, row, col, width);
		}
	}
}
