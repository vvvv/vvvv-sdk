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
	
	//3d vector
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3D
	{
		//data fields
		public double x, y, z;
		
		#region constructors
		
		public Vector3D(Vector3D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}
		
		public Vector3D(Vector2D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = 0;
		}
		
		public Vector3D(Vector2D v, double z)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = z;
		}
		
		public Vector3D(double x, double y, double z)  
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		public Vector3D(double a)  
		{
			this.x = a;
			this.y = a;
			this.z = a;
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
		
		//xz
		public Vector2D xz
		{
			get
			{
				return new Vector2D(x, z);
			}
			set
			{
				x = value.x;
				z = value.y;
			}
		}
		
		//xy
		public Vector2D yz
		{
			get
			{
				return new Vector2D(y, z);
			}
			set
			{
				y = value.x;
				z = value.y;
			}
		}
		
		#endregion properties
		
		#region unary operators
		
		//+ vector
		public static Vector3D operator +(Vector3D v)
		{
			return v;
		}
		
		//- vector
		public static Vector3D operator -(Vector3D v)
		{
			return new Vector3D( -v.x, -v.y, -v.z);
		}
		
		//! vector (returns the length of the vector)
		public static double operator !(Vector3D v)
		{
			return Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
		}
		
		//~ vector (returns the vector normalized)
		public static Vector3D operator ~(Vector3D v)
		{
			return v * (1 / Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z));	
		}

		#endregion unary operators
		
		#region binary operators
		
		//vector +
		public static Vector3D operator +(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		}
		
		public static Vector3D operator +(Vector3D v1, double a)
		{
			return new Vector3D(v1.x + a, v1.y + a, v1.z + a);
		}
		
		public static Vector3D operator +(double a, Vector3D v1)
		{
			return new Vector3D(a + v1.x, a + v1.y, a + v1.z);
		}
		
		//vector -
		public static Vector3D operator -(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		}
		
		public static Vector3D operator -(Vector3D v1, double a)
		{
			return new Vector3D(v1.x - a, v1.y - a, v1.z - a);
		}
		
		public static Vector3D operator -(double a, Vector3D v1)
		{
			return new Vector3D(a - v1.x, a - v1.y, a - v1.z);
		}
		
		//vector *
		public static double operator *(Vector3D v1, Vector3D v2)
		{
			return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
		}
		
		public static Vector3D operator *(Vector3D v1, double a)
		{
			return new Vector3D(v1.x * a, v1.y * a, v1.z * a);
		}
		
		public static Vector3D operator *(double a, Vector3D v1)
		{
			return new Vector3D(a * v1.x, a * v1.y, a * v1.z);
		}
		
		//vector /
		public static Vector3D operator /(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
		}
		
		public static Vector3D operator /(Vector3D v1, double a)
		{
			double rez = 1/a;
			return new Vector3D(v1.x * rez, v1.y * rez, v1.z * rez);
		}
		
		public static Vector3D operator /(double a, Vector3D v1)
		{
			return new Vector3D(a / v1.x, a / v1.y, a / v1.z);
		}
		
		//vector %
		public static Vector3D operator %(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x % v2.x, v1.y % v2.y, v1.z % v2.z);
		}
		
		public static Vector3D operator %(Vector3D v1, double a)
		{
			return new Vector3D(v1.x % a, v1.y % a, v1.z % a);
		}
		
		public static Vector3D operator %(double a, Vector3D v1)
		{
			return new Vector3D(a % v1.x, a % v1.y, a % v1.z);
		}
		
		//vector & (returns the cross product of two vectors)
		public static Vector3D operator &(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.y*v2.z - v1.z*v2.y, v1.z*v2.x - v1.x*v2.y, v1.x*v2.y - v1.y*v2.x);
		}
		
		//vector | (component wise product)
		public static Vector3D operator |(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		}
		
		#endregion binary operators
		
		#region comparison operators
		
		//vector >
		public static bool operator >(Vector3D v, double a)
		{
			return v.x > a && v.y > a && v.z > a;
		}
		
		//vector <
		public static bool operator <(Vector3D v, double a)
		{
			return v.x < a && v.y < a && v.z < a;
		}
		
		//vector >= 
		public static bool operator >=(Vector3D v, double a)
		{
			return v.x >= a && v.y >= a && v.z >= a;
		}
		
		//vector <=
		public static bool operator <=(Vector3D v, double a)
		{
			return v.x <= a && v.y <= a && v.z <= a;
		}
		
		#endregion comparison operators
	}

}

