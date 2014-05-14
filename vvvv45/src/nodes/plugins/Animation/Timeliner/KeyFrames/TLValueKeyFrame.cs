using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public enum TLInterpolationType {Step, Linear, Cubic, Spline};
	
	public class TLValueKeyFrame: TLBaseKeyFrame
	{
		private TLInterpolationType FInType, FOutType;
		private double FValue;
		private double FMinValue, FMaxValue;
		
		public double Value
		{
			get {return FValue;}
		}
		
		public double MinValue
		{
   			get {return FMinValue;}
   			set {FMinValue = value;}
   		}
		
		public double MaxValue
		{
   			get {return FMaxValue;}
   			set {FMaxValue = value;}
   		}
		
		public TLInterpolationType InType
		{
   			get {return FInType;}
   			set {FInType = value;}
   		}
		
		public TLInterpolationType OutType
		{
   			get {return FOutType;}
   			set {FOutType = value;}
   		}
		
		public override void MoveY(double Delta)
		{
			FValue = VMath.Map(FValue - Delta/FSliceHeight * (FMaxValue-FMinValue), FMinValue, FMaxValue, FMinValue, FMaxValue, TMapMode.Clamp);
			FPositionY = (float) FValue;
		}
		
		public TLValueKeyFrame(TLTransformer Transformer, double Time, double Value, double Minimum, double Maximum, TLInterpolationType InType, TLInterpolationType OutType, float SliceTop, float SliceHeight): base(Transformer, Time, SliceTop, SliceHeight)
		{
			SetValue(Value);
			FInType = InType;
			FOutType = OutType;
			FMinValue = Minimum;
			FMaxValue = Maximum;
		}
		
		public void SetValue(double Value)
		{
			FValue = Value;
		}
		
		public float GetValueAsY()
		{
			return (float) VMath.Map(FValue, FMinValue, FMaxValue, FSliceHeight, 0, TMapMode.Float);
		}
		
		protected override Region GetRedrawArea()
		{
			float size = 10;
			float x = GetTimeAsX() - size/2;
			float y = FSliceTop + (float) GetValueAsY() - size/2;
		
			Region flag = new Region(new RectangleF(x, y-15, 50, 45));
			return flag;
		}
		
		protected override Region GetHitArea()
		{
			float size = 10;
			float x = GetTimeAsX() - size/2;
			float y = FSliceTop + (float) GetValueAsY() - size/2;
		
			Region flag = new Region(new RectangleF(x, y, size, size));
			return flag;
		}
	}
}
