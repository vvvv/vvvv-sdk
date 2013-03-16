#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;


#endregion usings

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.TexturedVertex;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "QRCode", 
				Category = "EX9.Texture", 
				Help = "Encodes a string to be displayed as a QR code symbol on a texture. QR code is trademarked by Denso Wave, Inc.", Tags = "")]
	#endregion PluginInfo
	public class EX9_TextureQRCodeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		//little helper class used to store information for each
		//texture resource
		class Info
		{
			public int Slice;
			public int PixelSize;
			public string Text;
			public ErrorCorrectionLevel ECLevel;
			public QuietZoneModules QZModules;
			public Bitmap QRCodeBMP;
		}

		[Input("Text", DefaultString = "vvvv")]
		ISpread<string> FText;
		
		[Input("Pixel Size", DefaultValue = 5, MinValue = 1)]
		ISpread<int> FPixelSize;
		
		[Input("Back Color", DefaultColor = new double[] {1, 1, 1, 1})]
		IDiffSpread<RGBAColor> FBackColor;

		[Input("Fore Color", DefaultColor = new double[] {0, 0, 0, 1})]
		IDiffSpread<RGBAColor> FForeColor;
		
		[Input("Error Correction Level", DefaultEnumEntry="H", Visibility=PinVisibility.OnlyInspector)]
		ISpread<ErrorCorrectionLevel> FErrorCorrectionLevel;
		
		[Input("Quiet Zone Modules", DefaultEnumEntry="Two", Visibility=PinVisibility.OnlyInspector)]
		ISpread<QuietZoneModules> FQuietZoneModules;
		
		[Output("Texture Out")]
		ISpread<TextureResource<Info>> FTextureOut;
		
		private MemoryStream FMemoryStream;

		[Import()]
		ILogger FLogger;

		public void OnImportsSatisfied()
		{
			//spreads have a length of one by default, change it
			//to zero so ResizeAndDispose works properly.
			FTextureOut.SliceCount = 0;
			FMemoryStream = new MemoryStream();
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			FTextureOut.ResizeAndDispose(spreadMax, CreateTextureResource);
			for (int i = 0; i < spreadMax; i++) 
			{
				var textureResource = FTextureOut[i];
				var info = textureResource.Metadata;
				//recreate textures if resolution was changed
				if (info.PixelSize != FPixelSize[i] || info.Text != FText[i] || info.ECLevel != FErrorCorrectionLevel[i] || info.QZModules != FQuietZoneModules[i]) 
				{
					textureResource.Dispose();
					textureResource = CreateTextureResource(i);
					info = textureResource.Metadata;
				}
				
				//update textures if their colors changed
				if (FBackColor.IsChanged || FForeColor.IsChanged) 
				{
					ComputeQRCode(info, i);
					textureResource.NeedsUpdate = true;
				}
				else 
					textureResource.NeedsUpdate = false;
				
				FTextureOut[i] = textureResource;
			}
		}
		
		void ComputeQRCode(Info info, int slice)
		{
			var qrEncoder = new QrEncoder(FErrorCorrectionLevel[slice]);
            var qrCode = new QrCode();
            if (qrEncoder.TryEncode(FText[slice], out qrCode))
			{
				using (var fore = new SolidBrush(FForeColor[slice].Color))
	            	using (var back = new SolidBrush(FBackColor[slice].Color))
		            {
		            	var renderer = new GraphicsRenderer(new FixedModuleSize(FPixelSize[slice], FQuietZoneModules[slice]), fore, back);
						DrawingSize dSize = renderer.SizeCalculator.GetSize(qrCode.Matrix.Width);
		            	var bmp = new Bitmap(dSize.CodeWidth, dSize.CodeWidth);
		            	using (var g = Graphics.FromImage(bmp))
		            		renderer.Draw(g, qrCode.Matrix);
						info.QRCodeBMP = bmp;
		            }
			}
			else
				info.QRCodeBMP = null;
		}

		TextureResource<Info> CreateTextureResource(int slice)
		{
			var info = new Info {
				Slice = slice,
				PixelSize = FPixelSize[slice],
				ECLevel = FErrorCorrectionLevel[slice],
				QZModules = FQuietZoneModules[slice],
				Text = FText[slice]
			};
			
			ComputeQRCode(info, slice);
			return TextureResource.Create(info, CreateTexture, UpdateTexture);
		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		Texture CreateTexture(Info info, Device device)
		{
			if (info.QRCodeBMP != null)
				return TextureUtils.CreateTexture(device, Math.Max(info.QRCodeBMP.Width, 1), Math.Max(info.QRCodeBMP.Height, 1));
			else
				return null;
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe void UpdateTexture(Info info, Texture texture)
		{
			if (info.QRCodeBMP != null)
				TextureUtils.CopyBitmapToTexture(info.QRCodeBMP, texture);
		}
	}
}
