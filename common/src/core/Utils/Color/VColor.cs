/*
 * 
 * the c# vvvv color library
 * 
 * 
 */

using System;
using System.Runtime.InteropServices;

using System.Drawing;

namespace VVVV.Utils.VColor
{
	//TRGBA
	[StructLayout(LayoutKind.Sequential)]
	public struct RGBAColor
	{
		//data fields
		public double R, G, B, A;
		
		public RGBAColor(double Red, double Green, double Blue, double Alpha)
		{
			R = Red;
			G = Green;
			B = Blue;
			A = Alpha;
		}

		public Color Color
		{
			get {return Color.FromArgb((int)(A*255), (int)(R*255), (int)(G*255), (int)(B*255));}
			set {A = value.A / 255.0; R = value.R / 255.0; G = value.G / 255.0; B = value.B / 255.0;}
		}
		
		public override string ToString()
		{
			return Color.Name;		
		}
		
		#region binary operators
		
		public static RGBAColor operator +(RGBAColor C1, RGBAColor C2)
		{
			return new RGBAColor(C1.R + C2.R, C1.G + C2.G, C1.B + C2.B, C1.A + C2.A);
		}
		
		public static RGBAColor operator -(RGBAColor C1, RGBAColor C2)
		{
			return new RGBAColor(C1.R - C2.R, C1.G - C2.G, C1.B - C2.B, C1.A - C2.A);
		}
		
		public static RGBAColor operator *(RGBAColor C, double factor)
		{
			return new RGBAColor(C.R * factor, C.G * factor, C.B * factor, C.A * factor);
		}
		
		public static RGBAColor operator *(double factor, RGBAColor C)
		{
			return new RGBAColor(C.R * factor, C.G * factor, C.B * factor, C.A * factor);
		}
		
		#endregion binary operators
	}
	
	public sealed class VColor
	{
		#region constants

		public static readonly RGBAColor Red = new RGBAColor(1, 0, 0, 1);
		public static readonly RGBAColor Green = new RGBAColor(0, 1, 0, 1);
		public static readonly RGBAColor Blue = new RGBAColor(0, 0, 1, 1);
		public static readonly RGBAColor White = new RGBAColor(1, 1, 1, 1);
		public static readonly RGBAColor Black = new RGBAColor(0, 0, 0, 1);
		
		#endregion constants
		
		#region color modification

		public static RGBAColor Complement(RGBAColor Col)
		{
			Col.R = 1 - Col.R;
			Col.G = 1 - Col.G;
			Col.B = 1 - Col.B;
			
			return Col;
		}
		
		public static RGBAColor Offset(RGBAColor Col, double offset)
		{
			Col.R = (Col.R + offset) % 1.0;
			Col.G = (Col.G + offset) % 1.0;
			Col.B = (Col.B + offset) % 1.0;
			
			return Col;
		}
		
		public static Color Invert(Color C)
		{
			Color inv = Color.FromArgb(255, (C.R + 64) % 255, (C.G + 64) % 255, (C.B + 64) % 255);
			inv = HSLAToColor(0, 0, inv.GetBrightness(), 1);
			return inv;
		}
		
			public static RGBAColor LerpRGBA(RGBAColor Col1, RGBAColor Col2, double x)
		{
			return Col1 + x * (Col2 - Col1);
		}
		
		#endregion color modification
					
		#region color conversion

		public static Color HSLAToColor(double H, double S, double L, double A)
		{
			//color conversion code borrowed from Richard Newman:
			//http://richnewman.wordpress.com/hslcolor-class/
		
			double r = 0, g = 0, b = 0;
		   	if (L != 0)
		 	{
		 		if (S == 0)
		        	r = g = b = L;
		     	else
		     	{
		        	double temp2 = GetTemp2(L, S);
		        	double temp1 = 2.0 * L - temp2;
		
		        	r = GetColorComponent(temp1, temp2, H + 1.0 / 3.0);
		        	g = GetColorComponent(temp1, temp2, H);
		        	b = GetColorComponent(temp1, temp2, H - 1.0 / 3.0);
		     	}
		 	}
		 	return Color.FromArgb((int) (255 * A), (int)(255 * r), (int)(255 * g), (int)(255 * b));
		}
				
