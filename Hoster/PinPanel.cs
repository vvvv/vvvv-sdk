using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
	
using VVVV.PluginInterfaces.V1;

namespace Hoster
{
	/// <summary>
	/// Description of PinPanel.
	/// </summary>
	public partial class PinPanel : UserControl
	{
		const int FPinWidth = 70;
		const int FSliceHeight = 15;
		private List<TBasePin> FPinList;
		private int FTargetPin;
		private int FTargetSlice;
		private Point FMousePoint;
		private bool FMouseDown;
		private int FDelta;
		private List<TBasePin> FVisiblePins;
		
		private OpenFileDialog FOpenDialog = new OpenFileDialog();
		
		
		public List<TBasePin> PinList 
		{
			set {FPinList = value;}
		}
		
		public int TargetPin
		{
			set {FTargetPin = value;}
		}
		
		public int TargetSlice
		{
			set {FTargetSlice = value;}		
		}		
		
		public PinPanel()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		public void UpdateVisiblePinList()
		{
			FVisiblePins = FPinList.FindAll(delegate(TBasePin p) {return (p as TBasePin).Visibility != TPinVisibility.False;});
		}

		void PinPanelPaint(object sender, PaintEventArgs e)
		{
			if (FVisiblePins == null)
				return;
			
			FVisiblePins.Sort(delegate(TBasePin p1, TBasePin p2) {return (p1 as TBasePin).Order.CompareTo((p2 as TBasePin).Order);});
			
			System.Drawing.Graphics g = e.Graphics;
			
			//mark current slice
			Brush bgray = new System.Drawing.SolidBrush(Color.Gray);
			Rectangle r = new Rectangle(0, 0, 0, 0);
			
			r.Location = new Point(FTargetPin * FPinWidth, (FTargetSlice+1) * FSliceHeight);
			r.Width = FPinWidth;
			r.Height = FSliceHeight+1;
			g.FillRectangle(bgray, r);
			
			//draw inputs
			Font f = new Font("Lucida Sans", 8);
			Brush bblack = new System.Drawing.SolidBrush(Color.Black);
			Pen p = new Pen(Color.Black);
			
			int idx=0;
			foreach (TBasePin pin in FVisiblePins)
			{
				r.X = idx*FPinWidth;
				r.Y = 0;
				r.Width = FPinWidth;
				r.Height = this.ClientRectangle.Height-1;
				
				g.ResetClip();
				g.DrawRectangle(p, r);
				
				g.SetClip(r);				
				pin.Draw(g, f, bblack, p, r);
				idx++;
			}
		}
		
		void PinPanelMouseDown(object sender, MouseEventArgs e)
		{
			FMouseDown = true;
			FMousePoint = e.Location;
		}
		
		void PinPanelMouseMove(object sender, MouseEventArgs e)
		{
			if (!FMouseDown)
			{
				int pin = (int) (e.X / FPinWidth);
				if (pin < FVisiblePins.Count)
					FTargetPin = pin;
				else
					FTargetPin = -1;
				
				int slice = (int) (e.Y / FSliceHeight) - 1;
				if ((FTargetPin >= 0) && (slice >= 0))
				{
					if (slice < (FVisiblePins[FTargetPin] as TBasePin).SliceCount * (FVisiblePins[FTargetPin] as TBasePin).Dimension)
						FTargetSlice = slice;
					else
						FTargetSlice = -1;
				}	
				else
					FTargetSlice = -1;
			}
			else
			{
				FDelta = (e.Y - FMousePoint.Y);

				FMousePoint = e.Location;
			}
		}
		
		void PinPanelMouseUp(object sender, MouseEventArgs e)
		{
			FMouseDown = false;
			
			if ((e.Button == MouseButtons.Right) && (FVisiblePins[FTargetPin] is TStringPin))
			{
				FOpenDialog.ShowDialog();
				(FVisiblePins[FTargetPin] as TStringPin).SetString(FTargetSlice, FOpenDialog.FileName);
			}
		}
		
		public void SetInputs()
		{
        	//set inputs
        	if ((FTargetSlice > -1) && (FMouseDown))
        	{
        		if (FVisiblePins[FTargetPin] is TValuePin)
        		{
        			switch ((FVisiblePins[FTargetPin] as TValuePin).Dimension)
			        {
        				case 1: (FVisiblePins[FTargetPin] as TValuePin).SetDeltaValue(FTargetSlice, FDelta); break;
						case 2: 
        				{
        					if (FTargetSlice % 2 == 0)
	        					(FVisiblePins[FTargetPin] as TValuePin).SetDeltaValue2D(FTargetSlice, FDelta, 0); 
        					else
        						(FVisiblePins[FTargetPin] as TValuePin).SetDeltaValue2D(FTargetSlice-1, 0, FDelta); 
        					break;
        				}
			        }
        			FDelta = 0;
        			
        		} 
        		else if (FVisiblePins[FTargetPin] is TStringPin)
        		{
        			
        		}
        		
        		Invalidate();
        	}    
		}
	}
}
