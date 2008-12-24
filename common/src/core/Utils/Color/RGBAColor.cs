/*
 * 
 * the c# vvvv color library
 * 
 * 
 */

using System;
using System.Runtime.InteropServices;
using System.Drawing;
using VVVV.Utils.VMath;

namespace VVVV.Utils.VColor
{
	/// <summary>
	/// 256-bit color struct, compatible with vvvv colors
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct RGBAColor
	{
		#region data fields

		/// <summary>
		/// The Red data field
		/// </summary>
		public double R;
		/// <summary>
		/// The Green data field
		/// </summary>
		public double G;
		/// <summary>
		/// The Blue data field
		/// </summary>
		public double B;
		/// <summary>
		/// The Alpha data field
		/// </summary>
		public double A;
		
		#endregion data fields
		
		#region casting operators
		
		/// <summary>
		/// Casts a color to a 4d-vector
		/// </summary>
		/// <param name="a">color to cast</param>
		/// <returns>4d-vector with same values than input the color</returns>
		public static implicit operator Vector4D(RGBAColor a)
        {
			return new Vector4D(a.R, a.G, a.B, a.A);
        }
		
		/// <summary>
		/// Casts a 4d-vector to a color
		/// </summary>
		/// <param name="a">4d-vector to cast</param>
		/// <returns>color with same values like than input 4d-vector</returns>
		public static implicit operator RGBAColor(Vector4D a)
        {
			return new RGBAColor(a.x, a.y, a.z, a.w);
        }
		
		#endregion casting operators
		
		#region constructor, properties
		
		/// <summary>
		/// vvvv color constructor
		/// </summary>
		/// <param name="Red">red component, 0..1</param>
		/// <param name="Green">green component, 0..1</param>
		/// <param name="Blue">blue component, 0..1</param>
		/// <param name="Alpha">alpha component, 0..1</param>
		public RGBAColor(double Red, double Green, double Blue, double Alpha)
		{
			R = Red;
			G = Green;
			B = Blue;
			A = Alpha;
		}
	
		/// <summary>
		/// C# color type property, can be used for conversion
		/// </summary>
		public Color Color
		{
			get {return Color.FromArgb((int)(A*255), (int)(R*255), (int)(G*255), (int)(B*255));}
			set {A = value.A / 255.0; R = value.R / 255.0; G = value.G / 255.0; B = value.B / 255.0;}
		}
		
		/// <summary>
		/// Get string name for the color
		/// </summary>
		/// <returns>Color name</returns>
		public override string ToString()
		{
			return Color.Name;		
		}
		
		#endregion constructor, properties
		
		#region binary operators
		
		/// <summary>
		/// Adds the color components of two colors
		/// </summary>
		/// <param name="C1"></param>
		/// <param name="C2"></param>
		/// <returns>Sum of two colors</returns>
		public static RGBAColor operator +(RGBAColor C1, RGBAColor C2)
		{
			return new RGBAColor(C1.R + C2.R, C1.G + C2.G, C1.B + C2.B, C1.A + C2.A);
		}
		
		/// <summary>
		/// Subtracts the color components of two colors
		/// </summary>
		/// <param name="C1"></param>
		/// <param name="C2"></param>
		/// <returns>Difference of two colors</returns>
		public static RGBAColor operator -(RGBAColor C1, RGBAColor C2)
		{
			return new RGBAColor(C1.R - C2.R, C1.G - C2.G, C1.B - C2.B, C1.A - C2.A);
		}
		
		/// <summary>
		/// Multiplies a color with a factor
		/// </summary>
		/// <param name="C"></param>
		/// <param name="factor"></param>
		/// <returns>Color multiplied by the factor</returns>
		public static RGBAColor operator *(RGBAColor C, double factor)
		{
			return new RGBAColor(C.R * factor, C.G * factor, C.B * factor, C.A * factor);
		}
		
		/// <summary>
		/// Multiplies a factor with a color
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="C"></param>
		/// <returns>Color multiplied by the factor</returns>
		public static RGBAColor operator *(double factor, RGBAColor C)
		{
			return new RGBAColor(C.R * factor, C.G * factor, C.B * factor, C.A * factor);
		}
		
		#endregion binary operators
	}
}
