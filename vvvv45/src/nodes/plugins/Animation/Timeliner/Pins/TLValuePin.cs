using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLValuePin: TLPin
	{
		private IValueOut FValueOut;

		private TLInterpolationType FInterpolationIN;
		
		public TLValuePin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
			InitializeComponent();
			
			//only load slices after all super constructors have been executed:
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("SliceCount") as XmlAttribute;
        	LoadSlices(int.Parse(attr.Value));
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("InterpolationIn") as XmlAttribute;
				FInterpolationIN = (TLInterpolationType) Enum.Parse(typeof(TLInterpolationType), attr.Value);
        	}
        	catch
        	{
        		FInterpolationIN = TLInterpolationType.Cubic;
        	}
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("Minimum") as XmlAttribute;
				MinIO.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
        	}
        	catch
        	{
        		MinIO.Value = -1;
        	}
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("Maximum") as XmlAttribute;
				MaxIO.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
        	}
        	catch
        	{
        		MaxIO.Value = 1;
        	}
        	
        	MinIO.Minimum = MaxIO.Minimum = double.MinValue;
	    	MinIO.Maximum = MaxIO.Maximum = double.MaxValue;
		}

		public override void UpdateSliceSpecificSettings()
		{
			base.UpdateSliceSpecificSettings();
			UpdateMinMax();	
        	UpdateInterpolationInState();
		}
		
		protected override void CreatePins()
		{
			base.CreatePins();
			
	    	FHost.CreateValueOutput(Name, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
        	FValueOut.Order=Order;
        	FValueOut.SetSubType(double.MinValue, double.MaxValue, 0.0001D, 0, false, false, false);
		}
		
		protected override void InitializeHeight()
		{
			FMinimalHeight = DIP(110);
			
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = DIP(150);
			
			base.InitializeHeight();
		}
		
		protected override XmlNode GetSettings()
		{
			XmlNode pin = base.GetSettings();
			
			XmlAttribute attr = FSettings.CreateAttribute("InterpolationIn");
			attr.Value = FInterpolationIN.ToString();
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Minimum");
    		string tmp = String.Format("{0:F4}", MinIO.Value);
			attr.Value = tmp.Replace(',', '.');
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Maximum");
			tmp = String.Format("{0:F4}", MaxIO.Value);
			attr.Value = tmp.Replace(',', '.');
    		pin.Attributes.Append(attr);
    		
    		return pin;				
		}
		
		public override void DestroyPins()
		{
			// DELETE ALL PINS
			///////////////////////
			
			FHost.DeletePin(FValueOut);
			FValueOut = null;
			
			base.DestroyPins();
		}
		
		protected override void PinNameChanged()
		{
			base.PinNameChanged();
			FValueOut.Name = Name;
		}
		
		public override void PinOrderChanged()
		{
			base.PinOrderChanged();
			FValueOut.Order = FOrder;
		}
		
		protected override void LoadSlices(int SavedSliceCount)
		{
			for(int i=0; i<SavedSliceCount; i++)
				AddSlice(FOutputSlices.Count);
			
			FValueOut.SliceCount = SavedSliceCount;
		}
		
		public override void RemoveSlice(TLSlice Slice)
		{
			base.RemoveSlice(Slice);
			FValueOut.SliceCount = FOutputSlices.Count;
		}
		
		public override void AddSlice(int At)
		{
			TLValueSlice ss = new TLValueSlice(FHost, this, FOutputSlices.Count, FOrder);
			ss.MinValue = MinIO.Value;
			ss.MaxValue = MaxIO.Value;
			
			AddSlice(At, ss);
			
			FValueOut.SliceCount = FOutputSlices.Count;
		}
		
		private void MinMaxIOChangedCB(double NewValue)
		{
			UpdateMinMax();
			PinChanged();
		}
		
		private void RadioButtonClick(object sender, EventArgs e)
		{
			if (sender == StepRadio)
				FInterpolationIN = TLInterpolationType.Step;
			else if (sender == LinearRadio)
				FInterpolationIN = TLInterpolationType.Linear;
			else if (sender == CubicRadio)
				FInterpolationIN = TLInterpolationType.Cubic;
	
			UpdateInterpolationInState();
			SaveKeyFrames();
			
			UpdateView();
			PinChanged();
		}
		
		private void UpdateInterpolationInState()
		{
			StepRadio.Checked = FInterpolationIN == TLInterpolationType.Step;
			LinearRadio.Checked = FInterpolationIN == TLInterpolationType.Linear;
			CubicRadio.Checked = FInterpolationIN == TLInterpolationType.Cubic;
			
			foreach (TLValueSlice s in FOutputSlices)
				s.SetKeyFrameTypes(FInterpolationIN, FInterpolationIN);
		}
		
		private void UpdateMinMax()
		{
			foreach (TLValueSlice vs in FOutputSlices)
			{
				vs.MinValue = MinIO.Value;
				vs.MaxValue = MaxIO.Value;
			}			
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			for (int i=0; i<FOutputSlices.Count; i++)
				FValueOut.SetValue(i, (FOutputSlices[i] as TLValueSlice).Output);	
		}
	}
}
