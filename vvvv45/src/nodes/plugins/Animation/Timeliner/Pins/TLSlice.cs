using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public abstract class TLSlice
	{
		protected TLBasePin FPin;
		protected Font FFont = new Font("Lucida Sans Unicode", 7);
		protected IPluginHost FHost;
		
		protected List<TLBaseKeyFrame> FKeyFrames = new List<TLBaseKeyFrame>();
		protected List<TLBaseKeyFrame> FInvalidKeyFrames;

		protected int FPinsOrder;
		protected int FSliceIndex;
		
		public int PinsOrder
		{
			set
			{
				FPinsOrder = value;
				SliceOrPinOrderChanged();
			}
		}

		private bool FOutputChanged;
		public bool OutputChanged
		{
			get{return FOutputChanged;}
		}
		
		private string FOutputAsString;
		public string OutputAsString
		{
			get {return FOutputAsString;}
			set
			{
				if (FOutputAsString != value)
				{
					FOutputAsString = value;
					FOutputChanged = true;
				}
			}
		}
		
		public int SliceIndex
		{
			get {return FSliceIndex;}
			set
			{
				FSliceIndex = value;
				SliceOrPinOrderChanged();
			}
		}
		
		public List<TLBaseKeyFrame> KeyFrames
		{
			get {return FKeyFrames;}
		}
		
		public TLSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder)
		{
			FHost = Host;
			FPin = Pin;
			FSliceIndex = SliceIndex;
			FPinsOrder = PinsOrder;
			
			CreatePins();
		}
		
		private void UpdateInvalidKeyFrames(double From, double To)
		{
			//find all kfs within the clipping region
			FInvalidKeyFrames = FKeyFrames.FindAll(delegate (TLBaseKeyFrame k) {return (k.Time >= From) && (k.Time <= To);});
			
			//+ find the first ones left and right out of the region
			TLBaseKeyFrame kf = null;
			kf = FKeyFrames.Find(delegate (TLBaseKeyFrame k) {return (k.Time > To);});
			if (kf != null)
				FInvalidKeyFrames.Add(kf);
			kf = FKeyFrames.FindLast(delegate (TLBaseKeyFrame k) {return (k.Time < From);});
			if (kf != null)
				FInvalidKeyFrames.Add(kf);
			
			FInvalidKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return k0.Time.CompareTo(k1.Time); });
		}

		public virtual void DrawSlice(Graphics g, double From, double To, bool AllInOne, bool Collapsed)
		{
			UpdateInvalidKeyFrames(From, To);
		}
		
		protected void DrawCollapsedKeyFrames(Graphics g)
		{
			SolidBrush silver = new SolidBrush(Color.Silver);
			SolidBrush white = new SolidBrush(Color.White);
			SolidBrush black = new SolidBrush(Color.Black);
			SolidBrush gray = new SolidBrush(Color.Gray);
			
			try
			{
				foreach (TLBaseKeyFrame k in FInvalidKeyFrames)
				{
					float sliceWidth = g.ClipBounds.Width;
					float width = 5;
					float sliceHeight = FPin.Height / FPin.SliceCount;
					float x = k.GetTimeAsX() - width/2;
					float y = 0;
					
					if (k.Selected)
					{
						g.FillRectangle(silver, x, y, width, sliceHeight);
						g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, black, x+5, y + 4);
					}
					else
					{
						g.FillRectangle(white, x, y, width, sliceHeight);
					}
					
					float sWidth = g.MeasureString(OutputAsString, FFont).Width + 2;
					g.DrawString(OutputAsString, FFont, gray, sliceWidth-sWidth, sliceHeight-16);
				}
			}
			finally
			{
				silver.Dispose();
				white.Dispose();
				black.Dispose();
				gray.Dispose();
			}
		}
		
		public virtual void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if (FirstFrame)
			//	(FPin as TLPin).UpdateSliceSpecificSettings();
		}
		
		protected abstract void CreatePins();
		public abstract void DestroyPins();
		
		protected abstract void SliceOrPinOrderChanged();

		public virtual TLBaseKeyFrame AddKeyFrame(Point P)
		{return null;}
		
		public virtual void SaveKeyFrames()
		{}
		
		public virtual void Evaluate(double CurrentTime)
		{
			FOutputChanged = false;
		}
	}
}
