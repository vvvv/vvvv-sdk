using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;

namespace VVVV.Nodes.Timeliner
{
	public class TLColorKeyFrame: TLBaseKeyFrame
	{
		double FHue, FSaturation, FValue, FAlpha;
		
		public RGBAColor RGBAColor
		{
			get {return VColor.FromHSVA(FHue, FSaturation, FValue, FAlpha);}
		}
		
		public double Hue
		{
			get {return FHue;}
		}
		
		public double Saturation
		{
			get {return FSaturation;}
		}
		
		public double Value
		{
			get {return FValue;}
		}
		
		public double Alpha
		{
			get {return FAlpha;}
		}
		
		public void SetColorComponents(double Hue, double Saturation, double Value, double Alpha)
		{
			FHue = Hue;
			FSaturation = Saturation;
			FValue = Value;
			FAlpha = Alpha;
		}
		
		public void MoveValue(double Delta)
		{
			FValue = Math.Max(0, Math.Min(FValue * FSliceHeight - Delta, FSliceHeight)) / FSliceHeight;
		}
		
		public void MoveHue(double Delta)
		{
			FHue = VMath.Map(FHue + Delta, 0, 1, 0, 1, TMapMode.Wrap);
		}
		
		public void MoveSaturation(double Delta)
		{
			FSaturation = Math.Max(0, Math.Min(FSaturation * FSliceHeight - Delta, FSliceHeight)) / FSliceHeight;
		}
		
		public void MoveAlpha(double Delta)
		{
			FAlpha = Math.Max(0, Math.Min(FAlpha * FSliceHeight - Delta, FSliceHeight)) / FSliceHeight;
		}
		
		public TLColorKeyFrame(TLTransformer Transformer, double Time, RGBAColor Color, float SliceTop, float SliceHeight): base(Transformer, Time, SliceTop, SliceHeight)
		{
			VColor.RGBtoHSV(Color.R, Color.G, Color.B, out FHue, out FSaturation, out FValue);
			FAlpha = Color.A;
		}
		
		protected override Region GetRedrawArea()
		{
			Region flag = new Region(new RectangleF((float) (GetTimeAsX() - 20), FSliceTop, 40, FSliceHeight));
			return flag;
		}
	}
}
