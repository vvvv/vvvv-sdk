using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLStringPin: TLPin
	{
		private IStringOut FStringOut;
		
		public TLStringPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
			//only load slices after all super constructors have been executed:
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("SliceCount") as XmlAttribute;
        	LoadSlices(int.Parse(attr.Value));
		}

		protected override void CreatePins()
		{
			base.CreatePins();
			
	    	FHost.CreateStringOutput(Name, TSliceMode.Dynamic, TPinVisibility.True, out FStringOut);
        	FStringOut.SetSubType("default",false);
	    	FStringOut.Order=Order;
		}
		
		public override void DestroyPins()
		{
			// DELETE ALL PINS
			///////////////////////
			
			FHost.DeletePin(FStringOut);
			FStringOut = null;
			
			base.DestroyPins();
		}
		
		protected override void LoadSlices(int SavedSliceCount)
		{
			for(int i=0; i<SavedSliceCount; i++)
				AddSlice(FOutputSlices.Count);
			
			FStringOut.SliceCount = SavedSliceCount;
		}

		protected override void PinNameChanged()
		{
			base.PinNameChanged();
			FStringOut.Name = Name;
		}
		
		public override void PinOrderChanged()
		{
			base.PinOrderChanged();
			FStringOut.Order = FOrder;
		}
		
		public override void RemoveSlice(TLSlice Slice)
		{
			base.RemoveSlice(Slice);
			FStringOut.SliceCount = FOutputSlices.Count;
		}
		
		public override void AddSlice(int At)
		{
			TLStringSlice ss = new TLStringSlice(FHost, this, FOutputSlices.Count, FOrder);

			AddSlice(At, ss);
			
			FStringOut.SliceCount = FOutputSlices.Count;
		}

		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			for (int i=0; i<FOutputSlices.Count; i++)
				FStringOut.SetString(i, (FOutputSlices[i] as TLStringSlice).Output);
				
		}
	}
}
