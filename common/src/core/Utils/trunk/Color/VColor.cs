/*
 * 
 * the c# vvvv color library
 * 
 * 
 */

using System;
using System.Drawing;

/// <summary>
/// VVVV Color Utilities 
/// </summary>
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
		/// Note that the ! operator of RGBAColor does the same
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
		
		/// <summary>
		/// 2d linear interpolation in x and y direction for colors
		/// </summary>
		/// <param name="x">The x position where to interpolate, 0..1</param>
		/// <param name="y">The y position where to interpolate, 0..1</param>
		/// <param name="P1">Upper left color</param>
		/// <param name="P2">Upper right color</param>
		/// <param name="P3">Lower right color</param>
		/// <param name="P4">Lower left color</param>
		/// <returns>Interpolated color between the 4 colors in the corners</returns>
		public static RGBAColor BilerpRGBA(double x, double y, RGBAColor P1, RGBAColor P2, RGBAColor P3, RGBAColor P4)
		{
			
			//interpolate lower colors in x direction
			P1 = LerpRGBA(P1, P2, x);
			
			//interpolate upper colors in x direction
			P3 = LerpRGBA(P4, P3, x);
			
			//interpolate results in y direction
			return LerpRGBA(P3, P1, y);
			
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
			
			double r, g, b;
			
			//get rgb values
			HSLtoRGB(H, S, L, out r, out g, out b);
			
			return Color.FromArgb((int) (255 * A), (int)(255 * r), (int)(255 * g), (int)(255 * b));
			
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
		/// <returns>false, if color is gray, hue has no defined value in that case</returns>
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
		/// Computes RGB values from HSL values, found on:
		/// http://www.geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm
		/// </summary>
		/// <param name="H">Hue</param>
		/// <param name="S">Saturation</param>
		/// <param name="L">Lightness</param>
		/// <param name="Red">Output parameter, gets filled with the red value</param>
		/// <param name="Green">Output parameter, gets filled with the green value</param>
		/// <param name="Blue">Output parameter, gets filled with the blue value</param>
		public static void HSLtoRGB(double H, double S, double L, out double Red, out double Green, out double Blue)
		{
			
			Red = Green = Blue = L;   // default to gray
			
			double v;
			v = (L <= 0.5) ? (L * (1.0 + S)) : (L + S - L * S);

			if (v > 0)
			{
				
				double m;
				double sv;
				int sextant;
				double fract, vsf, mid1, mid2;

				m = L + L - v;
				sv = (v - m ) / v;
				H *= 6.0;
				sextant = (int)H;
				fract = H - sextant;
				vsf = v * sv * fract;
				mid1 = m + vsf;
				mid2 = v - vsf;
				
				switch (sextant)
				{
					case 0:
						Red = v;
						Green = mid1;
						Blue = m;
						break;
						
					case 1:
						Red = mid2;
						Green = v;
						Blue = m;
						break;
						
					case 2:
						Red = m;
						Green = v;
						Blue = mid1;
						break;
						
					case 3:
						Red = m;
						Green = mid2;
						Blue = v;
						break;
						
					case 4:
						Red = mid1;
						Green = m;
						Blue = v;
						break;
						
					case 5:
						Red = v;
						Green = m;
						Blue = mid2;
						break;
				}
			}
		}

		
		/// <summary>
		/// Computes HSL values from RGB values, found on:
		/// http://www.geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm
		/// </summary>
		/// <param name="r">Red</param>
		/// <param name="g">Green</param>
		/// <param name="b">Blue</param>
		/// <param name="h">Output parameter, gets filled with the hue value</param>
		/// <param name="s">Output parameter, gets filled with the saturation value</param>
		/// <param name="l">Output parameter, gets filled with the lightness value</param>
		/// <returns>false, if color is gray, in that case hue is not defined</returns>
		public static bool RGBtoHSL (double r, double g, double b, out double h, out double s, out double l)
		{

			double v;
			double m;
			double vm;
			double r2, g2, b2;
			
			h = 0; // default to black
			s = 0;
			l = 0;

			v = Math.Max(r,g);
			v = Math.Max(v,b);
			m = Math.Min(r,g);
			m = Math.Min(m,b);

			l = (m + v) * 0.5;
			
			if (l <= 0.0)
			{
				return false;
			}
			
			vm = v - m;
			s = vm;
			
			if (s > 0.0)
			{
				s /= (l <= 0.5) ? (v + m ) : (2.0 - v - m) ;
			}
			else
			{
				return false;
			}
			
			r2 = (v - r) / vm;
			g2 = (v - g) / vm;
			b2 = (v - b) / vm;
			
			if (r == v)
			{
				h = (g == m ? 5.0 + b2 : 1.0 - g2);
			}
			else if (g == v)
			{
				h = (b == m ? 1.0 + r2 : 3.0 - b2);
			}
			else
			{
				h = (r == m ? 3.0 + g2 : 5.0 - r2);
			}
			
			h /= 6.0;
			
			return true;
		}
		
		#endregion color conversion
		
	}
}
