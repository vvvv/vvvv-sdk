using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLPin: TLBasePin
	{
		protected IPluginHost FHost;
		
		private bool FAllInOne = false;
		
		//default constructor for designer in inherited classes to work
		public TLPin():base(){InitializeComponent();}

		public TLPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings, bool ShowRemoveButton, bool ShowPinName):base(Transformer, Order, PinSettings)
		{
            float originalHeight = Height;

			InitializeComponent();
			
			FHost = Host;
			FUncollapsedHeight = Height;

            //compute dpifactor here instead of getting it from graphics.DPIY
            //because controls are set to AutoSizeMode font 
            //and therefore the actual scaling factor is different to the dpi-value
            FDPIFactor = Height / originalHeight;

            this.RemoveButton.Visible = ShowRemoveButton;
			this.PinNameEdit.Visible = ShowPinName;
			
			//initializecomponent overrides the name! reset it here
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("Name") as XmlAttribute;
        	this.Name = attr.Value;
        	PinNameEdit.Text = Name;
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("AllInOne") as XmlAttribute;
        		FAllInOne = bool.Parse(attr.Value);
        	}        	
        	catch
        	{}
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("Collapsed") as XmlAttribute;
        		FCollapsed = bool.Parse(attr.Value);
        	}
			catch
        	{}

			
			CreatePins();				
		}
		
		public virtual void UpdateSliceSpecificSettings()
		{
        	UpdateAllInOneState();    
        	UpdateCollapsedState();
		}
		
		protected override void InitializeHeight()
		{
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = DIP(80);
			
			base.InitializeHeight();
		}
		
		protected override XmlNode GetSettings()
		{
			XmlNode pin = base.GetSettings();
			
			XmlAttribute attr = FSettings.CreateAttribute("AllInOne");
			attr.Value = FAllInOne.ToString();
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Collapsed");
    		attr.Value = FCollapsed.ToString();
    		pin.Attributes.Append(attr);
    		
    		return pin;				
		}

		public override void PinOrderChanged()
		{
			foreach (TLSlice s in FOutputSlices)
   				s.PinsOrder = FOrder;
		}
		
		protected virtual void PinNameChanged()
		{
			Name = PinNameEdit.Text;
			PinChanged();
		}

		protected virtual void CreatePins()
		{
		}
		
		public override void DestroyPins()
		{
			// CALL SLICE DISPOSE METHODS
			///////////////////////
			for (int i=0;i<FOutputSlices.Count;i++)
			{
				FOutputSlices[i].DestroyPins();
			}
			
			// DELETE ALL SLICES
			//should already be empty
			FOutputSlices.Clear();
			FOutputSlices = null;
			
			FHost = null;
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			base.Configurate(Input, FirstFrame);
			
			for(int i=0; i<OutputSlices.Count; i++)
				FOutputSlices[i].Configurate(Input, FirstFrame);
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			for(int i=0; i<OutputSlices.Count; i++)
				FOutputSlices[i].Evaluate(CurrentTime);
		}
				
		public override void DrawSlices(Graphics g, double From, double To)
		{
			RectangleF r = g.ClipBounds;
			float y=0;
			//if (!FCollapsed)
			{
				Matrix CurrentTransform = g.Transform;
				
				if (AllInOne.Checked)
				{
					//y = Math.Max(r.Y, 0); 
					//g.Clip = new Region(new RectangleF(0, y, r.Width, r.Height));
					foreach (TLSlice s in FOutputSlices)
						s.DrawSlice(g, From, To, true, FCollapsed);
				}
				else
				{
					float sh = Height / FOutputSlices.Count;
					int i=0;
					foreach (TLSlice s in FOutputSlices)
					{
						y = Math.Max(r.Y - i*sh, 0); 
						g.Clip = new Region(new RectangleF(0, y, r.Width, Math.Max(0, Math.Min(r.Height - (i*sh), sh-y))));
						
						s.DrawSlice(g, From, To, false, FCollapsed);
						
						//line on top of slice
						if (i>0)
							g.DrawLine(new Pen(Color.LightGray, 2), 0, 1, r.Width, 1);
						i++;
						
						if (i*sh > r.Height)
							break;
						
						g.TranslateTransform(0, sh);
					}
				}
				
				g.Transform = CurrentTransform;
				g.Clip = new Region(r);
				
				//top and bottom line of pin
				g.DrawLine(new Pen(Color.Gray, 1), 0, 1, r.Width, 1);
				g.DrawLine(new Pen(Color.Gray, 1), 0, r.Top + r.Height-0.5f, r.Width, r.Top + r.Height-0.5f);
			}
			/*else
			{
				y = Math.Max(r.Y - Height, 0); 
				g.Clip = new Region(new RectangleF(0, y, r.Width, Height));
				g.FillRectangle(new SolidBrush(Color.Silver), r);
			}*/
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			
			UpdateKeyFrameAreas();
		}
		
		protected override void OnLocationChanged(EventArgs e)
		{
			UpdateKeyFrameAreas();
			
			base.OnLocationChanged(e);
		}
		
		public override void UpdateKeyFrameAreas()
		{
			if (FOutputSlices == null)//needed for mono 1.91 (as OnLocationChanged is still called after pin is deleted?!)  
				return;
			
			if (FOutputSlices.Count == 0) 
				return;
			
			if (AllInOne.Checked)
			{
				foreach (TLSlice s in FOutputSlices)
					foreach (TLBaseKeyFrame k in s.KeyFrames)
					{
						k.SliceTop = Top;
						k.SliceHeight = Height;
					}
			}
			else
			{
				float sliceheight = Height / FOutputSlices.Count;
				int i = 0;
				
				foreach (TLSlice s in FOutputSlices)
				{
					foreach (TLBaseKeyFrame k in s.KeyFrames)
					{
						k.SliceTop = Top + sliceheight * i;
						k.SliceHeight = sliceheight;
					}
					i++;
				}
			}
		}
		
		public TLBaseKeyFrame AddKeyFrame(Point P)
		{
			TLBaseKeyFrame k = null;
			if (AllInOne.Checked) //create a keyframe for every slice
			{
				foreach (TLSlice s in FOutputSlices)
				{
					k = s.AddKeyFrame(P);
					s.SaveKeyFrames();
				}
			}
			else	//create a keyframe in the correct slice
			{
				int y = P.Y - Top;
				float sliceheight = Height / FOutputSlices.Count;
				int sliceidx = (int) (y / sliceheight);
				
				k = FOutputSlices[sliceidx].AddKeyFrame(P);				
				FOutputSlices[sliceidx].SaveKeyFrames();				
			}
			
			return k;
		}
		
		void RemoveButtonClick(object sender, EventArgs e)
		{
			RemovePin();
		}

		void PinNameEditKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				PinNameChanged();
			}
		}

		void CollapseButtonClick(object sender, EventArgs e)
		{
			FCollapsed = !FCollapsed;
			
			UserChangedCollapsedState();
		}
		
		protected override void UpdateCollapsedState()
		{
			if(FCollapsed)
			{
				FUncollapsedHeight = Height;
				Height =  FOutputSlices.Count * DIP(20);
				CollapseButton.Text = ">";				
			}
			else
			{
				Height = FUncollapsedHeight;
				CollapseButton.Text = "V";
			}
		}
		
		void AllInOneClick(object sender, EventArgs e)
		{
			//generate a compound list of all slices keyframes if AllInOne is checked?!
			FAllInOne = !FAllInOne;
			
			UpdateAllInOneState();
			
			PinChanged();
			UpdateView();
		}
		
		void UpdateAllInOneState()
		{
			AllInOne.Checked = FAllInOne;
			UpdateKeyFrameAreas();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);			
		}
		
		void PinNameEditLeave(object sender, EventArgs e)
		{
			PinNameChanged();
		}
		
		void TopPanelMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				DoDragDrop(new DataObject(DataFormats.Serializable, this), DragDropEffects.All);
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			const int WM_KEYDOWN = 0x100;
    
    		bool handled = false;
    		
			if (m.Msg == WM_KEYDOWN)
			{
				KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
				
				if (ke.KeyCode == Keys.Space)
					handled = true;
				if (ke.KeyCode == Keys.Back)
					handled = true;
			}
				
			return handled;
		}
		
		void CollapseButtonMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				return;
		}
	}
}
