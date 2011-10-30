#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using Svg;
using Svg.Transforms;

#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SVGTexture", Category = "EX9.Texture", Help = "Renders a SVG graphic into a texture with a given size", Tags = "")]
	#endregion PluginInfo
	public class EX9_TextureSVGTextureNode : DXTextureOutPluginBase, IPluginEvaluate
	{
		#region fields & pins

		[Input("XML")]
		IDiffSpread<string> FXMLIn;
		
		[Input("Transform")]
		IDiffSpread<Matrix> FTransformIn;

		[Input("Width", DefaultValue = 128)]
		IDiffSpread<int> FWidthIn;

		[Input("Height", DefaultValue = 128)]
		IDiffSpread<int> FHeightIn;

		[Import()]
		ILogger FLogger;
		
		SvgDocument FSVGDoc;
		Bitmap FBitmap;
		RectangleF FOriginalDim;

		//track the current texture slice
		int FCurrentSlice;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public EX9_TextureSVGTextureNode(IPluginHost host) : base(host)
		{
		}
		
		//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
        static extern void CopyMemory(IntPtr dest, IntPtr src, int size);

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SetSliceCount(SpreadMax);

			//recreate texture if resolution was changed
			if (FWidthIn.IsChanged || FHeightIn.IsChanged) {
				//set new texture size
				Reinitialize();
			}
			
			if (FXMLIn.IsChanged)
			{
				var s = new MemoryStream(UTF8Encoding.Default.GetBytes(FXMLIn[0]));
                FSVGDoc = SvgDocument.Open(s, null);
				FOriginalDim = FSVGDoc.GetDimensions();
			}

			//update texture
			if (FXMLIn.IsChanged || FWidthIn.IsChanged || FHeightIn.IsChanged || FTransformIn.IsChanged) {
							
				
				FSVGDoc.Transforms = new SvgTransformCollection();

			    var sx = FWidthIn[0] / (float)FOriginalDim.Width;
				var sy = FHeightIn[0] / (float)FOriginalDim.Height;
				
				sx = Math.Max(sx, sy);
				
				var c = new SvgCircle();
				c.CenterX = 50;
				c.CenterY = 50;
				c.Radius = 100;
				
				//FSVGDoc.Children.Add(c);
				
                FSVGDoc.Transforms.Add(new SvgScale(sx, sx));
				var m = FTransformIn[0];
				var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
				FSVGDoc.Transforms.Add(mat);
                FSVGDoc.Width = new SvgUnit(SvgUnitType.Pixel, FWidthIn[0]);
                FSVGDoc.Height = new SvgUnit(SvgUnitType.Pixel, FHeightIn[0]);
                FBitmap = FSVGDoc.Draw();
				
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
		unsafe protected override void UpdateTexture(int Slice, Texture texture)
		{
			FCurrentSlice = Slice;
			var data = FBitmap.LockBits(new Rectangle(0, 0, FBitmap.Width, FBitmap.Height), ImageLockMode.ReadOnly, FBitmap.PixelFormat);
			var rect = texture.LockRectangle(0, LockFlags.None);
			
			CopyMemory(rect.Data.DataPointer, data.Scan0, data.Stride * data.Height);
			
			texture.UnlockRectangle(0);
			FBitmap.UnlockBits(data);

		}
	}
}
