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
		protected string FOutputAsString;
		
		public int PinsOrder
		{
			set 
			{
				FPinsOrder = value;
				SliceOrPinOrderChanged();
			}
		}
		
		public string OutputAsString
		{
			get {return FOutputAsString;}
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

		public virtual void DrawSlice(Graphics g, double From, double To, bool AllInOne)
		{
			UpdateInvalidKeyFrames(From, To);
		}
		
		public virtual void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			if (FirstFrame)
				(FPin as TLPin).UpdateSliceSpecificSettings();
		}
		
		protected abstract void CreatePins();
		public abstract void DestroyPins();
		
		protected abstract void SliceOrPinOrderChanged();

		public abstract TLBaseKeyFrame AddKeyFrame(Point P);
		public abstract void SaveKeyFrames();
		
		
		
		public abstract void Evaluate(double CurrentTime);
	}
}
