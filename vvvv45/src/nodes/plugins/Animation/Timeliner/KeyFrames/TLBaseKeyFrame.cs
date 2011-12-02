using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public abstract class TLBaseKeyFrame
	{
		protected Font FFont = new Font("Verdana", 7);
		
		private bool FSelected;
		protected float FSliceTop;
		protected float FSliceHeight;
		protected TLTransformer FTransformer;
		
		protected double FTime;
		public double Time
		{
			get {return FTime;}
			set {FTime = value;}
		}
		
		protected float FPositionY;
		public float PositionY
		{
			get {return FPositionY;}
			set {FPositionY = value;}
		}
		
		public float SliceTop
		{
			set {FSliceTop = value;}
			get {return FSliceTop;}
		}
		
		public float SliceHeight
		{
			set {FSliceHeight = value;}
			get {return FSliceHeight;}
		}
		
		public Region RedrawArea
		{
			get {return GetRedrawArea();}
		}
		
		public Region HitArea
		{
			get {return GetHitArea();}
		}
		
		public Region CollapsedHitArea
		{
			get 
			{
				float size = 10;
				float x = GetTimeAsX() - size/2;
				float y = FSliceTop;
		
				Region flag = new Region(new RectangleF(x, y, size, FSliceHeight));
				return flag;
			}
		}
		
		public bool Selected
		{
			get {return FSelected;}
			set {FSelected = value;}
		}
		
		public TLBaseKeyFrame(TLTransformer Transformer, double NewTime, float SliceTop, float SliceHeight)
		{
			FTime = NewTime;
			FSliceTop = SliceTop;
			FSliceHeight = SliceHeight;
			FTransformer = Transformer;
		}

		protected abstract Region GetRedrawArea();
		protected virtual Region GetHitArea()
		{
			return GetRedrawArea();
		}
		
		public bool HitByPoint(PointF P, bool Collapsed)
		{
			if (Collapsed)
				return CollapsedHitArea.IsVisible(P);
			else
				return HitArea.IsVisible(P);
		}
		
		public bool HitByRect(RectangleF R, bool Collapsed)
		{
			if (Collapsed)
				return CollapsedHitArea.IsVisible(R);
			else
				return HitArea.IsVisible(R);
		}
		
		public bool SelectByRect(RectangleF R, bool Collapsed)
		{
			return FSelected = HitByRect(R, Collapsed);
		}

		public virtual void MoveTime(double DeltaTime, double Minimum, double Maximum)
		{
			FTime = Math.Min(Math.Max(FTime + DeltaTime / FTransformer.GTimeScale, Minimum), Maximum);
		}
		
		public virtual void MoveY(double Delta)
		{}
		
		public float GetTimeAsX()
		{
			return FTransformer.TransformPoint(new PointF((float) FTime, 0)).X;
		}
		
		
	}
}
