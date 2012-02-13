using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.Nodes.Timeliner
{
	public class TLColorSlice :  TLSlice
	{
		private IValueConfig FKeyTime;
		private IColorConfig FKeyColor;
		private RGBAColor FOutput;
		
		public RGBAColor Output
		{
			get {return FOutput;}
		}
		
		public TLColorSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder): base(Host, Pin, SliceIndex, PinsOrder)
		{}
		
		protected override void CreatePins()
		{
			FHost.CreateValueConfig(FPin.Name + "-Time" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyTime);
			FKeyTime.SliceCount=0;
			FKeyTime.SetSubType(Double.MinValue, Double.MaxValue,0.001D,0,false, false,false);
			
			FHost.CreateColorConfig(FPin.Name + "-Color" + FSliceIndex.ToString(), TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyColor);
			FKeyColor.SliceCount=0;
			FKeyColor.SetSubType(VColor.Black, true);
		}
		
		public override void DestroyPins()
		{
			FHost.DeletePin(FKeyTime);
			FHost.DeletePin(FKeyColor);
			FKeyTime = null;
			FKeyColor = null;

			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			FKeyTime.Name = FPin.Name + "-Time" + FSliceIndex.ToString();
			FKeyColor.Name = FPin.Name + "-Color" + FSliceIndex.ToString();
			
			FKeyTime.Order = FPin.Order;
			FKeyColor.Order = FPin.Order;
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			if (FKeyFrames.Count == 0)
				FOutput = VColor.Black;
			else
			{
				TLColorKeyFrame from = (TLColorKeyFrame) FKeyFrames.FindLast(delegate(TLBaseKeyFrame k) {return k.Time <= CurrentTime;});
				TLColorKeyFrame to = (TLColorKeyFrame) FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.Time > CurrentTime;});
				
				if (from == null)
					from = to;
				if (to == null)
					to = from;
				/*
				
				int a1, r1, g1, b1, a2, r2, g2, b2;
				a1 = from.Color.A;
				a2 = to.Color.A;
				r1 = from.Color.R;
				r2 = to.Color.R;
				g1 = from.Color.G;
				g2 = to.Color.G;
				b1 = from.Color.B;
				b2 = to.Color.B;
				
				double tSpan, f, tCurrent;
				tSpan = to.Time - from.Time;
				tCurrent = CurrentTime - from.Time;
				
				if (tSpan == 0)
					f = 0;
				else
					f = tCurrent / tSpan;
				
				;
				
				int a, r, g, b;
				
				a = (int) Math.Round(a2 * f + a1 * (1-f));
				r = (int) Math.Round(r2 * f + r1 * (1-f));
				g = (int) Math.Round(g2 * f + g1 * (1-f));
				b = (int) Math.Round(b2 * f + b1 * (1-f));
				 */
				
				double tSpan, f, tCurrent;
				tSpan = to.Time - from.Time;
				tCurrent = CurrentTime - from.Time;
				
				if (tSpan == 0)
					f = 0;
				else
					f = tCurrent / tSpan;
				FOutput = VColor.LerpRGBA(from.RGBAColor, to.RGBAColor, f);
			}
			
			double h, s, v, a;
			VColor.RGBtoHSV(FOutput.R, FOutput.G, FOutput.B, out h, out s, out v);
			a = FOutput.A;
			
			OutputAsString = "H: " + h.ToString("f2") + " S: " + s.ToString("f2") + " V: " + v.ToString("f2") + " A: " + a.ToString("f2");
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!
			if (Input == FKeyColor && FirstFrame)
			{
				FKeyFrames.Clear();
				
				RGBAColor c;
				double time;
				
				for (int i = 0; i<FKeyColor.SliceCount;i++)
				{
					FKeyTime.GetValue(i, out time);
					FKeyColor.GetColor(i, out c);
					AddKeyFrame(time, c);
				}
				
				FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return k0.Time.CompareTo(k1.Time); });
			}
			
			base.Configurate(Input, FirstFrame);
		}
		
		private TLBaseKeyFrame AddKeyFrame(double Time, RGBAColor Color)
		{
			//called via configpinchanged and gui-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			TLColorKeyFrame k = new TLColorKeyFrame(FPin.Transformer, Time, Color, slicetop, sliceheight);
			FKeyFrames.Add(k);
			
			return k;
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			return AddKeyFrame(FPin.Transformer.XPosToTime(P.X), VColor.Green);
		}
		
		public override void SaveKeyFrames()
		{
			FKeyTime.SliceCount = FKeyFrames.Count;
			FKeyColor.SliceCount = FKeyFrames.Count;
			
			FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return  k0.Time.CompareTo(k1.Time); });
			
			for (int i = 0; i<FKeyFrames.Count; i++)
			{
				FKeyTime.SetValue(i, FKeyFrames[i].Time);
				TLColorKeyFrame kfc = (FKeyFrames[i] as TLColorKeyFrame);
				FKeyColor.SetColor(i, kfc.RGBAColor);
			}
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne, bool Collapsed)
		{
			base.DrawSlice(g, From, To, AllInOne, Collapsed);
			
			
			TLColorKeyFrame kfc;
			float left, width;
			Color c1, c2;
			RectangleF r;
			
			float sliceHeight = FPin.Height / FPin.SliceCount;
			width = g.ClipBounds.Width;
			
			g.FillRectangle(new HatchBrush(HatchStyle.LargeCheckerBoard, Color.White, Color.Gray), g.ClipBounds);
			
			//draw all keyframes that have a right neighbour
			for (int i=0; i<FInvalidKeyFrames.Count-1; i++)
			{
				kfc = FInvalidKeyFrames[i] as TLColorKeyFrame;

				left = kfc.GetTimeAsX();
				c1 = kfc.RGBAColor.Color;
				c2 = (FInvalidKeyFrames[i+1] as TLColorKeyFrame).RGBAColor.Color;
				width = FInvalidKeyFrames[i+1].GetTimeAsX() - left;
				r = new RectangleF(left, 0, width, sliceHeight);
				
				using (Brush b = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Horizontal))
				{
					g.FillRectangle(b, r);
				}
			}
			
			if (FInvalidKeyFrames.Count > 0)
			{
				//System.Console.Beep();
				//draw left of first keyframe
				if (FInvalidKeyFrames[0] == FKeyFrames[0])
				{
					left = 0;
					width = FKeyFrames[0].GetTimeAsX();
					c1 = (FKeyFrames[0] as TLColorKeyFrame).RGBAColor.Color;
					r = new RectangleF(left, 0, width, sliceHeight);
					
					using (Brush b = new SolidBrush(c1))
					{
						g.FillRectangle(b, r);
					}
				}
				
				//draw right of last keyframe
				if (FInvalidKeyFrames[FInvalidKeyFrames.Count-1] == FKeyFrames[FKeyFrames.Count-1])
				{
					left = FKeyFrames[FKeyFrames.Count-1].GetTimeAsX();
					width = g.ClipBounds.Width -left;
					
					c1 = (FKeyFrames[FKeyFrames.Count-1] as TLColorKeyFrame).RGBAColor.Color;
					r = new RectangleF(left, 0, width, sliceHeight);
					
					using (Brush b = new SolidBrush(c1))
					{
						g.FillRectangle(b, r);
					}
				}
			}
			
			//draw infos to visible
			if (Collapsed)
				DrawCollapsedKeyFrames(g);
			else
			{
				foreach (TLColorKeyFrame k in FInvalidKeyFrames)
				{
					float x = k.GetTimeAsX();
					Color inv = VColor.Offset(k.RGBAColor, 0.5).Color;
					
					if (k.Selected)
					{
						using (Pen p = new Pen(inv))
							g.DrawRectangle(p, x-20, 0, 40, sliceHeight);
						
						//draw color values hsva
						string s = "H: " + (k.Hue).ToString("f2", TimelinerPlugin.GNumberFormat);
						using (Brush b = new SolidBrush(inv))
						{
							g.DrawString(s, FFont, b, x-20, 0);
							s = "S: " + k.Saturation.ToString("f2", TimelinerPlugin.GNumberFormat);
							g.DrawString(s, FFont, b, x-20, 10);
							s = "V: " + k.Value.ToString("f2", TimelinerPlugin.GNumberFormat);
							g.DrawString(s, FFont, b, x-20, 20);
							s = "A: " + (k.Alpha).ToString("f2", TimelinerPlugin.GNumberFormat);
							g.DrawString(s, FFont, b, x-20, 30);
						}
						
						using (Brush b = new SolidBrush(Color.White))
							g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat) + "s", FFont, b, x-20, 45);
					}
					else
						using (Pen p = new Pen(Color.Silver))
						g.DrawRectangle(p, x-20, 0, 40, sliceHeight);

					SizeF sz = g.MeasureString(OutputAsString, FFont);
					inv = inv = VColor.Offset(FOutput, 0.5).Color;
					
					using (Brush b = new SolidBrush(FOutput.Color))
						g.FillRectangle(b, new RectangleF(g.ClipBounds.Width-sz.Width, sliceHeight-sz.Height, sz.Width, sz.Height));
					using (Pen p = new Pen(Color.Silver))
						g.DrawLine(p, x, 0, x, 3);
					using (Brush b = new SolidBrush(inv))
						g.DrawString(OutputAsString, FFont, b, g.ClipBounds.Width-sz.Width-2, sliceHeight-sz.Height);
				}
			}			
		}
	}
}

