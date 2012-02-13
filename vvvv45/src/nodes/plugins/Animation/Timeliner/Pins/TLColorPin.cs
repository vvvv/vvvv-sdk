using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.Nodes.Timeliner
{
	public class TLColorPin: TLPin
	{
		private IColorOut FColorOut;
		
		public TLColorPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
			//only load slices after all super constructors have been executed:
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("SliceCount") as XmlAttribute;
        	LoadSlices(int.Parse(attr.Value));
		}

		protected override void CreatePins()
		{
			base.CreatePins();
			
			FHost.CreateColorOutput(Name, TSliceMode.Dynamic, TPinVisibility.True, out FColorOut);
        	FColorOut.SetSubType(VColor.Black, true);
	    	FColorOut.Order=Order;
			FColorOut.SliceCount = 0;
		}
		
		public override void DestroyPins()
		{
			// DELETE ALL PINS
			///////////////////////
			//FHost.DeletePin(FArm);

			FHost.DeletePin(FColorOut);
			FColorOut = null;
			
			base.DestroyPins();
		}
		
		protected override void LoadSlices(int SavedSliceCount)
		{
			for(int i=0; i<SavedSliceCount; i++)
				AddSlice(FOutputSlices.Count);
			
			FColorOut.SliceCount = SavedSliceCount;
		}
		
		protected override void PinNameChanged()
		{
			base.PinNameChanged();
			FColorOut.Name = Name;
		}
		
		public override void PinOrderChanged()
		{
			base.PinOrderChanged();
			FColorOut.Order = FOrder;
		}
		
		public override void RemoveSlice(TLSlice Slice)
		{
			base.RemoveSlice(Slice);
			FColorOut.SliceCount = FOutputSlices.Count;
		}
		
		public override void AddSlice(int At)
		{
			TLColorSlice ss = new TLColorSlice(FHost, this, FOutputSlices.Count, FOrder);

			AddSlice(At, ss);
			
			FColorOut.SliceCount = FOutputSlices.Count;
		}

		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			for (int i=0; i<FOutputSlices.Count; i++)
			{
				FColorOut.SetColor(i, (FOutputSlices[i] as TLColorSlice).Output);
			}				
		}
	}
}
