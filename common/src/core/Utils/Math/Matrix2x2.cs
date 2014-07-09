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
    /// 2x2 transform matrix struct with operators
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix2x2
    {
        #region data fields

        /// <summary>
        /// The 1. data element of 1. row
        /// </summary>
        public double a;

        /// <summary>
        /// The 2. data element of 1. row
        /// </summary>
        public double b;

        /// <summary>
        /// The 1. data element of 2. row
        /// </summary>
        public double c;

        /// <summary>
        /// The 2. data element of 2. row
        /// </summary>
        public double d;
        [DataMember]
        public double[] Values
        {
            get
            {
                double[] l = { a, b, c, d };
                return l;
            }
            set
            {
                a = value[0];
                b = value[1];
                c = value[2];
                d = value[3];
            }
        }
        #endregion data fields

        #region constructors

        /// <summary>
        /// Copy constructor for the 2x2 matrix struct
        /// </summary>
        /// <param name="A">Matrix to be copied</param>
        public Matrix2x2(Matrix2x2 A)
        {
            this.a = A.a;
            this.b = A.b;
            this.c = A.c;
            this.d = A.d;
        }

        /// <summary>
        /// Contructor for a 2x2 matrix from 4 float values, order is row major
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public Matrix2x2(double a, double b, double c, double d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        /// <summary>
        /// Copies the significant elements m11, m12, m21, m22 from the 4x4 matrix
        /// </summary>
        /// <param name="A"></param>
        public Matrix2x2(Matrix4x4 A)
        {
            this.a = A.m11;
            this.b = A.m12;
            this.c = A.m21;
            this.d = A.m22;
        }

        #endregion constructors

        #region operators

        /// <summary>
        /// matrix / value, divides all matrix components with a value
        /// </summary>
        /// <param name="A"></param>
        /// <param name="v"></param>
        /// <returns>New matrix with all components of A divided by v</returns>
        public static Matrix2x2 operator /(Matrix2x2 A, double v)
        {
            v = 1 / v;
            return new Matrix2x2(A.a * v, A.b * v,
                                 A.c * v, A.d * v);
        }

        /// <summary>
        /// matrix + matrix, adds the values of two matrices component wise
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>New matrix with the pair wise sum of the components of A and B</returns>
        public static Matrix2x2 operator +(Matrix2x2 A, Matrix2x2 B)
        {
            return new Matrix2x2(A.a + B.a, A.b + B.b,
                                 A.c + B.c, A.d + B.d);
        }

        /// <summary>
        /// 2d matrix multiplication
        /// </summary>
        public static Matrix2x2 operator *(Matrix2x2 A, Matrix2x2 B)
        {
            return new Matrix2x2(A.a * B.a + A.b * B.c, A.a * B.b + A.b * B.d,
                                 A.c * B.a + A.d * B.c, A.c * B.b + A.d * B.d);
        }

        /// <summary>
        /// matrix * 2d vector, applies a matrix transform to a 2d-vector
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns>Vector b transformed by matrix A</returns>
        public static Vector2D operator *(Matrix2x2 A, Vector2D b)
        {
            return new Vector2D(A.a * b.x + A.c * b.y,
                                A.b * b.x + A.d * b.y);
        }

        #endregion operators

        #region methods

        /// <summary>
        /// Transpose this 2x2 matrix
        /// </summary>
        /// <returns>a, c, b, d</returns>
        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(a, c, b, d);
        }

        /// <summary>
        /// Superfast 2d decomposition
        /// </summary>
        /// <param name="scale">Scale XY of the matrix</param>
        /// <param name="rotate">Rotation of the matrix in radian</param>
        public void Decompose(out Vector2D scale, out double rotate)
        {
            var q = this + new Matrix2x2(d, -c, -b, a);
            q = q / Math.Sqrt(q.a * q.a + q.c * q.c);
            var s = this * q.Transpose();

            rotate = -Math.Atan2(q.c, q.d);
            scale = new Vector2D(s.a, s.d);
        }

        #endregion methods

        #region Equals and GetHashCode implementation

        public override bool Equals(object obj)
        {
            return (obj is Matrix2x2) && Equals((Matrix2x2)obj);
        }

        public bool Equals(Matrix2x2 other)
        {
            return this.a == other.a && this.b == other.b && this.c == other.c && this.d == other.d;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                hashCode += 1000000007 * a.GetHashCode();
                hashCode += 1000000009 * b.GetHashCode();
                hashCode += 1000000021 * c.GetHashCode();
                hashCode += 1000000033 * d.GetHashCode();
            }
            return hashCode;
        }
        #endregion Equals and GetHashCode implementation
    }
}