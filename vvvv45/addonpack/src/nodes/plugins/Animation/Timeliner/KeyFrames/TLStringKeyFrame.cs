using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public class TLStringKeyFrame: TLBaseKeyFrame
	{
		private string FValue;
		private int FWidth;
		private float FFlagY;
		
		public string Value
		{
			get {return FValue;}
			set 
			{
				FValue = value;
				System.Windows.Forms.Control c = new System.Windows.Forms.Control();
				//no better idea how to measure the string at this place?
				Graphics g = c.CreateGraphics();
				int width = (int) g.MeasureString(FValue, FFont).Width;
				g.Dispose();
				c.Dispose();
				
				FWidth = width + 6;
			}
		}
		
		public int Width
		{
			get {return FWidth;}
		}
		
		public float FlagY
		{
			get {return FFlagY;}
		}
		
		public override void MoveY(double Delta)
		{
			FFlagY = (float) VMath.Map(FFlagY*FSliceHeight + Delta, 0, FSliceHeight, 0, 1, TMapMode.Clamp);
		}
		
		public TLStringKeyFrame(TLTransformer Transformer, double Time, float Flag, string Value, float SliceTop, float SliceHeight): base(Transformer, Time, SliceTop, SliceHeight)
		{
			this.Value = Value;
			FFlagY = Flag;
		}
		
		public float GetFlagPosAsY()
		{
			return FFlagY * FSliceHeight;
		}
		
		protected override Region GetRedrawArea()
		{
			Region flag = new Region(new RectangleF((float) (GetTimeAsX())-2, FSliceTop + GetFlagPosAsY() - 15, Math.Max(FWidth+2, 40), 30));
			return flag;
		}
		
		protected override Region GetHitArea()
		{
			Region flag = new Region(new RectangleF((float) (GetTimeAsX()), FSliceTop + GetFlagPosAsY(), Math.Max(FWidth, 20), 15));
			return flag;
		}
	}
}
