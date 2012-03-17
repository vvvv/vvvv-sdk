using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLIOBoxValue : UserControl
	{
		public event ValueChangeHandler OnValueChange;
		private double FValue, FMinimum, FMaximum;
		private bool FMouseDown = false;
		private Point FMousePoint;
		private Point FMouseDownPoint;
		private bool FCyclic = false;
		private bool FIsInteger = false;
		
		public double Value
		{
			get {return FValue;}
			set 
			{
				FValue = value;
				Invalidate();
			}
		}
		
		public double Minimum
		{
			get {return FMinimum;}
			set 
			{
				FMinimum = value;
				Invalidate();
			}
		}
		
		public double Maximum
		{
			get {return FMaximum;}
			set 
			{
				FMaximum = value;
				Invalidate();
			}
		}
		
		public bool Cyclic
		{
			get {return FCyclic;}
			set {FCyclic = value;}
		}
		
		public bool IsInteger
		{
			get {return FIsInteger;}
			set {FIsInteger = value;}
		}
		
		public TLIOBoxValue()
		{
			InitializeComponent();
		}
		
		private void ChangeValue(double NewValue)
		{
			FValue = Math.Min(Math.Max(NewValue, FMinimum), FMaximum);
			Invalidate();
			
			if (OnValueChange != null)
				OnValueChange(NewValue);
		}
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			
			if (e.Button == MouseButtons.Right)
			{
				FMouseDown = true;
				FMousePoint = Cursor.Position;
				FMouseDownPoint = Cursor.Position;
				Cursor.Hide();
			}
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
				
			if (FMouseDown)
			{
				int cX = Cursor.Position.X;
				int cY = Cursor.Position.Y;
				int mpY = FMousePoint.Y;

				if (cY == Screen.FromPoint(Cursor.Position).WorkingArea.Height-1)
				{
					Cursor.Position = new Point(cX, 0);
					mpY = Cursor.Position.Y-2;
				}
				else if (cY == 0)
				{
					Cursor.Position = new Point(cX, Screen.FromPoint(Cursor.Position).WorkingArea.Height-1);
					mpY = Cursor.Position.Y+2;
				}
				
				cY = Cursor.Position.Y;
				
				double delta = (cY - mpY) / -100.0;
				double faktor;
				if (FIsInteger)
					faktor = 100;
				else 
					faktor = 1;
				
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
					faktor /= 10;
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
					faktor /= 10;
				if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
					faktor = 1 / faktor;
				
				double newvalue = FValue + delta * faktor;
				ChangeValue(newvalue);
				FMousePoint = Cursor.Position;
			}
		}
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			
			if (FMouseDown)
			{
				FMouseDown = false;
				Cursor.Position = FMouseDownPoint;
				Cursor.Show();
			}
		}
		
		protected override void OnDoubleClick(System.EventArgs e)
		{
			base.OnDoubleClick(e);
			ShowValueBox();
		}
		
		public void ShowValueBox()
		{
			if (FIsInteger)
				ValueBox.Text = FValue.ToString("f0", TimelinerPlugin.GNumberFormat);
			else
				ValueBox.Text = FValue.ToString("f4", TimelinerPlugin.GNumberFormat);
			ValueBox.SelectAll();
			ValueBox.Show();
			ValueBox.Focus();
		}			
		
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			
			string text;
			if (FIsInteger)
				text = FValue.ToString("f0", TimelinerPlugin.GNumberFormat);
			else
				text = FValue.ToString("f4", TimelinerPlugin.GNumberFormat);
			Font f = new Font("Lucida Sans Unicode", 7);
			SizeF s = g.MeasureString(text, f);
			g.DrawString(text, f, new SolidBrush(Color.Black), Width - s.Width-4, 1);
		}
		
		void ValueBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) 13)
			{
				DoChangeValue();
				ValueBox.Hide();
			}
			this.Focus();
		}
		
		private void DoChangeValue()
		{
			try
			{
				double val = Convert.ToDouble(ValueBox.Text.Replace(',', '.'), TimelinerPlugin.GNumberFormat);
				ChangeValue(val);
			}
			catch (FormatException)
			{
				
			}
		}
		
		void ValueBoxLeave(object sender, EventArgs e)
		{
			if (ValueBox.Visible)
			{
				DoChangeValue();
				ValueBox.Hide();
			}		
		}
	}
	
	public delegate void ValueChangeHandler(double Value);
}
