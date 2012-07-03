using System;

namespace VVVV.Utils.SlimDX
{
	/// <summary>
	/// Low level utils to use uint as 32bit color
	/// </summary>
	public static class UInt32Utils
	{
		/// <summary>
		/// Converts a color stored as uint to a color of type int.
		/// </summary>
		/// <param name="col">The color to convert.</param>
		/// <returns>The converted color as int.</returns>
		public static int BitwiseToInt(this uint col)
		{
			unchecked
			{
				return (int)col;
			}
		}
		
		/// <summary>
		/// Converts a color stored as int to a color of type uint.
		/// </summary>
		/// <param name="col">The color to convert.</param>
		/// <returns>The converted color as uint.</returns>
		public static uint BitwiseToUInt(this int col)
		{
			unchecked
			{
				return (uint)col;
			}
		}
		
		/// <summary>
		/// Set up uint color.
		/// </summary>
		/// <param name="a">Alpha.</param>
		/// <param name="r">Red.</param>
		/// <param name="g">Green.</param>
		/// <param name="b">Blue.</param>
		/// <returns>The color as uint.</returns>
		public static uint fromARGB(byte a, byte r, byte g, byte b)
		{
			return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | ((uint)b);
		}
		
		/// <summary>
		/// Retrieve color channels from uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <param name="a">Alpha.</param>
		/// <param name="r">Red.</param>
		/// <param name="g">Green.</param>
		/// <param name="b">Blue.</param>
		public static void getARGB(this uint col, out byte a, out byte r, out byte g, out byte b)
		{
			a = col.getA();
			r = col.getR();
			g = col.getG();
			b = col.getB();
		}
		
		/// <summary>
		/// Retrieves the alpha channel from a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <returns>The alpha channel.</returns>
		public static byte getA(this uint col)
		{
			unchecked
			{
				return (byte)(col >> 24);
			}
		}
		
		/// <summary>
		/// Retrieves the red channel from a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <returns>The red channel.</returns>
		public static byte getR(this uint col)
		{
			unchecked
			{
				return (byte)(col >> 16);
			}
		}
		
		/// <summary>
		/// Retrieves the green channel from a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <returns>The green channel.</returns>
		public static byte getG(this uint col)
		{
			unchecked
			{
				return (byte)(col >> 8);
			}
		}
		
		/// <summary>
		/// Retrieves the blue channel from a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <returns>The blue channel.</returns>
		public static byte getB(this uint col)
		{
			unchecked
			{
				return (byte)col;
			}
		}
		
		/// <summary>
		/// Sets the alpha channel in a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <param name="a">The alpha channel.</param>
		/// <returns>The color as uint with new alpha channel set.</returns>
		public static uint setA(this uint col, byte a)
		{
			return (col | 0xFF000000) & ((uint)a << 24);
		}
		
		/// <summary>
		/// Sets the red channel in a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <param name="r">The red channel.</param>
		/// <returns>The color as uint with new red channel set.</returns>
		public static uint setR(this uint col, byte r)
		{
			return (col | 0x00FF0000) & ((uint)r << 16);
		}
		
		/// <summary>
		/// Sets the green channel in a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <param name="g">The green channel.</param>
		/// <returns>The color as uint with new green channel set.</returns>
		public static uint setG(this uint col, byte g)
		{
			return (col | 0x0000FF00) & ((uint)g << 8);
		}
		
		/// <summary>
		/// Sets the blue channel in a uint color.
		/// </summary>
		/// <param name="col">The color as uint.</param>
		/// <param name="b">The blue channel.</param>
		/// <returns>The color as uint with new blue channel set.</returns>
		public static uint setB(this uint col, byte b)
		{
			return (col | 0x000000FF) & ((uint)b);
		}
		
	}
}
