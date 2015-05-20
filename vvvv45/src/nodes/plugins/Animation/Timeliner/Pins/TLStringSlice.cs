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
	public class TLStringSlice :  TLSlice
	{
		
		private IValueConfig FKeyTime, FKeyFlagY;
		private IStringConfig FKeyValue;
		
		private string FOutput;
		
		public string Output
		{
			get {return FOutput;}
		}
		
		public TLStringSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder): base(Host, Pin, SliceIndex, PinsOrder)
		{}
		
		protected override void CreatePins()
		{
			FHost.CreateValueConfig(FPin.Name + "-Time" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyTime);
			FKeyTime.SliceCount=0;
			FKeyTime.SetSubType(Double.MinValue, Double.MaxValue,0.001D,0,false, false,false);
			
			FHost.CreateValueConfig(FPin.Name + "-FlagY" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyFlagY);
			FKeyFlagY.SliceCount=0;
			FKeyFlagY.SetSubType(Double.MinValue, Double.MaxValue,0.001D,0,false, false,false);
			
			FHost.CreateStringConfig(FPin.Name + "-Value" + FSliceIndex.ToString(), TSliceMode.Dynamic, TPinVisibility.Hidden, out FKeyValue);
			FKeyValue.SliceCount=0;
			FKeyValue.SetSubType("default",false);
		}
		
		public override void DestroyPins()
		{
			FHost.DeletePin(FKeyTime);
			FHost.DeletePin(FKeyFlagY);
			FHost.DeletePin(FKeyValue);
			FKeyTime = null;
			FKeyValue = null;
			FKeyFlagY = null;
			
			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			FKeyTime.Name = FPin.Name + "-Time" + FSliceIndex.ToString();
			FKeyFlagY.Name = FPin.Name + "-FlagY" + FSliceIndex.ToString();
			FKeyValue.Name = FPin.Name + "-Value" + FSliceIndex.ToString();
			
			FKeyTime.Order = FPin.Order;
			FKeyFlagY.Order = FPin.Order;
			FKeyValue.Order = FPin.Order;
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			TLBaseKeyFrame kf = FKeyFrames.FindLast(delegate(TLBaseKeyFrame k) {return k.Time <= CurrentTime;});
			if (kf == null)
				FOutput = "";
			else
				FOutput = (kf as TLStringKeyFrame).Value;
			
			OutputAsString = FOutput;
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!
			if (Input == FKeyValue && FirstFrame)
			{
				FKeyFrames.Clear();

				double time, flag;
				string val;
				for (int i = 0; i<FKeyValue.SliceCount;i++)
				{
					FKeyTime.GetValue(i, out time);
					FKeyFlagY.GetValue(i, out flag);
					FKeyValue.GetString(i, out val);
					AddKeyFrame(time, (float) flag, val);
				}
				
				FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return k0.Time.CompareTo(k1.Time); });
			}
			
			base.Configurate(Input, FirstFrame);
		}
		
		private TLBaseKeyFrame AddKeyFrame(double Time, float FlagY, string Value)
		{
			//called via configpinchanged and gui-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			TLStringKeyFrame k = new TLStringKeyFrame(FPin.Transformer, Time, FlagY, Value, slicetop, sliceheight);
			FKeyFrames.Add(k);
			return k;
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			return AddKeyFrame(FPin.Transformer.XPosToTime(P.X), (float) VMath.Map(P.Y, slicetop, slicetop + sliceheight, 0, 1, TMapMode.Clamp), "(empty)");
		}
		
		public override void SaveKeyFrames()
		{
			FKeyTime.SliceCount = FKeyFrames.Count;
			FKeyFlagY.SliceCount = FKeyFrames.Count;
			FKeyValue.SliceCount = FKeyFrames.Count;
			
			FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return  k0.Time.CompareTo(k1.Time); });
			
			for (int i = 0; i<FKeyFrames.Count; i++)
			{
				FKeyTime.SetValue(i, FKeyFrames[i].Time);
				FKeyFlagY.SetValue(i, (FKeyFrames[i] as TLStringKeyFrame).PositionY);
				FKeyValue.SetString(i, (FKeyFrames[i] as TLStringKeyFrame).Value);
			}
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne, bool Collapsed)
		{
			base.DrawSlice(g, From, To, AllInOne, Collapsed);
			if (Collapsed)
				DrawCollapsedKeyFrames(g);
			else
			{
				Region SliceClip = g.Clip;
				foreach (TLStringKeyFrame k in FInvalidKeyFrames)
				{
					//transform the keyframes time by the current transformation
					float x = k.GetTimeAsX();
					float y = k.GetFlagPosAsY();

					if (k.Selected)
					{
						g.FillRectangle(new SolidBrush(Color.Silver), x, y, k.Width-1, 14);
						//don't clip so infos are always visible
						g.Clip = new Region();
						g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, new SolidBrush(Color.Black), x+2, y-14);
						g.Clip = SliceClip;
					}
					else
					{
						g.FillRectangle(new SolidBrush(Color.White), x, y, k.Width-1, 14);
					}
					
					g.DrawString(k.Value, FFont, new SolidBrush(Color.Black), x+2, y);
				}
				
				float sliceheight = FPin.Height / FPin.SliceCount;
				float sWidth = g.MeasureString(OutputAsString, FFont).Width + 2;
				g.DrawString(OutputAsString, FFont, new SolidBrush(Color.Gray), g.ClipBounds.Width-sWidth, sliceheight-16);
			}
		}
	}
}

