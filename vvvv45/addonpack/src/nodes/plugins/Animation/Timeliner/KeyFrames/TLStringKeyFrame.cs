using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public class TLStringKeyFrame: TLBaseKeyFrame
	{
		private int FWidth;
		private string FValue;
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
		
		public override void MoveY(double Delta)
		{
			FPositionY = (float) VMath.Map(FPositionY*FSliceHeight + Delta, 0, FSliceHeight, 0, 1, TMapMode.Clamp);
		}
		
		public TLStringKeyFrame(TLTransformer Transformer, double Time, float Flag, string Value, float SliceTop, float SliceHeight): base(Transformer, Time, SliceTop, SliceHeight)
		{
			this.Value = Value;
			FPositionY = Flag;
		}
		
		public float GetFlagPosAsY()
		{
			return FPositionY * FSliceHeight;
		}
		
		protected override Region GetRedrawArea()
		{
			Region flag = new Region(new RectangleF((float) (GetTimeAsX())-5, FSliceTop + GetFlagPosAsY() - 15, Math.Max(FWidth+5, 40), 30));
			return flag;
		}
		
		protected override Region GetHitArea()
		{
			Region flag = new Region(new RectangleF((float) (GetTimeAsX()), FSliceTop + GetFlagPosAsY(), Math.Max(FWidth, 20), 15));
			return flag;
		}
	}
}
