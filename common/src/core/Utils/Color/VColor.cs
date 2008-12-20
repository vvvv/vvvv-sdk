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
	/// <summary>
	/// The vvvv c# color routines library
	/// </summary>
	public sealed class VColor
	{
		#region constants

		/// <summary>
		/// Red as constant, (1,0,0,1)
		/// </summary>
		public static readonly RGBAColor Red = new RGBAColor(1, 0, 0, 1);
		/// <summary>
		/// Green as constant, (0,1,0,1)
		/// </summary>
		public static readonly RGBAColor Green = new RGBAColor(0, 1, 0, 1);
		/// <summary>
		/// Blue as constant, (0,0,1,1)
		/// </summary>
		public static readonly RGBAColor Blue = new RGBAColor(0, 0, 1, 1);
		/// <summary>
		/// White as constant, (1,1,1,1)
		/// </summary>
		public static readonly RGBAColor White = new RGBAColor(1, 1, 1, 1);
		/// <summary>
		/// Black as constant, (0,0,0,1)
		/// </summary>
		public static readonly RGBAColor Black = new RGBAColor(0, 0, 0, 1);
		
		#endregion constants
		
		#region color modification

		/// <summary>
		/// Function to calculate the complementary color
		/// </summary>
		/// <param name="Col">Input color</param>
		/// <returns>Complement color of the RGB channels of the input color</returns>
		public static RGBAColor Complement(RGBAColor Col)
		{
			Col.R = 1 - Col.R;
			Col.G = 1 - Col.G;
			Col.B = 1 - Col.B;
			
			return Col;
		}
		
		/// <summary>
		/// Adds a value to the RGB channels of a color and takes the result modulo 1
		/// </summary>
		/// <param name="Col"></param>
		/// <param name="Offset"></param>
		/// <returns>(Col.RGB + Offset) modulo 1</returns>
		public static RGBAColor Offset(RGBAColor Col, double Offset)
		{
			Col.R = (Col.R + Offset) % 1.0;
			Col.G = (Col.G + Offset) % 1.0;
			Col.B = (Col.B + Offset) % 1.0;
			
			return Col;
		}
		
		/// <summary>
		/// Function to get black or white, which ever has higher contrast to the input color, e.g. for text on colored backgrounds
		/// </summary>
		/// <param name="C">Input color</param>
		/// <returns>Black or white in C# color format</returns>
		public static Color Invert(Color C)
		{			
			RGBAColor col = new RGBAColor(C.R/255.0, C.R/255.0, C.R/255.0, 1);
			
			if (Brightness(col) > 0.5)
				return Color.White;
			else
				return Color.Black;
		}
		
		/// <summary>
		/// Linear interpolation (blending) between two colors
		/// </summary>
		/// <param name="Col1"></param>
		/// <param name="Col2"></param>
		/// <param name="x">Blending factor, 0..1</param>
		/// <returns>Linear interpolation (blending) between Col1 and Col2 if x in the range ]0..1[, Col1 if x = 0, Col2 if x = 1</returns>
		public static RGBAColor LerpRGBA(RGBAColor Col1, RGBAColor Col2, double x)
		{
			return Col1 + x * (Col2 - Col1);
		}
		
		#endregion color modification
					
		#region color conversion
		
		/// <summary>
		/// Calculates the brighness of a color with the formula 0.222 * R + 0.707 * G + 0.071 * B
		/// </summary>
		/// <param name="C"></param>
		/// <returns>Brightness value of the input color C</returns>
		public static double Brightness(RGBAColor C)
		{
			return 0.222 * C.R + 0.707 * C.G + 0.071 * C.B;
		}

		/// <summary>
		/// Get a C# color type from hue, saturation, lightness and alpha values
		/// </summary>
		/// <param name="H"></param>
		/// <param name="S"></param>
		/// <param name="L"></param>
		/// <param name="A"></param>
		/// <returns>C# Color in RGB format</returns>
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
		
		/// <summary>
		/// Get a color from hue, saturation, brightness and alpha values
		/// </summary>
		/// <param name="Hue"></param>
		/// <param name="Saturation"></param>
		/// <param name="Value"></param>
		/// <param name="Alpha"></param>
		/// <returns>Color in RGB format</returns>
		public static RGBAColor FromHSVA(double Hue, double Saturation, double Value, double Alpha)
		{
			double R, G, B;
			VColor.HSVtoRGB(Hue, Saturation, Value, out R, out G, out B);
			
			return new RGBAColor(R, G, B, Alpha);
		}
		
		/// <summary>
		/// Function to convert RGB values to HSV values
		/// 
		/// merged from http://www.easyrgb.com/math.php?MATH=M20#text20
		/// and http://www.efg2.com/Lab/Graphics/Colors/HSV.htm
		/// </summary>
		/// <param name="R"></param>
		/// <param name="G"></param>
		/// <param name="B"></param>
		/// <param name="Hue">Output parameter, this variable gets filled with the Hue value</param>
		/// <param name="Sat">Output parameter, this variable gets filled with the Saturation value</param>
		/// <param name="Value">Output parameter, this variable gets filled with the Brightness value</param>
		/// <returns>true if conversion was successful</returns>
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

	
		/// <summary>
		/// Function to convert HSV values to RGB values
		/// 
		/// merged methods from EasyRGB (http://www.easyrgb.com/math.php?MATH=M21#text21) 
		/// and the book GRAPHICS GEMS
		/// </summary>
		/// <param name="H"></param>
		/// <param name="S"></param>
		/// <param name="V"></param>
		/// <param name="Red">Output parameter, this variable gets filled with the red value</param>
		/// <param name="Green">Output parameter, this variable gets filled with the green value</param>
		/// <param name="Blue">Output parameter, this variable gets filled with the blue value</param>
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
