using System;

namespace VVVV.Utils.SlimDX
{
	/// <summary>
	/// low level utils to use uint as 32bit color
	/// </summary>
	public static class UInt32Extensions
	{
		//bitwise int/uint conversion
		public static int BitwiseToInt(this uint col)
		{
			unchecked
			{
				return (int)col;
			}
		}
		
		public static uint BitwiseToUInt(this int col)
		{
			unchecked
			{
				return (uint)col;
			}
		}
		
		//set up uint color
		public static uint setARGB(this uint col, byte a, byte r, byte g, byte b)
		{
			return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | ((uint)b);
		}
		
		public static void getARGB(this uint col, out byte a, out byte r, out byte g, out byte b)
		{
			a = col.getA();
			r = col.getR();
			g = col.getG();
			b = col.getB();
		}
		
		//get channels
		public static byte getA(this uint col)
		{
			unchecked
			{
				return (byte)(col >> 24);
			}
		}
		
		public static byte getR(this uint col)
		{
			unchecked
			{
				return (byte)(col >> 16);
			}
		}
		
		public static byte getG(this uint col)
		{
			unchecked
			{
				return (byte)(col >> 8);
			}
		}
		
		public static byte getB(this uint col)
		{
			unchecked
			{
				return (byte)col;
			}
		}
		
		//set channels
		public static uint setA(this uint col, byte a)
		{
			return (col | 0xFF000000) & ((uint)a << 24);
		}
		
		public static uint setR(this uint col, byte r)
		{
			return (col | 0x00FF0000) & ((uint)r << 16);
		}
		
		public static uint setG(this uint col, byte g)
		{
			return (col | 0x0000FF00) & ((uint)g << 8);
		}
		
		public static uint setB(this uint col, byte b)
		{
			return (col | 0x000000FF) & ((uint)b);
		}
		
	}
}
