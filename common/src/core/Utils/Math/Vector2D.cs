/*
 * 
 * the c# vvvv math library
 * 
 * 
 */

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace VVVV.Utils.VMath
{
	
	/// <summary>
	/// Fast 2d vector struct with operators
	/// </summary>
	[DataContract]
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2D
    {
        #region constants
        public static readonly Vector2D Zero = new Vector2D();
        #endregion

        #region data fields

        /// <summary>
		/// Data component for the x dimension
		/// </summary>
        [DataMember]
        public double x;
		/// <summary>
		/// Data component for the y dimension
		/// </summary>
        [DataMember]
        public double y;
		
		#endregion data fields
		
		#region constructors
		
		/// <summary>
		/// Copies a 2d-vecor
		/// </summary>
		/// <param name="v">Vector to copy</param>
		public Vector2D(Vector2D v)  
		{
			this.x = v.x;
			this.y = v.y;
		}
		
		/// <summary>
		/// Makes a 2d-vector from 2 values
		/// </summary>
		/// <param name="x">x component of output vector</param>
		/// <param name="y">y component of output vector</param>
		public Vector2D(double x, double y)  
		{
			this.x = x;
			this.y = y;
		}
		
		/// <summary>
		/// Makes a 2d-vector from 1 value, all vector components are set to the input value
		/// </summary>
		/// <param name="a">Value for vector components</param>
		public Vector2D(double a)  
		{
			this.x = a;
			this.y = a;
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

        //indexer
		/// <summary>
		/// Unsafe but very fast indexer for 2d-vector, [0..1]
		/// </summary>
		unsafe public double this[int i]
		{
			get
			{	
				fixed (Vector2D* p = &this)
				{
					return ((double*)p)[i];
				}	
			}
			set
			{
				fixed (Vector2D* p = &this)
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
		public static Vector2D operator +(Vector2D v)
		{
			return v;
		}
		
		/// <summary>
		/// - vector, flips the sign off all vector components
		/// </summary>
		/// <param name="v"></param>
		/// <returns>New vector with all components of v negatived</returns>
		public static Vector2D operator -(Vector2D v)
		{
			return new Vector2D(-v.x, -v.y);
		}
		
		/// <summary>
		/// ! vector, calculates the length of the vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Length of input vector v</returns>
		public static double operator !(Vector2D v)
		{
			return Math.Sqrt(v.x*v.x + v.y*v.y);
		}
	
		/// <summary>
		/// ~ vector, normalizes a vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Vector with same direction than v but length 1</returns>
		public static Vector2D operator ~(Vector2D v)
		{
			double length = Math.Sqrt(v.x*v.x + v.y*v.y);
			
			if (length != 0) 
				return v * (1 / length);	
			else 
				return new Vector2D(0);
		}
		
		#endregion unary operators
		
		#region binary operators
	
		/// <summary>
		/// vector + vector, adds the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise sum of the components of v1 and v2</returns>
		public static Vector2D operator +(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x + v2.x, v1.y + v2.y);
		}
		
		/// <summary>
		/// vector + value, adds a value to all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a added to all components of v1</returns>
		public static Vector2D operator +(Vector2D v1, double a)
		{
			return new Vector2D(v1.x + a, v1.y + a);
		}
		
		/// <summary>
		/// value + vector, adds a value to all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a added to all components of v1</returns>
		public static Vector2D operator +(double a, Vector2D v1)
		{
			return new Vector2D(a + v1.x, a + v1.y);
		}
		
		/// <summary>
		/// vector - vector, subtracts the components of v2 from the components of v1
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise difference of the components of v1 and v2</returns>
		public static Vector2D operator -(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x - v2.x, v1.y - v2.y);
		}
		
		/// <summary>
		/// vector - value, subtracts a value from all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a subtracted from all components of v1</returns>
		public static Vector2D operator -(Vector2D v1, double a)
		{
			return new Vector2D(v1.x - a, v1.y - a);
		}
		
		/// <summary>
		/// value - vector, subtracts all vector components from a value
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 subtracted from a</returns>
		public static Vector2D operator -(double a, Vector2D v1)
		{
			return new Vector2D(a - v1.x, a - v1.y);
		}
		
		/// <summary>
		/// vector * vector, multiplies the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise product of the components of v1 and v2</returns>
		public static Vector2D operator *(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x * v2.x, v1.y * v2.y);
		}
		
		/// <summary>
		/// vector * value, multiplies a value by all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 multiplied by a</returns>
		public static Vector2D operator *(Vector2D v1, double a)
		{
			return new Vector2D(v1.x * a, v1.y * a);
		}
		
		/// <summary>
		/// value * vector, multiplies a value by all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with all components of v1 multiplied by a</returns>
		public static Vector2D operator *(double a, Vector2D v1)
		{
			return new Vector2D(a * v1.x, a * v1.y);
		}
		
		/// <summary>
		/// vector / vector, divides the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with components of v1 divided by components of v2</returns>
		public static Vector2D operator /(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x / v2.x, v1.y / v2.y);
		}
		
		/// <summary>
		/// vector / value, divides all vector components by a value 
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 divided by a</returns>
		public static Vector2D operator /(Vector2D v1, double a)
		{
			double rez = 1/a;
			return new Vector2D(v1.x * rez, v1.y * rez);
		}
		
		/// <summary>
		/// value / vector, divides a value by all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with a divided by all components of v1</returns>
		public static Vector2D operator /(double a, Vector2D v1)
		{
			return new Vector2D(a / v1.x, a / v1.y);
		}
		
		/// <summary>
		/// vector % vector, component wise modulo for vectors
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with components of v1 modulo components of v2</returns>
		public static Vector2D operator %(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.x % v2.x, v1.y % v2.y);
		}
		
		/// <summary>
		/// vector % value, all vector components modulo a value
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with components of v1 modulo a</returns>
		public static Vector2D operator %(Vector2D v1, double a)
		{
			return new Vector2D(v1.x % a, v1.y % a);
		}
		
		/// <summary>
		/// value % vector, a value modulo all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with input a modulo components of v1</returns>
		public static Vector2D operator %(double a, Vector2D v1)
		{
			return new Vector2D(a % v1.x, a % v1.y);
		}
		
		/// <summary>
		/// 2d-vector &amp; 2d-vector, performs a 2d-cross product,
		/// this is the signed size of the parallelogram spanned by v1 and v2.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>Scalar cross product of v1 and v2</returns>
		public static double operator &(Vector2D v1, Vector2D v2)
		{
			return v1.x * v2.y - v1.y * v2.x;
		}
		
		/// <summary>
		/// vector | vector, dot product for vectors, that is the sum of all component wise products
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>Dot product of v1 and v2</returns>
		public static double operator |(Vector2D v1, Vector2D v2)
		{
			return v1.x * v2.x + v1.y * v2.y;
		}
	
		#endregion binary operators
		
		#region comparison operators
		
		/// <summary>
		/// vector &gt; value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are greater than a</returns>
		public static bool operator >(Vector2D v, double a)
		{
			return v.x > a && v.y > a;
		}
		
		/// <summary>
		/// vector &lt; value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are smaller than a</returns>
		public static bool operator <(Vector2D v, double a)
		{
			return v.x < a && v.y < a;
		}
		
		/// <summary>
		/// vector &gt;= value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are greater or equal to a</returns> 
		public static bool operator >=(Vector2D v, double a)
		{
			return v.x >= a && v.y >= a;
		}
		
		/// <summary>
		/// vector &lt;= value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are smaller or equal to a</returns>
		public static bool operator <=(Vector2D v, double a)
		{
			return v.x <= a && v.y <= a;
		}

        /// <summary>
        /// vector == vector, checks if the two vectors are equal
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>true, if all components of v1 equal the components of v2</returns>
        public static bool operator ==(Vector2D v1, Vector2D v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }

        /// <summary>
        /// vector != vector, checks if the two vectors are not equal
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>true, if any component of v1 is different from the components of v2</returns>
        public static bool operator !=(Vector2D v1, Vector2D v2)
        {
            return v1.x != v2.x || v1.y != v2.y;
        }
		
		#endregion comparison operators
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
        {
            return (obj is Vector2D) && Equals((Vector2D)obj);
        }
		
        public bool Equals(Vector2D other)
        {
            return this.x == other.x && this.y == other.y;
        }
		
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * x.GetHashCode();
                hashCode += 1000000009 * y.GetHashCode();
            }
            return hashCode;
        }
		#endregion

	}
}

