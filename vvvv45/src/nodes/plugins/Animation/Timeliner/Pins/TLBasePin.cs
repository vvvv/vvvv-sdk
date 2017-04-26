using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public enum TLPinType {Automata, Ruler, Value, String, Color, Midi, Wave};
	
	//common base to TLPin and TLRulerPin
	public class TLBasePin : UserControl
	{
		// Track whether Dispose has been called.
   		private bool FDisposed = false;
		
		private bool FMouseDown = false;
		protected int FMinimalHeight = 40;
		protected XmlDocument FSettings;
		
		public event PinHandler OnPinChanged;
		public event PinHandler OnRemovePin;
		public event PinHandler OnRedraw;
		
		public new int Top
		{
			get 
			{
				if (Parent != null) 
					return base.Top + Parent.Parent.Top;
				else 
					return base.Top;
			}
		}

        protected int DIP(int pixel)
        {
            return (int)Math.Round(pixel * FDPIFactor);
        }
        protected float FDPIFactor = 1;

        //default constructor for designer in inherited classes to work
        public TLBasePin():base(){}
		
		public TLBasePin(TLTransformer Transformer, int Order, XmlNode PinSettings):base()
		{
			FSettings = new XmlDocument();

            FTransformer = Transformer;
			FOrder = Order;			
			
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("Name") as XmlAttribute;
        	this.Name = attr.Value;
			
        	try
        	{
        		//keep this order: type, height, ...
        		attr = PinSettings.Attributes.GetNamedItem("Type") as XmlAttribute;
        		FPinType = (TLPinType) Enum.Parse(typeof(TLPinType), attr.Value);
        	
        		attr = PinSettings.Attributes.GetNamedItem("Height") as XmlAttribute;
        		FUncollapsedHeight = Math.Max(FMinimalHeight, int.Parse(attr.Value));
        	}
        	catch
        	{
        		FUncollapsedHeight = 0;
        	}
        	InitializeHeight();
		}
		
		protected override void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			// Dispose managed resources.
        			FSettings = null;

        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
				//..
        	}
        	FDisposed = true;
        }
		
		protected virtual void InitializeHeight()
		{
			this.Height = Math.Max(FMinimalHeight, FUncollapsedHeight);
		}
		
		protected TLPinType FPinType;
		public TLPinType PinType
		{
			get { return FPinType;}
		}
		
		protected int FOrder;
		public int Order 
		{
   			get { return FOrder;}
   			set 
   			{ 
   				FOrder = value;
   				PinOrderChanged();
   			}
		}
		
		protected TLTransformer FTransformer;
		public TLTransformer Transformer
		{
			get{return FTransformer;}
		}
		
		protected bool FCollapsed = false;
		public bool Collapsed
		{
   			get { return FCollapsed;}
   			set 
   			{
   				FCollapsed = value;
   				UserChangedCollapsedState();   				
   			}
		}
		
		protected int FUncollapsedHeight;
		public int UncollapsedHeight
		{
			get { return FUncollapsedHeight;}
		}

        public int SliceCount
		{
			get{return FOutputSlices.Count;}
		}
		
		protected List<TLSlice> FOutputSlices = new List<TLSlice>();
  		public List<TLSlice> OutputSlices 
		{
   			get { return FOutputSlices;}
		}

  		protected void UserChangedCollapsedState()
		{
			UpdateCollapsedState();
			
			PinChanged();
			UpdateView();
		}
		
		protected virtual void UpdateCollapsedState()
		{
			
		}
  		
		protected void PinChanged()
		{
			if (OnPinChanged != null)
				OnPinChanged(this);
			
			UpdateKeyFrameAreas();	
			UpdateSliceIndices();
		}

		protected void UpdateView()
		{
			if (OnRedraw != null)
				OnRedraw(this);
		}
		
		public virtual void AddSlice(int InsertAt)
		{}
		
		protected void AddSlice(int At, TLSlice Slice)
		{
			FOutputSlices.Insert(At, Slice);
			PinChanged();
		}
		
		public virtual void RemoveSlice(TLSlice Slice)
		{
			if (MessageBox.Show("You sure?", "Deleting Slice of Pin: " + Name, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Slice.DestroyPins();
				FOutputSlices.Remove(Slice);
				PinChanged();
			}
		}
		
		protected void RemovePin()
		{
			if (OnRemovePin != null)
				OnRemovePin(this);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Y > Height - 10)
				FMouseDown = true;
			else
			{
				DragDropEffects dde = DoDragDrop(new DataObject(DataFormats.Serializable, this), DragDropEffects.All);
				if (dde == DragDropEffects.All)
					PinChanged();
			}
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (FMouseDown)
			{
				FUncollapsedHeight = Math.Max(e.Y, FMinimalHeight);
				Height = FUncollapsedHeight;
				PinChanged();
			}
			
			if (e.Y > Height - 10)
				this.Cursor = Cursors.HSplit;
			else
				this.Cursor = null;
		}
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
			FMouseDown = false;
		}
		
		public void SaveKeyFrames()
		{
			foreach (TLSlice s in OutputSlices)
				s.SaveKeyFrames();
		}
		
		public virtual void UpdateSliceIndices()
		{
			for (int i=0; i<FOutputSlices.Count; i++)
				FOutputSlices[i].SliceIndex = i;
		}
		
		protected virtual XmlNode GetSettings()
		{
			XmlNode pin = FSettings.CreateElement("PIN");
			XmlAttribute attr = FSettings.CreateAttribute("Name");
    		attr.Value = Name;
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Type");
    		attr.Value = PinType.ToString();
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("SliceCount");
    		attr.Value = SliceCount.ToString();
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Height");
            attr.Value = ((int) Math.Round(UncollapsedHeight / FDPIFactor)).ToString();
            pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Parent");
    		attr.Value = Parent.Tag.ToString();
    		pin.Attributes.Append(attr);
    		
    		return pin;
		}
	
		public XmlNode Settings
		{
			get {return GetSettings();}
		}
		
		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);

			UpdateView();
		}
		
		protected virtual void LoadSlices(int SavedSliceCount)
		{}
		
		public virtual void Configurate(IPluginConfig Input, bool FirstFrame)
		{}
		
		public virtual void Evaluate(double CurrentTime)
		{}
		
		public virtual void UpdateKeyFrameAreas()
		{}
		
		public virtual void PinOrderChanged()
		{}
		
		public virtual void DestroyPins()
		{}
		
		public virtual void DrawSlices(Graphics g, double From, double To)
		{}
		
		public virtual void DeleteSelectedKeyFrames()
		{}
	}
	
	public delegate void PinHandler(TLBasePin Pin);
	public delegate void MovePinHandler(TLBasePin Pin, bool Up);
}
