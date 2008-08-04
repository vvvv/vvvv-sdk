/*
 * 
 * the c# vvvv math library
 * 
 * 
 */

using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.VMath
{
	
	//2d vector
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2D
	{
		//data fields
		public double x, y;
		
		#region constructors
		
		public Vector2D(Vector2D v)  
		{
			this.x = v.x;
			this.y = v.y;
		}
		
		public Vector2D(double x, double y)  
		{
			this.x = x;
			this.y = y;
		}
		
		public Vector2D(double a)  
		{
			this.x = a;
			this.y = a;
		}
		
		#endregion constructors
		
		#region unary operators
		
		//+ vector
		public static Vector2D operator +(Vector2D v)
		{
			return v;
		}
		
		//- vector
		public static Vector2D operator -(Vector2D v)
		{
			return new Vector2D(-v.x, -v.y);
		}
		
		//! vector (returns the length of the vector)
		public static double operator !(Vector2D v)
		{
			return Math.Sqrt(v.x*v.x + v.y*v.y);
		}
	
		//~ vector (returns the vector normalized)
		public static Vector2D operator ~(Vector2D v)
		{
			return v * (1 / Math.Sqrt(v.x*v.x + v.y*v.y));
		}
		
		#endregion unary operators
		
		#region binary operators
	
		//vector +
		public static Vector2D operator +(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x + v2.x, v1.y + v2.y);
		}
		
		public static Vector2D operator +(Vector2D v1, double a)
		{
			return new Vector2D(v1.x + a, v1.y + a);
		}
		
		public static Vector2D operator +(double a, Vector2D v1)
		{
			return new Vector2D(a + v1.x, a + v1.y);
		}
		
		//vector -
		public static Vector2D operator -(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x - v2.x, v1.y - v2.y);
		}
		
		public static Vector2D operator -(Vector2D v1, double a)
		{
			return new Vector2D(v1.x - a, v1.y - a);
		}
		
		public static Vector2D operator -(double a, Vector2D v1)
		{
			return new Vector2D(a - v1.x, a - v1.y);
		}
		
		//vector *
		public static Vector2D operator *(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x * v2.x, v1.y * v2.y);
		}
		
		public static Vector2D operator *(Vector2D v1, double a)
		{
			return new Vector2D(v1.x * a, v1.y * a);
		}
		
		public static Vector2D operator *(double a, Vector2D v1)
		{
			return new Vector2D(a * v1.x, a * v1.y);
		}
		
		//vector /
		public static Vector2D operator /(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x / v2.x, v1.y / v2.y);
		}
		
		public static Vector2D operator /(Vector2D v1, double a)
		{
			double rez = 1/a;
			return new Vector2D(v1.x * rez, v1.y * rez);
		}
		
		public static Vector2D operator /(double a, Vector2D v1)
		{
			return new Vector2D(a / v1.x, a / v1.y);
		}
		
		//vector %
		public static Vector2D operator %(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x % v2.x, v1.y % v2.y);
		}
		
		public static Vector2D operator %(Vector2D v1, double a)
		{
			return new Vector2D(v1.x % a, v1.y % a);
		}
		
		public static Vector2D operator %(double a, Vector2D v1)
		{
			return new Vector2D(a % v1.x, a % v1.y);
		}
		
		//vector & (returns the cross product of two vectors)
		public static double operator &(Vector2D v1, Vector2D v2)
		{
			return v1.x * v2.y - v1.y * v2.x;
		}
		
		//vector | (dot product)
		public static double operator |(Vector2D v1, Vector2D v2)
		{
			return v1.x * v2.x + v1.y * v2.y;
		}
	
		#endregion binary operators
		
		#region comparison operators
		
		//vector > 
		public static bool operator >(Vector2D v, double a)
		{
			return v.x > a && v.y > a;
		}
		
		//vector <
		public static bool operator <(Vector2D v, double a)
		{
			return v.x < a && v.y < a;
		}
		
		//vector >= 
		public static bool operator >=(Vector2D v, double a)
		{
			return v.x >= a && v.y >= a;
		}
		
		//vector <=
		public static bool operator <=(Vector2D v, double a)
		{
			return v.x <= a && v.y <= a;
		}
		
		#endregion comparison operators
	}
}

