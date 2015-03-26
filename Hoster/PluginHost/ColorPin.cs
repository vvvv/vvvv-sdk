using System;
using System.Collections;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace Hoster
{
	public class TColorPin: TBasePin, IColorIn, IColorConfig, IColorOut
	{
		public RGBAColor[] FValues;
		private RGBAColor FDefault;
		
		public TColorPin(IPluginHost Parent, string PinName, TPinDirection PinDirection, TOnConfigurate Callback, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, PinDirection, Callback, SliceMode, Visibility)
		{}

		
		public void GetColor(int Index, out RGBAColor Value)
		{
			Value = FValues[Index % FSliceCount];
		}
		
		public void SetColor(int Index, RGBAColor Value)
		{
			//if (Value != FValues[Index])
			{
				FValues[Index] = Value;
				FPinIsChanged = true;
			}		
			
			if ((FPinIsChanged) && (FOnConfigurate != null))
				FOnConfigurate(this);
		}
		
		override protected void ChangeSliceCount()
		{
			int oldCount = 0;
			if (FValues != null)
				oldCount = FValues.Length;
			RGBAColor[] tmp = new RGBAColor[oldCount];
			
			//save old values
			for (int i=0; i<oldCount; i++)
				tmp[i] = FValues[i];
			
			FValues = new RGBAColor[FSliceCount];
			
			//set old values to new array
			for (int i=0; i<Math.Min(FSliceCount, oldCount); i++)
				FValues[i] =  tmp[i];
			if (oldCount > 0)
			{
				for (int i=oldCount; i<FSliceCount; i++)
					FValues[i] =  tmp[oldCount-1];
			}
			else
				for (int i=oldCount; i<FSliceCount; i++)
					FValues[i] = FDefault;
		}
		
		public void SetSubType(RGBAColor Default, bool HasAlpha)
		{
			FDefault = Default;		

			for (int i=0; i<SliceCount; i++)
				SetColor(i, Default);

			FPinIsChanged = true;
		}
		
		unsafe public void GetColorPointer(out int SliceCount, out double* ValueP)
		{
			//TODO: return sth meaningfull
			SliceCount = 0;
			fixed(double* p = &FValues[0].R)
			{ValueP = p;}
		}
		
		unsafe public void GetColorPointer(out double* ValueP)
		{
			//TODO: return sth meaningfull
			fixed(double* p = &FValues[0].R)
			{ValueP = p;}
		}
		
		unsafe public void GetColorPointer(out double** ppDst)
		{
		    //TODO: not implemented
		    fixed(double* p = &FValues[0].R)
		    {ppDst = (double**)p;}
		}
		
		unsafe public void GetColorPointer(out int* pLength, out double** ppData)
		{
		    //TODO: not implemented
		    fixed(double* p = &FValues[0].R)
		    {
		        ppData = (double**)p;
		        pLength = (int*)0;
		    }
		}
		
		override protected string AsString(int index)
		{
			System.Globalization.NumberFormatInfo nf = new System.Globalization.NumberFormatInfo();
			nf.NumberDecimalSeparator = ".";
			return String.Format(nf, "|{0:F5},{1:F5},{2:F5},{3:F5}|", FValues[index].R, FValues[index].G, FValues[index].B, FValues[index].A);
		}
		
		override public void SetSpreadAsString(string Spread)
		{
			if (Spread == "")
			{
				if (FSliceCount > 0)
				{
					FSliceCountIsChanged = true;
					FPinIsChanged = true;
				}
				FSliceCount = 0;
			}
			else
			{
				string[] s = {"|,|"};
				string[] slices = Spread.Split(s, StringSplitOptions.RemoveEmptyEntries);
				int newSliceCount = (int) slices.Length / FDimension;
				if (newSliceCount != FSliceCount)
				{
					FSliceCount = newSliceCount;
					FValues = new RGBAColor[FSliceCount];
					FSliceCountIsChanged = true;
				}
				
				string tmp;
				char[] t = {'|'};
				char[] sp = {','};
				System.Globalization.NumberFormatInfo nf = new System.Globalization.NumberFormatInfo();
				nf.NumberDecimalSeparator = ".";
				for (int i=0; i<SliceCount; i++)
				{
					tmp = slices[i].Trim(t);
					string[] rgba = tmp.Split(sp);
					
					FValues[i] = new RGBAColor(Convert.ToDouble(rgba[0], nf), Convert.ToDouble(rgba[1], nf), Convert.ToDouble(rgba[2], nf), Convert.ToDouble(rgba[3], nf));
				}
				FPinIsChanged = true;
			}
			
			if (FOnConfigurate != null)
				FOnConfigurate(this);
		}
		
		override public void Draw(Graphics g, Font f, Brush b, Pen p, Rectangle r)
		{
			g.DrawRectangle(p, r);
			g.DrawString(Name, f, b, r.X+2, 2);
				
			Brush col;
			r.Height = FSliceHeight;
			for (int i=0; i<SliceCount; i++)
			{
				col = new SolidBrush(FValues[i].Color);
				r.Y = FSliceHeight + i*FSliceHeight;
				
				g.FillRectangle(col, r);
				g.DrawString(AsString(i), f, b, r.X+2, r.Y+2);
			}
		}
	}	
}
