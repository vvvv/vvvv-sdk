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
	/// Fast 4d vector struct with operators
	/// </summary>
	[DataContract]
    [StructLayout(LayoutKind.Sequential)]
	public struct Vector4D
	{
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
		/// <summary>
		/// Data component for the z dimension
		/// </summary>
        [DataMember]
        public double z;
		/// <summary>
		/// Data component for the w dimension
		/// </summary>
        [DataMember]
        public double w;
		
		#endregion data fields
		
		#region constructors
		
		/// <summary>
		/// Copies a 4d-vecor
		/// </summary>
		/// <param name="v">Vector to copy</param>
		public Vector4D(Vector4D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = v.w;
		}
		
		/// <summary>
		/// Makes a 4d-vector copy from a 3d-vector, w is set to 1
		/// </summary>
		/// <param name="v">3d-vector to copy</param>
		public Vector4D(Vector3D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = 1;
		}
		
		/// <summary>
		/// Makes a 4d-vector copy from a 3d-vector and w component
		/// </summary>
		/// <param name="v">3d-vector to copy</param>
		/// <param name="w">w component of output vector</param>
		public Vector4D(Vector3D v, double w)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = w;
		}
		
		/// <summary>
		/// Makes a 4d-vector copy from a 2d-vector, z is set to 0 and w to 1
		/// </summary>
		/// <param name="v">2d-vector to copy</param>
		public Vector4D(Vector2D v)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = 0;
			this.w = 1;
		}
		
		/// <summary>
		/// Makes a 4d-vector copy from a two 2d-vectors
		/// </summary>
		/// <param name="v1">2d-vector for x and y</param>
		/// <param name="v2">2d-vector for z and w</param>
		public Vector4D(Vector2D v1, Vector2D v2)  
		{
			this.x = v1.x;
			this.y = v1.y;
			this.z = v2.x;
			this.w = v2.y;
		}
		
		/// <summary>
		/// Makes a 4d-vector copy from a 2d-vector and z and w component
		/// </summary>
		/// <param name="v">2d-vector to copy</param>
		/// <param name="z">z component of output vector</param>
		/// <param name="w">w component of output vector</param>
		public Vector4D(Vector2D v, double z, double w)  
		{
			this.x = v.x;
			this.y = v.y;
			this.z = z;
			this.w = w;
		}
		
		/// <summary>
		/// Makes a 4d-vector from 4 values
		/// </summary>
		/// <param name="x">x component of output vector</param>
		/// <param name="y">y component of output vector</param>
		/// <param name="z">z component of output vector</param>
		/// <param name="w">w component of output vector</param>
		public Vector4D(double x, double y, double z, double w)  
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
		
		/// <summary>
		/// Makes a 4d-vector from 1 value, all vector components are set to the input value
		/// </summary>
		/// <param name="a">Value for vector components</param>
		public Vector4D(double a)  
		{
			this.x = a;
			this.y = a;
			this.z = a;
			this.w = a;
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
		/// Get/set x, y and z components as 3d-vector
		/// </summary>
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
		
		//indexer
		/// <summary>
		/// Unsafe but very fast indexer for 4d-vector, [0..3]
		/// </summary>
		unsafe public double this[int i]
		{
			get
			{	
				fixed (Vector4D* p = &this)
				{
					return ((double*)p)[i];
				}	
			}
			set
			{
				fixed (Vector4D* p = &this)
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
		public static Vector4D operator +(Vector4D v)
		{
			return v;
		}
		
		/// <summary>
		/// - vector, flips the sign off all vector components
		/// </summary>
		/// <param name="v"></param>
		/// <returns>New vector with all components of v negatived</returns>
		public static Vector4D operator -(Vector4D v)
		{
			return new Vector4D( -v.x, -v.y, -v.z, -v.w);
		}
		
		/// <summary>
		/// ! vector, calculates the length of the vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Length of input vector v</returns>
		public static double operator !(Vector4D v)
		{
			return Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z + v.w*v.w);
		}
		
		/// <summary>
		/// ~ vector, normalizes a vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Vector with same direction than v but length 1</returns>
		public static Vector4D operator ~(Vector4D v)
		{
			double length = Math.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z + v.w*v.w);
			
			if (length != 0) 
				return v * (1 / length);	
			else 
				return new Vector4D(0);
		}	
		
		#endregion unary operators
		
		#region binary operators
	
		/// <summary>
		/// vector + vector, adds the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise sum of the components of v1 and v2</returns>
		public static Vector4D operator +(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}
		
		/// <summary>
		/// vector + value, adds a value to all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a added to all components of v1</returns>
		public static Vector4D operator +(Vector4D v1, double a)
		{
			return new Vector4D(v1.x + a, v1.y + a, v1.z + a, v1.w + a);
		}
		
		/// <summary>
		/// value + vector, adds a value to all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a added to all components of v1</returns>
		public static Vector4D operator +(double a, Vector4D v1)
		{
			return new Vector4D(a + v1.x, a + v1.y, a + v1.z, a + v1.w);
		}
		
		/// <summary>
		/// vector - vector, subtracts the components of v2 from the components of v1
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise difference of the components of v1 and v2</returns>
		public static Vector4D operator -(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}
		
		/// <summary>
		/// vector - value, subtracts a value from all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with a subtracted from all components of v1</returns>
		public static Vector4D operator -(Vector4D v1, double a)
		{
			return new Vector4D(v1.x - a, v1.y - a, v1.z - a, v1.w - a);
		}
		
		/// <summary>
		/// value - vector, subtracts all vector components from a value
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 subtracted from a</returns>
		public static Vector4D operator -(double a, Vector4D v1)
		{
			return new Vector4D(a - v1.x, a - v1.y, a - v1.z, a - v1.w);
		}
		
		/// <summary>
		/// vector * vector, multiplies the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with the pair wise product of the components of v1 and v2</returns>
		public static Vector4D operator *(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
		}
		
		/// <summary>
		/// vector * value, multiplies a value by all vector components
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 multiplied by a</returns>
		public static Vector4D operator *(Vector4D v1, double a)
		{
			return new Vector4D(v1.x * a, v1.y * a, v1.z * a, v1.w * a);
		}
		
		/// <summary>
		/// value * vector, multiplies a value by all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with all components of v1 multiplied by a</returns>
		public static Vector4D operator *(double a, Vector4D v1)
		{
			return new Vector4D(a * v1.x, a * v1.y, a * v1.z, a * v1.w);
		}
		
		/// <summary>
		/// vector / vector, divides the values of two vectors component wise
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with components of v1 divided by components of v2</returns>
		public static Vector4D operator /(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z, v1.w / v2.w);
		}
		
		/// <summary>
		/// vector / value, divides all vector components by a value 
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with all components of v1 divided by a</returns>
		public static Vector4D operator /(Vector4D v1, double a)
		{
			double rez = 1/a;
			return new Vector4D(v1.x * rez, v1.y * rez, v1.z * rez, v1.w * rez);
		}
		
		/// <summary>
		/// value / vector, divides a value by all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with a divided by all components of v1</returns>
		public static Vector4D operator /(double a, Vector4D v1)
		{
			return new Vector4D(a / v1.x, a / v1.y, a / v1.z, a / v1.w);
		}
		
		/// <summary>
		/// vector % vector, component wise modulo for vectors
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>New vector with components of v1 modulo components of v2</returns>
		public static Vector4D operator %(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.x % v2.x, v1.y % v2.y, v1.z % v2.z, v1.w % v2.w);
		}
		
		/// <summary>
		/// vector % value, all vector components modulo a value
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="a"></param>
		/// <returns>New vector with components of v1 modulo a</returns>
		public static Vector4D operator %(Vector4D v1, double a)
		{
			return new Vector4D(v1.x % a, v1.y % a, v1.z % a, v1.w % a);
		}
		
		/// <summary>
		/// value % vector, a value modulo all vector components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="v1"></param>
		/// <returns>New vector with input a modulo components of v1</returns>
		public static Vector4D operator %(double a, Vector4D v1)
		{
			return new Vector4D(a % v1.x, a % v1.y, a % v1.z, a % v1.w);
		}
			
		/// <summary>
		/// 4d-vector &amp; 4d-vector, performs a quaternion multiplication
		/// 
		/// defined as:
		///	w = v1.w * v2.w - (v1.xyz | v2.xyz)
		///	xyz = v1.xyz * v2.w + v2.xyz * v1.w + (v1.xyz &amp; v2.xyz)
		///
		/// code is:
		/// <c>
		/// x = v1.w*v2.x + v1.x*v2.w + v1.y*v2.z - v1.z*v2.y
		///	y = v1.w*v2.y + v1.y*v2.w + v1.z*v2.x - v1.x*v2.z
		///	z = v1.w*v2.z + v1.z*v2.w + v1.x*v2.y - v1.y*v2.x
		///	w = v1.w*v2.w - v1.x*v2.x - v1.y*v2.y - v1.z*v2.z
		/// </c>
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>Quaternion v1 multiplied by quaternion v2</returns>
		public static Vector4D operator &(Vector4D v1, Vector4D v2)
		{
			return new Vector4D(v1.w*v2.x + v1.x*v2.w + v1.y*v2.z - v1.z*v2.y,
			                    v1.w*v2.y + v1.y*v2.w + v1.z*v2.x - v1.x*v2.z,
			                    v1.w*v2.z + v1.z*v2.w + v1.x*v2.y - v1.y*v2.x,
			                    v1.w*v2.w - v1.x*v2.x - v1.y*v2.y - v1.z*v2.z);
		}
		
		
		/// <summary>
		/// vector | vector, dot product for vectors, that is the sum of all component wise products
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>Dot product of v1 and v2</returns>
		public static double operator |(Vector4D v1, Vector4D v2)
		{
			return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z + v1.w * v2.w;
		}
		
		#endregion binary operators		
		
		#region comparison operators
		
		/// <summary>
		/// vector &gt; value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are greater than a</returns>
		public static bool operator >(Vector4D v, double a)
		{
			return v.x > a && v.y > a && v.z > a && v.w > a;
		}
		
		/// <summary>
		/// vector &lt; value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are smaller than a</returns>
		public static bool operator <(Vector4D v, double a)
		{
			return v.x < a && v.y < a && v.z < a && v.w < a;
		}
		
		/// <summary>
		/// vector &gt;= value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are greater or equal to a</returns>
		public static bool operator >=(Vector4D v, double a)
		{
			return v.x >= a && v.y >= a && v.z >= a && v.w >= a;
		}
		
		/// <summary>
		/// vector &lt;= value, compares all vector components to a value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="a"></param>
		/// <returns>true, if all components of v are smaller or equal to a</returns>
		public static bool operator <=(Vector4D v, double a)
		{
			return v.x <= a && v.y <= a && v.z <= a && v.w <= a;
		}

        /// <summary>
        /// vector == vector, checks if the two vectors are equal
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>true, if all components of v1 equal the components of v2</returns>
        public static bool operator ==(Vector4D v1, Vector4D v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z && v1.w == v2.w;
        }

        /// <summary>
        /// vector != vector, checks if the two vectors are not equal
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>true, if any component of v1 is different from the components of v2</returns>
        public static bool operator !=(Vector4D v1, Vector4D v2)
        {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z || v1.w != v2.w;
        }
		
		#endregion comparison operators
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
        {
            return (obj is Vector4D) && Equals((Vector4D)obj);
        }
		
        public bool Equals(Vector4D other)
        {
            return this.x == other.x && this.y == other.y && this.z == other.z && this.w == other.w;
        }
		
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * x.GetHashCode();
                hashCode += 1000000009 * y.GetHashCode();
                hashCode += 1000000021 * z.GetHashCode();
                hashCode += 1000000033 * w.GetHashCode();
            }
            return hashCode;
        }
		#endregion

	}
}

