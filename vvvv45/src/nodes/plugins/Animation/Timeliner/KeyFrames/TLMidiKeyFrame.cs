using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public class TLMidiKeyFrame: TLBaseKeyFrame
	{
		private int FNote;
		public int Note
		{
			get {return FNote;}
			set 
			{
				FNote = value;
				FPositionY = FNote;
			}
		}
		
		private int FVelocity;
		public int Velocity
		{
			get {return FVelocity;}
			set {FVelocity = value;}
		}
		
		private double FEnd;
		public double End
		{
   			get {return FEnd;}
   			set {FEnd = value;}
   		}
		
		private int FMinNote;
		public int MinNote
		{
   			get {return FMinNote;}
   			set {FMinNote = value;}
   		}
		
		private int FTrack;
		public int Track
		{
			get{return FTrack;}
			set {FTrack = value;}
		}
		
		private int FChannel;
		public int Channel
		{
			get{return FChannel;}
			set {FChannel = value;}
		}
		
		private int FMaxNote;
		public int MaxNote
		{
   			get {return FMaxNote;}
   			set {FMaxNote = value;}
   		}

		public override void MoveY(double Delta)
		{
			FNote -= (int) (Delta / FSliceHeight * (FMaxNote - FMinNote));
			FNote = (int) VMath.Clamp(FNote, 0, 127);
			FPositionY = FNote;
		}
		
		public TLMidiKeyFrame(TLTransformer Transformer, int Track, int Channel, int Note, int Velocity, double Start, double End, int MinNote, int MaxNote, float SliceTop, float SliceHeight): base(Transformer, Start, SliceTop, SliceHeight)
		{
			FTrack = Track;
			FChannel = Channel;
			this.Note = Note;
			FVelocity = Velocity;
			
			FMinNote = MinNote;
			FMaxNote = MaxNote;
			FEnd = End;			
		}
		
		public override void MoveTime(double DeltaTime, double Minimum, double Maximum)
		{
			double duration = FEnd - FTime;
			base.MoveTime(DeltaTime, Minimum, Maximum);
			FEnd = FTime + duration;
		}
		
		public float GetValueAsY()
		{
			return (float) VMath.Map(FNote, FMinNote, FMaxNote, FSliceHeight, 0, TMapMode.Float);
		}
		
		public float GetDurationAsX()
		{
			return FTransformer.TransformPoint(new PointF((float) FEnd, 0)).X - GetTimeAsX();
		}
		
		protected override Region GetRedrawArea()
		{
			float size = 10;
			float x = GetTimeAsX() - size/2;
			float y = FSliceTop + (float) GetValueAsY() - size/2;
			float width = Math.Max(GetDurationAsX() + size, 50);
			
			Region flag = new Region(new RectangleF(x, y-15, width, 45));
			return flag;
		}
		
		protected override Region GetHitArea()
		{
			float height = 10;
			float x = GetTimeAsX();
			float y = FSliceTop + (float) GetValueAsY();
			float width = GetDurationAsX();
		
			Region flag = new Region(new RectangleF(x, y, width, height));
			return flag;
		}
	}
}
