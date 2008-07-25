using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLValuePin: TLPin
	{
		private IValueOut FValueOut;
		private RadioButton StepRadio, LinearRadio, CubicRadio;
		private TLIOBoxValue FMinIO;
		private TLIOBoxValue FMaxIO;
		private Label FMinLabel, FMaxLabel;
		
		private TLInterpolationType FInterpolationIN;
		
		public TLValuePin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
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
				FMinIO.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
        	}
        	catch
        	{
        		FMinIO.Value = -1;
        	}
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("Maximum") as XmlAttribute;
				FMaxIO.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
        	}
        	catch
        	{
        		FMaxIO.Value = 1;
        	}
        	
        	UpdateSliceSpecificSettings();
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
	    	
	    	StepRadio = new System.Windows.Forms.RadioButton();
	    	LinearRadio = new System.Windows.Forms.RadioButton();
	    	CubicRadio = new System.Windows.Forms.RadioButton();
	    	
	    	FMinLabel = new Label();
	    	FMinLabel.ForeColor = Color.Black;
	    	FMinLabel.Text = "Minimum";
	    	FMinLabel.AutoSize = true;
	    	FMinLabel.Location = new Point(70, 70);
	    	
	    	FMaxLabel = new Label();
	    	FMaxLabel.ForeColor = Color.Black;
	    	FMaxLabel.Text = "Maximum";
	    	FMaxLabel.Location = new Point(70, 40);
	    	
	    	FMinIO = new TLIOBoxValue();
	    	FMinIO.Location = new Point(70, 85);
	    	FMinIO.Size = new Size(60, 15);	    	
	    	FMaxIO = new TLIOBoxValue();
	    	FMaxIO.Location = new Point(70, 55);
	    	FMaxIO.Size = new Size(60, 15);	    	
			
	    	FMinIO.Minimum = FMaxIO.Minimum = double.MinValue;
	    	FMinIO.Maximum = FMaxIO.Maximum = double.MaxValue;
	    	
	    	UpdateMinMax();
	    	
			base.Controls.Add(StepRadio);
			base.Controls.Add(LinearRadio);
			base.Controls.Add(CubicRadio);
			base.Controls.Add(FMinIO);
			base.Controls.Add(FMaxIO);
			base.Controls.Add(FMinLabel);
			base.Controls.Add(FMaxLabel);			
			
			FMinIO.OnValueChange += new ValueChangeHandler(MinMaxIOChangedCB);
			FMaxIO.OnValueChange += new ValueChangeHandler(MinMaxIOChangedCB);
			
			// 
        	// StepRadio
        	// 
        	this.StepRadio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.StepRadio.Location = new System.Drawing.Point(4, 40);
        	this.StepRadio.Name = "StepRadio";
        	this.StepRadio.Size = new System.Drawing.Size(46, 26);
        	this.StepRadio.TabIndex = 9;
        	this.StepRadio.Text = "Step";
        	this.StepRadio.UseVisualStyleBackColor = true;
        	this.StepRadio.MouseDown += new System.Windows.Forms.MouseEventHandler(RadioButtonClick);
        	// 
        	// LinearRadio
        	// 
        	this.LinearRadio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.LinearRadio.Location = new System.Drawing.Point(4, 60);
        	this.LinearRadio.Name = "LinearRadio";
        	this.LinearRadio.Size = new System.Drawing.Size(53, 26);
        	this.LinearRadio.Text = "Linear";
        	this.LinearRadio.UseVisualStyleBackColor = true;
        	this.LinearRadio.MouseDown += new System.Windows.Forms.MouseEventHandler(RadioButtonClick);
        	// 
        	// CubicRadio
        	// 
        	this.CubicRadio.Checked = true;
        	this.CubicRadio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.CubicRadio.Location = new System.Drawing.Point(4, 80);
        	this.CubicRadio.Name = "CubicRadio";
        	this.CubicRadio.Size = new System.Drawing.Size(51, 26);
        	this.CubicRadio.Text = "Cubic";
        	this.CubicRadio.UseVisualStyleBackColor = true;
        	this.CubicRadio.MouseDown += new System.Windows.Forms.MouseEventHandler(RadioButtonClick);
		}
		
		protected override void InitializeHeight()
		{
			FMinimalHeight = 110;
			
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = 150;
			
			base.InitializeHeight();
		}
		
		protected override XmlNode GetSettings()
		{
			XmlNode pin = base.GetSettings();
			
			XmlAttribute attr = FSettings.CreateAttribute("InterpolationIn");
			attr.Value = FInterpolationIN.ToString();
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Minimum");
    		string tmp = String.Format("{0:F4}", FMinIO.Value);
			attr.Value = tmp.Replace(',', '.');
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Maximum");
			tmp = String.Format("{0:F4}", FMaxIO.Value);
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
			ss.MinValue = FMinIO.Value;
			ss.MaxValue = FMaxIO.Value;
			
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
			
			UpdateView();
			PinChanged();
		}
		
		private void UpdateInterpolationInState()
		{
			StepRadio.Checked = FInterpolationIN == TLInterpolationType.Step;
			LinearRadio.Checked = FInterpolationIN == TLInterpolationType.Linear;
			CubicRadio.Checked = FInterpolationIN == TLInterpolationType.Cubic;
			
			foreach (TLValueSlice s in FOutputSlices)
			{
				s.SetKeyFrameTypes(FInterpolationIN, FInterpolationIN);
			}
		}
		
		private void UpdateMinMax()
		{
			foreach (TLValueSlice vs in FOutputSlices)
			{
				vs.MinValue = FMinIO.Value;
				vs.MaxValue = FMaxIO.Value;
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
