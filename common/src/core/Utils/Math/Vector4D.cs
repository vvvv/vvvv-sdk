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
	
	//4d vector
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector4D
	{
		//data fields
		public double x, y, z, w;
		
		#region constructors
		
		public Vector4D(Vector4D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = v.w;
		}
		
		public Vector4D(Vector3D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = 1;
		}
		
		public Vector4D(Vector3D v, double w)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = w;
		}
		
		public Vector4D(Vector2D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = 0;
			this.w = 1;
		}
		
		public Vector4D(Vector2D v1, Vector2D v2)  
		{
			this.x = v1.x;
			this.y = v1.y;
			this.z = v2.x;
			this.w = v2.y;
		}
		
		public Vector4D(Vector2D v, double z, double w)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = z;
			this.w = w;
		}
		
		public Vector4D(double x, double y, double z, double w)  
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
		
		public Vector4D(double a)  
		{
			this.x = a;
			this.y = a;
			this.z = a;
			this.w = a;
		}
		
		#endregion constructors
		
		#region properties
		
		//xy
		public Vector2D xy
		{
			get
			{
				return new Vector2D(x, y);
			}
			set
			{
				x = value.x;
				y = value.y;
			}
		}
		
		//xyz
		public Vector3D xyz
		{
			get
			{
				return new Vector3D(x, y, z);
			}
			set
			{
				x = value.x;
				y = value.y;
				z = value.z;
			}
		}
		
		#endregion properties

		#region unary operators
		
		//+ vector
		public static Vector4D operator +(Vector4D v)
		{
			return v;
		}
		
		//- vector
		public static Vector4D operator -(Vector4D v)
		{
			return new Vector4D( -v.x, -v.y, -v.z, -v.w);
		}
		
		//! vector (returns the length of the vector)
		public static double operator !(Vector4D v)
		{
			return Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z + v.w*v.w);
		}
		
		//~ vector (returns the vector normalized)
		public static Vector4D operator ~(Vector4D v)
		{
			return v * (1 / Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z + v.w*v.w));
		}	
		
		#endregion unary operators
		
		#region binary operators
	
		//vector +
		public static Vector4D operator +(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}
		
		public static Vector4D operator +(Vector4D v1, double a)
		{
			return new Vector4D(v1.x + a, v1.y + a, v1.z + a, v1.w + a);
		}
		
		public static Vector4D operator +(double a, Vector4D v1)
		{
			return new Vector4D(a + v1.x, a + v1.y, a + v1.z, a + v1.w);
		}
		
		//vector -
		public static Vector4D operator -(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}
		
		public static Vector4D operator -(Vector4D v1, double a)
		{
			return new Vector4D(v1.x - a, v1.y - a, v1.z - a, v1.w - a);
		}
		
		public static Vector4D operator -(double a, Vector4D v1)
		{
			return new Vector4D(a - v1.x, a - v1.y, a - v1.z, a - v1.w);
		}
		
		//vector *
		public static double operator *(Vector4D v1, Vector4D v2)
		{
			return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z + v1.w * v2.w;
		}
		
		public static Vector4D operator *(Vector4D v1, double a)
		{
			return new Vector4D(v1.x * a, v1.y * a, v1.z * a, v1.w * a);
		}
		
		public static Vector4D operator *(double a, Vector4D v1)
		{
			return new Vector4D(a * v1.x, a * v1.y, a * v1.z, a * v1.w);
		}
		
		//vector /
		public static Vector4D operator /(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z, v1.w / v2.w);
		}
		
		public static Vector4D operator /(Vector4D v1, double a)
		{
			double rez = 1/a;
			return new Vector4D(v1.x * rez, v1.y * rez, v1.z * rez, v1.w * rez);
		}
		
		public static Vector4D operator /(double a, Vector4D v1)
		{
			return new Vector4D(a / v1.x, a / v1.y, a / v1.z, a / v1.w);
		}
		
		//vector %
		public static Vector4D operator %(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x % v2.x, v1.y % v2.y, v1.z % v2.z, v1.w % v2.w);
		}
		
		public static Vector4D operator %(Vector4D v1, double a)
		{
			return new Vector4D(v1.x % a, v1.y % a, v1.z % a, v1.w % a);
		}
		
		public static Vector4D operator %(double a, Vector4D v1)
		{
			return new Vector4D(a % v1.x, a % v1.y, a % v1.z, a % v1.w);
		}
		
		//vector & (returns the quaternion product of two vectors)	
//
//		defined:
//		real = v1.w * v2.w - (v1.xyz | v2.xyz);
//		imaginary = v1.xyz * v2.w + v2.xyz * v1.w + (v1.xyz & v2.xyz);
//		
//		public static Vector4D operator &(Vector4D v1, Vector4D v2)
//		{
//			return new Vector4D(v1.w*v2.xyz + v2.w*v2.xyz + (v1.xyz & v2.xyz), v1.w*v2.w - (v1.xyz|v2.xyz));
//		}
//		
//		expanded:
//		w = v1.w*v2.w - v1.x*v2.x - v1.y*v2.y - v1.z*v2.z
//		x = v1.w*v2.x + v1.x*v2.w + v1.y*v2.z - v1.z*v2.y
//		y = v1.w*v2.y + v1.y*v2.w + v1.z*v2.x - v1.x*v2.z
//		z = v1.w*v2.z + v1.z*v2.w + v1.x*v2.y - v1.y*v2.x
//		
//		
		
		public static Vector4D operator &(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.w*v2.x + v1.x*v2.w + v1.y*v2.z - v1.z*v2.y,
			                    v1.w*v2.y + v1.y*v2.w + v1.z*v2.x - v1.x*v2.z,
			                    v1.w*v2.z + v1.z*v2.w + v1.x*v2.y - v1.y*v2.x,
			                    v1.w*v2.w - v1.x*v2.x - v1.y*v2.y - v1.z*v2.z);
		}
		
		
		//vector | (component wise product)
		public static Vector4D operator |(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
		}
		
		#endregion binary operators		
		
		#region comparison operators
		
		//vector > 
		public static bool operator >(Vector4D v, double a)
		{
			return v.x > a && v.y > a && v.z > a && v.w > a;
		}
		
		//vector <
		public static bool operator <(Vector4D v, double a)
		{
			return v.x < a && v.y < a && v.z < a && v.w < a;
		}
		
		//vector >= 
		public static bool operator >=(Vector4D v, double a)
		{
			return v.x >= a && v.y >= a && v.z >= a && v.w >= a;
		}
		
		//vector <=
		public static bool operator <=(Vector4D v, double a)
		{
			return v.x <= a && v.y <= a && v.z <= a && v.w <= a;
		}
		
		#endregion comparison operators
	}
}

