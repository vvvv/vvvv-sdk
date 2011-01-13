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
	
	/// <summary>
	/// Fast 3d vector struct with operators
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3D
	{
		#region data fields
		
		/// <summary>
		/// Data component for the x dimension
		/// </summary>
		public double x;
		/// <summary>
		/// Data component for the y dimension
		/// </summary>
		public double y;
		/// <summary>
		/// Data component for the z dimension
		/// </summary>
		public double z;
		
		#endregion data fields
		
		#region constructors
		
		/// <summary>
		/// Copies a 3d-vecor
		/// </summary>
		/// <param name="v">Vector to copy</param>
		public Vector3D(Vector3D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}
		
		/// <summary>
		/// Makes a 3d-vector copy from a 2d-vector, z is set to 0
		/// </summary>
		/// <param name="v">2d-vector to copy</param>
		public Vector3D(Vector2D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = 0;
		}
		
		/// <summary>
		/// Makes a 3d-vector copy from a 2d-vector and z component
		/// </summary>
		/// <param name="v">2d-vector to copy</param>
		/// <param name="z">z component of output vector</param>
		public Vector3D(Vector2D v, double z)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = z;
		}
		
		/// <summary>
		/// Makes a 3d-vector from 3 values
		/// </summary>
		/// <param name="x">x component of output vector</param>
		/// <param name="y">y component of output vector</param>
		/// <param name="z">z component of output vector</param>
		public Vector3D(double x, double y, double z)  
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		/// <summary>
		/// Makes a 3d-vector from 1 value, all vector components are set to the input value
		/// </summary>
		/// <param name="a">Value for vector components</param>
		public Vector3D(double a)  
		{
			this.x = a;
			this.y = a;
			this.z = a;
		}
		
		#endregion constructors
		
		#region properties, indexer

        /// <summary>
        /// Get or Set the Length of this vector
        /// </summary>
        public double Length
        {
            get
            {
                return !this;
            }

            set
            {
                this = ~this * value;
            }
        }
		
		/// <summary>
		/// Get/set x and y components as 2d-vector
		/// </summary>
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
		
		/// <summary>
		/// Get/set x and z components as 2d-vector
		/// </summary>
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
		
		/// <summary>
		/// Get/set y and z components as 2d-vector
		/// </summary>
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
		
		//indexer
		/// <summary>
		/// Unsafe but very fast indexer for 3d-vector, [0..2]
		/// </summary>
		unsafe public double this[int i]
		{
			get
			{	
				fixed (Vector3D* p = &this)
				{
					return ((double*)p)[i];
				}	
			}
			set
			{
				fixed (Vector3D* p = &this)
				{
					((double*)p)[i] = value;
				}
			}
		}
		
		#endregion properties, indexer
		
		#region unary operators
		
		/// <summary>
		/// + vector, makes no changes to a vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Input vector v unchanged</returns>
		public static Vector3D operator +(Vector3D v)
		{
			return v;
		}
		
		/// <summary>
		/// - vector, flips the sign off all vector components
		/// </summary>
		/// <param name="v"></param>
		/// <returns>New vector with all components of v negatived</returns>
		public static Vector3D operator -(Vector3D v)
		{
			return new Vector3D( -v.x, -v.y, -v.z);
		}
		
		/// <summary>
		/// ! vector, calculates the length of the vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Length of input vector v</returns>
		public static double operator !(Vector3D v)
		{
			return Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
		}
		
		/// <summary>
		/// ~ vector, normalizes a vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Vector with same direction than v but length 1</returns>
		public static Vector3D operator ~(Vector3D v)
		{
			double length = Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
			
			if (length != 0) 
				return v * (1 / length);	
			else 
				return new Vector3D(0);
		}

		#endregion unary operators
		
		#region binary operators
		
		/// <summary>
		/// vector + vector, adds the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise sum of the components of v1 and v2</returns>
		public static Vector3D operator +(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		}
		
		/// <summary>
		/// vector + value, adds a value to all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a added to all components of v1</returns>
		public static Vector3D operator +(Vector3D v1, double a)
		{
			return new Vector3D(v1.x + a, v1.y + a, v1.z + a);
		}
		
		/// <summary>
		/// value + vector, adds a value to all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a added to all components of v1</returns>
		public static Vector3D operator +(double a, Vector3D v1)
		{
			return new Vector3D(a + v1.x, a + v1.y, a + v1.z);
		}
		
		/// <summary>
		/// vector - vector, subtracts the components of v2 from the components of v1
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise difference of the components of v1 and v2</returns>
		public static Vector3D operator -(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		}
		
		/// <summary>
		/// vector - value, subtracts a value from all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a subtracted from all components of v1</returns>
		public static Vector3D operator -(Vector3D v1, double a)
		{
			return new Vector3D(v1.x - a, v1.y - a, v1.z - a);
		}
		
		/// <summary>
		/// value - vector, subtracts all vector components from a value
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 subtracted from a</returns>
		public static Vector3D operator -(double a, Vector3D v1)
		{
			return new Vector3D(a - v1.x, a - v1.y, a - v1.z);
		}
		
		/// <summary>
		/// vector * vector, multiplies the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise product of the components of v1 and v2</returns>
		public static Vector3D operator *(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		}
		
		/// <summary>
		/// vector * value, multiplies a value by all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 multiplied by a</returns>
		public static Vector3D operator *(Vector3D v1, double a)
		{
			return new Vector3D(v1.x * a, v1.y * a, v1.z * a);
		}
		
		/// <summary>
		/// value * vector, multiplies a value by all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with all components of v1 multiplied by a</returns>
		public static Vector3D operator *(double a, Vector3D v1)
		{
			return new Vector3D(a * v1.x, a * v1.y, a * v1.z);
		}
		
		/// <summary>
		/// vector / vector, divides the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with components of v1 divided by components of v2</returns>
		public static Vector3D operator /(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
		}
		
		/// <summary>
		/// vector / value, divides all vector components by a value 
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 divided by a</returns>
		public static Vector3D operator /(Vector3D v1, double a)
		{
			double rez = 1/a;
			return new Vector3D(v1.x * rez, v1.y * rez, v1.z * rez);
		}
		
		/// <summary>
		/// value / vector, divides a value by all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with a divided by all components of v1</returns>
		public static Vector3D operator /(double a, Vector3D v1)
		{
			return new Vector3D(a / v1.x, a / v1.y, a / v1.z);
		}
		
		/// <summary>
		/// vector % vector, component wise modulo for vectors
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with components of v1 modulo components of v2</returns>
		public static Vector3D operator %(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.x % v2.x, v1.y % v2.y, v1.z % v2.z);
		}
		
		/// <summary>
		/// vector % value, all vector components modulo a value
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with components of v1 modulo a</returns>
		public static Vector3D operator %(Vector3D v1, double a)
		{
			return new Vector3D(v1.x % a, v1.y % a, v1.z % a);
		}
		
		/// <summary>
		/// value % vector, a value modulo all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with input a modulo components of v1</returns>
		public static Vector3D operator %(double a, Vector3D v1)
		{
			return new Vector3D(a % v1.x, a % v1.y, a % v1.z);
		}
		
		/// <summary>
		/// 3d-vector &amp; 3d-vector, performs a 3d cross product
		/// 
		/// code is:
		/// <c>
		/// x = v1.y * v2.z - v1.z * v2.y
		/// y = v1.z * v2.x - v1.x * v2.y
		/// z = v1.x * v2.y - v1.y * v2.x
		/// </c>
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector which is orthogonal to v1 and v2</returns>
		public static Vector3D operator &(Vector3D v1, Vector3D v2)
		{
			return new Vector3D(v1.y*v2.z - v1.z*v2.y, v1.z*v2.x - v1.x*v2.y, v1.x*v2.y - v1.y*v2.x);
		}
		
		/// <summary>
		/// vector | vector, dot product for vectors, that is the sum of all component wise products
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>Dot product of v1 and v2</returns>
		public static double operator |(Vector3D v1, Vector3D v2)
		{
			return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
		}
		
		#endregion binary operators
		
		#region comparison operators
		
		/// <summary>
		/// vector &gt; value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are greater than a</returns>
		public static bool operator >(Vector3D v, double a)
		{
			return v.x > a && v.y > a && v.z > a;
		}
		
		/// <summary>
		/// vector &lt; value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are smaller than a</returns>
		public static bool operator <(Vector3D v, double a)
		{
			return v.x < a && v.y < a && v.z < a;
		}
		
		/// <summary>
		/// vector &gt;= value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are greater or equal to a</returns> 
		public static bool operator >=(Vector3D v, double a)
		{
			return v.x >= a && v.y >= a && v.z >= a;
		}
		
		/// <summary>
		/// vector &lt;= value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are smaller or equal to a</returns>
		public static bool operator <=(Vector3D v, double a)
		{
			return v.x <= a && v.y <= a && v.z <= a;
		}
		
		#endregion comparison operators
	}

}

