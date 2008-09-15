using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public class TLValueSlice :  TLSlice
	{
		private IValueConfig FKeyTime, FKeyValue, FKeyInType, FKeyOutType;
		private double FOutput;
		private double FMinValue;
		private double FMaxValue;
		private TLInterpolationType FCurrentInType = TLInterpolationType.Cubic;
		private TLInterpolationType FCurrentOutType = TLInterpolationType.Cubic;
		
		public double MinValue 
		{
   			get { return FMinValue;}
   			set 
   			{ 
   				FMinValue = value;
   				foreach (TLValueKeyFrame k in FKeyFrames)
   					k.MinValue = FMinValue;
   			}
		}
		
		public double MaxValue 
		{
   			get { return FMaxValue;}
   			set 
   			{ 
   				FMaxValue = value;
   				foreach (TLValueKeyFrame k in FKeyFrames)
   					k.MaxValue = FMaxValue;
   			}
		}		
		
		public double Output
		{
			get {return FOutput;}
		}
		
		public TLValueSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder): base(Host, Pin, SliceIndex, PinsOrder)
		{}
		
		protected override void CreatePins()
		{
			FHost.CreateValueConfig(FPin.Name + "-Time" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyTime);        	
			FKeyTime.SliceCount=0;
			FKeyTime.SetSubType(double.MinValue, double.MaxValue, 0.001D, 0, false, false, false);
			
			FHost.CreateValueConfig(FPin.Name + "-Value" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyValue);        	
			FKeyValue.SliceCount=0;
			FKeyValue.SetSubType(double.MinValue, double.MaxValue, 0.001D, 0, false, false, false);
			
			FHost.CreateValueConfig(FPin.Name + "-InType" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyInType);        	
			FKeyInType.SliceCount=0;
			FKeyInType.SetSubType(0, 5, 1, 0, false, false, true);
			
			FHost.CreateValueConfig(FPin.Name + "-OutType" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyOutType);        	
			FKeyOutType.SliceCount=0;
			FKeyOutType.SetSubType(0, 5, 1, 0, false, false, true);
		}
		
		public override void DestroyPins()
		{
			FHost.DeletePin(FKeyTime);
			FHost.DeletePin(FKeyValue);
			FHost.DeletePin(FKeyInType);
			FHost.DeletePin(FKeyOutType);
			FKeyTime = null;
			FKeyValue = null;
			FKeyInType = null;
			FKeyOutType = null;
			
			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			FKeyTime.Name = FPin.Name + "-Time" + FSliceIndex.ToString();
			FKeyValue.Name = FPin.Name + "-Value" + FSliceIndex.ToString();
			FKeyInType.Name = FPin.Name + "-InType" + FSliceIndex.ToString();			
			FKeyOutType.Name = FPin.Name + "-OutType" + FSliceIndex.ToString();			
		}
		
		public override void Evaluate(double CurrentTime)
		{
			double t;
			TLBaseKeyFrame kf = FKeyFrames.FindLast(delegate(TLBaseKeyFrame k) {return k.Time <= CurrentTime;});
			TLBaseKeyFrame kf1 = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.Time >= CurrentTime;});

			if (kf == null || kf1 == null)
				FOutput = 0;
			else
			{
				if((kf as TLValueKeyFrame).OutType == TLInterpolationType.Step)
					FOutput = (kf as TLValueKeyFrame).Value;
				else if((kf as TLValueKeyFrame).OutType == TLInterpolationType.Linear && kf1 != null)
				{
					// LINEAR INTERPOLATION
					t = VMath.Map(CurrentTime, kf.Time, kf1.Time, 0, 1, TMapMode.Float);
					FOutput = VMath.Lerp((kf as TLValueKeyFrame).Value, (kf1 as TLValueKeyFrame).Value, t);
				}
				else if((kf as TLValueKeyFrame).OutType == TLInterpolationType.Cubic && kf1 != null)
				{
					// CUBIC INTERPOLATION
					t = VMath.Map(CurrentTime, kf.Time, kf1.Time, 0, 1, TMapMode.Float);
					FOutput = VMath.SolveCubic(t, (kf as TLValueKeyFrame).Value, (kf as TLValueKeyFrame).Value, (kf1 as TLValueKeyFrame).Value, (kf1 as TLValueKeyFrame).Value);
				}
				//spline here
			}
			
			FOutputAsString = FOutput.ToString("f4", TimelinerPlugin.GNumberFormat);
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!
			if (Input == FKeyOutType && FirstFrame)
			{
				FKeyFrames.Clear();
				//System.Windows.Forms.MessageBox.Show("FKeyOutType: " + FKeyOutType.SliceCount.ToString());
				double time, val, intype, outtype;
				for (int i = 0; i<FKeyOutType.SliceCount;i++)	
				{
					FKeyTime.GetValue(i, out time);
					FKeyValue.GetValue(i, out val);
					FKeyInType.GetValue(i, out intype);
					FKeyOutType.GetValue(i, out outtype);
					AddKeyFrame(time, val, (TLInterpolationType) intype, (TLInterpolationType) outtype);
				}
				
				FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return k0.Time.CompareTo(k1.Time); });	
			}
			
			base.Configurate(Input, FirstFrame);
		}
		
		private TLBaseKeyFrame AddKeyFrame(double Time, double Value, TLInterpolationType InType, TLInterpolationType OutType)
		{
			//called via configpinchanged and gui-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			TLValueKeyFrame k = new TLValueKeyFrame(FPin.Transformer, Time, Value, FMinValue, FMaxValue, InType, OutType, slicetop, sliceheight);
			FKeyFrames.Add(k);
			return k;
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			return AddKeyFrame(FPin.Transformer.XPosToTime(P.X), (float) VMath.Map(P.Y, slicetop, slicetop + sliceheight, FMaxValue, FMinValue, TMapMode.Float), FCurrentInType, FCurrentOutType);
		}
		
		public override void SaveKeyFrames()
		{
			FKeyTime.SliceCount = FKeyFrames.Count;
			FKeyValue.SliceCount = FKeyFrames.Count;			
			FKeyInType.SliceCount = FKeyFrames.Count;
			FKeyOutType.SliceCount = FKeyFrames.Count;
			
			FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return  k0.Time.CompareTo(k1.Time); });
			
			for (int i = 0; i<FKeyFrames.Count; i++)
			{
				FKeyTime.SetValue(i, FKeyFrames[i].Time);
				FKeyValue.SetValue(i, (FKeyFrames[i] as TLValueKeyFrame).Value);				
				FKeyInType.SetValue(i, (int) (FKeyFrames[i] as TLValueKeyFrame).InType);
				FKeyOutType.SetValue(i, (int) (FKeyFrames[i] as TLValueKeyFrame).OutType);
			}
		}
		
		public void SetKeyFrameTypes(TLInterpolationType InType, TLInterpolationType OutType)
		{
			FCurrentInType = InType;
			FCurrentOutType = OutType;
			
			foreach (TLValueKeyFrame k in KeyFrames)
			{
				if (k.Selected)
				{
					k.InType = InType;
					k.OutType = OutType;					
				}
			}
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne)
		{
			if (Math.Abs(FMinValue-FMaxValue) < 0.0001)
				return;
			
			base.DrawSlice(g, From, To, AllInOne);
						
			float sliceHeight = FPin.Height / FPin.SliceCount;
			
			//draw 0 line
			float y_;
			if (AllInOne)
			{
				if  (FSliceIndex == 0)
				{
					y_ = (float) VMath.Map(0, FMinValue, FMaxValue, FPin.Height, 0, TMapMode.Float);
					g.DrawLine(new Pen(Color.Snow), 0, y_, g.ClipBounds.Width, y_);
				}
			}				
			else
			{
				y_ = (float) VMath.Map(0, FMinValue, FMaxValue, sliceHeight, 0, TMapMode.Float);
				g.DrawLine(new Pen(Color.Snow), 0, y_, g.ClipBounds.Width, y_);
			}
				
			//draw lines
			for (int i=0; i < FInvalidKeyFrames.Count-1; i++)
			{
				float x0 = (FInvalidKeyFrames[i] as TLValueKeyFrame).GetTimeAsX();
				float y0 = (float) (FInvalidKeyFrames[i] as TLValueKeyFrame).GetValueAsY();
				
				float x1 = (FInvalidKeyFrames[i+1] as TLValueKeyFrame).GetTimeAsX();
				float y1 = (float) (FInvalidKeyFrames[i+1] as TLValueKeyFrame).GetValueAsY();
				
				Pen p = new Pen(Color.Gray, 2);
				
				if ((FInvalidKeyFrames[i] as TLValueKeyFrame).OutType == TLInterpolationType.Step)
				{
					//STEP Interpolation
					g.DrawLine(p, x0, y0, x1, y0);
					g.DrawLine(p, x1, y0, x1, y1);
				}
				else if ((FInvalidKeyFrames[i] as TLValueKeyFrame).OutType == TLInterpolationType.Linear)
				{
					//LINEAR Interpolation
					g.DrawLine(p, x0, y0, x1, y1);
				}
				else if ((FInvalidKeyFrames[i] as TLValueKeyFrame).OutType == TLInterpolationType.Cubic)
				{
					//CUBIC Interpolation
					g.DrawBezier(p,x0,y0, x0+((x1-x0)/2), y0, x1-((x1-x0)/2), y1, x1, y1);
				}
			}
			
			//draw handler
			foreach (TLValueKeyFrame k in FInvalidKeyFrames)
			{
				//transform the keyframes time by the current transformation
				float size = 5;
				float x = k.GetTimeAsX() - size/2;
				float y = k.GetValueAsY() - size/2;
				
				if (k.Selected)
				{
					g.FillRectangle(new SolidBrush(Color.Silver), x, y, size, size);
					g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, new SolidBrush(Color.Black), x, y-14);
					g.DrawString(k.Value.ToString("f4", TimelinerPlugin.GNumberFormat), FFont, new SolidBrush(Color.Black), x, y+7);
				}
				else
				{
					g.FillRectangle(new SolidBrush(Color.White), x, y, size, size);
				}
				
				float sWidth = g.MeasureString(FOutputAsString, FFont).Width + 2;
				g.DrawString(FOutputAsString, FFont, new SolidBrush(Color.Gray), g.ClipBounds.Width-sWidth, sliceHeight-16);
				
				/*Random r = new Random();
				Color c = Color.FromArgb(255, 255, 255, r.Next(255));
				g.FillRegion(new SolidBrush(c), g.Clip);*/
			}
		}
	}
}