		private static double GetColorComponent(double temp1, double temp2, double temp3)
		{
			temp3 = MoveIntoRange(temp3);
		    if (temp3 < 1.0 / 6.0)
		        return temp1 + (temp2 - temp1) * 6.0 * temp3;
		    else if (temp3 < 0.5)
		        return temp2;
		    else if (temp3 < 2.0 / 3.0)
		        return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
		    else
		        return temp1;
		}
				
		private static double MoveIntoRange(double temp3)
		{
			if (temp3 < 0.0)
				temp3 += 1.0;
			else if (temp3 > 1.0)
			    temp3 -= 1.0;
			return temp3;
		}
				
		private static double GetTemp2(double L, double S)
		{
			double temp2;
			if (L < 0.5)  
				temp2 = L * (1.0 + S);
			else
				temp2 = L + S - (L * S);
			
			return temp2;
		}
		
		public static RGBAColor FromHSVA(double Hue, double Saturation, double Value, double Alpha)
		{
			double R, G, B;
			VColor.HSVtoRGB(Hue, Saturation, Value, out R, out G, out B);
			
			return new RGBAColor(R, G, B, Alpha);
		}
		
		//merged from http://www.easyrgb.com/math.php?MATH=M20#text20
		//and http://www.efg2.com/Lab/Graphics/Colors/HSV.htm
		public static bool RGBtoHSV(double R, double G, double B, out double Hue, out double Sat, out double Value)
		{
			double min = Math.Min(R, Math.Min(G, B));
  			Value = Math.Max(R, Math.Max(G, B));
  			double delta = Value - min;

  			//Calculate saturation: saturation is 0 if r, g and b are all the same
  			if (delta == 0 || Value == 0)
  			{
    			Sat = 0;
    			Hue = 0; //NaN;				// Achromatic: When s = 0, h is undefined
    			return false;
  			}

  			Sat = delta / Value;

  			if (R == Value) 						// between yellow and magenta [degrees]
    			Hue = (G - B) / (6 * delta);
  			else if (G == Value)					// between cyan and yellow
			    Hue = (2 + (B - R) / delta) / 6;
    		else			                    	// between magenta and cyan
      			Hue = (4 + (R - G) / delta) / 6;

    		if (Hue < 0) 
    		{
    			Hue = Hue + 1;
    		}
    		
    		return true;
		}

		//merged methods from EasyRGB (http://www.easyrgb.com/math.php?MATH=M21#text21) 
		//and the book GRAPHICS GEMS
		public static void HSVtoRGB (double H, double S, double V, out double Red, out double Green, out double Blue)
		{
			Red = Green = Blue = V;
			
			if (S != 0)
			{
				H = H - Math.Truncate(H);
				double min = V * (1 - S);
			
			    H = 6 * H;
			    int sextant = (int) Math.Truncate(H);
			    double fract = H - sextant;
			    double vsf = V * S * fract;
			    double mid1 = min + vsf;
			    double mid2 = V - vsf;
			    switch (sextant)
			    {
			    	case 0: {Red = V; Green = mid1; Blue = min; break;}
			    	case 1: {Red = mid2; Green = V; Blue = min; break;}
			    	case 2: {Red = min; Green = V; Blue = mid1; break;}
			    	case 3: {Red = min; Green = mid2; Blue = V; break;}
			    	case 4: {Red = mid1; Green = min; Blue = V; break;}
			    	case 5: {Red = V; Green = min; Blue = mid2; break;}
			    }
			}
		}
		
		#endregion color conversion
		
	}
}
