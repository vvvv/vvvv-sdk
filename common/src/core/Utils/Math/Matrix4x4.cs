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
	/// 4x4 transform matrix struct with operators, much faster then matrix classes
	/// </summary>
	[DataContract]
    [StructLayout(LayoutKind.Sequential)]
	public struct Matrix4x4
	{
		#region data fields

		/// <summary>
		/// The 1. data element of 1. row
		/// </summary>
		public double m11;
		/// <summary>
		/// The 2. data element of 1. row
		/// </summary>
		public double m12;
		/// <summary>
		/// The 3. data element of 1. row
		/// </summary>
		public double m13;
		/// <summary>
		/// The 4. data element of 1. row
		/// </summary>
		public double m14;
		
		/// <summary>
		/// The 1. data element of 2. row
		/// </summary>
		public double m21;
		/// <summary>
		/// The 2. data element of 2. row
		/// </summary>
		public double m22;
		/// <summary>
		/// The 3. data element of 2. row
		/// </summary>
		public double m23;
		/// <summary>
		/// The 4. data element of 2. row
		/// </summary>
		public double m24;
		
		/// <summary>
		/// The 1. data element of 3. row
		/// </summary>
		public double m31;
		/// <summary>
		/// The 2. data element of 3. row
		/// </summary>
		public double m32;
		/// <summary>
		/// The 3. data element of 3. row
		/// </summary>
		public double m33;
		/// <summary>
		/// The 4. data element of 3. row
		/// </summary>
		public double m34;
		
		/// <summary>
		/// The 1. data element of 4. row
		/// </summary>
		public double m41;
		/// <summary>
		/// The 2. data element of 4. row
		/// </summary>
		public double m42;
		/// <summary>
		/// The 3. data element of 4. row
		/// </summary>
		public double m43;
		/// <summary>
		/// The 4. data element of 4. row
		/// </summary>
		public double m44;
        
        [DataMember]
        public double[] Values
        {
            get
            {
                double[] l = { m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44 };
                return l;
            }
            set
            {
                m11 = value[0];
                m12 = value[1];
                m13 = value[2];
                m14 = value[3];

                m21 = value[4];
                m22 = value[5];
                m23 = value[6];
                m24 = value[7];

                m31 = value[8];
                m32 = value[9];
                m33 = value[10];
                m34 = value[11];

                m41 = value[12];
                m42 = value[13];
                m43 = value[14];
                m44 = value[15];
            }
        }		
		#endregion data fields
						  
		#region constructors
		
		/// <summary>
		/// Copy constructor for the 4x4 matrix struct
		/// </summary>
		/// <param name="A">Matrix to be copied</param>
		public Matrix4x4 (Matrix4x4 A)
		{
			m11 = A.m11; m12 = A.m12; m13 = A.m13; m14 = A.m14;			
			m21 = A.m21; m22 = A.m22; m23 = A.m23; m24 = A.m24;			
			m31 = A.m31; m32 = A.m32; m33 = A.m33; m34 = A.m34;		
			m41 = A.m41; m42 = A.m42; m43 = A.m43; m44 = A.m44;
		}
		
		/// <summary>
		/// Contructor for a 4x4 matrix from four 4d-vectors, the vectors are treated as rows
		/// </summary>
		/// <param name="v1">1. row</param>
		/// <param name="v2">2. row</param>
		/// <param name="v3">3. row</param>
		/// <param name="v4">4. row</param>
		public Matrix4x4 (Vector4D v1, Vector4D v2, Vector4D v3, Vector4D v4)
		{
			m11 = v1.x; m12 = v1.y; m13 = v1.z; m14 = v1.w;		
			m21 = v2.x; m22 = v2.y; m23 = v2.z; m24 = v2.w;		
			m31 = v3.x; m32 = v3.y; m33 = v3.z; m34 = v3.w;		
			m41 = v4.x; m42 = v4.y; m43 = v4.z; m44 = v4.w;
			
		}
		
		/// <summary>
		/// Contructor for a 4x4 matrix from four 4d-vectors, the vectors are treated as rows or columns depending on the boolean parameter
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <param name="columns">if true, the vectors are treated as columns, else as rows</param>
		public Matrix4x4 (Vector4D v1, Vector4D v2, Vector4D v3, Vector4D v4, bool columns)
		{
			if (columns)
			{
				m11 = v1.x; m12 = v2.x; m13 = v3.x; m14 = v4.x;			
				m21 = v1.y; m22 = v2.y; m23 = v3.y; m24 = v4.y;			
				m31 = v1.z; m32 = v2.z; m33 = v3.z; m34 = v4.z;
				m41 = v1.w; m42 = v2.w; m43 = v3.w; m44 = v4.w;
			}
			else
			{
				m11 = v1.x; m12 = v1.y; m13 = v1.z; m14 = v1.w;
				m21 = v2.x; m22 = v2.y; m23 = v2.z; m24 = v2.w;		
				m31 = v3.x; m32 = v3.y; m33 = v3.z; m34 = v3.w;		
				m41 = v4.x; m42 = v4.y; m43 = v4.z; m44 = v4.w;
			}
		}
		
		/// <summary>
		/// Contructor for a 4x4 matrix from 16 float values, order is row major
		/// </summary>
		/// <param name="m11"></param>
		/// <param name="m12"></param>
		/// <param name="m13"></param>
		/// <param name="m14"></param>
		/// <param name="m21"></param>
		/// <param name="m22"></param>
		/// <param name="m23"></param>
		/// <param name="m24"></param>
		/// <param name="m31"></param>
		/// <param name="m32"></param>
		/// <param name="m33"></param>
		/// <param name="m34"></param>
		/// <param name="m41"></param>
		/// <param name="m42"></param>
		/// <param name="m43"></param>
		/// <param name="m44"></param>
		public Matrix4x4 (double m11, double m12, double m13, double m14,
		                  double m21, double m22, double m23, double m24,
		                  double m31, double m32, double m33, double m34,
		                  double m41, double m42, double m43, double m44)
		{
			this.m11 = m11; this.m12 = m12; this.m13 = m13; this.m14 = m14;	
			this.m21 = m21; this.m22 = m22; this.m23 = m23; this.m24 = m24;	
			this.m31 = m31; this.m32 = m32; this.m33 = m33; this.m34 = m34;	
			this.m41 = m41; this.m42 = m42; this.m43 = m43; this.m44 = m44;
		}

        /// <summary>
        /// Contructor for a 4x4 matrix from a Vector4D v, given by the matrix representation of Quaternions into a Matrix4x4
        /// ( see http://en.wikipedia.org/wiki/Quaternion#Matrix_representations )
        /// </summary>
        /// <param name="v"></param>
        public Matrix4x4(Vector4D v)
        {
            m11 = v.x; m12 = -v.y; m13 = -v.z; m14 = -v.w;         
            m21 = v.y; m22 =  v.x; m23 =  v.w; m24 = -v.z;
            m31 = v.z; m32 = -v.w; m33 =  v.x; m34 =  v.y;
            m41 = v.w; m42 =  v.z; m43 = -v.y; m44 =  v.x;   
        }		
		#endregion constructors
		
		#region properties/indexer
		
		//rows
		/// <summary>
		/// Get/Set the 1. row as 4d-vector
		/// </summary>
		public Vector4D row1
		{
			get
			{
				return new Vector4D(m11, m12, m13, m14);
			}
			set
			{
				m11 = value.x;
				m12 = value.y;
				m13 = value.z;
				m14 = value.w;
			}
		}
		
		/// <summary>
		/// Get/Set the 2. row as 4d-vector
		/// </summary>
		public Vector4D row2
		{
			get
			{
				return new Vector4D(m21, m22, m23, m24);
			}
			set
			{
				m21 = value.x;
				m22 = value.y;
				m23 = value.z;
				m24 = value.w;
			}
		}
		
		/// <summary>
		/// Get/Set the 3. row as 4d-vector
		/// </summary>
		public Vector4D row3
		{
			get
			{
				return new Vector4D(m31, m32, m33, m34);
			}
			set
			{
				m31 = value.x;
				m32 = value.y;
				m33 = value.z;
				m34 = value.w;
			}
		}
		
		/// <summary>
		/// Get/Set the 4. row as 4d-vector
		/// </summary>
		public Vector4D row4
		{
			get
			{
				return new Vector4D(m41, m42, m43, m44);
			}
			set
			{
				m41 = value.x;
				m42 = value.y;
				m43 = value.z;
				m44 = value.w;
			}
		}
		
		//columns
		/// <summary>
		/// Get/Set the 1. column as 4d-vector
		/// </summary>
		public Vector4D col1
		{
			get
			{
				return new Vector4D(m11, m21, m31, m41);
			}
			set
			{
				m11 = value.x;
				m21 = value.y;
				m31 = value.z;
				m41 = value.w;
			}
		}
		
		/// <summary>
		/// Get/Set the 2. column as 4d-vector
		/// </summary>
		public Vector4D col2
		{
			get
			{
				return new Vector4D(m12, m22, m32, m42);
			}
			set
			{
				m12 = value.x;
				m22 = value.y;
				m32 = value.z;
				m42 = value.w;
			}
		}
		
		/// <summary>
		/// Get/Set the 3. column as 4d-vector
		/// </summary>
		public Vector4D col3
		{
			get
			{
				return new Vector4D(m13, m23, m33, m43);
			}
			set
			{
				m13 = value.x;
				m23 = value.y;
				m33 = value.z;
				m43 = value.w;
			}
		}
		
		/// <summary>
		/// Get/Set the 4. column as 4d-vector
		/// </summary>
		public Vector4D col4
		{
			get
			{
				return new Vector4D(m14, m24, m34, m44);
			}
			set
			{
				m14 = value.x;
				m24 = value.y;
				m34 = value.z;
				m44 = value.w;
			}
		}
		
		//indexer
		/// <summary>
		/// Unsafe but very fast indexer for 4x4 matrix, [0..15]
		/// </summary>
		unsafe public double this[int i]
		{
			get
			{	
				fixed (Matrix4x4* p = &this)
				{
					return ((double*)p)[i];
				}	
			}
			set
			{
				fixed (Matrix4x4* p = &this)
				{
					((double*)p)[i] = value;
				}
			}
		}
		
		/// <summary>
		/// Unsafe but very fast 2-d indexer for 4x4 matrix, [0..3, 0..3]
		/// </summary>
		unsafe public double this[int i, int j]
		{
			get
			{
				fixed (Matrix4x4* p = &this)
				{
					return ((double*)p)[i*4+j];
				}	
			}
			set
			{
				fixed (Matrix4x4* p = &this)
				{
					((double*)p)[i*4+j] = value;
				}
			}
		}
		
		
		#endregion properties/indexer
		
		#region unary operators
		
		/// <summary>
		/// + matrix, makes no changes to a matrix
		/// </summary>
		/// <param name="A"></param>
		/// <returns>Input matrix A unchanged</returns>
		public static Matrix4x4 operator +(Matrix4x4 A)
		{
			return A;
		}
		
		/// <summary>
		/// - matrix, flips the sign off all matrix components
		/// </summary>
		/// <param name="A"></param>
		/// <returns>New matrix with all components of A negatived</returns>
		public static Matrix4x4 operator -(Matrix4x4 A)
		{
			return new Matrix4x4(-A.m11, -A.m12, -A.m13, -A.m14,
			                     -A.m21, -A.m22, -A.m23, -A.m24,
			                     -A.m31, -A.m32, -A.m33, -A.m34,
			                     -A.m41, -A.m42, -A.m43, -A.m44);
		}
		
		/// <summary>
		/// ! matrix, calculates the inverse of the matrix
		/// 
		/// optimized 4x4 matrix inversion using cramer's rule, found in the game engine http://www.ogre3d.org
		///	takes about 1,8ns to execute on intel core2 duo 2Ghz, the intel reference
		///	implementation (not assembly optimized) was about 2,2ns.
		///	http://www.intel.com/design/pentiumiii/sml/24504301.pdf
		/// </summary>
		/// <param name="A"></param>
		/// <returns>Inverse matrix</returns>
		public static Matrix4x4 operator !(Matrix4x4 A)
		{
			
			double a11 = A.m11, a12 = A.m12, a13 = A.m13, a14 = A.m14;
			double a21 = A.m21, a22 = A.m22, a23 = A.m23, a24 = A.m24;
			double a31 = A.m31, a32 = A.m32, a33 = A.m33, a34 = A.m34;
			double a41 = A.m41, a42 = A.m42, a43 = A.m43, a44 = A.m44;

			double term1 = a31 * a42 - a32 * a41;
			double term2 = a31 * a43 - a33 * a41;
			double term3 = a31 * a44 - a34 * a41;
			double term4 = a32 * a43 - a33 * a42;
			double term5 = a32 * a44 - a34 * a42;
			double term6 = a33 * a44 - a34 * a43;

			double subterm1 = + (term6 * a22 - term5 * a23 + term4 * a24);
			double subterm2 = - (term6 * a21 - term3 * a23 + term2 * a24);
			double subterm3 = + (term5 * a21 - term3 * a22 + term1 * a24);
			double subterm4 = - (term4 * a21 - term2 * a22 + term1 * a23);

			double invDet = 1 / (subterm1 * a11 + subterm2 * a12 + subterm3 * a13 + subterm4 * a14);

			double ret11 = subterm1 * invDet;
			double ret21 = subterm2 * invDet;
			double ret31 = subterm3 * invDet;
			double ret41 = subterm4 * invDet;

			double ret12 = - (term6 * a12 - term5 * a13 + term4 * a14) * invDet;
			double ret22 = + (term6 * a11 - term3 * a13 + term2 * a14) * invDet;
			double ret32 = - (term5 * a11 - term3 * a12 + term1 * a14) * invDet;
			double ret42 = + (term4 * a11 - term2 * a12 + term1 * a13) * invDet;

			term1 = a21 * a42 - a22 * a41;
			term2 = a21 * a43 - a23 * a41;
			term3 = a21 * a44 - a24 * a41;
			term4 = a22 * a43 - a23 * a42;
			term5 = a22 * a44 - a24 * a42;
			term6 = a23 * a44 - a24 * a43;

			double ret13 = + (term6 * a12 - term5 * a13 + term4 * a14) * invDet;
			double ret23 = - (term6 * a11 - term3 * a13 + term2 * a14) * invDet;
			double ret33 = + (term5 * a11 - term3 * a12 + term1 * a14) * invDet;
			double ret43 = - (term4 * a11 - term2 * a12 + term1 * a13) * invDet;

			term1 = a32 * a21 - a31 * a22;
			term2 = a33 * a21 - a31 * a23;
			term3 = a34 * a21 - a31 * a24;
			term4 = a33 * a22 - a32 * a23;
			term5 = a34 * a22 - a32 * a24;
			term6 = a34 * a23 - a33 * a24;

			double ret14 = - (term6 * a12 - term5 * a13 + term4 * a14) * invDet;
			double ret24 = + (term6 * a11 - term3 * a13 + term2 * a14) * invDet;
			double ret34 = - (term5 * a11 - term3 * a12 + term1 * a14) * invDet;
			double ret44 = + (term4 * a11 - term2 * a12 + term1 * a13) * invDet;

			return new Matrix4x4(ret11, ret12, ret13, ret14,
								 ret21, ret22, ret23, ret24,
								 ret31, ret32, ret33, ret34,
								 ret41, ret42, ret43, ret44);
		}
		
		
		/// <summary>
		/// ~ matrix, calculates the determinant of the matrix
		/// </summary>
		/// <param name="A"></param>
		/// <returns>Determinat of the matrix</returns>
		public static double operator ~(Matrix4x4 A)
		{
			double m00 = A.m11, m01 = A.m12, m02 = A.m13, m03 = A.m14;
			double m10 = A.m21, m11 = A.m22, m12 = A.m23, m13 = A.m24;
			double m20 = A.m31, m21 = A.m32, m22 = A.m33, m23 = A.m34;
			double m30 = A.m41, m31 = A.m42, m32 = A.m33, m33 = A.m44;
			
			return	m03 * m12 * m21 * m30-m02 * m13 * m21 * m30-m03 * m11 * m22 * m30+m01 * m13 * m22 * m30+
					m02 * m11 * m23 * m30-m01 * m12 * m23 * m30-m03 * m12 * m20 * m31+m02 * m13 * m20 * m31+
					m03 * m10 * m22 * m31-m00 * m13 * m22 * m31-m02 * m10 * m23 * m31+m00 * m12 * m23 * m31+
					m03 * m11 * m20 * m32-m01 * m13 * m20 * m32-m03 * m10 * m21 * m32+m00 * m13 * m21 * m32+
					m01 * m10 * m23 * m32-m00 * m11 * m23 * m32-m02 * m11 * m20 * m33+m01 * m12 * m20 * m33+
					m02 * m10 * m21 * m33-m00 * m12 * m21 * m33-m01 * m10 * m22 * m33+m00 * m11 * m22 * m33;
		}
		
		#endregion unary operators
		
		#region binary operators
		
		
		/// <summary>
		/// matrix + matrix, adds the values of two matrices component wise
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns>New matrix with the pair wise sum of the components of A and B</returns>
		public static Matrix4x4 operator +(Matrix4x4 A, Matrix4x4 B)
		{
			return new Matrix4x4(A.m11 + B.m11, A.m12 + B.m12, A.m13 + B.m13, A.m14 + B.m14,
			                     A.m21 + B.m21, A.m22 + B.m22, A.m23 + B.m23, A.m24 + B.m24,
			                     A.m31 + B.m31, A.m32 + B.m32, A.m33 + B.m33, A.m34 + B.m34,
			                     A.m41 + B.m41, A.m42 + B.m42, A.m43 + B.m43, A.m44 + B.m44);
		}
		
		/// <summary>
		/// matrix + value, adds a value to all matrix components
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>New matrix with b added to all components of A</returns>
		public static Matrix4x4 operator +(Matrix4x4 A, double b)
		{
			return new Matrix4x4(A.m11 + b, A.m12 + b, A.m13 + b, A.m14 + b,
			                     A.m21 + b, A.m22 + b, A.m23 + b, A.m24 + b,
			                     A.m31 + b, A.m32 + b, A.m33 + b, A.m34 + b,
			                     A.m41 + b, A.m42 + b, A.m43 + b, A.m44 + b);
		}
		
		/// <summary>
		/// value + matrix, adds a value to all matrix components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="B"></param>
		/// <returns>New matrix with b added to all components of A</returns>
		public static Matrix4x4 operator +(double a, Matrix4x4 B)
		{
			return new Matrix4x4(a + B.m11, a + B.m12, a + B.m13, a + B.m14,
			                     a + B.m21, a + B.m22, a + B.m23, a + B.m24,
			                     a + B.m31, a + B.m32, a + B.m33, a + B.m34,
			                     a + B.m41, a + B.m42, a + B.m43, a + B.m44);
		}
		
		
		/// <summary>
		/// matrix - matrix, subtracts the components of B from the components of A
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns>New matrix with the pair wise difference of the components of A and B</returns>
		public static Matrix4x4 operator -(Matrix4x4 A, Matrix4x4 B)
		{
			return new Matrix4x4(A.m11 - B.m11, A.m12 - B.m12, A.m13 - B.m13, A.m14 - B.m14,
			                     A.m21 - B.m21, A.m22 - B.m22, A.m23 - B.m23, A.m24 - B.m24,
			                     A.m31 - B.m31, A.m32 - B.m32, A.m33 - B.m33, A.m34 - B.m34,
			                     A.m41 - B.m41, A.m42 - B.m42, A.m43 - B.m43, A.m44 - B.m44);
		}
		
		/// <summary>
		/// matrix - value, subtracts a value from all matrix components
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>New matrix with b subtracted from all components of A</returns>
		public static Matrix4x4 operator -(Matrix4x4 A, double b)
		{
			return new Matrix4x4(A.m11 - b, A.m12 - b, A.m13 - b, A.m14 - b,
			                     A.m21 - b, A.m22 - b, A.m23 - b, A.m24 - b,
			                     A.m31 - b, A.m32 - b, A.m33 - b, A.m34 - b,
			                     A.m41 - b, A.m42 - b, A.m43 - b, A.m44 - b);
		}
		
		/// <summary>
		/// value - matrix, subtracts all matrix components from a value 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="B"></param>
		/// <returns>New matrix with all components of A subtracted from b</returns>
		public static Matrix4x4 operator -(double a, Matrix4x4 B)
		{
			return new Matrix4x4(a - B.m11, a - B.m12, a - B.m13, a - B.m14,
			                     a - B.m21, a - B.m22, a - B.m23, a - B.m24,
			                     a - B.m31, a - B.m32, a - B.m33, a - B.m34,
			                     a - B.m41, a - B.m42, a - B.m43, a - B.m44);
		}
		
		/// <summary>
		/// matrix * matrix, performs a matrix multiplication
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns>Matrix product of A and B</returns>
		public static Matrix4x4 operator *(Matrix4x4 A, Matrix4x4 B)
		{
			return new Matrix4x4(B.m11 * A.m11 + B.m21 * A.m12 + B.m31 * A.m13 + B.m41 * A.m14,
			                     B.m12 * A.m11 + B.m22 * A.m12 + B.m32 * A.m13 + B.m42 * A.m14,
			                     B.m13 * A.m11 + B.m23 * A.m12 + B.m33 * A.m13 + B.m43 * A.m14,
			                     B.m14 * A.m11 + B.m24 * A.m12 + B.m34 * A.m13 + B.m44 * A.m14,
			                     
			                     B.m11 * A.m21 + B.m21 * A.m22 + B.m31 * A.m23 + B.m41 * A.m24,
			                     B.m12 * A.m21 + B.m22 * A.m22 + B.m32 * A.m23 + B.m42 * A.m24,
			                     B.m13 * A.m21 + B.m23 * A.m22 + B.m33 * A.m23 + B.m43 * A.m24,
			                     B.m14 * A.m21 + B.m24 * A.m22 + B.m34 * A.m23 + B.m44 * A.m24,
			                     
			                     B.m11 * A.m31 + B.m21 * A.m32 + B.m31 * A.m33 + B.m41 * A.m34,
			                     B.m12 * A.m31 + B.m22 * A.m32 + B.m32 * A.m33 + B.m42 * A.m34,
			                     B.m13 * A.m31 + B.m23 * A.m32 + B.m33 * A.m33 + B.m43 * A.m34,
			                     B.m14 * A.m31 + B.m24 * A.m32 + B.m34 * A.m33 + B.m44 * A.m34,
			                     
			                     B.m11 * A.m41 + B.m21 * A.m42 + B.m31 * A.m43 + B.m41 * A.m44,
			                     B.m12 * A.m41 + B.m22 * A.m42 + B.m32 * A.m43 + B.m42 * A.m44,
			                     B.m13 * A.m41 + B.m23 * A.m42 + B.m33 * A.m43 + B.m43 * A.m44,
			                     B.m14 * A.m41 + B.m24 * A.m42 + B.m34 * A.m43 + B.m44 * A.m44);
		}
		
		
		/// <summary>
		/// matrix * 4d vector, applies a matrix transform to a 4d-vector
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>Vector b transformed by matrix A</returns>
		public static Vector4D operator *(Matrix4x4 A, Vector4D b)
		{
			return new Vector4D(A.m11 * b.x + A.m21 * b.y + A.m31 * b.z + A.m41 * b.w,
			                    A.m12 * b.x + A.m22 * b.y + A.m32 * b.z + A.m42 * b.w,
			                    A.m13 * b.x + A.m23 * b.y + A.m33 * b.z + A.m43 * b.w,
			                    A.m14 * b.x + A.m24 * b.y + A.m34 * b.z + A.m44 * b.w);	
		}
		
		
		/// <summary>
		/// matrix * 3d vector, applies a matrix transform to a 3d-vector, (x, y, z, 1) and divides by w
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>Vector b transformed by matrix A</returns>
		public static Vector3D operator *(Matrix4x4 A, Vector3D b)
		{
			double wFactor = 1/(A.m14 * b.x + A.m24 * b.y + A.m34 * b.z + A.m44);
			
			return new Vector3D((A.m11 * b.x + A.m21 * b.y + A.m31 * b.z + A.m41) * wFactor,
			                    (A.m12 * b.x + A.m22 * b.y + A.m32 * b.z + A.m42) * wFactor,
			                    (A.m13 * b.x + A.m23 * b.y + A.m33 * b.z + A.m43) * wFactor);
		}
		
		/// <summary>
		/// matrix * 2d vector, applies a matrix transform to a 2d-vector, (x, y, 0, 1)
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>Vector b transformed by matrix A</returns>
		public static Vector3D operator *(Matrix4x4 A, Vector2D b)
		{
			double wFactor = 1/(A.m14 * b.x + A.m24 * b.y + A.m44);
			
			return new Vector3D((A.m11 * b.x + A.m21 * b.y + A.m41) * wFactor,
			                    (A.m12 * b.x + A.m22 * b.y + A.m42) * wFactor,
			                    (A.m13 * b.x + A.m23 * b.y + A.m43) * wFactor);
		}
		
		/// <summary>
		/// matrix * value, multiplies all matrix components with a value
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>New matrix with all components of A multiplied by b</returns>
		public static Matrix4x4 operator *(Matrix4x4 A, double b)
		{
			return new Matrix4x4(A.m11 * b, A.m12 * b, A.m13 * b, A.m14 * b,
			                     A.m21 * b, A.m22 * b, A.m23 * b, A.m24 * b,
			                     A.m31 * b, A.m32 * b, A.m33 * b, A.m34 * b,
			                     A.m41 * b, A.m42 * b, A.m43 * b, A.m44 * b);
		}
		
		/// <summary>
		/// value * matrix, multiplies all matrix components with a value
		/// </summary>
		/// <param name="a"></param>
		/// <param name="B"></param>
		/// <returns>New matrix with all components of B multiplied by a</returns>
		public static Matrix4x4 operator *(double a, Matrix4x4 B)
		{
			return new Matrix4x4(a * B.m11, a * B.m12, a * B.m13, a * B.m14,
			                     a * B.m21, a * B.m22, a * B.m23, a * B.m24,
			                     a * B.m31, a * B.m32, a * B.m33, a * B.m34,
			                     a * B.m41, a * B.m42, a * B.m43, a * B.m44);
		}
		
		/// <summary>
		/// matrix / value, divides all matrix components with a value
		/// </summary>
		/// <param name="A"></param>
		/// <param name="b"></param>
		/// <returns>New matrix with all components of A divided by b</returns>
		public static Matrix4x4 operator /(Matrix4x4 A, double b)
		{
			double rez = 1/b;
			return new Matrix4x4(A.m11 * rez, A.m12 * rez, A.m13 * rez, A.m14 * rez,
			                     A.m21 * rez, A.m22 * rez, A.m23 * rez, A.m24 * rez,
			                     A.m31 * rez, A.m32 * rez, A.m33 * rez, A.m34 * rez,
			                     A.m41 * rez, A.m42 * rez, A.m43 * rez, A.m44 * rez);
		}
		
		/// <summary>
		/// value / matrix, divides a value by all matrix components
		/// </summary>
		/// <param name="a"></param>
		/// <param name="B"></param>
		/// <returns>New matrix with a divided by all components of B</returns>
		public static Matrix4x4 operator /(double a, Matrix4x4 B)
		{
			return new Matrix4x4(a / B.m11, a / B.m12, a / B.m13, a / B.m14,
			                     a / B.m21, a / B.m22, a / B.m23, a / B.m24,
			                     a / B.m31, a / B.m32, a / B.m33, a / B.m34,
			                     a / B.m41, a / B.m42, a / B.m43, a / B.m44);
		}
		
		public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return lhs.Equals(rhs);
        }
		
        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return !(lhs == rhs);
        }
		
		#endregion binary operators

        #region methods

        /// <summary>
        /// Transpose thi 4x4 matrix
        /// </summary>
        /// <returns>Transpose of this matrix</returns>
        public Matrix4x4 Transpose()
        {
            return new Matrix4x4(m11, m21, m31, m41,
                                 m12, m22, m32, m42,
                                 m13, m23, m33, m43,
                                 m14, m24, m34, m44);
        }

        public override string ToString()
		{
			string row1 = m11.ToString("f4") + " " + m12.ToString("f4") + " " + m13.ToString("f4") + " " + m14.ToString("f4");
			string row2 = m21.ToString("f4") + " " + m22.ToString("f4") + " " + m23.ToString("f4") + " " + m24.ToString("f4");
			string row3 = m31.ToString("f4") + " " + m32.ToString("f4") + " " + m33.ToString("f4") + " " + m34.ToString("f4");
			string row4 = m41.ToString("f4") + " " + m42.ToString("f4") + " " + m43.ToString("f4") + " " + m44.ToString("f4");
			
			return "\n" + row1 + "\n" + row2 + "\n" + row3 + "\n" + row4;
        }

        #endregion methods

        #region Equals and GetHashCode implementation

        public override bool Equals(object obj)
        {
            return (obj is Matrix4x4) && Equals((Matrix4x4)obj);
        }

        public bool Equals(Matrix4x4 other)
        {
            return this.m11 == other.m11 && this.m12 == other.m12 && this.m13 == other.m13 && this.m14 == other.m14 && this.m21 == other.m21 && this.m22 == other.m22 && this.m23 == other.m23 && this.m24 == other.m24 && this.m31 == other.m31 && this.m32 == other.m32 && this.m33 == other.m33 && this.m34 == other.m34 && this.m41 == other.m41 && this.m42 == other.m42 && this.m43 == other.m43 && this.m44 == other.m44;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                hashCode += 1000000007 * m11.GetHashCode();
                hashCode += 1000000009 * m12.GetHashCode();
                hashCode += 1000000021 * m13.GetHashCode();
                hashCode += 1000000033 * m14.GetHashCode();
                hashCode += 1000000087 * m21.GetHashCode();
                hashCode += 1000000093 * m22.GetHashCode();
                hashCode += 1000000097 * m23.GetHashCode();
                hashCode += 1000000103 * m24.GetHashCode();
                hashCode += 1000000123 * m31.GetHashCode();
                hashCode += 1000000181 * m32.GetHashCode();
                hashCode += 1000000207 * m33.GetHashCode();
                hashCode += 1000000223 * m34.GetHashCode();
                hashCode += 1000000241 * m41.GetHashCode();
                hashCode += 1000000271 * m42.GetHashCode();
                hashCode += 1000000289 * m43.GetHashCode();
                hashCode += 1000000297 * m44.GetHashCode();
            }
            return hashCode;
        }
        #endregion Equals and GetHashCode implementation
    }
}
